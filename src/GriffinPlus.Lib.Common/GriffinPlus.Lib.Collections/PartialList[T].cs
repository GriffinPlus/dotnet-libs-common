///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace GriffinPlus.Lib.Collections;

/// <summary>
/// A read-only subset of another collection implementing the <see cref="IList{T}"/> interface.
/// </summary>
/// <typeparam name="T">Type of the list items.</typeparam>
[DebuggerDisplay("Count = {" + nameof(Count) + "})]")]
[DebuggerTypeProxy(typeof(PartialList<>.DebugView))]
public sealed class PartialList<T> : IList<T>, IReadOnlyList<T>, IList
{
	private readonly int      mInitialOffset;
	private readonly IList<T> mList;

	/// <summary>
	/// Initializes a new instance of the <see cref="PartialList{T}"/> class taking an entire specified list
	/// effectively providing a read-only wrapper for the list.
	/// </summary>
	/// <param name="list">
	/// List to wrap (must not change afterward as the <see cref="PartialList{T}"/> directly refers to this list).
	/// </param>
	/// <exception cref="ArgumentNullException">The <paramref name="list"/> is <c>null.</c></exception>
	public PartialList(IList<T> list)
	{
		mList = list ?? throw new ArgumentNullException(nameof(list));
		mInitialOffset = 0;
		Count = list.Count;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PartialList{T}"/> class providing a subset of the specified list
	/// with the specified number of items starting at the specified index.
	/// </summary>
	/// <param name="list">
	/// List to wrap (must not change afterward as the <see cref="PartialList{T}"/> directly refers to this list).
	/// </param>
	/// <param name="offset">Index in the list to start at.</param>
	/// <param name="count">Number of items the subset should contain.</param>
	/// <exception cref="ArgumentNullException">The <paramref name="list"/> is <c>null.</c></exception>
	/// <exception cref="ArgumentOutOfRangeException">The specified <paramref name="offset"/>  or <paramref name="count"/> is negative.</exception>
	/// <exception cref="ArgumentException">The range [offset, offset + count) is not within the range [0, list.Count).</exception>
	public PartialList(IList<T> list, int offset, int count)
	{
		if (list == null) throw new ArgumentNullException(nameof(list));
		if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), $"The offset ({offset}) must be positive.");
		if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), $"The count ({count}) must be positive.");
		if (list.Count - offset < count) throw new ArgumentException($"Invalid offset ({offset}) or count ({count}) for source length ({list.Count})");

		mList = list;
		mInitialOffset = offset;
		Count = count;
	}

	/// <summary>
	/// Gets the number of elements contained in this list.
	/// </summary>
	/// <returns>The number of elements contained in this list.</returns>
	public int Count { get; }

	/// <summary>
	/// Gets a value indicating whether this list has a fixed size.
	/// This implementation always returns <c>true</c>.
	/// </summary>
	/// <returns>Always <c>true</c>.</returns>
	bool IList.IsFixedSize => true;

	/// <summary>
	/// Gets a value indicating whether this list is read-only.
	/// This implementation always returns <c>true</c>.
	/// </summary>
	/// <returns>Always <c>true</c>.</returns>
	bool ICollection<T>.IsReadOnly => true;

	/// <summary>
	/// Gets a value indicating whether this list is read-only.
	/// This implementation always returns <c>true</c>.
	/// </summary>
	/// <returns>Always <c>true</c>.</returns>
	bool IList.IsReadOnly => true;

	/// <summary>
	/// Gets a value indicating whether access to the list is synchronized (thread safe).
	/// This implementation always returns <c>false</c>.
	/// </summary>
	/// <returns>Always <c>false</c>.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>
	/// Gets an object that can be used to synchronize access to the list.
	/// </summary>
	object ICollection.SyncRoot => this;

	/// <summary>
	/// Gets the item at the specified index.
	/// </summary>
	/// <param name="index">The index of the item to get or set.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in this list.</exception>
	public T this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException(
					nameof(index),
					$"The index ({index}) is out of bounds for the list length ({Count}).");
			}

			return mList[mInitialOffset + index];
		}
	}

	/// <summary>
	/// Gets the item at the specified index (setting is not supported).
	/// </summary>
	/// <param name="index">The index of the item to get or set.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in this list.</exception>
	/// <exception cref="NotSupportedException">This property is set and the list is read-only.</exception>
	T IList<T>.this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException(
					nameof(index),
					$"The index ({index}) is out of bounds for the list length ({Count}).");
			}

			return mList[mInitialOffset + index];
		}

		set => throw new NotSupportedException("This list is read-only.");
	}

	/// <summary>
	/// Gets the item at the specified index (setting is not supported).
	/// </summary>
	/// <param name="index">The index of the item to get or set.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in this list.</exception>
	/// <exception cref="NotSupportedException">This property is set and the list is read-only.</exception>
	object IList.this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException(
					nameof(index),
					$"The index ({index}) is out of bounds for the list length ({Count}).");
			}

			return mList[mInitialOffset + index];
		}

		set => throw new NotSupportedException("This list is read-only.");
	}

	/// <summary>
	/// Adds an item to the end of this list (not supported).
	/// </summary>
	/// <param name="item">The object to add to this list.</param>
	/// <exception cref="NotSupportedException">This list is read-only.</exception>
	void ICollection<T>.Add(T item)
	{
		throw new NotSupportedException("This list is read-only.");
	}

	/// <summary>
	/// Adds an item to the end of this list (not supported).
	/// </summary>
	/// <param name="item">The object to add to this list.</param>
	/// <returns>
	/// The position into which the new element was inserted,
	/// or -1 to indicate that the item was not inserted into the collection.
	/// </returns>
	/// <exception cref="NotSupportedException">This list is read-only.</exception>
	int IList.Add(object item)
	{
		throw new NotSupportedException("This list is read-only.");
	}

	/// <summary>
	/// Removes all items from this list (not supported).
	/// </summary>
	/// <exception cref="NotSupportedException">This list is read-only.</exception>
	void ICollection<T>.Clear()
	{
		throw new NotSupportedException("This list is read-only.");
	}

	/// <summary>
	/// Removes all items from this list (not supported).
	/// </summary>
	/// <exception cref="NotSupportedException">This list is read-only.</exception>
	void IList.Clear()
	{
		throw new NotSupportedException("This list is read-only.");
	}

	/// <summary>
	/// Determines whether this list contains a specific value.
	/// </summary>
	/// <param name="item">The object to locate in this list.</param>
	/// <returns>
	/// <c>true</c> if <paramref name="item"/> is found in this list;
	/// otherwise <c>false</c>.
	/// </returns>
	public bool Contains(T item)
	{
		return IndexOf(item) >= 0;
	}

	/// <summary>
	/// Determines whether this list contains a specific value.
	/// </summary>
	/// <param name="item">The object to locate in this list.</param>
	/// <returns>
	/// <c>true</c> if <paramref name="item"/> is found in this list;
	/// otherwise <c>false</c>.
	/// </returns>
	bool IList.Contains(object item)
	{
		if (item is T value)
			return IndexOf(value) >= 0;

		return false;
	}

	/// <summary>
	/// Copies the elements of this list to an <see cref="Array"/>, starting at a particular array index.
	/// </summary>
	/// <param name="array">
	/// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from this slice.
	/// The <see cref="Array"/> must have zero-based indexing.
	/// </param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
	/// <exception cref="ArgumentNullException"><paramref name="array"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
	/// <exception cref="ArgumentException">
	/// The number of items in the source collection is greater than the available space from <paramref name="arrayIndex"/>
	/// to the end of the destination array.
	/// </exception>
	public void CopyTo(T[] array, int arrayIndex)
	{
		if (array == null) throw new ArgumentNullException(nameof(array));
		if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex), "The array index must be positive.");
		if (arrayIndex == 0 && Count == 0) return;
		if (arrayIndex >= array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex), "The index is greater than the length of the array.");
		if ((long)array.Length - arrayIndex < Count) throw new ArgumentException("The number of items in the source collection is greater than the available space from arrayIndex to the end of the destination array.");

		for (int i = 0; i < Count; i++)
		{
			array[arrayIndex + i] = mList[mInitialOffset + i];
		}
	}

	/// <summary>
	/// Copies the elements of this list to an <see cref="Array"/>, starting at a particular array index.
	/// </summary>
	/// <param name="array">
	/// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from this slice.
	/// The <see cref="Array"/> must have zero-based indexing.
	/// </param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
	/// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
	/// -or-
	/// The number of elements in the source <see cref="ICollection"/> is greater than the available space from <paramref name="arrayIndex"/>
	/// to the end of the destination <paramref name="array"/>.
	/// </exception>
	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		if (array == null) throw new ArgumentNullException(nameof(array));
		if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex), "The array index must be positive.");
		if (array.Rank != 1) throw new ArgumentException("The specified array is multi-dimensional.", nameof(array));
		if (arrayIndex == 0 && Count == 0) return;
		if (arrayIndex >= array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex), "The index is greater than the length of the array.");
		if ((long)array.Length - arrayIndex < Count) throw new ArgumentException("The number of items in the source collection is greater than the available space from arrayIndex to the end of the destination array.");

		try
		{
			for (int i = 0; i < Count; i++)
			{
				array.SetValue(mList[mInitialOffset + i], arrayIndex + i); // throws InvalidCastException in case of type mismatch
			}
		}
		catch (InvalidCastException ex)
		{
			throw new ArgumentException("Destination array is of incorrect type.", nameof(array), ex);
		}
	}

	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	/// <returns>
	/// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
	/// </returns>
	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i != Count; ++i)
		{
			yield return mList[mInitialOffset + i];
		}
	}

	/// <summary>
	/// Returns an enumerator that iterates through a collection.
	/// </summary>
	/// <returns>
	/// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
	/// </returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>
	/// Determines the index of a specific item in this list.
	/// </summary>
	/// <param name="item">The object to locate in this list.</param>
	/// <returns>
	/// The index of <paramref name="item"/> if found in this list;
	/// otherwise -1.
	/// </returns>
	public int IndexOf(T item)
	{
		var comparer = EqualityComparer<T>.Default;

		for (int index = 0; index != Count; ++index)
		{
			T sourceItem = mList[mInitialOffset + index];
			if (comparer.Equals(item, sourceItem))
			{
				return index;
			}
		}

		return -1;
	}

	/// <summary>
	/// Determines the index of a specific item in this list.
	/// </summary>
	/// <param name="item">The object to locate in this list.</param>
	/// <returns>
	/// The index of <paramref name="item"/> if found in this list;
	/// otherwise -1.
	/// </returns>
	int IList.IndexOf(object item)
	{
		if (item is T value)
			return IndexOf(value);

		return -1;
	}

	/// <summary>
	/// Inserts an item into this list at the specified index (not supported).
	/// </summary>
	/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
	/// <param name="item">The object to insert into this list.</param>
	/// <exception cref="NotSupportedException">This list is read-only.</exception>
	void IList<T>.Insert(int index, T item)
	{
		throw new NotSupportedException("This list is read-only.");
	}

	/// <summary>
	/// Inserts an item into this list at the specified index (not supported).
	/// </summary>
	/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
	/// <param name="item">The object to insert into this list.</param>
	/// <exception cref="NotSupportedException">This list is read-only.</exception>
	void IList.Insert(int index, object item)
	{
		throw new NotSupportedException("This list is read-only.");
	}

	/// <summary>
	/// Removes the first occurrence of a specific object from this list.
	/// </summary>
	/// <param name="item">The object to remove from this list.</param>
	/// <returns>
	/// <c>true</c> if <paramref name="item"/> was successfully removed from the list;
	/// <c>false</c> if the list does not contain <paramref name="item"/>.
	/// </returns>
	/// <exception cref="NotSupportedException">This list is read-only.</exception>
	bool ICollection<T>.Remove(T item)
	{
		throw new NotSupportedException("This list is read-only.");
	}

	/// <summary>
	/// Removes the first occurrence of a specific object from this list.
	/// </summary>
	/// <param name="item">The object to remove from this list.</param>
	/// <exception cref="NotSupportedException">This list is read-only.</exception>
	void IList.Remove(object item)
	{
		throw new NotSupportedException("This list is read-only.");
	}

	/// <summary>
	/// Removes the item at the specified index (not supported).
	/// </summary>
	/// <param name="index">The zero-based index of the item to remove.</param>
	/// <exception cref="NotSupportedException">This list is read-only.</exception>
	void IList<T>.RemoveAt(int index)
	{
		throw new NotSupportedException("This list is read-only.");
	}

	/// <summary>
	/// Removes the item at the specified index (not supported).
	/// </summary>
	/// <param name="index">The zero-based index of the item to remove.</param>
	/// <exception cref="NotSupportedException">This list is read-only.</exception>
	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException("This list is read-only.");
	}

	/// <summary>
	/// Gets an array containing all elements in the list.
	/// </summary>
	/// <returns>All items contained in the list.</returns>
	public T[] ToArray()
	{
		var array = new T[Count];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = mList[mInitialOffset + i];
		}

		return array;
	}

	#region Debug View

	[DebuggerNonUserCode]
	internal sealed class DebugView(PartialList<T> list)
	{
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		// ReSharper disable once UnusedMember.Local
		public T[] Items => list.ToArray();
	}

	#endregion
}
