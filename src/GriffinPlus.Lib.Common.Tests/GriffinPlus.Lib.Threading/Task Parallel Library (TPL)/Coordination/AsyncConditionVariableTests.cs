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
using System.Threading.Tasks;

using GriffinPlus.Lib.Tests;

using Xunit;

namespace GriffinPlus.Lib.Threading
{

	[Collection(nameof(NoParallelizationCollection))]
	public class AsyncConditionVariableTests
	{
		[Fact]
		public async Task WaitAsync_WithoutNotify_IsNotCompleted()
		{
			var mutex = new AsyncLock();
			var cv = new AsyncConditionVariable(mutex);

			await mutex.LockAsync();
			Task task = cv.WaitAsync();

			await AsyncAssert.DoesNotCompleteAsync(task);
		}

		[Fact]
		public async Task WaitAsync_Notified_IsCompleted()
		{
			var mutex = new AsyncLock();
			var cv = new AsyncConditionVariable(mutex);
			await mutex.LockAsync();
			Task task = cv.WaitAsync();

			await Task.Run(
				async () =>
				{
					using (await mutex.LockAsync())
					{
						cv.Notify();
					}
				});
			await task;
		}

		[Fact]
		public async Task WaitAsync_AfterNotify_IsNotCompleted()
		{
			var mutex = new AsyncLock();
			var cv = new AsyncConditionVariable(mutex);
			await Task.Run(
				async () =>
				{
					using (await mutex.LockAsync())
					{
						cv.Notify();
					}
				});

			await mutex.LockAsync();
			Task task = cv.WaitAsync();

			await AsyncAssert.DoesNotCompleteAsync(task);
		}

		[Fact]
		public async Task MultipleWaits_NotifyAll_AllAreCompleted()
		{
			var mutex = new AsyncLock();
			var cv = new AsyncConditionVariable(mutex);
			IDisposable key1 = await mutex.LockAsync();
			Task task1 = cv.WaitAsync();
			Task __ = task1.ContinueWith(_ => key1.Dispose());
			IDisposable key2 = await mutex.LockAsync();
			Task task2 = cv.WaitAsync();
			Task ___ = task2.ContinueWith(_ => key2.Dispose());

			await Task.Run(
				async () =>
				{
					using (await mutex.LockAsync())
					{
						cv.NotifyAll();
					}
				});

			await task1;
			await task2;
		}

		[Fact]
		public async Task MultipleWaits_Notify_OneIsCompleted()
		{
			var mutex = new AsyncLock();
			var cv = new AsyncConditionVariable(mutex);
			IDisposable key = await mutex.LockAsync();
			Task task1 = cv.WaitAsync();
			Task __ = task1.ContinueWith(_ => key.Dispose());
			await mutex.LockAsync();
			Task task2 = cv.WaitAsync();

			await Task.Run(
				async () =>
				{
					using (await mutex.LockAsync())
					{
						cv.Notify();
					}
				});

			await task1;
			await AsyncAssert.DoesNotCompleteAsync(task2);
		}

		[Fact]
		public void Id_IsNotZero()
		{
			var mutex = new AsyncLock();
			var cv = new AsyncConditionVariable(mutex);
			Assert.NotEqual(0, cv.Id);
		}
	}

}
