///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using GriffinPlus.Lib.Threading;

// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// A cascadable configuration allowing to build hierarchical configurations with multiple levels of inheritance and
/// different kinds of persistence.
/// </summary>
/// <remarks>
/// This class provides everything that is needed to build up a configuration system with multiple levels of inheritance
/// by chaining configurations together. The base configuration (i.e. a configuration that does not inherit from any
/// other configuration) must always provide a value for each and every configuration item. Therefore, it is recommended
/// to populate the base configuration with default settings. Configurations deriving from the base configuration
/// may hide default settings by overwriting configuration items. A query will always return the value of the most specific
/// configuration item that provides a value.
/// 
/// Any configuration can have multiple child configurations that allow to create hierarchical configurations.
/// </remarks>
[DebuggerDisplay("Configuration | Path: {" + nameof(Path) + "}")]
public class CascadedConfiguration
{
	private readonly List<CascadedConfiguration>              mChildren                 = [];
	private readonly List<ICascadedConfigurationItemInternal> mItems                    = [];
	private readonly List<CascadedConfiguration>              mInheritingConfigurations = [];
	private          bool                                     mIsModified;

	/// <summary>
	/// Initializes a new instance of the <see cref="CascadedConfiguration"/> class
	/// (for root configurations that do not inherit from another configuration).
	/// </summary>
	/// <param name="name">Name of the configuration.</param>
	/// <param name="persistence">
	/// A persistence strategy that is responsible for persisting configuration items
	/// (<c>null</c>, if persistence is not needed).
	/// </param>
	public CascadedConfiguration(string name, ICascadedConfigurationPersistenceStrategy persistence)
	{
		Name = name;
		Path = "/";
		Sync = new object();
		PersistenceStrategy = persistence;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CascadedConfiguration"/> class
	/// (for root configurations that inherit from another configuration).
	/// </summary>
	/// <param name="configurationToInheritFrom">
	/// Configuration to inherit from, i.e. this configuration is queried, if the current configuration does not provide
	/// a value for a configuration item.
	/// </param>
	/// <param name="persistence">
	/// A persistence strategy that is responsible for persisting configuration items
	/// (<c>null</c>, if persistence is not needed).
	/// </param>
	public CascadedConfiguration(CascadedConfiguration configurationToInheritFrom, ICascadedConfigurationPersistenceStrategy persistence)
	{
		InheritedConfiguration = configurationToInheritFrom;
		mInheritingConfigurations = [];
		Name = InheritedConfiguration.Name;
		Sync = InheritedConfiguration.Sync;
		Path = InheritedConfiguration.Path;
		PersistenceStrategy = persistence;
		lock (Sync)
		{
			InheritedConfiguration.mInheritingConfigurations.Add(this);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CascadedConfiguration"/> class (for child configurations,
	/// i.e. configurations that have a parent configuration). It does not matter whether the configuration is
	/// a base configuration or whether it inherits from another configuration.
	/// </summary>
	/// <param name="name">Name of the configuration.</param>
	/// <param name="parent">Parent configuration.</param>
	protected CascadedConfiguration(string name, CascadedConfiguration parent)
	{
		lock (parent.Sync)
		{
			Name = name;
			Path = CascadedConfigurationPathHelper.CombinePath(parent.Path, CascadedConfigurationPathHelper.EscapeName(name));
			Sync = parent.Sync;
			Parent = parent;
			PersistenceStrategy = parent.PersistenceStrategy;
			parent.mChildren.Add(this);
			parent.mIsModified = true;
			if (parent.InheritedConfiguration == null) return;
			InheritedConfiguration = parent.InheritedConfiguration.GetChildConfiguration(CascadedConfigurationPathHelper.EscapeName(name), true);
			InheritedConfiguration.mInheritingConfigurations.Add(this);
			parent.InheritedConfiguration.mIsModified = true;
		}
	}

	/// <summary>
	/// Gets the name of the configuration.
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Gets the path of the configuration in the configuration hierarchy.
	/// </summary>
	public string Path { get; private set; }

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
	/// Gets child configurations of the configuration.
	/// </summary>
	public IEnumerable<CascadedConfiguration> Children
	{
		get
		{
			lock (Sync)
			{
				IEnumerator<CascadedConfiguration> enumerator = new MonitorSynchronizedEnumerator<CascadedConfiguration>(mChildren.GetEnumerator(), Sync);
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
	/// Gets the configuration the current configuration inherits from.
	/// (<c>null</c>, if the current configuration does not inherit from another configuration).
	/// </summary>
	public CascadedConfiguration InheritedConfiguration { get; private set; }

	/// <summary>
	/// Gets the parent of the configuration
	/// (<c>null</c>, if the current configuration is a root configuration).
	/// </summary>
	public CascadedConfiguration Parent { get; private set; }

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
	/// Gets the root configuration.
	/// </summary>
	public CascadedConfiguration RootConfiguration => Parent != null ? Parent.RootConfiguration : this;

	/// <summary>
	/// Gets the persistence strategy to use when loading/saving the configuration.
	/// </summary>
	public ICascadedConfigurationPersistenceStrategy PersistenceStrategy { get; private set; }

	/// <summary>
	/// Gets the object used to synchronize access to the configuration and it's items
	/// (used in conjunction with <see cref="System.Threading.Monitor"/> class or a lock() statement).
	/// </summary>
	public object Sync { get; }

	/// <summary>
	/// Adds a configuration item with the specified type at the specified location, if it does not exist, yet.
	/// </summary>
	/// <typeparam name="T">Type of the value in the configuration item.</typeparam>
	/// <param name="path">
	/// Relative path of the configuration item to add. If a path segment contains path delimiters ('/' and '\'),
	/// escape these characters. Otherwise, the segment will be split up. The configuration helper function
	/// <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <returns>The item at the specified path.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>.</exception>
	/// <exception cref="ConfigurationException">
	/// The configuration already contains an item with the specified name, but with a different type -or-
	/// The specified item type or value is not supported by the persistence strategy.
	/// </exception>
	public CascadedConfigurationItem<T> SetItem<T>(string path)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		lock (Sync)
		{
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(PersistenceStrategy, path, true, true);

			if (pathSegments.Length > 1)
			{
				// the path contains child configurations
				// => dive into the appropriate configuration
				string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
				CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, true);
				return configuration.SetItem<T>(pathSegments[^1]);
			}

			CascadedConfigurationItem<T> item;
			string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[0]);

			for (int i = 0; i < mItems.Count; i++)
			{
				ICascadedConfigurationItem ci = mItems[i];

				if (ci.Name != itemName)
					continue;

				if (ci.Type != typeof(T))
					throw new ConfigurationException("The configuration already contains an item with the specified name, but with a different type.");

				item = (CascadedConfigurationItem<T>)ci;
				return item;
			}

			// check whether the persistence strategy can handle the type
			CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanHandleValueType(PersistenceStrategy, typeof(T));

			item = new CascadedConfigurationItem<T>(itemName, CascadedConfigurationPathHelper.CombinePath(Path, pathSegments));
			mItems.Add(item);
			item.Configuration = this;
			if (PersistenceStrategy == null) return item;
			bool wasModified = mIsModified;
			PersistenceStrategy.LoadItem(item);
			mIsModified = wasModified;

			return item;
		}
	}

	/// <summary>
	/// Adds a configuration item with the specified type at the specified location in the configuration.
	/// </summary>
	/// <typeparam name="T">Type of the value in the configuration item.</typeparam>
	/// <param name="path">
	/// Relative path of the configuration item to add. If a path segment contains path delimiters ('/' and '\'),
	/// escape these characters. Otherwise, the segment will be split up. The configuration helper function
	/// <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="value">Initial value of the configuration item.</param>
	/// <returns>The item at the specified path.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>.</exception>
	/// <exception cref="ConfigurationException">
	/// The configuration already contains an item with the specified name, but with a different type -or-
	/// The specified item type or value is not supported by the persistence strategy.
	/// </exception>
	public CascadedConfigurationItem<T> SetValue<T>(string path, T value)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		lock (Sync)
		{
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(PersistenceStrategy, path, true, true);

			if (pathSegments.Length > 1)
			{
				// the path contains child configurations
				// => dive into the appropriate configuration
				string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
				CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, true);
				return configuration.SetValue(pathSegments[^1], value);
			}

			CascadedConfigurationItem<T> item;
			string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[0]);

			for (int i = 0; i < mItems.Count; i++)
			{
				ICascadedConfigurationItem ci = mItems[i];

				if (ci.Name != itemName)
					continue;

				// ensure the specified type is the same as the type of the existing configuration item
				if (ci.Type != typeof(T))
					throw new ConfigurationException("The configuration already contains an item with the specified name, but with a different type.");

				// check whether the persistence strategy accepts the specified value for that configuration item
				CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanAssignValue(PersistenceStrategy, ci.Type, value);

				item = (CascadedConfigurationItem<T>)ci;
				item.Value = value;
				return item;
			}

			// check whether the persistence strategy can handle the type
			CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanHandleValueType(PersistenceStrategy, typeof(T));

			// check whether the persistence strategy accepts the specified value for that configuration item
			CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanAssignValue(PersistenceStrategy, typeof(T), value);

			item = new CascadedConfigurationItem<T>(itemName, CascadedConfigurationPathHelper.CombinePath(Path, pathSegments)) { Configuration = this };
			mItems.Add(item);
			item.Value = value; // sets the 'modified' flag

			return item;
		}
	}

	/// <summary>
	/// Adds a configuration item with the specified type at the specified location in the configuration, if it does not exist, yet.
	/// </summary>
	/// <param name="type">Type of the value in the configuration item.</param>
	/// <param name="path">
	/// Relative path of the configuration item to add. If a path segment contains path delimiters ('/' and '\'),
	/// escape these characters. Otherwise, the segment will be split up. The configuration helper function
	/// <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <returns>The item at the specified path.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> or <paramref name="type"/> is <c>null</c>.</exception>
	/// <exception cref="ConfigurationException">
	/// The configuration already contains an item with the specified name, but with a different type -or-
	/// The specified item type is not supported by the persistence strategy.
	/// </exception>
	public ICascadedConfigurationItem SetItem(string path, Type type)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));
		if (type == null) throw new ArgumentNullException(nameof(type));

		lock (Sync)
		{
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(PersistenceStrategy, path, true, true);

			if (pathSegments.Length > 1)
			{
				// the path contains child configurations
				// => dive into the appropriate configuration
				string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
				CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, true);
				return configuration.SetItem(pathSegments[^1], type);
			}

			string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[0]);

			for (int i = 0; i < mItems.Count; i++)
			{
				ICascadedConfigurationItem ci = mItems[i];

				if (ci.Name != itemName)
					continue;

				// ensure the specified type is the same as the type of the existing configuration item
				if (ci.Type != type)
					throw new ConfigurationException("The configuration already contains an item with the specified name, but with a different type.");

				return ci;
			}

			// check whether the persistence strategy can handle the type
			CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanHandleValueType(PersistenceStrategy, type);

			// add configuration item
			ICascadedConfigurationItemInternal item = CreateItem(itemName, CascadedConfigurationPathHelper.CombinePath(Path, pathSegments), type);
			item.SetConfiguration(this);
			mItems.Add(item);
			if (PersistenceStrategy == null) return item;
			bool wasModified = mIsModified;
			PersistenceStrategy.LoadItem(item);
			mIsModified = wasModified;

			return item;
		}
	}

	/// <summary>
	/// Adds a configuration item with the specified type at the specified location in the configuration.
	/// </summary>
	/// <param name="path">
	/// Relative path of the configuration item to add. If a path segment contains path delimiters ('/' and '\'),
	/// escape these characters. Otherwise, the segment will be split up. The configuration helper function
	/// <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="type">Type of the value in the configuration item.</param>
	/// <param name="value">Initial value of the configuration item.</param>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> or <paramref name="type"/> is <c>null</c>.</exception>
	/// <exception cref="ConfigurationException">
	/// The configuration already contains an item with the specified name, but with a different type -or-
	/// The specified item type or value is not supported by the persistence strategy.
	/// </exception>
	public ICascadedConfigurationItem SetValue(string path, Type type, object value)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));
		if (type == null) throw new ArgumentNullException(nameof(type));

		lock (Sync)
		{
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(PersistenceStrategy, path, true, true);

			if (pathSegments.Length > 1)
			{
				// the path contains child configurations
				// => dive into the appropriate configuration
				string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
				CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, true);
				return configuration.SetValue(pathSegments[^1], type, value);
			}

			string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[0]);

			for (int i = 0; i < mItems.Count; i++)
			{
				ICascadedConfigurationItem ci = mItems[i];

				if (ci.Name != itemName)
					continue;

				// ensure the specified type is the same as the type of the existing configuration item
				if (ci.Type != type)
					throw new ConfigurationException("The configuration already contains an item with the specified name, but with a different type.");

				// check whether the persistence strategy accepts the specified value for that configuration item
				CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanAssignValue(PersistenceStrategy, ci.Type, value);

				ci.Value = value; // sets the 'modified' flag
				return ci;
			}

			// check whether the persistence strategy can handle the type
			CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanHandleValueType(PersistenceStrategy, type);

			// check whether the persistence strategy accepts the specified value for that configuration item
			CascadedConfigurationPathHelper.EnsureThatPersistenceStrategyCanAssignValue(PersistenceStrategy, type, value);

			// add configuration item
			ICascadedConfigurationItemInternal item = CreateItem(itemName, CascadedConfigurationPathHelper.CombinePath(Path, pathSegments), type);
			item.SetConfiguration(this);
			mItems.Add(item);

			// everything is ok, set the configuration item
			item.Value = value; // sets the 'modified' flag
			return item;
		}
	}

	/// <summary>
	/// Removes the configuration item at the specified location.
	/// </summary>
	/// <param name="path">
	/// Relative path of the configuration item to remove. If a path segment contains path delimiters ('/' and '\'),
	/// escape these characters. Otherwise, the segment will be split up. The configuration helper function
	/// <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>.</exception>
	/// <exception cref="ConfigurationException">
	/// <paramref name="path"/> contains parts that are not supported by the persistence strategy.
	/// </exception>
	public bool RemoveItem(string path)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		lock (Sync)
		{
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(PersistenceStrategy, path, true, true);

			if (pathSegments.Length > 1)
			{
				// the path contains child configurations
				// => dive into the appropriate configuration
				string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
				CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, false);
				return configuration != null && configuration.RemoveItem(pathSegments[^1]);
			}

			string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[0]);

			for (int i = 0; i < mItems.Count; i++)
			{
				ICascadedConfigurationItemInternal item = mItems[i];
				if (item.Name != itemName) continue;
				item.SetConfiguration(null);
				mItems.RemoveAt(i);
				mIsModified = true;
				return true;
			}

			return false;
		}
	}

	/// <summary>
	/// Clears the entire configuration.
	/// </summary>
	public void Clear()
	{
		lock (Sync)
		{
			// remove all children
			// -------------------------------------------------------------------------
			for (int i = 0; i < mChildren.Count; i++)
			{
				CascadedConfiguration configuration = mChildren[i];

				// let children clear their collections
				configuration.Clear();

				// remove current configuration
				configuration.Name = "<<< Deleted >>>";
				configuration.Path = "/";
				configuration.Parent = null;
				configuration.PersistenceStrategy = null;
				if (configuration.InheritedConfiguration == null) continue;
				configuration.InheritedConfiguration.mInheritingConfigurations.Remove(this);
				configuration.InheritedConfiguration = null;
			}

			if (mChildren.Count > 0)
			{
				mChildren.Clear();
				mIsModified = true;
			}

			// remove all items
			// -------------------------------------------------------------------------
			for (int i = 0; i < mItems.Count; i++)
			{
				mItems[i].SetConfiguration(null);
			}

			if (mItems.Count <= 0) return;
			mItems.Clear();
			mIsModified = true;
		}
	}

	/// <summary>
	/// Resets all items of the current configuration (and optionally all items of child configurations as well),
	/// so inherited item values become visible.
	/// </summary>
	/// <param name="recursively">
	/// <c>true</c> to reset items of child configurations as well;
	/// <c>false</c> to reset items of the current configuration.
	/// </param>
	public void ResetItems(bool recursively = false)
	{
		lock (Sync)
		{
			if (recursively)
			{
				for (int i = 0; i < mChildren.Count; i++)
				{
					mChildren[i].ResetItems(true);
				}
			}

			for (int i = 0; i < mItems.Count; i++)
			{
				mItems[i].ResetValue();
			}
		}
	}

	/// <summary>
	/// Gets the value of the configuration item at the specified location.
	/// </summary>
	/// <typeparam name="T">Type of the value to get.</typeparam>
	/// <param name="path">
	/// Relative path of the configuration item. If a path segment contains path delimiters ('/' and '\'),
	/// escape these characters. Otherwise, the segment will be split up. The configuration helper function
	/// <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="inherit">
	/// <c>true</c> to try to retrieve the value from the current configuration first, then check inherited configurations;
	/// <c>false</c> to try to retrieve the value from the current configuration only.
	/// </param>
	/// <returns>Value of the configuration value.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>.</exception>
	/// <exception cref="ConfigurationException">
	/// <paramref name="path"/> contains parts that are not supported by the persistence strategy -or-
	/// The configuration does not contain an item at the specified location -or-
	/// The configuration contains an item at the specified location, but the item has a different type.
	/// </exception>
	public T GetValue<T>(string path, bool inherit = true)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		lock (Sync)
		{
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(PersistenceStrategy, path, true, true);

			if (pathSegments.Length > 1)
			{
				// the path contains child configurations
				// => dive into the appropriate configuration
				string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
				CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, false);
				if (configuration == null)
				{
					if (inherit && InheritedConfiguration != null)
						configuration = InheritedConfiguration.GetChildConfiguration(childConfigurationPath, false);
				}

				if (configuration == null)
				{
					throw new ConfigurationException(
						"The configuration does not contain an item at the specified path ({0}) or the item does not contain a valid value.",
						path);
				}

				return configuration.GetValue<T>(pathSegments[^1], inherit);
			}

			string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[0]);

			for (int i = 0; i < mItems.Count; i++)
			{
				ICascadedConfigurationItem item = mItems[i];

				if (item.Name != itemName)
					continue;

				// ensure that the type of the configuration item matches the specified one
				if (item.Type != typeof(T))
				{
					throw new ConfigurationException(
						"The configuration contains an item at the specified path, but the item has a different type (configuration item: {0}, specified: {1}).",
						item.Type.FullName,
						typeof(T).FullName);
				}

				// return value, if the configuration item provides one
				if (item.HasValue)
					return (T)item.Value;
			}

			// the current configuration does not contain a configuration item with the specified name or the configuration item does not contain a valid value
			// => query the next configuration in the configuration cascade, if allowed...
			if (inherit && InheritedConfiguration != null)
				return InheritedConfiguration.GetValue<T>(path);

			// there is no configuration item with the specified name and a valid value in the configuration cascade
			throw new ConfigurationException(
				"The configuration does not contain an item with the specified name ({0}) or the item does not contain a valid value.",
				path);
		}
	}

	/// <summary>
	/// Gets the comment of the configuration item at the specified location.
	/// </summary>
	/// <param name="path">
	/// Relative path of the configuration item. If a path segment contains path delimiters ('/' and '\'),
	/// escape these characters. Otherwise, the segment will be split up. The configuration helper function
	/// <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="inherit">
	/// <c>true</c> to try to retrieve the comment from an item in the current configuration first, then check inherited configurations;
	/// <c>false</c> to try to retrieve the comment from the current configuration only.
	/// </param>
	/// <returns>
	/// Comment of the configuration item;
	/// <c>null</c> if the item does not exist or does not have a comment.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>.</exception>
	/// <exception cref="ConfigurationException">
	/// <paramref name="path"/> contains parts that are not supported by the persistence strategy.
	/// </exception>
	public string GetComment(string path, bool inherit = true)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		lock (Sync)
		{
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(PersistenceStrategy, path, true, true);

			if (pathSegments.Length > 1)
			{
				// the path contains child configurations
				// => dive into the appropriate configuration
				string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
				CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, false);
				if (configuration != null) return configuration.GetComment(pathSegments[^1], inherit);
				if (inherit && InheritedConfiguration != null)
					configuration = InheritedConfiguration.GetChildConfiguration(childConfigurationPath, false);

				return configuration?.GetComment(pathSegments[^1], true);
			}

			string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[0]);

			// query the current configuration first
			for (int i = 0; i < mItems.Count; i++)
			{
				ICascadedConfigurationItem item = mItems[i];
				if (item.Name == itemName && item.HasComment)
					return item.Comment;
			}

			return inherit ? InheritedConfiguration?.GetComment(path, true) : null;
		}
	}

	/// <summary>
	/// Gets the configuration item at the specified location.
	/// </summary>
	/// <typeparam name="T">Type of the value in the configuration item.</typeparam>
	/// <param name="path">
	/// Relative path of the configuration item to get. If a path segment contains path delimiters ('/' and '\'),
	/// escape these characters. Otherwise, the segment will be split up. The configuration helper function
	/// <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <returns>
	/// The configuration item at the specified path;
	/// <c>null</c>, if the configuration does not contain a configuration item with the specified name.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>.</exception>
	/// <exception cref="ConfigurationException">
	/// <paramref name="path"/> contains parts that are not supported by the persistence strategy -or-
	/// The configuration contains an item with the specified name, but the item has a different type.
	/// </exception>
	public CascadedConfigurationItem<T> GetItem<T>(string path)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		lock (Sync)
		{
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(PersistenceStrategy, path, true, true);

			if (pathSegments.Length > 1)
			{
				// the path contains child configurations
				// => dive into the appropriate configuration
				string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
				CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, false);
				return configuration?.GetItem<T>(pathSegments[^1]);
			}

			string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[0]);

			for (int i = 0; i < mItems.Count; i++)
			{
				ICascadedConfigurationItem item = mItems[i];
				if (item.Name != itemName) continue;

				if (item.Type != typeof(T))
				{
					throw new ConfigurationException(
						"The configuration contains an item at the specified path, but with a different type (configuration item: {0}, specified: {1}).",
						item.Type.FullName,
						typeof(T).FullName);
				}

				return (CascadedConfigurationItem<T>)item;
			}

			return null;
		}
	}

	/// <summary>
	/// Gets the configuration item at the specified location.
	/// </summary>
	/// <param name="path">
	/// Relative path of the configuration item to get. If a path segment contains path delimiters ('/' and '\'),
	/// escape these characters. Otherwise, the segment will be split up. The configuration helper function
	/// <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <returns>
	/// The configuration item at the specified location;
	/// <c>null</c>, if the configuration does not contain a configuration item at the specified location.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>.</exception>
	/// <exception cref="ConfigurationException">
	/// <paramref name="path"/> contains parts that are not supported by the persistence strategy.
	/// </exception>
	public ICascadedConfigurationItem GetItem(string path)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		lock (Sync)
		{
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(PersistenceStrategy, path, true, true);

			if (pathSegments.Length > 1)
			{
				// the path contains child configurations
				// => dive into the appropriate configuration
				string childConfigurationPath = string.Join("/", pathSegments, 0, pathSegments.Length - 1);
				CascadedConfiguration configuration = GetChildConfiguration(childConfigurationPath, false);
				return configuration?.GetItem(pathSegments[^1]);
			}

			string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[0]);

			for (int i = 0; i < mItems.Count; i++)
			{
				ICascadedConfigurationItem item = mItems[i];
				if (item.Name == itemName)
					return item;
			}

			return null;
		}
	}

	/// <summary>
	/// Gets all configuration items of the configuration and optionally all items of its child configurations
	/// (does not dive into the configuration inheriting from, if any).
	/// </summary>
	/// <param name="recursively">
	/// <c>true</c> to get the items of the child configuration as well;<br/>
	/// <c>false</c> to get the items of the current configuration only.
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

	/// <summary>
	/// Gets the child configuration at the specified location (optionally creates new configurations on the path).
	/// </summary>
	/// <param name="path">
	/// Relative path of the configuration to get/create. If a path segment contains path delimiters ('/' and '\'),
	/// escape these characters. Otherwise, the segment will be split up. The configuration helper function
	/// <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="create">
	/// <c>true</c> to create the child configuration, if it does not exist;<br/>
	/// <c>false</c> to return <c>null</c>, if the configuration does not exist.
	/// </param>
	/// <returns>
	/// The requested child configuration;<br/>
	/// <c>null</c>, if the child configuration at the specified path does not exist and <paramref name="create"/> is <c>false</c>.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>.</exception>
	/// <exception cref="ConfigurationException">
	/// <paramref name="path"/> contains parts that are not supported by the persistence strategy.
	/// </exception>
	public CascadedConfiguration GetChildConfiguration(string path, bool create)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		lock (Sync)
		{
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(PersistenceStrategy, path, false, true);
			string childConfigurationPath = string.Join("/", pathSegments, 1, pathSegments.Length - 1);
			string configurationName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[0]);

			CascadedConfiguration configuration;
			for (int i = 0; i < mChildren.Count; i++)
			{
				configuration = mChildren[i];
				if (configuration.Name != configurationName) continue;
				return pathSegments.Length == 1 ? configuration : configuration.GetChildConfiguration(childConfigurationPath, create);
			}

			// configuration does not exist
			// => create and add a new child configuration with the specified name, if requested
			if (!create) return null;
			configuration = AddChildConfiguration(configurationName);
			return pathSegments.Length == 1 ? configuration : configuration.GetChildConfiguration(childConfigurationPath, true);
		}
	}

	/// <summary>
	/// Creates new instance of the <see cref="CascadedConfiguration"/> class for use as a child configuration of the current configuration
	/// (the caller will do the integration of the object into the configuration).
	/// </summary>
	/// <param name="name">Name of the configuration to create.</param>
	/// <returns>The created child configuration.</returns>
	protected virtual CascadedConfiguration AddChildConfiguration(string name)
	{
		return new CascadedConfiguration(name, this); // links itself to the current configuration
	}

	/// <summary>
	/// Loads the current settings from the storage backend (<see cref="CascadedConfiguration"/> does not have a backend storage,
	/// but derived classes may override this method to implement a storage backend).
	/// </summary>
	/// <exception cref="NotSupportedException">The configuration does not support persistence.</exception>
	public virtual void Load()
	{
		if (PersistenceStrategy == null)
			throw new NotSupportedException("The configuration does not support persistence.");

		PersistenceStrategy.Load(this);
		IsModified = false; // works recursively
	}

	/// <summary>
	/// Saves the current settings to the storage backend (<see cref="CascadedConfiguration"/> does not have a backend storage,
	/// but derived classes may override this method to implement a storage backend).
	/// </summary>
	/// <param name="flags">Flags controlling the save behavior.</param>
	/// <exception cref="NotSupportedException">The configuration does not support persistence.</exception>
	public virtual void Save(CascadedConfigurationSaveFlags flags)
	{
		if (PersistenceStrategy == null)
			throw new NotSupportedException("The configuration does not support persistence.");

		PersistenceStrategy.Save(this, flags);
		IsModified = false; // works recursively
	}

	/// <summary>
	/// Notifies configurations deriving from the current one of a change to the value of a configuration item.
	/// </summary>
	/// <param name="item">Configuration item that has changed.</param>
	internal void NotifyItemValueChanged<T>(CascadedConfigurationItem<T> item)
	{
		string escapedItemName = CascadedConfigurationPathHelper.EscapeName(item.Name);

		for (int i = 0; i < mInheritingConfigurations.Count; i++)
		{
			CascadedConfiguration configuration = mInheritingConfigurations[i];
			CascadedConfigurationItem<T> derivedItem = configuration.GetItem<T>(escapedItemName);
			if (derivedItem == null || derivedItem.HasValue) continue;
			derivedItem.OnPropertyChanged(nameof(CascadedConfigurationItem<T>.Value));
			configuration.NotifyItemValueChanged(item);
		}

		mIsModified = true;
	}

	/// <summary>
	/// Notifies configurations deriving from the current one of a change to the comment of a configuration item.
	/// </summary>
	/// <param name="item">Configuration item that has changed.</param>
	internal void NotifyItemCommentChanged<T>(CascadedConfigurationItem<T> item)
	{
		string escapedItemName = CascadedConfigurationPathHelper.EscapeName(item.Name);

		for (int i = 0; i < mInheritingConfigurations.Count; i++)
		{
			CascadedConfiguration configuration = mInheritingConfigurations[i];
			CascadedConfigurationItem<T> derivedItem = configuration.GetItem<T>(escapedItemName);
			if (derivedItem == null || derivedItem.HasComment) continue;
			derivedItem.OnPropertyChanged(nameof(CascadedConfigurationItem<T>.Comment));
			configuration.NotifyItemCommentChanged(item);
		}

		mIsModified = true;
	}

	/// <summary>
	/// Creates a new configuration item with the specified name and type.
	/// </summary>
	/// <param name="name">Name of the configuration item to create.</param>
	/// <param name="path">Path of the configuration item in the configuration hierarchy.</param>
	/// <param name="type">Type of the configuration item value.</param>
	/// <returns>The created configuration item.</returns>
	private static ICascadedConfigurationItemInternal CreateItem(string name, string path, Type type)
	{
		Type itemType = typeof(CascadedConfigurationItem<>).MakeGenericType(type);
		ConstructorInfo constructor = itemType.GetConstructor(
			BindingFlags.NonPublic | BindingFlags.Instance,
			null,
			[typeof(string), typeof(string)],
			null);

		Debug.Assert(constructor != null, nameof(constructor) + " != null");
		return (ICascadedConfigurationItemInternal)constructor.Invoke([name, path]);
	}
}
