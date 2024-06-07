///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// An item comparer that only considers the name of the item.
/// </summary>
public sealed class ItemByNameComparer : IComparer<ICascadedConfigurationItem>
{
	private readonly IComparer<string> mComparer;

	/// <summary>
	/// Creates a new instance of the <see cref="ItemByNameComparer"/> class.
	/// </summary>
	/// <param name="comparer">Comparer to use to compare item names.</param>
	public ItemByNameComparer(IComparer<string> comparer)
	{
		mComparer = comparer;
	}

	/// <summary>
	/// Gets a comparer using <see cref="StringComparer.InvariantCulture"/> to compare item names.
	/// </summary>
	public static ItemByNameComparer InvariantCultureComparer { get; } = new(StringComparer.InvariantCulture);

	/// <summary>
	/// Gets a comparer using <see cref="StringComparer.InvariantCultureIgnoreCase"/> to compare item names.
	/// </summary>
	public static ItemByNameComparer InvariantCultureIgnoreCaseComparer { get; } = new(StringComparer.InvariantCultureIgnoreCase);

	/// <inheritdoc/>
	public int Compare(ICascadedConfigurationItem x, ICascadedConfigurationItem y)
	{
		if (x == null && y == null) return 0;
		if (x == null) return -1;
		if (y == null) return 1;
		return mComparer.Compare(x.Name, y.Name);
	}
}
