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

// ReSharper disable ForCanBeConvertedToForeach

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// The base of a cascadable configuration hierarchy containing items with default values.<br/>
/// This configuration does not have any persistence strategy.
/// </summary>
[DebuggerDisplay("Default | Path: {" + nameof(Path) + "}")]
public class DefaultCascadedConfiguration : CascadedConfigurationBase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DefaultCascadedConfiguration"/> class (for root configurations).
	/// </summary>
	/// <param name="name">Name of the configuration.</param>
	public DefaultCascadedConfiguration(string name) :
		base(name, (ICascadedConfigurationPersistenceStrategy)null) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="DefaultCascadedConfiguration"/> class
	/// (for child configurations, i.e. configurations that have a parent configuration).
	/// </summary>
	/// <param name="name">Name of the configuration.</param>
	/// <param name="parent">Parent configuration.</param>
	protected DefaultCascadedConfiguration(string name, DefaultCascadedConfiguration parent) :
		base(name, parent) { }

	/// <summary>
	/// Gets child configurations of the configuration.
	/// </summary>
	public new IEnumerable<DefaultCascadedConfiguration> Children
	{
		get
		{
			lock (Sync)
			{
				IEnumerator<DefaultCascadedConfiguration> enumerator = new MonitorSynchronizedEnumerator<DefaultCascadedConfiguration>(
					mChildren.Cast<DefaultCascadedConfiguration>().GetEnumerator(),
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
	public new DefaultCascadedConfiguration Parent => (DefaultCascadedConfiguration)base.Parent;

	/// <summary>
	/// Gets the root configuration.
	/// </summary>
	public new DefaultCascadedConfiguration RootConfiguration => (DefaultCascadedConfiguration)base.RootConfiguration;

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
	public new DefaultCascadedConfiguration GetChildConfiguration(string path)
	{
		return (DefaultCascadedConfiguration)base.GetChildConfiguration(path);
	}

	/// <summary>
	/// Gets the child configuration at the specified location (optionally creates new configurations on the path).
	/// </summary>
	/// <param name="path">
	/// Relative path of the configuration to get/create.
	/// If a path segment contains path delimiters ('/'), escape these characters.
	/// Otherwise, the segment will be split up.
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
	/// <paramref name="path"/> contains a part that is not supported by the persistence strategy of an inheriting configuration.
	/// </exception>
	public DefaultCascadedConfiguration GetChildConfiguration(string path, bool create)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		lock (Sync)
		{
			return (DefaultCascadedConfiguration)GetChildConfigurationInternal(path, create);
		}
	}

	/// <summary>
	/// Adds a configuration item with the specified type and default value at the specified location.
	/// </summary>
	/// <typeparam name="T">Type of the value in the configuration item.</typeparam>
	/// <param name="path">
	/// Relative path of the configuration item to add. If a path segment contains path delimiters ('/'),
	/// escape them. Otherwise, the segment will be split up. The configuration helper function
	/// <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="defaultValue">The value of the item in the current configuration.</param>
	/// <returns>The added configuration item.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">The configuration already contains an item at the specified path.</exception>
	/// <exception cref="ConfigurationException">
	/// <paramref name="path"/> contains a part that is not supported by the persistence strategy of an inheriting configuration.<br/>
	/// -or-<br/>
	/// <typeparamref name="T"/> is not supported by the persistence strategy of an inheriting configuration.
	/// </exception>
	public CascadedConfigurationItem<T> AddItem<T>(string path, T defaultValue)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		lock (Sync)
		{
			// ensure that the persistence strategy of the current configuration and inheriting configurations can handle the type
			EnsureThatPersistenceStrategyCanHandleValueType(typeof(T)); // can throw ConfigurationException

			// split the item path into path segments (can throw ConfigurationException)
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(
				PersistenceStrategy,
				path,
				isItemPath: true,
				checkValidity: true);

			// ensure that the item does not exist, yet
			if (TryGetItemInternal(pathSegments, 0, out ICascadedConfigurationItem _))
			{
				string relativePath = string.Join("/", pathSegments);
				throw new ArgumentException(
					$"The configuration already contains an item at the specified path ({relativePath}).",
					nameof(path));
			}

			// add the configuration item in the current configuration (with value) and in all inheriting configurations (without value)
			return AddItemInternal(pathSegments, 0, defaultValue);
		}
	}

	/// <summary>
	/// Adds a configuration item with the specified type and default value at the specified location,
	/// if there is an inheriting configuration that provides a value for it (for rare edge cases).
	/// </summary>
	/// <typeparam name="T">Type of the value in the configuration item.</typeparam>
	/// <param name="path">
	/// Relative path of the configuration item to add. If a path segment contains path delimiters ('/'),
	/// escape them. Otherwise, the segment will be split up. The configuration helper function
	/// <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="defaultValue">The value of the item in the current configuration.</param>
	/// <returns>
	/// The added configuration item if there is an inheriting configuration providing a value for it;<br/>
	/// otherwise <c>null</c>.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">The configuration already contains an item at the specified path.</exception>
	/// <exception cref="ConfigurationException">
	/// <paramref name="path"/> contains a part that is not supported by the persistence strategy of an inheriting configuration.<br/>
	/// -or-<br/>
	/// <typeparamref name="T"/> is not supported by the persistence strategy of an inheriting configuration.
	/// </exception>
	public CascadedConfigurationItem<T> AddItemIfInheritingConfigurationContainsValue<T>(string path, T defaultValue)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		lock (Sync)
		{
			// ensure that the persistence strategy of the current configuration and inheriting configurations can handle the type
			EnsureThatPersistenceStrategyCanHandleValueType(typeof(T)); // can throw ConfigurationException

			// split the item path into path segments (can throw ConfigurationException)
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(
				PersistenceStrategy,
				path,
				isItemPath: true,
				checkValidity: true);

			// ensure that the item does not exist, yet
			if (TryGetItemInternal(pathSegments, 0, out ICascadedConfigurationItem _))
			{
				string relativePath = string.Join("/", pathSegments);
				throw new ArgumentException(
					$"The configuration already contains an item at the specified path ({relativePath}).",
					nameof(path));
			}

			// determine whether an inheriting configuration has a value for the item at the specified path
			bool valueExists = false;
			for (int i = 0; !valueExists && i < mInheritingConfigurations.Count; i++)
			{
				valueExists = mInheritingConfigurations[i]
					.ContainsValueAtPathInternal(
						pathSegments,
						startIndex: 0,
						considerInheritingConfigurations: true);
			}

			// abort if no inheriting configuration could provide a value for the item
			if (!valueExists)
				return null;

			// add the configuration item in the current configuration (with value) and in all inheriting configurations (without value)
			// (the inheriting configuration(s) that have a persistence strategy will fill their item with the appropriate value)
			return AddItemInternal(pathSegments, 0, defaultValue);
		}
	}

	private CascadedConfigurationItem<T> AddItemInternal<T>(string[] pathSegments, int startIndex, T defaultValue)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The configuration is expected to be locked.");
		Debug.Assert(pathSegments is { Length: > 0 });

		// create configurations down to the specified path, if necessary
		if (startIndex + 1 < pathSegments.Length)
		{
			// the path contains child configurations
			// => dive into the appropriate configuration
			var configuration = (DefaultCascadedConfiguration)GetChildConfigurationInternal(
				pathSegments,
				startIndex,
				count: pathSegments.Length - startIndex - 1,
				create: true);

			return configuration.AddItemInternal(
				pathSegments,
				startIndex: pathSegments.Length - 1,
				defaultValue);
		}

		Debug.Assert(startIndex == pathSegments.Length - 1);

		// add configuration item in the current configuration
		string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[^1]);
		var item = new CascadedConfigurationItem<T>(
			this,
			itemName,
			CascadedConfigurationPathHelper.CombinePath(Path, pathSegments[^1]),
			defaultValue);
		InsertItemHonoringOrderInternal(item);

		// add corresponding configuration items in inheriting configurations
		for (int i = 0; i < mInheritingConfigurations.Count; i++)
		{
			mInheritingConfigurations[i].AddItemInternal<T>(pathSegments, startIndex: pathSegments.Length - 1);
		}

		return item;
	}

	/// <summary>
	/// Adds a configuration item with the specified type and default value at the specified location.
	/// </summary>
	/// <param name="path">
	/// Relative path of the configuration item to add. If a path segment contains path delimiters ('/'),
	/// escape them. Otherwise, the segment will be split up. The configuration helper function
	/// <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="type">Type of the value in the configuration item.</param>
	/// <param name="defaultValue">The value of the item in the current configuration.</param>
	/// <returns>The added configuration item.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">The configuration already contains an item at the specified path.</exception>
	/// <exception cref="ConfigurationException">
	/// <paramref name="path"/> contains a part that is not supported by the persistence strategy of an inheriting configuration.<br/>
	/// -or-<br/>
	/// <paramref name="type"/> is not supported by the persistence strategy of an inheriting configuration.
	/// </exception>
	public ICascadedConfigurationItem AddItemDynamically(string path, Type type, object defaultValue)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));
		if (type == null) throw new ArgumentNullException(nameof(type));

		lock (Sync)
		{
			// ensure that the persistence strategy of the current configuration and inheriting configurations can handle the type
			EnsureThatPersistenceStrategyCanHandleValueType(type); // can throw ConfigurationException

			// split the item path into path segments (can throw ConfigurationException)
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(
				PersistenceStrategy,
				path,
				isItemPath: true,
				checkValidity: true);

			// ensure that the item does not exist, yet
			if (TryGetItemInternal(pathSegments, 0, out ICascadedConfigurationItem _))
			{
				string relativePath = string.Join("/", pathSegments);
				throw new ArgumentException(
					$"The configuration already contains an item at the specified path ({relativePath}).",
					nameof(path));
			}

			// add the configuration item in the current configuration (with value) and in all inheriting configurations (without value)
			return AddItemDynamicallyInternal(pathSegments, 0, type, defaultValue);
		}
	}

	/// <summary>
	/// Adds a configuration item with the specified type and default value at the specified location,
	/// if there is an inheriting configuration that provides a value for it (for rare edge cases).
	/// </summary>
	/// <param name="path">
	/// Relative path of the configuration item to add. If a path segment contains path delimiters ('/'),
	/// escape them. Otherwise, the segment will be split up. The configuration helper function
	/// <see cref="CascadedConfigurationPathHelper.EscapeName(string)"/> might come in handy for this.
	/// </param>
	/// <param name="type">Type of the value in the configuration item.</param>
	/// <param name="defaultValue">The value of the item in the current configuration.</param>
	/// <returns>
	/// The added configuration item if there is an inheriting configuration providing a value for it;<br/>
	/// otherwise <c>null</c>.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">The configuration already contains an item at the specified path.</exception>
	/// <exception cref="ConfigurationException">
	/// <paramref name="path"/> contains a part that is not supported by the persistence strategy of an inheriting configuration.<br/>
	/// -or-<br/>
	/// <paramref name="type"/> is not supported by the persistence strategy of an inheriting configuration.
	/// </exception>
	public ICascadedConfigurationItem AddItemDynamicallyIfInheritingConfigurationContainsValue(string path, Type type, object defaultValue)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));
		if (type == null) throw new ArgumentNullException(nameof(type));

		lock (Sync)
		{
			// ensure that the persistence strategy of the current configuration and inheriting configurations can handle the type
			EnsureThatPersistenceStrategyCanHandleValueType(type); // can throw ConfigurationException

			// split the item path into path segments (can throw ConfigurationException)
			string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(
				PersistenceStrategy,
				path,
				isItemPath: true,
				checkValidity: true);

			// ensure that the item does not exist, yet
			if (TryGetItemInternal(pathSegments, 0, out ICascadedConfigurationItem _))
			{
				string relativePath = string.Join("/", pathSegments);
				throw new ArgumentException(
					$"The configuration already contains an item at the specified path ({relativePath}).",
					nameof(path));
			}

			// determine whether an inheriting configuration has a value for the item at the specified path
			bool valueExists = false;
			for (int i = 0; !valueExists && i < mInheritingConfigurations.Count; i++)
			{
				valueExists = mInheritingConfigurations[i]
					.ContainsValueAtPathInternal(
						pathSegments,
						startIndex: 0,
						considerInheritingConfigurations: true);
			}

			// abort if no inheriting configuration could provide a value for the item
			if (!valueExists)
				return null;

			// add the configuration item in the current configuration (with value) and in all inheriting configurations (without value)
			// (the inheriting configuration(s) that have a persistence strategy will fill their item with the appropriate value)
			return AddItemDynamicallyInternal(pathSegments, 0, type, defaultValue);
		}
	}

	private ICascadedConfigurationItem AddItemDynamicallyInternal(
		string[] pathSegments,
		int      startIndex,
		Type     type,
		object   defaultValue)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The configuration is expected to be locked.");
		Debug.Assert(pathSegments is { Length: > 0 });

		// create configurations down to the specified path, if necessary
		if (startIndex + 1 < pathSegments.Length)
		{
			// the path contains child configurations
			// => dive into the appropriate configuration
			var configuration = (DefaultCascadedConfiguration)GetChildConfigurationInternal(
				pathSegments,
				startIndex,
				count: pathSegments.Length - startIndex - 1,
				create: true);

			return configuration.AddItemDynamicallyInternal(pathSegments, pathSegments.Length - 1, type, defaultValue);
		}

		Debug.Assert(startIndex == pathSegments.Length - 1);

		// add configuration item in the current configuration
		string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[^1]);
		ICascadedConfigurationItem item = CreateItem(
			itemName,
			CascadedConfigurationPathHelper.CombinePath(Path, pathSegments[^1]),
			type,
			defaultValue);
		InsertItemHonoringOrderInternal(item);

		// add corresponding configuration items in inheriting configurations
		for (int i = 0; i < mInheritingConfigurations.Count; i++)
		{
			mInheritingConfigurations[i].AddItemInternal(pathSegments, pathSegments.Length - 1, type);
		}

		return item;
	}

	/// <inheritdoc/>
	protected override CascadedConfigurationBase AddChildConfiguration(string name)
	{
		Debug.Assert(Monitor.IsEntered(Sync), "The configuration is expected to be locked.");
		Debug.Assert(name != null);
		return new DefaultCascadedConfiguration(name, this); // links itself to the current configuration
	}
}
