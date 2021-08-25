///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Xunit;

// ReSharper disable RedundantAssignment
// ReSharper disable AssignNullToNotNullAttribute

namespace GriffinPlus.Lib.Collections
{

	public abstract partial class ByteSequenceKeyedDictionaryTests_Base<TValue>
	{
		#region ValueCollection (Creation)

		/// <summary>
		/// Tests creating an instance of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ValueCollection_Create(int count)
		{
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict);
			Assert.Equal(count, collection.Count);
		}

		#endregion

		#region ValueCollection.CopyTo(TValue[],int)

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection.CopyTo"/> method.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData))]
		public void ValueCollection_CopyTo(int count, int index)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict);

			// copy the collection into an array
			var destination = new TValue[count + index];
			collection.CopyTo(destination, index);

			// compare collection elements with the expected values
			// (the order should be the same as returned by the dictionary enumerator) 
			Assert.Equal(
				dict.Select(x => x.Value),
				destination.Skip(index));
		}

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection.CopyTo"/> method passing <c>null</c> for the destination array.
		/// </summary>
		[Fact]
		public void ValueCollection_CopyTo_ArrayNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict);
			Assert.Throws<ArgumentNullException>(() => collection.CopyTo(null, 0));
		}

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection.CopyTo"/> method passing an array index that is out of range.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_IndexOutOfBounds))]
		public void ValueCollection_CopyTo_IndexOutOfRange(int count, int index)
		{
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict);
			var destination = new TValue[count];
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(destination, index));
			Assert.Equal("index", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection.CopyTo"/> method passing an destination array that is too small to store all
		/// elements.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="arraySize">Size of the destination array.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_ArrayTooSmall))]
		public void ValueCollection_CopyTo_ArrayTooSmall(int count, int arraySize, int index)
		{
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict);
			var destination = new TValue[arraySize];
			Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, index));
		}

		#endregion

		#region ValueCollection.GetEnumerator()

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection.GetEnumerator"/> method and all
		/// enumerator functionality along with it.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ValueCollection_GetEnumerator(int count)
		{
			// create a dictionary and a value collection on top of it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict);
			Assert.Equal(count, collection.Count);

			// create an enumerator and enumerate the collection
			var enumerator = collection.GetEnumerator();
			var enumerated = new List<TValue>();
			while (enumerator.MoveNext()) enumerated.Add(enumerator.Current);
			enumerator.Dispose();

			// compare collection elements with the expected keys
			// (the order should be the same as returned by the dictionary enumerator) 
			Assert.Equal(
				dict.Select(x => x.Value),
				enumerated);
		}

		#endregion

		#region ValueCollection.ICollection.Count

		/// <summary>
		/// Tests the <see cref="ICollection.Count"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ValueCollection_ICollection_Count_Get(int count)
		{
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection;
			Assert.Equal(data.Count, collection.Count);
		}

		#endregion

		#region ValueCollection.ICollection.IsSynchronized

		/// <summary>
		/// Tests the <see cref="ICollection.IsSynchronized"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollection_IsSynchronized_Get()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection;
			Assert.False(collection.IsSynchronized);
		}

		#endregion

		#region ValueCollection.ICollection.SyncRoot

		/// <summary>
		/// Tests the <see cref="ICollection.SyncRoot"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollection_SyncRoot_Get()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection;

			// getting the sync root twice should return the same instance
			object sync1 = collection.SyncRoot;
			object sync2 = collection.SyncRoot;
			Assert.NotNull(sync1);
			Assert.Same(sync1, sync2);

			// the sync root should be the same as the sync root of the dictionary
			Assert.Same(((ICollection)dict).SyncRoot, sync1);
		}

		#endregion

		#region ValueCollection.ICollection.CopyTo(Array, int)

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class
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
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection;

			// copy the collection into an array
			var destination = new TValue[count + index];
			collection.CopyTo(destination, index);

			// compare collection elements with the expected data set
			// (the order should be the same as returned by the dictionary enumerator) 
			Assert.Equal(
				dict.Select(x => x.Value),
				destination.Skip(index));
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class
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
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection;

			// copy the collection into an array
			object[] destination = new object[count + index];
			collection.CopyTo(destination, index);

			// compare collection elements with the expected data set
			// (the order should be the same as returned by the dictionary enumerator) 
			Assert.Equal(
				dict.Select(x => x.Value),
				destination.Skip(index).Cast<TValue>());
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class
		/// passing <c>null</c> for the destination array.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollection_CopyTo_ArrayNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection;
			var exception = Assert.Throws<ArgumentNullException>(() => collection.CopyTo(null, 0));
			Assert.Equal("array", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class
		/// passing a multi-dimensional destination array.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollection_CopyTo_MultidimensionalArray()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection;
			var destination = new TValue[0, 0];
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, 0));
			Assert.Equal("array", exception.ParamName);
			Assert.StartsWith("The array is multidimensional.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class
		/// passing a one-dimensional destination array with a lower bound other than zero.
		/// </summary>
		[Theory]
		[InlineData(-1)]
		[InlineData(1)]
		public void ValueCollection_ICollection_CopyTo_InvalidLowerArrayBound(int lowerBound)
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection;
			var destination = Array.CreateInstance(typeof(TValue), new[] { 0 }, new[] { lowerBound });
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, 0));
			Assert.Equal("array", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class
		/// passing an incompatible array type.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollection_CopyTo_InvalidArrayType()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection;
			float[] destination = new float[0];
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, 0));
			Assert.Equal("array", exception.ParamName);
			Assert.StartsWith("Invalid array type.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class
		/// passing an array index that is out of range.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_IndexOutOfBounds))]
		public void ValueCollection_ICollection_CopyTo_IndexOutOfRange(int count, int index)
		{
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection;
			var destination = new TValue[count];
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(destination, index));
			Assert.Equal("index", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class
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
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection;
			var destination = new TValue[arraySize];
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, index));
			Assert.StartsWith("The destination array is too small.", exception.Message);
		}

		#endregion

		#region ValueCollection.ICollection<T>.IsReadOnly

		/// <summary>
		/// Tests the <see cref="ICollection{T}.IsReadOnly"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollectionT_IsReadOnly()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection<TValue>;
			Assert.True(collection.IsReadOnly);
		}

		#endregion

		#region ValueCollection.ICollection<T>.Contains(T)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Contains"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class.
		/// The value is in the collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ValueCollection_ICollectionT_Contains(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection<TValue>;

			// test whether values of test data are reported to be in the collection
			foreach (var kvp in data)
			{
				bool contains = collection.Contains(kvp.Value);
				Assert.True(contains);
			}
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Contains"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class.
		/// The value is not in the collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes_WithoutZero))]
		public void ValueCollection_ICollectionT_Contains_NotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection<TValue>;

			// test whether some other value is reported to be not in the collection
			bool contains = collection.Contains(ValueNotInTestData);
			Assert.False(contains);
		}

		#endregion

		#region ValueCollection.ICollection<T>.Add(T)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Add"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class.
		/// The implementation is expected to throw a <see cref="NotSupportedException"/>.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollectionT_Add_NotSupported()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection<TValue>;
			Assert.Throws<NotSupportedException>(() => collection.Add(default));
		}

		#endregion

		#region ValueCollection.ICollection<T>.Clear()

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Clear"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class.
		/// The implementation is expected to throw a <see cref="NotSupportedException"/>.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollectionT_Clear_NotSupported()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection<TValue>;
			Assert.Throws<NotSupportedException>(() => collection.Clear());
		}

		#endregion

		#region ValueCollection.ICollection<T>.Remove(T)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Remove"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class.
		/// The implementation is expected to throw a <see cref="NotSupportedException"/>.
		/// </summary>
		[Fact]
		public void ValueCollection_ICollectionT_Remove_NotSupported()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as ICollection<TValue>;
			Assert.Throws<NotSupportedException>(() => collection.Remove(default));
		}

		#endregion

		#region ValueCollection.IEnumerable.GetEnumerator() - incl. all enumerator functionality

		/// <summary>
		/// Tests the <see cref="IEnumerable.GetEnumerator"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ValueCollection_IEnumerable_GetEnumerator(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as IEnumerable;

			// get an enumerator
			var enumerator = collection.GetEnumerator();

			// the enumerator should point to the position before the first valid element,
			// but the 'Current' property should not throw an exception
			object _ = enumerator.Current;

			// enumerate the values in the collection
			var enumerated = new List<TValue>();
			while (enumerator.MoveNext()) enumerated.Add((TValue)enumerator.Current);

			// the order of values should be the same as returned by the dictionary enumerator
			Assert.Equal(
				dict.Select(x => x.Value),
				enumerated);

			// the enumerator should point to the position after the last valid element now,
			// but the 'Current' property should not throw an exception
			_ = enumerator.Current;

			// reset the enumerator and try again
			enumerator.Reset();
			enumerated = new List<TValue>();
			while (enumerator.MoveNext()) enumerated.Add((TValue)enumerator.Current);

			// the order of values should be the same as returned by the dictionary enumerator
			Assert.Equal(
				dict.Select(x => x.Value),
				enumerated);

			// modify the collection, the enumerator should recognize this
			dict[KeyNotInTestData] = ValueNotInTestData;
			Assert.Throws<InvalidOperationException>(() => enumerator.Reset());
			Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
		}

		#endregion

		#region ValueCollection.IEnumerable<T>.GetEnumerator() - incl. all enumerator functionality

		/// <summary>
		/// Tests the <see cref="IEnumerable{T}.GetEnumerator"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.ValueCollection"/> class.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ValueCollection_IEnumerableT_GetEnumerator(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.ValueCollection(dict) as IEnumerable<TValue>;

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
				enumerated);

			// the enumerator should point to the position after the last valid element now,
			// but the 'Current' property should not throw an exception
			_ = enumerator.Current;

			// reset the enumerator and try again
			enumerator.Reset();
			enumerated = new List<TValue>();
			while (enumerator.MoveNext()) enumerated.Add(enumerator.Current);

			// the order of values should be the same as returned by the dictionary enumerator
			Assert.Equal(
				dict.Select(x => x.Value),
				enumerated);

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
