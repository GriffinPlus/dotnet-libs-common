///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable CS0162

namespace GriffinPlus.Lib.Threading
{
	public class AsyncLazyTests
	{
		[Fact]
		public void AsyncLazy_NeverAwaited_DoesNotCallFunc()
		{
			Task<int> Func()
			{
				throw new Exception();
				return Task.FromResult(13);
			}

			var lazy = new AsyncLazy<int>(Func);
		}

		[Fact]
		public async Task AsyncLazy_WithCallDirectFlag_CallsFuncDirectly()
		{
			var testThread = Thread.CurrentThread.ManagedThreadId;
			var funcThread = testThread + 1;

			Task<int> Func()
			{
				funcThread = Thread.CurrentThread.ManagedThreadId;
				return Task.FromResult(13);
			}

			var lazy = new AsyncLazy<int>(Func, AsyncLazyFlags.ExecuteOnCallingThread);

			await lazy;

			Assert.Equal(testThread, funcThread);
		}

		[Fact]
		public async Task AsyncLazy_ByDefault_CallsFuncOnThreadPool()
		{
			var testThread = Thread.CurrentThread.ManagedThreadId;
			var funcThread = testThread;

			Task<int> Func()
			{
				funcThread = Thread.CurrentThread.ManagedThreadId;
				return Task.FromResult(13);
			}

			var lazy = new AsyncLazy<int>(Func);

			await lazy;

			Assert.NotEqual(testThread, funcThread);
		}

		[Fact]
		public async Task AsyncLazy_Start_CallsFunc()
		{
			var tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

			Task<int> Func()
			{
				tcs.SetResult(null);
				return Task.FromResult(13);
			}

			var lazy = new AsyncLazy<int>(Func);

			lazy.Start();
			await tcs.Task;
		}

		[Fact]
		public async Task AsyncLazy_Await_ReturnsFuncValue()
		{
			async Task<int> Func()
			{
				await Task.Yield();
				return 13;
			}

			var lazy = new AsyncLazy<int>(Func);

			var result = await lazy;
			Assert.Equal(13, result);
		}

		[Fact]
		public async Task AsyncLazy_MultipleAwaiters_OnlyInvokeFuncOnce()
		{
			int invokeCount = 0;
			var tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

			async Task<int> Func()
			{
				Interlocked.Increment(ref invokeCount);
				await tcs.Task;
				return 13;
			}

			var lazy = new AsyncLazy<int>(Func);

			var task1 = Task.Run(async () => await lazy);
			var task2 = Task.Run(async () => await lazy);

			Assert.False(task1.IsCompleted);
			Assert.False(task2.IsCompleted);
			tcs.SetResult(null);
			var results = await Task.WhenAll(task1, task2);
			Assert.Equal(new[] { 13, 13 }, results);
			Assert.Equal(1, invokeCount);
		}

		[Fact]
		public async Task AsyncLazy_FailureCachedByDefault()
		{
			int invokeCount = 0;

			async Task<int> Func()
			{
				Interlocked.Increment(ref invokeCount);
				await Task.Yield();
				if (invokeCount == 1) throw new InvalidOperationException("Not today, punk.");
				return 13;
			}

			var lazy = new AsyncLazy<int>(Func);
			await Assert.ThrowsAsync<InvalidOperationException>(() => lazy.Task);

			await Assert.ThrowsAsync<InvalidOperationException>(() => lazy.Task);
			Assert.Equal(1, invokeCount);
		}

		[Fact]
		public async Task AsyncLazy_WithRetryOnFailure_DoesNotCacheFailure()
		{
			int invokeCount = 0;

			async Task<int> Func()
			{
				Interlocked.Increment(ref invokeCount);
				await Task.Yield();
				if (invokeCount == 1) throw new InvalidOperationException("Not today, punk.");
				return 13;
			}

			var lazy = new AsyncLazy<int>(Func, AsyncLazyFlags.RetryOnFailure);
			await Assert.ThrowsAsync<InvalidOperationException>(() => lazy.Task);

			Assert.Equal(13, await lazy);
			Assert.Equal(2, invokeCount);
		}

		[Fact]
		public async Task AsyncLazy_WithRetryOnFailure_DoesNotRetryOnSuccess()
		{
			int invokeCount = 0;
			Func<Task<int>> func = async () =>
			{
				Interlocked.Increment(ref invokeCount);
				await Task.Yield();
				if (invokeCount == 1)
					throw new InvalidOperationException("Not today, punk.");
				return 13;
			};
			var lazy = new AsyncLazy<int>(func, AsyncLazyFlags.RetryOnFailure);
			await Assert.ThrowsAsync<InvalidOperationException>(() => lazy.Task);

			await lazy;
			await lazy;

			Assert.Equal(13, await lazy);
			Assert.Equal(2, invokeCount);
		}

		[Fact]
		public void Id_IsNotZero()
		{
			var lazy = new AsyncLazy<object>(() => Task.FromResult<object>(null));
			Assert.NotEqual(0, lazy.Id);
		}
	}
}