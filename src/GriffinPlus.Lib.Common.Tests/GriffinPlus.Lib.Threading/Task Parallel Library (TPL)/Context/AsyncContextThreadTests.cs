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

using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace GriffinPlus.Lib.Threading
{

	public class AsyncContextThreadTests
	{
		[Fact]
		public async Task AsyncContextThread_IsAnIndependentThread()
		{
			int testThread = Thread.CurrentThread.ManagedThreadId;
			var thread = new AsyncContextThread();
			int contextThread = await thread.Factory.Run(() => Thread.CurrentThread.ManagedThreadId);
			Assert.NotEqual(testThread, contextThread);
			await thread.JoinAsync();
		}

		[Fact]
		public async Task AsyncDelegate_ResumesOnSameThread()
		{
			var thread = new AsyncContextThread();
			int contextThread = -1, resumeThread = -1;
			await thread.Factory.Run(
				async () =>
				{
					contextThread = Thread.CurrentThread.ManagedThreadId;
					await Task.Yield();
					resumeThread = Thread.CurrentThread.ManagedThreadId;
				});
			Assert.Equal(contextThread, resumeThread);
			await thread.JoinAsync();
		}

		[Fact]
		public async Task Join_StopsTask()
		{
			var context = new AsyncContextThread();
			var thread = await context.Factory.Run(() => Thread.CurrentThread);
			await context.JoinAsync();
		}

		[Fact]
		public async Task Context_IsCorrectAsyncContext()
		{
			using (var thread = new AsyncContextThread())
			{
				var observedContext = await thread.Factory.Run(() => AsyncContext.Current);
				Assert.Same(observedContext, thread.Context);
			}
		}
	}

}
