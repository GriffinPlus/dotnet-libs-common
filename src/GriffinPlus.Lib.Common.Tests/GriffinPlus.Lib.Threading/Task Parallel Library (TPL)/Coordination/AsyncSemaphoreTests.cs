///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
//
// This file incorporates work covered by the following copyright and permission notice:
//
//     MIT License
//
//     Copyright (c) 2019 Stephen Cleary
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
using System.Threading;
using System.Threading.Tasks;

using GriffinPlus.Lib.Tests;

using Xunit;

// ReSharper disable MethodHasAsyncOverload

namespace GriffinPlus.Lib.Threading
{

	[Collection(nameof(NoParallelizationCollection))]
	public class AsyncSemaphoreTests
	{
		[Fact]
		public async Task WaitAsync_NoSlotsAvailable_IsNotCompleted()
		{
			var semaphore = new AsyncSemaphore(0);
			Assert.Equal(0, semaphore.CurrentCount);
			Task task = semaphore.WaitAsync();
			Assert.Equal(0, semaphore.CurrentCount);
			await AsyncAssert.DoesNotCompleteAsync(task);
		}

		[Fact]
		public async Task WaitAsync_SlotAvailable_IsCompleted()
		{
			var semaphore = new AsyncSemaphore(1);
			Assert.Equal(1, semaphore.CurrentCount);
			Task task1 = semaphore.WaitAsync();
			Assert.Equal(0, semaphore.CurrentCount);
			Assert.True(task1.IsCompleted);
			Task task2 = semaphore.WaitAsync();
			Assert.Equal(0, semaphore.CurrentCount);
			await AsyncAssert.DoesNotCompleteAsync(task2);
		}

		[Fact]
		public void WaitAsync_PreCancelled_SlotAvailable_SucceedsSynchronously()
		{
			var semaphore = new AsyncSemaphore(1);
			Assert.Equal(1, semaphore.CurrentCount);
			var token = new CancellationToken(true);

			Task task = semaphore.WaitAsync(token);

			Assert.Equal(0, semaphore.CurrentCount);
			Assert.True(task.IsCompleted);
			Assert.False(task.IsCanceled);
			Assert.False(task.IsFaulted);
		}

		[Fact]
		public void WaitAsync_PreCancelled_NoSlotAvailable_CancelsSynchronously()
		{
			var semaphore = new AsyncSemaphore(0);
			Assert.Equal(0, semaphore.CurrentCount);
			var token = new CancellationToken(true);

			Task task = semaphore.WaitAsync(token);

			Assert.Equal(0, semaphore.CurrentCount);
			Assert.True(task.IsCompleted);
			Assert.True(task.IsCanceled);
			Assert.False(task.IsFaulted);
		}

		[Fact]
		public async Task WaitAsync_Cancelled_DoesNotTakeSlot()
		{
			var semaphore = new AsyncSemaphore(0);
			Assert.Equal(0, semaphore.CurrentCount);
			var cts = new CancellationTokenSource();
			Task task = semaphore.WaitAsync(cts.Token);
			Assert.Equal(0, semaphore.CurrentCount);
			Assert.False(task.IsCompleted);

			cts.Cancel();

			try { await task; }
			catch (OperationCanceledException) { }

			semaphore.Release();
			Assert.Equal(1, semaphore.CurrentCount);
			Assert.True(task.IsCanceled);
		}

		[Fact]
		public void Release_WithoutWaiters_IncrementsCount()
		{
			var semaphore = new AsyncSemaphore(0);
			Assert.Equal(0, semaphore.CurrentCount);
			semaphore.Release();
			Assert.Equal(1, semaphore.CurrentCount);
			Task task = semaphore.WaitAsync();
			Assert.Equal(0, semaphore.CurrentCount);
			Assert.True(task.IsCompleted);
		}

		[Fact]
		public async Task Release_WithWaiters_ReleasesWaiters()
		{
			var semaphore = new AsyncSemaphore(0);
			Assert.Equal(0, semaphore.CurrentCount);
			Task task = semaphore.WaitAsync();
			Assert.Equal(0, semaphore.CurrentCount);
			Assert.False(task.IsCompleted);
			semaphore.Release();
			Assert.Equal(0, semaphore.CurrentCount);
			await task;
		}

		[Fact]
		public void Release_Overflow_ThrowsException()
		{
			var semaphore = new AsyncSemaphore(long.MaxValue);
			Assert.Equal(long.MaxValue, semaphore.CurrentCount);
			Assert.Throws<OverflowException>(() => semaphore.Release());
		}

		[Fact]
		public void Release_ZeroSlots_HasNoEffect()
		{
			var semaphore = new AsyncSemaphore(1);
			Assert.Equal(1, semaphore.CurrentCount);
			semaphore.Release(0);
			Assert.Equal(1, semaphore.CurrentCount);
		}

		[Fact]
		public void Id_IsNotZero()
		{
			var semaphore = new AsyncSemaphore(0);
			Assert.NotEqual(0, semaphore.Id);
		}
	}

}
