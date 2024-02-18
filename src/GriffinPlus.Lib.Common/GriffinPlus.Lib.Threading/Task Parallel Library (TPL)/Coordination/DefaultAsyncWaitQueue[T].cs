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

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using GriffinPlus.Lib.Collections;

namespace GriffinPlus.Lib.Threading
{

	/// <summary>
	/// The default wait queue implementation, which uses a double-ended queue.
	/// </summary>
	/// <typeparam name="T">The type of the results. If this isn't needed, use <see cref="object"/>.</typeparam>
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(DefaultAsyncWaitQueue<>.DebugView))]
	sealed class DefaultAsyncWaitQueue<T> : IAsyncWaitQueue<T>
	{
		private readonly Deque<TaskCompletionSource<T>> mQueue = [];

		private int Count => mQueue.Count;

		bool IAsyncWaitQueue<T>.IsEmpty => Count == 0;

		Task<T> IAsyncWaitQueue<T>.Enqueue()
		{
			TaskCompletionSource<T> tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<T>();
			mQueue.AddToBack(tcs);
			return tcs.Task;
		}

		void IAsyncWaitQueue<T>.Dequeue(T result)
		{
			mQueue.RemoveFromFront().TrySetResult(result);
		}

		void IAsyncWaitQueue<T>.DequeueAll(T result)
		{
			foreach (TaskCompletionSource<T> source in mQueue)
			{
				source.TrySetResult(result);
			}

			mQueue.Clear();
		}

		bool IAsyncWaitQueue<T>.TryCancel(Task task, CancellationToken cancellationToken)
		{
			for (int i = 0; i != mQueue.Count; ++i)
			{
				if (mQueue[i].Task == task)
				{
					mQueue[i].TrySetCanceled(cancellationToken);
					mQueue.RemoveAt(i);
					return true;
				}
			}

			return false;
		}

		void IAsyncWaitQueue<T>.CancelAll(CancellationToken cancellationToken)
		{
			foreach (TaskCompletionSource<T> source in mQueue)
			{
				source.TrySetCanceled(cancellationToken);
			}

			mQueue.Clear();
		}

		[DebuggerNonUserCode]
		internal sealed class DebugView(DefaultAsyncWaitQueue<T> queue)
		{
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public Task<T>[] Tasks
			{
				get
				{
					var result = new List<Task<T>>(queue.mQueue.Count);
					foreach (TaskCompletionSource<T> entry in queue.mQueue)
					{
						result.Add(entry.Task);
					}

					return result.ToArray();
				}
			}
		}
	}

}
