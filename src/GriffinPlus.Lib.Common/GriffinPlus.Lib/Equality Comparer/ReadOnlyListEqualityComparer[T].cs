///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace GriffinPlus.Lib;

/// <summary>
/// An equality comparer for comparing the contents of a <see cref="IReadOnlyList{T}"/> (thread-safe).
/// </summary>
public class ReadOnlyListEqualityComparer<T> : IEqualityComparer<IReadOnlyList<T>>
{
	private readonly IEqualityComparer<T> mElementComparer;

	/// <summary>
	/// Gets the <see cref="ReadOnlyListEqualityComparer{T}"/> instance.
	/// </summary>
	public static readonly ReadOnlyListEqualityComparer<T> Instance = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="ReadOnlyListEqualityComparer{T}"/> class using the default comparer of <typeparamref name="T"/>.
	/// </summary>
	public ReadOnlyListEqualityComparer() : this(null) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="ReadOnlyListEqualityComparer{T}"/> class using the specified element comparer.
	/// </summary>
	/// <param name="comparer">Comparer to use for comparing elements (may also be <c>null</c> to use the default comparer).</param>
	public ReadOnlyListEqualityComparer(IEqualityComparer<T> comparer)
	{
		mElementComparer = comparer ?? EqualityComparer<T>.Default;
	}

	/// <summary>
	/// Determines whether the specified lists are equal.
	/// </summary>
	/// <param name="x">The first list to compare.</param>
	/// <param name="y">The second list to compare.</param>
	/// <returns>
	/// <c>true</c>true if the specified lists are equal;
	/// otherwise <c>false</c>false.
	/// </returns>
	public bool Equals(IReadOnlyList<T> x, IReadOnlyList<T> y)
	{
		if (ReferenceEquals(x, y))
			return true;

		if (x == null || y == null || x.Count != y.Count)
			return false;

		int count = x.Count;
		for (int i = 0; i < count; i++)
		{
			if (!mElementComparer.Equals(x[i], y[i]))
				return false;
		}

		return true;
	}

	/// <summary>
	/// Returns a hash code for the specified list.
	/// </summary>
	/// <param name="obj">The list to calculate a hash code for.</param>
	/// <returns>The hash code for the specified list.</returns>
	/// <exception cref="System.ArgumentNullException"><paramref name="obj"/> is null.</exception>
	public int GetHashCode(IReadOnlyList<T> obj)
	{
		if (obj == null) throw new ArgumentNullException(nameof(obj));
		int hash = 560689 ^ obj.Count;
		int count = obj.Count;
		for (int i = 0; i < count; i++) hash ^= mElementComparer.GetHashCode(obj[i]);
		return hash;
	}
}
