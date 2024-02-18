///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
//
// This file incorporates work covered by the following copyright and permission notice:
//
//     MIT License
//
//     Copyright (c) 2016 Stephen Cleary
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in all
//     copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//     SOFTWARE.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace GriffinPlus.Lib.Collections
{

	/// <summary>
	/// A double-ended queue (deque), which provides O(1) indexed access, O(1) removals from the front and back, amortized
	/// O(1) insertions to the front and back, and O(N) insertions and removals anywhere else (with the operations getting
	/// slower as the index approaches the middle).
	/// </summary>
	/// <typeparam name="T">The type of elements contained in the deque.</typeparam>
	[DebuggerDisplay("Count = {Count}, Capacity = {Capacity}")]
	[DebuggerTypeProxy(typeof(Deque<>.DebugView))]
	public sealed class Deque<T> : IList<T>, IReadOnlyList<T>, IList
	{
		/// <summary>
		/// The default capacity.
		/// </summary>
		private const int DefaultCapacity = 8;

		/// <summary>
		/// The circular buffer that holds the view.
		/// </summary>
		private T[] mBuffer;

		/// <summary>
		/// The offset into <see cref="mBuffer"/> where the view begins.
		/// </summary>
		private int mOffset;

		/// <summary>
		/// Initializes a new instance of the <see cref="Deque{T}"/> class with the specified capacity.
		/// </summary>
		/// <param name="capacity">The initial capacity (must be greater than <c>0</c>).</param>
		public Deque(int capacity)
		{
			if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must not be negative.");
			mBuffer = new T[capacity];
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Deque{T}"/> class with the elements from the specified collection.
		/// </summary>
		/// <param name="collection">The collection (must not be <c>null</c>).</param>
		public Deque(IEnumerable<T> collection)
		{
			if (collection == null) throw new ArgumentNullException(nameof(collection));

			IReadOnlyCollection<T> source = CollectionHelpers.ReifyCollection(collection);
			int count = source.Count;
			if (count > 0)
			{
				mBuffer = new T[count];
				DoInsertRange(0, source);
			}
			else
			{
				mBuffer = new T[DefaultCapacity];
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Deque{T}"/> class.
		/// </summary>
		public Deque()
			: this(DefaultCapacity) { }

		#region Implementation of IList<T>

		/// <summary>
		/// Gets a value indicating whether this list is read-only.
		/// This implementation always returns <c>false</c>.
		/// </summary>
		/// <returns>
		/// true if this list is read-only;
		/// otherwise false.
		/// </returns>
		bool ICollection<T>.IsReadOnly => false;

		/// <summary>
		/// Gets or sets the item at the specified index.
		/// </summary>
		/// <param name="index">The index of the item to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is not a valid index in this list.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// This property is set and the list is read-only.
		/// </exception>
		public T this[int index]
		{
			get
			{
				CheckExistingIndexArgument(Count, index);
				return DoGetItem(index);
			}

			set
			{
				CheckExistingIndexArgument(Count, index);
				DoSetItem(index, value);
			}
		}

		/// <summary>
		/// Inserts an item to this list at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert into this list.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is not a valid index in this list.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// This list is read-only.
		/// </exception>
		public void Insert(int index, T item)
		{
			CheckNewIndexArgument(Count, index);
			DoInsert(index, item);
		}

		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is not a valid index in this list.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// This list is read-only.
		/// </exception>
		public void RemoveAt(int index)
		{
			CheckExistingIndexArgument(Count, index);
			DoRemoveAt(index);
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

			int index = 0;
			foreach (T sourceItem in this)
			{
				if (comparer.Equals(item, sourceItem))
					return index;

				++index;
			}

			return -1;
		}

		/// <summary>
		/// Adds an item to the end of this list.
		/// </summary>
		/// <param name="item">The object to add to this list.</param>
		/// <exception cref="NotSupportedException">
		/// This list is read-only.
		/// </exception>
		void ICollection<T>.Add(T item)
		{
			DoInsert(Count, item);
		}

		/// <summary>
		/// Determines whether this list contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in this list.</param>
		/// <returns>
		/// true if <paramref name="item"/> is found in this list;
		/// otherwise false.
		/// </returns>
		bool ICollection<T>.Contains(T item)
		{
			var comparer = EqualityComparer<T>.Default;

			foreach (T entry in this)
			{
				if (comparer.Equals(item, entry))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Copies the elements of this list to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from this slice.
		/// The <see cref="Array"/> must have zero-based indexing.
		/// </param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is null.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is less than 0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
		/// -or-
		/// The number of elements in the source <see cref="ICollection{T}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end
		/// of the destination <paramref name="array"/>.
		/// </exception>
		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			int count = Count;
			CheckRangeArguments(array.Length, arrayIndex, count);
			CopyToArray(array, arrayIndex);
		}

		/// <summary>
		/// Copies the deque elements into an array.
		/// The resulting array always has all the deque elements contiguously.
		/// </summary>
		/// <param name="array">The destination array.</param>
		/// <param name="arrayIndex">The optional index in the destination array at which to begin writing.</param>
		private void CopyToArray(Array array, int arrayIndex = 0)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));

			if (IsSplit)
			{
				// The existing buffer is split, so we have to copy it in parts
				int length = Capacity - mOffset;
				Array.Copy(mBuffer, mOffset, array, arrayIndex, length);
				Array.Copy(mBuffer, 0, array, arrayIndex + length, Count - length);
			}
			else
			{
				// The existing buffer is whole
				Array.Copy(mBuffer, mOffset, array, arrayIndex, Count);
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from this list.
		/// </summary>
		/// <param name="item">The object to remove from this list.</param>
		/// <returns>
		/// true, if <paramref name="item"/> was successfully removed from the list;
		/// false, if the list does not contain <paramref name="item"/>.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// This list is read-only.
		/// </exception>
		public bool Remove(T item)
		{
			int index = IndexOf(item);
			if (index == -1)
				return false;

			DoRemoveAt(index);
			return true;
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator()
		{
			int count = Count;
			for (int i = 0; i != count; ++i)
			{
				yield return DoGetItem(i);
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

		#endregion

		#region Implementation of IList

		private static bool IsT(object value)
		{
			if (value is T) return true;
			if (value != null) return false;
			return default(T) == null;
		}

		int IList.Add(object value)
		{
			if (value == null && default(T) != null)
				throw new ArgumentNullException(nameof(value), "Value cannot be null.");

			if (!IsT(value))
				throw new ArgumentException("Value is of incorrect type.", nameof(value));

			AddToBack((T)value);
			return Count - 1;
		}

		bool IList.Contains(object value)
		{
			return IsT(value) && ((ICollection<T>)this).Contains((T)value);
		}

		int IList.IndexOf(object value)
		{
			return IsT(value) ? IndexOf((T)value) : -1;
		}

		void IList.Insert(int index, object value)
		{
			if (value == null && default(T) != null)
				throw new ArgumentNullException(nameof(value), "Value cannot be null.");

			if (!IsT(value))
				throw new ArgumentException("Value is of incorrect type.", nameof(value));

			Insert(index, (T)value);
		}

		bool IList.IsFixedSize => false;

		bool IList.IsReadOnly => false;

		void IList.Remove(object value)
		{
			if (IsT(value))
				Remove((T)value);
		}

		object IList.this[int index]
		{
			get => this[index];

			set
			{
				if (value == null && default(T) != null)
					throw new ArgumentNullException(nameof(value), "Value cannot be null.");

				if (!IsT(value))
					throw new ArgumentException("Value is of incorrect type.", nameof(value));

				this[index] = (T)value;
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array), "Destination array cannot be null.");

			CheckRangeArguments(array.Length, index, Count);

			try
			{
				CopyToArray(array, index);
			}
			catch (ArrayTypeMismatchException ex)
			{
				throw new ArgumentException("Destination array is of incorrect type.", nameof(array), ex);
			}
			catch (RankException ex)
			{
				throw new ArgumentException("Destination array must be single dimensional.", nameof(array), ex);
			}
		}

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => this;

		#endregion

		#region Generic List Helpers

		/// <summary>
		/// Checks the <paramref name="index"/> argument to see, if it refers to a valid insertion point in a source of a given length.
		/// </summary>
		/// <param name="sourceLength">The length of the source (not checked for validity).</param>
		/// <param name="index">The index into the source.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is not a valid index to an insertion point for the source.
		/// </exception>
		private static void CheckNewIndexArgument(int sourceLength, int index)
		{
			if (index < 0 || index > sourceLength)
			{
				throw new ArgumentOutOfRangeException(
					nameof(index),
					$"Invalid new index {index} for source length {sourceLength}");
			}
		}

		/// <summary>
		/// Checks the <paramref name="index"/> argument to see if it refers to an existing element in a source of a given length.
		/// </summary>
		/// <param name="sourceLength">The length of the source (not checked for validity).</param>
		/// <param name="index">The index into the source.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is not a valid index to an existing element for the source.
		/// </exception>
		private static void CheckExistingIndexArgument(int sourceLength, int index)
		{
			if (index < 0 || index >= sourceLength)
			{
				throw new ArgumentOutOfRangeException(
					nameof(index),
					$"Invalid existing index {index} for source length {sourceLength}");
			}
		}

		/// <summary>
		/// Checks the <paramref name="offset"/> and <paramref name="count"/> arguments for validity when applied to a source of a given length.
		/// Allows 0-element ranges, including a 0-element range at the end of the source.
		/// </summary>
		/// <param name="sourceLength">The length of the source (not checked for validity).</param>
		/// <param name="offset">The index into source at which the range begins.</param>
		/// <param name="count">The number of elements in the range.</param>
		/// <exception cref="ArgumentOutOfRangeException">Either <paramref name="offset"/> or <paramref name="count"/> is less than 0.</exception>
		/// <exception cref="ArgumentException">The range [offset, offset + count) is not within the range [0, sourceLength).</exception>
		private static void CheckRangeArguments(int sourceLength, int offset, int count)
		{
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset), $"Invalid offset {offset}");
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), $"Invalid count {count}");
			}

			if (sourceLength - offset < count)
			{
				throw new ArgumentException($"Invalid offset ({offset}) or count ({count}) for source length ({sourceLength})");
			}
		}

		#endregion

		/// <summary>
		/// Gets a value indicating whether this instance is empty.
		/// </summary>
		private bool IsEmpty => Count == 0;

		/// <summary>
		/// Gets a value indicating whether this instance is at full capacity.
		/// </summary>
		private bool IsFull => Count == Capacity;

		/// <summary>
		/// Gets a value indicating whether the buffer is "split" (meaning the beginning of the view is at a later index in <see cref="mBuffer"/> than the end).
		/// </summary>
		private bool IsSplit => mOffset > Capacity - Count; // overflow-safe version of "(mOffset + Count) > Capacity"

		/// <summary>
		/// Gets or sets the capacity for this deque.
		/// This value must always be greater than zero, and this property cannot be set to a value less than <see cref="Count"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// <see cref="Capacity"/> cannot be set to a value less than <see cref="Count"/>.
		/// </exception>
		public int Capacity
		{
			get => mBuffer.Length;

			set
			{
				if (value < Count) throw new ArgumentOutOfRangeException(nameof(value), "Capacity cannot be set to a value less than Count");
				if (value == mBuffer.Length) return;

				// Create the new InternalBuffer and copy our existing range.
				var newBuffer = new T[value];
				CopyToArray(newBuffer);

				// Set up to use the new InternalBuffer.
				mBuffer = newBuffer;
				mOffset = 0;
			}
		}

		/// <summary>
		/// Gets the number of elements contained in this deque.
		/// </summary>
		/// <returns>The number of elements contained in this deque.</returns>
		public int Count { get; private set; }

		/// <summary>
		/// Applies the offset to <paramref name="index"/>, resulting in a buffer index.
		/// </summary>
		/// <param name="index">The deque index.</param>
		/// <returns>The buffer index.</returns>
		private int DequeIndexToBufferIndex(int index)
		{
			return (index + mOffset) % Capacity;
		}

		/// <summary>
		/// Gets an element at the specified view index.
		/// </summary>
		/// <param name="index">
		/// The zero-based view index of the element to get.
		/// This index is guaranteed to be valid.
		/// </param>
		/// <returns>The element at the specified index.</returns>
		private T DoGetItem(int index)
		{
			return mBuffer[DequeIndexToBufferIndex(index)];
		}

		/// <summary>
		/// Sets an element at the specified view index.
		/// </summary>
		/// <param name="index">
		/// The zero-based view index of the element to get.
		/// This index is guaranteed to be valid.
		/// </param>
		/// <param name="item">The element to store in the list.</param>
		private void DoSetItem(int index, T item)
		{
			mBuffer[DequeIndexToBufferIndex(index)] = item;
		}

		/// <summary>
		/// Inserts an element at the specified view index.
		/// </summary>
		/// <param name="index">
		/// The zero-based view index at which the element should be inserted.
		/// This index is guaranteed to be valid.
		/// </param>
		/// <param name="item">The element to store in the list.</param>
		private void DoInsert(int index, T item)
		{
			EnsureCapacityForOneElement();

			if (index == 0)
			{
				DoAddToFront(item);
				return;
			}

			if (index == Count)
			{
				DoAddToBack(item);
				return;
			}

			DoInsertRange(index, new[] { item });
		}

		/// <summary>
		/// Removes an element at the specified view index.
		/// </summary>
		/// <param name="index">
		/// The zero-based view index of the element to remove.
		/// This index is guaranteed to be valid.
		/// </param>
		private void DoRemoveAt(int index)
		{
			if (index == 0)
			{
				DoRemoveFromFront();
				return;
			}

			if (index == Count - 1)
			{
				DoRemoveFromBack();
				return;
			}

			DoRemoveRange(index, 1);
		}

		/// <summary>
		/// Increments <see cref="mOffset"/> by <paramref name="value"/> using modulo-<see cref="Capacity"/> arithmetic.
		/// </summary>
		/// <param name="value">
		/// The value by which to increase <see cref="mOffset"/>.
		/// Must not be negative.
		/// </param>
		/// <returns>The value of <see cref="mOffset"/> after it was incremented.</returns>
		private int PostIncrement(int value)
		{
			int result = mOffset;
			mOffset += value;
			mOffset %= Capacity;
			return result;
		}

		/// <summary>
		/// Decrements <see cref="mOffset"/> by <paramref name="value"/> using modulo-<see cref="Capacity"/> arithmetic.
		/// </summary>
		/// <param name="value">
		/// The value by which to reduce <see cref="mOffset"/>.
		/// May not be negative or greater than <see cref="Capacity"/>.
		/// </param>
		/// <returns>The value of <see cref="mOffset"/> before it was decremented.</returns>
		private int PreDecrement(int value)
		{
			mOffset -= value;
			if (mOffset < 0) mOffset += Capacity;
			return mOffset;
		}

		/// <summary>
		/// Inserts a single element to the back of the view.
		/// <see cref="IsFull"/> must be false when this method is called.
		/// </summary>
		/// <param name="value">The element to insert.</param>
		private void DoAddToBack(T value)
		{
			mBuffer[DequeIndexToBufferIndex(Count)] = value;
			++Count;
		}

		/// <summary>
		/// Inserts a single element to the front of the view.
		/// <see cref="IsFull"/> must be false when this method is called.
		/// </summary>
		/// <param name="value">The element to insert.</param>
		private void DoAddToFront(T value)
		{
			mBuffer[PreDecrement(1)] = value;
			++Count;
		}

		/// <summary>
		/// Removes and returns the last element in the view.
		/// <see cref="IsEmpty"/> must be false when this method is called.
		/// </summary>
		/// <returns>The former last element.</returns>
		private T DoRemoveFromBack()
		{
			T ret = mBuffer[DequeIndexToBufferIndex(Count - 1)];
			--Count;
			return ret;
		}

		/// <summary>
		/// Removes and returns the first element in the view.
		/// <see cref="IsEmpty"/> must be false when this method is called.
		/// </summary>
		/// <returns>The former first element.</returns>
		private T DoRemoveFromFront()
		{
			--Count;
			return mBuffer[PostIncrement(1)];
		}

		/// <summary>
		/// Inserts a range of elements into the view.
		/// </summary>
		/// <param name="index">The index into the view at which the elements are to be inserted.</param>
		/// <param name="collection">
		/// The elements to insert.
		/// The sum of <c>collection.Count</c> and <see cref="Count"/> must be less than or equal to <see cref="Capacity"/>.
		/// </param>
		private void DoInsertRange(int index, IReadOnlyCollection<T> collection)
		{
			int collectionCount = collection.Count;

			// make room in the existing list
			if (index < Count / 2)
			{
				// Inserting into the first half of the list

				// Move lower items down: [0, index) -> [Capacity - collectionCount, Capacity - collectionCount + index)
				// This clears out the low "index" number of items, moving them "collectionCount" places down;
				//   after rotation, there will be a "collectionCount"-sized hole at "index".
				int copyCount = index;
				int writeIndex = Capacity - collectionCount;
				for (int j = 0; j != copyCount; ++j)
				{
					mBuffer[DequeIndexToBufferIndex(writeIndex + j)] = mBuffer[DequeIndexToBufferIndex(j)];
				}

				// Rotate to the new view
				PreDecrement(collectionCount);
			}
			else
			{
				// Inserting into the second half of the list

				// Move higher items up: [index, count) -> [index + collectionCount, collectionCount + count)
				int copyCount = Count - index;
				int writeIndex = index + collectionCount;
				for (int j = copyCount - 1; j != -1; --j)
				{
					mBuffer[DequeIndexToBufferIndex(writeIndex + j)] = mBuffer[DequeIndexToBufferIndex(index + j)];
				}
			}

			// Copy new items into place
			int i = index;
			foreach (T item in collection)
			{
				mBuffer[DequeIndexToBufferIndex(i)] = item;
				++i;
			}

			// Adjust valid count
			Count += collectionCount;
		}

		/// <summary>
		/// Removes a range of elements from the view.
		/// </summary>
		/// <param name="index">The index into the view at which the range begins.</param>
		/// <param name="collectionCount">
		/// The number of elements in the range.
		/// This must be greater than 0 and less than or equal to <see cref="Count"/>.
		/// </param>
		private void DoRemoveRange(int index, int collectionCount)
		{
			if (index == 0)
			{
				// Removing from the beginning: rotate to the new view
				PostIncrement(collectionCount);
				Count -= collectionCount;
				return;
			}

			if (index == Count - collectionCount)
			{
				// Removing from the ending: trim the existing view
				Count -= collectionCount;
				return;
			}

			if (index + collectionCount / 2 < Count / 2)
			{
				// Removing from first half of list

				// Move lower items up: [0, index) -> [collectionCount, collectionCount + index)
				int copyCount = index;
				int writeIndex = collectionCount;
				for (int j = copyCount - 1; j != -1; --j)
				{
					mBuffer[DequeIndexToBufferIndex(writeIndex + j)] = mBuffer[DequeIndexToBufferIndex(j)];
				}

				// Rotate to new view
				PostIncrement(collectionCount);
			}
			else
			{
				// Removing from second half of list

				// Move higher items down: [index + collectionCount, count) -> [index, count - collectionCount)
				int copyCount = Count - collectionCount - index;
				int readIndex = index + collectionCount;
				for (int j = 0; j != copyCount; ++j)
				{
					mBuffer[DequeIndexToBufferIndex(index + j)] = mBuffer[DequeIndexToBufferIndex(readIndex + j)];
				}
			}

			// Adjust valid count
			Count -= collectionCount;
		}

		/// <summary>
		/// Doubles the capacity if necessary to make room for one more element.
		/// When this method returns, <see cref="IsFull"/> is false.
		/// </summary>
		private void EnsureCapacityForOneElement()
		{
			if (IsFull)
			{
				Capacity = Capacity == 0 ? 1 : Capacity * 2;
			}
		}

		/// <summary>
		/// Inserts a single element at the back of this deque.
		/// </summary>
		/// <param name="value">The element to insert.</param>
		public void AddToBack(T value)
		{
			EnsureCapacityForOneElement();
			DoAddToBack(value);
		}

		/// <summary>
		/// Inserts a single element at the front of this deque.
		/// </summary>
		/// <param name="value">The element to insert.</param>
		public void AddToFront(T value)
		{
			EnsureCapacityForOneElement();
			DoAddToFront(value);
		}

		/// <summary>
		/// Inserts a collection of elements into this deque.
		/// </summary>
		/// <param name="index">The index at which the collection is inserted.</param>
		/// <param name="collection">The collection of elements to insert.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is not a valid index to an insertion point for the source.
		/// </exception>
		public void InsertRange(int index, IEnumerable<T> collection)
		{
			CheckNewIndexArgument(Count, index);
			IReadOnlyCollection<T> source = CollectionHelpers.ReifyCollection(collection);
			int collectionCount = source.Count;

			// Overflow-safe check for "Count + collectionCount > Capacity"
			if (collectionCount > Capacity - Count)
			{
				Capacity = checked(Count + collectionCount);
			}

			if (collectionCount == 0)
			{
				return;
			}

			DoInsertRange(index, source);
		}

		/// <summary>
		/// Removes a range of elements from this deque.
		/// </summary>
		/// <param name="offset">The index into the deque at which the range begins.</param>
		/// <param name="count">The number of elements to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Either <paramref name="offset"/> or <paramref name="count"/> is less than 0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The range [<paramref name="offset"/>, <paramref name="offset"/> + <paramref name="count"/>) is not within the range [0, <see cref="Count"/>).
		/// </exception>
		public void RemoveRange(int offset, int count)
		{
			CheckRangeArguments(Count, offset, count);
			if (count == 0) return;
			DoRemoveRange(offset, count);
		}

		/// <summary>
		/// Removes and returns the last element of this deque.
		/// </summary>
		/// <returns>The former last element.</returns>
		/// <exception cref="InvalidOperationException">The deque is empty.</exception>
		public T RemoveFromBack()
		{
			if (IsEmpty) throw new InvalidOperationException("The deque is empty.");
			return DoRemoveFromBack();
		}

		/// <summary>
		/// Removes and returns the first element of this deque.
		/// </summary>
		/// <returns>The former first element.</returns>
		/// <exception cref="InvalidOperationException">The deque is empty.</exception>
		public T RemoveFromFront()
		{
			if (IsEmpty) throw new InvalidOperationException("The deque is empty.");
			return DoRemoveFromFront();
		}

		/// <summary>
		/// Removes all items from this deque.
		/// </summary>
		public void Clear()
		{
			mOffset = 0;
			Count = 0;
		}

		/// <summary>
		/// Creates and returns a new array containing the elements in this deque.
		/// </summary>
		public T[] ToArray()
		{
			var result = new T[Count];
			((ICollection<T>)this).CopyTo(result, 0);
			return result;
		}

		[DebuggerNonUserCode]
		private sealed class DebugView(Deque<T> deque)
		{
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			// ReSharper disable once UnusedMember.Local
			public T[] Items => deque.ToArray();
		}
	}

}
