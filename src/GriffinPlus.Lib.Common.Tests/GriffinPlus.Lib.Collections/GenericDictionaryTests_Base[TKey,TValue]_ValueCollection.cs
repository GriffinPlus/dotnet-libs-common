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
		#region ValueCollection # ICollection.Count

		/// <summary>
		/// Tests the <see cref="ICollection.Count"/> property of the <see cref="IDictionary{TKey,TValue}.Values"/> collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ValueCollection_ICollection_Count_Get(int count)
		{
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Values;
			Assert.Equal(data.Count, collection.Count);
		}

		#endregion

		#region ValueCollection # ICollection.IsSynchronized

		/// <summary>
		/// Tests the <see cref="ICollection.IsSynchronized"/> property of the <see cref="IDictionary{TKey,TValue}.Values"/> collection.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollection_IsSynchronized_Get()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Values;
			Assert.False(collection.IsSynchronized);
		}

		#endregion

		#region ValueCollection # ICollection.SyncRoot

		/// <summary>
		/// Tests the <see cref="ICollection.SyncRoot"/> property of the <see cref="IDictionary{TKey,TValue}.Values"/> collection.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollection_SyncRoot_Get()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Values;

			// getting the sync root twice should return the same instance
			object sync1 = collection.SyncRoot;
			object sync2 = collection.SyncRoot;
			Assert.NotNull(sync1);
			Assert.Same(sync1, sync2);

			// the sync root should be the same as the sync root of the dictionary
			Assert.Same(((ICollection)dict).SyncRoot, sync1);
		}

		#endregion

		#region ValueCollection # ICollection.CopyTo(Array, int)

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection
		/// (with an array of the specific item type).
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData))]
		public void ValueCollection_ICollection_CopyTo_TypedArray(int count, int index)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Values;

			// copy the collection into an array
			var destination = new TValue[count + index];
			collection.CopyTo(destination, index);

			// compare collection elements with the expected data set
			// (the order should be the same as returned by the dictionary enumerator) 
			Assert.Equal(
				dict.Select(x => x.Value),
				destination.Skip(index),
				ValueEqualityComparer);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection
		/// (with an array of <see cref="System.Object"/>).
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData))]
		public void ValueCollection_ICollection_CopyTo_ObjectArray(int count, int index)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Values;

			// copy the dictionary into an array
			object[] destination = new object[count + index];
			collection.CopyTo(destination, index);

			// compare collection elements with the expected data set
			// (the order should be the same as returned by the dictionary enumerator) 
			Assert.Equal(
				dict.Select(x => x.Value),
				destination.Skip(index).Cast<TValue>(),
				ValueEqualityComparer);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection
		/// passing <c>null</c> for the destination array.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollection_CopyTo_ArrayNull()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Values;
			// ReSharper disable once AssignNullToNotNullAttribute
			var exception = Assert.Throws<ArgumentNullException>(() => collection.CopyTo(null, 0));
			Assert.Equal("array", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection
		/// passing a multi-dimensional destination array.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollection_CopyTo_MultidimensionalArray()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Values;
			var destination = new TValue[0, 0];
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, 0));
			Assert.Equal("array", exception.ParamName);
			Assert.StartsWith("The array is multidimensional.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection
		/// passing a one-dimensional destination array with a lower bound other than zero.
		/// </summary>
		[Theory]
		[InlineData(-1)]
		[InlineData(1)]
		public void ValueCollection_ICollection_CopyTo_InvalidLowerArrayBound(int lowerBound)
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Values;
			var destination = Array.CreateInstance(typeof(TValue), new[] { 0 }, new[] { lowerBound });
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, 0));
			Assert.Equal("array", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection
		/// passing an incompatible array type.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollection_CopyTo_InvalidArrayType()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Values;
			object destination = Array.Empty<int>();
			if (typeof(TValue) == typeof(int)) destination = Array.Empty<uint>();
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo((Array)destination, 0));
			Assert.Equal("array", exception.ParamName);
			Assert.StartsWith("Invalid array type.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection
		/// passing an array index that is out of range.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_IndexOutOfBounds))]
		public void ValueCollection_ICollection_CopyTo_IndexOutOfRange(int count, int index)
		{
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Values;
			var destination = new TValue[count];
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(destination, index));
			Assert.Equal("index", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection
		/// passing an destination array that is too small to store all elements.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="arraySize">Size of the destination array.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_ArrayTooSmall))]
		public void ValueCollection_ICollection_CopyTo_ArrayTooSmall(int count, int arraySize, int index)
		{
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			var collection = (ICollection)dict.Values;
			var destination = new TValue[arraySize];
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, index));
			Assert.StartsWith("The destination array is too small.", exception.Message);
		}

		#endregion

		#region ValueCollection # ICollection<T>.Count

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Count"/> property of the <see cref="IDictionary{TKey,TValue}.Values"/> collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ValueCollection_ICollectionT_Count_Get(int count)
		{
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TValue> collection = dict.Values;
			Assert.Equal(data.Count, collection.Count);
		}

		#endregion

		#region ValueCollection # ICollection<T>.IsReadOnly

		/// <summary>
		/// Tests the <see cref="ICollection{T}.IsReadOnly"/> property of the <see cref="IDictionary{TKey,TValue}.Values"/> collection.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollectionT_IsReadOnly()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			ICollection<TValue> collection = dict.Values;
			Assert.True(collection.IsReadOnly);
		}

		#endregion

		#region ValueCollection # ICollection<T>.Add(T)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Add"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection.
		/// The implementation is expected to throw a <see cref="NotSupportedException"/>.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollectionT_Add_NotSupported()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			ICollection<TValue> collection = dict.Values;
			Assert.Throws<NotSupportedException>(() => collection.Add(default));
		}

		#endregion

		#region ValueCollection # ICollection<T>.Clear()

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Add"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection.
		/// The implementation is expected to throw a <see cref="NotSupportedException"/>.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollectionT_Clear_NotSupported()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			ICollection<TValue> collection = dict.Values;
			Assert.Throws<NotSupportedException>(() => collection.Clear());
		}

		#endregion

		#region ValueCollection # ICollection<T>.Contains(T)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Contains"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection.
		/// The value is in the collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ValueCollection_ICollectionT_Contains(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TValue> collection = dict.Values;

			// test whether values of test data are reported to be in the collection
			foreach (var kvp in data)
			{
				bool contains = collection.Contains(kvp.Value);
				Assert.True(contains);
			}
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Contains"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection.
		/// The value is not in the collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes_WithoutZero))]
		public void ValueCollection_ICollectionT_Contains_ValueNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TValue> collection = dict.Values;

			// test whether some other value is reported to be not in the collection
			bool contains = collection.Contains(ValueNotInTestData);
			Assert.False(contains);
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Contains"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection.
		/// The method should return <c>false</c>, if the passed value is <c>null</c> (only for reference types).
		/// </summary>
		[Fact]
		public void ValueCollection_ICollectionT_Contains_ValueNull()
		{
			if (!typeof(TValue).IsValueType)
			{
				var dict = GetDictionary() as IDictionary<TKey, TValue>;
				ICollection<TValue> collection = dict.Values;
				bool contains = collection.Contains(default);
				Assert.False(contains);
			}
		}

		#endregion

		#region ValueCollection # ICollection<T>.CopyTo(T[],int)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData))]
		public void ValueCollection_ICollectionT_CopyTo(int count, int index)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TValue> collection = dict.Values;

			// copy the collection into an array
			var destination = new TValue[count + index];
			collection.CopyTo(destination, index);

			// compare collection elements with the expected keys
			// (the order should be the same as returned by the dictionary enumerator) 
			Assert.Equal(
				dict.Select(x => x.Value),
				destination.Skip(index),
				ValueEqualityComparer);
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection
		/// passing <c>null</c> for the destination array.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollectionT_CopyTo_ArrayNull()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			ICollection<TValue> collection = dict.Values;
			// ReSharper disable once AssignNullToNotNullAttribute
			Assert.Throws<ArgumentNullException>(() => collection.CopyTo(null, 0));
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection
		/// passing an array index that is out of range.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_IndexOutOfBounds))]
		public void ValueCollection_ICollectionT_CopyTo_IndexOutOfRange(int count, int index)
		{
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TValue> collection = dict.Values;
			var destination = new TValue[count];
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(destination, index));
			Assert.Equal("index", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.CopyTo"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection
		/// passing an destination array that is too small to store all.
		/// elements.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="arraySize">Size of the destination array.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_ArrayTooSmall))]
		public void ValueCollection_ICollectionT_CopyTo_ArrayTooSmall(int count, int arraySize, int index)
		{
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TValue> collection = dict.Values;
			var destination = new TValue[arraySize];
			Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, index));
		}

		#endregion

		#region ValueCollection # ICollection<T>.Remove(T)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Remove"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection.
		/// The implementation is expected to throw a <see cref="NotSupportedException"/>.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollectionT_Remove_NotSupported()
		{
			var dict = GetDictionary() as IDictionary<TKey, TValue>;
			ICollection<TValue> collection = dict.Values;
			Assert.Throws<NotSupportedException>(() => collection.Remove(default));
		}

		#endregion

		#region ValueCollection # IEnumerable.GetEnumerator() - incl. all enumerator functionality

		/// <summary>
		/// Tests the <see cref="IEnumerable.GetEnumerator"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ValueCollection_IEnumerable_GetEnumerator(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TValue> collection = dict.Values;

			// get an enumerator
			var enumerable = (IEnumerable)collection;
			var enumerator = enumerable.GetEnumerator();

			// the enumerator should point to the position before the first valid element,
			// but the 'Current' property should not throw an exception
			object _ = enumerator.Current;

			// enumerate the values in the collection
			var enumerated = new List<TValue>();
			while (enumerator.MoveNext()) enumerated.Add((TValue)enumerator.Current);

			// the order of values should be the same as returned by the dictionary enumerator
			Assert.Equal(
				dict.Select(x => x.Value),
				enumerated,
				ValueEqualityComparer);

			// the enumerator should point to the position after the last valid element now,
			// but the 'Current' property should not throw an exception
			// ReSharper disable once RedundantAssignment
			_ = enumerator.Current;

			// reset the enumerator and try again
			enumerator.Reset();
			enumerated = new List<TValue>();
			while (enumerator.MoveNext()) enumerated.Add((TValue)enumerator.Current);

			// the order of values should be the same as returned by the dictionary enumerator
			Assert.Equal(
				dict.Select(x => x.Value),
				enumerated,
				ValueEqualityComparer);

			// modify the collection, the enumerator should recognize this
			dict[KeyNotInTestData] = ValueNotInTestData;
			Assert.Throws<InvalidOperationException>(() => enumerator.Reset());
			Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
		}

		#endregion

		#region ValueCollection # IEnumerable<T>.GetEnumerator() - incl. all enumerator functionality

		/// <summary>
		/// Tests the <see cref="IEnumerable{T}.GetEnumerator"/> method of the <see cref="IDictionary{TKey,TValue}.Values"/> collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ValueCollection_IEnumerableT_GetEnumerator(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;
			ICollection<TValue> collection = dict.Values;

			// get an enumerator
			var enumerator = collection.GetEnumerator();

			// the enumerator should point to the position before the first valid element,
			// but the 'Current' property should not throw an exception
			var _ = enumerator.Current;

			// enumerate the values in the collection
			var enumerated = new List<TValue>();
			while (enumerator.MoveNext()) enumerated.Add(enumerator.Current);

			// the order of values should be the same as returned by the dictionary enumerator
			Assert.Equal(
				dict.Select(x => x.Value),
				enumerated,
				ValueEqualityComparer);

			// the enumerator should point to the position after the last valid element now,
			// but the 'Current' property should not throw an exception
			// ReSharper disable once RedundantAssignment
			_ = enumerator.Current;

			// reset the enumerator and try again
			enumerator.Reset();
			enumerated = new List<TValue>();
			while (enumerator.MoveNext()) enumerated.Add(enumerator.Current);

			// the order of values should be the same as returned by the dictionary enumerator
			Assert.Equal(
				dict.Select(x => x.Value),
				enumerated,
				ValueEqualityComparer);

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
