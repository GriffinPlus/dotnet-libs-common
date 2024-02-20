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

using Xunit;

// ReSharper disable MethodHasAsyncOverload
// ReSharper disable UnusedVariable

namespace GriffinPlus.Lib.Threading;

[Collection(nameof(NoParallelizationCollection))]
public class AsyncLockTests
{
	[Fact]
	public void AsyncLock_Unlocked_SynchronouslyPermitsLock()
	{
		var mutex = new AsyncLock();

		Task<IDisposable> lockTask = mutex.LockAsync().AsTask();

		Assert.True(lockTask.IsCompleted);
		Assert.False(lockTask.IsFaulted);
		Assert.False(lockTask.IsCanceled);
	}

	[Fact]
	public async Task AsyncLock_Locked_PreventsLockUntilUnlocked()
	{
		var mutex = new AsyncLock();
		TaskCompletionSource<object> task1HasLock = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
		TaskCompletionSource<object> task1Continue = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

		Task task1 = Task.Run(
			async () =>
			{
				using (await mutex.LockAsync())
				{
					task1HasLock.SetResult(null);
					await task1Continue.Task;
				}
			});
		await task1HasLock.Task;

		Task task2 = Task.Run(
			async () =>
			{
				await mutex.LockAsync();
			});

		Assert.False(task2.IsCompleted);
		task1Continue.SetResult(null);
		await task2;
	}

	[Fact]
	public async Task AsyncLock_DoubleDispose_OnlyPermitsOneTask()
	{
		var mutex = new AsyncLock();
		TaskCompletionSource<object> task1HasLock = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
		TaskCompletionSource<object> task1Continue = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

		await Task.Run(
			async () =>
			{
				IDisposable key = await mutex.LockAsync();
				key.Dispose();
				key.Dispose();
			});

		Task task1 = Task.Run(
			async () =>
			{
				using (await mutex.LockAsync())
				{
					task1HasLock.SetResult(null);
					await task1Continue.Task;
				}
			});
		await task1HasLock.Task;

		Task task2 = Task.Run(
			async () =>
			{
				await mutex.LockAsync();
			});

		Assert.False(task2.IsCompleted);
		task1Continue.SetResult(null);
		await task2;
	}

	[Fact]
	public async Task AsyncLock_Locked_OnlyPermitsOneLockerAtATime()
	{
		var mutex = new AsyncLock();
		TaskCompletionSource<object> task1HasLock = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
		TaskCompletionSource<object> task1Continue = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
		TaskCompletionSource<object> task2Ready = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
		TaskCompletionSource<object> task2HasLock = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
		TaskCompletionSource<object> task2Continue = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

		Task task1 = Task.Run(
			async () =>
			{
				using (await mutex.LockAsync())
				{
					task1HasLock.SetResult(null);
					await task1Continue.Task;
				}
			});
		await task1HasLock.Task;

		Task task2 = Task.Run(
			async () =>
			{
				AwaitableDisposable<IDisposable> key = mutex.LockAsync();
				task2Ready.SetResult(null);
				using (await key)
				{
					task2HasLock.SetResult(null);
					await task2Continue.Task;
				}
			});
		await task2Ready.Task;

		Task task3 = Task.Run(
			async () =>
			{
				await mutex.LockAsync();
			});

		task1Continue.SetResult(null);
		await task2HasLock.Task;

		Assert.False(task3.IsCompleted);
		task2Continue.SetResult(null);
		await task2;
		await task3;
	}

	[Fact]
	public void AsyncLock_PreCancelled_Unlocked_SynchronouslyTakesLock()
	{
		var mutex = new AsyncLock();
		var token = new CancellationToken(true);

		Task<IDisposable> task = mutex.LockAsync(token).AsTask();

		Assert.True(task.IsCompleted);
		Assert.False(task.IsCanceled);
		Assert.False(task.IsFaulted);
	}

	[Fact]
	public void AsyncLock_PreCancelled_Locked_SynchronouslyCancels()
	{
		var mutex = new AsyncLock();
		AwaitableDisposable<IDisposable> lockTask = mutex.LockAsync();
		var token = new CancellationToken(true);

		Task<IDisposable> task = mutex.LockAsync(token).AsTask();

		Assert.True(task.IsCompleted);
		Assert.True(task.IsCanceled);
		Assert.False(task.IsFaulted);
	}

	[Fact]
	public async Task AsyncLock_CancelledLock_LeavesLockUnlocked()
	{
		var mutex = new AsyncLock();
		var cts = new CancellationTokenSource();
		TaskCompletionSource<object> taskReady = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

		IDisposable unlock = await mutex.LockAsync();
		Task task = Task.Run(
			async () =>
			{
				AwaitableDisposable<IDisposable> lockTask = mutex.LockAsync(cts.Token);
				taskReady.SetResult(null);
				await lockTask;
			},
			CancellationToken.None);
		await taskReady.Task;
		cts.Cancel();
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
		Assert.True(task.IsCanceled);
		unlock.Dispose();

		AwaitableDisposable<IDisposable> finalLockTask = mutex.LockAsync();
		await finalLockTask;
	}

	[Fact]
	public async Task AsyncLock_CanceledLock_ThrowsException()
	{
		var mutex = new AsyncLock();
		var cts = new CancellationTokenSource();

		await mutex.LockAsync();
		Task<IDisposable> canceledLockTask = mutex.LockAsync(cts.Token).AsTask();
		cts.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => canceledLockTask);
	}

	[Fact]
	public async Task AsyncLock_CanceledTooLate_StillTakesLock()
	{
		var mutex = new AsyncLock();
		var cts = new CancellationTokenSource();

		AwaitableDisposable<IDisposable> cancelableLockTask;
		using (await mutex.LockAsync())
		{
			cancelableLockTask = mutex.LockAsync(cts.Token);
		}

		IDisposable key = await cancelableLockTask;
		cts.Cancel();

		Task<IDisposable> nextLocker = mutex.LockAsync().AsTask();
		Assert.False(nextLocker.IsCompleted);

		key.Dispose();
		await nextLocker;
	}

	[Fact]
	public void Id_IsNotZero()
	{
		var mutex = new AsyncLock();
		Assert.NotEqual(0, mutex.Id);
	}

	[Fact]
	public Task AsyncLock_SupportsMultipleAsynchronousLocks()
	{
		return Task.Run(
			() =>
			{
				var asyncLock = new AsyncLock();
				var cancellationTokenSource = new CancellationTokenSource();
				CancellationToken cancellationToken = cancellationTokenSource.Token;
				Task task1 = Task.Run(
					async () =>
					{
						while (!cancellationToken.IsCancellationRequested)
						{
							using (await asyncLock.LockAsync())
							{
								Thread.Sleep(10);
							}
						}
					},
					CancellationToken.None);
				Task task2 = Task.Run(
					() =>
					{
						using (asyncLock.Lock())
						{
							Thread.Sleep(1000);
						}
					},
					CancellationToken.None);

				task2.Wait(CancellationToken.None);
				cancellationTokenSource.Cancel();
				task1.Wait(CancellationToken.None);
			});
	}
}
