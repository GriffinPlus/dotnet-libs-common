///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace GriffinPlus.Lib.Threading
{

	/// <summary>
	/// A thread-safe implementation of a stack using non-blocking interlocked operations.
	/// </summary>
	public class LocklessStack<T>
	{
		private class Item
		{
			public T    Value;
			public Item NextItem;
		}

		private readonly bool mCanGrow;
		private          Item mFreeStack;
		private          Item mUsedStack;
		private          int  mCapacity;
		private          int  mFreeItemCount;
		private          int  mUsedItemCount;

		/// <summary>
		/// Initializes a new instance of the <see cref="LocklessStack{T}"/> class.
		/// </summary>
		/// <param name="initialCapacity">Maximum number of items the stack can store.</param>
		/// <param name="growOnDemand">
		/// true to allow resizing, if the number of items exceeds the specified capacity when pushing an item onto the stack;
		/// false to reject pushing the new item.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">The initial capacity is negative or zero.</exception>
		public LocklessStack(int initialCapacity, bool growOnDemand)
		{
			if (initialCapacity <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(initialCapacity), "The capacity must be greater than 0.");
			}

			mCapacity = initialCapacity;
			mCanGrow = growOnDemand;
			mFreeItemCount = initialCapacity;
			mUsedItemCount = 0;

			// init 'free' stack
			Item previousItem = null;
			for (int i = 0; i < mCapacity; i++)
			{
				var item = new Item();
				if (previousItem != null)
				{
					previousItem.NextItem = item;
				}
				else
				{
					mFreeStack = item;
				}

				previousItem = item;
			}

			// init 'used' stack
			mUsedStack = null;
		}

		/// <summary>
		/// Gets a value indicating whether the stack can grow, if necessary.
		/// </summary>
		public bool CanGrow => mCanGrow;

		/// <summary>
		/// Gets the number of items the stack accepts before it rejects pushing an item or before it resizes its
		/// internal buffer (depending on the setting specified at construction time).
		/// </summary>
		public int FreeItemCount => Interlocked.CompareExchange(ref mFreeItemCount, 0, 0);

		/// <summary>
		/// Gets the number of items on the stack.
		/// </summary>
		public int UsedItemCount => Interlocked.CompareExchange(ref mUsedItemCount, 0, 0);

		/// <summary>
		/// Gets the total number of items that can be pushed onto the stack before the stack rejects pushing an item or
		/// before it resizes its internal buffer (depending on the setting specified at construction time).
		/// </summary>
		public int Capacity => mCapacity;

		/// <summary>
		/// Tries to push an item onto the stack.
		/// </summary>
		/// <param name="element">Element to push onto the stack.</param>
		/// <returns>
		/// true, if the item was successfully pushed onto the stack;
		/// false, if the stack is full and resizing is not allowed.
		/// </returns>
		public bool TryPush(T element)
		{
			// get item from the 'free' stack
			Item item;
			while (true)
			{
				// abort, if no free item left
				item = Interlocked.CompareExchange(ref mFreeStack, null, null);
				if (item == null && !mCanGrow)
				{
					return false;
				}

				if (item != null)
				{
					// remove the topmost item from the 'free' stack
					var nextItem = Interlocked.CompareExchange(ref item.NextItem, null, null);
					if (Interlocked.CompareExchange(ref mFreeStack, nextItem, item) == item)
					{
						item.NextItem = null;
						Interlocked.Decrement(ref mFreeItemCount);
						break;
					}
				}
				else
				{
					// create item
					item = new Item { NextItem = null };
					Interlocked.Increment(ref mCapacity);
					break;
				}
			}

			// initialize item
			item.Value = element;

			// push item onto the 'used' stack
			while (true)
			{
				var firstItem = item.NextItem = Interlocked.CompareExchange(ref mUsedStack, null, null);
				if (Interlocked.CompareExchange(ref mUsedStack, item, firstItem) == firstItem)
				{
					Interlocked.Increment(ref mUsedItemCount);
					return true;
				}
			}
		}

		/// <summary>
		/// Tries tp push an item onto the stack.
		/// </summary>
		/// <param name="element">Element to push onto the stack.</param>
		/// <param name="first">
		/// Receives 'true', if this is the first element pushed onto the stack;
		/// otherwise 'false' (only valid, if the method returns with 'true').
		/// </param>
		/// <returns>
		/// true, if the item was successfully pushed onto the stack;
		/// false, if the stack is full and resizing is not allowed.
		/// </returns>
		public bool TryPush(T element, out bool first)
		{
			// get item from the 'free' stack
			var item = GetFreeItem();

			// abort, if the stack is full and growing is not allowed
			if (item == null)
			{
				first = false;
				return false;
			}

			// initialize item
			item.Value = element;

			// push item onto the 'used' stack
			while (true)
			{
				var firstItem = item.NextItem = Interlocked.CompareExchange(ref mUsedStack, null, null);
				if (Interlocked.CompareExchange(ref mUsedStack, item, firstItem) == firstItem)
				{
					first = firstItem == null;
					return true;
				}
			}
		}

		/// <summary>
		/// Tries to atomically push multiple items onto the stack.
		/// </summary>
		/// <param name="elements">Elements to push onto the stack.</param>
		/// <returns>
		/// true, if all items was successfully pushed onto the stack;
		/// false, if the stack is full and resizing is not allowed.
		/// </returns>
		public bool TryPushMany(T[] elements)
		{
			return TryPushMany(elements, out _);
		}

		/// <summary>
		/// Tries to atomically push multiple items onto the stack.
		/// </summary>
		/// <param name="elements">Elements to push onto the stack.</param>
		/// <param name="first">
		/// Receives 'true', if this is the first element pushed onto the stack;
		/// otherwise 'false' (only valid, if the method returns with 'true').
		/// </param>
		/// <returns>
		/// true, if all items were successfully pushed onto the stack;
		/// false, if the stack is full and resizing is not allowed.
		/// </returns>
		public bool TryPushMany(T[] elements, out bool first)
		{
			// ensure the specified array is not null
			if (elements == null)
				throw new ArgumentNullException(nameof(elements));

			// ensure the specified array contains at least one item
			int elementCount = elements.Length;
			if (elementCount == 0)
				throw new ArgumentException("The specified array does not contain any items.", nameof(elements));

			// get items from the free stack
			var chain = GetFreeItems(elementCount);
			if (chain == null)
			{
				first = false;
				return false;
			}

			// populate items with specified elements in reverse order, so they appear in the correct order on the stack
			var chainStart = chain;
			var chainEnd = chain;
			var item = chain;
			for (int i = elementCount - 1; i >= 0; i--)
			{
				item.Value = elements[i];
				chainEnd = item;
				item = item.NextItem;
			}

			// push chain onto the 'used' stack
			while (true)
			{
				var firstItem = chainEnd.NextItem = Interlocked.CompareExchange(ref mUsedStack, null, null);
				if (Interlocked.CompareExchange(ref mUsedStack, chainStart, firstItem) == firstItem)
				{
					for (int i = 0; i < elementCount; i++) Interlocked.Increment(ref mUsedItemCount);
					first = firstItem == null;
					return true;
				}
			}
		}

		/// <summary>
		/// Tries to pop an item from the stack.
		/// </summary>
		/// <param name="element">Receives the the popped element.</param>
		/// <returns>
		/// true, if the element was popped successfully;
		/// false, if the stack is empty.
		/// </returns>
		public bool TryPop(out T element)
		{
			// get item from the 'free' stack
			Item item;
			while (true)
			{
				// abort, if no 'used' item left
				item = Interlocked.CompareExchange(ref mUsedStack, null, null);
				if (item == null)
				{
					element = default;
					return false;
				}

				// remove the topmost item from the 'used' stack
				var nextItem = Interlocked.CompareExchange(ref item.NextItem, null, null);
				if (Interlocked.CompareExchange(ref mUsedStack, nextItem, item) == item)
				{
					item.NextItem = null;
					Interlocked.Decrement(ref mUsedItemCount);
					break;
				}
			}

			// initialize returned element
			element = item.Value;

			// push item onto the 'free' stack
			item.Value = default;
			while (true)
			{
				var firstItem = item.NextItem = Interlocked.CompareExchange(ref mFreeStack, null, null);
				if (Interlocked.CompareExchange(ref mFreeStack, item, firstItem) == firstItem)
				{
					Interlocked.Increment(ref mFreeItemCount);
					return true;
				}
			}
		}

		/// <summary>
		/// Flushes the stack returning all elements that are currently on the stack in an array.
		/// </summary>
		/// <returns>
		/// All elements that are currently on the stack (top-most element at index 0);
		/// null, if the stack is empty.
		/// </returns>
		public T[] Flush()
		{
			var firstItem = Interlocked.Exchange(ref mUsedStack, null);

			int count = 0;
			var item = firstItem;
			while (item != null)
			{
				Interlocked.Decrement(ref mUsedItemCount);
				item = item.NextItem;
				count++;
			}

			// abort, if there are no items on the stack
			if (count == 0)
				return null;

			// initialize the array to deliver back to the caller
			item = firstItem;
			var result = new T[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = item.Value;
				var nextItem = item.NextItem;
				item.Value = default;
				while (true)
				{
					firstItem = item.NextItem = Interlocked.CompareExchange(ref mFreeStack, null, null);
					if (Interlocked.CompareExchange(ref mFreeStack, item, firstItem) == firstItem)
					{
						Interlocked.Increment(ref mFreeItemCount);
						break;
					}
				}

				// proceed with the next item
				item = nextItem;
			}

			return result;
		}

		/// <summary>
		/// Flushes the stack returning all elements that are currently on the stack in an array reversing the order of the elements.
		/// </summary>
		/// <returns>
		/// All elements that are currently on the stack (lowest element at index 0);
		/// null, if the stack is empty.
		/// </returns>
		public T[] FlushAndReverse()
		{
			var firstItem = Interlocked.Exchange(ref mUsedStack, null);

			int count = 0;
			var item = firstItem;
			while (item != null)
			{
				Interlocked.Decrement(ref mUsedItemCount);
				item = item.NextItem;
				count++;
			}

			// abort, if there are no items on the stack
			if (count == 0)
				return null;

			// initialize the array to deliver back to the caller
			item = firstItem;
			var result = new T[count];
			for (int i = count; i > 0; i--)
			{
				result[i - 1] = item.Value;
				var nextItem = item.NextItem;
				item.Value = default;
				while (true)
				{
					firstItem = item.NextItem = Interlocked.CompareExchange(ref mFreeStack, null, null);
					if (Interlocked.CompareExchange(ref mFreeStack, item, firstItem) == firstItem)
					{
						Interlocked.Increment(ref mFreeItemCount);
						break;
					}
				}

				// proceed with the next item
				item = nextItem;
			}

			return result;
		}

		/// <summary>
		/// Gets an item from the free stack, creates a new item, if growing is allowed.
		/// </summary>
		/// <returns>
		/// An item from the free stack;
		/// null, if the free stack is empty and growing is not allowed.
		/// </returns>
		private Item GetFreeItem()
		{
			// remove the topmost item from the 'free' stack
			Item item;
			while (true)
			{
				item = Interlocked.CompareExchange(ref mFreeStack, null, null);
				if (item == null) break;
				var nextItem = Interlocked.CompareExchange(ref item.NextItem, null, null);
				if (Interlocked.CompareExchange(ref mFreeStack, nextItem, item) == item)
				{
					item.NextItem = null;
					Interlocked.Decrement(ref mFreeItemCount);
					return item;
				}
			}

			// no item on the free stack and growing is not allowed => abort
			if (!mCanGrow)
				return null;

			// create item
			item = new Item { NextItem = null };
			Interlocked.Increment(ref mCapacity);
			return item;
		}

		/// <summary>
		/// Gets the specified number of items from the free stack, creates a new items, if growing is allowed.
		/// </summary>
		/// <param name="count">Number of items to get.</param>
		/// <returns>
		/// The first item in the chain of free items;
		/// null, if the free stack does not contain enough items and growing is not allowed.
		/// </returns>
		private Item GetFreeItems(int count)
		{
			Item chainStart = null;
			Item chainEnd = null;
			int chainLength = 0;

			for (int i = 0; i < count; i++)
			{
				// remove the topmost item from the 'free' stack
				Item item;
				while (true)
				{
					item = Interlocked.CompareExchange(ref mFreeStack, null, null);
					if (item == null) break;
					var nextItem = Interlocked.CompareExchange(ref item.NextItem, null, null);
					if (Interlocked.CompareExchange(ref mFreeStack, nextItem, item) == item)
					{
						item.NextItem = null;
						Interlocked.Decrement(ref mFreeItemCount);
						if (chainStart == null) chainStart = item;
						if (chainEnd != null) chainEnd.NextItem = item;
						chainEnd = item;
						chainLength++;
						break;
					}
				}

				if (item == null)
				{
					// no item on the free stack and resizing is not allowed
					// => abort
					if (!mCanGrow)
					{
						// push already fetched items back onto the free stack
						if (chainEnd != null)
						{
							while (true)
							{
								var firstItem = chainEnd.NextItem = Interlocked.CompareExchange(ref mFreeStack, null, null);
								if (Interlocked.CompareExchange(ref mFreeStack, chainStart, firstItem) == firstItem)
								{
									Interlocked.Add(ref mFreeItemCount, chainLength);
									break;
								}
							}
						}

						return null;
					}

					// create item
					item = new Item { NextItem = null };
					if (chainStart == null) chainStart = item;
					if (chainEnd != null) chainEnd.NextItem = item;
					chainEnd = item;
					chainLength++;
					Interlocked.Increment(ref mCapacity);
				}
			}

			return chainStart;
		}
	}

}
