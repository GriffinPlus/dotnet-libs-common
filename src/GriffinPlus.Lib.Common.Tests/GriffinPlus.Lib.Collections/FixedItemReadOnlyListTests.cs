///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using Xunit;

// ReSharper disable CollectionNeverQueried.Local
// ReSharper disable CollectionNeverUpdated.Local

namespace GriffinPlus.Lib.Collections
{

	/// <summary>
	/// Unit tests targeting the <see cref="FixedItemReadOnlyList{T}"/> class.
	/// </summary>
	public class FixedItemReadOnlyListTests
	{
		#region Test Data

		public class TestItem { }

		/// <summary>
		/// Test data for tests expecting a certain number of items in the collection.
		/// </summary>
		public static IEnumerable<object[]> TestData
		{
			get
			{
				var items = new[] { null, new TestItem() };
				foreach (var item in items)
				{
					yield return new object[] { item, 0 }; // empty collection
					yield return new object[] { item, 1 }; // one item only
					yield return new object[] { item, 2 }; // more than one item
				}
			}
		}

		#endregion

		#region Construction

		/// <summary>
		/// Tests the <see cref="FixedItemReadOnlyList{T}"/> constructor succeeds with valid arguments.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Create(TestItem item, int count)
		{
			var _ = new FixedItemReadOnlyList<TestItem>(item, count);
		}

		/// <summary>
		/// Tests the <see cref="FixedItemReadOnlyList{T}"/> constructor fails if the number of items is negative.
		/// </summary>
		[Fact]
		public void Create_CountIsNegative()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new FixedItemReadOnlyList<TestItem>(new TestItem(), -1));
		}

		#endregion

		#region Count

		/// <summary>
		/// Tests getting the <see cref="FixedItemReadOnlyList{T}.Count"/> property.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Count_Get(TestItem item, int count)
		{
			var list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Equal(count, list.Count);
		}

		#endregion

		#region IsFixedSize

		/// <summary>
		/// Tests the explicit implementation of the <see cref="IList.IsFixedSize"/> property.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void IsFixedSize_IList_Get(TestItem item, int count)
		{
			IList list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.True(list.IsFixedSize);
		}

		#endregion

		#region IsReadOnly

		/// <summary>
		/// Tests the explicit implementation of the <see cref="ICollection{T}.IsReadOnly"/> property.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void IsReadOnly_ICollectionT_Get(TestItem item, int count)
		{
			ICollection<TestItem> list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.True(list.IsReadOnly);
		}

		/// <summary>
		/// Tests the explicit implementation of the <see cref="IList.IsReadOnly"/> property.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void IsReadOnly_IList_Get(TestItem item, int count)
		{
			IList list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.True(list.IsReadOnly);
		}

		#endregion

		#region IsSynchronized

		/// <summary>
		/// Tests the explicit implementation of the <see cref="ICollection.IsSynchronized"/> property.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void IsSynchronized_ICollection_Get(TestItem item, int count)
		{
			ICollection list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.False(list.IsSynchronized);
		}

		#endregion

		#region SyncRoot

		/// <summary>
		/// Tests the explicit implementation of the <see cref="ICollection.SyncRoot"/> property.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void SyncRoot_ICollection_Get(TestItem item, int count)
		{
			ICollection list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Same(list, list.SyncRoot);
		}

		#endregion

		#region this[]

		/// <summary>
		/// Tests getting the <see cref="FixedItemReadOnlyList{T}.this"/> property.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Indexer_Get(TestItem item, int count)
		{
			var list = new FixedItemReadOnlyList<TestItem>(item, count);

			// all items reachable via the the indexer should be the same as the item passed during construction
			for (int i = 0; i < count; i++)
			{
				Assert.Same(item, list[i]);
			}

			// getting item outside the expected bounds should fail
			Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => list[-1]).ParamName);
			Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => list[count]).ParamName);
		}

		/// <summary>
		/// Tests the explicit implementation of the getter of the <see cref="IList{T}.this"/> property.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Indexer_IListT_Get(TestItem item, int count)
		{
			IList<TestItem> list = new FixedItemReadOnlyList<TestItem>(item, count);

			// all items reachable via the the indexer should be the same as the item passed during construction
			for (int i = 0; i < count; i++)
			{
				Assert.Same(item, list[i]);
			}

			// getting item outside the expected bounds should fail
			Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => list[-1]).ParamName);
			Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => list[count]).ParamName);
		}

		/// <summary>
		/// Tests the explicit implementation of the setter of the <see cref="IList{T}.this"/> property.
		/// The setter should throw a <see cref="NotSupportedException"/>.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Indexer_IListT_Set(TestItem item, int count)
		{
			IList<TestItem> list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Throws<NotSupportedException>(() => list[0] = item);
		}

		/// <summary>
		/// Tests the explicit implementation of the getter of the <see cref="IList.this"/> property.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Indexer_IList_Get(TestItem item, int count)
		{
			IList list = new FixedItemReadOnlyList<TestItem>(item, count);

			// all items reachable via the the indexer should be the same as the item passed during construction
			for (int i = 0; i < count; i++)
			{
				Assert.Same(item, list[i]);
			}

			// getting item outside the expected bounds should fail
			Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => list[-1]).ParamName);
			Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => list[count]).ParamName);
		}

		/// <summary>
		/// Tests the explicit implementation of the setter of the <see cref="IList.this"/> property.
		/// The setter should throw a <see cref="NotSupportedException"/>.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Indexer_IList_Set(TestItem item, int count)
		{
			IList list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Throws<NotSupportedException>(() => list[0] = item);
		}

		#endregion

		#region Add()

		/// <summary>
		/// Tests the explicit implementation of the <see cref="ICollection{T}.Add"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Add_ICollectionT(TestItem item, int count)
		{
			ICollection<TestItem> list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Throws<NotSupportedException>(() => list.Add(item));
		}

		/// <summary>
		/// Tests the explicit implementation of the <see cref="IList.Add"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Add_IList(TestItem item, int count)
		{
			IList list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Throws<NotSupportedException>(() => list.Add(item));
		}

		#endregion

		#region Clear()

		/// <summary>
		/// Tests the explicit implementation of the <see cref="ICollection{T}.Clear"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Clear_ICollectionT(TestItem item, int count)
		{
			ICollection<TestItem> list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Throws<NotSupportedException>(() => list.Clear());
		}

		/// <summary>
		/// Tests the explicit implementation of the <see cref="IList.Clear"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Clear_IList(TestItem item, int count)
		{
			IList list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Throws<NotSupportedException>(() => list.Clear());
		}

		#endregion

		#region Contains()

		/// <summary>
		/// Tests the <see cref="FixedItemReadOnlyList{T}.Contains"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Contains(TestItem item, int count)
		{
#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection
			var list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.True(list.Contains(item));
			Assert.False(list.Contains(new TestItem()));
#pragma warning restore xUnit2017 // Do not use Contains() to check if a value exists in a collection
		}

		/// <summary>
		/// Tests the explicit implementation of the <see cref="IList.Contains"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Contains_IList(TestItem item, int count)
		{
			IList list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.True(list.Contains(item));
			Assert.False(list.Contains(new TestItem()));
		}

		#endregion

		#region CopyTo()

		/// <summary>
		/// Tests the <see cref="FixedItemReadOnlyList{T}.CopyTo"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void CopyTo(TestItem item, int count)
		{
			var list = new FixedItemReadOnlyList<TestItem>(item, count);
			var array = new TestItem[count];
			list.CopyTo(array, 0);
			Assert.All(array, x => Assert.Same(item, x));
		}

		/// <summary>
		/// Tests the <see cref="FixedItemReadOnlyList{T}.CopyTo"/> method.
		/// The method should throw an exception if the specified destination array is <c>null</c>.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void CopyTo_ArrayIsNull(TestItem item, int count)
		{
			var list = new FixedItemReadOnlyList<TestItem>(item, count);
			// ReSharper disable once AssignNullToNotNullAttribute
			Assert.Equal("array", Assert.Throws<ArgumentNullException>(() => list.CopyTo(null, 0)).ParamName);
		}

		/// <summary>
		/// Tests the <see cref="FixedItemReadOnlyList{T}.CopyTo"/> method.
		/// The method should throw an exception if the specified start index is out of bounds.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void CopyTo_IndexIsOutOfBounds(TestItem item, int count)
		{
			var list = new FixedItemReadOnlyList<TestItem>(item, count);
			var array = new TestItem[count];
			Assert.Equal("arrayIndex", Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(array, -1)).ParamName);
			Assert.Equal("arrayIndex", Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(array, array.Length + 1)).ParamName);
			if (count > 0) Assert.Null(Assert.Throws<ArgumentException>(() => list.CopyTo(array, 1)).ParamName);
		}

		/// <summary>
		/// Tests the explicit implementation of the <see cref="ICollection.CopyTo"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void CopyTo_ICollection(TestItem item, int count)
		{
			ICollection list = new FixedItemReadOnlyList<TestItem>(item, count);
			var array = new TestItem[count];
			list.CopyTo(array, 0);
			Assert.All(array, x => Assert.Same(item, x));
		}

		/// <summary>
		/// Tests the explicit implementation of the <see cref="ICollection.CopyTo"/> method.
		/// The method should throw an exception if the specified destination array is <c>null</c>.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void CopyTo_ICollection_ArrayIsNull(TestItem item, int count)
		{
			ICollection list = new FixedItemReadOnlyList<TestItem>(item, count);
			// ReSharper disable once AssignNullToNotNullAttribute
			Assert.Equal("array", Assert.Throws<ArgumentNullException>(() => list.CopyTo(null, 0)).ParamName);
		}

		/// <summary>
		/// Tests the explicit implementation of the <see cref="ICollection.CopyTo"/> method.
		/// The method should throw an exception if the specified start index is out of bounds.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void CopyTo_ICollection_IndexIsOutOfBounds(TestItem item, int count)
		{
			ICollection list = new FixedItemReadOnlyList<TestItem>(item, count);
			var array = new TestItem[count];
			Assert.Equal("arrayIndex", Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(array, -1)).ParamName);
			Assert.Equal("arrayIndex", Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(array, array.Length + 1)).ParamName);
			if (count > 0) Assert.Null(Assert.Throws<ArgumentException>(() => list.CopyTo(array, 1)).ParamName);
		}

		/// <summary>
		/// Tests the explicit implementation of the <see cref="ICollection.CopyTo"/> method.
		/// The method should throw an exception if the specified array is multi-dimensional.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void CopyTo_ICollection_ArrayIsMultiDimensional(TestItem item, int count)
		{
			ICollection list = new FixedItemReadOnlyList<TestItem>(item, count);
			var array = new TestItem[count, count];
			Assert.Equal("array", Assert.Throws<ArgumentException>(() => list.CopyTo(array, 0)).ParamName);
		}

		#endregion

		#region GetEnumerator()

		/// <summary>
		/// Tests the <see cref="FixedItemReadOnlyList{T}.GetEnumerator"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void GetEnumerator(TestItem item, int count)
		{
			var list = new FixedItemReadOnlyList<TestItem>(item, count);
			var enumerated1 = new List<TestItem>();
			var enumerated2 = new List<TestItem>();
			var enumerator = list.GetEnumerator();
			while (enumerator.MoveNext()) enumerated1.Add(enumerator.Current);
			enumerator.Reset();
			while (enumerator.MoveNext()) enumerated2.Add(enumerator.Current);
			enumerator.Dispose();
			Assert.Equal(count, enumerated1.Count);
			Assert.Equal(count, enumerated2.Count);
			Assert.All(enumerated1, x => Assert.Same(item, x));
			Assert.All(enumerated2, x => Assert.Same(item, x));
		}

		/// <summary>
		/// Tests the explicit implementation of the <see cref="IEnumerable.GetEnumerator"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void GetEnumerator_IEnumerable(TestItem item, int count)
		{
			IEnumerable list = new FixedItemReadOnlyList<TestItem>(item, count);
			var enumerated1 = new List<TestItem>();
			var enumerated2 = new List<TestItem>();
			var enumerator = list.GetEnumerator();
			while (enumerator.MoveNext()) enumerated1.Add((TestItem)enumerator.Current);
			enumerator.Reset();
			while (enumerator.MoveNext()) enumerated2.Add((TestItem)enumerator.Current);
			Assert.Equal(count, enumerated1.Count);
			Assert.Equal(count, enumerated2.Count);
			Assert.All(enumerated1, x => Assert.Same(item, x));
			Assert.All(enumerated2, x => Assert.Same(item, x));
		}

		#endregion

		#region IndexOf()

		/// <summary>
		/// Tests the <see cref="FixedItemReadOnlyList{T}.IndexOf"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void IndexOf(TestItem item, int count)
		{
			var list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Equal(0, list.IndexOf(item));
			Assert.Equal(-1, list.IndexOf(new TestItem()));
		}

		/// <summary>
		/// Tests the explicit implementation of the <see cref="IList.IndexOf"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void IndexOf_IList(TestItem item, int count)
		{
			IList list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Equal(0, list.IndexOf(item));
			Assert.Equal(-1, list.IndexOf(new TestItem()));
		}

		#endregion

		#region Insert()

		/// <summary>
		/// Tests the explicit implementation of the <see cref="IList{T}.Insert"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Insert_IListT(TestItem item, int count)
		{
			IList<TestItem> list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Throws<NotSupportedException>(() => list.Insert(0, new TestItem()));
		}

		/// <summary>
		/// Tests the explicit implementation of the <see cref="IList.Insert"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Insert_IList(TestItem item, int count)
		{
			IList list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Throws<NotSupportedException>(() => list.Insert(0, new TestItem()));
		}

		#endregion

		#region Remove()

		/// <summary>
		/// Tests the explicit implementation of the <see cref="ICollection{T}.Remove"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Remove_ICollectionT(TestItem item, int count)
		{
			ICollection<TestItem> list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Throws<NotSupportedException>(() => list.Remove(item));
		}

		/// <summary>
		/// Tests the explicit implementation of the <see cref="IList.Remove"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void Remove_IList(TestItem item, int count)
		{
			IList list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Throws<NotSupportedException>(() => list.Remove(item));
		}

		#endregion

		#region RemoveAt()

		/// <summary>
		/// Tests the explicit implementation of the <see cref="IList{T}.RemoveAt"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void RemoveAt_IListT(TestItem item, int count)
		{
			IList<TestItem> list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Throws<NotSupportedException>(() => list.RemoveAt(0));
		}

		/// <summary>
		/// Tests the explicit implementation of the <see cref="IList.RemoveAt"/> method.
		/// </summary>
		/// <param name="item">The item the list should provide.</param>
		/// <param name="count">Number of times the list should provide the item.</param>
		[Theory]
		[MemberData(nameof(TestData))]
		public void RemoveAt_IList(TestItem item, int count)
		{
			IList list = new FixedItemReadOnlyList<TestItem>(item, count);
			Assert.Throws<NotSupportedException>(() => list.RemoveAt(0));
		}

		#endregion
	}

}
