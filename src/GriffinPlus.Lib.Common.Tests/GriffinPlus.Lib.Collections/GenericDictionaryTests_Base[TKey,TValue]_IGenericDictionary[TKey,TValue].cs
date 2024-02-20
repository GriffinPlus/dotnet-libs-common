///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace GriffinPlus.Lib.Collections;

public abstract partial class GenericDictionaryTests_Base<TKey, TValue>
{
	#region IGenericDictionary<TKey,TValue>.ContainsValue(TValue)

	/// <summary>
	/// Tests the <see cref="IGenericDictionary{TKey,TValue}.ContainsValue(TValue)"/> method.
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
		IDictionary<TKey, TValue> data = GetTestData(count);
		if (data.Count > 1 && !typeof(TValue).IsValueType) data[data.Last().Key] = default;
		IGenericDictionary<TKey, TValue> dict = GetDictionary(data);

		// test whether keys of test data are reported to be in the dictionary
		foreach (KeyValuePair<TKey, TValue> kvp in data)
		{
			Assert.True(dict.ContainsValue(kvp.Value));
		}
	}

	/// <summary>
	/// Tests the <see cref="IGenericDictionary{TKey,TValue}.ContainsValue"/> method.
	/// The value of the element is not in the dictionary.
	/// </summary>
	/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void ContainsValue_ValueNotFound(int count)
	{
		// get test data and create a new dictionary with it,
		IDictionary<TKey, TValue> data = GetTestData(count);
		IGenericDictionary<TKey, TValue> dict = GetDictionary(data);

		// test whether some other value is reported to be not in the dictionary
		// (just take the default value of the value type, the test data does not contain the default value)
		Assert.False(dict.ContainsValue(ValueNotInTestData));
	}

	#endregion

	#region IGenericDictionary<TKey,TValue>.TryAdd(TKey, TValue)

	/// <summary>
	/// Tests the <see cref="IGenericDictionary{TKey,TValue}.TryAdd(TKey,TValue)"/> method.
	/// </summary>
	/// <param name="count">Number of elements to add to the dictionary.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void IGenericDictionaryT_TryAdd_List(int count)
	{
		// get test data and create an empty dictionary
		IDictionary<TKey, TValue> data = GetTestData(count);
		IGenericDictionary<TKey, TValue> dict = GetDictionary();

		// add data to the dictionary
		foreach (KeyValuePair<TKey, TValue> kvp in data)
		{
			Assert.True(dict.TryAdd(kvp.Key, kvp.Value));
		}

		// enumerate the key/value pairs in the dictionary
		var enumerable = (IEnumerable<KeyValuePair<TKey, TValue>>)dict;
		List<KeyValuePair<TKey, TValue>> enumerated = enumerable.ToList();

		// compare collection elements with the expected key/value pairs
		Assert.Equal(
			data.OrderBy(x => x.Key, KeyComparer),
			enumerated.OrderBy(x => x.Key, KeyComparer),
			KeyValuePairEqualityComparer);
	}

	/// <summary>
	/// Tests whether the <see cref="IGenericDictionary{TKey,TValue}.TryAdd(TKey,TValue)"/> method fails,
	/// if the key is already in the dictionary.
	/// </summary>
	/// <param name="count">Number of elements to add to the dictionary.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void IGenericDictionaryT_TryAdd_List_DuplicateKey(int count)
	{
		// get test data and create a new dictionary with it
		IDictionary<TKey, TValue> data = GetTestData(count);
		IGenericDictionary<TKey, TValue> dict = GetDictionary();

		// add data to the dictionary
		KeyValuePair<TKey, TValue>? first = null;
		KeyValuePair<TKey, TValue>? last = null;
		foreach (KeyValuePair<TKey, TValue> kvp in data)
		{
			first ??= kvp;
			last = kvp;
			Assert.True(dict.TryAdd(kvp.Key, kvp.Value));
		}

		// try to add the first and the last element once again
		if (first != null) Assert.False(dict.TryAdd(first.Value.Key, first.Value.Value));
		if (last != null) Assert.False(dict.TryAdd(last.Value.Key, last.Value.Value));

		// enumerate the key/value pairs in the dictionary
		var enumerable = (IEnumerable<KeyValuePair<TKey, TValue>>)dict;
		List<KeyValuePair<TKey, TValue>> enumerated = enumerable.ToList();

		// compare collection elements with the expected key/value pairs
		Assert.Equal(
			data.OrderBy(x => x.Key, KeyComparer),
			enumerated.OrderBy(x => x.Key, KeyComparer),
			KeyValuePairEqualityComparer);
	}

	/// <summary>
	/// Tests whether the <see cref="IGenericDictionary{TKey,TValue}.TryAdd(TKey,TValue)"/> method fails, if the key is <c>null</c>.
	/// For reference types only.
	/// </summary>
	[Fact]
	public void IGenericDictionaryT_TryAdd_List_KeyNull()
	{
		if (typeof(TKey).IsValueType) return;
		IGenericDictionary<TKey, TValue> dict = GetDictionary();
		// ReSharper disable once AssignNullToNotNullAttribute
		var exception = Assert.Throws<ArgumentNullException>(() => dict.Add(default, default));
		Assert.Equal("key", exception.ParamName); // the 'key' is actually not the name of the method parameter
	}

	#endregion
}
