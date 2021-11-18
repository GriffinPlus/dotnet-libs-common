///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace GriffinPlus.Lib.Collections
{

	/// <summary>
	/// A read-only list that provides a certain object a specific number of times.
	/// </summary>
	/// <remarks>
	/// The collection can come in handy when optimizing collections that implement the <see cref="INotifyCollectionChanged"/>
	/// interface. These collections can then use the <see cref="FixedItemReadOnlyList{T}"/> to provide lists with dummy items when
	/// notifying about items that are added, moved or removed, if it is known that the actual items are not needed by the
	/// event recipient.
	/// </remarks>
	public sealed partial class FixedItemReadOnlyList<T> : IList<T>, IReadOnlyList<T>, IList
	{
		#region Member Variables

		private readonly T mItem;

		#endregion

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="FixedItemReadOnlyList{T}"/> class.
		/// </summary>
		/// <param name="item">Item the collection should contain.</param>
		/// <param name="count">Number of times the item should occur in the collection.</param>
		public FixedItemReadOnlyList(T item, int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count), count, "The number of items must be positive.");

			mItem = item;
			Count = count;
		}

		#endregion

		#region Count : Direct

		/// <summary>
		/// Gets the number of items in the collection.
		/// </summary>
		public int Count { get; }

		#endregion

		#region IsFixedSize : IList

		/// <summary>
		/// Gets a value indicating whether the collection has a fixed size.
		/// Always <c>true</c>.
		/// </summary>
		bool IList.IsFixedSize => true;

		#endregion

		#region IsReadOnly : ICollection<T>, IList

		/// <summary>
		/// Gets a value indicating whether the collection is read-only.
		/// Always <c>true</c>.
		/// </summary>
		bool ICollection<T>.IsReadOnly => true;

		/// <summary>
		/// Gets a value indicating whether the collection is read-only.
		/// Always <c>true</c>.
		/// </summary>
		bool IList.IsReadOnly => true;

		#endregion

		#region IsSynchronized : ICollection

		/// <summary>
		/// Gets a value indicating whether the collection is synchronized.
		/// Always <c>false</c>.
		/// </summary>
		bool ICollection.IsSynchronized => false;

		#endregion

		#region SyncRoot : ICollection

		/// <summary>
		/// Gets the synchronization root of the collection.
		/// </summary>
		object ICollection.SyncRoot => this;

		#endregion

		#region this[] : Direct, IList<T>, IList

		/// <summary>
		/// Gets the item at the specified index.
		/// </summary>
		/// <param name="index">Index of the item to get.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of bounds.</exception>
		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
				return mItem;
			}
		}

		/// <summary>
		/// Gets the item at the specified index.
		/// Setting is not supported.
		/// </summary>
		/// <param name="index">Index of the item to get.</param>
		/// <returns>The item at the specified index.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of bounds.</exception>
		T IList<T>.this[int index]
		{
			get
			{
				if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
				return mItem;
			}
			set => throw new NotSupportedException("The collection is read-only.");
		}

		/// <summary>
		/// Gets the item at the specified index.
		/// Setting is not supported.
		/// </summary>
		/// <param name="index">Index of the item to get.</param>
		/// <returns>The item at the specified index.</returns>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		object IList.this[int index]
		{
			get => this[index];
			set => throw new NotSupportedException("The collection is read-only.");
		}

		#endregion

		#region Add() : ICollection<T>, IList

		/// <summary>
		/// Adds an item to the collection (not supported).
		/// </summary>
		/// <param name="item">Item to add.</param>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		void ICollection<T>.Add(T item) => throw new NotSupportedException("The collection is read-only.");

		/// <summary>
		/// Adds an item to the collection (not supported).
		/// </summary>
		/// <param name="item">Item to add.</param>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		int IList.Add(object item) => throw new NotSupportedException("The collection is read-only.");

		#endregion

		#region Clear() : ICollection<T>, IList

		/// <summary>
		/// Removes all items from the collection (not supported).
		/// </summary>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		void ICollection<T>.Clear() => throw new NotSupportedException("The collection is read-only.");

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		void IList.Clear() => throw new NotSupportedException("The collection is read-only.");

		#endregion

		#region Contains() : Direct, IList

		/// <summary>
		/// Checks whether the collection contains the specified item.
		/// </summary>
		/// <param name="item">Item to check for.</param>
		/// <returns>
		/// <c>true</c> if the collection contains the item;
		/// otherwise <c>false</c>.
		/// </returns>
		public bool Contains(T item)
		{
			return EqualityComparer<T>.Default.Equals(mItem, item);
		}

		/// <summary>
		/// Checks whether the collection contains the specified item.
		/// </summary>
		/// <param name="item">Item to check for.</param>
		/// <returns>
		/// <c>true</c> if the collection contains the item;
		/// otherwise <c>false</c>.
		/// </returns>
		bool IList.Contains(object item) => Contains((T)item);

		#endregion

		#region CopyTo() : Direct, ICollection

		/// <summary>
		/// Copies all items into the specified array.
		/// </summary>
		/// <param name="array">Array to copy the items into.</param>
		/// <param name="arrayIndex">Index in the array to start copying to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="array"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="array"/> is no a one-dimensional array or the array is too small to store all items.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is out of bounds.</exception>
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex), "The array index is negative.");
			if (arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex), "The array index is outside the specified array.");
			if (Count > array.Length - arrayIndex) throw new ArgumentException("The specified array is too small to receive all items.");

			for (int i = 0; i < Count; i++)
			{
				array[i] = mItem;
			}
		}

		/// <summary>
		/// Copies all items into the specified array.
		/// </summary>
		/// <param name="array">Array to copy the items into.</param>
		/// <param name="arrayIndex">Index in the array to start copying to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="array"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="array"/> is no a one-dimensional array or the array is too small to store all items.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is out of bounds.</exception>
		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex), "The array index is negative.");
			if (array.Rank != 1) throw new ArgumentException("The specified array is multi-dimensional.", nameof(array));
			if (arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex), "The array index is outside the specified array.");
			if (Count > array.Length - arrayIndex) throw new ArgumentException("The specified array is too small to receive all items.");

			for (int i = 0; i < Count; i++)
			{
				array.SetValue(mItem, i);
			}
		}

		#endregion

		#region GetEnumerator() : Direct, IEnumerable

		/// <summary>
		/// Gets an enumerator iterating over the collection.
		/// </summary>
		/// <returns>An enumerator.</returns>
		public IEnumerator<T> GetEnumerator() => new Enumerator(this);

		/// <summary>
		/// Gets an enumerator iterating over the collection.
		/// </summary>
		/// <returns>An enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

		#endregion

		#region IndexOf() : Direct, IList

		/// <summary>
		/// Gets the index of the specified item.
		/// </summary>
		/// <param name="item">Item to locate in the collection.</param>
		/// <returns>
		/// Index of the item;
		/// -1, if the specified item is not in the collection.
		/// </returns>
		public int IndexOf(T item)
		{
			return EqualityComparer<T>.Default.Equals(mItem, item) ? 0 : -1;
		}

		/// <summary>
		/// Gets the index of the specified item.
		/// </summary>
		/// <param name="item">Item to locate in the collection.</param>
		/// <returns>
		/// Index of the item;
		/// -1, if the specified item is not in the collection.
		/// </returns>
		int IList.IndexOf(object item) => IndexOf((T)item);

		#endregion

		#region Insert() : IList<T>, IList

		/// <summary>
		/// Inserts an item at the specified position (not supported).
		/// </summary>
		/// <param name="index">The zero-based index at which the item should be inserted.</param>
		/// <param name="item">The item to insert into the collection.</param>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		void IList<T>.Insert(int index, T item) => throw new NotSupportedException("The collection is read-only.");

		/// <summary>
		/// Inserts an item at the specified position (not supported).
		/// </summary>
		/// <param name="index">The zero-based index at which the item should be inserted.</param>
		/// <param name="item">The item to insert into the collection.</param>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		void IList.Insert(int index, object item) => throw new NotSupportedException("The collection is read-only.");

		#endregion

		#region Remove() : ICollection<T>, IList

		/// <summary>
		/// Removes the specified item from the collection (not supported).
		/// </summary>
		/// <param name="item">Item to remove from the collection.</param>
		/// <returns>
		/// <c>true</c> if the item was removed;
		/// otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		bool ICollection<T>.Remove(T item) => throw new NotSupportedException("The collection is read-only.");

		/// <summary>
		/// Removes the specified item from the collection (not supported).
		/// </summary>
		/// <param name="item">Item to remove from the collection.</param>
		/// <returns>
		/// <c>true</c> if the item was removed;
		/// otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		void IList.Remove(object item) => throw new NotSupportedException("The collection is read-only.");

		#endregion

		#region RemoveAt() : IList<T>, IList

		/// <summary>
		/// Removes the item at the specified index (not supported).
		/// </summary>
		/// <param name="index">Index of the item to remove.</param>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		void IList<T>.RemoveAt(int index) => throw new NotSupportedException("The collection is read-only.");

		/// <summary>
		/// Removes the item at the specified index (not supported).
		/// </summary>
		/// <param name="index">Index of the item to remove.</param>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		void IList.RemoveAt(int index) => throw new NotSupportedException("The collection is read-only.");

		#endregion
	}

}
