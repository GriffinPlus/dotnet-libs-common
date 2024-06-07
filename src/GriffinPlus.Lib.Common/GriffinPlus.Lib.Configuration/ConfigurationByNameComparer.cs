///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// A configuration comparer that only considers the name of the configuration.
/// </summary>
public sealed class ConfigurationByNameComparer : IComparer<CascadedConfigurationBase>
{
	private readonly IComparer<string> mComparer;

	/// <summary>
	/// Creates a new instance of the <see cref="ConfigurationByNameComparer"/> class.
	/// </summary>
	/// <param name="comparer">Comparer to use to compare configuration names.</param>
	public ConfigurationByNameComparer(IComparer<string> comparer)
	{
		mComparer = comparer;
	}

	/// <summary>
	/// Gets a comparer using <see cref="StringComparer.InvariantCulture"/> to compare configuration names.
	/// </summary>
	public static ConfigurationByNameComparer InvariantCultureComparer { get; } = new(StringComparer.InvariantCulture);

	/// <summary>
	/// Gets a comparer using <see cref="StringComparer.InvariantCultureIgnoreCase"/> to compare configuration names.
	/// </summary>
	public static ConfigurationByNameComparer InvariantCultureIgnoreCaseComparer { get; } = new(StringComparer.InvariantCultureIgnoreCase);

	/// <inheritdoc/>
	public int Compare(CascadedConfigurationBase x, CascadedConfigurationBase y)
	{
		if (x == null && y == null) return 0;
		if (x == null) return -1;
		if (y == null) return 1;
		return mComparer.Compare(x.Name, y.Name);
	}
}
