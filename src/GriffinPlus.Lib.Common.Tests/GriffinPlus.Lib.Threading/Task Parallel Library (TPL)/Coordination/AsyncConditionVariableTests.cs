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

using GriffinPlus.Lib.Tests;
using System.Threading.Tasks;
using Xunit;

namespace GriffinPlus.Lib.Threading
{
	public class AsyncConditionVariableTests
	{
		[Fact]
		public async Task WaitAsync_WithoutNotify_IsNotCompleted()
		{
			var mutex = new AsyncLock();
			var cv = new AsyncConditionVariable(mutex);

			await mutex.LockAsync();
			var task = cv.WaitAsync();

			await AsyncAssert.DoesNotCompleteAsync(task);
		}

		[Fact]
		public async Task WaitAsync_Notified_IsCompleted()
		{
			var mutex = new AsyncLock();
			var cv = new AsyncConditionVariable(mutex);
			await mutex.LockAsync();
			var task = cv.WaitAsync();

			await Task.Run(async () =>
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
			await Task.Run(async () =>
			{
				using (await mutex.LockAsync())
				{
					cv.Notify();
				}
			});

			await mutex.LockAsync();
			var task = cv.WaitAsync();

			await AsyncAssert.DoesNotCompleteAsync(task);
		}

		[Fact]
		public async Task MultipleWaits_NotifyAll_AllAreCompleted()
		{
			var mutex = new AsyncLock();
			var cv = new AsyncConditionVariable(mutex);
			var key1 = await mutex.LockAsync();
			var task1 = cv.WaitAsync();
			var __ = task1.ContinueWith(_ => key1.Dispose());
			var key2 = await mutex.LockAsync();
			var task2 = cv.WaitAsync();
			var ___ = task2.ContinueWith(_ => key2.Dispose());

			await Task.Run(async () =>
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
			var key = await mutex.LockAsync();
			var task1 = cv.WaitAsync();
			var __ = task1.ContinueWith(_ => key.Dispose());
			await mutex.LockAsync();
			var task2 = cv.WaitAsync();

			await Task.Run(async () =>
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