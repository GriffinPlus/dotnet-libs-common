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
#if !NET461
using System.Runtime.InteropServices;
#endif

namespace GriffinPlus.Lib
{

	/// <summary>
	/// Provides various information about assemblies and types.<br/>
	/// <br/>
	/// Depending on the framework at runtime different assemblies are included:<br/>
	/// <br/>
	/// - .NET Framework 4+:<br/>
	/// --> Assemblies in the Global Assembly Cache (GAC)<br/>
	/// --> Assemblies in the application's base directory or a private bin path<br/>
	/// --> Assemblies that have been generated dynamically<br/>
	/// <br/>
	/// - .NET Core 2/3, .NET 5+:<br/>
	/// --> Assemblies that are loaded into the default assembly load context<br/>
	/// --> Assemblies that have been generated dynamically<br/>
	/// <br/>
	/// Assemblies that are loaded from the web are not considered.
	/// </summary>
	public static class RuntimeMetadata
	{
		private static readonly LogWriter                                 sLog                                        = LogWriter.Get(typeof(RuntimeMetadata));
		private static readonly ReaderWriterLockSlim                      sLock                                       = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
		private static readonly Type                                      sAssemblyLoadContextType                    = Type.GetType("System.Runtime.Loader.AssemblyLoadContext");
		private static readonly MethodInfo                                sGetLoadContextMethod                       = null;
		private static readonly object                                    sDefaultAssemblyLoadContext                 = null;
		private static readonly bool                                      sIsNetFramework                             = false;
		private static          bool                                      sInitialized                                = false;
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
			// .NET Core 2/3 and .NET 5+ only:
			// get the default assembly load context
			sDefaultAssemblyLoadContext = sAssemblyLoadContextType
				?.GetProperty("Default")
				?.GetMethod
				?.Invoke(null, Array.Empty<object>());

			// .NET Core 2/3 and .NET 5+ only:
			// get the method to retrieve the load context an assembly is associated with
			sGetLoadContextMethod = sAssemblyLoadContextType?.GetMethod("GetLoadContext");

			// determine whether the application is running with .NET Framework
#if NET461
			// the build for .NET Framework 4.6.1 will only run on .NET Framework 4.6.1+
			// => the runtime is guaranteed to be .NET Framework
			sIsNetFramework = true;
#else
			sIsNetFramework = RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");
#endif
		}

		/// <summary>
		/// Initializes static properties providing various information about assemblies and types and sets up an
		/// event to scan assemblies that are loaded later on.
		/// </summary>
		private static void Init()
		{
			// abort if the class is already initialized
			// (sInitialized is set to true and stays true until the process terminates)
			if (sInitialized)
				return;

			sLock.EnterWriteLock();
			try
			{
				// abort, if the class is already initialized
				if (sInitialized)
					return;

				// hook up the event that is raised when an assembly is loaded into the application domain
				AppDomain.CurrentDomain.AssemblyLoad += HandleAssemblyLoaded;

				// put currently loaded assemblies into the set of assemblies to scan
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly assembly in assemblies)
				{
					// skip assembly if it is loaded for reflection only
					if (assembly.ReflectionOnly)
						continue;

					// get all types from the assembly to trigger loading dependent assemblies
					// (assemblies will be put into sAssembliesToScan, so these assemblies are scanned first)
					try { assembly.GetTypes(); }
					catch (ReflectionTypeLoadException)
					{
						// swallow
					}

					sAssembliesToScan.Enqueue(assembly);
				}

				// indicate that initialization has completed
				sInitialized = true;

				// start worker thread to scan these assemblies
				TriggerUpdate();
			}
			finally
			{
				sLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Gets a dictionary mapping full assembly names to assemblies.
		/// </summary>
		public static IReadOnlyDictionary<string, Assembly> AssembliesByFullName
		{
			get
			{
				// initialize the class if necessary
				if (!sInitialized)
					Init();

				// get current copy of the dictionary
				// if there are no assemblies scheduled to be scanned
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

				// there are assemblies scheduled to be scanned
				// => scan and return the updated dictionary
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
				// initialize the class if necessary
				if (!sInitialized)
					Init();

				// get current copy of the dictionary
				// if there are no assemblies scheduled to be scanned
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

				// there are assemblies scheduled to be scanned
				// => scan and return the updated dictionary
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
				// initialize the class if necessary
				if (!sInitialized)
					Init();

				// get current copy of the dictionary
				// if there are no assemblies scheduled to be scanned
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

				// there are assemblies scheduled to be scanned
				// => scan and return the updated dictionary
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
				// initialize the class if necessary
				if (!sInitialized)
					Init();

				// get current copy of the dictionary
				// if there are no assemblies scheduled to be scanned
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

				// there are assemblies scheduled to be scanned
				// => scan and return the updated dictionary
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
				// initialize the class if necessary
				if (!sInitialized)
					Init();

				// get current copy of the dictionary
				// if there are no assemblies scheduled to be scanned
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

				// there are assemblies scheduled to be scanned
				// => scan and return the updated dictionary
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
			// initialize the class if necessary
			if (!sInitialized)
				Init();

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
			// initialize the class if necessary
			if (!sInitialized)
				Init();

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
			// skip assembly if it is loaded for reflection only
			if (args.LoadedAssembly.ReflectionOnly)
				return;

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
					if (Interlocked.CompareExchange(ref sAsynchronousUpdatePending, 0, 1) == 1)
					{
						ScanScheduledAssemblies();
					}
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
				Debug.Assert(assembly.FullName != null, "assembly.FullName != null");

				// log that the assembly was loaded
				sLog.Write(LogLevel.Debug, "Scanning assembly ({0})...", assembly.FullName);

				// ----------------------------------------------------------------------------------------------------------------
				// scan assembly if it has been generated dynamically
				// ----------------------------------------------------------------------------------------------------------------

				if (assembly.IsDynamic)
					goto scan;

				// ----------------------------------------------------------------------------------------------------------------
				// .NET Core 2/3 and .NET 5+ only:
				// scan assembly if it is loaded into the default assembly load context
				// ----------------------------------------------------------------------------------------------------------------

				if (sAssemblyLoadContextType != null)
				{
					Debug.Assert(sGetLoadContextMethod != null, nameof(sGetLoadContextMethod) + " != null");
					Debug.Assert(sDefaultAssemblyLoadContext != null, nameof(sDefaultAssemblyLoadContext) + " != null");
					object assemblyLoadContext = sGetLoadContextMethod.Invoke(null, new object[] { assembly });
					if (assemblyLoadContext != sDefaultAssemblyLoadContext) continue;
					goto scan;
				}

				// below this line there is only handling for the .NET Framework
				// .NET Core 2/3 and .NET 5+ is completely handled by the section above
				if (!sIsNetFramework) continue;

#if NETSTANDARD2_0 || NET461 || NET48
				// ----------------------------------------------------------------------------------------------------------------
				// .NET Framework only:
				// scan the assembly if it resides in the Global Assembly Cache (GAC)
				// ----------------------------------------------------------------------------------------------------------------

				if (assembly.GlobalAssemblyCache)
					goto scan;

				// ----------------------------------------------------------------------------------------------------------------
				// skip scanning assembly if has not been loaded from the file system
				// => dynamically generated assemblies and assemblies loaded from the web are not supported
				// ----------------------------------------------------------------------------------------------------------------

				var uri = new Uri(assembly.CodeBase);
				if (uri.Scheme != "file") continue;
				string assemblyFilePath = Path.GetFullPath(uri.LocalPath);

				// ----------------------------------------------------------------------------------------------------------------
				// .NET Framework only:
				// scan the assembly if it resides in the application's base directory
				// ----------------------------------------------------------------------------------------------------------------

				string assemblyDirectoryPath = Path.GetDirectoryName(assemblyFilePath)?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				string applicationBaseDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				if (assemblyDirectoryPath == applicationBaseDirectory)
					goto scan;

				// ----------------------------------------------------------------------------------------------------------------
				// .NET Framework only:
				// scan the assembly if it resides in the private bin path below the application's base directory
				// ----------------------------------------------------------------------------------------------------------------

				// get the setup information object of the current application domain
				object setupInformation = typeof(AppDomain)
					.GetProperty("SetupInformation")
					?.GetMethod
					?.Invoke(AppDomain.CurrentDomain, Array.Empty<object>());

				// get the private bin paths of the current application domain
				string[] privateBinPaths = (setupInformation
					                            ?.GetType()
					                            .GetProperty("PrivateBinPath")
					                            ?.GetMethod
					                            ?.Invoke(setupInformation, Array.Empty<object>()) as string)?.Split(';');

				if (privateBinPaths != null)
				{
					foreach (string privateBinPath in privateBinPaths)
					{
						// make the private bin path a full path to perform comparisons
						string fullPrivateBinPath;
						if (Path.IsPathRooted(privateBinPath))
						{
							fullPrivateBinPath = Path.GetFullPath(privateBinPath);
						}
						else
						{
							fullPrivateBinPath = Path.Combine(applicationBaseDirectory, privateBinPath);
							fullPrivateBinPath = Path.GetFullPath(fullPrivateBinPath);
						}

						// skip private bin path if it is not below the application's base directory
						if (!fullPrivateBinPath.StartsWith(applicationBaseDirectory + Path.DirectorySeparatorChar))
							continue;

						// scan the assembly if it is in the private bin path of the application domain
						string path = Path.Combine(fullPrivateBinPath, Path.GetFileName(assemblyFilePath));
						if (assemblyFilePath == path)
							goto scan;
					}
				}

#elif NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
#else
#error Unhandled target framework.
#endif

				// the previous checks did not indicate that the assembly should be scanned
				// => skip it...
				continue;

				// scan the assembly and update the temporary data set accordingly
				scan:
				InspectAssemblyAndCollectInformation(
					assembly,
					assembliesByFullName,
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
		/// <param name="assembliesByFullName">A dictionary mapping the full name of assemblies to the corresponding assembly objects.</param>
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
			IDictionary<string, Assembly>              assembliesByFullName,
			IDictionary<Assembly, IReadOnlyList<Type>> typesByAssembly,
			IDictionary<Assembly, IReadOnlyList<Type>> exportedTypesByAssembly,
			IDictionary<string, List<Type>>            typesByFullName,
			IDictionary<string, List<Type>>            exportedTypesByFullName)
		{
			// scan the assembly for types
			Type[] types;
			try { types = assembly.GetTypes(); }
			catch (ReflectionTypeLoadException ex) { types = ex.Types; }

			// add assembly to the table mapping assembly names to assembly objects
			assembliesByFullName.Add(assembly.FullName, assembly);

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
