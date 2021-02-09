///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using Xunit;

namespace GriffinPlus.Lib.Threading
{

	/// <summary>
	/// Unit tests targeting the <see cref="LocklessStack{T}"/> class.
	/// </summary>
	public class LocklessStackTests
	{
		/// <summary>
		/// Tests whether the constructor succeeds creating the stack with valid parameters.
		/// </summary>
		/// <param name="initialCapacity">Initial capacity of the stack (in items).</param>
		/// <param name="canGrow">
		/// true, if the stack can grow on demand;
		/// false, if the stack keeps its capacity and rejects pushing new items, if it is full.
		/// </param>
		[Theory]
		[InlineData(1, false)]
		[InlineData(1, true)]
		[InlineData(10, false)]
		[InlineData(10, true)]
		public void Create_Success(int initialCapacity, bool canGrow)
		{
			var stack = new LocklessStack<int>(initialCapacity, canGrow);
			Assert.Equal(initialCapacity, stack.Capacity);
			Assert.Equal(canGrow, stack.CanGrow);
			Assert.Equal(initialCapacity, stack.FreeItemCount);
			Assert.Equal(0, stack.UsedItemCount);
		}


		/// <summary>
		/// Tests whether the constructor fails on invalid initial capacities.
		/// </summary>
		/// <param name="initialCapacity">Initial capacity of the stack (in items).</param>
		/// <param name="canGrow">
		/// true, if the stack can grow on demand;
		/// false, if the stack keeps its capacity and rejects pushing new items, if it is full.
		/// </param>
		[Theory]
		[InlineData(-1, false)]
		[InlineData(-1, true)]
		public void Create_InvalidCapacity(int initialCapacity, bool canGrow)
		{
			var ex = Assert.Throws<ArgumentOutOfRangeException>(
				() =>
				{
					new LocklessStack<int>(initialCapacity, canGrow);
				});

			Assert.Equal("initialCapacity", ex.ParamName);
		}


		/// <summary>
		/// Tests whether pushing single items works up to the specified capacity and fails, if the stack is full.
		/// </summary>
		/// <param name="capacity">Capacity of the stack to test.</param>
		[Theory]
		[InlineData(1)]
		[InlineData(10)]
		public void Push_NoGrow(int capacity)
		{
			var stack = new LocklessStack<int>(capacity, false);

			// populate the stack
			for (int i = 0; i < capacity; i++)
			{
				Assert.Equal(capacity - i, stack.FreeItemCount);
				Assert.Equal(i, stack.UsedItemCount);
				Assert.True(stack.Push(i));
				Assert.Equal(capacity - i - 1, stack.FreeItemCount);
				Assert.Equal(i + 1, stack.UsedItemCount);
			}

			// pushing another item should fail
			Assert.False(stack.Push(42));
		}


		/// <summary>
		/// Tests whether pushing single items works up to the specified capacity and lets the stack grow on demand
		/// after that point.
		/// </summary>
		/// <param name="capacity">Initial capacity of the stack to test.</param>
		[Theory]
		[InlineData(1)]
		[InlineData(10)]
		public void Push_Grow(int capacity)
		{
			var stack = new LocklessStack<int>(capacity, true);

			// populate the stack
			for (int i = 0; i < capacity; i++)
			{
				Assert.Equal(capacity - i, stack.FreeItemCount);
				Assert.Equal(i, stack.UsedItemCount);
				Assert.True(stack.Push(i));
				Assert.Equal(capacity - i - 1, stack.FreeItemCount);
				Assert.Equal(i + 1, stack.UsedItemCount);
			}

			// pushing another item should let the stack grow
			Assert.Equal(capacity, stack.Capacity);
			Assert.Equal(0, stack.FreeItemCount);
			Assert.Equal(capacity, stack.UsedItemCount);
			Assert.True(stack.Push(42));
			Assert.Equal(capacity + 1, stack.Capacity);
			Assert.Equal(0, stack.FreeItemCount);
			Assert.Equal(capacity + 1, stack.UsedItemCount);
		}


		/// <summary>
		/// Tests whether pushing multiple items works up to the specified capacity and fails, if the stack is full.
		/// </summary>
		/// <param name="capacity">Capacity of the stack to test.</param>
		[Theory]
		[InlineData(1)]
		[InlineData(10)]
		public void PushMany_NoGrow(int capacity)
		{
			// create and populate the stack
			var stack = new LocklessStack<int>(capacity, false);
			PopulateStack(stack, capacity, capacity);

			// pushing another item should fail
			int[] data = { 42 };
			Assert.False(stack.PushMany(data));
		}


		/// <summary>
		/// Tests whether pushing multiple items works up to the specified capacity and lets the stack grow on demand
		/// after that point.
		/// </summary>
		/// <param name="capacity">Initial capacity of the stack to test.</param>
		[Theory]
		[InlineData(1)]
		[InlineData(10)]
		public void PushMany_Grow(int capacity)
		{
			// create and populate the stack
			var stack = new LocklessStack<int>(capacity, true);
			PopulateStack(stack, capacity, capacity);

			// pushing another item should let the stack grow
			int[] data = { 42 };
			Assert.Equal(capacity, stack.Capacity);
			Assert.Equal(0, stack.FreeItemCount);
			Assert.Equal(capacity, stack.UsedItemCount);
			Assert.True(stack.PushMany(data));
			Assert.Equal(capacity + 1, stack.Capacity);
			Assert.Equal(0, stack.FreeItemCount);
			Assert.Equal(capacity + 1, stack.UsedItemCount);
		}


		/// <summary>
		/// Tests popping an item from the stack.
		/// </summary>
		/// <param name="capacity">Capacity of the stack.</param>
		/// <param name="itemCount">Number of items on the stack before popping an item.</param>
		[Theory]
		[InlineData(1, 1)]
		[InlineData(10, 1)]
		[InlineData(10, 2)]
		[InlineData(10, 9)]
		[InlineData(10, 10)]
		public void Pop(int capacity, int itemCount)
		{
			// create and populate the stack
			var stack = new LocklessStack<int>(capacity, true);
			PopulateStack(stack, capacity, itemCount);

			// pop an item from the stack
			int item;
			Assert.True(stack.Pop(out item));
			Assert.Equal(itemCount - 1, item);
			Assert.Equal(capacity - itemCount + 1, stack.FreeItemCount);
			Assert.Equal(itemCount - 1, stack.UsedItemCount);
		}


		/// <summary>
		/// Tests flushing the entire stack.
		/// </summary>
		/// <param name="capacity">Capacity of the stack.</param>
		/// <param name="itemCount">Number of items on the stack before flushing the stack .</param>
		[Theory]
		[InlineData(1, 1)]
		[InlineData(10, 1)]
		[InlineData(10, 2)]
		[InlineData(10, 9)]
		[InlineData(10, 10)]
		public void Flush(int capacity, int itemCount)
		{
			// create and populate the stack
			var stack = new LocklessStack<int>(capacity, true);
			int[] pushedItems = PopulateStack(stack, capacity, itemCount);

			// flush the stack
			int[] items = stack.Flush();
			Assert.Equal(itemCount, items.Length);
			Assert.Equal(pushedItems.Reverse(), items);
			Assert.Equal(capacity, stack.FreeItemCount);
			Assert.Equal(0, stack.UsedItemCount);
		}


		/// <summary>
		/// Tests flushing the entire stack and returning the reverse sequence of items.
		/// </summary>
		/// <param name="capacity">Capacity of the stack.</param>
		/// <param name="itemCount">Number of items on the stack before flushing the stack .</param>
		[Theory]
		[InlineData(1, 1)]
		[InlineData(10, 1)]
		[InlineData(10, 2)]
		[InlineData(10, 9)]
		[InlineData(10, 10)]
		public void FlushAndReverse(int capacity, int itemCount)
		{
			// create and populate the stack
			var stack = new LocklessStack<int>(capacity, true);
			int[] pushedItems = PopulateStack(stack, capacity, itemCount);

			// flush the stack
			int[] items = stack.FlushAndReverse();
			Assert.Equal(itemCount, items.Length);
			Assert.Equal(pushedItems, items);
			Assert.Equal(capacity, stack.FreeItemCount);
			Assert.Equal(0, stack.UsedItemCount);
		}


		/// <summary>
		/// Populates the specified stack with incrementing integer values starting at zero.
		/// </summary>
		/// <param name="stack">Stack to populate.</param>
		/// <param name="capacity">Capacity of the stack (as specified at construction time).</param>
		/// <param name="itemCount">Number of items to push onto the stack.</param>
		/// <returns>The pushed items.</returns>
		private static int[] PopulateStack(LocklessStack<int> stack, int capacity, int itemCount)
		{
			int[] data = new int[itemCount];
			for (int i = 0; i < itemCount; i++) data[i] = i;
			Assert.Equal(capacity, stack.FreeItemCount);
			Assert.Equal(0, stack.UsedItemCount);
			Assert.True(stack.PushMany(data));
			Assert.Equal(capacity - itemCount, stack.FreeItemCount);
			Assert.Equal(itemCount, stack.UsedItemCount);
			return data;
		}
	}

}
