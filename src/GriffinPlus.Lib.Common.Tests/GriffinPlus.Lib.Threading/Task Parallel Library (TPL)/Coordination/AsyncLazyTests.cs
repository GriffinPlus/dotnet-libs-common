﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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

using Xunit;

#pragma warning disable CS0162

namespace GriffinPlus.Lib.Threading;

[Collection(nameof(NoParallelizationCollection))]
public class AsyncLazyTests
{
	[Fact]
	public void AsyncLazy_NeverAwaited_DoesNotCallFunc()
	{
		_ = new AsyncLazy<int>(Func);
		return;

		static Task<int> Func()
		{
			throw new Exception();
			return Task.FromResult(13);
		}
	}

	[Fact]
	public async Task AsyncLazy_WithCallDirectFlag_CallsFuncDirectly()
	{
		int testThread = Thread.CurrentThread.ManagedThreadId;
		int funcThread = testThread + 1;

		var lazy = new AsyncLazy<int>(Func, AsyncLazyFlags.ExecuteOnCallingThread);
		await lazy;

		Assert.Equal(testThread, funcThread);
		return;

		Task<int> Func()
		{
			funcThread = Thread.CurrentThread.ManagedThreadId;
			return Task.FromResult(13);
		}
	}

	[Fact]
	public async Task AsyncLazy_ByDefault_CallsFuncOnThreadPool()
	{
		int testThread = Thread.CurrentThread.ManagedThreadId;
		int funcThread = testThread;

		// let a pool thread run the factory function
		var lazy = new AsyncLazy<int>(Func);

		// give the TPL some time to schedule the factory function
		// (otherwise the following 'await lazy' will execute the factory function synchronously)
		while (lazy.Task.Status == TaskStatus.WaitingForActivation)
		{
			Thread.Sleep(50);
		}

		// wait for the factory callback to complete
		await lazy;

		// the test thread should be some other thread than the thread calling the factory function
		Assert.NotEqual(testThread, funcThread);

		return;

		Task<int> Func()
		{
			funcThread = Thread.CurrentThread.ManagedThreadId;
			return Task.FromResult(13);
		}
	}

	[Fact]
	public async Task AsyncLazy_Start_CallsFunc()
	{
		TaskCompletionSource<object> tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

		var lazy = new AsyncLazy<int>(Func);

		lazy.Start();
		await tcs.Task;
		return;

		Task<int> Func()
		{
			tcs.SetResult(null);
			return Task.FromResult(13);
		}
	}

	[Fact]
	public async Task AsyncLazy_Await_ReturnsFuncValue()
	{
		var lazy = new AsyncLazy<int>(Func);

		int result = await lazy;
		Assert.Equal(13, result);
		return;

		static async Task<int> Func()
		{
			await Task.Yield();
			return 13;
		}
	}

	[Fact]
	public async Task AsyncLazy_MultipleAwaiters_OnlyInvokeFuncOnce()
	{
		int invokeCount = 0;
		TaskCompletionSource<object> tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

		var lazy = new AsyncLazy<int>(Func);

		Task<int> task1 = Task.Run(async () => await lazy);
		Task<int> task2 = Task.Run(async () => await lazy);

		Assert.False(task1.IsCompleted);
		Assert.False(task2.IsCompleted);
		tcs.SetResult(null);
		int[] results = await Task.WhenAll(task1, task2);
		int[] expected = [13, 13];
		Assert.Equal(expected, results);
		Assert.Equal(1, invokeCount);
		return;

		async Task<int> Func()
		{
			Interlocked.Increment(ref invokeCount);
			await tcs.Task;
			return 13;
		}
	}

	[Fact]
	public async Task AsyncLazy_FailureCachedByDefault()
	{
		int invokeCount = 0;

		var lazy = new AsyncLazy<int>(Func);
		await Assert.ThrowsAsync<InvalidOperationException>(() => lazy.Task);

		await Assert.ThrowsAsync<InvalidOperationException>(() => lazy.Task);
		Assert.Equal(1, invokeCount);
		return;

		async Task<int> Func()
		{
			Interlocked.Increment(ref invokeCount);
			await Task.Yield();
			if (invokeCount == 1) throw new InvalidOperationException("Not today, punk.");
			return 13;
		}
	}

	[Fact]
	public async Task AsyncLazy_WithRetryOnFailure_DoesNotCacheFailure()
	{
		int invokeCount = 0;

		var lazy = new AsyncLazy<int>(Func, AsyncLazyFlags.RetryOnFailure);
		await Assert.ThrowsAsync<InvalidOperationException>(() => lazy.Task);

		Assert.Equal(13, await lazy);
		Assert.Equal(2, invokeCount);
		return;

		async Task<int> Func()
		{
			Interlocked.Increment(ref invokeCount);
			await Task.Yield();
			if (invokeCount == 1) throw new InvalidOperationException("Not today, punk.");
			return 13;
		}
	}

	[Fact]
	public async Task AsyncLazy_WithRetryOnFailure_DoesNotRetryOnSuccess()
	{
		int invokeCount = 0;

		var lazy = new AsyncLazy<int>(Func, AsyncLazyFlags.RetryOnFailure);
		await Assert.ThrowsAsync<InvalidOperationException>(() => lazy.Task);

		await lazy;
		await lazy;

		Assert.Equal(13, await lazy);
		Assert.Equal(2, invokeCount);
		return;

		async Task<int> Func()
		{
			Interlocked.Increment(ref invokeCount);
			await Task.Yield();
			if (invokeCount == 1)
				throw new InvalidOperationException("Not today, punk.");
			return 13;
		}
	}

	[Fact]
	public void Id_IsNotZero()
	{
		var lazy = new AsyncLazy<object>(() => Task.FromResult<object>(null));
		Assert.NotEqual(0, lazy.Id);
	}
}
