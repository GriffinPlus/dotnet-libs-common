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

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using GriffinPlus.Lib.Disposables;

namespace GriffinPlus.Lib.Threading
{

	/// <summary>
	/// A reader/writer lock that is compatible with async.
	/// Note that this lock is <b>not</b> recursive!
	/// </summary>
	[DebuggerDisplay("Id = {Id}, State = {GetStateForDebugger}, ReaderCount = {GetReaderCountForDebugger}")]
	[DebuggerTypeProxy(typeof(DebugView))]
	public sealed class AsyncReaderWriterLock
	{
		/// <summary>
		/// The queue of TCSs that other tasks are awaiting to acquire the lock as writers.
		/// </summary>
		private readonly IAsyncWaitQueue<IDisposable> mWriterQueue;

		/// <summary>
		/// The queue of TCSs that other tasks are awaiting to acquire the lock as readers.
		/// </summary>
		private readonly IAsyncWaitQueue<IDisposable> mReaderQueue;

		/// <summary>
		/// The object used for mutual exclusion.
		/// </summary>
		private readonly object mMutex;

		/// <summary>
		/// The semi-unique identifier for this instance.
		/// This is 0 if the id has not yet been created.
		/// </summary>
		private int mId;

		/// <summary>
		/// Number of reader locks held;
		/// -1 if a writer lock is held;
		/// 0 if no locks are held.
		/// </summary>
		private int mLocksHeld;

		[DebuggerNonUserCode]
		internal State GetStateForDebugger
		{
			get
			{
				if (mLocksHeld == 0) return State.Unlocked;
				if (mLocksHeld == -1) return State.WriteLocked;
				return State.ReadLocked;
			}
		}

		internal enum State
		{
			Unlocked,
			ReadLocked,
			WriteLocked
		}

		[DebuggerNonUserCode]
		internal int GetReaderCountForDebugger => mLocksHeld > 0 ? mLocksHeld : 0;

		/// <summary>
		/// Creates a new async-compatible reader/writer lock.
		/// </summary>
		/// <param name="writerQueue">The wait queue used to manage waiters for writer locks. This may be <c>null</c> to use a default (FIFO) queue.</param>
		/// <param name="readerQueue">The wait queue used to manage waiters for reader locks. This may be <c>null</c> to use a default (FIFO) queue.</param>
		internal AsyncReaderWriterLock(IAsyncWaitQueue<IDisposable> writerQueue, IAsyncWaitQueue<IDisposable> readerQueue)
		{
			mWriterQueue = writerQueue ?? new DefaultAsyncWaitQueue<IDisposable>();
			mReaderQueue = readerQueue ?? new DefaultAsyncWaitQueue<IDisposable>();
			mMutex = new object();
		}

		/// <summary>
		/// Creates a new async-compatible reader/writer lock.
		/// </summary>
		public AsyncReaderWriterLock()
			: this(null, null) { }

		/// <summary>
		/// Gets a semi-unique identifier for this asynchronous lock.
		/// </summary>
		public int Id => IdManager<AsyncReaderWriterLock>.GetId(ref mId);

		/// <summary>
		/// Applies a continuation to the task that will call <see cref="ReleaseWaiters"/>, if the task is canceled.
		/// This method may not be called while holding the sync lock.
		/// </summary>
		/// <param name="task">The task to observe for cancellation.</param>
		private void ReleaseWaitersWhenCanceled(Task task)
		{
			task.ContinueWith(
				t =>
				{
					lock (mMutex) { ReleaseWaiters(); }
				},
				CancellationToken.None,
				TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.Default);
		}

		/// <summary>
		/// Asynchronously acquires the lock as a reader.
		/// Returns a disposable that releases the lock when disposed.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation token used to cancel the lock.
		/// If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).
		/// </param>
		/// <returns>A disposable that releases the lock when disposed.</returns>
		private Task<IDisposable> RequestReaderLockAsync(CancellationToken cancellationToken)
		{
			lock (mMutex)
			{
				// If the lock is available or in read mode and there are no waiting writers, take it immediately.
				if (mLocksHeld >= 0 && mWriterQueue.IsEmpty)
				{
					++mLocksHeld;
					return Task.FromResult<IDisposable>(new ReaderKey(this));
				}

				// Wait for the lock to become available or cancellation.
				return mReaderQueue.Enqueue(mMutex, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously acquires the lock as a reader.
		/// Returns a disposable that releases the lock when disposed.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation token used to cancel the lock.
		/// If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).
		/// </param>
		/// <returns>A disposable that releases the lock when disposed.</returns>
		public AwaitableDisposable<IDisposable> ReaderLockAsync(CancellationToken cancellationToken)
		{
			return new AwaitableDisposable<IDisposable>(RequestReaderLockAsync(cancellationToken));
		}

		/// <summary>
		/// Asynchronously acquires the lock as a reader.
		/// Returns a disposable that releases the lock when disposed.
		/// </summary>
		/// <returns>A disposable that releases the lock when disposed.</returns>
		public AwaitableDisposable<IDisposable> ReaderLockAsync()
		{
			return ReaderLockAsync(CancellationToken.None);
		}

		/// <summary>
		/// Synchronously acquires the lock as a reader.
		/// Returns a disposable that releases the lock when disposed.
		/// This method may block the calling thread.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation token used to cancel the lock.
		/// If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).
		/// </param>
		/// <returns>A disposable that releases the lock when disposed.</returns>
		public IDisposable ReaderLock(CancellationToken cancellationToken)
		{
			return RequestReaderLockAsync(cancellationToken).WaitAndUnwrapException();
		}

		/// <summary>
		/// Synchronously acquires the lock as a reader.
		/// Returns a disposable that releases the lock when disposed.
		/// This method may block the calling thread.
		/// </summary>
		/// <returns>A disposable that releases the lock when disposed.</returns>
		public IDisposable ReaderLock()
		{
			return ReaderLock(CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously acquires the lock as a writer.
		/// Returns a disposable that releases the lock when disposed.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation token used to cancel the lock.
		/// If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).
		/// </param>
		/// <returns>A disposable that releases the lock when disposed.</returns>
		private Task<IDisposable> RequestWriterLockAsync(CancellationToken cancellationToken)
		{
			Task<IDisposable> task;
			lock (mMutex)
			{
				// If the lock is available, take it immediately.
				if (mLocksHeld == 0)
				{
					mLocksHeld = -1;
					task = Task.FromResult<IDisposable>(new WriterKey(this));
				}
				else
				{
					// Wait for the lock to become available or cancellation.
					task = mWriterQueue.Enqueue(mMutex, cancellationToken);
				}
			}

			ReleaseWaitersWhenCanceled(task);
			return task;
		}

		/// <summary>
		/// Asynchronously acquires the lock as a writer.
		/// Returns a disposable that releases the lock when disposed.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation token used to cancel the lock.
		/// If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).
		/// </param>
		/// <returns>A disposable that releases the lock when disposed.</returns>
		public AwaitableDisposable<IDisposable> WriterLockAsync(CancellationToken cancellationToken)
		{
			return new AwaitableDisposable<IDisposable>(RequestWriterLockAsync(cancellationToken));
		}

		/// <summary>
		/// Asynchronously acquires the lock as a writer.
		/// Returns a disposable that releases the lock when disposed.
		/// </summary>
		/// <returns>A disposable that releases the lock when disposed.</returns>
		public AwaitableDisposable<IDisposable> WriterLockAsync()
		{
			return WriterLockAsync(CancellationToken.None);
		}

		/// <summary>
		/// Synchronously acquires the lock as a writer.
		/// Returns a disposable that releases the lock when disposed.
		/// This method may block the calling thread.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation token used to cancel the lock.
		/// If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).
		/// </param>
		/// <returns>A disposable that releases the lock when disposed.</returns>
		public IDisposable WriterLock(CancellationToken cancellationToken)
		{
			return RequestWriterLockAsync(cancellationToken).WaitAndUnwrapException();
		}

		/// <summary>
		/// Asynchronously acquires the lock as a writer.
		/// Returns a disposable that releases the lock when disposed.
		/// This method may block the calling thread.
		/// </summary>
		/// <returns>A disposable that releases the lock when disposed.</returns>
		public IDisposable WriterLock()
		{
			return WriterLock(CancellationToken.None);
		}

		/// <summary>
		/// Grants lock(s) to waiting tasks.
		/// This method assumes the sync lock is already held.
		/// </summary>
		private void ReleaseWaiters()
		{
			if (mLocksHeld == -1)
				return;

			// Give priority to writers, then readers.
			if (!mWriterQueue.IsEmpty)
			{
				if (mLocksHeld == 0)
				{
					mLocksHeld = -1;
					mWriterQueue.Dequeue(new WriterKey(this));
				}
			}
			else
			{
				while (!mReaderQueue.IsEmpty)
				{
					mReaderQueue.Dequeue(new ReaderKey(this));
					++mLocksHeld;
				}
			}
		}

		/// <summary>
		/// Releases the lock as a reader.
		/// </summary>
		internal void ReleaseReaderLock()
		{
			lock (mMutex)
			{
				--mLocksHeld;
				ReleaseWaiters();
			}
		}

		/// <summary>
		/// Releases the lock as a writer.
		/// </summary>
		internal void ReleaseWriterLock()
		{
			lock (mMutex)
			{
				mLocksHeld = 0;
				ReleaseWaiters();
			}
		}

		/// <summary>
		/// The disposable which releases the reader lock.
		/// </summary>
		private sealed class ReaderKey : SingleDisposable<AsyncReaderWriterLock>
		{
			/// <summary>
			/// Creates the key for a lock.
			/// </summary>
			/// <param name="asyncReaderWriterLock">The lock to release. May not be <c>null</c>.</param>
			public ReaderKey(AsyncReaderWriterLock asyncReaderWriterLock)
				: base(asyncReaderWriterLock) { }

			protected override void Dispose(AsyncReaderWriterLock context)
			{
				context.ReleaseReaderLock();
			}
		}

		/// <summary>
		/// The disposable which releases the writer lock.
		/// </summary>
		private sealed class WriterKey : SingleDisposable<AsyncReaderWriterLock>
		{
			/// <summary>
			/// Creates the key for a lock.
			/// </summary>
			/// <param name="asyncReaderWriterLock">The lock to release. May not be <c>null</c>.</param>
			public WriterKey(AsyncReaderWriterLock asyncReaderWriterLock)
				: base(asyncReaderWriterLock) { }

			protected override void Dispose(AsyncReaderWriterLock context)
			{
				context.ReleaseWriterLock();
			}
		}

		// ReSharper disable UnusedMember.Local
		[DebuggerNonUserCode]
		private sealed class DebugView
		{
			private readonly AsyncReaderWriterLock mArwLock;

			public DebugView(AsyncReaderWriterLock arwLock)
			{
				mArwLock = arwLock;
			}

			public int                          Id              => mArwLock.Id;
			public State                        State           => mArwLock.GetStateForDebugger;
			public int                          ReaderCount     => mArwLock.GetReaderCountForDebugger;
			public IAsyncWaitQueue<IDisposable> ReaderWaitQueue => mArwLock.mReaderQueue;
			public IAsyncWaitQueue<IDisposable> WriterWaitQueue => mArwLock.mWriterQueue;
		}
		// ReSharper restore UnusedMember.Local
	}

}
