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

// ReSharper disable file AccessToModifiedClosure

namespace GriffinPlus.Lib.Threading;

[Collection(nameof(NoParallelizationCollection))]
public class AsyncMonitorUnitTests
{
	[Fact]
	public async Task Unlocked_PermitsLock()
	{
		var monitor = new AsyncMonitor();

		AwaitableDisposable<IDisposable> task = monitor.EnterAsync();
		await task;
	}

	[Fact]
	public async Task Locked_PreventsLockUntilUnlocked()
	{
		var monitor = new AsyncMonitor();
		TaskCompletionSource<object> task1HasLock = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
		TaskCompletionSource<object> task1Continue = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

		Task unused = Task.Run(
			async () =>
			{
				using (await monitor.EnterAsync())
				{
					task1HasLock.SetResult(null);
					await task1Continue.Task;
				}
			});
		await task1HasLock.Task;

		Task<IDisposable> lockTask = monitor.EnterAsync().AsTask();
		Assert.False(lockTask.IsCompleted);
		task1Continue.SetResult(null);
		await lockTask;
	}

	[Fact]
	public async Task Pulse_ReleasesOneWaiter()
	{
		var monitor = new AsyncMonitor();
		int completed = 0;
		TaskCompletionSource<object> task1Ready = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
		TaskCompletionSource<object> task2Ready = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
		Task task1 = Task.Run(
			async () =>
			{
				using (await monitor.EnterAsync())
				{
					Task waitTask1 = monitor.WaitAsync();
					task1Ready.SetResult(null);
					await waitTask1;
					Interlocked.Increment(ref completed);
				}
			});
		await task1Ready.Task;
		Task task2 = Task.Run(
			async () =>
			{
				using (await monitor.EnterAsync())
				{
					Task waitTask2 = monitor.WaitAsync();
					task2Ready.SetResult(null);
					await waitTask2;
					Interlocked.Increment(ref completed);
				}
			});
		await task2Ready.Task;

		using (await monitor.EnterAsync())
		{
			monitor.Pulse();
		}

		await Task.WhenAny(task1, task2);
		int result = Interlocked.CompareExchange(ref completed, 0, 0);

		Assert.Equal(1, result);
	}

	[Fact]
	public async Task PulseAll_ReleasesAllWaiters()
	{
		var monitor = new AsyncMonitor();
		int completed = 0;
		TaskCompletionSource<object> task1Ready = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
		TaskCompletionSource<object> task2Ready = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
		Task waitTask1;
		Task task1 = Task.Run(
			async () =>
			{
				using (await monitor.EnterAsync())
				{
					waitTask1 = monitor.WaitAsync();
					task1Ready.SetResult(null);
					await waitTask1;
					Interlocked.Increment(ref completed);
				}
			});
		await task1Ready.Task;
		Task waitTask2;
		Task task2 = Task.Run(
			async () =>
			{
				using (await monitor.EnterAsync())
				{
					waitTask2 = monitor.WaitAsync();
					task2Ready.SetResult(null);
					await waitTask2;
					Interlocked.Increment(ref completed);
				}
			});
		await task2Ready.Task;

		AwaitableDisposable<IDisposable> lockTask3 = monitor.EnterAsync();
		using (await lockTask3)
		{
			monitor.PulseAll();
		}

		await Task.WhenAll(task1, task2);
		int result = Interlocked.CompareExchange(ref completed, 0, 0);

		Assert.Equal(2, result);
	}

	[Fact]
	public void Id_IsNotZero()
	{
		var monitor = new AsyncMonitor();
		Assert.NotEqual(0, monitor.Id);
	}
}
