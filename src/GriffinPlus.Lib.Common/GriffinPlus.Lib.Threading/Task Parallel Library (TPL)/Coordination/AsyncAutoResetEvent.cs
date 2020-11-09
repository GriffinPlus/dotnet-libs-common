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

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading
{
	/// <summary>
	/// An async-compatible auto-reset event.
	/// </summary>
	[DebuggerDisplay("Id = {Id}, IsSet = {mSet}")]
	[DebuggerTypeProxy(typeof(DebugView))]
	public sealed class AsyncAutoResetEvent
	{
		/// <summary>
		/// The queue of TCSs that other tasks are awaiting.
		/// </summary>
		private readonly IAsyncWaitQueue<object> mQueue;

		/// <summary>
		/// The current state of the event.
		/// </summary>
		private bool mSet;

		/// <summary>
		/// The semi-unique identifier for this instance.
		/// This is 0 if the id has not yet been created.
		/// </summary>
		private int mId;

		/// <summary>
		/// The object used for mutual exclusion.
		/// </summary>
		private readonly object mMutex;

		/// <summary>
		/// Creates an async-compatible auto-reset event.
		/// </summary>
		/// <param name="set">Whether the auto-reset event is initially set or unset.</param>
		/// <param name="queue">
		/// The wait queue used to manage waiters.
		/// This may be <c>null</c> to use a default (FIFO) queue.
		/// </param>
		internal AsyncAutoResetEvent(bool set, IAsyncWaitQueue<object> queue)
		{
			mQueue = queue ?? new DefaultAsyncWaitQueue<object>();
			mSet = set;
			mMutex = new object();
		}

		/// <summary>
		/// Creates an async-compatible auto-reset event.
		/// </summary>
		/// <param name="set">
		/// <c>true</c>, if the auto-reset event is set initially;
		/// <c>false</c>, if the auto-reset event is not set initially.
		/// </param>
		public AsyncAutoResetEvent(bool set)
			: this(set, null)
		{
		}

		/// <summary>
		/// Creates an async-compatible auto-reset event that is initially unset.
		/// </summary>
		public AsyncAutoResetEvent()
		  : this(false, null)
		{
		}

		/// <summary>
		/// Gets a semi-unique identifier for this asynchronous auto-reset event.
		/// </summary>
		public int Id => IdManager<AsyncAutoResetEvent>.GetId(ref mId);

		/// <summary>
		/// Whether this event is currently set.
		/// This member is seldom used; code using this member has a high possibility of race conditions.
		/// </summary>
		public bool IsSet
		{
			get { lock (mMutex) return mSet; }
		}

		/// <summary>
		/// Asynchronously waits for this event to be set.
		/// If the event is set, this method will auto-reset it and return immediately, even if the cancellation token is already signaled.
		/// If the wait is canceled, then it will not auto-reset this event.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token used to cancel this wait.</param>
		public Task WaitAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			Task task;
			lock (mMutex)
			{
				if (mSet)
				{
					mSet = false;
					task = TaskConstants.Completed;
				}
				else
				{
					task = mQueue.Enqueue(mMutex, cancellationToken);
				}
			}

			return task;
		}

		/// <summary>
		/// Synchronously waits for this event to be set.
		/// If the event is set, this method will auto-reset it and return immediately, even if the cancellation token is already signaled.
		/// If the wait is canceled, then it will not auto-reset this event.
		/// This method may block the calling thread.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token used to cancel this wait.</param>
		public void Wait(CancellationToken cancellationToken = default(CancellationToken))
		{
			WaitAsync(cancellationToken).WaitAndUnwrapException(cancellationToken);
		}

		/// <summary>
		/// Sets the event, atomically completing a task returned by <see cref="WaitAsync(CancellationToken)"/>.
		/// If the event is already set, this method does nothing.
		/// </summary>
		public void Set()
		{
			lock (mMutex)
			{
				if (mQueue.IsEmpty) mSet = true;
				else                mQueue.Dequeue();
			}
		}

		// ReSharper disable UnusedMember.Local
		[DebuggerNonUserCode]
		private sealed class DebugView
		{
			private readonly AsyncAutoResetEvent mAre;

			public DebugView(AsyncAutoResetEvent are)
			{
				mAre = are;
			}

			public int Id => mAre.Id;
			public bool IsSet => mAre.mSet;
			public IAsyncWaitQueue<object> WaitQueue => mAre.mQueue;
		}
		// ReSharper restore UnusedMember.Local

	}
}
