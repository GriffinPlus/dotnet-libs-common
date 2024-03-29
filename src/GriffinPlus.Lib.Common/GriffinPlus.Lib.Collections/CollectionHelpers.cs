﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
//
// This file incorporates work covered by the following copyright and permission notice:
//
//     MIT License
//
//     Copyright (c) 2016 Stephen Cleary
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in all
//     copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//     SOFTWARE.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GriffinPlus.Lib.Collections;

/// <summary>
/// Common helpers for custom collections.
/// </summary>
public static class CollectionHelpers
{
	/// <summary>
	/// Reifies the specified enumerable as a collection.
	/// </summary>
	/// <typeparam name="T">Item type.</typeparam>
	/// <param name="source">Enumerable to reify as a collection.</param>
	/// <returns>A read-only collection containing the same elements as the specified enumerable.</returns>
	public static IReadOnlyCollection<T> ReifyCollection<T>(IEnumerable<T> source)
	{
		return source switch
		{
			null                             => throw new ArgumentNullException(nameof(source)),
			IReadOnlyCollection<T> result    => result,
			ICollection<T> collection        => new CollectionWrapper<T>(collection),
			ICollection nonGenericCollection => new NonGenericCollectionWrapper<T>(nonGenericCollection),
			var _                            => new List<T>(source)
		};
	}

	private sealed class NonGenericCollectionWrapper<T>(ICollection collection) : IReadOnlyCollection<T>
	{
		private readonly ICollection mCollection = collection ?? throw new ArgumentNullException(nameof(collection));

		public int Count => mCollection.Count;

		public IEnumerator<T> GetEnumerator()
		{
			return mCollection.Cast<T>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return mCollection.GetEnumerator();
		}
	}

	private sealed class CollectionWrapper<T>(ICollection<T> collection) : IReadOnlyCollection<T>
	{
		private readonly ICollection<T> mCollection = collection ?? throw new ArgumentNullException(nameof(collection));

		public int Count => mCollection.Count;

		public IEnumerator<T> GetEnumerator()
		{
			return mCollection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return mCollection.GetEnumerator();
		}
	}
}
