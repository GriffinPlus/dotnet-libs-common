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
//     Copyright (c) 2018 Stephen Cleary
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
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GriffinPlus.Lib.Threading
{
	public class AsyncReaderWriterLockTests
	{
		[Fact]
		public async Task Unlocked_PermitsWriterLock()
		{
			var rwl = new AsyncReaderWriterLock();
			await rwl.WriterLockAsync();
		}

		[Fact]
		public async Task Unlocked_PermitsMultipleReaderLocks()
		{
			var rwl = new AsyncReaderWriterLock();
			await rwl.ReaderLockAsync();
			await rwl.ReaderLockAsync();
		}

		[Fact]
		public async Task WriteLocked_PreventsAnotherWriterLock()
		{
			var rwl = new AsyncReaderWriterLock();
			await rwl.WriterLockAsync();
			var task = rwl.WriterLockAsync().AsTask();
			await NeverCompletesAsync(task);
		}

		[Fact]
		public async Task WriteLocked_PreventsReaderLock()
		{
			var rwl = new AsyncReaderWriterLock();
			await rwl.WriterLockAsync();
			var task = rwl.ReaderLockAsync().AsTask();
			await NeverCompletesAsync(task);
		}

		[Fact]
		public async Task WriteLocked_Unlocked_PermitsAnotherWriterLock()
		{
			var rwl = new AsyncReaderWriterLock();
			var firstWriteLockTaken = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
			var releaseFirstWriteLock = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
			var task = Task.Run(async () =>
			{
				using (await rwl.WriterLockAsync())
				{
					firstWriteLockTaken.SetResult(null);
					await releaseFirstWriteLock.Task;
				}
			});
			await firstWriteLockTaken.Task;
			var lockTask = rwl.WriterLockAsync().AsTask();
			Assert.False(lockTask.IsCompleted);
			releaseFirstWriteLock.SetResult(null);
			await lockTask;
		}

		[Fact]
		public async Task ReadLocked_PreventsWriterLock()
		{
			var rwl = new AsyncReaderWriterLock();
			await rwl.ReaderLockAsync();
			var task = rwl.WriterLockAsync().AsTask();
			await NeverCompletesAsync(task);
		}

		[Fact]
		public void Id_IsNotZero()
		{
			var rwl = new AsyncReaderWriterLock();
			Assert.NotEqual(0, rwl.Id);
		}

		[Fact]
		public void WriterLock_PreCancelled_LockAvailable_SynchronouslyTakesLock()
		{
			var rwl = new AsyncReaderWriterLock();
			var token = new CancellationToken(true);

			var task = rwl.WriterLockAsync(token).AsTask();

			Assert.True(task.IsCompleted);
			Assert.False(task.IsCanceled);
			Assert.False(task.IsFaulted);
		}

		[Fact]
		public void WriterLock_PreCancelled_LockNotAvailable_SynchronouslyCancels()
		{
			var rwl = new AsyncReaderWriterLock();
			var token = new CancellationToken(true);
			rwl.WriterLockAsync();

			var task = rwl.WriterLockAsync(token).AsTask();

			Assert.True(task.IsCompleted);
			Assert.True(task.IsCanceled);
			Assert.False(task.IsFaulted);
		}

		[Fact]
		public void ReaderLock_PreCancelled_LockAvailable_SynchronouslyTakesLock()
		{
			var rwl = new AsyncReaderWriterLock();
			var token = new CancellationToken(true);

			var task = rwl.ReaderLockAsync(token).AsTask();

			Assert.True(task.IsCompleted);
			Assert.False(task.IsCanceled);
			Assert.False(task.IsFaulted);
		}

		[Fact]
		public void ReaderLock_PreCancelled_LockNotAvailable_SynchronouslyCancels()
		{
			var rwl = new AsyncReaderWriterLock();
			var token = new CancellationToken(true);
			rwl.WriterLockAsync();

			var task = rwl.ReaderLockAsync(token).AsTask();

			Assert.True(task.IsCompleted);
			Assert.True(task.IsCanceled);
			Assert.False(task.IsFaulted);
		}

		[Fact]
		public async Task WriteLocked_WriterLockCancelled_DoesNotTakeLockWhenUnlocked()
		{
			var rwl = new AsyncReaderWriterLock();
			using (await rwl.WriterLockAsync())
			{
				var cts = new CancellationTokenSource();
				var task = rwl.WriterLockAsync(cts.Token).AsTask();
				cts.Cancel();
				await Assert.ThrowsAsync<TaskCanceledException>(() => task);
			}

			await rwl.WriterLockAsync();
		}

		[Fact]
		public async Task WriteLocked_ReaderLockCancelled_DoesNotTakeLockWhenUnlocked()
		{
			var rwl = new AsyncReaderWriterLock();
			using (await rwl.WriterLockAsync())
			{
				var cts = new CancellationTokenSource();
				var task = rwl.ReaderLockAsync(cts.Token).AsTask();
				cts.Cancel();
				await Assert.ThrowsAsync<TaskCanceledException>(() => task);
			}

			await rwl.ReaderLockAsync();
		}

		[Fact]
		public async Task LockReleased_WriteTakesPriorityOverRead()
		{
			var rwl = new AsyncReaderWriterLock();
			Task writeLock, readLock;
			using (await rwl.WriterLockAsync())
			{
				readLock = rwl.ReaderLockAsync().AsTask();
				writeLock = rwl.WriterLockAsync().AsTask();
			}

			await writeLock;
			await NeverCompletesAsync(readLock);
		}

		[Fact]
		public async Task ReaderLocked_ReaderReleased_ReaderAndWriterWaiting_DoesNotReleaseReaderOrWriter()
		{
			var rwl = new AsyncReaderWriterLock();
			Task readLock, writeLock;
			await rwl.ReaderLockAsync();
			using (await rwl.ReaderLockAsync())
			{
				writeLock = rwl.WriterLockAsync().AsTask();
				readLock = rwl.ReaderLockAsync().AsTask();
			}

			await Task.WhenAll(
				NeverCompletesAsync(writeLock),
				NeverCompletesAsync(readLock));
		}

		[Fact]
		public async Task LoadTest()
		{
			var rwl = new AsyncReaderWriterLock();
			var readKeys = new List<IDisposable>();
			for (int i = 0; i != 1000; ++i)
				readKeys.Add(rwl.ReaderLock());
			var writeTask = Task.Run(() => { rwl.WriterLock().Dispose(); });
			var readTasks = new List<Task>();
			for (int i = 0; i != 100; ++i)
				readTasks.Add(Task.Run(() => rwl.ReaderLock().Dispose()));
			await Task.Delay(1000);
			foreach (var readKey in readKeys)
				readKey.Dispose();
			await writeTask;
			foreach (var readTask in readTasks)
				await readTask;
		}

		[Fact]
		public async Task ReadLock_WriteLockCanceled_TakesLock()
		{
			var rwl = new AsyncReaderWriterLock();
			var readKey = rwl.ReaderLock();
			var cts = new CancellationTokenSource();

			var writerLockReady = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			var writerLockTask = Task.Run(async () =>
			{
				var writeKeyTask = rwl.WriterLockAsync(cts.Token);
				writerLockReady.SetResult(null);
				await Assert.ThrowsAnyAsync<OperationCanceledException>(() => writeKeyTask);
			});
			await writerLockReady.Task;

			var readerLockReady = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			var readerLockTask = Task.Run(async () =>
			{
				var readKeyTask = rwl.ReaderLockAsync();
				readerLockReady.SetResult(null);
				await readKeyTask;
			});

			await readerLockReady.Task;
			cts.Cancel();

			await readerLockTask;
		}

		/// <summary>
		/// Attempts to ensure that a task never completes.
		/// If the task takes a long time to complete, this method may not detect that it (incorrectly) completes.
		/// </summary>
		/// <param name="task">The task to observe.</param>
		/// <param name="timeout">The amount of time to (asynchronously) wait for the task to complete.</param>
		private static async Task NeverCompletesAsync(Task task, int timeout = 500)
		{
			// Wait for the task to complete, or the timeout to fire.
			var completedTask = await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false);
			if (completedTask == task) throw new Exception("Task completed unexpectedly.");

			// If the task didn't complete, attach a continuation that will raise an exception on a random thread pool thread, if it ever does complete.
			try
			{
				throw new Exception("Task completed unexpectedly.");
			}
			catch (Exception ex)
			{
				var info = ExceptionDispatchInfo.Capture(ex);
				var __ = task.ContinueWith(_ => info.Throw(), TaskScheduler.Default);
			}
		}
	}
}