///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using GriffinPlus.Lib.Threading;

// ReSharper disable InvertIf

// ReSharper disable InconsistentNaming
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// A cascadable configuration allowing to build hierarchical configurations with multiple levels of inheritance and
/// different kinds of persistence.
/// </summary>
/// <remarks>
/// This class provides everything that is needed to build up a configuration system with multiple levels of inheritance
/// by stacking configurations. The base configuration represented by the <see cref="DefaultCascadedConfiguration"/>
/// class forms a base layer with default values for configuration items. You can stack inherited configurations -
/// represented by the <see cref="CascadedConfiguration"/> class - on top of the base configuration to create a configuration
/// with a defaults and multiple configuration layers overriding the defaults with more specific values. These layers
/// allow to load and save settings using a specific persistence strategy. Configurations deriving from the base
/// configuration may hide default settings by overwriting configuration items.  A query will always return the value
/// of the most specific configuration item that provides a value.
/// </remarks>
[DebuggerDisplay("Inherited | Path: {" + nameof(Path) + "}")]
public class CascadedConfiguration : CascadedConfigurationBase
{
	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="CascadedConfiguration"/> class
	/// (for root configurations that inherit from another configuration).
	/// </summary>
	/// <param name="configurationToInheritFrom">Configuration to inherit from.</param>
	/// <param name="persistence">
	/// A persistence strategy that is responsible for persisting configuration items;<br/>
	/// <see langword="null"/> if persistence is not needed.
	/// </param>
	protected internal CascadedConfiguration(CascadedConfigurationBase configurationToInheritFrom, ICascadedConfigurationPersistenceStrategy persistence) :
		base(configurationToInheritFrom, persistence) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="CascadedConfiguration"/> class
	/// (for root configurations that do not inherit from another configuration).
	/// </summary>
	/// <param name="name">Name of the configuration.</param>
	/// <param name="persistence">
	/// A persistence strategy that is responsible for persisting configuration items;<br/>
	/// <see langword="null"/> if persistence is not needed.
	/// </param>
	protected CascadedConfiguration(string name, ICascadedConfigurationPersistenceStrategy persistence) :
		base(name, persistence) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="CascadedConfiguration"/> class
	/// (for child configurations, i.e. configurations that have a parent configuration).<br/>
	/// It does not matter whether the configuration is the default configuration or whether it inherits from another configuration.
	/// </summary>
	/// <param name="name">Name of the configuration.</param>
	/// <param name="parent">Parent configuration.</param>
	protected internal CascadedConfiguration(string name, CascadedConfigurationBase parent) :
		base(name, parent) { }

	#endregion

	#region Properties

	/// <summary>
	/// Gets child configurations of the configuration.
	/// </summary>
	public new IEnumerable<CascadedConfiguration> Children
	{
		get
		{
			lock (Sync)
			{
				IEnumerator<CascadedConfiguration> enumerator = new MonitorSynchronizedEnumerator<CascadedConfiguration>(
					mChildren.Cast<CascadedConfiguration>().GetEnumerator(),
					Sync);
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
	/// Gets the parent of the configuration
	/// (<see langword="null"/> if the current configuration is a root configuration).
	/// </summary>
	public new CascadedConfiguration Parent => (CascadedConfiguration)base.Parent;

	/// <summary>
	/// Gets the root configuration.
	/// </summary>
	public new CascadedConfiguration RootConfiguration => (CascadedConfiguration)base.RootConfiguration;

	#endregion

	#region Getting a Child Configuration

	/// <summary>
	/// Gets the child configuration at the specified location.
	/// </summary>
	/// <param name="path">
	/// Relative path of the configuration to get.
	/// If a path segment contains path delimiters ('/'), escape these characters.
	/// Otherwise, the segment will be split up.
	/// The configuration helper function <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <returns>
	/// The requested child configuration;<br/>
	/// <see langword="null"/> if the child configuration at the specified path does not exist.
	/// </returns>
	/// <exception cref="ConfigurationException">
	/// <paramref name="path"/> contains a part that is not supported by the persistence strategy of the current or an inheriting configuration.
	/// </exception>
	public new CascadedConfiguration GetChildConfiguration(string path)
	{
		return (CascadedConfiguration)base.GetChildConfiguration(path);
	}

	/// <inheritdoc/>
	protected override CascadedConfigurationBase AddChildConfiguration(string name)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The configuration is expected to be locked.");
		Debug.Assert(name != null);
		return new CascadedConfiguration(name, this); // links itself to the current configuration
	}

	#endregion

	#region Setting the Value of a Configuration Item (Generic)

	/// <summary>
	/// Sets the value of a configuration item at the specified location in the configuration.
	/// </summary>
	/// <typeparam name="T">Type of the value in the configuration item.</typeparam>
	/// <param name="path">
	/// Relative path of the configuration item to set.
	/// If a path segment contains path delimiters ('/'), escape these characters.
	/// Otherwise, the segment will be split up.
	/// The configuration helper function <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="value">Value to set.</param>
	/// <returns>The item at the specified path.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	/// <exception cref="ConfigurationException">
	/// The configuration does not contain an item at the specified path.<br/>
	/// -or-<br/>
	/// The configuration contains an item at the specified path, but with a different type.<br/>
	/// -or-<br/>
	/// The specified item type not supported by the persistence strategy or <paramref name="value"/> is not assignable to it.
	/// </exception>
	public CascadedConfigurationItem<T> SetValue<T>(string path, T value)
	{
		if (!TryGetItem(path, out ICascadedConfigurationItem item))
			throw new ConfigurationException($"The configuration does not contain an item at the specified path ({path}).");

		if (typeof(T) != item.Type)
		{
			throw new ConfigurationException(
				"The configuration contains an item at the specified path ({0}), but with a different type (configuration item: {1}, specified: {2}).",
				item.Path,
				item.Type.FullName,
				typeof(T).FullName);
		}

		var item2 = (CascadedConfigurationItem<T>)item;
		item2.Value = value;
		return item2;
	}

	#endregion

	#region Setting the Value of a Configuration Item (Dynamic)

	/// <summary>
	/// Sets the value of a configuration item at the specified location in the configuration.
	/// </summary>
	/// <param name="path">
	/// Relative path of the configuration item to set.
	/// If a path segment contains path delimiters ('/'), escape these characters.
	/// Otherwise, the segment will be split up.
	/// The configuration helper function <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="value">Value to set.</param>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	/// <exception cref="ConfigurationException">
	/// The configuration does not contain an item at the specified path.<br/>
	/// -or-<br/>
	/// The configuration contains an item at the specified path, but <paramref name="value"/> is not assignable to it.
	/// </exception>
	public ICascadedConfigurationItem SetValue(string path, object value)
	{
		if (!TryGetItem(path, out ICascadedConfigurationItem item))
			throw new ConfigurationException($"The configuration does not contain an item at the specified path ({path}).");

		if (value != null)
		{
			Type type = value.GetType();
			if (!item.Type.IsAssignableFrom(type))
			{
				throw new ConfigurationException(
					"The configuration contains an item at the specified path ({0}), but the specified value is not assignable to it (configuration item: {1}, specified: {2}).",
					item.Path,
					item.Type.FullName,
					type.FullName);
			}
		}
		else
		{
			if (item.Type.IsValueType)
			{
				throw new ConfigurationException(
					"The configuration contains an item at the specified path ({0}), but the specified value is not assignable to it (configuration item: {1}, specified: null).",
					item.Path,
					item.Type.FullName);
			}
		}

		item.Value = value;
		return item;
	}

	#endregion

	#region Resetting Item Values

	/// <summary>
	/// Resets all items of the current configuration (and optionally all items of child configurations as well),
	/// so inherited item values become visible.
	/// </summary>
	/// <param name="recursively">
	/// <see langword="true"/> to reset items of child configurations as well;<br/>
	/// <see langword="false"/> to reset items of the current configuration.
	/// </param>
	public void ResetItems(bool recursively = false)
	{
		lock (Sync)
		{
			if (recursively)
			{
				for (int i = 0; i < mChildren.Count; i++)
				{
					var child = (CascadedConfiguration)mChildren[i];
					child.ResetItems(true);
				}
			}

			for (int i = 0; i < mItems.Count; i++)
			{
				mItems[i].ResetValue();
			}
		}
	}

	#endregion

	#region Loading and Saving

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

	#endregion
}
