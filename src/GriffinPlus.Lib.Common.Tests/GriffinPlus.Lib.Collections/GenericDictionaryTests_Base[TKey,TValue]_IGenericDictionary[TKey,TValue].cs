///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace GriffinPlus.Lib.Collections
{

	public abstract partial class GenericDictionaryTests_Base<TKey, TValue>
	{
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
			var data = GetTestData(count);
			var dict = GetDictionary();

			// add data to the dictionary
			foreach (var kvp in data)
			{
				Assert.True(dict.TryAdd(kvp.Key, kvp.Value));
			}

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<TKey, TValue>>();
			var enumerable = (IEnumerable<KeyValuePair<TKey, TValue>>)dict;
			foreach (var kvp in enumerable) enumerated.Add(kvp);

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
			var data = GetTestData(count);
			var dict = GetDictionary();

			// add data to the dictionary
			KeyValuePair<TKey, TValue>? first = null;
			KeyValuePair<TKey, TValue>? last = null;
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
			var enumerated = new List<KeyValuePair<TKey, TValue>>();
			var enumerable = (IEnumerable<KeyValuePair<TKey, TValue>>)dict;
			foreach (var kvp in enumerable) enumerated.Add(kvp);

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
			if (!typeof(TKey).IsValueType)
			{
				var dict = GetDictionary();
				// ReSharper disable once AssignNullToNotNullAttribute
				var exception = Assert.Throws<ArgumentNullException>(() => dict.Add(default, default));
				Assert.Equal("key", exception.ParamName); // the 'key' is actually not the name of the method parameter
			}
		}

		#endregion
	}

}
