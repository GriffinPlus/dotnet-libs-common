///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace GriffinPlus.Lib
{
	/// <summary>
	/// An equality comparer that checks whether the identity of two objects is the same.
	/// </summary>
	/// <typeparam name="T">Type to compare.</typeparam>
	public class IdentityComparer<T> : IEqualityComparer<T> where T: class
	{
		/// <summary>
		/// Gets the default instance of the comparer.
		/// </summary>
		public static IdentityComparer<T> Default { get; } = new IdentityComparer<T>();

		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">The first object of type <c>T</c> to compare.</param>
		/// <param name="y">The second object of type <c>T</c> to compare.</param>
		/// <returns>true if the specified objects are the same; otherwise, false.</returns>
		public bool Equals(T x, T y)
		{
			return object.ReferenceEquals(x,y);
		}

		/// <summary>
		/// Gets a hash code for the specified object.
		/// </summary>
		/// <param name="obj">The object for which a hash code is to be returned.</param>
		/// <returns>A hash code for the specified object.</returns>
		public int GetHashCode(T obj)
		{
			return obj.GetHashCode();
		}
	}
}
