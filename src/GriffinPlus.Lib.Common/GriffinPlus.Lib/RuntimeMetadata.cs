///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

using GriffinPlus.Lib.Logging;

namespace GriffinPlus.Lib
{

	/// <summary>
	/// Provides various information about assemblies and types in the current application domain.
	/// </summary>
	public static class RuntimeMetadata
	{
		private static readonly LogWriter                                 sLog                                        = LogWriter.Get(typeof(RuntimeMetadata));
		private static readonly ReaderWriterLockSlim                      sLock                                       = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
		private static readonly ConcurrentQueue<Assembly>                 sAssembliesToScan                           = new ConcurrentQueue<Assembly>();
		private static readonly object                                    sLoadAssembliesSync                         = new object();
		private static          bool                                      sLoadedAssembliesInApplicationBaseDirectory = false;
		private static          bool                                      sLoadedAllAssemblies                        = false;
		private static          int                                       sAsynchronousUpdatePending                  = 0; // boolean: 0 = false, 1 = true
		private static          Dictionary<string, Assembly>              sAssembliesByFullName                       = new Dictionary<string, Assembly>();
		private static          Dictionary<Assembly, IReadOnlyList<Type>> sTypesByAssembly                            = new Dictionary<Assembly, IReadOnlyList<Type>>();
		private static          Dictionary<Assembly, IReadOnlyList<Type>> sExportedTypesByAssembly                    = new Dictionary<Assembly, IReadOnlyList<Type>>();
		private static          Dictionary<string, IReadOnlyList<Type>>   sTypesByFullName                            = new Dictionary<string, IReadOnlyList<Type>>();
		private static          Dictionary<string, IReadOnlyList<Type>>   sExportedTypesByFullName                    = new Dictionary<string, IReadOnlyList<Type>>();

		/// <summary>
		/// Initializes the <see cref="RuntimeMetadata"/> class.
		/// </summary>
		static RuntimeMetadata()
		{
			// hook up the event that is raised when an assembly is loaded into the application domain
			AppDomain.CurrentDomain.AssemblyLoad += HandleAssemblyLoaded;

			// put currently loaded assemblies into the set of assemblies to scan
			// and start worker thread to scan these assemblies
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				// get all types from the assembly to trigger loading dependent assemblies
				// (assemblies will be put into sAssembliesToScan, so these assemblies are scanned first)
				try { assembly.GetTypes(); }
				catch (ReflectionTypeLoadException)
				{
					// swallow
				}

				sAssembliesToScan.Enqueue(assembly);
			}
			TriggerUpdate();
		}

		/// <summary>
		/// Gets a dictionary mapping full assembly names to assemblies.
		/// </summary>
		public static IReadOnlyDictionary<string, Assembly> AssembliesByFullName
		{
			get
			{
				if (sAssembliesToScan.IsEmpty)
				{
					sLock.EnterReadLock();
					try
					{
						return sAssembliesByFullName;
					}
					finally
					{
						sLock.ExitReadLock();
					}
				}

				sLock.EnterWriteLock();
				try
				{
					ScanScheduledAssemblies();
					return sAssembliesByFullName;
				}
				finally
				{
					sLock.ExitWriteLock();
				}
			}
		}

		/// <summary>
		/// Gets a dictionary mapping assemblies to types stored within them.<br/>
		/// The dictionary contains public and non-public types.
		/// </summary>
		public static IReadOnlyDictionary<Assembly, IReadOnlyList<Type>> TypesByAssembly
		{
			get
			{
				if (sAssembliesToScan.IsEmpty)
				{
					sLock.EnterReadLock();
					try
					{
						return sTypesByAssembly;
					}
					finally
					{
						sLock.ExitReadLock();
					}
				}

				sLock.EnterWriteLock();
				try
				{
					ScanScheduledAssemblies();
					return sTypesByAssembly;
				}
				finally
				{
					sLock.ExitWriteLock();
				}
			}
		}

		/// <summary>
		/// Gets a dictionary mapping assemblies to types stored within them.<br/>
		/// The dictionary contains public types only.
		/// </summary>
		public static IReadOnlyDictionary<Assembly, IReadOnlyList<Type>> ExportedTypesByAssembly
		{
			get
			{
				if (sAssembliesToScan.IsEmpty)
				{
					sLock.EnterReadLock();
					try
					{
						return sExportedTypesByAssembly;
					}
					finally
					{
						sLock.ExitReadLock();
					}
				}

				sLock.EnterWriteLock();
				try
				{
					ScanScheduledAssemblies();
					return sExportedTypesByAssembly;
				}
				finally
				{
					sLock.ExitWriteLock();
				}
			}
		}

		/// <summary>
		/// Gets a dictionary mapping the full name of a type (namespace + type name) to the corresponding <see cref="Type"/> objects.<br/>
		/// Multiple assemblies may contain a type with the same full name.<br/>
		/// The dictionary contains public and non-public types.
		/// </summary>
		public static IReadOnlyDictionary<string, IReadOnlyList<Type>> TypesByFullName
		{
			get
			{
				if (sAssembliesToScan.IsEmpty)
				{
					sLock.EnterReadLock();
					try
					{
						return sTypesByFullName;
					}
					finally
					{
						sLock.ExitReadLock();
					}
				}

				sLock.EnterWriteLock();
				try
				{
					ScanScheduledAssemblies();
					return sTypesByFullName;
				}
				finally
				{
					sLock.ExitWriteLock();
				}
			}
		}

		/// <summary>
		/// Gets a dictionary mapping the full name of a type (namespace + type name) to the corresponding <see cref="Type"/> objects.<br/>
		/// Multiple assemblies may contain a type with the same full name.<br/>
		/// The dictionary contains public types only.
		/// </summary>
		public static IReadOnlyDictionary<string, IReadOnlyList<Type>> ExportedTypesByFullName
		{
			get
			{
				if (sAssembliesToScan.IsEmpty)
				{
					sLock.EnterReadLock();
					try
					{
						return sExportedTypesByFullName;
					}
					finally
					{
						sLock.ExitReadLock();
					}
				}

				sLock.EnterWriteLock();
				try
				{
					ScanScheduledAssemblies();
					return sExportedTypesByFullName;
				}
				finally
				{
					sLock.ExitWriteLock();
				}
			}
		}

		/// <summary>
		/// Loads all assemblies in the application's base directory.
		/// This is done the first time the method is called only.
		/// </summary>
		/// <param name="scanImmediately">
		/// <c>true</c> to scan loaded assemblies immediately;<br/>
		/// <c>false</c> to scan loaded assemblies asynchronously.
		/// </param>
		public static void LoadAssembliesInApplicationBaseDirectory(bool scanImmediately = false)
		{
			// abort if assemblies in the application base directory have already been loaded...
			// (flag switches to true and stays there until the process terminates, so it can be used without synchronization to save time)
			if (sLoadedAssembliesInApplicationBaseDirectory)
				return;

			lock (sLoadAssembliesSync)
			{
				// abort if assemblies in the application's directory have already been loaded...
				if (sLoadedAssembliesInApplicationBaseDirectory)
					return;

				// suppress triggering asynchronous updates
				int asynchronousUpdatePendingState = Interlocked.Exchange(ref sAsynchronousUpdatePending, 1);

				// load all assemblies in the application's base directory recursively
				// (should cover plugin assemblies that may reside in a sub-directory)
				string path = AppDomain.CurrentDomain.BaseDirectory;
				var regex = new Regex(@"\.(exe|dll)$", RegexOptions.IgnoreCase);
				foreach (string filename in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
				{
					if (!regex.IsMatch(filename))
						continue;

					try
					{
						Assembly.LoadFrom(filename);
						sLog.Write(LogLevel.Trace, "Assembly in application directory ({0}) has been loaded successfully.", filename);
					}
					catch (Exception ex)
					{
						sLog.Write(LogLevel.Debug, "Assembly in application directory ({0}) could not be loaded.\nError: {1}.", filename, ex.Message);
					}
				}

				// assemblies in the application base directory have been loaded now...
				sLoadedAssembliesInApplicationBaseDirectory = true;

				// resume triggering asynchronous updates
				if (asynchronousUpdatePendingState == 0)
					Interlocked.Exchange(ref sAsynchronousUpdatePending, 0);

				// scan new loaded assemblies immediately, if requested;
				// otherwise trigger updating asynchronously
				if (scanImmediately)
				{
					sLock.EnterWriteLock();
					try
					{
						ScanScheduledAssemblies();
					}
					finally
					{
						sLock.ExitWriteLock();
					}
				}
				else
				{
					TriggerUpdate();
				}
			}
		}

		/// <summary>
		/// Loads all assemblies in the application's base directory and referenced assemblies recursively.
		/// This is done the first time the method is called only.
		/// </summary>
		/// <param name="scanImmediately">
		/// <c>true</c> to scan loaded assemblies immediately;<br/>
		/// <c>false</c> to scan loaded assemblies asynchronously.
		/// </param>
		public static void LoadAllAssemblies(bool scanImmediately = false)
		{
			// abort if all assemblies have already been loaded...
			// (flag switches to true and stays there until the process terminates, so it can be used without synchronization to save time)
			if (sLoadedAllAssemblies)
				return;

			lock (sLoadAssembliesSync)
			{
				// abort if all assemblies have already been loaded...
				if (sLoadedAllAssemblies)
					return;

				// suppress triggering asynchronous updates
				int asynchronousUpdatePendingState = Interlocked.Exchange(ref sAsynchronousUpdatePending, 1);

				// load all assemblies in the application's base directory recursively
				// (should cover plugin assemblies that may reside in a sub-directory)
				if (!sLoadedAssembliesInApplicationBaseDirectory)
				{
					string path = AppDomain.CurrentDomain.BaseDirectory;
					var regex = new Regex(@"\.(exe|dll)$", RegexOptions.IgnoreCase);
					foreach (string filename in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
					{
						if (!regex.IsMatch(filename))
							continue;

						try
						{
							Assembly.LoadFrom(filename);
							sLog.Write(LogLevel.Trace, "Assembly in application directory ({0}) has been loaded successfully.", filename);
						}
						catch (Exception ex)
						{
							sLog.Write(LogLevel.Debug, "Assembly in application directory ({0}) could not be loaded.\nError: {1}.", filename, ex.Message);
						}
					}

					sLoadedAssembliesInApplicationBaseDirectory = true;
				}

				// load all assemblies referenced by already loaded assemblies
				var processedAssemblies = new HashSet<Assembly>();
				foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					LoadReferencedAssemblies(assembly, processedAssemblies);
				}

				// all assemblies have been loaded now...
				sLoadedAllAssemblies = true;

				// resume triggering asynchronous updates
				if (asynchronousUpdatePendingState == 0)
					Interlocked.Exchange(ref sAsynchronousUpdatePending, 0);

				// scan new loaded assemblies immediately, if requested;
				// otherwise trigger updating asynchronously
				if (scanImmediately)
				{
					sLock.EnterWriteLock();
					try
					{
						ScanScheduledAssemblies();
					}
					finally
					{
						sLock.ExitWriteLock();
					}
				}
				else
				{
					TriggerUpdate();
				}
			}
		}

		/// <summary>
		/// Loads all assemblies referenced by the specified assembly.
		/// </summary>
		/// <param name="assembly">Assembly whose references to load.</param>
		/// <param name="processedAssemblies">Set of already processed assemblies.</param>
		private static void LoadReferencedAssemblies(Assembly assembly, HashSet<Assembly> processedAssemblies = null)
		{
			// create a new set of loaded assemblies if it was not specified (first run)
			if (processedAssemblies == null) processedAssemblies = new HashSet<Assembly>();

			// abort if the specified assembly was already processed to avoid processing assemblies multiple times
			if (processedAssemblies.Contains(assembly))
				return;

			// remember that the assembly was processed
			processedAssemblies.Add(assembly);

			// load references of the assembly
			foreach (AssemblyName referencedAssemblyName in assembly.GetReferencedAssemblies())
			{
				try
				{
					Assembly referencedAssembly = Assembly.Load(referencedAssemblyName);
					if (processedAssemblies.Contains(referencedAssembly)) continue;
					sLog.Write(LogLevel.Trace, "Referenced assembly ({0}) has been loaded successfully.", referencedAssemblyName);
					LoadReferencedAssemblies(referencedAssembly, processedAssemblies);
				}
				catch (Exception ex)
				{
					sLog.Write(LogLevel.Debug, "Referenced assembly ({0}) could not be loaded.\nError: {1}.", referencedAssemblyName, ex.Message);
				}
			}
		}

		/// <summary>
		/// Is called when the <see cref="AppDomain.AssemblyLoad"/> event is raised.
		/// Scans the loaded assembly for information about types and integrates them into the provided data set.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="args">Event arguments (contains the loaded assembly).</param>
		private static void HandleAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
		{
			Debug.Assert(!sLock.IsReadLockHeld);
			Debug.Assert(!sLock.IsWriteLockHeld);

			// get all types from the loaded assembly to trigger loading dependent assemblies
			// (assemblies will be put into sAssembliesToScan, so these assemblies are scanned first)
			try { args.LoadedAssembly.GetTypes(); }
			catch (ReflectionTypeLoadException)
			{
				// swallow
			}

			// add the loaded assembly to the set of assemblies to scan and trigger updating asynchronously
			sAssembliesToScan.Enqueue(args.LoadedAssembly);
			TriggerUpdate();
		}

		/// <summary>
		/// Starts a worker thread to scan assemblies that have been scheduled.
		/// </summary>
		private static void TriggerUpdate()
		{
			// worker method for updating asynchronously
			void Work(object obj)
			{
				sLock.EnterWriteLock();
				try
				{
					Interlocked.Exchange(ref sAsynchronousUpdatePending, 0);
					ScanScheduledAssemblies();
				}
				finally
				{
					sLock.ExitWriteLock();
				}
			}

			// abort if a scan is already scheduled
			if (Interlocked.CompareExchange(ref sAsynchronousUpdatePending, 1, 0) == 1)
				return;

			// schedule a scan
			while (!ThreadPool.QueueUserWorkItem(Work))
			{
				Thread.Sleep(50);
			}
		}

		/// <summary>
		/// Scans currently scheduled assemblies.
		/// </summary>
		private static void ScanScheduledAssemblies()
		{
			Debug.Assert(sLock.IsWriteLockHeld);

			// abort if there are no scheduled assemblies...
			if (sAssembliesToScan.IsEmpty)
				return;

			// inspect assembly and collect information about it
			// create a copy of the existing dictionaries
			var assembliesByFullName = new Dictionary<string, Assembly>(sAssembliesByFullName);
			var typesByAssembly = new Dictionary<Assembly, IReadOnlyList<Type>>(sTypesByAssembly);
			var exportedTypesByAssembly = new Dictionary<Assembly, IReadOnlyList<Type>>(sExportedTypesByAssembly);
			Dictionary<string, List<Type>> typesByFullName = sTypesByFullName.ToDictionary(x => x.Key, x => x.Value.ToList());
			Dictionary<string, List<Type>> exportedTypesByFullName = sExportedTypesByFullName.ToDictionary(x => x.Key, x => x.Value.ToList());

			// scan scheduled assemblies
			while (sAssembliesToScan.TryDequeue(out Assembly assembly))
			{
				// log that the assembly was loaded
				sLog.Write(LogLevel.Debug, "Scanning assembly ({0}) for information about types...", assembly.FullName);

				// integrate information into temporary dictionaries
				assembliesByFullName.Add(assembly.FullName, assembly);
				InspectAssemblyAndCollectInformation(
					assembly,
					typesByAssembly,
					exportedTypesByAssembly,
					typesByFullName,
					exportedTypesByFullName);
			}

			// replace exposed dictionaries
			sAssembliesByFullName = assembliesByFullName;
			sTypesByAssembly = typesByAssembly;
			sExportedTypesByAssembly = exportedTypesByAssembly;
			sTypesByFullName = typesByFullName.ToDictionary(x => x.Key, x => (IReadOnlyList<Type>)x.Value.ToList());
			sExportedTypesByFullName = exportedTypesByFullName.ToDictionary(x => x.Key, x => (IReadOnlyList<Type>)x.Value.ToList());
		}

		/// <summary>
		/// Inspects the specified assembly and collects information to expose.
		/// </summary>
		/// <param name="assembly">Assembly to inspect.</param>
		/// <param name="typesByAssembly">A dictionary mapping assemblies to public and non-public types stored within them.</param>
		/// <param name="exportedTypesByAssembly">A dictionary mapping assemblies to public types stored within them.</param>
		/// <param name="typesByFullName">
		/// A dictionary mapping the full name of public and non-public types (namespace + type name) to the corresponding
		/// <see cref="Type"/> objects.
		/// </param>
		/// <param name="exportedTypesByFullName">
		/// A dictionary mapping the full name of public types (namespace + type name) to the corresponding
		/// <see cref="Type"/> objects.
		/// </param>
		private static void InspectAssemblyAndCollectInformation(
			Assembly                                   assembly,
			IDictionary<Assembly, IReadOnlyList<Type>> typesByAssembly,
			IDictionary<Assembly, IReadOnlyList<Type>> exportedTypesByAssembly,
			IDictionary<string, List<Type>>            typesByFullName,
			IDictionary<string, List<Type>>            exportedTypesByFullName)
		{
			if (!typesByAssembly.TryGetValue(assembly, out IReadOnlyList<Type> types))
			{
				// scan the assembly for types
				try { types = assembly.GetTypes(); }
				catch (ReflectionTypeLoadException ex) { types = ex.Types; }

				// add types to the table mapping assemblies to types defined in them
				types = types.Where(x => x != null).ToArray();
				typesByAssembly.Add(assembly, types);
				exportedTypesByAssembly.Add(assembly, types.Where(x => x.IsPublic).ToArray());

				// update the table mapping type names to type objects
				foreach (Type type in types)
				{
					Debug.Assert(type.FullName != null, "type.FullName != null");
					if (!typesByFullName.TryGetValue(type.FullName, out List<Type> list))
					{
						list = new List<Type>();
						typesByFullName.Add(type.FullName, list);
					}

					list.Add(type);

					if (type.IsPublic)
					{
						if (!exportedTypesByFullName.TryGetValue(type.FullName, out list))
						{
							list = new List<Type>();
							exportedTypesByFullName.Add(type.FullName, list);
						}

						list.Add(type);
					}
				}
			}
		}
	}

}
