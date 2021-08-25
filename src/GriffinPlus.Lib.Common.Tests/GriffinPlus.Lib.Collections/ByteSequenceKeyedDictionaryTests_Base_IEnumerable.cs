﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Xunit;

// ReSharper disable RedundantAssignment
// ReSharper disable AssignNullToNotNullAttribute

namespace GriffinPlus.Lib.Collections
{

	public abstract partial class ByteSequenceKeyedDictionaryTests_Base<TValue>
	{
		#region GetEnumerator() - incl. all enumerator functionality

		/// <summary>
		/// Tests enumerating key/value pairs using <see cref="IEnumerable.GetEnumerator"/>.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IEnumerable_GetEnumerator(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// get an enumerator
			var enumerator = ((IEnumerable)dict).GetEnumerator();

			// the enumerator should point to the position before the first valid element,
			// but the 'Current' property should not throw an exception
			object _ = enumerator.Current;

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			while (enumerator.MoveNext())
			{
				Assert.IsType<KeyValuePair<IReadOnlyList<byte>, TValue>>(enumerator.Current);
				var current = (KeyValuePair<IReadOnlyList<byte>, TValue>)enumerator.Current;
				Assert.IsAssignableFrom<IReadOnlyList<byte>>(current.Key);
				Assert.IsAssignableFrom<TValue>(current.Value);
				enumerated.Add(current);
			}

			// compare collection elements with the expected values
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);

			// the enumerator should point to the position after the last valid element now,
			// but the 'Current' property should not throw an exception
			_ = enumerator.Current;

			// reset the enumerator and try again
			enumerator.Reset();
			enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			while (enumerator.MoveNext())
			{
				Assert.IsType<KeyValuePair<IReadOnlyList<byte>, TValue>>(enumerator.Current);
				var current = (KeyValuePair<IReadOnlyList<byte>, TValue>)enumerator.Current;
				Assert.IsAssignableFrom<IReadOnlyList<byte>>(current.Key);
				Assert.IsAssignableFrom<TValue>(current.Value);
				enumerated.Add(current);
			}

			// compare collection elements with the expected values
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);

			// modify the collection, the enumerator should recognize this
			dict[KeyNotInTestData] = ValueNotInTestData;
			Assert.Throws<InvalidOperationException>(() => enumerator.Reset());
			Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
		}

		#endregion
	}

}
