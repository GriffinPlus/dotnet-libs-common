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

using Xunit;

namespace GriffinPlus.Lib.Threading
{

	[Collection(nameof(NoParallelizationCollection))]
	public class AsyncCountdownEventTests
	{
		[Fact]
		public async Task WaitAsync_Unset_IsNotCompleted()
		{
			var ce = new AsyncCountdownEvent(1);
			var task = ce.WaitAsync();

			Assert.Equal(1, ce.CurrentCount);
			Assert.False(task.IsCompleted);

			ce.Signal();
			await task;
		}

		[Fact]
		public void WaitAsync_Set_IsCompleted()
		{
			var ce = new AsyncCountdownEvent(0);
			var task = ce.WaitAsync();

			Assert.Equal(0, ce.CurrentCount);
			Assert.True(task.IsCompleted);
		}

		[Fact]
		public async Task AddCount_IncrementsCount()
		{
			var ce = new AsyncCountdownEvent(1);
			var task = ce.WaitAsync();
			Assert.Equal(1, ce.CurrentCount);
			Assert.False(task.IsCompleted);

			ce.AddCount();

			Assert.Equal(2, ce.CurrentCount);
			Assert.False(task.IsCompleted);

			ce.Signal(2);
			await task;
		}

		[Fact]
		public async Task Signal_Nonzero_IsNotCompleted()
		{
			var ce = new AsyncCountdownEvent(2);
			var task = ce.WaitAsync();
			Assert.False(task.IsCompleted);

			ce.Signal();

			Assert.Equal(1, ce.CurrentCount);
			Assert.False(task.IsCompleted);

			ce.Signal();
			await task;
		}

		[Fact]
		public void Signal_Zero_SynchronouslyCompletesWaitTask()
		{
			var ce = new AsyncCountdownEvent(1);
			var task = ce.WaitAsync();
			Assert.False(task.IsCompleted);

			ce.Signal();

			Assert.Equal(0, ce.CurrentCount);
			Assert.True(task.IsCompleted);
		}

		[Fact]
		public async Task Signal_AfterSet_CountsNegativeAndResetsTask()
		{
			var ce = new AsyncCountdownEvent(0);
			var originalTask = ce.WaitAsync();

			ce.Signal();

			var newTask = ce.WaitAsync();
			Assert.Equal(-1, ce.CurrentCount);
			Assert.NotSame(originalTask, newTask);

			ce.AddCount();
			await newTask;
		}

		[Fact]
		public async Task AddCount_AfterSet_CountsPositiveAndResetsTask()
		{
			var ce = new AsyncCountdownEvent(0);
			var originalTask = ce.WaitAsync();

			ce.AddCount();
			var newTask = ce.WaitAsync();

			Assert.Equal(1, ce.CurrentCount);
			Assert.NotSame(originalTask, newTask);

			ce.Signal();
			await newTask;
		}

		[Fact]
		public async Task Signal_PastZero_PulsesTask()
		{
			var ce = new AsyncCountdownEvent(1);
			var originalTask = ce.WaitAsync();

			ce.Signal(2);
			await originalTask;
			var newTask = ce.WaitAsync();

			Assert.Equal(-1, ce.CurrentCount);
			Assert.NotSame(originalTask, newTask);

			ce.AddCount();
			await newTask;
		}

		[Fact]
		public async Task AddCount_PastZero_PulsesTask()
		{
			var ce = new AsyncCountdownEvent(-1);
			var originalTask = ce.WaitAsync();

			ce.AddCount(2);
			await originalTask;
			var newTask = ce.WaitAsync();

			Assert.Equal(1, ce.CurrentCount);
			Assert.NotSame(originalTask, newTask);

			ce.Signal();
			await newTask;
		}

		[Fact]
		public void AddCount_Overflow_ThrowsException()
		{
			var ce = new AsyncCountdownEvent(long.MaxValue);
			Assert.ThrowsAny<OverflowException>(() => ce.AddCount());
		}

		[Fact]
		public void Signal_Underflow_ThrowsException()
		{
			var ce = new AsyncCountdownEvent(long.MinValue);
			Assert.ThrowsAny<OverflowException>(() => ce.Signal());
		}

		[Fact]
		public void Id_IsNotZero()
		{
			var ce = new AsyncCountdownEvent(0);
			Assert.NotEqual(0, ce.Id);
		}
	}

}
