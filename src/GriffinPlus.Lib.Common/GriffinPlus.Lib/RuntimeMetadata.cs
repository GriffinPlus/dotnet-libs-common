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
using System.Threading.Tasks;

using GriffinPlus.Lib.Logging;
using GriffinPlus.Lib.Threading;

using Exception = System.Exception;

#if !NET461
using System.Runtime.InteropServices;
#endif

namespace GriffinPlus.Lib;

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
	private static readonly ReaderWriterLockSlim                      sLock                                       = new(LockRecursionPolicy.NoRecursion);
	private static readonly AsyncManualResetEvent                     sScanningFinishedEvent                      = new();
	private static readonly Type                                      sAssemblyLoadContextType                    = Type.GetType("System.Runtime.Loader.AssemblyLoadContext");
	private static readonly MethodInfo                                sGetLoadContextMethod                       = null;
	private static readonly object                                    sDefaultAssemblyLoadContext                 = null;
	private static readonly bool                                      sIsNetFramework                             = false;
	private static readonly object                                    sInitializedSync                            = new();
	private static          bool                                      sInitialized                                = false;
	private static readonly ConcurrentQueue<Assembly>                 sAssembliesToScan                           = new();
	private static readonly object                                    sLoadAssembliesSync                         = new();
	private static          bool                                      sLoadedAssembliesInApplicationBaseDirectory = false;
	private static          bool                                      sLoadedAllAssemblies                        = false;
	private static          int                                       sAsynchronousUpdatePending                  = 0; // boolean: 0 = false, 1 = true
	private static          Dictionary<string, Assembly>              sAssembliesByFullName                       = new();
	private static          Dictionary<Assembly, IReadOnlyList<Type>> sTypesByAssembly                            = new();
	private static          Dictionary<Assembly, IReadOnlyList<Type>> sExportedTypesByAssembly                    = new();
	private static          Dictionary<string, IReadOnlyList<Type>>   sTypesByFullName                            = new();
	private static          Dictionary<string, IReadOnlyList<Type>>   sExportedTypesByFullName                    = new();

	/// <summary>
	/// Occurs when an assembly has been scanned by the <see cref="RuntimeMetadata"/> class,
	/// so its runtime metadata is available via the following properties:<br/>
	/// - <see cref="AssembliesByFullName"/><br/>
	/// - <see cref="ExportedTypesByAssembly"/><br/>
	/// - <see cref="ExportedTypesByFullName"/><br/>
	/// - <see cref="TypesByAssembly"/><br/>
	/// - <see cref="TypesByFullName"/><br/>
	/// This event is raised asynchronously, so you should take care of proper synchronization.
	/// </summary>
	public static event EventHandler<AssemblyScannedEventArgs> AssemblyScanned;

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
			?.Invoke(null, []);

		// .NET Core 2/3 and .NET 5+ only:
		// get the method to retrieve the load context an assembly is associated with
		sGetLoadContextMethod = sAssemblyLoadContextType?.GetMethod("GetLoadContext");

		// determine whether the application is running with .NET Framework
#if NET461
		// the build for .NET Framework 4.6.1 will only run on .NET Framework 4.6.1+
		// => the runtime is guaranteed to be .NET Framework
		sIsNetFramework = true;
#else
		sIsNetFramework = RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework", StringComparison.InvariantCulture);
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

		lock (sInitializedSync)
		{
			// abort, if the class is already initialized
			if (sInitialized)
				return;

			// there should not have been any scan before...
			Debug.Assert(sAssembliesToScan.IsEmpty);
			Debug.Assert(!sScanningFinishedEvent.IsSet);

			// hook up the event that is raised when an assembly is loaded into the application domain
			AppDomain.CurrentDomain.AssemblyLoad += HandleAssemblyLoaded;

			// put currently loaded assemblies into the set of assemblies to scan
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				// skip assembly if it is loaded for reflection only
				if (assembly.ReflectionOnly)
					continue;

				sAssembliesToScan.Enqueue(assembly);
			}

			// start worker thread to scan these assemblies
			TriggerUpdate();

			// wait for scanning assemblies to complete
			sScanningFinishedEvent.Wait();

			// indicate that initialization has completed
			sInitialized = true;
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
	}

	/// <summary>
	/// Synchronously waits for scanning scheduled assemblies to complete.
	/// </summary>
	/// <param name="timeout">Timeout (in ms, -1 to wait infinitely).</param>
	/// <returns>
	/// <c>true</c> if scanning completed within the specified time;<br/>
	/// otherwise <c>false</c>.
	/// </returns>
	public static bool WaitForScanningToComplete(int timeout = Timeout.Infinite)
	{
		using var cts = new CancellationTokenSource(timeout);
		try
		{
			sScanningFinishedEvent.Wait(cts.Token);
			return true;
		}
		catch (OperationCanceledException)
		{
			return false;
		}
	}

	/// <summary>
	/// Synchronously waits for scanning scheduled assemblies to complete.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token that can be signalled to cancel the operation.</param>
	/// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been signalled.</exception>
	public static void WaitForScanningToComplete(CancellationToken cancellationToken)
	{
		sScanningFinishedEvent.Wait(cancellationToken);
	}

	/// <summary>
	/// Asynchronously waits for scanning scheduled assemblies to complete.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token that can be signalled to cancel the operation.</param>
	/// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been signalled.</exception>
	public static Task WaitForScanningToCompleteAsync(CancellationToken cancellationToken)
	{
		return sScanningFinishedEvent.WaitAsync(cancellationToken);
	}

	/// <summary>
	/// Loads all assemblies in the application's base directory.
	/// This is done the first time the method is called only.
	/// </summary>
	/// <param name="waitForScanningToComplete">
	/// <c>true</c> to schedule assemblies to be scanned and wait for the scan to complete;<br/>
	/// <c>false</c> to return immediately after scheduling assemblies to be scanned.
	/// </param>
	public static void LoadAssembliesInApplicationBaseDirectory(bool waitForScanningToComplete)
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

			// signal that scanning has not finished, yet
			sScanningFinishedEvent.Reset();

			// suppress triggering asynchronous updates
			// that might occur due to assemblies to be loaded
			int asynchronousUpdatePendingState = Interlocked.Exchange(ref sAsynchronousUpdatePending, 1);

			// load all assemblies in the application's base directory recursively
			// (should cover plugin assemblies that may reside in a subdirectory)
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

			// trigger updating asynchronously and
			// wait for scanning to complete, if requested
			TriggerUpdate();
			if (waitForScanningToComplete) sScanningFinishedEvent.Wait();
		}
	}

	/// <summary>
	/// Loads all assemblies in the application's base directory and referenced assemblies recursively.
	/// This is done the first time the method is called only.
	/// </summary>
	/// <param name="waitForScanningToComplete">
	/// <c>true</c> to schedule assemblies to be scanned and wait for the scan to complete;<br/>
	/// <c>false</c> to return immediately after scheduling assemblies to be scanned.
	/// </param>
	public static void LoadAllAssemblies(bool waitForScanningToComplete)
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

			// signal that scanning has not finished, yet
			sScanningFinishedEvent.Reset();

			// suppress triggering asynchronous updates
			// that might occur due to assemblies to be loaded
			int asynchronousUpdatePendingState = Interlocked.Exchange(ref sAsynchronousUpdatePending, 1);

			// load all assemblies in the application's base directory recursively
			// (should cover plugin assemblies that may reside in a subdirectory)
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

			// trigger updating asynchronously and
			// wait for scanning to complete, if requested
			TriggerUpdate();
			if (waitForScanningToComplete) sScanningFinishedEvent.Wait();
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
		processedAssemblies ??= [];

		// abort if the specified assembly was already processed to avoid processing assemblies multiple times,
		// otherwise remember that the assembly was processed
		if (!processedAssemblies.Add(assembly))
			return;

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

		// add the loaded assembly to the set of assemblies to scan and trigger updating asynchronously
		sScanningFinishedEvent.Reset();
		sAssembliesToScan.Enqueue(args.LoadedAssembly);
		TriggerUpdate();
	}

	/// <summary>
	/// Starts a worker thread to scan assemblies that have been scheduled.
	/// </summary>
	private static void TriggerUpdate()
	{
		// abort if a scan is already scheduled
		if (Interlocked.CompareExchange(ref sAsynchronousUpdatePending, 1, 0) == 1)
			return;

		// schedule a scan
		while (!ThreadPool.QueueUserWorkItem(Work))
		{
			Thread.Sleep(50);
		}

		return;

		// worker method for updating asynchronously
		static void Work(object obj)
		{
			List<Assembly> scannedAssemblies;

			sLock.EnterWriteLock();
			try
			{
				// abort, if there are no asynchronous updates pending
				if (Interlocked.CompareExchange(ref sAsynchronousUpdatePending, 0, 1) == 0)
					return;

				// scan the scheduled assemblies
				ScanScheduledAssemblies(out scannedAssemblies);
				sScanningFinishedEvent.Set();
			}
			finally
			{
				sLock.ExitWriteLock();
			}

			// notify clients of the class about the scanned assemblies
			foreach (Assembly assembly in scannedAssemblies)
			{
				try
				{
					EventHandler<AssemblyScannedEventArgs> handler = AssemblyScanned;
					handler?.Invoke(null, new AssemblyScannedEventArgs(assembly));
				}
				catch (Exception ex)
				{
					sLog.Write(
						LogLevel.Alert,
						"The event handler of the {0}.{1} event threw an unhandled exception. Exception:\n{2}",
						nameof(RuntimeMetadata),
						nameof(AssemblyScanned),
						ex);
				}
			}
		}
	}

	/// <summary>
	/// Scans currently scheduled assemblies.
	/// </summary>
	/// <param name="scannedAssemblies">Receives a list of scanned assemblies.</param>
	private static void ScanScheduledAssemblies(out List<Assembly> scannedAssemblies)
	{
		Debug.Assert(sLock.IsWriteLockHeld);

		// abort if there are no scheduled assemblies...
		scannedAssemblies = [];
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

			// skip, if assembly has already been inspected...
			if (sTypesByAssembly.ContainsKey(assembly))
				continue;

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
				object assemblyLoadContext = sGetLoadContextMethod.Invoke(null, [assembly]);
				if (assemblyLoadContext != sDefaultAssemblyLoadContext) continue;
				goto scan;
			}

			// below this line there is only handling for the .NET Framework
			// .NET Core 2/3 and .NET 5+ is completely handled by the section above
			// ReSharper disable once RedundantJumpStatement
			if (!sIsNetFramework) continue;

#if NETSTANDARD2_0 || NET461 || NET48
			// ----------------------------------------------------------------------------------------------------------------
			// .NET Framework only:
			// scan the assembly if it resides in the Global Assembly Cache (GAC)
			// ----------------------------------------------------------------------------------------------------------------

			if (assembly.GlobalAssemblyCache)
				goto scan;

			// ----------------------------------------------------------------------------------------------------------------
			// skip scanning assembly if it has not been loaded from the file system
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
				?.Invoke(AppDomain.CurrentDomain, []);

			// get the private bin paths of the current application domain
			string[] privateBinPaths = (setupInformation
				                            ?.GetType()
				                            .GetProperty("PrivateBinPath")
				                            ?.GetMethod
				                            ?.Invoke(setupInformation, []) as string)?.Split(';');

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
					if (!fullPrivateBinPath.StartsWith(applicationBaseDirectory + Path.DirectorySeparatorChar, StringComparison.InvariantCulture))
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

			// store the assembly for notifying clients about the scanned assembly
			scannedAssemblies.Add(assembly);
		}

		// replace exposed dictionaries
		sAssembliesByFullName = assembliesByFullName;
		sTypesByAssembly = typesByAssembly;
		sExportedTypesByAssembly = exportedTypesByAssembly;
		sTypesByFullName = typesByFullName.ToDictionary(x => x.Key, x => (IReadOnlyList<Type>) [.. x.Value]);
		sExportedTypesByFullName = exportedTypesByFullName.ToDictionary(x => x.Key, x => (IReadOnlyList<Type>) [.. x.Value]);
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

#if NETCOREAPP3_0_OR_GREATER
		// abort if the assembly does not have a full name
		if (assembly.FullName == null) return;
#endif

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
				list = [];
				typesByFullName.Add(type.FullName, list);
			}

			list.Add(type);

			if (!type.IsPublic)
				continue;

			if (!exportedTypesByFullName.TryGetValue(type.FullName, out list))
			{
				list = [];
				exportedTypesByFullName.Add(type.FullName, list);
			}

			list.Add(type);
		}
	}
}
