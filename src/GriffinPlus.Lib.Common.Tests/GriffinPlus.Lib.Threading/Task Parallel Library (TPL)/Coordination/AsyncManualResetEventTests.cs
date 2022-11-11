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

using System.Threading.Tasks;

using GriffinPlus.Lib.Tests;

using Xunit;

namespace GriffinPlus.Lib.Threading
{

	[Collection(nameof(NoParallelizationCollection))]
	public class AsyncManualResetEventTests
	{
		[Fact]
		public async Task WaitAsync_Unset_IsNotCompleted()
		{
			var mre = new AsyncManualResetEvent();

			Task task = mre.WaitAsync();

			await AsyncAssert.DoesNotCompleteAsync(task);
		}

		[Fact]
		public async Task Wait_Unset_IsNotCompleted()
		{
			var mre = new AsyncManualResetEvent();

			Task task = Task.Run(() => mre.Wait());

			await AsyncAssert.DoesNotCompleteAsync(task);
		}

		[Fact]
		public void WaitAsync_AfterSet_IsCompleted()
		{
			var mre = new AsyncManualResetEvent();

			mre.Set();
			Task task = mre.WaitAsync();

			Assert.True(task.IsCompleted);
		}

		[Fact]
		public void Wait_AfterSet_IsCompleted()
		{
			var mre = new AsyncManualResetEvent();

			mre.Set();
			mre.Wait();
		}

		[Fact]
		public void WaitAsync_Set_IsCompleted()
		{
			var mre = new AsyncManualResetEvent(true);

			Task task = mre.WaitAsync();

			Assert.True(task.IsCompleted);
		}

		[Fact]
		public void Wait_Set_IsCompleted()
		{
			var mre = new AsyncManualResetEvent(true);

			mre.Wait();
		}

		[Fact]
		public void MultipleWaitAsync_AfterSet_IsCompleted()
		{
			var mre = new AsyncManualResetEvent();

			mre.Set();
			Task task1 = mre.WaitAsync();
			Task task2 = mre.WaitAsync();

			Assert.True(task1.IsCompleted);
			Assert.True(task2.IsCompleted);
		}

		[Fact]
		public void MultipleWait_AfterSet_IsCompleted()
		{
			var mre = new AsyncManualResetEvent();

			mre.Set();
			mre.Wait();
			mre.Wait();
		}

		[Fact]
		public void MultipleWaitAsync_Set_IsCompleted()
		{
			var mre = new AsyncManualResetEvent(true);

			Task task1 = mre.WaitAsync();
			Task task2 = mre.WaitAsync();

			Assert.True(task1.IsCompleted);
			Assert.True(task2.IsCompleted);
		}

		[Fact]
		public void MultipleWait_Set_IsCompleted()
		{
			var mre = new AsyncManualResetEvent(true);

			mre.Wait();
			mre.Wait();
		}

		[Fact]
		public async Task WaitAsync_AfterReset_IsNotCompleted()
		{
			var mre = new AsyncManualResetEvent();

			mre.Set();
			mre.Reset();
			Task task = mre.WaitAsync();

			await AsyncAssert.DoesNotCompleteAsync(task);
		}

		[Fact]
		public async Task Wait_AfterReset_IsNotCompleted()
		{
			var mre = new AsyncManualResetEvent();

			mre.Set();
			mre.Reset();
			Task task = Task.Run(() => mre.Wait());

			await AsyncAssert.DoesNotCompleteAsync(task);
		}

		[Fact]
		public void Id_IsNotZero()
		{
			var mre = new AsyncManualResetEvent();
			Assert.NotEqual(0, mre.Id);
		}
	}

}
