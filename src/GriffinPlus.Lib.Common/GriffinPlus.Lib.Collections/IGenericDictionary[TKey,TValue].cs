///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// ReSharper disable PossibleInterfaceMemberAmbiguity

using System;
using System.Collections;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Collections
{

	/// <summary>
	/// A generic dictionary (serves as common interface for generic dictionary implementations).
	/// </summary>
	/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
	public interface IGenericDictionary<TKey, TValue> :
		IDictionary<TKey, TValue>,
		IDictionary,
		IReadOnlyDictionary<TKey, TValue>
	{
		/// <summary>
		/// Determines whether the dictionary contains the specified value.
		/// </summary>
		/// <param name="value">
		/// The value to locate in the dictionary.
		/// The value can be <c>null</c> for reference types.
		/// </param>
		/// <returns>
		/// <c>true</c> if the dictionary contains an element with the specified value;
		/// otherwise <c>false</c>.
		/// </returns>
		bool ContainsValue(TValue value);

		/// <summary>
		/// Tries to add the specified key and value to dictionary.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add. The value can be <c>null</c> for reference types.</param>
		/// <returns>
		/// <c>true</c> if the element was successfully added to the dictionary;
		/// <c>false</c> if the dictionary already contains an element with the specified key.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
		bool TryAdd(TKey key, TValue value);
	}

}
