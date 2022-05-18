///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Collections
{

	/// <summary>
	/// Base class for unit tests targeting dictionaries implementing <see cref="IDictionary{TKey,TValue}"/>,
	/// <see cref="IDictionary"/>, <see cref="IReadOnlyDictionary{TKey,TValue}"/>, <see cref="ICollection{T}"/>,
	/// <see cref="ICollection"/>, <see cref="IEnumerable{T}"/> and <see cref="IEnumerable"/>.
	/// </summary>
	public abstract partial class GenericDictionaryTests_Base<TKey, TValue>
	{
		/// <summary>
		/// Gets an instance of the dictionary to test, populated with the specified data.
		/// </summary>
		/// <param name="data">Data to populate the dictionary with.</param>
		/// <returns>A new instance of the dictionary to test, populated with the specified data.</returns>
		protected abstract IGenericDictionary<TKey, TValue> GetDictionary(IDictionary<TKey, TValue> data = null);

		/// <summary>
		/// Gets a dictionary containing some test data.
		/// </summary>
		/// <param name="count">Number of entries in the dictionary.</param>
		/// <returns>A test data dictionary.</returns>
		protected abstract IDictionary<TKey, TValue> GetTestData(int count);

		/// <summary>
		/// Gets a key that is guaranteed to be not in the generated test data set.
		/// Must not be the default value of <see cref="TKey"/>.
		/// </summary>
		protected abstract TKey KeyNotInTestData { get; }

		/// <summary>
		/// Gets a value that is guaranteed to be not in the generated test data set.
		/// Must not be the default value of <see cref="TValue"/>.
		/// </summary>
		protected abstract TValue ValueNotInTestData { get; }

		/// <summary>
		/// Gets a comparer for comparing keys.
		/// </summary>
		protected abstract IComparer<TKey> KeyComparer { get; }

		/// <summary>
		/// Gets an equality comparer for comparing keys.
		/// </summary>
		protected abstract IEqualityComparer<TKey> KeyEqualityComparer { get; }

		/// <summary>
		/// Gets a comparer for comparing values.
		/// </summary>
		protected virtual IComparer<TValue> ValueComparer => Comparer<TValue>.Default;

		/// <summary>
		/// Gets an equality comparer for comparing values.
		/// </summary>
		protected virtual IEqualityComparer<TValue> ValueEqualityComparer => EqualityComparer<TValue>.Default;

		/// <summary>
		/// Gets an equality comparer for comparing key/value pairs returned by the dictionary
		/// </summary>
		protected virtual KeyValuePairEqualityComparer<TKey, TValue> KeyValuePairEqualityComparer => new KeyValuePairEqualityComparer<TKey, TValue>(KeyEqualityComparer, ValueEqualityComparer);

		#region Test Data

		/// <summary>
		/// Test data for tests expecting the size of the test data set only.
		/// Contains: 0, 1, 10, 100, 1000, 10000.
		/// </summary>
		public static IEnumerable<object[]> TestDataSetSizes
		{
			get
			{
				yield return new object[] { 0 };
				yield return new object[] { 1 };
				yield return new object[] { 10 };
				yield return new object[] { 100 };
			}
		}

		/// <summary>
		/// Test data for tests expecting the size of the test data set only.
		/// Contains: 1, 10, 100, 1000, 10000.
		/// </summary>
		public static IEnumerable<object[]> TestDataSetSizes_WithoutZero
		{
			get
			{
				yield return new object[] { 1 };
				yield return new object[] { 10 };
				yield return new object[] { 100 };
			}
		}

		/// <summary>
		/// Test data for CopyTo() tests expecting the size of the test data set and an index in the destination array to start copying to.
		/// For tests that should succeed.
		/// </summary>
		public static IEnumerable<object[]> CopyTo_TestData
		{
			get
			{
				foreach (object[] data in TestDataSetSizes)
				{
					int count = (int)data[0];
					yield return new object[] { count, 0 };
					yield return new object[] { count, 1 };
					yield return new object[] { count, 5 };
				}
			}
		}

		/// <summary>
		/// Test data for CopyTo() tests expecting the size of the test data set and an index in the destination array to start copying to.
		/// For tests that check whether CopyTo() fails, if the index is out of bounds.
		/// </summary>
		public static IEnumerable<object[]> CopyTo_TestData_IndexOutOfBounds
		{
			get
			{
				foreach (object[] data in TestDataSetSizes)
				{
					int count = (int)data[0];
					yield return new object[] { count, -1 };        // before start of array
					yield return new object[] { count, count + 1 }; // after end of array (count is ok, if there are no elements to copy)
				}
			}
		}

		/// <summary>
		/// Test data for CopyTo() tests expecting the size of the test data set, the size of the destination array and an
		/// index in the destination array to start copying to.
		/// For tests that check whether CopyTo() fails, if the array is too small.
		/// </summary>
		public static IEnumerable<object[]> CopyTo_TestData_ArrayTooSmall
		{
			get
			{
				foreach (object[] data in TestDataSetSizes)
				{
					int count = (int)data[0];

					if (count > 0)
					{
						// destination array is way too small to store any elements
						yield return new object[] { count, 0, 0 };

						// destination array itself is large enough, but start index shifts the destination out
						// (the last element does not fit into the array)
						yield return new object[] { count, count, 1 };

						// destination array itself is large enough, but start index shifts the destination out
						// (no space left for any elements)
						yield return new object[] { count, count, count };
					}
				}
			}
		}

		#endregion
	}

}
