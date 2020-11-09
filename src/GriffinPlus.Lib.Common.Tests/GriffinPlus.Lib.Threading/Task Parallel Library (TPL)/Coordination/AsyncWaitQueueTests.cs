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

namespace GriffinPlus.Lib.Threading
{
	public class AsyncWaitQueueTests
	{
		[Fact]
		public void IsEmpty_WhenEmpty_IsTrue()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			Assert.True(queue.IsEmpty);
		}

		[Fact]
		public void IsEmpty_WithOneItem_IsFalse()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			queue.Enqueue();
			Assert.False(queue.IsEmpty);
		}

		[Fact]
		public void IsEmpty_WithTwoItems_IsFalse()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			queue.Enqueue();
			queue.Enqueue();
			Assert.False(queue.IsEmpty);
		}

		[Fact]
		public void Dequeue_SynchronouslyCompletesTask()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			var task = queue.Enqueue();
			queue.Dequeue();
			Assert.True(task.IsCompleted);
		}

		[Fact]
		public async Task Dequeue_WithTwoItems_OnlyCompletesFirstItem()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			var task1 = queue.Enqueue();
			var task2 = queue.Enqueue();
			queue.Dequeue();
			Assert.True(task1.IsCompleted);
			await AsyncAssert.DoesNotCompleteAsync(task2);
		}

		[Fact]
		public void Dequeue_WithResult_SynchronouslyCompletesWithResult()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			var result = new object();
			var task = queue.Enqueue();
			queue.Dequeue(result);
			Assert.Same(result, task.Result);
		}

		[Fact]
		public void Dequeue_WithoutResult_SynchronouslyCompletesWithDefaultResult()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			var task = queue.Enqueue();
			queue.Dequeue();
			Assert.Equal(default(object), task.Result);
		}

		[Fact]
		public void DequeueAll_SynchronouslyCompletesAllTasks()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			var task1 = queue.Enqueue();
			var task2 = queue.Enqueue();
			queue.DequeueAll();
			Assert.True(task1.IsCompleted);
			Assert.True(task2.IsCompleted);
		}

		[Fact]
		public void DequeueAll_WithoutResult_SynchronouslyCompletesAllTasksWithDefaultResult()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			var task1 = queue.Enqueue();
			var task2 = queue.Enqueue();
			queue.DequeueAll();
			Assert.Equal(default(object), task1.Result);
			Assert.Equal(default(object), task2.Result);
		}

		[Fact]
		public void DequeueAll_WithResult_CompletesAllTasksWithResult()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			var result = new object();
			var task1 = queue.Enqueue();
			var task2 = queue.Enqueue();
			queue.DequeueAll(result);
			Assert.Same(result, task1.Result);
			Assert.Same(result, task2.Result);
		}

		[Fact]
		public void TryCancel_EntryFound_SynchronouslyCancelsTask()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			var task = queue.Enqueue();
			queue.TryCancel(task, new CancellationToken(true));
			Assert.True(task.IsCanceled);
		}

		[Fact]
		public void TryCancel_EntryFound_RemovesTaskFromQueue()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			var task = queue.Enqueue();
			queue.TryCancel(task, new CancellationToken(true));
			Assert.True(queue.IsEmpty);
		}

		[Fact]
		public void TryCancel_EntryNotFound_DoesNotRemoveTaskFromQueue()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			var task = queue.Enqueue();
			queue.Enqueue();
			queue.Dequeue();
			queue.TryCancel(task, new CancellationToken(true));
			Assert.False(queue.IsEmpty);
		}

		[Fact]
		public async Task Cancelled_WhenInQueue_CancelsTask()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			var cts = new CancellationTokenSource();
			var task = queue.Enqueue(new object(), cts.Token);
			cts.Cancel();
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
		}

		[Fact]
		public async Task Cancelled_WhenInQueue_RemovesTaskFromQueue()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			var cts = new CancellationTokenSource();
			var task = queue.Enqueue(new object(), cts.Token);
			cts.Cancel();
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
			Assert.True(queue.IsEmpty);
		}

		[Fact]
		public void Cancelled_WhenNotInQueue_DoesNotRemoveTaskFromQueue()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			var cts = new CancellationTokenSource();
			var _ = queue.Enqueue(new object(), cts.Token);
			var __ = queue.Enqueue();
			queue.Dequeue();
			cts.Cancel();
			Assert.False(queue.IsEmpty);
		}

		[Fact]
		public void Cancelled_BeforeEnqueue_SynchronouslyCancelsTask()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			var cts = new CancellationTokenSource();
			cts.Cancel();
			var task = queue.Enqueue(new object(), cts.Token);
			Assert.True(task.IsCanceled);
		}

		[Fact]
		public void Cancelled_BeforeEnqueue_RemovesTaskFromQueue()
		{
			var queue = new DefaultAsyncWaitQueue<object>() as IAsyncWaitQueue<object>;
			var cts = new CancellationTokenSource();
			cts.Cancel();
			var _ = queue.Enqueue(new object(), cts.Token);
			Assert.True(queue.IsEmpty);
		}
	}
}