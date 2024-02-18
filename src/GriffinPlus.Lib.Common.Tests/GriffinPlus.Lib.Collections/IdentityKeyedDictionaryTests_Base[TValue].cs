///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace GriffinPlus.Lib.Collections
{

	/// <summary>
	/// Unit tests targeting the <see cref="IdentityKeyedDictionary{TKey,TValue}"/> class.
	/// </summary>
	public abstract partial class IdentityKeyedDictionaryTests_Base<TValue> : GenericDictionaryTests_Base<string, TValue>
	{
		/// <summary>
		/// Gets a comparer for comparing keys.
		/// </summary>
		protected override IComparer<string> KeyComparer => StringComparer.Ordinal;

		/// <summary>
		/// Gets an equality comparer for comparing keys.
		/// </summary>
		protected override IEqualityComparer<string> KeyEqualityComparer => EqualityComparer<string>.Default;

		/// <summary>
		/// Gets a key that is guaranteed to be not in the generated test data set.
		/// </summary>
		protected override string KeyNotInTestData => "XXX"; // not a valid hex string

		#region Construction

		/// <summary>
		/// Tests the <see cref="IdentityKeyedDictionary{TKey,TValue}()"/> constructor succeeds.
		/// </summary>
		[Fact]
		public void Create_Default()
		{
			var dict = new IdentityKeyedDictionary<string, TValue>();
			Create_CheckEmptyDictionary(dict);
		}

		/// <summary>
		/// Tests the <see cref="IdentityKeyedDictionary{TKey,TValue}(int)"/> constructor succeeds with a positive capacity.
		/// </summary>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Create_WithCapacity(int capacity)
		{
			var dict = new IdentityKeyedDictionary<string, TValue>(capacity);
			Create_CheckEmptyDictionary(dict, capacity);
		}

		/// <summary>
		/// Tests whether the <see cref="IdentityKeyedDictionary{TKey,TValue}()"/> constructor throws a <see cref="ArgumentOutOfRangeException"/>
		/// if a negative capacity is passed.
		/// </summary>
		[Fact]
		public void Create_WithCapacity_NegativeCapacity()
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new IdentityKeyedDictionary<string, TValue>(-1));
			Assert.Equal("capacity", exception.ParamName);
		}

		/// <summary>
		/// Tests whether the <see cref="IdentityKeyedDictionary{TKey,TValue}(IDictionary{TKey, TValue})"/> constructor succeeds.
		/// </summary>
		/// <param name="count">Number of elements the data set to pass to the constructor should contain.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Create_WithDictionary(int count)
		{
			// get test data and create a new dictionary with it
			IDictionary<string, TValue> data = GetTestData(count);
			var dict = new IdentityKeyedDictionary<string, TValue>(data);

			// check the dictionary itself
			// --------------------------------------------------------------------------------------------------------

			// the Count property should return the number of elements
			Assert.Equal(data.Count, dict.Count);

			// the number of elements should always be lower than the capacity
			Assert.True(dict.Count <= dict.Capacity);

			if (count > 0)
			{
				// dictionary should contain elements
				Assert.NotEmpty(dict);                           // using the enumerator
				Assert.True(HashHelpers.IsPrime(dict.Capacity)); // the capacity of a hash table should always be a prime
			}
			else
			{
				// dictionary should not contain any elements
				Assert.Empty(dict);             // using the enumerator
				Assert.Equal(0, dict.Capacity); // the capacity should also be 0 (no internal data buffer)
			}

			// check collection of keys in the dictionary
			// --------------------------------------------------------------------------------------------------------

			// the Count property should return the number of elements
			Assert.Equal(data.Count, dict.Keys.Count);

			if (count > 0)
			{
				// dictionary should contain elements
				Assert.NotEmpty(dict.Keys); // using the enumerator
			}
			else
			{
				// dictionary should not contain any elements
				Assert.Empty(dict.Keys); // using the enumerator
			}

			// compare collection elements with the expected values
			// (use array comparer to force the keys into the same order and check for equality)
			Assert.Equal(
				data.Keys
					.OrderBy(x => x, KeyComparer),
				dict.Keys
					.OrderBy(x => x, KeyComparer));

			// check collection of values in the dictionary
			// --------------------------------------------------------------------------------------------------------

			// the Count property should return the number of elements
			Assert.Equal(data.Count, dict.Values.Count);

			if (count > 0)
			{
				// dictionary should contain elements
				Assert.NotEmpty(dict.Values); // using the enumerator
			}
			else
			{
				// dictionary should not contain any elements
				Assert.Empty(dict.Values); // using the enumerator
			}

			// compare collection elements with the expected values
			Assert.Equal(data.Values, dict.Values);
		}

		/// <summary>
		/// Tests whether the <see cref="IdentityKeyedDictionary{TKey,TValue}(IDictionary{TKey, TValue})"/> constructor
		/// throws a <see cref="ArgumentNullException"/> if the source dictionary is <c>null</c>.
		/// </summary>
		[Fact]
		public void Create_WithDictionary_DictionaryNull()
		{
			var exception = Assert.Throws<ArgumentNullException>(() => new IdentityKeyedDictionary<string, TValue>(null));
			Assert.Equal("dictionary", exception.ParamName);
		}

		/// <summary>
		/// Checks whether the dictionary has the expected state after construction.
		/// </summary>
		/// <param name="dict">Dictionary to check.</param>
		/// <param name="capacity">Initial capacity of the dictionary (as specified at construction time).</param>
		private static void Create_CheckEmptyDictionary(IdentityKeyedDictionary<string, TValue> dict, int capacity = 0)
		{
			// calculate the actual capacity of the dictionary
			// (empty dictionary: 0, non-empty dictionary: always the next prime greater than the specified capacity)
			int expectedCapacity = capacity > 0 ? HashHelpers.GetPrime(capacity) : 0;

			// check the dictionary itself
			Assert.Equal(expectedCapacity, dict.Capacity);
			Assert.Equal(0, dict.Count);
			Assert.Empty(dict);

			// check collection of keys in the dictionary
			Assert.Equal(0, dict.Keys.Count);
			Assert.Empty(dict.Keys);

			// check collection of values in the dictionary
			Assert.Equal(0, dict.Values.Count);
			Assert.Empty(dict.Values);
		}

		#endregion

		#region IdentityKeyedDictionary<TKey,TValue>.Capacity

		/// <summary>
		/// Tests getting the <see cref="IdentityKeyedDictionary{TKey,TValue}.Capacity"/> property.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Capacity_Get(int count)
		{
			IDictionary<string, TValue> data = GetTestData(count);
			var dict = new IdentityKeyedDictionary<string, TValue>(data);
			int expectedCapacity = count > 0 ? HashHelpers.GetPrime(count) : 0;
			Assert.Equal(expectedCapacity, dict.Capacity); // the capacity should always be prime
		}

		#endregion

		#region IdentityKeyedDictionary<TKey,TValue>.GetEnumerator() - incl. all enumerator functionality

		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void GetEnumerator(int count)
		{
			// get test data and create a new dictionary with it
			IDictionary<string, TValue> data = GetTestData(count);
			var dict = new IdentityKeyedDictionary<string, TValue>(data);

			// get an enumerator
			IdentityKeyedDictionary<string, TValue>.Enumerator enumerator = dict.GetEnumerator();

			// the enumerator should point to the position before the first valid element,
			// but the 'Current' property should not throw an exception
			KeyValuePair<string, TValue> _ = enumerator.Current;

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<string, TValue>>();
			while (enumerator.MoveNext())
			{
				Assert.IsType<KeyValuePair<string, TValue>>(enumerator.Current);
				KeyValuePair<string, TValue> current = enumerator.Current;
				enumerated.Add(current);
			}

			// compare collection elements with the expected values
			Assert.Equal(
				data.OrderBy(x => x.Key, KeyComparer),
				enumerated.OrderBy(x => x.Key, KeyComparer),
				KeyValuePairEqualityComparer);

			// the enumerator should point to the position after the last valid element now,
			// but the 'Current' property should not throw an exception
			// ReSharper disable once AssignmentInsteadOfDiscard
			// ReSharper disable once RedundantAssignment
			_ = enumerator.Current;

			// modify the collection, the enumerator should recognize this
			dict[KeyNotInTestData] = ValueNotInTestData;
			Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());

			// dispose the enumerator
			enumerator.Dispose();
		}

		#endregion
	}

}
