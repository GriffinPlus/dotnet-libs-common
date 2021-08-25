///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

// ReSharper disable AssignNullToNotNullAttribute

namespace GriffinPlus.Lib.Collections
{

	public abstract partial class ByteSequenceKeyedDictionaryTests_Base<TValue>
	{
		#region ICollection<T>.IsReadOnly

		/// <summary>
		/// Tests getting the <see cref="ICollection{T}.IsReadOnly"/> property.
		/// </summary>
		[Fact]
		public void ICollectionT_IsReadOnly_Get()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;
			Assert.False(dict.IsReadOnly);
		}

		#endregion

		#region ICollection<T>.Add(T)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Add"/> method.
		/// </summary>
		/// <param name="count">Number of elements to add to the dictionary.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ICollectionT_Add(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>() as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;

			// add data to the dictionary
			foreach (var kvp in data)
			{
				dict.Add(new KeyValuePair<IReadOnlyList<byte>, TValue>(kvp.Key, kvp.Value));
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
		/// Tests whether the <see cref="ICollection{T}.Add"/> method fails, if the key of the key/value pair is already in the dictionary.
		/// </summary>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ICollectionT_Add_DuplicateKey(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>() as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;

			// add data to the dictionary
			KeyValuePair<IReadOnlyList<byte>, TValue>? first = null;
			KeyValuePair<IReadOnlyList<byte>, TValue>? last = null;
			foreach (var kvp in data)
			{
				if (first == null) first = kvp;
				last = kvp;
				dict.Add(new KeyValuePair<IReadOnlyList<byte>, TValue>(kvp.Key, kvp.Value));
			}

			// try to add the first and the last element once again
			if (first != null) Assert.Throws<ArgumentException>(() => dict.Add(first.Value));
			if (last != null) Assert.Throws<ArgumentException>(() => dict.Add(last.Value));

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
		/// Tests whether the <see cref="ICollection{T}.Add"/> method fails, if the key of the passed key/value pair is <c>null</c>.
		/// </summary>
		[Fact]
		public void ICollectionT_Add_KeyNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;
			var exception = Assert.Throws<ArgumentNullException>(() => dict.Add(new KeyValuePair<IReadOnlyList<byte>, TValue>(null, default)));
			Assert.Equal("key", exception.ParamName); // the 'key' is actually not the name of the method parameter
		}

		#endregion

		#region ICollection<T>.Clear()

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Clear"/> method.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ICollectionT_Clear(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;

			// clear the dictionary
			dict.Clear();

			// the dictionary should be empty now
			Assert.Equal(0, dict.Count);
			Assert.Empty(dict);
		}

		#endregion

		#region ICollection<T>.Contains(T)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Contains"/> method.
		/// The element is in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ICollectionT_Contains(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;

			// test whether keys of test data are reported to be in the dictionary
			foreach (var kvp in data)
			{
				Assert.True(dict.Contains(new KeyValuePair<IReadOnlyList<byte>, TValue>(kvp.Key, kvp.Value)));
			}
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Contains"/> method.
		/// The key of the element is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes_WithoutZero))]
		public void ICollectionT_Contains_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;

			// test whether some other key is reported to be not in the dictionary
			Assert.False(
				dict.Contains(
					new KeyValuePair<IReadOnlyList<byte>, TValue>(
						KeyNotInTestData,
						data.First().Value)));
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Contains"/> method.
		/// The key of the element is in the dictionary, but the value does not match.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes_WithoutZero))]
		public void ICollectionT_Contains_ValueMismatch(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;

			// test whether some other key is reported to be not in the dictionary
			Assert.False(
				dict.Contains(
					new KeyValuePair<IReadOnlyList<byte>, TValue>(
						data.First().Key,
						ValueNotInTestData)));
		}

		/// <summary>
		/// Tests whether the <see cref="ICollection{T}.Contains"/> method fails, if the key of the passed key/value pair is <c>null</c>.
		/// </summary>
		[Fact]
		public void ICollectionT_Contains_KeyNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;
			var exception = Assert.Throws<ArgumentNullException>(() => dict.Contains(new KeyValuePair<IReadOnlyList<byte>, TValue>(null, default)));
			Assert.Equal("key", exception.ParamName); // the 'key' is actually not the name of the method parameter
		}

		#endregion

		#region ICollection<T>.CopyTo(T[], int)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.CopyTo"/> method.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData))]
		public void ICollectionT_CopyTo(int count, int index)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;

			// copy the dictionary into an array
			var destination = new KeyValuePair<IReadOnlyList<byte>, TValue>[count + index];
			dict.CopyTo(destination, index);

			// compare collection elements with the expected data set
			Assert.Equal(
				data.OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				destination.Skip(index).OrderBy(x => x.Key, ReadOnlyListComparer<byte>.Instance),
				sKeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.CopyTo"/> method passing <c>null</c> for the destination array.
		/// </summary>
		[Fact]
		public void ICollectionT_CopyTo_ArrayNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;
			Assert.Throws<ArgumentNullException>(() => dict.CopyTo(null, 0));
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.CopyTo"/> method passing an array index that is out of range.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_IndexOutOfBounds))]
		public void ICollectionT_CopyTo_IndexOutOfRange(int count, int index)
		{
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;
			var destination = new KeyValuePair<IReadOnlyList<byte>, TValue>[count];
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => dict.CopyTo(destination, index));
			Assert.Equal("index", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.CopyTo"/> method passing an destination array that is too small to store all elements.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="arraySize">Size of the destination array.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_ArrayTooSmall))]
		public void ICollectionT_CopyTo_ArrayTooSmall(int count, int arraySize, int index)
		{
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;
			var destination = new KeyValuePair<IReadOnlyList<byte>, TValue>[arraySize];
			Assert.Throws<ArgumentException>(() => dict.CopyTo(destination, index));
		}

		#endregion

		#region ICollection<T>.Remove(T)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Remove"/> method.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ICollectionT_Remove(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;

			// remove elements in random order until the dictionary is empty
			var random = new Random();
			var remainingData = data.ToList();
			while (remainingData.Count > 0)
			{
				int index = random.Next(0, remainingData.Count - 1);
				bool removed = dict.Remove(remainingData[index]);
				Assert.True(removed);
				remainingData.RemoveAt(index);
				Assert.Equal(remainingData.Count, dict.Count);
			}

			// the dictionary should be empty now
			Assert.Equal(0, dict.Count);
			Assert.Empty(dict);
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Remove"/> method.
		/// The key of the item to remove is not in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes_WithoutZero))]
		public void ICollectionT_Remove_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;

			// try to remove an element that does not exist
			var kvp = new KeyValuePair<IReadOnlyList<byte>, TValue>(KeyNotInTestData, data.First().Value);
			Assert.False(dict.Remove(kvp));
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Remove"/> method.
		/// The key of the item to remove is in the dictionary, but the value does not match.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes_WithoutZero))]
		public void ICollectionT_Remove_ValueMismatch(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;

			// try to remove an element that does not exist
			var kvp = new KeyValuePair<IReadOnlyList<byte>, TValue>(data.First().Key, ValueNotInTestData);
			Assert.False(dict.Remove(kvp));
		}

		/// <summary>
		/// Tests whether the <see cref="ICollection{T}.Remove"/> method fails, if the key of the passed key/value pair is <c>null</c>.
		/// </summary>
		[Fact]
		public void ICollectionT_Remove_KeyNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>() as ICollection<KeyValuePair<IReadOnlyList<byte>, TValue>>;
			var exception = Assert.Throws<ArgumentNullException>(() => dict.Remove(new KeyValuePair<IReadOnlyList<byte>, TValue>(null, default)));
			Assert.Equal("key", exception.ParamName); // the 'key' is actually not the name of the method parameter
		}

		#endregion
	}

}
