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
		#region IReadOnlyDictionary<TKey,TValue>.this[TKey]

		/// <summary>
		/// Tests accessing the key collection via <see cref="IReadOnlyDictionary{TKey,TValue}.this[TKey]"/>.
		/// The key of the element is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IReadOnlyDictionaryT_Indexer_Get_List(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IReadOnlyDictionary<TKey, TValue>;

			// test whether keys of test data are reported to be in the dictionary
			foreach (var kvp in data)
			{
				Assert.Equal(kvp.Value, dict[kvp.Key]);
			}
		}

		/// <summary>
		/// Tests accessing the key collection via <see cref="IReadOnlyDictionary{TKey,TValue}.this[TKey]"/>.
		/// The key of the element is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IReadOnlyDictionaryT_Indexer_Get_List_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IReadOnlyDictionary<TKey, TValue>;

			// test whether some other key is reported to be not in the dictionary
			Assert.Throws<KeyNotFoundException>(() => dict[KeyNotInTestData]);
		}

		/// <summary>
		/// Tests whether <see cref="IReadOnlyDictionary{TKey,TValue}.this[TKey]"/> fails, if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void IReadOnlyDictionaryT_Indexer_Get_List_KeyNull()
		{
			var dict = GetDictionary() as IReadOnlyDictionary<TKey, TValue>;
			var exception = Assert.Throws<ArgumentNullException>(() => dict[default]);
			Assert.Equal("key", exception.ParamName);
		}

		#endregion

		#region IReadOnlyDictionary<TKey,TValue>.Keys

		/// <summary>
		/// Tests accessing the key collection via <see cref="IReadOnlyDictionary{TKey,TValue}.Keys"/>.
		/// The key collection should present all keys in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IReadOnlyDictionaryT_Keys(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IReadOnlyDictionary<TKey, TValue>;

			// enumerate the keys in the dictionary
			var enumerated = dict.Keys.ToList();

			// compare collection elements with the expected values
			Assert.Equal(
				data.Select(x => x.Key).OrderBy(x => x, KeyComparer).ToArray(),
				enumerated.OrderBy(x => x, KeyComparer).ToArray(),
				KeyEqualityComparer);
		}

		#endregion

		#region IReadOnlyDictionary<TKey,TValue>.Values

		/// <summary>
		/// Tests accessing the value collection via <see cref="IReadOnlyDictionary{TKey,TValue}.Values"/>.
		/// The value collection should present all values in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IReadOnlyDictionaryT_Values(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IReadOnlyDictionary<TKey, TValue>;

			// enumerate the values in the dictionary
			var enumerated = dict.Values.ToList();

			// compare collection elements with the expected values
			Assert.Equal(
				data.Select(x => x.Value).OrderBy(x => x, ValueComparer).ToArray(),
				enumerated.OrderBy(x => x, ValueComparer).ToArray(),
				ValueEqualityComparer);
		}

		#endregion

		#region IReadOnlyDictionary<TKey,TValue>.ContainsKey(TKey)

		/// <summary>
		/// Tests the <see cref="IReadOnlyDictionary{TKey,TValue}.ContainsKey(TKey)"/> method.
		/// The key of the element is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IReadOnlyDictionaryT_ContainsKey_List(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IReadOnlyDictionary<TKey, TValue>;

			// test whether keys of test data are reported to be in the dictionary
			foreach (var kvp in data)
			{
				Assert.True(dict.ContainsKey(kvp.Key));
			}
		}

		/// <summary>
		/// Tests the <see cref="IReadOnlyDictionary{TKey,TValue}.ContainsKey(TKey)"/> method.
		/// The key of the element is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IReadOnlyDictionaryT_ContainsKey_List_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IReadOnlyDictionary<TKey, TValue>;

			// test whether some other key is reported to be not in the dictionary
			Assert.False(dict.ContainsKey(KeyNotInTestData));
		}

		/// <summary>
		/// Tests whether the <see cref="IReadOnlyDictionary{TKey,TValue}.ContainsKey(TKey)"/> method fails,
		/// if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void IReadOnlyDictionaryT_ContainsKey_List_KeyNull()
		{
			var dict = GetDictionary() as IReadOnlyDictionary<TKey, TValue>;
			var exception = Assert.Throws<ArgumentNullException>(() => dict.ContainsKey(default));
			Assert.Equal("key", exception.ParamName);
		}

		#endregion

		#region IReadOnlyDictionary<TKey,TValue>.TryGetValue(TKey, out TValue)

		/// <summary>
		/// Tests the <see cref="IReadOnlyDictionary{TKey,TValue}.TryGetValue(TKey,out TValue)"/> method.
		/// The key of the element to get is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IReadOnlyDictionaryT_TryGetValue_List(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IReadOnlyDictionary<TKey, TValue>;

			// test whether keys of test data are reported to be in the dictionary
			foreach (var kvp in data)
			{
				Assert.True(dict.TryGetValue(kvp.Key, out var value));
				Assert.Equal(kvp.Value, value);
			}
		}

		/// <summary>
		/// Tests the <see cref="IReadOnlyDictionary{TKey,TValue}.TryGetValue(TKey,out TValue)"/> method.
		/// The key of the element to get is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IReadOnlyDictionaryT_TryGetValue_List_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IReadOnlyDictionary<TKey, TValue>;

			// test whether some other key is reported to be not in the dictionary
			Assert.False(dict.TryGetValue(KeyNotInTestData, out _));
		}

		/// <summary>
		/// Tests whether the <see cref="IReadOnlyDictionary{TKey,TValue}.TryGetValue(TKey,out TValue)"/> method fails, if the passed key is <c>null</c>.
		/// For reference types only.
		/// </summary>
		[Fact]
		public void IReadOnlyDictionary_TryGetValue_List_KeyNull()
		{
			if (!typeof(TKey).IsValueType)
			{
				var dict = GetDictionary() as IReadOnlyDictionary<TKey, TValue>;
				// ReSharper disable once AssignNullToNotNullAttribute
				var exception = Assert.Throws<ArgumentNullException>(() => dict.TryGetValue(default, out _));
				Assert.Equal("key", exception.ParamName);
			}
		}

		#endregion
	}

}
