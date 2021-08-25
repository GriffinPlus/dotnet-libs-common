///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

// ReSharper disable RedundantAssignment
// ReSharper disable AssignNullToNotNullAttribute

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace GriffinPlus.Lib.Collections
{

	/// <summary>
	/// Unit tests targeting the <see cref="ByteSequenceKeyedDictionary{TValue}"/> class.
	/// </summary>
	public abstract partial class ByteSequenceKeyedDictionaryTests_Base<TValue>
	{
		/// <summary>
		/// Gets a dictionary containing some test data.
		/// </summary>
		/// <param name="count">Number of entries in the dictionary.</param>
		/// <param name="minKeyLength">Minimum number of bytes forming the key.</param>
		/// <param name="maxKeyLength">Maximum number of bytes forming the key.</param>
		/// <returns>A test data dictionary.</returns>
		protected abstract IDictionary<IReadOnlyList<byte>, TValue> GetTestData(int count, int minKeyLength = 0, int maxKeyLength = 50);

		/// <summary>
		/// Gets a key that is guaranteed to be not in the generated test data set.
		/// </summary>
		protected abstract IReadOnlyList<byte> KeyNotInTestData { get; }

		/// <summary>
		/// Gets a value that is guaranteed to be not in the generated test data set.
		/// Must not be the default value of <see cref="TValue"/>.
		/// </summary>
		protected abstract TValue ValueNotInTestData { get; }

		/// <summary>
		/// An equality comparing for comparing key/value pairs returned by the dictionary
		/// </summary>
		private static readonly KeyValuePairEqualityComparer<IReadOnlyList<byte>, TValue> sKeyValuePairEqualityComparer =
			new KeyValuePairEqualityComparer<IReadOnlyList<byte>, TValue>(ReadOnlyListEqualityComparer<byte>.Instance, null);

		#region Test Data

		/// <summary>
		/// Test data for tests expecting the size of the test data set only.
		/// Contains: 0, 1, 10, 100, 1000, 10000.
		/// </summary>
		public static IEnumerable<object[]> TestDataSetSizes
		{
			get
			{
				yield return new object[] { 0 };
				yield return new object[] { 1 };
				yield return new object[] { 10 };
				yield return new object[] { 100 };
			}
		}

		/// <summary>
		/// Test data for tests expecting the size of the test data set only.
		/// Contains: 1, 10, 100, 1000, 10000.
		/// </summary>
		public static IEnumerable<object[]> TestDataSetSizes_WithoutZero
		{
			get
			{
				yield return new object[] { 1 };
				yield return new object[] { 10 };
				yield return new object[] { 100 };
			}
		}

		/// <summary>
		/// Test data for CopyTo() tests expecting the size of the test data set and an index in the destination array to start copying to.
		/// For tests that should succeed.
		/// </summary>
		public static IEnumerable<object[]> CopyTo_TestData
		{
			get
			{
				foreach (object[] data in TestDataSetSizes)
				{
					int count = (int)data[0];
					yield return new object[] { count, 0 };
					yield return new object[] { count, 1 };
					yield return new object[] { count, 5 };
				}
			}
		}

		/// <summary>
		/// Test data for CopyTo() tests expecting the size of the test data set and an index in the destination array to start copying to.
		/// For tests that check whether CopyTo() fails, if the index is out of bounds.
		/// </summary>
		public static IEnumerable<object[]> CopyTo_TestData_IndexOutOfBounds
		{
			get
			{
				foreach (object[] data in TestDataSetSizes)
				{
					int count = (int)data[0];
					yield return new object[] { count, -1 };        // before start of array
					yield return new object[] { count, count + 1 }; // after end of array (count is ok, if there are no elements to copy)
				}
			}
		}

		/// <summary>
		/// Test data for CopyTo() tests expecting the size of the test data set, the size of the destination array and an
		/// index in the destination array to start copying to.
		/// For tests that check whether CopyTo() fails, if the array is too small.
		/// </summary>
		public static IEnumerable<object[]> CopyTo_TestData_ArrayTooSmall
		{
			get
			{
				foreach (object[] data in TestDataSetSizes)
				{
					int count = (int)data[0];

					if (count > 0)
					{
						// destination array is way too small to store any elements
						yield return new object[] { count, 0, 0 };

						// destination array itself is large enough, but start index shifts the destination out
						// (the last element does not fit into the array)
						yield return new object[] { count, count, 1 };

						// destination array itself is large enough, but start index shifts the destination out
						// (no space left for any elements)
						yield return new object[] { count, count, count };
					}
				}
			}
		}

		#endregion

		#region Construction

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}()"/> constructor succeeds.
		/// </summary>
		[Fact]
		public void Create_Default()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			Create_CheckEmptyDictionary(dict);
		}

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}(int)"/> constructor succeeds with a positive capacity.
		/// </summary>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Create_WithCapacity(int capacity)
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>(capacity);
			Create_CheckEmptyDictionary(dict, capacity);
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}()"/> constructor throws a <see cref="ArgumentOutOfRangeException"/>
		/// if a negative capacity is passed.
		/// </summary>
		[Fact]
		public void Create_WithCapacity_NegativeCapacity()
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new ByteSequenceKeyedDictionary<TValue>(-1));
			Assert.Equal("capacity", exception.ParamName);
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}(IDictionary{IReadOnlyList{byte}, TValue})"/> constructor succeeds.
		/// </summary>
		/// <param name="count">Number of elements the data set to pass to the constructor should contain.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Create_WithDictionary(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

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
					.OrderBy(x => x, ReadOnlyListComparer<byte>.Instance),
				dict.Keys
					.OrderBy(x => x, ReadOnlyListComparer<byte>.Instance));

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
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}(IDictionary{IReadOnlyList{byte}, TValue})"/> constructor
		/// throws a <see cref="ArgumentNullException"/> if the source dictionary is <c>null</c>.
		/// </summary>
		[Fact]
		public void Create_WithDictionary_DictionaryNull()
		{
			var exception = Assert.Throws<ArgumentNullException>(() => new ByteSequenceKeyedDictionary<TValue>(null));
			Assert.Equal("dictionary", exception.ParamName);
		}

		/// <summary>
		/// Checks whether the dictionary has the expected state after construction.
		/// </summary>
		/// <param name="dict">Dictionary to check.</param>
		/// <param name="capacity">Initial capacity of the dictionary (as specified at construction time).</param>
		private static void Create_CheckEmptyDictionary(ByteSequenceKeyedDictionary<TValue> dict, int capacity = 0)
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

		#region ByteSequenceKeyedDictionary<TValue>.Capacity

		/// <summary>
		/// Tests getting the <see cref="ByteSequenceKeyedDictionary{TValue}.Capacity"/> property.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Capacity_Get(int count)
		{
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			int expectedCapacity = count > 0 ? HashHelpers.GetPrime(count) : 0;
			Assert.Equal(expectedCapacity, dict.Capacity); // the capacity should always be prime
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.Count

		/// <summary>
		/// Tests getting the <see cref="ByteSequenceKeyedDictionary{TValue}.Count"/> property.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Count_Get(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			Assert.Equal(data.Count, dict.Count);
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.Keys

		/// <summary>
		/// Tests accessing the key collection via <see cref="ByteSequenceKeyedDictionary{TValue}.Keys"/>.
		/// The key collection should present all keys in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Keys(int count)
		{
			// get test data and create a new dictionary with it
			var expected = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(expected);

			// enumerate the keys in the dictionary
			var enumerated = dict.Keys.ToList();

			// compare collection elements with the expected values
			Assert.Equal(
				expected.Select(x => x.Key).OrderBy(x => x, ReadOnlyListComparer<byte>.Instance).ToArray(),
				enumerated.OrderBy(x => x, ReadOnlyListComparer<byte>.Instance).ToArray(),
				ReadOnlyListEqualityComparer<byte>.Instance);
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.Values

		/// <summary>
		/// Tests accessing the value collection via <see cref="ByteSequenceKeyedDictionary{TValue}.Values"/>.
		/// The value collection should present all values in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Values(int count)
		{
			// get test data and create a new dictionary with it
			var expected = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(expected);

			// enumerate the values in the dictionary
			var enumerated = dict.Values.ToList();

			// compare collection elements with the expected values
			Assert.Equal(
				expected.Select(x => x.Value).OrderBy(x => x, Comparer<TValue>.Default).ToArray(),
				enumerated.OrderBy(x => x, Comparer<TValue>.Default).ToArray(),
				EqualityComparer<TValue>.Default);
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.this[IReadOnlyList<byte>]

		/// <summary>
		/// Tests accessing the key collection via <see cref="ByteSequenceKeyedDictionary{TValue}.this[IReadOnlyList{byte}]"/>.
		/// The key of the element is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Indexer_Get_List(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// test whether keys of test data are reported to be in the dictionary
			foreach (var kvp in data)
			{
				Assert.Equal(kvp.Value, dict[kvp.Key]);
			}
		}

		/// <summary>
		/// Tests accessing the key collection via <see cref="ByteSequenceKeyedDictionary{TValue}.this[IReadOnlyList{byte}]"/>.
		/// The key of the element is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Indexer_Get_List_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// test whether some other key is reported to be not in the dictionary
			Assert.Throws<KeyNotFoundException>(() => dict[KeyNotInTestData]);
		}

		/// <summary>
		/// Tests whether <see cref="ByteSequenceKeyedDictionary{TValue}.this[IReadOnlyList{byte}]"/> fails, if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void Indexer_Get_List_KeyNull()
		{
			// ReSharper disable once CollectionNeverUpdated.Local
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var exception = Assert.Throws<ArgumentNullException>(() => dict[(IReadOnlyList<byte>)null]);
			Assert.Equal("key", exception.ParamName);
		}

		/// <summary>
		/// Tests accessing the key collection via <see cref="ByteSequenceKeyedDictionary{TValue}.this[IReadOnlyList{byte}]"/>.
		/// The item is added to the dictionary, because there is no item with the specified key, yet.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Indexer_Set_List_NewItem(int count)
		{
			// get test data and create an empty dictionary
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>();

			// add data to the dictionary
			foreach (var kvp in data)
			{
				dict[kvp.Key] = kvp.Value;
			}

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests accessing the key collection via <see cref="ByteSequenceKeyedDictionary{TValue}.this[IReadOnlyList{byte}]"/>.
		/// The item is overwritten, because there is already an item with the specified key in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes_WithoutZero))]
		public void Indexer_Set_List_OverwriteItem(int count)
		{
			// get test data and create an empty dictionary
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>();

			// add data to the dictionary
			foreach (var kvp in data)
			{
				dict[kvp.Key] = kvp.Value;
			}

			// overwrite an item
			byte[] key = data.First().Key.ToArray();
			data[key] = ValueNotInTestData;
			dict[new ReadOnlySpan<byte>(key)] = ValueNotInTestData;

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests whether <see cref="ByteSequenceKeyedDictionary{TValue}.this[IReadOnlyList{byte}]"/> fails, if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void Indexer_Set_List_KeyNull()
		{
			// ReSharper disable once CollectionNeverQueried.Local
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var exception = Assert.Throws<ArgumentNullException>(() => dict[(IReadOnlyList<byte>)null] = default);
			Assert.Equal("key", exception.ParamName);
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.this[ReadOnlySpan<byte>]

		/// <summary>
		/// Tests accessing the key collection via <see cref="ByteSequenceKeyedDictionary{TValue}.this[ReadOnlySpan{byte}]"/>.
		/// The key of the element is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Indexer_Get_Span(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// test whether keys of test data are reported to be in the dictionary
			foreach (var kvp in data)
			{
				Assert.Equal(kvp.Value, dict[new ReadOnlySpan<byte>((byte[])kvp.Key)]);
			}
		}

		/// <summary>
		/// Tests accessing the key collection via <see cref="ByteSequenceKeyedDictionary{TValue}.this[ReadOnlySpan{byte}]"/>.
		/// The key of the element is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Indexer_Get_Span_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// test whether some other key is reported to be not in the dictionary
			Assert.Throws<KeyNotFoundException>(() => dict[KeyNotInTestData.ToArray().AsSpan()]);
		}

		/// <summary>
		/// Tests whether <see cref="ByteSequenceKeyedDictionary{TValue}.this[ReadOnlySpan{byte}]"/> fails, if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void Indexer_Get_Span_KeyNull()
		{
			// ReSharper disable once CollectionNeverUpdated.Local
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var exception = Assert.Throws<ArgumentNullException>(() => dict[new ReadOnlySpan<byte>(null)]);
			Assert.Equal("key", exception.ParamName);
		}

		/// <summary>
		/// Tests accessing the key collection via <see cref="ByteSequenceKeyedDictionary{TValue}.this[ReadOnlySpan{byte}]"/>.
		/// The item is added to the dictionary, because there is no item with the specified key, yet.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Indexer_Set_Span_NewItem(int count)
		{
			// get test data and create an empty dictionary
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>();

			// add data to the dictionary
			foreach (var kvp in data)
			{
				dict[new ReadOnlySpan<byte>((byte[])kvp.Key)] = kvp.Value;
			}

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests accessing the key collection via <see cref="ByteSequenceKeyedDictionary{TValue}.this[ReadOnlySpan{byte}]"/>.
		/// The item is overwritten, because there is already an item with the specified key in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes_WithoutZero))]
		public void Indexer_Set_Span_OverwriteItem(int count)
		{
			// get test data and create an empty dictionary
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>();

			// add data to the dictionary
			foreach (var kvp in data)
			{
				dict[new ReadOnlySpan<byte>((byte[])kvp.Key)] = kvp.Value;
			}

			// overwrite an item
			byte[] key = data.First().Key.ToArray();
			data[key] = ValueNotInTestData;
			dict[new ReadOnlySpan<byte>(key)] = ValueNotInTestData;

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests whether <see cref="ByteSequenceKeyedDictionary{TValue}.this[ReadOnlySpan{byte}]"/> fails, if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void Indexer_Set_Span_KeyNull()
		{
			// ReSharper disable once CollectionNeverQueried.Local
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var exception = Assert.Throws<ArgumentNullException>(() => dict[new ReadOnlySpan<byte>(null)] = default);
			Assert.Equal("key", exception.ParamName);
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.Add(IReadOnlyList<byte>, TValue)

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.Add(IReadOnlyList{byte},TValue)"/> method.
		/// </summary>
		/// <param name="count">Number of elements to add to the dictionary.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Add_List(int count)
		{
			// get test data and create an empty dictionary
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>();

			// add data to the dictionary
			foreach (var kvp in data)
			{
				dict.Add(kvp.Key, kvp.Value);
			}

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.Add(IReadOnlyList{byte},TValue)"/> method fails,
		/// if the key is already in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to add to the dictionary.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Add_List_DuplicateKey(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>();

			// add data to the dictionary
			KeyValuePair<IReadOnlyList<byte>, TValue>? first = null;
			KeyValuePair<IReadOnlyList<byte>, TValue>? last = null;
			foreach (var kvp in data)
			{
				if (first == null) first = kvp;
				last = kvp;
				dict.Add(kvp.Key, kvp.Value);
			}

			// try to add the first and the last element once again
			if (first != null) Assert.Throws<ArgumentException>(() => dict.Add(first.Value.Key, first.Value.Value));
			if (last != null) Assert.Throws<ArgumentException>(() => dict.Add(last.Value.Key, last.Value.Value));

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.Add(IReadOnlyList{byte},TValue)"/> method fails,
		/// if the key is <c>null</c>.
		/// </summary>
		[Fact]
		public void Add_List_KeyNull()
		{
			// ReSharper disable once CollectionNeverQueried.Local
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var exception = Assert.Throws<ArgumentNullException>(() => dict.Add((IReadOnlyList<byte>)null, default(TValue)));
			Assert.Equal("key", exception.ParamName); // the 'key' is actually not the name of the method parameter
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.Add(ReadOnlySpan<byte>, TValue)

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.Add(ReadOnlySpan{byte},TValue)"/> method.
		/// </summary>
		/// <param name="count">Number of elements to add to the dictionary.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Add_Span(int count)
		{
			// get test data and create an empty dictionary
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>();

			// add data to the dictionary
			foreach (var kvp in data)
			{
				dict.Add(new ReadOnlySpan<byte>((byte[])kvp.Key), kvp.Value);
			}

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.Add(ReadOnlySpan{byte},TValue)"/> method fails,
		/// if the key is already in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to add to the dictionary.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Add_Span_DuplicateKey(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>();

			// add data to the dictionary
			KeyValuePair<IReadOnlyList<byte>, TValue>? first = null;
			KeyValuePair<IReadOnlyList<byte>, TValue>? last = null;
			foreach (var kvp in data)
			{
				if (first == null) first = kvp;
				last = kvp;
				dict.Add(new ReadOnlySpan<byte>((byte[])kvp.Key), kvp.Value);
			}

			// try to add the first and the last element once again
			if (first != null) Assert.Throws<ArgumentException>(() => dict.Add(new ReadOnlySpan<byte>((byte[])first.Value.Key), first.Value.Value));
			if (last != null) Assert.Throws<ArgumentException>(() => dict.Add(new ReadOnlySpan<byte>((byte[])last.Value.Key), last.Value.Value));

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.Add(ReadOnlySpan{byte},TValue)"/> method fails,
		/// if the key is <c>null</c>.
		/// </summary>
		[Fact]
		public void Add_Span_KeyNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var exception = Assert.Throws<ArgumentNullException>(() => dict.Add(new ReadOnlySpan<byte>(null), default(TValue)));
			Assert.Equal("key", exception.ParamName); // the 'key' is actually not the name of the method parameter
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.Clear()

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.Clear"/> method.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Clear(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// clear the dictionary
			dict.Clear();

			// the dictionary should be empty now
			Assert.Equal(0, dict.Count);
			Assert.Empty(dict);
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.ContainsKey(IReadOnlyList<byte>)

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.ContainsKey(IReadOnlyList{byte})"/> method.
		/// The key of the element is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ContainsKey_List(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// test whether keys of test data are reported to be in the dictionary
			foreach (var kvp in data)
			{
				Assert.True(dict.ContainsKey(kvp.Key));
			}
		}

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.ContainsKey(IReadOnlyList{byte})"/> method.
		/// The key of the element is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ContainsKey_List_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// test whether some other key is reported to be not in the dictionary
			Assert.False(dict.ContainsKey(KeyNotInTestData));
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.ContainsKey(IReadOnlyList{byte})"/> method fails,
		/// if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void ContainsKey_List_KeyNull()
		{
			// ReSharper disable once CollectionNeverUpdated.Local
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var exception = Assert.Throws<ArgumentNullException>(() => dict.ContainsKey((IReadOnlyList<byte>)null));
			Assert.Equal("key", exception.ParamName);
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.ContainsKey(ReadOnlySpan<byte>)

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.ContainsKey(ReadOnlySpan{byte})"/> method.
		/// The key of the element is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ContainsKey_Span(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// test whether keys of test data are reported to be in the dictionary
			foreach (var kvp in data)
			{
				Assert.True(dict.ContainsKey(new ReadOnlySpan<byte>((byte[])kvp.Key)));
			}
		}

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.ContainsKey(ReadOnlySpan{byte})"/> method.
		/// The key of the element is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ContainsKey_Span_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// test whether some other key is reported to be not in the dictionary
			Assert.False(dict.ContainsKey(KeyNotInTestData.ToArray().AsSpan()));
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.ContainsKey(ReadOnlySpan{byte})"/> method fails,
		/// if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void ContainsKey_Span_KeyNull()
		{
			// ReSharper disable once CollectionNeverUpdated.Local
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var exception = Assert.Throws<ArgumentNullException>(() => dict.ContainsKey(new ReadOnlySpan<byte>(null)));
			Assert.Equal("key", exception.ParamName);
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.ContainsValue(TValue)

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.ContainsValue"/> method.
		/// The value of the element is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ContainsValue(int count)
		{
			// get test data and create a new dictionary with it,
			// replace one element with a null reference, if TValue is a reference type to check that too
			// (last element is better than the first one as it requires to iterate over all elements => better code coverage)
			var data = GetTestData(count);
			if (data.Count > 1 && !typeof(TValue).IsValueType) data[data.Last().Key] = default;
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// test whether keys of test data are reported to be in the dictionary
			foreach (var kvp in data)
			{
				Assert.True(dict.ContainsValue(kvp.Value));
			}
		}

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.ContainsValue"/> method.
		/// The value of the element is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ContainsValue_ValueNotFound(int count)
		{
			// get test data and create a new dictionary with it,
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// test whether some other value is reported to be not in the dictionary
			// (just take the default value of the value type, the test data does not contain the default value)
			Assert.False(dict.ContainsValue(ValueNotInTestData));
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.GetEnumerator() - incl. all enumerator functionality

		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void GetEnumerator(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// get an enumerator
			var enumerator = dict.GetEnumerator();

			// the enumerator should point to the position before the first valid element,
			// but the 'Current' property should not throw an exception
			var _ = enumerator.Current;

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			while (enumerator.MoveNext())
			{
				Assert.IsType<KeyValuePair<IReadOnlyList<byte>, TValue>>(enumerator.Current);
				var current = enumerator.Current;
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

			// modify the collection, the enumerator should recognize this
			dict[KeyNotInTestData] = ValueNotInTestData;
			Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());

			// dispose the enumerator
			enumerator.Dispose();
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.Remove(IReadOnlyList<byte>)

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.Remove(IReadOnlyList{byte})"/> method.
		/// The key of the element to remove is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Remove_List(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// remove elements in random order until the dictionary is empty
			var random = new Random();
			var remainingData = data.ToList();
			while (remainingData.Count > 0)
			{
				int index = random.Next(0, remainingData.Count - 1);
				dict.Remove(remainingData[index].Key);
				remainingData.RemoveAt(index);
				Assert.Equal(remainingData.Count, dict.Count);
			}

			// the dictionary should be empty now
			Assert.Equal(0, dict.Count);
			Assert.Empty(dict);
		}

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.Remove(IReadOnlyList{byte})"/> method.
		/// The key of the element to remove is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Remove_List_NotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// try to remove an element that does not exist
			Assert.False(dict.Remove(KeyNotInTestData));
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.Remove(IReadOnlyList{byte})"/> method fails,
		/// if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void Remove_List_KeyNull()
		{
			// ReSharper disable once CollectionNeverUpdated.Local
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var exception = Assert.Throws<ArgumentNullException>(() => dict.Remove((IReadOnlyList<byte>)null));
			Assert.Equal("key", exception.ParamName);
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.Remove(ReadOnlySpan<byte>)

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.Remove(ReadOnlySpan{byte})"/> method.
		/// The key of the element to remove is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Remove_Span(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// remove elements in random order until the dictionary is empty
			var random = new Random();
			var remainingData = data.ToList();
			while (remainingData.Count > 0)
			{
				int index = random.Next(0, remainingData.Count - 1);
				dict.Remove(new ReadOnlySpan<byte>((byte[])remainingData[index].Key));
				remainingData.RemoveAt(index);
				Assert.Equal(remainingData.Count, dict.Count);
			}

			// the dictionary should be empty now
			Assert.Equal(0, dict.Count);
			Assert.Empty(dict);
		}

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.Remove(ReadOnlySpan{byte})"/> method.
		/// The key of the element to remove is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void Remove_Span_NotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// try to remove an element that does not exist
			Assert.False(dict.Remove(KeyNotInTestData.ToArray().AsSpan()));
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.Remove(ReadOnlySpan{byte})"/> method fails,
		/// if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void Remove_Span_KeyNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var exception = Assert.Throws<ArgumentNullException>(() => dict.Remove((ReadOnlySpan<byte>)null));
			Assert.Equal("key", exception.ParamName);
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.TryAdd(IReadOnlyList<byte>, TValue)

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.TryAdd(IReadOnlyList{byte},TValue)"/> method.
		/// </summary>
		/// <param name="count">Number of elements to add to the dictionary.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void TryAdd_List(int count)
		{
			// get test data and create an empty dictionary
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>();

			// add data to the dictionary
			foreach (var kvp in data)
			{
				Assert.True(dict.TryAdd(kvp.Key, kvp.Value));
			}

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.TryAdd(IReadOnlyList{byte},TValue)"/> method fails,
		/// if the key is already in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to add to the dictionary.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void TryAdd_List_DuplicateKey(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>();

			// add data to the dictionary
			KeyValuePair<IReadOnlyList<byte>, TValue>? first = null;
			KeyValuePair<IReadOnlyList<byte>, TValue>? last = null;
			foreach (var kvp in data)
			{
				if (first == null) first = kvp;
				last = kvp;
				Assert.True(dict.TryAdd(kvp.Key, kvp.Value));
			}

			// try to add the first and the last element once again
			if (first != null) Assert.False(dict.TryAdd(first.Value.Key, first.Value.Value));
			if (last != null) Assert.False(dict.TryAdd(last.Value.Key, last.Value.Value));

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.TryAdd(IReadOnlyList{byte},TValue)"/> method fails,
		/// if the key is <c>null</c>.
		/// </summary>
		[Fact]
		public void TryAdd_List_KeyNull()
		{
			// ReSharper disable once CollectionNeverQueried.Local
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var exception = Assert.Throws<ArgumentNullException>(() => dict.Add((IReadOnlyList<byte>)null, default(TValue)));
			Assert.Equal("key", exception.ParamName); // the 'key' is actually not the name of the method parameter
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.TryAdd(ReadOnlySpan<byte>, TValue)

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.TryAdd(ReadOnlySpan{byte},TValue)"/> method.
		/// </summary>
		/// <param name="count">Number of elements to add to the dictionary.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void TryAdd_Span(int count)
		{
			// get test data and create an empty dictionary
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>();

			// add data to the dictionary
			foreach (var kvp in data)
			{
				Assert.True(dict.TryAdd(new ReadOnlySpan<byte>((byte[])kvp.Key), kvp.Value));
			}

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.TryAdd(ReadOnlySpan{byte},TValue)"/> method fails,
		/// if the key is already in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to add to the dictionary.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void TryAdd_Span_DuplicateKey(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>();

			// add data to the dictionary
			KeyValuePair<IReadOnlyList<byte>, TValue>? first = null;
			KeyValuePair<IReadOnlyList<byte>, TValue>? last = null;
			foreach (var kvp in data)
			{
				if (first == null) first = kvp;
				last = kvp;
				Assert.True(dict.TryAdd(new ReadOnlySpan<byte>((byte[])kvp.Key), kvp.Value));
			}

			// try to add the first and the last element once again
			if (first != null) Assert.False(dict.TryAdd(new ReadOnlySpan<byte>((byte[])first.Value.Key), first.Value.Value));
			if (last != null) Assert.False(dict.TryAdd(new ReadOnlySpan<byte>((byte[])last.Value.Key), last.Value.Value));

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.TryAdd(ReadOnlySpan{byte},TValue)"/> method fails,
		/// if the key is <c>null</c>.
		/// </summary>
		[Fact]
		public void TryAdd_Span_KeyNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var exception = Assert.Throws<ArgumentNullException>(() => dict.TryAdd(new ReadOnlySpan<byte>(null), default(TValue)));
			Assert.Equal("key", exception.ParamName); // the 'key' is actually not the name of the method parameter
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.TryGetValue(IReadOnlyList<byte>, out TValue)

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.TryGetValue(IReadOnlyList{byte},out TValue)"/> method.
		/// The key of the element to get is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void TryGetValue_List(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// test whether keys of test data are reported to be in the dictionary
			foreach (var kvp in data)
			{
				Assert.True(dict.TryGetValue(kvp.Key, out var value));
				Assert.Equal(kvp.Value, value);
			}
		}

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.TryGetValue(IReadOnlyList{byte},out TValue)"/> method.
		/// The key of the element to get is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void TryGetValue_List_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// test whether some other key is reported to be not in the dictionary
			Assert.False(dict.TryGetValue(KeyNotInTestData, out _));
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.TryGetValue(IReadOnlyList{byte},out TValue)"/> method fails,
		/// if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void TryGetValue_List_KeyNull()
		{
			// ReSharper disable once CollectionNeverUpdated.Local
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var exception = Assert.Throws<ArgumentNullException>(() => dict.TryGetValue((IReadOnlyList<byte>)null, out _));
			Assert.Equal("key", exception.ParamName);
		}

		#endregion

		#region ByteSequenceKeyedDictionary<TValue>.TryGetValue(ReadOnlySpan<byte>, out TValue)

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.TryGetValue(ReadOnlySpan{byte},out TValue)"/> method.
		/// The key of the element to get is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void TryGetValue_Span(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// test whether keys of test data are reported to be in the dictionary
			foreach (var kvp in data)
			{
				Assert.True(dict.TryGetValue(new ReadOnlySpan<byte>((byte[])kvp.Key), out var value));
				Assert.Equal(kvp.Value, value);
			}
		}

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.TryGetValue(ReadOnlySpan{byte},out TValue)"/> method.
		/// The key of the element to get is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void TryGetValue_Span_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);

			// test whether some other key is reported to be not in the dictionary
			Assert.False(dict.TryGetValue(KeyNotInTestData.ToArray().AsSpan(), out _));
		}

		/// <summary>
		/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.TryGetValue(ReadOnlySpan{byte},out TValue)"/> method fails,
		/// if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void TryGetValue_Span_KeyNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var exception = Assert.Throws<ArgumentNullException>(() => dict.TryGetValue((ReadOnlySpan<byte>)null, out _));
			Assert.Equal("key", exception.ParamName);
		}

		#endregion

		#region Add-Remove-Add, Entry Recycling (IReadOnlyList<byte>)

		/// <summary>
		/// Tests adding items using the <see cref="ByteSequenceKeyedDictionary{TValue}.Add(IReadOnlyList{byte},TValue)"/> method,
		/// then removing items using the <see cref="ByteSequenceKeyedDictionary{TValue}.Remove(IReadOnlyList{byte})"/> method,
		/// then adding the removed items again.
		/// This tests whether the free-list in the dictionary is used correctly.
		/// </summary>
		/// <param name="count">Number of elements to add to the dictionary.</param>
		[Theory]
		[InlineData(10000)]
		public void AddAfterRemove_List(int count)
		{
			// get test data and create an empty dictionary
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>();

			// add data to the dictionary
			foreach (var kvp in data)
			{
				dict.Add(kvp.Key, kvp.Value);
			}

			// compare collection elements with the expected key/value pairs
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);

			// remove elements in random order until the dictionary is empty
			var random = new Random();
			var remainingData = data.ToList();
			while (remainingData.Count > 0)
			{
				int index = random.Next(0, remainingData.Count - 1);
				dict.Remove(remainingData[index].Key);
				remainingData.RemoveAt(index);
				Assert.Equal(remainingData.Count, dict.Count);
			}

			// the dictionary should be empty now
			Assert.Equal(0, dict.Count);
			Assert.Empty(dict);

			// add data to the dictionary
			foreach (var kvp in data)
			{
				dict.Add(kvp.Key, kvp.Value);
			}

			// the dictionary should now contain the expected key/value pairs
			enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		#endregion

		#region Add-Remove-Add, Entry Recycling (ReadOnlySpan<byte>)

		/// <summary>
		/// Tests adding items using the <see cref="ByteSequenceKeyedDictionary{TValue}.Add(ReadOnlySpan{byte},TValue)"/> method,
		/// then removing items using the <see cref="ByteSequenceKeyedDictionary{TValue}.Remove(ReadOnlySpan{byte})"/> method,
		/// then adding the removed items again.
		/// This tests whether the free-list in the dictionary is used correctly.
		/// </summary>
		/// <param name="count">Number of elements to add to the dictionary.</param>
		[Theory]
		[InlineData(10000)]
		public void AddAfterRemove_Span(int count)
		{
			// get test data and create an empty dictionary
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>();

			// add data to the dictionary
			foreach (var kvp in data)
			{
				dict.Add(kvp.Key.ToArray().AsSpan(), kvp.Value);
			}

			// compare collection elements with the expected key/value pairs
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);

			// remove elements in random order until the dictionary is empty
			var random = new Random();
			var remainingData = data.ToList();
			while (remainingData.Count > 0)
			{
				int index = random.Next(0, remainingData.Count - 1);
				dict.Remove(remainingData[index].Key.ToArray().AsSpan());
				remainingData.RemoveAt(index);
				Assert.Equal(remainingData.Count, dict.Count);
			}

			// the dictionary should be empty now
			Assert.Equal(0, dict.Count);
			Assert.Empty(dict);

			// add data to the dictionary
			foreach (var kvp in data)
			{
				dict.Add(kvp.Key.ToArray().AsSpan(), kvp.Value);
			}

			// the dictionary should now contain the expected key/value pairs
			enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (var kvp in dict) enumerated.Add(kvp);
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		#endregion
	}

}
