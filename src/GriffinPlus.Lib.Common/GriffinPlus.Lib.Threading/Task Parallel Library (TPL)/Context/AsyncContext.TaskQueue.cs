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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading
{
	public sealed partial class AsyncContext
	{
		/// <summary>
		/// A blocking queue.
		/// </summary>
		private sealed class TaskQueue : IDisposable
		{
			/// <summary>
			/// The underlying blocking collection.
			/// </summary>
			private readonly BlockingCollection<Tuple<Task, bool>> mQueue;

			/// <summary>
			/// Initializes a new instance of the <see cref="TaskQueue"/> class.
			/// </summary>
			public TaskQueue()
			{
				mQueue = new BlockingCollection<Tuple<Task, bool>>();
			}

			/// <summary>
			/// Gets a blocking enumerable that removes items from the queue.
			/// This enumerable only completes after <see cref="CompleteAdding"/> has been called.
			/// </summary>
			/// <returns>A blocking enumerable that removes items from the queue.</returns>
			public IEnumerable<Tuple<Task, bool>> GetConsumingEnumerable()
			{
				return mQueue.GetConsumingEnumerable();
			}

			/// <summary>
			/// Generates an enumerable of <see cref="System.Threading.Tasks.Task"/> instances currently queued to the
			/// scheduler waiting to be executed.
			/// </summary>
			/// <returns>An enumerable that allows traversal of tasks currently queued to this scheduler.</returns>
			[System.Diagnostics.DebuggerNonUserCode]
			internal IEnumerable<Task> GetScheduledTasks()
			{
				foreach (var item in mQueue) {
					yield return item.Item1;
				}
			}

			/// <summary>
			/// Attempts to add the item to the queue.
			/// If the queue has been marked as complete for adding, this method returns <c>false</c>.
			/// </summary>
			/// <param name="item">The item to enqueue.</param>
			/// <param name="propagateExceptions">
			/// <c>true</c> to propagate exceptions out of the main loop;
			/// <c>false</c> to discard exceptions.
			/// </param>
			public bool TryAdd(Task item, bool propagateExceptions)
			{
				try
				{
					return mQueue.TryAdd(Tuple.Create(item, propagateExceptions));
				}
				catch (InvalidOperationException)
				{
					// vexing exception
					return false;
				}
			}

			/// <summary>
			/// Marks the queue as complete for adding, allowing the enumerator returned from <see cref="GetConsumingEnumerable"/>
			/// to eventually complete. This method may be called several times.
			/// </summary>
			public void CompleteAdding()
			{
				mQueue.CompleteAdding();
			}

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose()
			{
				mQueue.Dispose();
			}
		}
	}
}