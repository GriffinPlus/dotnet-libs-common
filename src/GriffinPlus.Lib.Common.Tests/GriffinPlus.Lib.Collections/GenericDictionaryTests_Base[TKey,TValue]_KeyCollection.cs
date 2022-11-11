///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace GriffinPlus.Lib.Collections
{

	public abstract partial class GenericDictionaryTests_Base<TKey, TValue>
	{
		#region KeyCollection # ICollection.Count

		/// <summary>
		/// Tests the <see cref="ICollection.Count"/> property of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void KeyCollection_ICollection_Count_Get(int count)
		{
			IDictionary<TKey, TValue> data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Keys;
			Assert.Equal(data.Count, collection.Count);
		}

		#endregion

		#region KeyCollection # ICollection.IsSynchronized

		/// <summary>
		/// Tests the <see cref="ICollection.IsSynchronized"/> property of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollection_IsSynchronized_Get()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Keys;
			Assert.False(collection.IsSynchronized);
		}

		#endregion

		#region KeyCollection # ICollection.SyncRoot

		/// <summary>
		/// Tests the <see cref="ICollection.SyncRoot"/> property of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollection_SyncRoot_Get()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Keys;

			// getting the sync root twice should return the same instance
			object sync1 = collection.SyncRoot;
			object sync2 = collection.SyncRoot;
			Assert.NotNull(sync1);
			Assert.Same(sync1, sync2);

			// the sync root should be the same as the sync root of the dictionary
			Assert.Same(((ICollection)dict).SyncRoot, sync1);
		}

		#endregion

		#region KeyCollection # ICollection.CopyTo(Array, int)

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection
		/// (with an array of the specific item type).
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData))]
		public void KeyCollection_ICollection_CopyTo_TypedArray(int count, int index)
		{
			// get test data and create a new dictionary with it
			IDictionary<TKey, TValue> data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Keys;

			// copy the collection into an array
			var destination = new TKey[count + index];
			collection.CopyTo(destination, index);

			// compare collection elements with the expected data set
			// (the order should be the same as returned by the dictionary enumerator) 
			Assert.Equal(
				dict.Select(x => x.Key),
				destination.Skip(index),
				KeyEqualityComparer);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection
		/// (with an array of <see cref="System.Object"/>).
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData))]
		public void KeyCollection_ICollection_CopyTo_ObjectArray(int count, int index)
		{
			// get test data and create a new dictionary with it
			IDictionary<TKey, TValue> data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Keys;

			// copy the dictionary into an array
			object[] destination = new object[count + index];
			collection.CopyTo(destination, index);

			// compare collection elements with the expected data set
			// (the order should be the same as returned by the dictionary enumerator) 
			Assert.Equal(
				dict.Select(x => x.Key),
				destination.Skip(index).Cast<TKey>(),
				KeyEqualityComparer);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection
		/// passing <c>null</c> for the destination array.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollection_CopyTo_ArrayNull()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Keys;
			// ReSharper disable once AssignNullToNotNullAttribute
			var exception = Assert.Throws<ArgumentNullException>(() => collection.CopyTo(null, 0));
			Assert.Equal("array", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection
		/// passing a multi-dimensional destination array.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollection_CopyTo_MultidimensionalArray()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Keys;
			var destination = new TKey[0, 0];
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, 0));
			Assert.Equal("array", exception.ParamName);
			Assert.StartsWith("The array is multidimensional.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection
		/// passing a one-dimensional destination array with a lower bound other than zero.
		/// </summary>
		[Theory]
		[InlineData(-1)]
		[InlineData(1)]
		public void KeyCollection_ICollection_CopyTo_InvalidLowerArrayBound(int lowerBound)
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Keys;
			var destination = Array.CreateInstance(typeof(TKey), new[] { 0 }, new[] { lowerBound });
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, 0));
			Assert.Equal("array", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection
		/// passing an incompatible array type.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollection_CopyTo_InvalidArrayType()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Keys;
			object destination = Array.Empty<int>();
			if (typeof(TKey) == typeof(int)) destination = Array.Empty<uint>();
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo((Array)destination, 0));
			Assert.Equal("array", exception.ParamName);
			Assert.StartsWith("Invalid array type.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection
		/// passing an array index that is out of range.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_IndexOutOfBounds))]
		public void KeyCollection_ICollection_CopyTo_IndexOutOfRange(int count, int index)
		{
			IDictionary<TKey, TValue> data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Keys;
			var destination = new TKey[count];
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(destination, index));
			Assert.Equal("index", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection
		/// passing an destination array that is too small to store all elements.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="arraySize">Size of the destination array.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_ArrayTooSmall))]
		public void KeyCollection_ICollection_CopyTo_ArrayTooSmall(int count, int arraySize, int index)
		{
			IDictionary<TKey, TValue> data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Keys;
			var destination = new TKey[arraySize];
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, index));
			Assert.StartsWith("The destination array is too small.", exception.Message);
		}

		#endregion

		#region KeyCollection # ICollection<T>.Count

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Count"/> property of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void KeyCollection_ICollectionT_Count_Get(int count)
		{
			IDictionary<TKey, TValue> data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TKey> collection = dict.Keys;
			Assert.Equal(data.Count, collection.Count);
		}

		#endregion

		#region KeyCollection # ICollection<T>.IsReadOnly

		/// <summary>
		/// Tests the <see cref="ICollection{T}.IsReadOnly"/> property of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollectionT_IsReadOnly()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			ICollection<TKey> collection = dict.Keys;
			Assert.True(collection.IsReadOnly);
		}

		#endregion

		#region KeyCollection # ICollection<T>.Add(T)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Add"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection.
		/// The implementation is expected to throw a <see cref="NotSupportedException"/>.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollectionT_Add_NotSupported()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			ICollection<TKey> collection = dict.Keys;
			Assert.Throws<NotSupportedException>(() => collection.Add(default));
		}

		#endregion

		#region KeyCollection # ICollection<T>.Clear()

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Add"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection.
		/// The implementation is expected to throw a <see cref="NotSupportedException"/>.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollectionT_Clear_NotSupported()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			ICollection<TKey> collection = dict.Keys;
			Assert.Throws<NotSupportedException>(() => collection.Clear());
		}

		#endregion

		#region KeyCollection # ICollection<T>.Contains(T)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Contains"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection.
		/// The key is in the collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void KeyCollection_ICollectionT_Contains(int count)
		{
			// get test data and create a new dictionary with it
			IDictionary<TKey, TValue> data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TKey> collection = dict.Keys;

			// test whether keys of test data are reported to be in the collection
			foreach (KeyValuePair<TKey, TValue> kvp in data)
			{
				bool contains = collection.Contains(kvp.Key);
				Assert.True(contains);
			}
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Contains"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection.
		/// The key is not in the collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes_WithoutZero))]
		public void KeyCollection_ICollectionT_Contains_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			IDictionary<TKey, TValue> data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TKey> collection = dict.Keys;

			// test whether some other key is reported to be not in the collection
			bool contains = collection.Contains(KeyNotInTestData);
			Assert.False(contains);
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Contains"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection.
		/// The method should return <c>false</c>, if the passed key is <c>null</c> (only for reference types).
		/// </summary>
		[Fact]
		public void KeyCollection_ICollectionT_Contains_KeyNull()
		{
			if (!typeof(TKey).IsValueType)
			{
				var dict = GetDictionary() as IDictionary<TKey, TValue>;
				ICollection<TKey> collection = dict.Keys;
				bool contains = collection.Contains(default);
				Assert.False(contains);
			}
		}

		#endregion

		#region KeyCollection # ICollection<T>.CopyTo(T[],int)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData))]
		public void KeyCollection_ICollectionT_CopyTo(int count, int index)
		{
			// get test data and create a new dictionary with it
			IDictionary<TKey, TValue> data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TKey> collection = dict.Keys;

			// copy the collection into an array
			var destination = new TKey[count + index];
			collection.CopyTo(destination, index);

			// compare collection elements with the expected keys
			// (the order should be the same as returned by the dictionary enumerator) 
			Assert.Equal(
				dict.Select(x => x.Key),
				destination.Skip(index),
				KeyEqualityComparer);
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection
		/// passing <c>null</c> for the destination array.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollectionT_CopyTo_ArrayNull()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			ICollection<TKey> collection = dict.Keys;
			// ReSharper disable once AssignNullToNotNullAttribute
			Assert.Throws<ArgumentNullException>(() => collection.CopyTo(null, 0));
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection
		/// passing an array index that is out of range.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_IndexOutOfBounds))]
		public void KeyCollection_ICollectionT_CopyTo_IndexOutOfRange(int count, int index)
		{
			IDictionary<TKey, TValue> data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TKey> collection = dict.Keys;
			var destination = new TKey[count];
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(destination, index));
			Assert.Equal("index", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection
		/// passing an destination array that is too small to store all.
		/// elements.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="arraySize">Size of the destination array.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_ArrayTooSmall))]
		public void KeyCollection_ICollectionT_CopyTo_ArrayTooSmall(int count, int arraySize, int index)
		{
			IDictionary<TKey, TValue> data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TKey> collection = dict.Keys;
			var destination = new TKey[arraySize];
			Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, index));
		}

		#endregion

		#region KeyCollection # ICollection<T>.Remove(T)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Remove"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection.
		/// The implementation is expected to throw a <see cref="NotSupportedException"/>.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollectionT_Remove_NotSupported()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			ICollection<TKey> collection = dict.Keys;
			Assert.Throws<NotSupportedException>(() => collection.Remove(default));
		}

		#endregion

		#region KeyCollection # IEnumerable.GetEnumerator() - incl. all enumerator functionality

		/// <summary>
		/// Tests the <see cref="IEnumerable.GetEnumerator"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void KeyCollection_IEnumerable_GetEnumerator(int count)
		{
			// get test data and create a new dictionary with it
			IDictionary<TKey, TValue> data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TKey> collection = dict.Keys;

			// get an enumerator
			var enumerable = (IEnumerable)collection;
			IEnumerator enumerator = enumerable.GetEnumerator();

			// the enumerator should point to the position before the first valid element,
			// but the 'Current' property should not throw an exception
			object _ = enumerator.Current;

			// enumerate the keys in the collection
			var enumerated = new List<TKey>();
			while (enumerator.MoveNext()) enumerated.Add((TKey)enumerator.Current);

			// the order of keys should be the same as returned by the dictionary enumerator
			Assert.Equal(
				dict.Select(x => x.Key),
				enumerated,
				KeyEqualityComparer);

			// the enumerator should point to the position after the last valid element now,
			// but the 'Current' property should not throw an exception
			// ReSharper disable once RedundantAssignment
			_ = enumerator.Current;

			// reset the enumerator and try again
			enumerator.Reset();
			enumerated = new List<TKey>();
			while (enumerator.MoveNext()) enumerated.Add((TKey)enumerator.Current);

			// the order of keys should be the same as returned by the dictionary enumerator
			Assert.Equal(
				dict.Select(x => x.Key),
				enumerated,
				KeyEqualityComparer);

			// modify the collection, the enumerator should recognize this
			dict[KeyNotInTestData] = ValueNotInTestData;
			Assert.Throws<InvalidOperationException>(() => enumerator.Reset());
			Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
		}

		#endregion

		#region KeyCollection # IEnumerable<T>.GetEnumerator() - incl. all enumerator functionality

		/// <summary>
		/// Tests the <see cref="IEnumerable{T}.GetEnumerator"/> method of the <see cref="IDictionary{TKey,TValue}.Keys"/> collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void KeyCollection_IEnumerableT_GetEnumerator(int count)
		{
			// get test data and create a new dictionary with it
			IDictionary<TKey, TValue> data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TKey> collection = dict.Keys;

			// get an enumerator
			IEnumerator<TKey> enumerator = collection.GetEnumerator();

			// the enumerator should point to the position before the first valid element,
			// but the 'Current' property should not throw an exception
			TKey _ = enumerator.Current;

			// enumerate the keys in the collection
			var enumerated = new List<TKey>();
			while (enumerator.MoveNext()) enumerated.Add(enumerator.Current);

			// the order of keys should be the same as returned by the dictionary enumerator
			Assert.Equal(
				dict.Select(x => x.Key),
				enumerated,
				KeyEqualityComparer);

			// the enumerator should point to the position after the last valid element now,
			// but the 'Current' property should not throw an exception
			// ReSharper disable once RedundantAssignment
			_ = enumerator.Current;

			// reset the enumerator and try again
			enumerator.Reset();
			enumerated = new List<TKey>();
			while (enumerator.MoveNext()) enumerated.Add(enumerator.Current);

			// the order of keys should be the same as returned by the dictionary enumerator
			Assert.Equal(
				dict.Select(x => x.Key),
				enumerated,
				KeyEqualityComparer);

			// modify the collection, the enumerator should recognize this
			dict[KeyNotInTestData] = ValueNotInTestData;
			Assert.Throws<InvalidOperationException>(() => enumerator.Reset());
			Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());

			// dispose the enumerator
			enumerator.Dispose();
		}

		#endregion
	}

}
