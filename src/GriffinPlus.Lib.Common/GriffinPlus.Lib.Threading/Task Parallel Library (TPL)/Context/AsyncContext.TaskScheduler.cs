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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading
{
	public sealed partial class AsyncContext
	{
		/// <summary>
		/// A task scheduler which schedules tasks to an async context.
		/// </summary>
		private sealed class AsyncContextTaskScheduler : TaskScheduler
		{
			/// <summary>
			/// The async context for this task scheduler.
			/// </summary>
			private readonly AsyncContext mContext;

			/// <summary>
			/// Initializes a new instance of the <see cref="AsyncContextTaskScheduler"/> class.
			/// </summary>
			/// <param name="context">The async context for this task scheduler. May not be <c>null</c>.</param>
			public AsyncContextTaskScheduler(AsyncContext context)
			{
				mContext = context;
			}

			/// <summary>
			/// Generates an enumerable of <see cref="System.Threading.Tasks.Task"/> instances currently queued to the
			/// scheduler waiting to be executed.
			/// </summary>
			/// <returns>An enumerable that allows traversal of tasks currently queued to this scheduler.</returns>
			[System.Diagnostics.DebuggerNonUserCode]
			protected override IEnumerable<Task> GetScheduledTasks()
			{
				return mContext.mQueue.GetScheduledTasks();
			}

			/// <summary>
			/// Queues a <see cref="System.Threading.Tasks.Task"/> to the scheduler.
			/// If all tasks have been completed and the outstanding asynchronous operation count is zero, then this method has undefined behavior.
			/// </summary>
			/// <param name="task">The <see cref="System.Threading.Tasks.Task"/> to be queued.</param>
			protected override void QueueTask(Task task)
			{
				mContext.Enqueue(task, false);
			}

			/// <summary>
			/// Determines whether the provided <see cref="System.Threading.Tasks.Task"/> can be executed synchronously in
			/// this call, and if it can, executes it.
			/// </summary>
			/// <param name="task">The <see cref="System.Threading.Tasks.Task"/> to be executed.</param>
			/// <param name="taskWasPreviouslyQueued">
			/// true, if the task may have been previously queued (scheduled);
			/// false, if the task is known not to have been queued, and this call is being made in order to execute the task inline without queuing it.
			/// </param>
			/// <returns>
			/// true, if the task was executed inline; otherwise false.</returns>
			/// <exception cref="System.InvalidOperationException">The <paramref name="task"/> was already executed.</exception>
			protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
			{
				return (AsyncContext.Current == mContext) && TryExecuteTask(task);
			}

			/// <summary>
			/// Gets the maximum maximum concurrency level this <see cref="System.Threading.Tasks.TaskScheduler"/> is able to support.
			/// </summary>
			public override int MaximumConcurrencyLevel
			{
				get { return 1; }
			}

			/// <summary>
			/// Exposes the base <see cref="TaskScheduler.TryExecuteTask"/> method.
			/// </summary>
			/// <param name="task">The task to attempt to execute.</param>
			public void DoTryExecuteTask(Task task)
			{
				TryExecuteTask(task);
			}
		}
	}
}