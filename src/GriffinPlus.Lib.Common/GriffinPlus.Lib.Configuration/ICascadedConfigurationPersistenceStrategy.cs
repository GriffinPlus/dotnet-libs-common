﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

using GriffinPlus.Lib.Conversion;

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// Interface for classes implementing a persistence strategy to enable the <see cref="CascadedConfiguration"/> to load/save its data.
/// </summary>
public interface ICascadedConfigurationPersistenceStrategy
{
	/// <summary>
	/// Checks whether the specified name is a valid configuration name.
	/// </summary>
	/// <param name="name">Name to check.</param>
	/// <returns>
	/// <see langword="true"/> if the specified configuration name is valid for use with the strategy;<br/>
	/// otherwise <see langword="false"/>.
	/// </returns>
	bool IsValidConfigurationName(string name);

	/// <summary>
	/// Checks whether the specified name is a valid item name.
	/// </summary>
	/// <param name="name">Name to check.</param>
	/// <returns>
	/// <see langword="true"/> if the specified item name is valid for use with the strategy;<br/>
	/// otherwise <see langword="false"/>.
	/// </returns>
	bool IsValidItemName(string name);

	/// <summary>
	/// Registers a converter that tells the configuration how to convert an object in the configuration to its
	/// string representation and vice versa.
	/// </summary>
	/// <param name="converter">Converter to register.</param>
	void RegisterValueConverter(IConverter converter);

	/// <summary>
	/// Gets a converter for the specified type.
	/// </summary>
	/// <param name="type">Type to get a converter for.</param>
	/// <returns>
	/// The requested converter;<br/>
	/// <see langword="null"/> if there is no converter registered for the specified type.
	/// </returns>
	IConverter GetValueConverter(Type type);

	/// <summary>
	/// Checks whether the persistence strategy supports the specified type.
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <returns>
	/// <see langword="true"/> if the persistence strategy supports the specified type;<br/>
	/// otherwise <see langword="false"/>.
	/// </returns>
	bool SupportsType(Type type);

	/// <summary>
	/// Gets a value indicating whether the persistence strategy supports comments.
	/// </summary>
	bool SupportsComments { get; }

	/// <summary>
	/// Checks whether a configuration item of the specified type may be set to the specified value.
	/// </summary>
	/// <param name="type">Type of the value of the configuration item to check.</param>
	/// <param name="value">Value to check.</param>
	/// <returns>
	/// <see langword="true"/> if the specified value may be assigned to a configuration item of the specified type;<br/>
	/// otherwise <see langword="false"/>.
	/// </returns>
	/// <remarks>
	/// This method always returns <see langword="true"/> if the type of the value matches the specified type.
	/// Otherwise, <see langword="false"/> is returned.<br/>
	/// This is useful, if the persistence strategy does not save any type information, so the type of the configuration
	/// item is the only chance to determine the type to construct when loading a configuration item.
	/// </remarks>
	bool IsAssignable(Type type, object value);

	/// <summary>
	/// Loads configuration data from the backend storage into the specified configuration.
	/// </summary>
	/// <param name="configuration">Configuration to update.</param>
	/// <exception cref="ConfigurationException">Loading the configuration failed (reason depends on the persistence strategy).</exception>
	/// <exception cref="InvalidOperationException">
	/// The configuration is a child configuration (try to load the root configuration instead).
	/// </exception>
	void Load(CascadedConfiguration configuration);

	/// <summary>
	/// Loads the value of the specified configuration item from the persistent storage.
	/// </summary>
	/// <param name="item">Item to load.</param>
	/// <exception cref="ConfigurationException">Loading the configuration failed (reason depends on the persistence strategy).</exception>
	void LoadItem(ICascadedConfigurationItem item);

	/// <summary>
	/// Saves configuration data from the specified configuration into the backend storage.
	/// </summary>
	/// <param name="configuration">Configuration to save.</param>
	/// <param name="flags">Flags controlling the saving behavior.</param>
	/// <exception cref="ConfigurationException">Saving the configuration failed (reason depends on the persistence strategy).</exception>
	void Save(CascadedConfiguration configuration, CascadedConfigurationSaveFlags flags);
}
