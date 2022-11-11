///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
//
// This file incorporates work covered by the following copyright and permission notice:
//
//     MIT License
//
//     Copyright (c) 2014-2018 Stephen Cleary
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

	public class TaskCompletionSourceExtensionsTests
	{
		[Fact]
		public async Task TryCompleteFromCompletedTaskTResult_PropagatesResult()
		{
			var tcs = new TaskCompletionSource<int>();
			tcs.TryCompleteFromCompletedTask(TaskConstants.Int32NegativeOne);
			int result = await tcs.Task;
			Assert.Equal(-1, result);
		}

		[Fact]
		public async Task TryCompleteFromCompletedTaskTResult_WithDifferentTResult_PropagatesResult()
		{
			var tcs = new TaskCompletionSource<object>();
			tcs.TryCompleteFromCompletedTask(TaskConstants.Int32NegativeOne);
			object result = await tcs.Task;
			Assert.Equal(-1, result);
		}

		[Fact]
		public async Task TryCompleteFromCompletedTaskTResult_PropagatesCancellation()
		{
			var tcs = new TaskCompletionSource<int>();
			tcs.TryCompleteFromCompletedTask(TaskConstants<int>.Canceled);
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => tcs.Task);
		}

		[Fact]
		public async Task TryCompleteFromCompletedTaskTResult_PropagatesException()
		{
			var source = new TaskCompletionSource<int>();
			source.TrySetException(new NotImplementedException());

			var tcs = new TaskCompletionSource<int>();
			tcs.TryCompleteFromCompletedTask(source.Task);
			await Assert.ThrowsAsync<NotImplementedException>(() => tcs.Task);
		}

		[Fact]
		public async Task TryCompleteFromCompletedTask_PropagatesResult()
		{
			var tcs = new TaskCompletionSource<int>();
			tcs.TryCompleteFromCompletedTask(TaskConstants.Completed, () => -1);
			int result = await tcs.Task;
			Assert.Equal(-1, result);
		}

		[Fact]
		public async Task TryCompleteFromCompletedTask_PropagatesCancellation()
		{
			var tcs = new TaskCompletionSource<int>();
			tcs.TryCompleteFromCompletedTask(TaskConstants.Canceled, () => -1);
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => tcs.Task);
		}

		[Fact]
		public async Task TryCompleteFromCompletedTask_PropagatesException()
		{
			var tcs = new TaskCompletionSource<int>();
			tcs.TryCompleteFromCompletedTask(Task.FromException(new NotImplementedException()), () => -1);
			await Assert.ThrowsAsync<NotImplementedException>(() => tcs.Task);
		}

		[Fact]
		public async Task CreateAsyncTaskSource_PermitsCompletingTask()
		{
			TaskCompletionSource<object> tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
			tcs.SetResult(null);

			await tcs.Task;
		}
	}

}
