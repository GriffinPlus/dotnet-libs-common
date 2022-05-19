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
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace GriffinPlus.Lib.Threading
{

	public class AsyncContextTests
	{
		[Fact]
		public void AsyncContext_StaysOnSameThread()
		{
			int testThread = Thread.CurrentThread.ManagedThreadId;
			int contextThread = AsyncContext.Run(() => Thread.CurrentThread.ManagedThreadId);
			Assert.Equal(testThread, contextThread);
		}

		[Fact]
		public void Run_AsyncVoid_BlocksUntilCompletion()
		{
			bool resumed = false;
			AsyncContext.Run(
				// ReSharper disable once AsyncVoidLambda
				(Action)(async () =>
					        {
						        await Task.Yield();
						        resumed = true;
					        }));
			Assert.True(resumed);
		}

		[Fact]
		public void Run_FuncThatCallsAsyncVoid_BlocksUntilCompletion()
		{
			bool resumed = false;
			int result = AsyncContext.Run(
				() =>
				{
					async void Action()
					{
						await Task.Yield();
						resumed = true;
					}

					Action();
					return 13;
				});
			Assert.True(resumed);
			Assert.Equal(13, result);
		}

		[Fact]
		public void Run_AsyncTask_BlocksUntilCompletion()
		{
			bool resumed = false;
			AsyncContext.Run(
				async () =>
				{
					await Task.Yield();
					resumed = true;
				});
			Assert.True(resumed);
		}

		[Fact]
		public void Run_AsyncTaskWithResult_BlocksUntilCompletion()
		{
			bool resumed = false;
			int result = AsyncContext.Run(
				async () =>
				{
					await Task.Yield();
					resumed = true;
					return 17;
				});
			Assert.True(resumed);
			Assert.Equal(17, result);
		}

		[Fact]
		public void Current_WithoutAsyncContext_IsNull()
		{
			Assert.Null(AsyncContext.Current);
		}

		[Fact]
		public void Current_FromAsyncContext_IsAsyncContext()
		{
			AsyncContext observedContext = null;
			var context = new AsyncContext();
			context.Factory.Run(
				() =>
				{
					observedContext = AsyncContext.Current;
				});

			context.Execute();

			Assert.Same(context, observedContext);
		}

		[Fact]
		public void SynchronizationContextCurrent_FromAsyncContext_IsAsyncContextSynchronizationContext()
		{
			SynchronizationContext observedContext = null;
			var context = new AsyncContext();
			context.Factory.Run(
				() =>
				{
					observedContext = SynchronizationContext.Current;
				});

			context.Execute();

			Assert.Same(context.SynchronizationContext, observedContext);
		}

		[Fact]
		public void TaskSchedulerCurrent_FromAsyncContext_IsThreadPoolTaskScheduler()
		{
			TaskScheduler observedScheduler = null;
			var context = new AsyncContext();
			context.Factory.Run(
				() =>
				{
					observedScheduler = TaskScheduler.Current;
				});

			context.Execute();

			Assert.Same(TaskScheduler.Default, observedScheduler);
		}

		[Fact]
		public void TaskScheduler_MaximumConcurrency_IsOne()
		{
			var context = new AsyncContext();
			Assert.Equal(1, context.Scheduler.MaximumConcurrencyLevel);
		}

		[Fact]
		public void Run_PropagatesException()
		{
			void Test()
			{
				AsyncContext.Run(() => throw new NotImplementedException());
			}

			Assert.Throws<NotImplementedException>(Test);
		}

		[Fact]
		public void Run_Async_PropagatesException()
		{
			void Test()
			{
				AsyncContext.Run(
					async () =>
					{
						await Task.Yield();
						throw new NotImplementedException();
					});
			}

			Assert.Throws<NotImplementedException>(Test);
		}

		[Fact]
		public void SynchronizationContextPost_PropagatesException()
		{
			void Test()
			{
				AsyncContext.Run(
					async () =>
					{
						SynchronizationContext.Current.Post(_ => throw new NotImplementedException(), null);
						await Task.Yield();
					});
			}

			Assert.Throws<NotImplementedException>(Test);
		}

		[Fact]
		public async Task SynchronizationContext_Send_ExecutesSynchronously()
		{
			using (var thread = new AsyncContextThread())
			{
				var synchronizationContext = await thread.Factory.Run(() => SynchronizationContext.Current);
				int value = 0;
				synchronizationContext.Send(_ => { value = 13; }, null);
				Assert.Equal(13, value);
			}
		}

		[Fact]
		public async Task SynchronizationContext_Send_ExecutesInlineIfNecessary()
		{
			using (var thread = new AsyncContextThread())
			{
				int value = 0;
				await thread.Factory.Run(
					() =>
					{
						SynchronizationContext.Current.Send(_ => { value = 13; }, null);
						Assert.Equal(13, value);
					});
				Assert.Equal(13, value);
			}
		}

		[Fact]
		public void Task_AfterExecute_NeverRuns()
		{
			int value = 0;
			var context = new AsyncContext();
			context.Factory.Run(() => { value = 1; });
			context.Execute();

			var task = context.Factory.Run(() => { value = 2; });

			task.ContinueWith(_ => throw new Exception("Should not run"), TaskScheduler.Default);
			Assert.Equal(1, value);
		}

		[Fact]
		public void SynchronizationContext_IsEqualToCopyOfItself()
		{
			var synchronizationContext1 = AsyncContext.Run(() => SynchronizationContext.Current);
			var synchronizationContext2 = synchronizationContext1.CreateCopy();
			Assert.Equal(synchronizationContext1.GetHashCode(), synchronizationContext2.GetHashCode());
			Assert.True(synchronizationContext1.Equals(synchronizationContext2));
			Assert.False(synchronizationContext1.Equals(new SynchronizationContext()));
		}

		[Fact]
		public void Id_IsEqualToTaskSchedulerId()
		{
			var context = new AsyncContext();
			Assert.Equal(context.Scheduler.Id, context.Id);
		}
	}

}
