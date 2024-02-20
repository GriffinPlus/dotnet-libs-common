///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace GriffinPlus.Lib.Collections;

/// <summary>
/// Unit tests targeting the <see cref="ByteSequenceKeyedDictionary{TValue}"/> class.
/// </summary>
public abstract partial class ByteSequenceKeyedDictionaryTests_Base<TValue> : GenericDictionaryTests_Base<IReadOnlyList<byte>, TValue>
{
	/// <summary>
	/// Gets a comparer for comparing keys.
	/// </summary>
	protected override IComparer<IReadOnlyList<byte>> KeyComparer => new ReadOnlyListComparer<byte>();

	/// <summary>
	/// Gets an equality comparer for comparing keys.
	/// </summary>
	protected override IEqualityComparer<IReadOnlyList<byte>> KeyEqualityComparer => new ReadOnlyListEqualityComparer<byte>();

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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>(data);
		int expectedCapacity = count > 0 ? HashHelpers.GetPrime(count) : 0;
		Assert.Equal(expectedCapacity, dict.Capacity); // the capacity should always be prime
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>(data);

		// test whether keys of test data are reported to be in the dictionary
		foreach (KeyValuePair<IReadOnlyList<byte>, TValue> kvp in data)
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>(data);

		// test whether some other key is reported to be not in the dictionary
		Assert.Throws<KeyNotFoundException>(() => dict[[.. KeyNotInTestData]]);
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>();

		// add data to the dictionary
		foreach (KeyValuePair<IReadOnlyList<byte>, TValue> kvp in data)
		{
			dict[new ReadOnlySpan<byte>((byte[])kvp.Key)] = kvp.Value;
		}

		// enumerate the key/value pairs in the dictionary
		List<KeyValuePair<IReadOnlyList<byte>, TValue>> enumerated = [.. dict];

		// compare collection elements with the expected key/value pairs
		Assert.Equal(
			data.OrderBy(x => x.Key, KeyComparer),
			enumerated.OrderBy(x => x.Key, KeyComparer),
			KeyValuePairEqualityComparer);
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>();

		// add data to the dictionary
		foreach (KeyValuePair<IReadOnlyList<byte>, TValue> kvp in data)
		{
			dict[new ReadOnlySpan<byte>((byte[])kvp.Key)] = kvp.Value;
		}

		// overwrite an item
		byte[] key = [.. data.First().Key];
		data[key] = ValueNotInTestData;
		dict[new ReadOnlySpan<byte>(key)] = ValueNotInTestData;

		// enumerate the key/value pairs in the dictionary
		List<KeyValuePair<IReadOnlyList<byte>, TValue>> enumerated = [.. dict];

		// compare collection elements with the expected key/value pairs
		Assert.Equal(
			data.OrderBy(x => x.Key, KeyComparer),
			enumerated.OrderBy(x => x.Key, KeyComparer),
			KeyValuePairEqualityComparer);
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>();

		// add data to the dictionary
		foreach (KeyValuePair<IReadOnlyList<byte>, TValue> kvp in data)
		{
			dict.Add(new ReadOnlySpan<byte>((byte[])kvp.Key), kvp.Value);
		}

		// enumerate the key/value pairs in the dictionary
		List<KeyValuePair<IReadOnlyList<byte>, TValue>> enumerated = [.. dict];

		// compare collection elements with the expected key/value pairs
		Assert.Equal(
			data.OrderBy(x => x.Key, KeyComparer),
			enumerated.OrderBy(x => x.Key, KeyComparer),
			KeyValuePairEqualityComparer);
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>();

		// add data to the dictionary
		KeyValuePair<IReadOnlyList<byte>, TValue>? first = null;
		KeyValuePair<IReadOnlyList<byte>, TValue>? last = null;
		foreach (KeyValuePair<IReadOnlyList<byte>, TValue> kvp in data)
		{
			first ??= kvp;
			last = kvp;
			dict.Add(new ReadOnlySpan<byte>((byte[])kvp.Key), kvp.Value);
		}

		// try to add the first and the last element once again
		if (first != null) Assert.Throws<ArgumentException>(() => dict.Add(new ReadOnlySpan<byte>((byte[])first.Value.Key), first.Value.Value));
		if (last != null) Assert.Throws<ArgumentException>(() => dict.Add(new ReadOnlySpan<byte>((byte[])last.Value.Key), last.Value.Value));

		// enumerate the key/value pairs in the dictionary
		List<KeyValuePair<IReadOnlyList<byte>, TValue>> enumerated = [.. dict];

		// compare collection elements with the expected key/value pairs
		Assert.Equal(
			data.OrderBy(x => x.Key, KeyComparer),
			enumerated.OrderBy(x => x.Key, KeyComparer),
			KeyValuePairEqualityComparer);
	}

	/// <summary>
	/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.Add(ReadOnlySpan{byte},TValue)"/> method fails,
	/// if the key is <c>null</c>.
	/// </summary>
	[Fact]
	public void Add_Span_KeyNull()
	{
		var dict = new ByteSequenceKeyedDictionary<TValue>();
		var exception = Assert.Throws<ArgumentNullException>(() => dict.Add(new ReadOnlySpan<byte>(null), default));
		Assert.Equal("key", exception.ParamName); // the 'key' is actually not the name of the method parameter
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>(data);

		// test whether keys of test data are reported to be in the dictionary
		foreach (KeyValuePair<IReadOnlyList<byte>, TValue> kvp in data)
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>(data);

		// test whether some other key is reported to be not in the dictionary
		Assert.False(dict.ContainsKey([.. KeyNotInTestData]));
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

	#region ByteSequenceKeyedDictionary<TValue>.GetEnumerator() - incl. all enumerator functionality

	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void GetEnumerator(int count)
	{
		// get test data and create a new dictionary with it
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>(data);

		// get an enumerator
		ByteSequenceKeyedDictionary<TValue>.Enumerator enumerator = dict.GetEnumerator();

		// the enumerator should point to the position before the first valid element,
		// but the 'Current' property should not throw an exception
		KeyValuePair<IReadOnlyList<byte>, TValue> _ = enumerator.Current;

		// enumerate the key/value pairs in the dictionary
		var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
		while (enumerator.MoveNext())
		{
			Assert.IsType<KeyValuePair<IReadOnlyList<byte>, TValue>>(enumerator.Current);
			KeyValuePair<IReadOnlyList<byte>, TValue> current = enumerator.Current;
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>(data);

		// remove elements in random order until the dictionary is empty
		var random = new Random();
		List<KeyValuePair<IReadOnlyList<byte>, TValue>> remainingData = [.. data];
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>(data);

		// try to remove an element that does not exist
		Assert.False(dict.Remove([.. KeyNotInTestData]));
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>();

		// add data to the dictionary
		foreach (KeyValuePair<IReadOnlyList<byte>, TValue> kvp in data)
		{
			Assert.True(dict.TryAdd(new ReadOnlySpan<byte>((byte[])kvp.Key), kvp.Value));
		}

		// enumerate the key/value pairs in the dictionary
		List<KeyValuePair<IReadOnlyList<byte>, TValue>> enumerated = [.. dict];

		// compare collection elements with the expected key/value pairs
		Assert.Equal(
			data.OrderBy(x => x.Key, KeyComparer),
			enumerated.OrderBy(x => x.Key, KeyComparer),
			KeyValuePairEqualityComparer);
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>();

		// add data to the dictionary
		KeyValuePair<IReadOnlyList<byte>, TValue>? first = null;
		KeyValuePair<IReadOnlyList<byte>, TValue>? last = null;
		foreach (KeyValuePair<IReadOnlyList<byte>, TValue> kvp in data)
		{
			first ??= kvp;
			last = kvp;
			Assert.True(dict.TryAdd(new ReadOnlySpan<byte>((byte[])kvp.Key), kvp.Value));
		}

		// try to add the first and the last element once again
		if (first != null) Assert.False(dict.TryAdd(new ReadOnlySpan<byte>((byte[])first.Value.Key), first.Value.Value));
		if (last != null) Assert.False(dict.TryAdd(new ReadOnlySpan<byte>((byte[])last.Value.Key), last.Value.Value));

		// enumerate the key/value pairs in the dictionary
		List<KeyValuePair<IReadOnlyList<byte>, TValue>> enumerated = [.. dict];

		// compare collection elements with the expected key/value pairs
		Assert.Equal(
			data.OrderBy(x => x.Key, KeyComparer),
			enumerated.OrderBy(x => x.Key, KeyComparer),
			KeyValuePairEqualityComparer);
	}

	/// <summary>
	/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.TryAdd(ReadOnlySpan{byte},TValue)"/> method fails,
	/// if the key is <c>null</c>.
	/// </summary>
	[Fact]
	public void TryAdd_Span_KeyNull()
	{
		var dict = new ByteSequenceKeyedDictionary<TValue>();
		var exception = Assert.Throws<ArgumentNullException>(() => dict.TryAdd(new ReadOnlySpan<byte>(null), default));
		Assert.Equal("key", exception.ParamName); // the 'key' is actually not the name of the method parameter
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>(data);

		// test whether keys of test data are reported to be in the dictionary
		foreach (KeyValuePair<IReadOnlyList<byte>, TValue> kvp in data)
		{
			Assert.True(dict.TryGetValue(new ReadOnlySpan<byte>((byte[])kvp.Key), out TValue value));
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>(data);

		// test whether some other key is reported to be not in the dictionary
		Assert.False(dict.TryGetValue([.. KeyNotInTestData], out TValue _));
	}

	/// <summary>
	/// Tests whether the <see cref="ByteSequenceKeyedDictionary{TValue}.TryGetValue(ReadOnlySpan{byte},out TValue)"/> method fails,
	/// if the passed key is <c>null</c>.
	/// </summary>
	[Fact]
	public void TryGetValue_Span_KeyNull()
	{
		var dict = new ByteSequenceKeyedDictionary<TValue>();
		var exception = Assert.Throws<ArgumentNullException>(() => dict.TryGetValue((ReadOnlySpan<byte>)null, out TValue _));
		Assert.Equal("key", exception.ParamName);
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
		IDictionary<IReadOnlyList<byte>, TValue> data = GetTestData(count);
		var dict = new ByteSequenceKeyedDictionary<TValue>();

		// add data to the dictionary
		foreach (KeyValuePair<IReadOnlyList<byte>, TValue> kvp in data)
		{
			dict.Add([.. kvp.Key], kvp.Value);
		}

		// compare collection elements with the expected key/value pairs
		List<KeyValuePair<IReadOnlyList<byte>, TValue>> enumerated = [.. dict];
		Assert.Equal(
			data.OrderBy(x => x.Key, KeyComparer),
			enumerated.OrderBy(x => x.Key, KeyComparer),
			KeyValuePairEqualityComparer);

		// remove elements in random order until the dictionary is empty
		var random = new Random();
		List<KeyValuePair<IReadOnlyList<byte>, TValue>> remainingData = [.. data];
		while (remainingData.Count > 0)
		{
			int index = random.Next(0, remainingData.Count - 1);
			dict.Remove([.. remainingData[index].Key]);
			remainingData.RemoveAt(index);
			Assert.Equal(remainingData.Count, dict.Count);
		}

		// the dictionary should be empty now
		Assert.Equal(0, dict.Count);
		Assert.Empty(dict);

		// add data to the dictionary
		foreach (KeyValuePair<IReadOnlyList<byte>, TValue> kvp in data)
		{
			dict.Add([.. kvp.Key], kvp.Value);
		}

		// the dictionary should now contain the expected key/value pairs
		enumerated = [.. dict];
		Assert.Equal(
			data.OrderBy(x => x.Key, KeyComparer),
			enumerated.OrderBy(x => x.Key, KeyComparer),
			KeyValuePairEqualityComparer);
	}

	#endregion
}
