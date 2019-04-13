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

using System;
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
	/// <typeparam name="T">The type of the results. If this isn't needed, use <see cref="Object"/>.</typeparam>
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(DefaultAsyncWaitQueue<>.DebugView))]
	internal sealed class DefaultAsyncWaitQueue<T> : IAsyncWaitQueue<T>
	{
		private readonly Deque<TaskCompletionSource<T>> mQueue = new Deque<TaskCompletionSource<T>>();

		private int Count => mQueue.Count;

		bool IAsyncWaitQueue<T>.IsEmpty => Count == 0;

		Task<T> IAsyncWaitQueue<T>.Enqueue()
		{
			var tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<T>();
			mQueue.AddToBack(tcs);
			return tcs.Task;
		}

		void IAsyncWaitQueue<T>.Dequeue(T result)
		{
			mQueue.RemoveFromFront().TrySetResult(result);
		}

		void IAsyncWaitQueue<T>.DequeueAll(T result)
		{
			foreach (var source in mQueue)
				source.TrySetResult(result);
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
			foreach (var source in mQueue)
				source.TrySetCanceled(cancellationToken);

			mQueue.Clear();
		}

		[DebuggerNonUserCode]
		internal sealed class DebugView
		{
			private readonly DefaultAsyncWaitQueue<T> mQueue;

			public DebugView(DefaultAsyncWaitQueue<T> queue)
			{
				mQueue = queue;
			}

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public Task<T>[] Tasks
			{
				get
				{
					var result = new List<Task<T>>(mQueue.mQueue.Count);
					foreach (var entry in mQueue.mQueue)
						result.Add(entry.Task);
					return result.ToArray();
				}
			}
		}
	}
}
