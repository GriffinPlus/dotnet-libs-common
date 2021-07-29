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

	[Collection(nameof(NoParallelizationCollection))]
	public class AsyncAutoResetEventTests
	{
		[Fact]
		public async Task WaitAsync_Unset_IsNotCompleted()
		{
			var are = new AsyncAutoResetEvent();

			var task = are.WaitAsync();

			await AsyncAssert.DoesNotCompleteAsync(task);
		}

		[Fact]
		public void WaitAsync_AfterSet_CompletesSynchronously()
		{
			var are = new AsyncAutoResetEvent();

			are.Set();
			var task = are.WaitAsync();

			Assert.True(task.IsCompleted);
		}

		[Fact]
		public void WaitAsync_Set_CompletesSynchronously()
		{
			var are = new AsyncAutoResetEvent(true);

			var task = are.WaitAsync();

			Assert.True(task.IsCompleted);
		}

		[Fact]
		public async Task MultipleWaitAsync_AfterSet_OnlyOneIsCompleted()
		{
			var are = new AsyncAutoResetEvent();

			are.Set();
			var task1 = are.WaitAsync();
			var task2 = are.WaitAsync();

			Assert.True(task1.IsCompleted);
			await AsyncAssert.DoesNotCompleteAsync(task2);
		}

		[Fact]
		public async Task MultipleWaitAsync_Set_OnlyOneIsCompleted()
		{
			var are = new AsyncAutoResetEvent(true);

			var task1 = are.WaitAsync();
			var task2 = are.WaitAsync();

			Assert.True(task1.IsCompleted);
			await AsyncAssert.DoesNotCompleteAsync(task2);
		}

		[Fact]
		public async Task MultipleWaitAsync_AfterMultipleSet_OnlyOneIsCompleted()
		{
			var are = new AsyncAutoResetEvent();

			are.Set();
			are.Set();
			var task1 = are.WaitAsync();
			var task2 = are.WaitAsync();

			Assert.True(task1.IsCompleted);
			await AsyncAssert.DoesNotCompleteAsync(task2);
		}

		[Fact]
		public void WaitAsync_PreCancelled_Set_SynchronouslyCompletesWait()
		{
			var are = new AsyncAutoResetEvent(true);
			var token = new CancellationToken(true);

			var task = are.WaitAsync(token);

			Assert.True(task.IsCompleted);
			Assert.False(task.IsCanceled);
			Assert.False(task.IsFaulted);
		}

		[Fact]
		public async Task WaitAsync_Cancelled_DoesNotAutoReset()
		{
			var are = new AsyncAutoResetEvent();
			var cts = new CancellationTokenSource();

			cts.Cancel();
			var task1 = are.WaitAsync(cts.Token);
			task1.WaitWithoutException(CancellationToken.None);
			are.Set();
			var task2 = are.WaitAsync(CancellationToken.None);

			await task2;
		}

		[Fact]
		public void WaitAsync_PreCancelled_Unset_SynchronouslyCancels()
		{
			var are = new AsyncAutoResetEvent(false);
			var token = new CancellationToken(true);

			var task = are.WaitAsync(token);

			Assert.True(task.IsCompleted);
			Assert.True(task.IsCanceled);
			Assert.False(task.IsFaulted);
		}

		[Fact]
		public void WaitAsyncFromCustomSynchronizationContext_PreCancelled_Unset_SynchronouslyCancels()
		{
			AsyncContext.Run(
				() =>
				{
					var are = new AsyncAutoResetEvent(false);
					var token = new CancellationToken(true);

					var task = are.WaitAsync(token);

					Assert.True(task.IsCompleted);
					Assert.True(task.IsCanceled);
					Assert.False(task.IsFaulted);
				});
		}

		[Fact]
		public async Task WaitAsync_Cancelled_ThrowsException()
		{
			var are = new AsyncAutoResetEvent();
			var cts = new CancellationTokenSource();
			cts.Cancel();
			var task = are.WaitAsync(cts.Token);
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
		}

		[Fact]
		public void Id_IsNotZero()
		{
			var are = new AsyncAutoResetEvent();
			Assert.NotEqual(0, are.Id);
		}
	}

}
