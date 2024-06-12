///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

using GriffinPlus.Lib.Threading;

// ReSharper disable InvertIf

// ReSharper disable InconsistentNaming
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// Base class for <see cref="DefaultCascadedConfiguration"/> and <see cref="CascadedConfiguration"/>.
/// </summary>
[DebuggerDisplay("Configuration | Path: {" + nameof(Path) + "}")]
public abstract class CascadedConfigurationBase
{
	/// <summary>
	/// Child configurations of the current configurations, i.e. configurations at the next lower hierarchy level.
	/// </summary>
	protected readonly List<CascadedConfigurationBase> mChildren = [];

	/// <summary>
	/// Configuration items in the current configuration.
	/// </summary>
	protected readonly List<ICascadedConfigurationItem> mItems = [];

	/// <summary>
	/// Configurations inheriting from the current configuration (same configuration path).
	/// </summary>
	protected readonly List<CascadedConfigurationBase> mInheritingConfigurations = [];

	/// <summary>
	/// Flag indicating whether the current configuration has been modified.
	/// </summary>
	protected bool mIsModified;

	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="CascadedConfigurationBase"/> class
	/// (for root configurations that inherit from another configuration).
	/// </summary>
	/// <param name="configurationToInheritFrom">Configuration to inherit from.</param>
	/// <param name="persistence">
	/// A persistence strategy that is responsible for persisting configuration items;<br/>
	/// <see langword="null"/> if persistence is not needed.
	/// </param>
	protected CascadedConfigurationBase(CascadedConfigurationBase configurationToInheritFrom, ICascadedConfigurationPersistenceStrategy persistence)
	{
		Debug.Assert(Monitor.IsEntered(configurationToInheritFrom.Sync), "The configuration is expected to be locked.");

		InheritedConfiguration = configurationToInheritFrom;
		mInheritingConfigurations = [];
		Name = InheritedConfiguration.Name;
		Sync = InheritedConfiguration.Sync;
		Path = InheritedConfiguration.Path;
		PersistenceStrategy = persistence;

		// register the current configuration with the inherited configuration
		InheritedConfiguration.mInheritingConfigurations.Add(this);

		// add the same child configurations as they exist in the inherited configuration
		for (int i = 0; i < InheritedConfiguration.mChildren.Count; i++)
		{
			_ = new CascadedConfiguration(InheritedConfiguration.mChildren[i].Name, this); // links itself to the current configuration
		}

		// add the same items as they exist in the inherited configuration
		for (int i = 0; i < InheritedConfiguration.mItems.Count; i++)
		{
			string name = InheritedConfiguration.mItems[i].Name;
			Type type = InheritedConfiguration.mItems[i].Type;
			AddItemInternal([CascadedConfigurationPathHelper.EscapeName(name)], 0, type);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CascadedConfigurationBase"/> class
	/// (for root configurations that do not inherit from another configuration).
	/// </summary>
	/// <param name="name">Name of the configuration.</param>
	/// <param name="persistence">
	/// A persistence strategy that is responsible for persisting configuration items;<br/>
	/// <see langword="null"/> if persistence is not needed.
	/// </param>
	protected CascadedConfigurationBase(string name, ICascadedConfigurationPersistenceStrategy persistence)
	{
		Name = name;
		Path = "/";
		Sync = new object();
		PersistenceStrategy = persistence;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CascadedConfigurationBase"/> class
	/// (for child configurations, i.e. configurations that have a parent configuration).<br/>
	/// It does not matter whether the configuration is the default configuration or whether it inherits from another configuration.
	/// </summary>
	/// <param name="name">Name of the configuration.</param>
	/// <param name="parent">Parent configuration.</param>
	protected CascadedConfigurationBase(string name, CascadedConfigurationBase parent)
	{
		Debug.Assert(Monitor.IsEntered(parent.Sync), "The configuration is expected to be locked.");
		Debug.Assert(parent != null);

		Name = name;
		Path = CascadedConfigurationPathHelper.CombinePath(parent.Path, CascadedConfigurationPathHelper.EscapeName(name));
		Sync = parent.Sync;
		Parent = parent;
		PersistenceStrategy = parent.PersistenceStrategy;

		// link current configuration with the parent configuration
		int index = parent.mChildren.BinarySearch(this, ConfigurationByNameComparer.InvariantCultureIgnoreCaseComparer);
		Debug.Assert(index < 0);
		parent.mChildren.Insert(~index, this);
		parent.mIsModified = true;

		// get child configuration of the inherited configuration with the same name
		// (should already be there as configurations are added starting with the base configuration)
		if (parent.InheritedConfiguration != null)
		{
			InheritedConfiguration = parent.InheritedConfiguration.GetChildConfigurationInternal(CascadedConfigurationPathHelper.EscapeName(name), create: false);
			Debug.Assert(InheritedConfiguration != null);
		}

		// add the same items as they exist in the inherited configuration
		if (InheritedConfiguration != null)
		{
			for (int i = 0; i < InheritedConfiguration.mItems.Count; i++)
			{
				string itemName = InheritedConfiguration.mItems[i].Name;
				Type itemType = InheritedConfiguration.mItems[i].Type;
				AddItemInternal([CascadedConfigurationPathHelper.EscapeName(itemName)], 0, itemType);
			}
		}

		// let inheriting configurations create a child configuration with the specified name as well
		for (int i = 0; i < parent.mInheritingConfigurations.Count; i++)
		{
			CascadedConfigurationBase child = parent
				.mInheritingConfigurations[i]
				.GetChildConfigurationInternal(
					CascadedConfigurationPathHelper.EscapeName(name),
					create: true);

			mInheritingConfigurations.Add(child);
		}
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets child configurations of the configuration.
	/// </summary>
	public IEnumerable<CascadedConfigurationBase> Children
	{
		get
		{
			lock (Sync)
			{
				IEnumerator<CascadedConfigurationBase> enumerator = new MonitorSynchronizedEnumerator<CascadedConfigurationBase>(mChildren.GetEnumerator(), Sync);
				try
				{
					while (enumerator.MoveNext())
					{
						yield return enumerator.Current;
					}
				}
				finally
				{
					enumerator.Dispose();
				}
			}
		}
	}

	/// <summary>
	/// Gets the configuration the current configuration inherits from
	/// (<see langword="null"/> if the current configuration does not inherit from another configuration).
	/// </summary>
	public CascadedConfigurationBase InheritedConfiguration { get; }

	/// <summary>
	/// Gets a value indicating whether the configuration has been modified.
	/// </summary>
	public bool IsModified
	{
		get
		{
			lock (Sync)
			{
				for (int i = 0; i < mChildren.Count; i++)
				{
					if (mChildren[i].IsModified)
						return true;
				}

				return mIsModified;
			}
		}

		internal set
		{
			lock (Sync)
			{
				if (value)
				{
					mIsModified = true;
					return;
				}

				for (int i = 0; i < mChildren.Count; i++)
				{
					mChildren[i].IsModified = false;
				}

				mIsModified = false;
			}
		}
	}

	/// <summary>
	/// Gets the items in the configuration.
	/// </summary>
	public IEnumerable<ICascadedConfigurationItem> Items
	{
		get
		{
			lock (Sync)
			{
				IEnumerator<ICascadedConfigurationItem> enumerator = new MonitorSynchronizedEnumerator<ICascadedConfigurationItem>(mItems.GetEnumerator(), Sync);
				try
				{
					while (enumerator.MoveNext())
					{
						yield return enumerator.Current;
					}
				}
				finally
				{
					enumerator.Dispose();
				}
			}
		}
	}

	/// <summary>
	/// Gets the name of the configuration.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the parent of the configuration
	/// (<see langword="null"/> if the current configuration is a root configuration).
	/// </summary>
	public CascadedConfigurationBase Parent { get; }

	/// <summary>
	/// Gets the path of the configuration in the configuration hierarchy.
	/// </summary>
	public string Path { get; }

	/// <summary>
	/// Gets the persistence strategy to use when loading/saving the configuration.
	/// </summary>
	public ICascadedConfigurationPersistenceStrategy PersistenceStrategy { get; }

	/// <summary>
	/// Gets the root configuration.
	/// </summary>
	public CascadedConfigurationBase RootConfiguration => Parent != null ? Parent.RootConfiguration : this;

	/// <summary>
	/// Gets the object used to synchronize access to the configuration and it's items
	/// (used in conjunction with <see cref="System.Threading.Monitor"/> class or a lock() statement).
	/// </summary>
	public object Sync { get; }

	#endregion

	#region Adding an Inheriting Configuration

	/// <summary>
	/// Adds a new configuration layer inheriting from the current configuration.
	/// </summary>
	/// <param name="persistence">
	/// A persistence strategy that is responsible for persisting configuration items;<br/>
	/// <see langword="null"/> if persistence is not needed.
	/// </param>
	public CascadedConfiguration AddInheritingConfiguration(ICascadedConfigurationPersistenceStrategy persistence)
	{
		// TODO: Check whether the persistence strategy can handle paths/types of already added items...
		lock (Sync)
		{
			var inheritedRootConfiguration = new CascadedConfiguration(RootConfiguration, persistence);
			return inheritedRootConfiguration.Parent != null
				       ? (CascadedConfiguration)inheritedRootConfiguration.GetChildConfigurationInternal(Path, false)
				       : inheritedRootConfiguration;
		}
	}

	#endregion

	#region Getting a Child Configuration

	/// <summary>
	/// Gets the child configuration at the specified location.
	/// </summary>
	/// <param name="path">
	/// Relative path of the configuration to get.<br/>
	/// If a path segment contains path delimiters ('/'), escape these characters.<br/>
	/// Otherwise, the segment will be split up.<br/>
	/// The configuration helper function <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <returns>
	/// The requested child configuration;<br/>
	/// <see langword="null"/> if the child configuration at the specified path does not exist.
	/// </returns>
	/// <exception cref="ConfigurationException">
	/// <paramref name="path"/> contains a part that is not supported by the persistence strategy of the current or an inheriting configuration.
	/// </exception>
	public CascadedConfigurationBase GetChildConfiguration(string path)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		lock (Sync)
		{
			return GetChildConfigurationInternal(path, false);
		}
	}

	/// <summary>
	/// Gets the child configuration at the specified location, optionally creates new configurations on the path
	/// (for internal use only, not synchronized).
	/// </summary>
	/// <param name="path">
	/// Relative path of the configuration to get.<br/>
	/// If a path segment contains path delimiters ('/'), escape these characters.<br/>
	/// Otherwise, the segment will be split up.<br/>
	/// The configuration helper function <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="create">
	/// <see langword="true"/> to create the child configuration, if it does not exist;<br/>
	/// <see langword="false"/> to return <see langword="null"/> if the configuration does not exist.
	/// </param>
	/// <returns>
	/// The requested child configuration;<br/>
	/// <see langword="null"/> if the child configuration at the specified path does not exist and <paramref name="create"/> is <see langword="false"/>.
	/// </returns>
	/// <exception cref="ConfigurationException">
	/// <paramref name="path"/> contains a part that is not supported by the persistence strategy of the current or an inheriting configuration.
	/// </exception>
	protected CascadedConfigurationBase GetChildConfigurationInternal(string path, bool create)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The configuration is expected to be locked.");
		Debug.Assert(path != null);

		// split the configuration path into path segments (can throw ConfigurationException)
		string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(
			PersistenceStrategy,
			path,
			isItemPath: false,
			checkValidity: true);

		// get/create the configuration
		return GetChildConfigurationInternal(pathSegments, 0, pathSegments.Length, create);
	}

	/// <summary>
	/// Gets the child configuration at the specified location, optionally creates new configurations on the path
	/// (for internal use only, not synchronized).
	/// </summary>
	/// <param name="pathSegments">
	/// Path segments of the relative path of the configuration to get/create.<br/>
	/// If a path segment contains path delimiters ('/' and '\'), escape these characters.<br/>
	/// Otherwise, the segment will be split up.<br/>
	/// The configuration helper function <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="startIndex">Index in <paramref name="pathSegments"/> to start at.</param>
	/// <param name="count">Number of path segments to process.</param>
	/// <param name="create">
	/// <see langword="true"/> to create the child configuration, if it does not exist;<br/>
	/// <see langword="false"/> to return <see langword="null"/> if the configuration does not exist.
	/// </param>
	/// <returns>
	/// The requested child configuration;<br/>
	/// <see langword="null"/> if the child configuration at the specified path does not exist and <paramref name="create"/> is <see langword="false"/>.
	/// </returns>
	protected CascadedConfigurationBase GetChildConfigurationInternal(
		string[] pathSegments,
		int      startIndex,
		int      count,
		bool     create)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The configuration is expected to be locked.");
		Debug.Assert(pathSegments is { Length: > 0 });
		Debug.Assert(count > 0);

		string configurationName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[startIndex]);

		// use existing configuration, if available
		CascadedConfigurationBase configuration;
		for (int i = 0; i < mChildren.Count; i++)
		{
			configuration = mChildren[i];
			if (configuration.Name != configurationName) continue;
			return count == 1
				       ? configuration
				       : configuration.GetChildConfigurationInternal(pathSegments, startIndex: startIndex + 1, count: count - 1, create);
		}

		// configuration does not exist
		// => create and add a new child configuration with the specified name, if requested
		if (!create) return null;
		configuration = AddChildConfiguration(configurationName);

		return count == 1
			       ? configuration
			       : configuration.GetChildConfigurationInternal(pathSegments, startIndex: startIndex + 1, count: count - 1, create: true);
	}

	/// <summary>
	/// Creates a child configuration below the current configuration (for internal use only).<br/>
	/// This method does _not_ add the child configuration to the child configuration collection.
	/// </summary>
	/// <param name="name">Name of the configuration to create.</param>
	/// <returns>The created child configuration.</returns>
	protected abstract CascadedConfigurationBase AddChildConfiguration(string name);

	#endregion

	#region Adding a Configuration Item (Generic)

	/// <summary>
	/// Adds a configuration item with the specified type at the specified location (for internal use only, no synchronized).<br/>
	/// This method does _not_ validate the type or the path.
	/// </summary>
	/// <typeparam name="T">Type of the value in the configuration item.</typeparam>
	/// <param name="pathSegments">
	/// Path segments of the relative path of the configuration item to add.<br/>
	/// If a path segment contains path delimiters ('/'), escape them.<br/>
	/// Otherwise, the segment will be split up.<br/>
	/// The configuration helper function <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="startIndex">Index in <paramref name="pathSegments"/> to start at.</param>
	/// <returns>The added configuration item.</returns>
	/// <exception cref="ArgumentException">The configuration already contains an item at the specified path.</exception>
	protected internal CascadedConfigurationItem<T> AddItemInternal<T>(string[] pathSegments, int startIndex)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The configuration is expected to be locked.");
		Debug.Assert(pathSegments != null);

		// create configurations down to the specified path, if necessary
		if (startIndex + 1 < pathSegments.Length)
		{
			// the path contains child configurations
			// => dive into the appropriate configuration
			CascadedConfigurationBase configuration = GetChildConfigurationInternal(
				pathSegments,
				startIndex: startIndex + 1,
				count: pathSegments.Length - startIndex - 1,
				create: true);

			return configuration.AddItemInternal<T>(pathSegments, pathSegments.Length - 1);
		}

		Debug.Assert(startIndex == pathSegments.Length - 1);

		// ensure that the configuration at the current level does not contain an item with the specified name
		string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[^1]);
		if (mItems.Any(x => x.Name == itemName))
		{
			string path = string.Join("/", pathSegments, 0, pathSegments.Length);
			throw new ArgumentException($"The configuration already contains an item at the specified path ({path}).", nameof(pathSegments));
		}

		// add configuration item in the current configuration
		var item = new CascadedConfigurationItem<T>(this, itemName, CascadedConfigurationPathHelper.CombinePath(Path, pathSegments[^1]));
		InsertItemHonoringOrderInternal(item);

		// add corresponding configuration items in inheriting configurations
		for (int i = 0; i < mInheritingConfigurations.Count; i++)
		{
			mInheritingConfigurations[i].AddItemInternal<T>(pathSegments, startIndex);
		}

		// load the value of the item, if available
		PersistenceStrategy?.LoadItem(item);

		return item;
	}

	#endregion

	#region Adding a Configuration Item (Dynamic)

	/// <summary>
	/// Adds a configuration item with the specified type at the specified location (for internal use only, not synchronized).<br/>
	/// This method does _not_ check whether the current configuration or inheriting configurations support the item value type.
	/// </summary>
	/// <param name="pathSegments">
	/// Path segments of the relative path of the configuration item to add.<br/>
	/// If a path segment contains path delimiters ('/'), escape them.<br/>
	/// Otherwise, the segment will be split up.<br/>
	/// The configuration helper function <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="startIndex">Index in <paramref name="pathSegments"/> to start at.</param>
	/// <param name="type">Type of the value in the configuration item.</param>
	/// <returns>The added configuration item.</returns>
	/// <exception cref="ArgumentException">The configuration already contains an item at the specified path.</exception>
	protected internal ICascadedConfigurationItem AddItemInternal(string[] pathSegments, int startIndex, Type type)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The configuration is expected to be locked.");
		Debug.Assert(pathSegments != null);

		// create configurations down to the specified path, if necessary
		if (startIndex + 1 < pathSegments.Length)
		{
			// the path contains child configurations
			// => dive into the appropriate configuration
			CascadedConfigurationBase configuration = GetChildConfigurationInternal(
				pathSegments,
				startIndex: startIndex + 1,
				count: pathSegments.Length - startIndex - 1,
				create: true);

			return configuration.AddItemInternal(pathSegments, pathSegments.Length - 1, type);
		}

		Debug.Assert(startIndex == pathSegments.Length - 1);

		// ensure that the configuration at the current level does not contain an item with the specified name
		string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[^1]);
		if (mItems.Any(x => x.Name == itemName))
		{
			string path = string.Join("/", pathSegments, 0, pathSegments.Length);
			throw new ArgumentException($"The configuration already contains an item at the specified path ({path}).", nameof(pathSegments));
		}

		// add configuration item in the current configuration
		ICascadedConfigurationItem item = CreateItem(itemName, CascadedConfigurationPathHelper.CombinePath(Path, pathSegments[^1]), type);
		InsertItemHonoringOrderInternal(item);

		// add corresponding configuration items in inheriting configurations
		for (int i = 0; i < mInheritingConfigurations.Count; i++)
		{
			mInheritingConfigurations[i].AddItemInternal(pathSegments, startIndex, type);
		}

		// load the value of the item, if available
		PersistenceStrategy?.LoadItem(item);

		return item;
	}

	#endregion

	#region Getting a Configuration Item (Generic)

	#region CascadedConfigurationItem<T> GetItem<T>(string path)

	/// <summary>
	/// Gets the configuration item at the specified location.
	/// </summary>
	/// <typeparam name="T">Type of the value in the configuration item.</typeparam>
	/// <param name="path">
	/// Relative path of the configuration item to get.<br/>
	/// If a path segment contains path delimiters ('/'), escape these characters.<br/>
	/// Otherwise, the segment will be split up.<br/>
	/// The configuration helper function <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <returns>The configuration item at the specified path.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	/// <exception cref="ConfigurationException">
	/// The configuration does not contain an item at the specified path.<br/>
	/// -or-<br/>
	/// The configuration contains an item at the specified location, but the item has a different type.
	/// </exception>
	/// <remarks>
	/// Due to performance reasons this method does not validate the specified path as it is just used
	/// to look up existing items. So you should not expect that the method throws an exception when passing
	/// invalid paths.
	/// </remarks>
	public CascadedConfigurationItem<T> GetItem<T>(string path)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		// split the item path into path segments
		string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(
			PersistenceStrategy,
			path,
			isItemPath: true,
			checkValidity: false);

		lock (Sync)
		{
			return GetItemInternal<T>(pathSegments, 0);
		}
	}

	private CascadedConfigurationItem<T> GetItemInternal<T>(string[] pathSegments, int startIndex)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The configuration is expected to be locked.");

		if (startIndex + 1 < pathSegments.Length)
		{
			// the path contains child configurations
			// => dive into the appropriate configuration
			CascadedConfigurationBase configuration = GetChildConfigurationInternal(
				pathSegments,
				startIndex,
				count: pathSegments.Length - startIndex - 1,
				create: false);
			if (configuration != null) return configuration.GetItemInternal<T>(pathSegments, pathSegments.Length - 1);
			throw new ConfigurationException(
				"The configuration does not contain an item at the specified path ({0}).",
				CascadedConfigurationPathHelper.CombinePath("/", pathSegments));
		}

		Debug.Assert(startIndex == pathSegments.Length - 1);

		string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[^1]);

		for (int i = 0; i < mItems.Count; i++)
		{
			ICascadedConfigurationItem item = mItems[i];

			if (item.Name != itemName)
				continue;

			if (item.Type != typeof(T))
			{
				throw new ConfigurationException(
					"The configuration contains an item at the specified path ({0}), but with a different type (configuration item: {1}, specified: {2}).",
					item.Path,
					item.Type.FullName,
					typeof(T).FullName);
			}

			return (CascadedConfigurationItem<T>)item;
		}

		throw new ConfigurationException(
			"The configuration does not contain an item at the specified path ({0}).",
			CascadedConfigurationPathHelper.CombinePath("/", pathSegments));
	}

	#endregion

	#region bool TryGetItem<T>(string path, out CascadedConfigurationItem<T> item)

	/// <summary>
	/// Tries to get the configuration item at the specified location.
	/// </summary>
	/// <typeparam name="T">Type of the value in the configuration item.</typeparam>
	/// <param name="path">
	/// Relative path of the configuration item to get.<br/>
	/// If a path segment contains path delimiters ('/'), escape these characters.<br/>
	/// Otherwise, the segment will be split up.<br/>
	/// The configuration helper function <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="item">
	/// Receives the configuration item at the specified path;<br/>
	/// <see langword="null"/> if the item does not exist or has a different type.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if the item at the specified path exists and its value type is <typeparamref name="T"/>;<br/>
	/// otherwise <see langword="false"/>.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	/// <remarks>
	/// Due to performance reasons this method does not validate the specified path as it is just used
	/// to look up existing items. So you should not expect that the method throws an exception when passing
	/// invalid paths.
	/// </remarks>
	public bool TryGetItem<T>(string path, out CascadedConfigurationItem<T> item)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		// split the item path into path segments
		string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(
			PersistenceStrategy,
			path,
			isItemPath: true,
			checkValidity: false);

		lock (Sync)
		{
			return TryGetItemInternal(pathSegments, 0, out item);
		}
	}

	private bool TryGetItemInternal<T>(string[] pathSegments, int startIndex, out CascadedConfigurationItem<T> item)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The configuration is expected to be locked.");

		item = null;

		if (startIndex + 1 < pathSegments.Length)
		{
			// the path contains child configurations
			// => dive into the appropriate configuration
			CascadedConfigurationBase configuration = GetChildConfigurationInternal(
				pathSegments,
				startIndex,
				count: pathSegments.Length - startIndex - 1,
				create: false);

			return configuration != null && configuration.TryGetItemInternal(
				       pathSegments,
				       startIndex: pathSegments.Length - 1,
				       out item);
		}

		Debug.Assert(startIndex == pathSegments.Length - 1);

		string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[^1]);

		for (int i = 0; i < mItems.Count; i++)
		{
			if (mItems[i].Name != itemName)
				continue;

			if (mItems[i].Type != typeof(T))
				return false;

			item = (CascadedConfigurationItem<T>)mItems[i];
			return true;
		}

		return false;
	}

	#endregion

	#endregion

	#region Getting a Configuration Item (Dynamic)

	#region ICascadedConfigurationItem GetItem(string path)

	/// <summary>
	/// Gets the configuration item at the specified location.
	/// </summary>
	/// <param name="path">
	/// Relative path of the configuration item to get.
	/// If a path segment contains path delimiters ('/'), escape these characters.
	/// Otherwise, the segment will be split up.
	/// The configuration helper function <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <returns>The configuration item at the specified path.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	/// <exception cref="ConfigurationException">The configuration does not contain an item at the specified path.</exception>
	/// <remarks>
	/// Due to performance reasons this method does not validate the specified path as it is just used
	/// to look up existing items. So you should not expect that the method throws an exception when passing
	/// invalid paths.
	/// </remarks>
	public ICascadedConfigurationItem GetItem(string path)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		// split the item path into path segments
		string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(
			PersistenceStrategy,
			path,
			isItemPath: true,
			checkValidity: false);

		lock (Sync)
		{
			return GetItemInternal(pathSegments, 0);
		}
	}

	private ICascadedConfigurationItem GetItemInternal(string[] pathSegments, int startIndex)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The configuration is expected to be locked.");

		if (startIndex + 1 < pathSegments.Length)
		{
			// the path contains child configurations
			// => dive into the appropriate configuration
			CascadedConfigurationBase configuration = GetChildConfigurationInternal(
				pathSegments,
				startIndex,
				count: pathSegments.Length - startIndex - 1,
				create: false);

			if (configuration != null) return configuration.GetItemInternal(pathSegments, startIndex: pathSegments.Length - 1);
			throw new ConfigurationException(
				"The configuration does not contain an item at the specified path ({0}).",
				CascadedConfigurationPathHelper.CombinePath("/", pathSegments));
		}

		string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[startIndex]);

		for (int i = 0; i < mItems.Count; i++)
		{
			ICascadedConfigurationItem item = mItems[i];

			if (item.Name != itemName)
				continue;

			return item;
		}

		throw new ConfigurationException(
			"The configuration does not contain an item at the specified path ({0}).",
			CascadedConfigurationPathHelper.CombinePath("/", pathSegments));
	}

	#endregion

	#region bool TryGetItem(string path, out ICascadedConfigurationItem item)

	/// <summary>
	/// Tries to get the configuration item at the specified location.
	/// </summary>
	/// <param name="path">
	/// Relative path of the configuration item to get.<br/>
	/// If a path segment contains path delimiters ('/'), escape these characters.<br/>
	/// Otherwise, the segment will be split up.<br/>
	/// The configuration helper function <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="item">
	/// Receives the configuration item at the specified path;<br/>
	/// <see langword="null"/> if the item does not exist or has a different type.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if the item at the specified path exists;<br/>
	/// otherwise <see langword="false"/>.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	/// <remarks>
	/// Due to performance reasons this method does not validate the specified path as it is just used
	/// to look up existing items. So you should not expect that the method throws an exception when passing
	/// invalid paths.
	/// </remarks>
	public bool TryGetItem(string path, out ICascadedConfigurationItem item)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		// split the item path into path segments
		string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(
			PersistenceStrategy,
			path,
			isItemPath: true,
			checkValidity: false);

		lock (Sync)
		{
			return TryGetItemInternal(pathSegments, 0, out item);
		}
	}

	internal bool TryGetItemInternal(string[] pathSegments, int startIndex, out ICascadedConfigurationItem item)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The configuration is expected to be locked.");

		item = null;

		if (startIndex + 1 < pathSegments.Length)
		{
			// the path contains child configurations
			// => dive into the appropriate configuration
			CascadedConfigurationBase configuration = GetChildConfigurationInternal(
				pathSegments,
				startIndex,
				count: pathSegments.Length - startIndex - 1,
				create: false);

			return configuration != null && configuration.TryGetItemInternal(
				       pathSegments,
				       startIndex: pathSegments.Length - 1,
				       out item);
		}

		string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[startIndex]);

		for (int i = 0; i < mItems.Count; i++)
		{
			if (mItems[i].Name != itemName)
				continue;

			item = mItems[i];
			return true;
		}

		return false;
	}

	#endregion

	#endregion

	#region Getting all Items

	/// <summary>
	/// Gets all configuration items of the configuration and optionally all items of its child configurations
	/// (does not dive into the configuration inheriting from, if any).
	/// </summary>
	/// <param name="recursively">
	/// <see langword="true"/> to get the items of the child configuration as well;<br/>
	/// <see langword="false"/> to get the items of the current configuration only.
	/// </param>
	/// <returns>The requested configuration items.</returns>
	public ICascadedConfigurationItem[] GetAllItems(bool recursively)
	{
		var items = new List<ICascadedConfigurationItem>();

		lock (Sync)
		{
			for (int i = 0; i < mItems.Count; i++)
			{
				items.Add(mItems[i]);
			}

			if (!recursively) return [.. items];

			for (int i = 0; i < mChildren.Count; i++)
			{
				items.AddRange(mChildren[i].GetAllItems(true));
			}
		}

		return [.. items];
	}

	#endregion

	#region Getting the Value of an Configuration Item (Generic)

	/// <summary>
	/// Gets the value of the configuration item at the specified location,
	/// optionally the value of the item with the same path in an inherited configuration.
	/// </summary>
	/// <typeparam name="T">Type of the value to get.</typeparam>
	/// <param name="path">
	/// Relative path of the configuration item.
	/// If a path segment contains path delimiters ('/'), escape these characters.
	/// Otherwise, the segment will be split up.
	/// The configuration helper function <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="inherit">
	/// <see langword="true"/> to try to retrieve the value from the current configuration first, then check inherited configurations;<br/>
	/// <see langword="false"/> to try to retrieve the value from the current configuration only.
	/// </param>
	/// <param name="value">Receives the value of the item.</param>
	/// <returns>
	/// <see langword="true"/> if the specified value was found; otherwise <see langword="false"/>.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	/// <exception cref="ConfigurationException">
	/// The configuration contains an item at the specified location, but the item has a different type.
	/// </exception>
	/// <remarks>
	/// Due to performance reasons this method does not validate the specified path as it is just used
	/// to look up existing items. So you should not expect that the method throws an exception when passing
	/// invalid paths.
	/// </remarks>
	public bool TryGetValue<T>(string path, bool inherit, out T value)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		lock (Sync)
		{
			// split the item path into path segments
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(
				PersistenceStrategy,
				path,
				isItemPath: true,
				checkValidity: false);

			return TryGetValueInternal(pathSegments, 0, inherit, out value);
		}
	}

	private bool TryGetValueInternal<T>(
		string[] pathSegments,
		int      startIndex,
		bool     inherit,
		out T    value)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The configuration is expected to be locked.");

		if (startIndex + 1 < pathSegments.Length)
		{
			// the path contains child configurations
			// => dive into the appropriate configuration
			CascadedConfigurationBase configuration = GetChildConfigurationInternal(
				pathSegments,
				startIndex: startIndex,
				count: pathSegments.Length - startIndex - 1,
				create: false);

			if (configuration != null) return configuration.TryGetValueInternal(pathSegments, pathSegments.Length - 1, inherit, out value);
			value = default;
			return false;
		}

		Debug.Assert(startIndex == pathSegments.Length - 1);

		string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[^1]);

		for (int i = 0; i < mItems.Count; i++)
		{
			ICascadedConfigurationItem item = mItems[i];

			if (item.Name != itemName)
				continue;

			// ensure that the type of the configuration item matches the specified one
			if (item.Type != typeof(T))
			{
				string relativePath = string.Join("/", pathSegments);
				throw new ConfigurationException(
					"The configuration contains an item at the specified path ({0}), but the item has a different type (configuration item: {1}, specified: {2}).",
					relativePath,
					item.Type.FullName,
					typeof(T).FullName);
			}

			// return value, if the configuration item provides one
			if (item.HasValue)
			{
				value = (T)item.Value;
				return true;
			}
		}

		// the current configuration does not contain a configuration item with the specified name or the configuration item does not contain a valid value
		// => query the next configuration in the configuration cascade, if allowed...
		if (inherit && InheritedConfiguration != null)
		{
			return InheritedConfiguration.TryGetValueInternal(
				pathSegments,
				startIndex,
				inherit: true,
				out value);
		}

		// there is no configuration item with the specified name and a valid value in the configuration cascade
		value = default;
		return false;
	}

	#endregion

	#region Getting the Comment of an Configuration Item

	/// <summary>
	/// Gets the comment of the configuration item at the specified location,
	/// optionally the comment of the item with the same path in an inherited configuration.
	/// </summary>
	/// <param name="path">
	/// Relative path of the configuration item.
	/// If a path segment contains path delimiters ('/'), escape these characters.
	/// Otherwise, the segment will be split up.
	/// The configuration helper function <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="inherit">
	/// <see langword="true"/> to try to retrieve the comment from the current configuration first, then check inherited configurations;<br/>
	/// <see langword="false"/> to try to retrieve the comment from the current configuration only.
	/// </param>
	/// <param name="comment">Receives the comment of the item.</param>
	/// <returns>
	/// <see langword="true"/> if the specified value was found; otherwise <see langword="false"/>.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	/// <remarks>
	/// Due to performance reasons this method does not validate the specified path as it is just used
	/// to look up existing items. So you should not expect that the method throws an exception when passing
	/// invalid paths.
	/// </remarks>
	public bool TryGetComment(string path, bool inherit, out string comment)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		lock (Sync)
		{
			// split the item path into path segments
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(
				PersistenceStrategy,
				path,
				isItemPath: true,
				checkValidity: false);

			return TryGetCommentInternal(pathSegments, startIndex: 0, inherit, out comment);
		}
	}

	private bool TryGetCommentInternal(
		string[]   pathSegments,
		int        startIndex,
		bool       inherit,
		out string comment)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The configuration is expected to be locked.");

		if (startIndex + 1 < pathSegments.Length)
		{
			// the path contains child configurations
			// => dive into the appropriate configuration
			CascadedConfigurationBase configuration = GetChildConfigurationInternal(
				pathSegments,
				startIndex,
				count: pathSegments.Length - startIndex - 1,
				create: false);

			if (configuration != null) configuration.TryGetComment(pathSegments[^1], inherit, out comment);
			comment = default;
			return false;
		}

		Debug.Assert(startIndex == pathSegments.Length - 1);

		string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[^1]);

		// query the current configuration first
		for (int i = 0; i < mItems.Count; i++)
		{
			ICascadedConfigurationItem item = mItems[i];
			if (item.Name == itemName)
			{
				if (!item.HasComment) break;
				comment = item.Comment;
				return true;
			}
		}

		// query the inherited configuration
		if (inherit && InheritedConfiguration != null)
		{
			return InheritedConfiguration.TryGetCommentInternal(
				pathSegments,
				startIndex,
				inherit: true,
				out comment);
		}

		// no comment found for the specified item
		comment = default;
		return false;
	}

	#endregion

	#region Creating an Item Dynamically

	/// <summary>
	/// Creates a new configuration item with the specified name and type.
	/// </summary>
	/// <param name="name">Name of the configuration item to create.</param>
	/// <param name="path">Path of the configuration item in the configuration hierarchy.</param>
	/// <param name="type">Type of the configuration item value.</param>
	/// <returns>The created configuration item.</returns>
	protected ICascadedConfigurationItem CreateItem(string name, string path, Type type)
	{
		Type itemType = typeof(CascadedConfigurationItem<>).MakeGenericType(type);
		ConstructorInfo constructor = itemType.GetConstructor(
			BindingFlags.NonPublic | BindingFlags.Instance,
			null,
			[typeof(CascadedConfiguration), typeof(string), typeof(string)],
			null);

		Debug.Assert(constructor != null, nameof(constructor) + " != null");
		return (ICascadedConfigurationItem)constructor.Invoke([this, name, path]);
	}

	/// <summary>
	/// Creates a new configuration item with the specified name, type and value.
	/// </summary>
	/// <param name="name">Name of the configuration item to create.</param>
	/// <param name="path">Path of the configuration item in the configuration hierarchy.</param>
	/// <param name="type">Type of the configuration item value.</param>
	/// <param name="value">Value of the configuration item.</param>
	/// <returns>The created configuration item.</returns>
	protected ICascadedConfigurationItem CreateItem(
		string name,
		string path,
		Type   type,
		object value)
	{
		Type itemType = typeof(CascadedConfigurationItem<>).MakeGenericType(type);
		ConstructorInfo constructor = itemType.GetConstructor(
			BindingFlags.NonPublic | BindingFlags.Instance,
			null,
			[typeof(CascadedConfiguration), typeof(string), typeof(string), type],
			null);

		Debug.Assert(constructor != null, nameof(constructor) + " != null");
		return (ICascadedConfigurationItem)constructor.Invoke([this, name, path, value]);
	}

	#endregion

	#region Inserting an Item

	/// <summary>
	/// Inserts the specified item into <see cref="mItems"/> honoring that the collection is sorted by item names.
	/// </summary>
	/// <param name="item"></param>
	protected void InsertItemHonoringOrderInternal(ICascadedConfigurationItem item)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The configuration is expected to be locked.");
		Debug.Assert(item != null);

		int index = mItems.BinarySearch(item, ItemByNameComparer.InvariantCultureIgnoreCaseComparer);
		Debug.Assert(index < 0);
		mItems.Insert(~index, item);
	}

	#endregion

	#region Raising Events

	/// <summary>
	/// Notifies configurations deriving from the current configuration of a change to the value of a configuration item.
	/// </summary>
	/// <param name="item">Configuration item that has changed.</param>
	internal void NotifyItemValueChanged<T>(CascadedConfigurationItem<T> item)
	{
		string escapedItemName = CascadedConfigurationPathHelper.EscapeName(item.Name);

		for (int i = 0; i < mInheritingConfigurations.Count; i++)
		{
			CascadedConfigurationBase configuration = mInheritingConfigurations[i];
			CascadedConfigurationItem<T> derivedItem = configuration.GetItem<T>(escapedItemName);
			Debug.Assert(derivedItem != null);
			if (derivedItem.HasValue) continue;
			derivedItem.OnPropertyChanged(nameof(CascadedConfigurationItem<T>.Value));
			configuration.NotifyItemValueChanged(item);
		}

		mIsModified = true;
	}

	/// <summary>
	/// Notifies configurations deriving from the current configuration of a change to the comment of a configuration item.
	/// </summary>
	/// <param name="item">Configuration item that has changed.</param>
	internal void NotifyItemCommentChanged<T>(CascadedConfigurationItem<T> item)
	{
		string escapedItemName = CascadedConfigurationPathHelper.EscapeName(item.Name);

		for (int i = 0; i < mInheritingConfigurations.Count; i++)
		{
			CascadedConfigurationBase configuration = mInheritingConfigurations[i];
			CascadedConfigurationItem<T> derivedItem = configuration.GetItem<T>(escapedItemName);
			Debug.Assert(derivedItem != null);
			if (derivedItem.HasComment) continue;
			derivedItem.OnPropertyChanged(nameof(CascadedConfigurationItem<T>.Comment));
			configuration.NotifyItemCommentChanged(item);
		}

		mIsModified = true;
	}

	#endregion

	#region Argument Check Helpers

	/// <summary>
	/// Throws an exception if the specified item value type cannot be handled by the persistence strategy
	/// (see <see cref="PersistenceStrategy"/>) of the current configuration or persistence strategies of inheriting configurations.
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <exception cref="ConfigurationException">
	/// The specified type is not supported by the persistence strategy of the current configuration or an inheriting configurations.
	/// </exception>
	internal void EnsureThatPersistenceStrategyCanHandleValueType(Type type)
	{
		// check persistence strategy of the current configuration
		CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanHandleValueType(PersistenceStrategy, type);

		// ensure persistence strategies of inheriting configurations
		for (int i = 0; i < mInheritingConfigurations.Count; i++)
		{
			mInheritingConfigurations[i].EnsureThatPersistenceStrategyCanHandleValueType(type);
		}
	}

	#endregion
}
