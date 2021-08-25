///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Xunit;

// ReSharper disable AssignNullToNotNullAttribute

namespace GriffinPlus.Lib.Collections
{

	public abstract partial class ByteSequenceKeyedDictionaryTests_Base<TValue>
	{
		#region IDictionary.IsFixedSize

		/// <summary>
		/// Tests getting the <see cref="IDictionary.IsFixedSize"/> property.
		/// </summary>
		[Fact]
		public void IDictionary_IsFixedSize_Get()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;
			Assert.False(dict.IsFixedSize);
		}

		#endregion

		#region IDictionary.IsReadOnly

		/// <summary>
		/// Tests getting the <see cref="IDictionary.IsReadOnly"/> property.
		/// </summary>
		[Fact]
		public void IDictionary_IsReadOnly_Get()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;
			Assert.False(dict.IsReadOnly);
		}

		#endregion

		#region IDictionary.Keys

		/// <summary>
		/// Tests accessing the key collection via <see cref="IDictionary.Keys"/>.
		/// The key collection should present all keys in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IDictionary_Keys(int count)
		{
			// get test data and create a new dictionary with it
			var expected = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(expected) as IDictionary;

			// enumerate the keys in the dictionary
			var enumerated = new List<IReadOnlyList<byte>>();
			foreach (object current in dict.Keys)
			{
				Assert.IsAssignableFrom<IReadOnlyList<byte>>(current);
				enumerated.Add((IReadOnlyList<byte>)current);
			}

			// compare collection elements with the expected values
			Assert.Equal(
				expected.Select(x => x.Key).OrderBy(x => x, ReadOnlyListComparer<byte>.Instance).ToArray(),
				enumerated.OrderBy(x => x, ReadOnlyListComparer<byte>.Instance).ToArray(),
				ReadOnlyListEqualityComparer<byte>.Instance);
		}

		#endregion

		#region IDictionary.Values

		/// <summary>
		/// Tests accessing the value collection via <see cref="IDictionary.Values"/>.
		/// The value collection should present all values in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IDictionary_Values(int count)
		{
			// get test data and create a new dictionary with it
			var expected = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(expected) as IDictionary;

			// enumerate the keys in the dictionary
			var enumerated = new List<TValue>();
			foreach (object current in dict.Values)
			{
				Assert.IsAssignableFrom<TValue>(current);
				enumerated.Add((TValue)current);
			}

			// compare collection elements with the expected values
			Assert.Equal(
				expected.Select(x => x.Value).OrderBy(x => x, Comparer<TValue>.Default).ToArray(),
				enumerated.OrderBy(x => x, Comparer<TValue>.Default).ToArray(),
				EqualityComparer<TValue>.Default);
		}

		#endregion

		#region IDictionary.this[object]

		/// <summary>
		/// Tests accessing the key collection via <see cref="IDictionary.this"/>.
		/// The key of the element is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IDictionary_Indexer_Get(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as IDictionary;

			// test whether keys of test data are reported to be in the dictionary
			foreach (var kvp in data)
			{
				Assert.Equal(kvp.Value, dict[kvp.Key]);
			}
		}

		/// <summary>
		/// Tests accessing the key collection via <see cref="IDictionary.this"/>.
		/// The key of the element is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IDictionary_Indexer_Get_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as IDictionary;

			// test whether some other key is reported to be not in the dictionary
			Assert.Null(dict[KeyNotInTestData]);
		}

		/// <summary>
		/// Tests whether <see cref="IDictionary.this"/> fails, if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void IDictionary_Indexer_Get_KeyNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;
			var exception = Assert.Throws<ArgumentNullException>(() => dict[null]);
			Assert.Equal("key", exception.ParamName);
		}

		/// <summary>
		/// Tests accessing the key collection via <see cref="IDictionary.this"/>.
		/// The dictionary does not contain an item with the specified key, so the item is added.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IDictionary_Indexer_Set_NewItem(int count)
		{
			// get test data and create an empty dictionary
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;

			// add data to the dictionary
			foreach (var kvp in data)
			{
				dict[kvp.Key] = kvp.Value;
			}

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (object current in dict)
			{
				Assert.IsType<DictionaryEntry>(current);
				var kvp = (DictionaryEntry)current;
				Assert.IsAssignableFrom<IReadOnlyList<byte>>(kvp.Key);
				Assert.IsAssignableFrom<TValue>(kvp.Value);
				enumerated.Add(new KeyValuePair<IReadOnlyList<byte>, TValue>((IReadOnlyList<byte>)kvp.Key, (TValue)kvp.Value));
			}

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests accessing the key collection via <see cref="IDictionary.this"/>.
		/// The dictionary contains an item with the specified key, so the item is overwritten.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes_WithoutZero))]
		public void IDictionary_Indexer_Set_OverwriteItem(int count)
		{
			// get test data and create an empty dictionary
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;

			// add data to the dictionary
			foreach (var kvp in data)
			{
				dict[kvp.Key] = kvp.Value;
			}

			// overwrite an item
			byte[] key = data.First().Key.ToArray();
			data[key] = ValueNotInTestData;
			dict[key] = ValueNotInTestData;

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (object current in dict)
			{
				Assert.IsType<DictionaryEntry>(current);
				var kvp = (DictionaryEntry)current;
				Assert.IsAssignableFrom<IReadOnlyList<byte>>(kvp.Key);
				Assert.IsAssignableFrom<TValue>(kvp.Value);
				enumerated.Add(new KeyValuePair<IReadOnlyList<byte>, TValue>((IReadOnlyList<byte>)kvp.Key, (TValue)kvp.Value));
			}

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests whether <see cref="IDictionary.this"/> fails, if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void IDictionary_Indexer_Set_KeyNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;
			var exception = Assert.Throws<ArgumentNullException>(() => dict[null] = default(TValue));
			Assert.Equal("key", exception.ParamName);
		}

		/// <summary>
		/// Tests whether <see cref="IDictionary.this"/> fails, if the passed key is not of the expected type.
		/// </summary>
		[Fact]
		public void IDictionary_Indexer_Set_InvalidKeyType()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;
			int key = 0; // should be IReadOnlyList<byte>
			var exception = Assert.Throws<ArgumentException>(() => dict[key] = default(TValue));
			Assert.Equal("key", exception.ParamName);
			Assert.StartsWith("Wrong key type", exception.Message);
		}

		/// <summary>
		/// Tests whether <see cref="IDictionary.this"/> fails, if the passed value is not of the expected type.
		/// </summary>
		[Fact]
		public void IDictionary_Indexer_Set_InvalidValueType()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;
			byte[] key = new byte[0];
			float value = 0.0f; // float is not a value type used to test the dictionary
			var exception = Assert.Throws<ArgumentException>(() => dict[key] = value);
			Assert.Equal("value", exception.ParamName);
			Assert.StartsWith("Wrong value type", exception.Message);
		}

		/// <summary>
		/// Tests whether <see cref="IDictionary.this"/> fails, if the passed value is <c>null</c> and
		/// <see cref="TValue"/> is a value type.
		/// </summary>
		[Fact]
		public void IDictionary_Indexer_Set_ValueNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;
			byte[] key = new byte[0];
			if (typeof(TValue).IsValueType)
			{
				// TValue is a value type, null value is not allowed
				var exception = Assert.Throws<ArgumentNullException>(() => dict[key] = null);
				Assert.Equal("value", exception.ParamName);
			}
			else
			{
				// TValue is a reference type, null value is allowed
				dict[key] = null;
				Assert.Null(dict[key]);
			}
		}

		#endregion

		#region IDictionary.Add(object, object)

		/// <summary>
		/// Tests the <see cref="IDictionary.Add"/> method.
		/// </summary>
		/// <param name="count">Number of elements to add to the dictionary.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IDictionary_Add(int count)
		{
			// get test data and create an empty dictionary
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;

			// add data to the dictionary
			foreach (var kvp in data)
			{
				dict.Add(kvp.Key, kvp.Value);
			}

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			foreach (object current in dict)
			{
				Assert.IsType<DictionaryEntry>(current);
				var kvp = (DictionaryEntry)current;
				Assert.IsAssignableFrom<IReadOnlyList<byte>>(kvp.Key);
				Assert.IsAssignableFrom<TValue>(kvp.Value);
				enumerated.Add(new KeyValuePair<IReadOnlyList<byte>, TValue>((IReadOnlyList<byte>)kvp.Key, (TValue)kvp.Value));
			}

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests whether the <see cref="IDictionary.Add"/> method fails, if the key of the key/value pair is already in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to add to the dictionary.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IDictionary_Add_DuplicateKey(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;

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
			foreach (DictionaryEntry kvp in dict) enumerated.Add(new KeyValuePair<IReadOnlyList<byte>, TValue>((IReadOnlyList<byte>)kvp.Key, (TValue)kvp.Value));

			// compare collection elements with the expected key/value pairs
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests whether the <see cref="IDictionary.Add"/> method fails, if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void IDictionary_Add_KeyNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;
			var exception = Assert.Throws<ArgumentNullException>(() => dict.Add(null, default(TValue)));
			Assert.Equal("key", exception.ParamName); // the 'key' is actually not the name of the method parameter
		}

		/// <summary>
		/// Tests whether <see cref="IDictionary.Add"/> fails, if the passed key is not of the expected type.
		/// </summary>
		[Fact]
		public void IDictionary_Add_InvalidKeyType()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;
			int key = 0; // should be IReadOnlyList<byte>
			var exception = Assert.Throws<ArgumentException>(() => dict.Add(key, default(TValue)));
			Assert.Equal("key", exception.ParamName);
			Assert.StartsWith("Wrong key type", exception.Message);
		}

		/// <summary>
		/// Tests whether <see cref="IDictionary.Add"/> fails, if the passed value is not of the expected type.
		/// </summary>
		[Fact]
		public void IDictionary_Add_InvalidValueType()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;
			byte[] key = new byte[0];
			float value = 0.0f; // float is not a value type used to test the dictionary
			var exception = Assert.Throws<ArgumentException>(() => dict.Add(key, value));
			Assert.Equal("value", exception.ParamName);
			Assert.StartsWith("Wrong value type", exception.Message);
		}

		/// <summary>
		/// Tests whether <see cref="IDictionary.Add"/> fails, if the passed value is <c>null</c> and
		/// <see cref="TValue"/> is a value type.
		/// </summary>
		[Fact]
		public void IDictionary_Add_ValueNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;
			byte[] key = new byte[0];
			if (typeof(TValue).IsValueType)
			{
				// TValue is a value type, null value is not allowed
				var exception = Assert.Throws<ArgumentNullException>(() => dict.Add(key, null));
				Assert.Equal("value", exception.ParamName);
			}
			else
			{
				// TValue is a reference type, null value is allowed
				dict.Add(key, null);
				Assert.Null(dict[key]);
			}
		}

		#endregion

		#region IDictionary.Contains(object)

		/// <summary>
		/// Tests the <see cref="IDictionary.Contains"/> method.
		/// The key of the element is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IDictionary_Contains(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as IDictionary;

			// test whether keys of test data are reported to be in the dictionary
			foreach (var kvp in data)
			{
				Assert.True(dict.Contains(kvp.Key));
			}
		}

		/// <summary>
		/// Tests the <see cref="IDictionary.Contains"/> method.
		/// The key of the element is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IDictionary_Contains_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as IDictionary;

			// test whether some other key is reported to be not in the dictionary
			Assert.False(dict.Contains(KeyNotInTestData));
		}

		/// <summary>
		/// Tests the <see cref="IDictionary.Contains"/> method.
		/// The key is not an <see cref="IReadOnlyList{T}"/> of <see cref="System.Byte"/>.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IDictionary_Contains_IncompatibleKey(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as IDictionary;

			// test whether some other key is reported to be not in the dictionary
			int[] key = { 0xff }; // int[] (IReadOnlyList<int>) != byte[] (IReadOnlyList<byte>)
			Assert.False(dict.Contains(key));
		}

		/// <summary>
		/// Tests whether the <see cref="IDictionary.Contains"/> method fails, if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void IDictionary_Contains_KeyNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;
			var exception = Assert.Throws<ArgumentNullException>(() => dict.Contains(null));
			Assert.Equal("key", exception.ParamName);
		}

		#endregion

		#region IDictionary.GetEnumerator() - incl. all enumerator functionality

		/// <summary>
		/// Tests enumerating using <see cref="IDictionary.GetEnumerator"/>.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IDictionary_GetEnumerator(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as IDictionary;

			// get an enumerator
			var enumerator = dict.GetEnumerator();

			// the enumerator should point to the position before the first valid element
			Assert.Throws<InvalidOperationException>(() => enumerator.Entry);
			Assert.Throws<InvalidOperationException>(() => enumerator.Key);
			Assert.Throws<InvalidOperationException>(() => enumerator.Value);
			object _ = enumerator.Current; // should not throw

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			while (enumerator.MoveNext())
			{
				// IEnumerator
				Assert.IsType<DictionaryEntry>(enumerator.Current);
				var current = (DictionaryEntry)enumerator.Current;
				Assert.IsAssignableFrom<IReadOnlyList<byte>>(current.Key);
				Assert.IsAssignableFrom<TValue>(current.Value);

				// IDictionaryEnumerator
				Assert.Equal(current, enumerator.Entry);
				Assert.Equal(current.Key, enumerator.Key);
				Assert.Equal(current.Value, enumerator.Value);

				enumerated.Add(new KeyValuePair<IReadOnlyList<byte>, TValue>((IReadOnlyList<byte>)current.Key, (TValue)current.Value));
			}

			// compare collection elements with the expected values
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				enumerated.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);

			// the enumerator should point to the position after the last valid element now
			Assert.Throws<InvalidOperationException>(() => enumerator.Entry);
			Assert.Throws<InvalidOperationException>(() => enumerator.Key);
			Assert.Throws<InvalidOperationException>(() => enumerator.Value);
			_ = enumerator.Current; // should not throw

			// reset the enumerator and try again
			enumerator.Reset();
			enumerated = new List<KeyValuePair<IReadOnlyList<byte>, TValue>>();
			while (enumerator.MoveNext())
			{
				// IEnumerator
				Assert.IsType<DictionaryEntry>(enumerator.Current);
				var current = (DictionaryEntry)enumerator.Current;
				Assert.IsAssignableFrom<IReadOnlyList<byte>>(current.Key);
				Assert.IsAssignableFrom<TValue>(current.Value);

				// IDictionaryEnumerator
				Assert.Equal(current, enumerator.Entry);
				Assert.Equal(current.Key, enumerator.Key);
				Assert.Equal(current.Value, enumerator.Value);

				enumerated.Add(new KeyValuePair<IReadOnlyList<byte>, TValue>((IReadOnlyList<byte>)current.Key, (TValue)current.Value));
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

		#region IDictionary.Remove(object)

		/// <summary>
		/// Tests the <see cref="IDictionary.Remove"/> method.
		/// The key of the element to remove is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IDictionary_Remove(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as IDictionary;

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
		/// Tests the <see cref="IDictionary.Remove"/> method.
		/// The key of the element to remove is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IDictionary_Remove_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as IDictionary;

			// try to remove an element that does not exist
			dict.Remove(KeyNotInTestData);
			Assert.Equal(count, dict.Count);
		}

		/// <summary>
		/// Tests whether the <see cref="IDictionary.Remove"/> method fails, if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void IDictionary_Remove_KeyNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as IDictionary;
			var exception = Assert.Throws<ArgumentNullException>(() => dict.Remove(null));
			Assert.Equal("key", exception.ParamName); // the 'key' is actually not the name of the method parameter
		}

		#endregion
	}

}
