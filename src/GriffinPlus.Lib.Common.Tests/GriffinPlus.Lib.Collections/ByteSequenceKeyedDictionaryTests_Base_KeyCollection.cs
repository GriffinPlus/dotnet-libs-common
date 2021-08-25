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
		#region KeyCollection (Creation)

		/// <summary>
		/// Tests creating an instance of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void KeyCollection_Create(int count)
		{
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict);
			Assert.Equal(count, collection.Count);
		}

		#endregion

		#region KeyCollection.CopyTo(IReadOnlyList<byte>[],int)

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection.CopyTo"/> method.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData))]
		public void KeyCollection_CopyTo(int count, int index)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict);

			// copy the collection into an array
			var destination = new IReadOnlyList<byte>[count + index];
			collection.CopyTo(destination, index);

			// compare collection elements with the expected keys
			// (the order should be the same as returned by the dictionary enumerator) 
			Assert.Equal(
				dict.Select(x => x.Key),
				destination.Skip(index),
				ReadOnlyListEqualityComparer<byte>.Instance);
		}

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection.CopyTo"/> method passing <c>null</c> for the destination array.
		/// </summary>
		[Fact]
		public void KeyCollection_CopyTo_ArrayNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict);
			Assert.Throws<ArgumentNullException>(() => collection.CopyTo(null, 0));
		}

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection.CopyTo"/> method passing an array index that is out of range.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_IndexOutOfBounds))]
		public void KeyCollection_CopyTo_IndexOutOfRange(int count, int index)
		{
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict);
			var destination = new IReadOnlyList<byte>[count];
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(destination, index));
			Assert.Equal("index", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection.CopyTo"/> method passing an destination array that is too small to store all
		/// elements.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="arraySize">Size of the destination array.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_ArrayTooSmall))]
		public void KeyCollection_CopyTo_ArrayTooSmall(int count, int arraySize, int index)
		{
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict);
			var destination = new IReadOnlyList<byte>[arraySize];
			Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, index));
		}

		#endregion

		#region KeyCollection.GetEnumerator()

		/// <summary>
		/// Tests the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection.GetEnumerator"/> method and all
		/// enumerator functionality along with it.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void KeyCollection_GetEnumerator(int count)
		{
			// create a dictionary and a key collection on top of it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict);
			Assert.Equal(count, collection.Count);

			// create an enumerator and enumerate the collection
			var enumerator = collection.GetEnumerator();
			var enumerated = new List<IReadOnlyList<byte>>();
			while (enumerator.MoveNext()) enumerated.Add(enumerator.Current);
			enumerator.Dispose();

			// compare collection elements with the expected keys
			// (the order should be the same as returned by the dictionary enumerator) 
			Assert.Equal(
				dict.Select(x => x.Key),
				enumerated,
				ReadOnlyListEqualityComparer<byte>.Instance);
		}

		#endregion

		#region KeyCollection.ICollection.Count

		/// <summary>
		/// Tests the <see cref="ICollection.Count"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void KeyCollection_ICollection_Count_Get(int count)
		{
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection;
			Assert.Equal(data.Count, collection.Count);
		}

		#endregion

		#region KeyCollection.ICollection.IsSynchronized

		/// <summary>
		/// Tests the <see cref="ICollection.IsSynchronized"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollection_IsSynchronized_Get()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection;
			Assert.False(collection.IsSynchronized);
		}

		#endregion

		#region KeyCollection.ICollection.SyncRoot

		/// <summary>
		/// Tests the <see cref="ICollection.SyncRoot"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollection_SyncRoot_Get()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection;

			// getting the sync root twice should return the same instance
			object sync1 = collection.SyncRoot;
			object sync2 = collection.SyncRoot;
			Assert.NotNull(sync1);
			Assert.Same(sync1, sync2);

			// the sync root should be the same as the sync root of the dictionary
			Assert.Same(((ICollection)dict).SyncRoot, sync1);
		}

		#endregion

		#region KeyCollection.ICollection.CopyTo(Array, int)

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class
		/// (with an array of the specific item type).
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData))]
		public void KeyCollection_ICollection_CopyTo_TypedArray(int count, int index)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection;

			// copy the collection into an array
			var destination = new IReadOnlyList<byte>[count + index];
			collection.CopyTo(destination, index);

			// compare collection elements with the expected data set
			// (the order should be the same as returned by the dictionary enumerator) 
			Assert.Equal(
				dict.Select(x => x.Key),
				destination.Skip(index),
				ReadOnlyListEqualityComparer<byte>.Instance);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class
		/// (with an array of <see cref="System.Object"/>).
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData))]
		public void KeyCollection_ICollection_CopyTo_ObjectArray(int count, int index)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection;

			// copy the dictionary into an array
			object[] destination = new object[count + index];
			collection.CopyTo(destination, index);

			// compare collection elements with the expected data set
			// (the order should be the same as returned by the dictionary enumerator) 
			Assert.Equal(
				dict.Select(x => x.Key),
				destination.Skip(index).Cast<IReadOnlyList<byte>>(),
				ReadOnlyListEqualityComparer<byte>.Instance);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class
		/// passing <c>null</c> for the destination array.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollection_CopyTo_ArrayNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection;
			var exception = Assert.Throws<ArgumentNullException>(() => collection.CopyTo(null, 0));
			Assert.Equal("array", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class
		/// passing a multi-dimensional destination array.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollection_CopyTo_MultidimensionalArray()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection;
			var destination = new IReadOnlyList<byte>[0, 0];
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, 0));
			Assert.Equal("array", exception.ParamName);
			Assert.StartsWith("The array is multidimensional.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class
		/// passing a one-dimensional destination array with a lower bound other than zero.
		/// </summary>
		[Theory]
		[InlineData(-1)]
		[InlineData(1)]
		public void KeyCollection_ICollection_CopyTo_InvalidLowerArrayBound(int lowerBound)
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection;
			var destination = Array.CreateInstance(typeof(IReadOnlyList<byte>), new[] { 0 }, new[] { lowerBound });
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, 0));
			Assert.Equal("array", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class
		/// passing an incompatible array type.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollection_CopyTo_InvalidArrayType()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection;
			float[] destination = new float[0];
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, 0));
			Assert.Equal("array", exception.ParamName);
			Assert.StartsWith("Invalid array type.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class
		/// passing an array index that is out of range.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_IndexOutOfBounds))]
		public void KeyCollection_ICollection_CopyTo_IndexOutOfRange(int count, int index)
		{
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection;
			var destination = new IReadOnlyList<byte>[count];
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo(destination, index));
			Assert.Equal("index", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class
		/// passing an destination array that is too small to store all elements.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="arraySize">Size of the destination array.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_ArrayTooSmall))]
		public void KeyCollection_ICollection_CopyTo_ArrayTooSmall(int count, int arraySize, int index)
		{
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection;
			var destination = new IReadOnlyList<byte>[arraySize];
			var exception = Assert.Throws<ArgumentException>(() => collection.CopyTo(destination, index));
			Assert.StartsWith("The destination array is too small.", exception.Message);
		}

		#endregion

		#region KeyCollection.ICollection<T>.IsReadOnly

		/// <summary>
		/// Tests the <see cref="ICollection{T}.IsReadOnly"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollectionT_IsReadOnly()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection<IReadOnlyList<byte>>;
			Assert.True(collection.IsReadOnly);
		}

		#endregion

		#region KeyCollection.ICollection<T>.Contains(T)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Contains"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class.
		/// The key is in the collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void KeyCollection_ICollectionT_Contains(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection<IReadOnlyList<byte>>;

			// test whether keys of test data are reported to be in the collection
			foreach (var kvp in data)
			{
				bool contains = collection.Contains(kvp.Key);
				Assert.True(contains);
			}
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Contains"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class.
		/// The key is not in the collection.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes_WithoutZero))]
		public void KeyCollection_ICollectionT_Contains_KeyNotFound(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection<IReadOnlyList<byte>>;

			// test whether some other key is reported to be not in the collection
			bool contains = collection.Contains(KeyNotInTestData);
			Assert.False(contains);
		}

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Contains"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class.
		/// The method should return <c>false</c>, if the passed key is <c>null</c>.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollectionT_Contains_KeyNull()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection<IReadOnlyList<byte>>;
			bool contains = collection.Contains(null);
			Assert.False(contains);
		}

		#endregion

		#region KeyCollection.ICollection<T>.Add(T)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Add"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class.
		/// The implementation is expected to throw a <see cref="NotSupportedException"/>.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollectionT_Add_NotSupported()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection<IReadOnlyList<byte>>;
			Assert.Throws<NotSupportedException>(() => collection.Add(new byte[0]));
		}

		#endregion

		#region KeyCollection.ICollection<T>.Clear()

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Clear"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class.
		/// The implementation is expected to throw a <see cref="NotSupportedException"/>.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollectionT_Clear_NotSupported()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection<IReadOnlyList<byte>>;
			Assert.Throws<NotSupportedException>(() => collection.Clear());
		}

		#endregion

		#region KeyCollection.ICollection<T>.Remove(T)

		/// <summary>
		/// Tests the <see cref="ICollection{T}.Remove"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class.
		/// The implementation is expected to throw a <see cref="NotSupportedException"/>.
		/// </summary>
		[Fact]
		public void KeyCollection_ICollectionT_Remove_NotSupported()
		{
			var dict = new ByteSequenceKeyedDictionary<TValue>();
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as ICollection<IReadOnlyList<byte>>;
			Assert.Throws<NotSupportedException>(() => collection.Remove(new byte[0]));
		}

		#endregion

		#region KeyCollection.IEnumerable.GetEnumerator() - incl. all enumerator functionality

		/// <summary>
		/// Tests the <see cref="IEnumerable.GetEnumerator"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void KeyCollection_IEnumerable_GetEnumerator(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as IEnumerable;

			// get an enumerator
			var enumerator = collection.GetEnumerator();

			// the enumerator should point to the position before the first valid element,
			// but the 'Current' property should not throw an exception
			object _ = enumerator.Current;

			// enumerate the keys in the collection
			var enumerated = new List<IReadOnlyList<byte>>();
			while (enumerator.MoveNext()) enumerated.Add((IReadOnlyList<byte>)enumerator.Current);

			// the order of keys should be the same as returned by the dictionary enumerator
			Assert.Equal(
				dict.Select(x => x.Key),
				enumerated,
				ReadOnlyListEqualityComparer<byte>.Instance);

			// the enumerator should point to the position after the last valid element now,
			// but the 'Current' property should not throw an exception
			_ = enumerator.Current;

			// reset the enumerator and try again
			enumerator.Reset();
			enumerated = new List<IReadOnlyList<byte>>();
			while (enumerator.MoveNext()) enumerated.Add((IReadOnlyList<byte>)enumerator.Current);

			// the order of keys should be the same as returned by the dictionary enumerator
			Assert.Equal(
				dict.Select(x => x.Key),
				enumerated,
				ReadOnlyListEqualityComparer<byte>.Instance);

			// modify the collection, the enumerator should recognize this
			dict[KeyNotInTestData] = ValueNotInTestData;
			Assert.Throws<InvalidOperationException>(() => enumerator.Reset());
			Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
		}

		#endregion

		#region KeyCollection.IEnumerable<T>.GetEnumerator() - incl. all enumerator functionality

		/// <summary>
		/// Tests the <see cref="IEnumerable{T}.GetEnumerator"/> implementation of the <see cref="ByteSequenceKeyedDictionary{TValue}.KeyCollection"/> class.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void KeyCollection_IEnumerableT_GetEnumerator(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data);
			var collection = new ByteSequenceKeyedDictionary<TValue>.KeyCollection(dict) as IEnumerable<IReadOnlyList<byte>>;

			// get an enumerator
			var enumerator = collection.GetEnumerator();

			// the enumerator should point to the position before the first valid element,
			// but the 'Current' property should not throw an exception
			var _ = enumerator.Current;

			// enumerate the keys in the collection
			var enumerated = new List<IReadOnlyList<byte>>();
			while (enumerator.MoveNext()) enumerated.Add(enumerator.Current);

			// the order of keys should be the same as returned by the dictionary enumerator
			Assert.Equal(
				dict.Select(x => x.Key),
				enumerated,
				ReadOnlyListEqualityComparer<byte>.Instance);

			// the enumerator should point to the position after the last valid element now,
			// but the 'Current' property should not throw an exception
			_ = enumerator.Current;

			// reset the enumerator and try again
			enumerator.Reset();
			enumerated = new List<IReadOnlyList<byte>>();
			while (enumerator.MoveNext()) enumerated.Add(enumerator.Current);

			// the order of keys should be the same as returned by the dictionary enumerator
			Assert.Equal(
				dict.Select(x => x.Key),
				enumerated,
				ReadOnlyListEqualityComparer<byte>.Instance);

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
