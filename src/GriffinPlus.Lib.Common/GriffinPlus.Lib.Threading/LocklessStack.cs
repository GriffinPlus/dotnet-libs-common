///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/GriffinPlus/dotnet-libs-common)
//
// Copyright 2018-2019 Sascha Falk <sascha@falk-online.eu>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for
// the specific language governing permissions and limitations under the License.
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
			public T Value;
			public Item NextItem;
			public Item() { }
		};

		private bool mCanGrow;
		private Item mFreeStack;
		private Item mUsedStack;
		private int mCapacity;
		private int mFreeItemCount;
		private int mUsedItemCount;

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
			if (initialCapacity <= 0) {
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
				Item item = new Item();
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
		public bool CanGrow
		{
			get { return mCanGrow; }
		}

		/// <summary>
		/// Gets the number of items the stack accepts before it rejects pushing an item or before it resizes its
		/// internal buffer (depending on the setting specified at construction time).
		/// </summary>
		public int FreeItemCount
		{
			get
			{
				return Interlocked.CompareExchange(ref mFreeItemCount, 0, 0);
			}
		}

		/// <summary>
		/// Gets the number of items on the stack.
		/// </summary>
		public int UsedItemCount
		{
			get
			{
				return Interlocked.CompareExchange(ref mUsedItemCount, 0, 0);
			}
		}

		/// <summary>
		/// Gets the total number of items that can be pushed onto the stack before the stack rejects pushing an item or
		/// before it resizes its internal buffer (depending on the setting specified at construction time).
		/// </summary>
		public int Capacity
		{
			get
			{
				return mCapacity;
			}
		}

		/// <summary>
		/// Pushes an item onto the stack.
		/// </summary>
		/// <param name="element">Element to push onto the stack.</param>
		/// <returns>
		/// true, if the item was successfully pushed onto the stack;
		/// false, if the stack is full and resizing is not allowed.
		/// </returns>
		public bool Push(T element)
		{
			// get item from the 'free' stack
			Item item = null;
			while (true)
			{
				// abort, if no free item left
				item = Interlocked.CompareExchange<Item>(ref mFreeStack, null, null);
				if (item == null && !mCanGrow)
				{
					return false;
				}

				if (item != null)
				{
					// remove the topmost item from the 'free' stack
					Item nextItem = Interlocked.CompareExchange<Item>(ref item.NextItem, null, null);
					if (Interlocked.CompareExchange<Item>(ref mFreeStack, nextItem, item) == item)
					{
						item.NextItem = null;
						Interlocked.Decrement(ref mFreeItemCount);
						break;
					}
				}
				else
				{
					// create item
					item = new Item();
					item.NextItem = null;
					Interlocked.Increment(ref mCapacity);
					break;
				}
			}

			// initialize item
			item.Value = element;

			// push item onto the 'used' stack
			while (true)
			{
				Item firstItem = item.NextItem = Interlocked.CompareExchange<Item>(ref mUsedStack, null, null);
				if (Interlocked.CompareExchange<Item>(ref mUsedStack, item, firstItem) == firstItem)
				{
					Interlocked.Increment(ref mUsedItemCount);
					return true;
				}
			}
		}

		/// <summary>
		/// Pushes an item onto the stack.
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
		public bool Push(T element, out bool first)
		{
			// get item from the 'free' stack
			Item item = null;
			while (true)
			{
				// abort, if no free item left
				item = Interlocked.CompareExchange<Item>(ref mFreeStack, null, null);
				if (item == null && !mCanGrow)
				{
					first = false;
					return false;
				}

				if (item != null)
				{
					// remove the topmost item from the 'free' stack
					Item nextItem = Interlocked.CompareExchange<Item>(ref item.NextItem, null, null);
					if (Interlocked.CompareExchange<Item>(ref mFreeStack, nextItem, item) == item)
					{
						item.NextItem = null;
						Interlocked.Decrement(ref mFreeItemCount);
						break;
					}
				}
				else
				{
					// create item
					item = new Item();
					item.NextItem = null;
					Interlocked.Increment(ref mCapacity);
					break;
				}
			}

			// initialize item
			item.Value = element;

			// push item onto the 'used' stack
			while (true)
			{
				Item firstItem = item.NextItem = Interlocked.CompareExchange<Item>(ref mUsedStack, null, null);
				if (Interlocked.CompareExchange<Item>(ref mUsedStack, item, firstItem) == firstItem)
				{
					first = (firstItem == null);
					return true;
				}
			}
		}

		/// <summary>
		/// Pushes an item onto the stack.
		/// </summary>
		/// <param name="elements">Elements to push onto the stack.</param>
		/// <returns>
		/// true, if all items was successfully pushed onto the stack;
		/// false, if the stack is full and resizing is not allowed.
		/// </returns>
		public bool PushMany(T[] elements)
		{
			Item chainStart = null;
			Item chainEnd = null;
			int elementCount = elements.Length;

			// get item from the 'free' stack
			for (int i = 0; i < elementCount; i++)
			{
				T element = elements[i];

				Item item = null;
				while (true)
				{
					// abort, if no free item left
					item = Interlocked.CompareExchange<Item>(ref mFreeStack, null, null);
					if (item == null && !mCanGrow)
					{
						// stack does not contain enough free blocks
						// => release chain
						item = chainStart;
						while (item != null)
						{
							item.Value = default(T);
							Item next = item.NextItem;

							while (true)
							{
								Item firstItem = item.NextItem = Interlocked.CompareExchange<Item>(ref mFreeStack, null, null);
								if (Interlocked.CompareExchange<Item>(ref mFreeStack, item, firstItem) == firstItem)
								{
									Interlocked.Increment(ref mFreeItemCount);
									break;
								}
							}

							item = next;
						}

						// pushing elements failed...
						return false;
					}

					if (item != null)
					{
						// remove the topmost item from the 'free' stack
						Item nextItem = Interlocked.CompareExchange<Item>(ref item.NextItem, null, null);
						if (Interlocked.CompareExchange<Item>(ref mFreeStack, nextItem, item) == item)
						{
							item.NextItem = null;
							Interlocked.Decrement(ref mFreeItemCount);
							break;
						}
					}
					else
					{
						// create item
						item = new Item();
						item.NextItem = null;
						Interlocked.Increment(ref mCapacity);
						break;
					}
				}

				// initialize item
				item.Value = element;

				// chain the current item with the existing chain
				item.NextItem = chainStart;
				if (chainEnd == null) chainEnd = item;
				chainStart = item;
			}

			// push chain onto the 'used' stack
			while (true)
			{
				Item firstItem = chainEnd.NextItem = Interlocked.CompareExchange<Item>(ref mUsedStack, null, null);
				if (Interlocked.CompareExchange<Item>(ref mUsedStack, chainStart, firstItem) == firstItem)
				{
					for (int i = 0; i < elementCount; i++) Interlocked.Increment(ref mUsedItemCount);
					return true;
				}
			}
		}

		/// <summary>
		/// Pushes multiple items onto the stack.
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
		public bool PushMany(T[] elements, out bool first)
		{
			Item chainStart = null;
			Item chainEnd = null;
			int elementCount = elements.Length;

			// get item from the 'free' stack
			for (int i = 0; i < elementCount; i++)
			{
				T element = elements[i];

				Item item = null;
				while (true)
				{
					// abort, if no free item left
					item = Interlocked.CompareExchange<Item>(ref mFreeStack, null, null);
					if (item == null && !mCanGrow)
					{
						// stack does not contain enough free blocks
						// => release chain
						item = chainStart;
						while (item != null)
						{
							item.Value = default(T);
							Item next = item.NextItem;

							while (true)
							{
								Item firstItem = item.NextItem = Interlocked.CompareExchange<Item>(ref mFreeStack, null, null);
								if (Interlocked.CompareExchange<Item>(ref mFreeStack, item, firstItem) == firstItem)
								{
									Interlocked.Increment(ref mFreeItemCount);
									break;
								}
							}

							item = next;
						}

						// pushing elements failed...
						first = false;
						return false;
					}

					// remove the topmost item from the 'free' stack
					Item nextItem = Interlocked.CompareExchange<Item>(ref item.NextItem, null, null);
					if (Interlocked.CompareExchange<Item>(ref mFreeStack, nextItem, item) == item)
					{
						item.NextItem = null;
						Interlocked.Decrement(ref mFreeItemCount);
						break;
					}
				}

				// create item, if necessary
				if (item == null)
				{
					item = new Item();
					item.NextItem = null;
					Interlocked.Increment(ref mCapacity);
				}

				// initialize item
				item.Value = element;

				// chain the current item with the existing chain
				item.NextItem = chainStart;
				if (chainEnd == null) chainEnd = item;
				chainStart = item;
			}

			// push chain onto the 'used' stack
			while (true)
			{
				Item firstItem = chainEnd.NextItem = Interlocked.CompareExchange<Item>(ref mUsedStack, null, null);
				if (Interlocked.CompareExchange<Item>(ref mUsedStack, chainStart, firstItem) == firstItem)
				{
					for (int i = 0; i < elementCount; i++) Interlocked.Increment(ref mUsedItemCount);
					first = (firstItem == null);
					return true;
				}
			}
		}

		/// <summary>
		/// Pops an item from the stack.
		/// </summary>
		/// <param name="element">Receives the the popped element.</param>
		/// <returns>
		/// true, if the element was popped successfully;
		/// false, if the stack is empty.
		/// </returns>
		public bool Pop(out T element)
		{
			// get item from the 'free' stack
			Item item = null;
			while (true)
			{
				// abort, if no 'used' item left
				item = Interlocked.CompareExchange<Item>(ref mUsedStack, null, null);
				if (item == null)
				{
					element = default(T);
					return false;
				}

				// remove the topmost item from the 'used' stack
				Item nextItem = Interlocked.CompareExchange<Item>(ref item.NextItem, null, null);
				if (Interlocked.CompareExchange<Item>(ref mUsedStack, nextItem, item) == item)
				{
					item.NextItem = null;
					Interlocked.Decrement(ref mUsedItemCount);
					break;
				}
			}

			// initialize returned element
			element = item.Value;

			// push item onto the 'free' stack
			item.Value = default(T);
			while (true)
			{
				Item firstItem = item.NextItem = Interlocked.CompareExchange<Item>(ref mFreeStack, null, null);
				if (Interlocked.CompareExchange<Item>(ref mFreeStack, item, firstItem) == firstItem)
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
			Item firstItem = Interlocked.Exchange<Item>(ref mUsedStack, null);

			int count = 0;
			Item item = firstItem;
			while (item != null)
			{
				Interlocked.Decrement(ref mUsedItemCount);
				item = item.NextItem;
				count++;
			}

			// initialize the array to deliver back to the caller
			item = firstItem;
			T[] result = count > 0 ? new T[count] : null;
			for (int i = 0; i < count; i++)
			{
				result[i] = item.Value;
				Item nextItem = item.NextItem;
				item.Value = default(T);
				while (true)
				{
					firstItem = item.NextItem = Interlocked.CompareExchange<Item>(ref mFreeStack, null, null);
					if (Interlocked.CompareExchange<Item>(ref mFreeStack, item, firstItem) == firstItem)
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
			Item firstItem = Interlocked.Exchange<Item>(ref mUsedStack, null);

			int count = 0;
			Item item = firstItem;
			while (item != null)
			{
				Interlocked.Decrement(ref mUsedItemCount);
				item = item.NextItem;
				count++;
			}

			// initialize the array to deliver back to the caller
			item = firstItem;
			T[] result = count > 0 ? new T[count] : null;
			for (int i = count; i > 0; i--)
			{
				result[i - 1] = item.Value;
				Item nextItem = item.NextItem;
				item.Value = default(T);
				while (true)
				{
					firstItem = item.NextItem = Interlocked.CompareExchange<Item>(ref mFreeStack, null, null);
					if (Interlocked.CompareExchange<Item>(ref mFreeStack, item, firstItem) == firstItem)
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

	}
}
