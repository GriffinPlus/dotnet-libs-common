﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/GriffinPlus/dotnet-libs-common)
//
// Copyright 2019 Sascha Falk <sascha@falk-online.eu>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for
// the specific language governing permissions and limitations under the License.
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

using GriffinPlus.Lib.Tests;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GriffinPlus.Lib.Threading
{
	public class AsyncProducerConsumerQueueTests
	{
		[Fact]
		public void ConstructorWithZeroMaxCount_Throws()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new AsyncProducerConsumerQueue<int>(0));
		}

		[Fact]
		public void ConstructorWithZeroMaxCountAndCollection_Throws()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new AsyncProducerConsumerQueue<int>(new int[0], 0));
		}

		[Fact]
		public void ConstructorWithMaxCountSmallerThanCollectionCount_Throws()
		{
			Assert.Throws<ArgumentException>(() => new AsyncProducerConsumerQueue<int>(new[] { 3, 5 }, 1));
		}

		[Fact]
		public async Task ConstructorWithCollection_AddsItems()
		{
			var queue = new AsyncProducerConsumerQueue<int>(new[] { 3, 5, 7 });

			var result1 = await queue.DequeueAsync();
			var result2 = await queue.DequeueAsync();
			var result3 = await queue.DequeueAsync();

			Assert.Equal(3, result1);
			Assert.Equal(5, result2);
			Assert.Equal(7, result3);
		}

		[Fact]
		public async Task EnqueueAsync_SpaceAvailable_EnqueuesItem()
		{
			var queue = new AsyncProducerConsumerQueue<int>();

			await queue.EnqueueAsync(3);
			var result = await queue.DequeueAsync();

			Assert.Equal(3, result);
		}

		[Fact]
		public async Task EnqueueAsync_CompleteAdding_ThrowsException()
		{
			var queue = new AsyncProducerConsumerQueue<int>();
			queue.CompleteAdding();

			await Assert.ThrowsAsync<InvalidOperationException>(() => queue.EnqueueAsync(3));
		}

		[Fact]
		public async Task DequeueAsync_EmptyAndComplete_ThrowsException()
		{
			var queue = new AsyncProducerConsumerQueue<int>();
			queue.CompleteAdding();

			await Assert.ThrowsAsync<InvalidOperationException>(() => queue.DequeueAsync());
		}

		[Fact]
		public async Task DequeueAsync_Empty_DoesNotComplete()
		{
			var queue = new AsyncProducerConsumerQueue<int>();

			var task = queue.DequeueAsync();

			await AsyncAssert.DoesNotCompleteAsync(task);
		}

		[Fact]
		public async Task DequeueAsync_Empty_ItemAdded_Completes()
		{
			var queue = new AsyncProducerConsumerQueue<int>();
			var task = queue.DequeueAsync();

			await queue.EnqueueAsync(13);
			var result = await task;

			Assert.Equal(13, result);
		}

		[Fact]
		public async Task DequeueAsync_Cancelled_Throws()
		{
			var queue = new AsyncProducerConsumerQueue<int>();
			var cts = new CancellationTokenSource();
			var task = queue.DequeueAsync(cts.Token);

			cts.Cancel();

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
		}

		[Fact]
		public async Task EnqueueAsync_Full_DoesNotComplete()
		{
			var queue = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);

			var task = queue.EnqueueAsync(7);

			await AsyncAssert.DoesNotCompleteAsync(task);
		}

		[Fact]
		public async Task EnqueueAsync_SpaceAvailable_Completes()
		{
			var queue = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
			var task = queue.EnqueueAsync(7);

			await queue.DequeueAsync();

			await task;
		}

		[Fact]
		public async Task EnqueueAsync_Cancelled_Throws()
		{
			var queue = new AsyncProducerConsumerQueue<int>(new[] { 13 }, 1);
			var cts = new CancellationTokenSource();
			var task = queue.EnqueueAsync(7, cts.Token);

			cts.Cancel();

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
		}

		[Fact]
		public void CompleteAdding_MultipleTimes_DoesNotThrow()
		{
			var queue = new AsyncProducerConsumerQueue<int>();
			queue.CompleteAdding();

			queue.CompleteAdding();
		}

		[Fact]
		public async Task OutputAvailableAsync_NoItemsInQueue_IsNotCompleted()
		{
			var queue = new AsyncProducerConsumerQueue<int>();

			var task = queue.OutputAvailableAsync();

			await AsyncAssert.DoesNotCompleteAsync(task);
		}

		[Fact]
		public async Task OutputAvailableAsync_ItemInQueue_ReturnsTrue()
		{
			var queue = new AsyncProducerConsumerQueue<int>();
			queue.Enqueue(13);

			var result = await queue.OutputAvailableAsync();
			Assert.True(result);
		}

		[Fact]
		public async Task OutputAvailableAsync_NoItemsAndCompleted_ReturnsFalse()
		{
			var queue = new AsyncProducerConsumerQueue<int>();
			queue.CompleteAdding();

			var result = await queue.OutputAvailableAsync();
			Assert.False(result);
		}

		[Fact]
		public async Task OutputAvailableAsync_ItemInQueueAndCompleted_ReturnsTrue()
		{
			var queue = new AsyncProducerConsumerQueue<int>();
			queue.Enqueue(13);
			queue.CompleteAdding();

			var result = await queue.OutputAvailableAsync();
			Assert.True(result);
		}

		[Fact]
		public async Task StandardAsyncSingleConsumerCode()
		{
			var queue = new AsyncProducerConsumerQueue<int>();

			// producer
			var unused = Task.Run(() =>
			{
				queue.Enqueue(3);
				queue.Enqueue(13);
				queue.Enqueue(17);
				queue.CompleteAdding();
			});

			// consumer
			var results = new List<int>();
			while (await queue.OutputAvailableAsync())
			{
				results.Add(queue.Dequeue());
			}

			Assert.Equal(3, results.Count);
			Assert.Equal(3, results[0]);
			Assert.Equal(13, results[1]);
			Assert.Equal(17, results[2]);
		}
	}
}