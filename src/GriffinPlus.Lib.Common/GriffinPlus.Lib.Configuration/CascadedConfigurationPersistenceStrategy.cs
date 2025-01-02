///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using GriffinPlus.Lib.Conversion;

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// Base class for persistence strategies enabling the <see cref="CascadedConfiguration"/> to make its data persistent.
/// </summary>
public abstract class CascadedConfigurationPersistenceStrategy : ICascadedConfigurationPersistenceStrategy
{
	private readonly Dictionary<Type, IConverter> mValueConverters = new();

	/// <summary>
	/// Synchronization object used for synchronizing access to the persistence strategy.
	/// </summary>
	protected readonly object Sync = new();

	/// <inheritdoc/>
	public void RegisterValueConverter(IConverter converter)
	{
		lock (Sync)
		{
			mValueConverters.Add(converter.Type, converter);
		}
	}

	/// <inheritdoc/>
	public IConverter GetValueConverter(Type type)
	{
		lock (Sync)
		{
			mValueConverters.TryGetValue(type, out IConverter converter);
			return converter;
		}
	}

	/// <inheritdoc/>
	public abstract bool IsValidConfigurationName(string name);

	/// <inheritdoc/>
	public abstract bool IsValidItemName(string name);

	/// <inheritdoc/>
	public abstract bool SupportsType(Type type);

	/// <inheritdoc/>
	public abstract bool SupportsComments { get; }

	/// <inheritdoc/>
	public virtual bool IsAssignable(Type type, object value)
	{
		if (value != null)
			return value.GetType() == type;

		return !type.IsValueType;
	}

	/// <inheritdoc/>
	public abstract void Load(CascadedConfiguration configuration);

	/// <inheritdoc/>
	public abstract void LoadItem(ICascadedConfigurationItem item);

	/// <inheritdoc/>
	public abstract void Save(CascadedConfiguration configuration, CascadedConfigurationSaveFlags flags);
}
