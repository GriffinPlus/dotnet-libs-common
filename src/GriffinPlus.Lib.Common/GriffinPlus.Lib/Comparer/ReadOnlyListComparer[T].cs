///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace GriffinPlus.Lib
{

	/// <summary>
	/// An generic comparer for <see cref="IReadOnlyList{T}"/>.
	/// The content of the lists are compared first, followed by the length of the arrays.
	/// </summary>
	/// <typeparam name="T">Type of a list element.</typeparam>
	public class ReadOnlyListComparer<T> : IComparer<IReadOnlyList<T>>
		where T : IComparable<T>
	{
		/// <summary>
		/// The singleton instance of the comparer.
		/// </summary>
		public static readonly ReadOnlyListComparer<T> Instance = new ReadOnlyListComparer<T>();

		/// <summary>
		/// Compares two lists and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="x">The first array to compare.</param>
		/// <param name="y">The second array to compare.</param>
		/// <returns>
		/// A signed integer that indicates the relative values of x and y:
		/// Less than zero: <paramref name="x"/> is less than <paramref name="x"/>.
		/// Zero : <paramref name="x"/> equals <paramref name="y"/>.
		/// Greater than zero: <paramref name="x"/> is greater than <paramref name="y"/>.
		/// </returns>
		public int Compare(IReadOnlyList<T> x, IReadOnlyList<T> y)
		{
			if (x == null && y == null) return 0;
			if (x != null && y == null) return -1;
			if (x == null) return 1;

			int len = Math.Min(x.Count, y.Count);
			for (int i = 0; i < len; i++)
			{
				int c = x[i].CompareTo(y[i]);
				if (c != 0)
				{
					return c;
				}
			}

			return x.Count.CompareTo(y.Count);
		}
	}

}
