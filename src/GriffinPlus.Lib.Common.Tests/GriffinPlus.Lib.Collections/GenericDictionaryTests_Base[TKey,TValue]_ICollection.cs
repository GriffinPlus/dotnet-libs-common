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
		#region ICollection.Count

		/// <summary>
		/// Tests getting the <see cref="ICollection.Count"/> property.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void ICollection_Count_Get(int count)
		{
			var data = GetTestData(count);
			var dict = GetDictionary(data) as ICollection;
			Assert.Equal(data.Count, dict.Count);
		}

		#endregion

		#region ICollection.IsSynchronized

		/// <summary>
		/// Tests getting the <see cref="ICollection.IsSynchronized"/> property.
		/// </summary>
		[Fact]
		public void ICollection_IsSynchronized_Get()
		{
			var dict = GetDictionary() as ICollection;
			Assert.False(dict.IsSynchronized);
		}

		#endregion

		#region ICollection.SyncRoot

		/// <summary>
		/// Tests getting the <see cref="ICollection.SyncRoot"/> property.
		/// </summary>
		[Fact]
		public void ICollection_SyncRoot_Get()
		{
			var dict = GetDictionary() as ICollection;
			object sync1 = dict.SyncRoot;
			object sync2 = dict.SyncRoot;
			Assert.NotNull(sync1);
			Assert.Same(sync1, sync2);
		}

		#endregion

		#region ICollection.CopyTo(Array, int)

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method (with an array of the specific item type).
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData))]
		public void ICollection_CopyTo_TypedArray(int count, int index)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as ICollection;

			// copy the dictionary into an array
			var destination = new KeyValuePair<TKey, TValue>[count + index];
			dict.CopyTo(destination, index);

			// compare collection elements with the expected data set
			Assert.Equal(
				data
					.OrderBy(x => x.Key, KeyComparer),
				destination
					.Skip(index)
					.OrderBy(x => x.Key, KeyComparer),
				KeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method (with an array of <see cref="DictionaryEntry"/>).
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData))]
		public void ICollection_CopyTo_DictionaryEntryArray(int count, int index)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as ICollection;

			// copy the dictionary into an array
			var destination = new DictionaryEntry[count + index];
			dict.CopyTo(destination, index);

			// compare collection elements with the expected data set
			Assert.Equal(
				data.OrderBy(x => x.Key, KeyComparer),
				destination
					.Skip(index)
					.Select(x => new KeyValuePair<TKey, TValue>((TKey)x.Key, (TValue)x.Value))
					.OrderBy(x => x.Key, KeyComparer),
				KeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method (with an array of <see cref="System.Object"/>).
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData))]
		public void ICollection_CopyTo_ObjectArray(int count, int index)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = GetDictionary(data) as ICollection;

			// copy the dictionary into an array
			object[] destination = new object[count + index];
			dict.CopyTo(destination, index);

			// compare collection elements with the expected data set
			Assert.Equal(
				data
					.OrderBy(x => x.Key, KeyComparer),
				destination
					.Skip(index)
					.Cast<KeyValuePair<TKey, TValue>>()
					.OrderBy(x => x.Key, KeyComparer),
				KeyValuePairEqualityComparer);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method passing <c>null</c> for the destination array.
		/// </summary>
		[Fact]
		public void ICollection_CopyTo_ArrayNull()
		{
			var dict = GetDictionary() as ICollection;
			// ReSharper disable once AssignNullToNotNullAttribute
			var exception = Assert.Throws<ArgumentNullException>(() => dict.CopyTo(null, 0));
			Assert.Equal("array", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method passing a multi-dimensional destination array.
		/// </summary>
		[Fact]
		public void ICollection_CopyTo_MultidimensionalArray()
		{
			var dict = GetDictionary() as ICollection;
			var destination = new KeyValuePair<TKey, TValue>[0, 0];
			var exception = Assert.Throws<ArgumentException>(() => dict.CopyTo(destination, 0));
			Assert.Equal("array", exception.ParamName);
			Assert.StartsWith("The array is multidimensional.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method passing a one-dimensional destination array with a lower
		/// bound other than zero.
		/// </summary>
		[Theory]
		[InlineData(-1)]
		[InlineData(1)]
		public void ICollection_CopyTo_InvalidLowerArrayBound(int lowerBound)
		{
			var dict = GetDictionary() as ICollection;
			var destination = Array.CreateInstance(typeof(KeyValuePair<TKey, TValue>), new[] { 0 }, new[] { lowerBound });
			var exception = Assert.Throws<ArgumentException>(() => dict.CopyTo(destination, 0));
			Assert.Equal("array", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method passing an incompatible array type.
		/// </summary>
		[Fact]
		public void ICollection_CopyTo_InvalidArrayType()
		{
			var dict = GetDictionary() as ICollection;
			int[] destination = Array.Empty<int>(); // actual collection element is always KeyValuePair<TKey,TValue>
			var exception = Assert.Throws<ArgumentException>(() => dict.CopyTo(destination, 0));
			Assert.Equal("array", exception.ParamName);
			Assert.StartsWith("Invalid array type.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method passing an array index that is out of range.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_IndexOutOfBounds))]
		public void ICollection_CopyTo_IndexOutOfRange(int count, int index)
		{
			var data = GetTestData(count);
			var dict = GetDictionary(data) as ICollection;
			var destination = new KeyValuePair<TKey, TValue>[count];
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => dict.CopyTo(destination, index));
			Assert.Equal("index", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="ICollection.CopyTo"/> method passing an destination array that is too small to store all elements.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		/// <param name="arraySize">Size of the destination array.</param>
		/// <param name="index">Index in the destination array to start copying to.</param>
		[Theory]
		[MemberData(nameof(CopyTo_TestData_ArrayTooSmall))]
		public void ICollection_CopyTo_ArrayTooSmall(int count, int arraySize, int index)
		{
			var data = GetTestData(count);
			var dict = GetDictionary(data) as ICollection;
			var destination = new KeyValuePair<TKey, TValue>[arraySize];
			var exception = Assert.Throws<ArgumentException>(() => dict.CopyTo(destination, index));
			Assert.StartsWith("The destination array is too small.", exception.Message);
		}

		#endregion
	}

}
