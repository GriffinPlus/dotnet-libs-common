///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

using GriffinPlus.Lib.Caching;

// ReSharper disable PossibleMultipleEnumeration

namespace GriffinPlus.Lib.Collections
{

	/// <summary>
	/// A collection that stores items in an <see cref="IObjectCache"/> allowing them to be collected by the garbage
	/// collection to free memory. Collected items can be reconstructed by the object cache on demand.
	/// </summary>
	/// <typeparam name="T">Type of the items in the collection.</typeparam>
	public partial class ObjectCacheCollection<T> :
		IList<T>,
		IList,
		INotifyCollectionChanged,
		INotifyPropertyChanged
		where T : class
	{
		private readonly IObjectCache                mCache;
		private readonly List<IObjectCacheItem<T>>   mItems;
		private readonly List<IObjectCacheItem<T[]>> mItemPages;
		private readonly int                         mPageSize = 1;
		private          int                         mCount;

		/// <summary>
		/// Occurs when the collection changes
		/// (always executed by the thread raising the event).
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		/// Occurs when a property of the collection changes
		/// (always executed by the thread raising the event).
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectCacheCollection{T}"/> class
		/// (every single item is cached separately).
		/// </summary>
		/// <param name="cache">Object cache to store items in.</param>
		public ObjectCacheCollection(IObjectCache cache)
		{
			mCache = cache;
			mItems = [];
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectCacheCollection{T}"/> class
		/// (items in the collection are grouped to cached pages reducing the number of cache items for performance reasons).
		/// </summary>
		/// <param name="cache">Object cache to store items in.</param>
		/// <param name="pageSize">Maximum number of items to keep in a page.</param>
		public ObjectCacheCollection(IObjectCache cache, int pageSize)
		{
			if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "The page size must be greater than or equal to 1.");

			mCache = cache;
			mPageSize = pageSize;
			if (mPageSize == 1) mItems = [];
			else mItemPages = [];
		}

		/// <summary>
		/// Gets the total number of items in the collection.
		/// </summary>
		public int Count => mCount;

		/// <summary>
		/// Gets a value indicating whether the collection is read-only (always <c>false</c>).
		/// </summary>
		public bool IsReadOnly => false;

		/// <summary>
		/// Gets a value indicating whether the collection size is fixed (always <c>false</c>).
		/// </summary>
		public bool IsFixedSize => false;

		/// <summary>
		/// Gets a value indicating whether the collection is synchronized (always <c>false</c>).
		/// </summary>
		public bool IsSynchronized => false;

		/// <summary>
		/// Gets an object that can be used to synchronize access to the collection (always the collection itself).
		/// </summary>
		public object SyncRoot => this;

		/// <summary>
		/// Gets an enumerator iterating over the collection.
		/// </summary>
		/// <returns>Enumerator iterating over the collection.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			return new Enumerator(this);
		}

		/// <summary>
		/// Gets an enumerator iterating over the collection.
		/// </summary>
		/// <returns>Enumerator iterating over the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		/// <summary>
		/// Gets or sets the item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to get or set.</param>
		/// <returns>The item at the specified index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">The specified index is not a valid index in the collection.</exception>
		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= mCount) throw new ArgumentOutOfRangeException(nameof(index));

				if (mItems != null)
				{
					// without paging
					IObjectCacheItem<T> oci = mItems[index];
					return oci.Value;
				}
				else
				{
					// with paging
					int pageIndex = index / mPageSize;
					int itemIndex = index - pageIndex * mPageSize;
					IObjectCacheItem<T[]> oci = mItemPages[pageIndex];
					return oci.Value[itemIndex];
				}
			}

			set
			{
				if (index < 0 || index >= mCount) throw new ArgumentOutOfRangeException(nameof(index));

				NotifyCollectionChangedEventHandler handler = CollectionChanged;
				var oldItem = default(T);

				if (mItems != null)
				{
					// without paging
					IObjectCacheItem<T> oci = mItems[index];
					if (handler != null) oldItem = oci.Value;
					oci.Value = value;
					if (handler != null)
					{
						var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem);
						OnCollectionChanged(e);
					}
				}
				else
				{
					// with paging
					int pageIndex = index / mPageSize;
					int itemIndex = index - pageIndex * mPageSize;
					IObjectCacheItem<T[]> oci = mItemPages[pageIndex];
					T[] page = oci.Value;
					if (handler != null) oldItem = page[itemIndex];
					page[itemIndex] = value;
					oci.Value = page;
					if (handler != null)
					{
						var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem);
						OnCollectionChanged(e);
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to get or set.</param>
		/// <returns>The item at the specified index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">The specified index is not a valid index in the collection.</exception>
		object IList.this[int index]
		{
			get => this[index];
			set => this[index] = (T)value;
		}

		/// <summary>
		/// Copies the items of the collection to an array, starting at a particular array index.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional array that is the destination of the items copied from the collection.
		/// The array must have zero-based indexing.
		/// </param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		/// <exception cref="ArgumentNullException"><paramref name="array"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
		/// <exception cref="ArgumentException">
		/// The number of items in the source collection is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination
		/// array.
		/// </exception>
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex), "The array index must be positive.");
			if (array.Length <= arrayIndex) throw new ArgumentException("The index is greater than the length of the array.");
			if ((long)array.Length - arrayIndex < mCount) throw new ArgumentException("The number of items in the source collection is greater than the available space from arrayIndex to the end of the destination array.");

			if (mItems != null)
			{
				// without paging
				foreach (IObjectCacheItem<T> item in mItems)
				{
					array[arrayIndex++] = item.Value;
				}
			}
			else
			{
				// with paging
				int remaining = mCount;
				foreach (IObjectCacheItem<T[]> page in mItemPages)
				{
					T[] data = page.Value;
					int itemsToCopy = Math.Min(remaining, data.Length);
					Array.Copy(data, 0, array, arrayIndex, itemsToCopy);
					arrayIndex += itemsToCopy;
					remaining -= itemsToCopy;
				}
			}
		}

		/// <summary>
		/// Copies the items of the collection to an array, starting at a particular array index.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional array that is the destination of the items copied from the collection.
		/// The array must have zero-based indexing.
		/// </param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		/// <exception cref="ArgumentNullException"><paramref name="array"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
		/// <exception cref="ArgumentException">
		/// The number of items in the source collection is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination
		/// array.
		/// </exception>
		public void CopyTo(Array array, int arrayIndex)
		{
			CopyTo((T[])array, arrayIndex);
		}

		/// <summary>
		/// Determines the index of a specific item in the collection (not supported).
		/// </summary>
		/// <param name="item">The object to locate in the collection.</param>
		/// <returns>Always -1.</returns>
		public int IndexOf(T item)
		{
			return -1;
		}

		/// <summary>
		/// Determines the index of a specific item in the collection (not supported).
		/// </summary>
		/// <param name="item">The object to locate in the collection.</param>
		/// <returns>Always <c>-1</c> indicating that the item was not found in the collection.</returns>
		public int IndexOf(object item)
		{
			return IndexOf((T)item);
		}

		/// <summary>
		/// Determines whether the collection contains a specific item (not supported).
		/// </summary>
		/// <param name="item">The object to locate in the collection.</param>
		/// <returns>Always <c>false</c>.</returns>
		public bool Contains(T item)
		{
			return false;
		}

		/// <summary>
		/// Determines whether the collection contains a specific item (not supported).
		/// </summary>
		/// <param name="item">The object to locate in the collection.</param>
		/// <returns>Always <c>false</c>.</returns>
		public bool Contains(object item)
		{
			return Contains((T)item);
		}

		/// <summary>
		/// Adds an item to the collection.
		/// </summary>
		/// <param name="item">The item to add to the collection.</param>
		public void Add(T item)
		{
			AddInternal(item);
		}

		/// <summary>
		/// Adds an item to the collection.
		/// </summary>
		/// <param name="item">The item to add to the collection.</param>
		/// <returns>Index of the item in the collection.</returns>
		public int Add(object item)
		{
			return AddInternal((T)item);
		}

		/// <summary>
		/// Adds an item to the collection (for internal use only).
		/// </summary>
		/// <param name="item">Item to add to the collection.</param>
		/// <returns>Index of the item in the collection.</returns>
		private int AddInternal(T item)
		{
			int index = mCount;

			if (mItems != null)
			{
				// without paging
				IObjectCacheItem<T> oci = mCache.Set(item);
				mItems.Add(oci);
				mCount++;

				if (CollectionChanged != null)
				{
					var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);
					OnCollectionChanged(e);
				}
			}
			else
			{
				// with paging
				int pageIndex = mCount / mPageSize;
				int itemIndex = mCount - pageIndex * mPageSize;

				if (itemIndex == 0)
				{
					var page = new T[1];
					page[0] = item;
					mItemPages.Add(mCache.Set(page));
				}
				else
				{
					T[] page = mItemPages[pageIndex].Value;
					if (itemIndex >= page.Length)
					{
						var newPage = new T[page.Length + 1];
						page.CopyTo(newPage, 0);
						page = newPage;
					}

					page[itemIndex] = item;
					mItemPages[pageIndex].Value = page;
				}

				mCount++;

				if (CollectionChanged != null)
				{
					var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);
					OnCollectionChanged(e);
				}
			}

			OnPropertyChanged("Count");
			OnPropertyChanged("Item[]");

			return index;
		}

		/// <summary>
		/// Adds multiple items to the collection.
		/// </summary>
		/// <param name="items">Items to add.</param>
		public void AddRange(IEnumerable<T> items)
		{
			NotifyCollectionChangedEventHandler handler = CollectionChanged;
			List<T> changedItems = null;
			int startIndex = mCount;

			if (mItems != null)
			{
				// without paging
				if (handler != null) changedItems = items as List<T> ?? [..items];

				foreach (T item in items)
				{
					IObjectCacheItem<T> oci = mCache.Set(item);
					mItems.Add(oci);
					mCount++;
				}

				if (handler != null)
				{
					var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems, startIndex);
					OnCollectionChanged(e);
				}
			}
			else
			{
				// with paging
				if (handler != null) changedItems = items as List<T> ?? [..items];

				T[] currentPage = null;
				int currentPageIndex = -1;
				bool addCurrentPage = false;
				foreach (T item in items)
				{
					int pageIndex = mCount / mPageSize;
					int itemIndex = mCount - pageIndex * mPageSize;

					if (itemIndex == 0)
					{
						// save current page
						if (currentPage != null)
						{
							if (addCurrentPage)
							{
								mItemPages.Add(mCache.Set(currentPage));
							}
							else
							{
								mItemPages[currentPageIndex].Value = currentPage;
							}

							// addCurrentPage = false;
						}

						// create new page
						currentPage = new T[1];
						currentPage[0] = item;
						currentPageIndex = pageIndex;
						addCurrentPage = true;
					}
					else
					{
						// load current page, if necessary
						if (currentPage == null)
						{
							currentPage = mItemPages[pageIndex].Value;
							currentPageIndex = pageIndex;
						}

						if (itemIndex >= currentPage.Length)
						{
							// not enough space
							// => resize page
							var newPage = new T[currentPage.Length + 1];
							currentPage.CopyTo(newPage, 0);
							currentPage = newPage;
						}

						currentPage[itemIndex] = item;
					}

					mCount++;
				}

				// save the last page
				if (currentPage != null)
				{
					if (addCurrentPage)
					{
						mItemPages.Add(mCache.Set(currentPage));
					}
					else
					{
						mItemPages[currentPageIndex].Value = currentPage;
					}

					// addCurrentPage = false;
				}

				if (handler != null && changedItems.Count > 0)
				{
					var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems, startIndex);
					OnCollectionChanged(e);
				}
			}

			OnPropertyChanged("Count");
			OnPropertyChanged("Item[]");
		}

		/// <summary>
		/// Adds multiple items selected from the specified collection to the collection.
		/// </summary>
		/// <param name="collection">Collection containing items to add.</param>
		/// <param name="selector">Selector called per item in <paramref name="collection"/> providing the actual item to add.</param>
		public void AddRange<TSource>(IEnumerable<TSource> collection, Func<TSource, T> selector)
		{
			NotifyCollectionChangedEventHandler handler = CollectionChanged;
			List<T> changedItems = handler != null ? [] : null;
			int startIndex = mCount;

			if (mItems != null)
			{
				// without paging
				foreach (TSource element in collection)
				{
					T item = selector(element);
					changedItems?.Add(item);
					IObjectCacheItem<T> oci = mCache.Set(item);
					mItems.Add(oci);
					mCount++;
				}

				if (handler != null)
				{
					var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems, startIndex);
					OnCollectionChanged(e);
				}
			}
			else
			{
				// with paging
				foreach (TSource element in collection)
				{
					T item = selector(element);
					int pageIndex = mCount / mPageSize;
					int itemIndex = mCount - pageIndex * mPageSize;

					if (itemIndex == 0)
					{
						var page = new T[1];
						page[0] = item;
						mItemPages.Add(mCache.Set(page));
						changedItems?.Add(item);
					}
					else
					{
						T[] page = mItemPages[pageIndex].Value;
						if (itemIndex >= page.Length)
						{
							var newPage = new T[page.Length + 1];
							page.CopyTo(newPage, 0);
							page = newPage;
						}

						page[itemIndex] = item;
						mItemPages[pageIndex].Value = page;
						changedItems?.Add(item);
					}

					mCount++;
				}

				if (handler != null && changedItems!.Count > 0)
				{
					var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems, startIndex);
					OnCollectionChanged(e);
				}
			}

			OnPropertyChanged("Count");
			OnPropertyChanged("Item[]");
		}

		/// <summary>
		/// Inserts an item into the collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which item should be inserted.</param>
		/// <param name="item">The object to insert into the collection.</param>
		/// <exception cref="NotSupportedException">
		/// Inserting items into the middle of the collection is not supported, if the collection is running in paging
		/// mode.
		/// </exception>
		public void Insert(int index, T item)
		{
			if (mItems != null)
			{
				// without paging
				IObjectCacheItem<T> oci = mCache.Set(item);
				mItems.Insert(index, oci);
				mCount++;
			}
			else
			{
				// with paging
				if (index < mCount) throw new NotSupportedException("Inserting items into the middle of the collection is not supported, if the collection is running in paging mode.");
				AddInternal(item);
			}
		}

		/// <summary>
		/// Inserts an item into the collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which item should be inserted.</param>
		/// <param name="item">The object to insert into the collection.</param>
		public void Insert(int index, object item)
		{
			Insert(index, (T)item);
		}

		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">The specified index does not refer to a valid item in the collection.</exception>
		/// <exception cref="NotSupportedException">
		/// Removing items from the middle of the collection is not supported, if the collection is running in paging
		/// mode.
		/// </exception>
		public void RemoveAt(int index)
		{
			if (index < 0 || index >= mCount)
				throw new ArgumentOutOfRangeException(nameof(index), "The specified index does not refer to a valid item in the collection.");

			NotifyCollectionChangedEventHandler handler = CollectionChanged;
			var oldItem = default(T);

			if (mItems != null)
			{
				// without paging
				IObjectCacheItem<T> oci = mItems[index];
				if (handler != null) oldItem = oci.Value;
				mItems.RemoveAt(index);
				mCount--;
				if (handler != null)
				{
					var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index);
					OnCollectionChanged(e);
				}

				oci.Dispose();
			}
			else
			{
				// with paging
				if (index < mCount) throw new NotSupportedException("Removing items from the middle of the collection is not supported, if the collection is running in paging mode.");
				int pageIndex = index / mPageSize;
				int itemIndex = index - pageIndex * mPageSize;
				T[] page = mItemPages[pageIndex].Value;
				if (handler != null) oldItem = page[itemIndex];

				if (itemIndex == 0)
				{
					// page is empty now
					// => remove page
					mItemPages[pageIndex].Dispose();
					mItemPages.RemoveAt(pageIndex);
				}
				else
				{
					// page still contains items
					// => update page
					page[itemIndex] = default(T);
					mItemPages[pageIndex].Value = page;
				}

				mCount--;

				if (handler != null)
				{
					var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index);
					OnCollectionChanged(e);
				}
			}

			OnPropertyChanged("Count");
			OnPropertyChanged("Item[]");
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the collection (not supported).
		/// </summary>
		/// <param name="item">The object to remove from the collection.</param>
		/// <returns>Always false.</returns>
		public bool Remove(T item)
		{
			return false;
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the collection (not supported).
		/// </summary>
		/// <param name="item">The object to remove from the collection.</param>
		/// <returns>Always false.</returns>
		public void Remove(object item)
		{
			Remove((T)item);
		}

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		public void Clear()
		{
			if (mItems != null)
			{
				// without paging
				foreach (IObjectCacheItem<T> oci in mItems) oci.Dispose();
				mItems.Clear();
			}
			else
			{
				// with paging
				foreach (IObjectCacheItem<T[]> oci in mItemPages) oci.Dispose();
				mItemPages.Clear();
			}

			mCount = 0;

			if (CollectionChanged != null)
			{
				var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
				OnCollectionChanged(e);
			}

			OnPropertyChanged("Count");
			OnPropertyChanged("Item[]");
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="name">Name of the property that has changed.</param>
		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		/// <summary>
		/// Raises the <see cref="CollectionChanged"/> event.
		/// </summary>
		/// <param name="e">Event arguments to pass to registered event handlers.</param>
		protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler handler = CollectionChanged;
			handler?.Invoke(this, e);
		}
	}

}
