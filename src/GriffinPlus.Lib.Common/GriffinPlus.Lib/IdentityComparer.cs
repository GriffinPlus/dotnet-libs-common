///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/GriffinPlus/dotnet-libs-common)
//
// Copyright 2018-2019 Sascha Falk <sascha@falk-online.eu>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for
// the specific language governing permissions and limitations under the License.
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
