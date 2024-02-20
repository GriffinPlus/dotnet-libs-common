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

public abstract partial class GenericDictionaryTests_Base<TKey, TValue>
{
	#region IDictionary<TKey,TValue>.this[TKey]

	/// <summary>
	/// Tests accessing the key collection via <see cref="IDictionary{TKey,TValue}.this[TKey]"/>.
	/// The key of the element is in the dictionary.
	/// </summary>
	/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void IDictionaryT_Indexer_Get_List(int count)
	{
		// get test data and create a new dictionary with it
		IDictionary<TKey, TValue> data = GetTestData(count);
		var dict = GetDictionary(data) as IDictionary<TKey, TValue>;

		// test whether keys of test data are reported to be in the dictionary
		foreach (KeyValuePair<TKey, TValue> kvp in data)
		{
			Assert.Equal(kvp.Value, dict[kvp.Key]);
		}
	}

	/// <summary>
	/// Tests accessing the key collection via <see cref="IDictionary{TKey,TValue}.this[TKey]"/>.
	/// The key of the element is not in the dictionary.
	/// </summary>
	/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void IDictionaryT_Indexer_Get_List_KeyNotFound(int count)
	{
		// get test data and create a new dictionary with it
		IDictionary<TKey, TValue> data = GetTestData(count);
		var dict = GetDictionary(data) as IDictionary<TKey, TValue>;

		// test whether some other key is reported to be not in the dictionary
		Assert.Throws<KeyNotFoundException>(() => dict[KeyNotInTestData]);
	}

	/// <summary>
	/// Tests whether <see cref="IDictionary{TKey,TValue}.this[TKey]"/> fails, if the passed key is <c>null</c>.
	/// Only for reference types.
	/// </summary>
	[Fact]
	public void IDictionaryT_Indexer_Get_List_KeyNull()
	{
		if (typeof(TKey).IsValueType) return;
		var dict = GetDictionary() as IDictionary<TKey, TValue>;
		// ReSharper disable once AssignNullToNotNullAttribute
		var exception = Assert.Throws<ArgumentNullException>(() => dict[default]);
		Assert.Equal("key", exception.ParamName);
	}

	/// <summary>
	/// Tests accessing the key collection via <see cref="IDictionary{TKey,TValue}.this[TKey]"/>.
	/// The item is added to the dictionary, because there is no item with the specified key, yet.
	/// </summary>
	/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void IDictionaryT_Indexer_Set_List_NewItem(int count)
	{
		// get test data and create an empty dictionary
		IDictionary<TKey, TValue> data = GetTestData(count);
		var dict = GetDictionary(data) as IDictionary<TKey, TValue>;

		// add data to the dictionary
		foreach (KeyValuePair<TKey, TValue> kvp in data)
		{
			dict[kvp.Key] = kvp.Value;
		}

		// enumerate the key/value pairs in the dictionary
		List<KeyValuePair<TKey, TValue>> enumerated = [.. dict];

		// compare collection elements with the expected key/value pairs
		Assert.Equal(
			data.OrderBy(x => x.Key, KeyComparer),
			enumerated.OrderBy(x => x.Key, KeyComparer),
			KeyValuePairEqualityComparer);
	}

	/// <summary>
	/// Tests accessing the key collection via <see cref="IDictionary{TKey,TValue}.this[TKey]"/>.
	/// The item is overwritten, because there is already an item with the specified key in the dictionary.
	/// </summary>
	/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes_WithoutZero))]
	public void IDictionaryT_Indexer_Set_List_OverwriteItem(int count)
	{
		// get test data and create an empty dictionary
		IDictionary<TKey, TValue> data = GetTestData(count);
		var dict = GetDictionary(data) as IDictionary<TKey, TValue>;

		// add data to the dictionary
		foreach (KeyValuePair<TKey, TValue> kvp in data)
		{
			dict[kvp.Key] = kvp.Value;
		}

		// overwrite an item
		TKey key = data.First().Key;
		data[key] = ValueNotInTestData;
		dict[key] = ValueNotInTestData;

		// enumerate the key/value pairs in the dictionary
		List<KeyValuePair<TKey, TValue>> enumerated = [.. dict];

		// compare collection elements with the expected key/value pairs
		Assert.Equal(
			data.OrderBy(x => x.Key, KeyComparer),
			enumerated.OrderBy(x => x.Key, KeyComparer),
			KeyValuePairEqualityComparer);
	}

	/// <summary>
	/// Tests whether <see cref="IDictionary{TKey,TValue}.this[TKey]"/> fails, if the passed key is <c>null</c>.
	/// Only for reference types.
	/// </summary>
	[Fact]
	public void IDictionaryT_Indexer_Set_List_KeyNull()
	{
		if (typeof(TKey).IsValueType) return;
		var dict = GetDictionary() as IDictionary<TKey, TValue>;
		// ReSharper disable once AssignNullToNotNullAttribute
		var exception = Assert.Throws<ArgumentNullException>(() => dict[default] = default);
		Assert.Equal("key", exception.ParamName);
	}

	#endregion

	#region IDictionary<TKey,TValue>.Keys

	/// <summary>
	/// Tests accessing the key collection via <see cref="IDictionary{TKey,TValue}.Keys"/>.
	/// The key collection should present all keys in the dictionary.
	/// </summary>
	/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void IDictionaryT_Keys(int count)
	{
		// get test data and create a new dictionary with it
		IDictionary<TKey, TValue> expected = GetTestData(count);
		var dict = GetDictionary(expected) as IDictionary<TKey, TValue>;

		// enumerate the keys in the dictionary
		List<TKey> enumerated = [.. dict.Keys];

		// compare collection elements with the expected values
		Assert.Equal(
			expected.Select(x => x.Key).OrderBy(x => x, KeyComparer),
			enumerated.OrderBy(x => x, KeyComparer),
			KeyEqualityComparer);
	}

	#endregion

	#region IDictionary<TKey,TValue>.Values

	/// <summary>
	/// Tests accessing the value collection via <see cref="IDictionary{TKey,TValue}.Values"/>.
	/// The value collection should present all values in the dictionary.
	/// </summary>
	/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void IDictionaryT_Values(int count)
	{
		// get test data and create a new dictionary with it
		IDictionary<TKey, TValue> expected = GetTestData(count);
		var dict = GetDictionary(expected) as IDictionary<TKey, TValue>;

		// enumerate the values in the dictionary
		List<TValue> enumerated = [.. dict.Values];

		// compare collection elements with the expected values
		Assert.Equal(
			expected.Select(x => x.Value).OrderBy(x => x, ValueComparer),
			enumerated.OrderBy(x => x, ValueComparer),
			ValueEqualityComparer);
	}

	#endregion

	#region IDictionary<TKey,TValue>.Add(TKey, TValue)

	/// <summary>
	/// Tests the <see cref="IDictionary{TKey,TValue}.Add(TKey,TValue)"/> method.
	/// </summary>
	/// <param name="count">Number of elements to add to the dictionary.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void IDictionaryT_Add_List(int count)
	{
		// get test data and create an empty dictionary
		IDictionary<TKey, TValue> data = GetTestData(count);
		var dict = GetDictionary() as IDictionary<TKey, TValue>;

		// add data to the dictionary
		foreach (KeyValuePair<TKey, TValue> kvp in data)
		{
			dict.Add(kvp.Key, kvp.Value);
		}

		// enumerate the key/value pairs in the dictionary
		List<KeyValuePair<TKey, TValue>> enumerated = [.. dict];

		// compare collection elements with the expected key/value pairs
		Assert.Equal(
			data.OrderBy(x => x.Key, KeyComparer),
			enumerated.OrderBy(x => x.Key, KeyComparer),
			KeyValuePairEqualityComparer);
	}

	/// <summary>
	/// Tests whether the <see cref="IDictionary{TKey,TValue}.Add(TKey,TValue)"/> method fails,
	/// if the key is already in the dictionary.
	/// </summary>
	/// <param name="count">Number of elements to add to the dictionary.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void IDictionaryT_Add_List_DuplicateKey(int count)
	{
		// get test data and create a new dictionary with it
		IDictionary<TKey, TValue> data = GetTestData(count);
		var dict = GetDictionary() as IDictionary<TKey, TValue>;

		// add data to the dictionary
		KeyValuePair<TKey, TValue>? first = null;
		KeyValuePair<TKey, TValue>? last = null;
		foreach (KeyValuePair<TKey, TValue> kvp in data)
		{
			first ??= kvp;
			last = kvp;
			dict.Add(kvp.Key, kvp.Value);
		}

		// try to add the first and the last element once again
		if (first != null) Assert.Throws<ArgumentException>(() => dict.Add(first.Value.Key, first.Value.Value));
		if (last != null) Assert.Throws<ArgumentException>(() => dict.Add(last.Value.Key, last.Value.Value));

		// enumerate the key/value pairs in the dictionary
		List<KeyValuePair<TKey, TValue>> enumerated = [.. dict];

		// compare collection elements with the expected key/value pairs
		Assert.Equal(
			data.OrderBy(x => x.Key, KeyComparer),
			enumerated.OrderBy(x => x.Key, KeyComparer),
			KeyValuePairEqualityComparer);
	}

	/// <summary>
	/// Tests whether the <see cref="IDictionary{TKey,TValue}.Add(TKey,TValue)"/> method fails, if the key is <c>null</c>.
	/// Only for reference types.
	/// </summary>
	[Fact]
	public void IDictionaryT_Add_List_KeyNull()
	{
		if (typeof(TKey).IsValueType) return;
		var dict = GetDictionary() as IDictionary<TKey, TValue>;
		// ReSharper disable once AssignNullToNotNullAttribute
		var exception = Assert.Throws<ArgumentNullException>(() => dict.Add(default, default));
		Assert.Equal("key", exception.ParamName); // the 'key' is actually not the name of the method parameter
	}

	#endregion

	#region IDictionary<TKey,TValue>.ContainsKey(TKey)

	/// <summary>
	/// Tests the <see cref="IDictionary{TKey,TValue}.ContainsKey(TKey)"/> method.
	/// The key of the element is in the dictionary.
	/// </summary>
	/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void IDictionaryT_ContainsKey_List(int count)
	{
		// get test data and create a new dictionary with it
		IDictionary<TKey, TValue> data = GetTestData(count);
		var dict = GetDictionary(data) as IDictionary<TKey, TValue>;

		// test whether keys of test data are reported to be in the dictionary
		foreach (KeyValuePair<TKey, TValue> kvp in data)
		{
			Assert.True(dict.ContainsKey(kvp.Key));
		}
	}

	/// <summary>
	/// Tests the <see cref="IDictionary{TKey,TValue}.ContainsKey(TKey)"/> method.
	/// The key of the element is not in the dictionary.
	/// </summary>
	/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void IDictionaryT_ContainsKey_List_KeyNotFound(int count)
	{
		// get test data and create a new dictionary with it
		IDictionary<TKey, TValue> data = GetTestData(count);
		var dict = GetDictionary(data) as IDictionary<TKey, TValue>;

		// test whether some other key is reported to be not in the dictionary
		Assert.False(dict.ContainsKey(KeyNotInTestData));
	}

	/// <summary>
	/// Tests whether the <see cref="IDictionary{TKey,TValue}.ContainsKey(TKey)"/> method fails, if the passed key is <c>null</c>.
	/// Only for reference types.
	/// </summary>
	[Fact]
	public void IDictionaryT_ContainsKey_List_KeyNull()
	{
		if (typeof(TKey).IsValueType) return;
		var dict = GetDictionary() as IDictionary<TKey, TValue>;
		// ReSharper disable once AssignNullToNotNullAttribute
		var exception = Assert.Throws<ArgumentNullException>(() => dict.ContainsKey(default));
		Assert.Equal("key", exception.ParamName);
	}

	#endregion

	#region IDictionary<TKey,TValue>.Remove(TKey)

	/// <summary>
	/// Tests the <see cref="IDictionary{TKey,TValue}.Remove(TKey)"/> method.
	/// The key of the element to remove is in the dictionary.
	/// </summary>
	/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void IDictionaryT_Remove_List(int count)
	{
		// get test data and create a new dictionary with it
		IDictionary<TKey, TValue> data = GetTestData(count);
		var dict = GetDictionary(data) as IDictionary<TKey, TValue>;

		// remove elements in random order until the dictionary is empty
		var random = new Random();
		List<KeyValuePair<TKey, TValue>> remainingData = [.. data];
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
	/// Tests the <see cref="IDictionary{TKey,TValue}.Remove(TKey)"/> method.
	/// The key of the element to remove is not in the dictionary.
	/// </summary>
	/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void IDictionaryT_Remove_List_NotFound(int count)
	{
		// get test data and create a new dictionary with it
		IDictionary<TKey, TValue> data = GetTestData(count);
		var dict = GetDictionary(data) as IDictionary<TKey, TValue>;

		// try to remove an element that does not exist
		Assert.False(dict.Remove(KeyNotInTestData));
	}

	/// <summary>
	/// Tests whether the <see cref="IDictionary{TKey,TValue}.Remove(TKey)"/> method fails, if the passed key is <c>null</c>.
	/// Only for reference types.
	/// </summary>
	[Fact]
	public void IDictionaryT_Remove_List_KeyNull()
	{
		if (typeof(TKey).IsValueType) return;
		var dict = GetDictionary() as IDictionary<TKey, TValue>;
		// ReSharper disable once AssignNullToNotNullAttribute
		var exception = Assert.Throws<ArgumentNullException>(() => dict.Remove(default));
		Assert.Equal("key", exception.ParamName);
	}

	#endregion

	#region IDictionary<TKey,TValue>.TryGetValue(TKey, out TValue)

	/// <summary>
	/// Tests the <see cref="IDictionary{TKey,TValue}.TryGetValue(TKey,out TValue)"/> method.
	/// The key of the element to get is in the dictionary.
	/// </summary>
	/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void IDictionaryT_TryGetValue_List(int count)
	{
		// get test data and create a new dictionary with it
		IDictionary<TKey, TValue> data = GetTestData(count);
		var dict = GetDictionary(data) as IDictionary<TKey, TValue>;

		// test whether keys of test data are reported to be in the dictionary
		foreach (KeyValuePair<TKey, TValue> kvp in data)
		{
			Assert.True(dict.TryGetValue(kvp.Key, out TValue value));
			Assert.Equal(kvp.Value, value);
		}
	}

	/// <summary>
	/// Tests the <see cref="IDictionary{TKey,TValue}.TryGetValue(TKey,out TValue)"/> method.
	/// The key of the element to get is not in the dictionary.
	/// </summary>
	/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void IDictionaryT_TryGetValue_List_KeyNotFound(int count)
	{
		// get test data and create a new dictionary with it
		IDictionary<TKey, TValue> data = GetTestData(count);
		var dict = GetDictionary(data) as IDictionary<TKey, TValue>;

		// test whether some other key is reported to be not in the dictionary
		Assert.False(dict.TryGetValue(KeyNotInTestData, out TValue _));
	}

	/// <summary>
	/// Tests whether the <see cref="IDictionary{TKey,TValue}.TryGetValue(TKey,out TValue)"/> method fails, if the passed key is <c>null</c>.
	/// Only for reference types.
	/// </summary>
	[Fact]
	public void IDictionary_TryGetValue_List_KeyNull()
	{
		if (typeof(TKey).IsValueType) return;
		var dict = GetDictionary() as IDictionary<TKey, TValue>;
		// ReSharper disable once AssignNullToNotNullAttribute
		var exception = Assert.Throws<ArgumentNullException>(() => dict.TryGetValue(default, out TValue _));
		Assert.Equal("key", exception.ParamName);
	}

	#endregion

	#region Add-Remove-Add, Entry Recycling

	/// <summary>
	/// Tests adding items using the <see cref="IDictionary{TKey,TValue}.Add(TKey,TValue)"/> method,
	/// then removing items using the <see cref="IDictionary{TKey,TValue}.Remove(TKey)"/> method,
	/// then adding the removed items again.
	/// This tests whether the free-list in the dictionary is used correctly.
	/// </summary>
	/// <param name="count">Number of elements to add to the dictionary.</param>
	[Theory]
	[InlineData(5000)]
	public void AddAfterRemove_List(int count)
	{
		// get test data and create an empty dictionary
		IDictionary<TKey, TValue> data = GetTestData(count);
		var dict = GetDictionary() as IDictionary<TKey, TValue>;

		// add data to the dictionary
		foreach (KeyValuePair<TKey, TValue> kvp in data)
		{
			dict.Add(kvp.Key, kvp.Value);
		}

		// compare collection elements with the expected key/value pairs
		List<KeyValuePair<TKey, TValue>> enumerated = [.. dict];
		Assert.Equal(
			data.OrderBy(x => x.Key, KeyComparer),
			enumerated.OrderBy(x => x.Key, KeyComparer),
			KeyValuePairEqualityComparer);

		// remove elements in random order until the dictionary is empty
		var random = new Random();
		List<KeyValuePair<TKey, TValue>> remainingData = [.. data];
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
		foreach (KeyValuePair<TKey, TValue> kvp in data)
		{
			dict.Add(kvp.Key, kvp.Value);
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
