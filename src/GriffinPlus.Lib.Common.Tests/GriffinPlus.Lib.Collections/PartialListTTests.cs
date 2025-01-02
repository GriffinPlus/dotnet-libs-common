///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace GriffinPlus.Lib.Collections;

/// <summary>
/// Unit tests targeting the <see cref="PartialList{T}"/> class.
/// </summary>
public class PartialListTTests
{
	#region Test Data

	public class TestItem;

	/// <summary>
	/// Test data for tests expecting a certain number of items in the collection.
	/// </summary>
	public static IEnumerable<object[]> TestData
	{
		get
		{
			// source list contains no items
			var items0 = new TestItem[] { };
			yield return [items0, 0, 0];

			// source list contains one items
			var items1 = new[] { new TestItem() };
			yield return [items1, 0, 1];
			yield return [items1, 1, 0];

			// source list contains two items
			var items2 = new[] { new TestItem(), new TestItem() };
			yield return [items2, 0, 1];
			yield return [items2, 0, 2];
			yield return [items2, 1, 0];
			yield return [items2, 1, 1];
			yield return [items2, 2, 0];

			// source list contains three items
			var items3 = new[] { new TestItem(), new TestItem(), new TestItem() };
			yield return [items3, 0, 1];
			yield return [items3, 0, 2];
			yield return [items3, 0, 3];
			yield return [items3, 1, 0];
			yield return [items3, 1, 1];
			yield return [items3, 1, 2];
			yield return [items3, 2, 0];
			yield return [items3, 2, 1];
			yield return [items3, 3, 0];
		}
	}

	#endregion

	#region Construction

	/// <summary>
	/// Tests whether the <see cref="PartialList{T}"/> constructor succeeds with valid arguments.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void Create_Subset(TestItem[] items, int offset, int count)
	{
		var list = new PartialList<TestItem>(items, offset, count);
		Assert.Equal(items.Skip(offset).Take(count), list); // uses the enumerator to compare the lists
	}

	/// <summary>
	/// Tests whether the <see cref="PartialList{T}"/> convenience constructor for creating read-only lists succeeds.
	/// </summary>
	[Fact]
	public void Create_Complete()
	{
		var items = new[] { new TestItem(), new TestItem(), new TestItem() };
		var list = new PartialList<TestItem>(items);
		Assert.Equal(items, list); // uses the enumerator to compare the lists
	}

	/// <summary>
	/// Tests whether the <see cref="PartialList{T}"/> constructor fails if the number of items is negative.
	/// </summary>
	[Fact]
	public void Create_ListIsNull()
	{
		var exception = Assert.Throws<ArgumentNullException>(() => new PartialList<TestItem>(null, 0, 0));
		Assert.Equal("list", exception.ParamName);
	}

	/// <summary>
	/// Tests whether the <see cref="PartialList{T}"/> constructor fails if the offset in the original list is negative.
	/// </summary>
	[Fact]
	public void Create_OffsetIsNegative()
	{
		var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new PartialList<TestItem>([new TestItem()], -1, 0));
		Assert.Equal("offset", exception.ParamName);
	}

	/// <summary>
	/// Tests whether the <see cref="PartialList{T}"/> constructor fails if the number of items is negative.
	/// </summary>
	[Fact]
	public void Create_CountIsNegative()
	{
		var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new PartialList<TestItem>([new TestItem()], 0, -1));
		Assert.Equal("count", exception.ParamName);
	}

	/// <summary>
	/// Tests whether the <see cref="PartialList{T}"/> constructor fails if the specified range is out of bounds.
	/// </summary>
	[Fact]
	public void Create_RangeIsOutOfBounds()
	{
		var items = new[] { new TestItem() };
		Assert.Null(Assert.Throws<ArgumentException>(() => new PartialList<TestItem>(items, 0, 2)).ParamName);
		Assert.Null(Assert.Throws<ArgumentException>(() => new PartialList<TestItem>(items, 1, 1)).ParamName);
	}

	#endregion

	#region Count

	/// <summary>
	/// Tests getting the <see cref="PartialList{T}.Count"/> property.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void Count_Get(TestItem[] items, int offset, int count)
	{
		var list = new PartialList<TestItem>(items, offset, count);
		Assert.Equal(count, list.Count);
	}

	#endregion

	#region IsFixedSize

	/// <summary>
	/// Tests the explicit implementation of the <see cref="IList.IsFixedSize"/> property.
	/// </summary>
	[Fact]
	public void IsFixedSize_IList_Get()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		IList list = new PartialList<TestItem>(items, 0, 1);
		Assert.True(list.IsFixedSize);
	}

	#endregion

	#region IsReadOnly

	/// <summary>
	/// Tests the explicit implementation of the <see cref="ICollection{T}.IsReadOnly"/> property.
	/// </summary>
	[Fact]
	public void IsReadOnly_ICollectionT_Get()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		ICollection<TestItem> list = new PartialList<TestItem>(items, 0, 1);
		Assert.True(list.IsReadOnly);
	}

	/// <summary>
	/// Tests the explicit implementation of the <see cref="IList.IsReadOnly"/> property.
	/// </summary>
	[Fact]
	public void IsReadOnly_IList_Get()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		IList list = new PartialList<TestItem>(items, 0, 1);
		Assert.True(list.IsReadOnly);
	}

	#endregion

	#region IsSynchronized

	/// <summary>
	/// Tests the explicit implementation of the <see cref="ICollection.IsSynchronized"/> property.
	/// </summary>
	[Fact]
	public void IsSynchronized_ICollection_Get()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		ICollection list = new PartialList<TestItem>(items, 0, 1);
		Assert.False(list.IsSynchronized);
	}

	#endregion

	#region SyncRoot

	/// <summary>
	/// Tests the explicit implementation of the <see cref="ICollection.SyncRoot"/> property.
	/// </summary>
	[Fact]
	public void SyncRoot_ICollection_Get()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		ICollection list = new PartialList<TestItem>(items, 0, 1);
		Assert.Same(list, list.SyncRoot);
	}

	#endregion

	#region this[]

	/// <summary>
	/// Tests getting the <see cref="PartialList{T}.this"/> property.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void Indexer_Get(TestItem[] items, int offset, int count)
	{
		var list = new PartialList<TestItem>(items, offset, count);

		// all items reachable via the indexer should be the same as the item passed during construction
		for (int i = offset; i < offset + count; i++)
		{
			Assert.Same(items[i], list[i - offset]);
		}

		// getting item outside the expected bounds should fail
		Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => list[-1]).ParamName);
		Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => list[count]).ParamName);
	}

	/// <summary>
	/// Tests the explicit implementation of the getter of the <see cref="IList{T}.this"/> property.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void Indexer_IListT_Get(TestItem[] items, int offset, int count)
	{
		IList<TestItem> list = new PartialList<TestItem>(items, offset, count);

		// all items reachable via the indexer should be the same as the item passed during construction
		for (int i = offset; i < offset + count; i++)
		{
			Assert.Same(items[i], list[i - offset]);
		}

		// getting item outside the expected bounds should fail
		Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => list[-1]).ParamName);
		Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => list[count]).ParamName);
	}

	/// <summary>
	/// Tests the explicit implementation of the setter of the <see cref="IList{T}.this"/> property.
	/// The setter should throw a <see cref="NotSupportedException"/>.
	/// </summary>
	[Fact]
	public void Indexer_IListT_Set()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		IList<TestItem> list = new PartialList<TestItem>(items);
		Assert.Throws<NotSupportedException>(() => list[0] = new TestItem());
	}

	/// <summary>
	/// Tests the explicit implementation of the getter of the <see cref="IList.this"/> property.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void Indexer_IList_Get(TestItem[] items, int offset, int count)
	{
		IList list = new PartialList<TestItem>(items, offset, count);

		// all items reachable via the indexer should be the same as the item passed during construction
		for (int i = offset; i < offset + count; i++)
		{
			Assert.Same(items[i], list[i - offset]);
		}

		// getting item outside the expected bounds should fail
		Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => list[-1]).ParamName);
		Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => list[count]).ParamName);
	}

	/// <summary>
	/// Tests the explicit implementation of the setter of the <see cref="IList.this"/> property.
	/// The setter should throw a <see cref="NotSupportedException"/>.
	/// </summary>
	[Fact]
	public void Indexer_IList_Set()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		IList list = new PartialList<TestItem>(items);
		Assert.Throws<NotSupportedException>(() => list[0] = new TestItem());
	}

	#endregion

	#region Add()

	/// <summary>
	/// Tests the explicit implementation of the <see cref="ICollection{T}.Add"/> method.
	/// </summary>
	[Fact]
	public void Add_ICollectionT()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		ICollection<TestItem> list = new PartialList<TestItem>(items);
		Assert.Throws<NotSupportedException>(() => list.Add(new TestItem()));
	}

	/// <summary>
	/// Tests the explicit implementation of the <see cref="IList.Add"/> method.
	/// </summary>
	[Fact]
	public void Add_IList()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		IList list = new PartialList<TestItem>(items);
		Assert.Throws<NotSupportedException>(() => list.Add(new TestItem()));
	}

	#endregion

	#region Clear()

	/// <summary>
	/// Tests the explicit implementation of the <see cref="ICollection{T}.Clear"/> method.
	/// </summary>
	[Fact]
	public void Clear_ICollectionT()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		ICollection<TestItem> list = new PartialList<TestItem>(items);
		Assert.Throws<NotSupportedException>(() => list.Clear());
	}

	/// <summary>
	/// Tests the explicit implementation of the <see cref="IList.Clear"/> method.
	/// </summary>
	[Fact]
	public void Clear_IList()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		IList list = new PartialList<TestItem>(items);
		Assert.Throws<NotSupportedException>(() => list.Clear());
	}

	#endregion

	#region Contains()

	/// <summary>
	/// Tests the <see cref="PartialList{T}.Contains"/> method.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void Contains(TestItem[] items, int offset, int count)
	{
#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection

		var list = new PartialList<TestItem>(items, offset, count);

		// check whether all items in the subset are in the partial list
		for (int i = offset; i < offset + count; i++)
		{
			Assert.True(list.Contains(items[i]));
		}

		// check whether a new item is not in the partial list
		Assert.False(list.Contains(new TestItem()));

#pragma warning restore xUnit2017 // Do not use Contains() to check if a value exists in a collection
	}

	/// <summary>
	/// Tests the explicit implementation of the <see cref="IList.Contains"/> method.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void Contains_IList(TestItem[] items, int offset, int count)
	{
#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection

		IList list = new PartialList<TestItem>(items, offset, count);

		// check whether all items in the subset are in the partial list
		for (int i = offset; i < offset + count; i++)
		{
			Assert.True(list.Contains(items[i]));
		}

		// check whether a new item is not in the partial list
		Assert.False(list.Contains(new TestItem())); // type of item is valid, but item is not in the list
		Assert.False(list.Contains(new object()));   // type of item is invalid

#pragma warning restore xUnit2017 // Do not use Contains() to check if a value exists in a collection
	}

	#endregion

	#region CopyTo()

	/// <summary>
	/// Tests the <see cref="PartialList{T}.CopyTo"/> method.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void CopyTo(TestItem[] items, int offset, int count)
	{
		var list = new PartialList<TestItem>(items, offset, count);
		var array = new TestItem[count];
		list.CopyTo(array, 0);
		Assert.Equal(items.Skip(offset).Take(count), array);
	}

	/// <summary>
	/// Tests the <see cref="PartialList{T}.CopyTo"/> method.
	/// The method should throw an exception if the specified destination array is <c>null</c>.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void CopyTo_ArrayIsNull(TestItem[] items, int offset, int count)
	{
		var list = new PartialList<TestItem>(items, offset, count);
		// ReSharper disable once AssignNullToNotNullAttribute
		Assert.Equal("array", Assert.Throws<ArgumentNullException>(() => list.CopyTo(null, 0)).ParamName);
	}

	/// <summary>
	/// Tests the <see cref="PartialList{T}.CopyTo"/> method.
	/// The method should throw an exception if the specified start index is out of bounds.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void CopyTo_IndexIsOutOfBounds(TestItem[] items, int offset, int count)
	{
		var list = new PartialList<TestItem>(items, offset, count);
		var array = new TestItem[count];
		Assert.Equal("arrayIndex", Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(array, -1)).ParamName);
		Assert.Equal("arrayIndex", Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(array, array.Length + 1)).ParamName);
		if (count > 1) Assert.Null(Assert.Throws<ArgumentException>(() => list.CopyTo(array, 1)).ParamName);
	}

	/// <summary>
	/// Tests the explicit implementation of the <see cref="ICollection.CopyTo"/> method.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void CopyTo_ICollection(TestItem[] items, int offset, int count)
	{
		ICollection list = new PartialList<TestItem>(items, offset, count);
		var array = new TestItem[count];
		list.CopyTo(array, 0);
		Assert.Equal(items.Skip(offset).Take(count), array);
	}

	/// <summary>
	/// Tests the explicit implementation of the <see cref="ICollection.CopyTo"/> method.
	/// The method should throw an exception if the specified destination array is <c>null</c>.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void CopyTo_ICollection_ArrayIsNull(TestItem[] items, int offset, int count)
	{
		ICollection list = new PartialList<TestItem>(items, offset, count);
		// ReSharper disable once AssignNullToNotNullAttribute
		Assert.Equal("array", Assert.Throws<ArgumentNullException>(() => list.CopyTo(null, 0)).ParamName);
	}

	/// <summary>
	/// Tests the explicit implementation of the <see cref="ICollection.CopyTo"/> method.
	/// The method should throw an exception if the specified start index is out of bounds.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void CopyTo_ICollection_IndexIsOutOfBounds(TestItem[] items, int offset, int count)
	{
		ICollection list = new PartialList<TestItem>(items, offset, count);
		var array = new TestItem[count];
		Assert.Equal("arrayIndex", Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(array, -1)).ParamName);
		Assert.Equal("arrayIndex", Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(array, array.Length + 1)).ParamName);
		if (count > 1) Assert.Null(Assert.Throws<ArgumentException>(() => list.CopyTo(array, 1)).ParamName);
	}

	/// <summary>
	/// Tests the explicit implementation of the <see cref="ICollection.CopyTo"/> method.
	/// The method should throw an exception if the specified array is multidimensional.
	/// </summary>
	[Fact]
	public void CopyTo_ICollection_ArrayIsMultiDimensional()
	{
		var items = new[] { new TestItem(), new TestItem() };
		ICollection list = new PartialList<TestItem>(items);
		var array = new TestItem[1, 1];
		Assert.Equal("array", Assert.Throws<ArgumentException>(() => list.CopyTo(array, 0)).ParamName);
	}

	/// <summary>
	/// Tests the explicit implementation of the <see cref="ICollection.CopyTo"/> method.
	/// The method should throw an exception if the specified array is of an incompatible type.
	/// </summary>
	[Fact]
	public void CopyTo_ICollection_ArrayIsOfIncompatibleType()
	{
		var items = new[] { new TestItem() };
		ICollection list = new PartialList<TestItem>(items);
		int[] array = new int[1];
		Assert.Equal("array", Assert.Throws<ArgumentException>(() => list.CopyTo(array, 0)).ParamName);
	}

	#endregion

	#region GetEnumerator()

	/// <summary>
	/// Tests the <see cref="PartialList{T}.GetEnumerator"/> method.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void GetEnumerator(TestItem[] items, int offset, int count)
	{
		var list = new PartialList<TestItem>(items, offset, count);
		var enumerated = new List<TestItem>();
		IEnumerator<TestItem> enumerator = list.GetEnumerator();
		while (enumerator.MoveNext()) enumerated.Add(enumerator.Current);
		enumerator.Dispose();
		Assert.Equal(count, enumerated.Count);
		Assert.Equal(items.Skip(offset).Take(count), enumerated);
	}

	/// <summary>
	/// Tests the explicit implementation of the <see cref="IEnumerable.GetEnumerator"/> method.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void GetEnumerator_IEnumerable(TestItem[] items, int offset, int count)
	{
		IEnumerable list = new PartialList<TestItem>(items, offset, count);
		var enumerated = new List<TestItem>();
		IEnumerator enumerator = list.GetEnumerator();
		while (enumerator.MoveNext()) enumerated.Add((TestItem)enumerator.Current);
		Assert.Equal(count, enumerated.Count);
		Assert.Equal(items.Skip(offset).Take(count), enumerated);
		(enumerator as IDisposable)!.Dispose();
	}

	#endregion

	#region IndexOf()

	/// <summary>
	/// Tests the <see cref="PartialList{T}.IndexOf"/> method.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void IndexOf(TestItem[] items, int offset, int count)
	{
		var list = new PartialList<TestItem>(items, offset, count);

		// check whether the list contains items in the specified subset
		for (int i = offset; i < offset + count; i++)
		{
			Assert.Equal(i - offset, list.IndexOf(items[i]));
		}

		// check whether the list does not contain a new item
		Assert.Equal(-1, list.IndexOf(new TestItem()));
	}

	/// <summary>
	/// Tests the explicit implementation of the <see cref="IList.IndexOf"/> method.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void IndexOf_IList(TestItem[] items, int offset, int count)
	{
		IList list = new PartialList<TestItem>(items, offset, count);

		// check whether the list contains items in the specified subset
		for (int i = offset; i < offset + count; i++)
		{
			Assert.Equal(i - offset, list.IndexOf(items[i]));
		}

		// check whether the list does not contain a new item
		Assert.Equal(-1, list.IndexOf(new TestItem())); // type of the item is valid, but item is not in the list
		Assert.Equal(-1, list.IndexOf(new object()));   // type of item is invalid
	}

	#endregion

	#region Insert()

	/// <summary>
	/// Tests the explicit implementation of the <see cref="IList{T}.Insert"/> method.
	/// </summary>
	[Fact]
	public void Insert_IListT()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		IList<TestItem> list = new PartialList<TestItem>(items);
		Assert.Throws<NotSupportedException>(() => list.Insert(0, new TestItem()));
	}

	/// <summary>
	/// Tests the explicit implementation of the <see cref="IList.Insert"/> method.
	/// </summary>
	[Fact]
	public void Insert_IList()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		IList list = new PartialList<TestItem>(items);
		Assert.Throws<NotSupportedException>(() => list.Insert(0, new TestItem()));
	}

	#endregion

	#region Remove()

	/// <summary>
	/// Tests the explicit implementation of the <see cref="ICollection{T}.Remove"/> method.
	/// </summary>
	[Fact]
	public void Remove_ICollectionT()
	{
		var items = new[] { new TestItem() };
		ICollection<TestItem> list = new PartialList<TestItem>(items);
		Assert.Throws<NotSupportedException>(() => list.Remove(items[0]));
	}

	/// <summary>
	/// Tests the explicit implementation of the <see cref="IList.Remove"/> method.
	/// </summary>
	[Fact]
	public void Remove_IList()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		IList list = new PartialList<TestItem>(items);
		Assert.Throws<NotSupportedException>(() => list.Remove(items[0]));
	}

	#endregion

	#region RemoveAt()

	/// <summary>
	/// Tests the explicit implementation of the <see cref="IList{T}.RemoveAt"/> method.
	/// </summary>
	[Fact]
	public void RemoveAt_IListT()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		IList<TestItem> list = new PartialList<TestItem>(items);
		Assert.Throws<NotSupportedException>(() => list.RemoveAt(0));
	}

	/// <summary>
	/// Tests the explicit implementation of the <see cref="IList.RemoveAt"/> method.
	/// </summary>
	[Fact]
	public void RemoveAt_IList()
	{
		var items = new[] { new TestItem() };
		// ReSharper disable once CollectionNeverQueried.Local
		IList list = new PartialList<TestItem>(items);
		Assert.Throws<NotSupportedException>(() => list.RemoveAt(0));
	}

	#endregion

	#region ToArray()

	/// <summary>
	/// Tests the <see cref="PartialList{T}.ToArray"/> method.
	/// </summary>
	/// <param name="items">The list to set the partial list upon.</param>
	/// <param name="offset">Offset of in <paramref name="items"/> the partial list should start at.</param>
	/// <param name="count">Number of items the partial list should contain.</param>
	[Theory]
	[MemberData(nameof(TestData))]
	public void ToArray(TestItem[] items, int offset, int count)
	{
		var list = new PartialList<TestItem>(items, offset, count);
		TestItem[] array = [.. list];
		Assert.Equal(count, array.Length);
		Assert.Equal(items.Skip(offset).Take(count), array);
	}

	#endregion

	#region Debug View

	/// <summary>
	/// Tests the debug view of the <see cref="PartialList{T}"/> class.
	/// </summary>
	[Fact]
	public void DebugView()
	{
		var items = new[] { new TestItem(), new TestItem(), new TestItem() };
		var list = new PartialList<TestItem>(items);
		var view = new PartialList<TestItem>.DebugView(list);
		Assert.Equal(items, view.Items);
	}

	#endregion
}
