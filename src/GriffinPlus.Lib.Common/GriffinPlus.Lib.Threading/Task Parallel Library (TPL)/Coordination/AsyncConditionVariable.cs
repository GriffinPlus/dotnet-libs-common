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
	/// An async-compatible condition variable.
	/// This type uses Mesa-style semantics (the notifying tasks do not yield).
	/// </summary>
	[DebuggerDisplay("Id = {Id}, AsyncLockId = {mAsyncLock.Id}")]
	[DebuggerTypeProxy(typeof(DebugView))]
	public sealed class AsyncConditionVariable
	{
		/// <summary>
		/// The lock associated with this condition variable.
		/// </summary>
		private readonly AsyncLock mAsyncLock;

		/// <summary>
		/// The queue of waiting tasks.
		/// </summary>
		private readonly IAsyncWaitQueue<object> mQueue;

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
		/// Creates an async-compatible condition variable associated with an async-compatible lock.
		/// </summary>
		/// <param name="asyncLock">The lock associated with this condition variable.</param>
		/// <param name="queue">
		/// The wait queue used to manage waiters.
		/// This may be <c>null</c> to use a default (FIFO) queue.
		/// </param>
		internal AsyncConditionVariable(AsyncLock asyncLock, IAsyncWaitQueue<object> queue)
		{
			mAsyncLock = asyncLock;
			mQueue = queue ?? new DefaultAsyncWaitQueue<object>();
			mMutex = new object();
		}

		/// <summary>
		/// Creates an async-compatible condition variable associated with an async-compatible lock.
		/// </summary>
		/// <param name="asyncLock">The lock associated with this condition variable.</param>
		public AsyncConditionVariable(AsyncLock asyncLock)
			: this(asyncLock, null) { }

		/// <summary>
		/// Gets a semi-unique identifier for this asynchronous condition variable.
		/// </summary>
		public int Id => IdManager<AsyncConditionVariable>.GetId(ref mId);

		/// <summary>
		/// Sends a signal to a single task waiting on this condition variable.
		/// The associated lock MUST be held when calling this method, and it will still be held when this method returns.
		/// </summary>
		public void Notify()
		{
			lock (mMutex)
			{
				if (!mQueue.IsEmpty) mQueue.Dequeue();
			}
		}

		/// <summary>
		/// Sends a signal to all tasks waiting on this condition variable.
		/// The associated lock MUST be held when calling this method, and it will still be held when this method returns.
		/// </summary>
		public void NotifyAll()
		{
			lock (mMutex)
			{
				mQueue.DequeueAll();
			}
		}

		/// <summary>
		/// Asynchronously waits for a signal on this condition variable.
		/// The associated lock MUST be held when calling this method, and it will still be held when this method returns,
		/// even if the method is cancelled.
		/// </summary>
		/// <param name="cancellationToken">The cancellation signal used to cancel this wait.</param>
		public Task WaitAsync(CancellationToken cancellationToken)
		{
			lock (mMutex)
			{
				// Begin waiting for either a signal or cancellation.
				Task task = mQueue.Enqueue(mMutex, cancellationToken);

				// Attach to the signal or cancellation.
				var ret = WaitAndRetakeLockAsync(task, mAsyncLock);

				// Release the lock while we are waiting.
				mAsyncLock.ReleaseLock();

				return ret;
			}
		}

		private static async Task WaitAndRetakeLockAsync(Task task, AsyncLock asyncLock)
		{
			try
			{
				await task.ConfigureAwait(false);
			}
			finally
			{
				// Re-take the lock.
				await asyncLock.LockAsync().ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Asynchronously waits for a signal on this condition variable.
		/// The associated lock MUST be held when calling this method, and it will still be held when the returned task completes.
		/// </summary>
		public Task WaitAsync()
		{
			return WaitAsync(CancellationToken.None);
		}

		/// <summary>
		/// Synchronously waits for a signal on this condition variable. This method may block the calling thread.
		/// The associated lock MUST be held when calling this method, and it will still be held when this method returns,
		/// even if the method is cancelled.
		/// </summary>
		/// <param name="cancellationToken">The cancellation signal used to cancel this wait.</param>
		public void Wait(CancellationToken cancellationToken)
		{
			WaitAsync(cancellationToken).WaitAndUnwrapException(CancellationToken.None);
		}

		/// <summary>
		/// Synchronously waits for a signal on this condition variable.
		/// This method may block the calling thread.
		/// The associated lock MUST be held when calling this method, and it will still be held when this method returns.
		/// </summary>
		public void Wait()
		{
			Wait(CancellationToken.None);
		}

		// ReSharper disable UnusedMember.Local
		[DebuggerNonUserCode]
		private sealed class DebugView
		{
			private readonly AsyncConditionVariable mCv;

			public DebugView(AsyncConditionVariable cv)
			{
				mCv = cv;
			}

			public int                     Id        => mCv.Id;
			public AsyncLock               AsyncLock => mCv.mAsyncLock;
			public IAsyncWaitQueue<object> WaitQueue => mCv.mQueue;
		}
		// ReSharper restore UnusedMember.Local
	}

}
