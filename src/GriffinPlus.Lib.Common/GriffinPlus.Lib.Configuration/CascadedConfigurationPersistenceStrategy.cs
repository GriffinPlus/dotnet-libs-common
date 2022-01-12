///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using GriffinPlus.Lib.Conversion;

namespace GriffinPlus.Lib.Configuration
{

	/// <summary>
	/// Base class for persistence strategies enabling the <see cref="CascadedConfiguration"/> to make its data persistent.
	/// </summary>
	public abstract class CascadedConfigurationPersistenceStrategy : ICascadedConfigurationPersistenceStrategy
	{
		private readonly Dictionary<Type, IConverter> mValueConverters = new Dictionary<Type, IConverter>();

		/// <summary>
		/// Synchronization object used for synchronizing access to the persistence strategy.
		/// </summary>
		protected readonly object Sync = new object();

		/// <summary>
		/// Registers a converter that tells the configuration how to convert an object in the configuration to its
		/// string representation and vice versa.
		/// </summary>
		/// <param name="converter">Converter to register.</param>
		public void RegisterValueConverter(IConverter converter)
		{
			lock (Sync)
			{
				mValueConverters.Add(converter.Type, converter);
			}
		}

		/// <summary>
		/// Gets a converter for the specified type.
		/// </summary>
		/// <param name="type">Type to get a converter for.</param>
		/// <returns>
		/// The requested converter;
		/// <c>null</c> if there is no converter registered for the specified type.
		/// </returns>
		public IConverter GetValueConverter(Type type)
		{
			lock (Sync)
			{
				mValueConverters.TryGetValue(type, out var converter);
				return converter;
			}
		}

		/// <summary>
		/// Checks whether the specified name is a valid configuration name.
		/// </summary>
		/// <param name="name">Name to check.</param>
		/// <returns>
		/// <c>true</c> if the specified configuration name is valid for use with the strategy;
		/// otherwise <c>false</c>.
		/// </returns>
		public abstract bool IsValidConfigurationName(string name);

		/// <summary>
		/// Checks whether the specified name is a valid item name.
		/// </summary>
		/// <param name="name">Name to check.</param>
		/// <returns>
		/// <c>true</c> if the specified item name is valid for use with the strategy;
		/// otherwise <c>false</c>.
		/// </returns>
		public abstract bool IsValidItemName(string name);

		/// <summary>
		/// Checks whether the persistence strategy supports the specified type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <returns>
		/// <c>true</c> if the persistence strategy supports the specified type;
		/// otherwise <c>false</c>.
		/// </returns>
		public abstract bool SupportsType(Type type);

		/// <summary>
		/// Gets a value indicating whether the persistence strategy supports comments.
		/// </summary>
		public abstract bool SupportsComments { get; }

		/// <summary>
		/// Checks whether a configuration item of the specified type may be set to the specified value.
		/// </summary>
		/// <param name="type">Type of the value of the configuration item to check.</param>
		/// <param name="value">Value to check.</param>
		/// <returns>
		/// <c>true</c> if the specified value may be assigned to a configuration item of the specified type;
		/// otherwise <c>false</c>.
		/// </returns>
		/// <remarks>
		/// This method always returns <c>true</c>, if the type of the value matches the specified type. Otherwise
		/// <c>false</c> is returned. This is useful, if the persistence strategy does not save any type information,
		/// so the type of the configuration item is the only chance to determine the type to construct when loading
		/// a configuration item.
		/// </remarks>
		public virtual bool IsAssignable(Type type, object value)
		{
			if (value != null)
				return value.GetType() == type;

			return !type.IsValueType;
		}

		/// <summary>
		/// Loads configuration data from the backend storage into the specified configuration.
		/// </summary>
		/// <param name="configuration">Configuration to update.</param>
		public abstract void Load(CascadedConfiguration configuration);

		/// <summary>
		/// Loads the value of the specified configuration item from the persistent storage.
		/// </summary>
		/// <param name="item">Item to load.</param>
		public abstract void LoadItem(ICascadedConfigurationItem item);

		/// <summary>
		/// Saves configuration data from the specified configuration into the backend storage.
		/// </summary>
		/// <param name="configuration">Configuration to save.</param>
		/// <param name="flags">Flags controlling the saving behavior.</param>
		public abstract void Save(CascadedConfiguration configuration, CascadedConfigurationSaveFlags flags);
	}

}
