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
	/// A mutual exclusion lock that is compatible with async.
	/// Note that this lock is <b>not</b> recursive!
	/// </summary>
	/// <remarks>
	///     <para>
	///     This is the async/await-ready almost-equivalent of the <c>lock</c> keyword or the <see cref="Mutex"/> type, similar to
	///     <a href="http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx">Stephen Toub´s AsyncLock</a>. It's only
	///     <i>almost</i> equivalent because the <c>lock</c> keyword permits reentrancy, which is not currently possible to do with
	///     an <c>async</c>-ready lock.
	///     </para>
	///     <para>
	///     An <see cref="AsyncLock"/> is either taken or not. The lock can be asynchronously acquired by calling <see cref="LockAsync()"/>,
	///     and it is released by disposing the result of that task. It takes an optional <see cref="CancellationToken"/>, which can be used
	///     to cancel the acquiring of the lock.
	///     </para>
	///     <para>
	///     The task returned from <see cref="LockAsync()"/> will enter the <c>Completed</c> state when it has acquired the <see cref="AsyncLock"/>.
	///     The same task will enter the <c>Canceled</c> state if the <see cref="CancellationToken"/> is signaled before the wait is satisfied;
	///     in that case, the <see cref="AsyncLock"/> is not taken by that task.
	///     </para>
	///     <para>
	///     You can call <see cref="Lock()"/> or <see cref="LockAsync()"/> with an already-cancelled <see cref="CancellationToken"/> to attempt to
	///     acquire the <see cref="AsyncLock"/> immediately without actually entering the wait queue.
	///     </para>
	/// </remarks>
	/// <example>
	///     <para>The vast majority of use cases are to just replace a <c>lock</c> statement. That is, with the original code looking like this:</para>
	///     <code>
	/// private readonly object mMutex = new object();
	/// public void DoStuff()
	/// {
	///     lock (mMutex)
	///     {
	///         Thread.Sleep(TimeSpan.FromSeconds(1));
	///     }
	/// }
	/// </code>
	///     <para>
	///     If we want to replace the blocking operation <c>Thread.Sleep</c> with an asynchronous equivalent, it's not directly possible because
	///     of the <c>lock</c> block. We cannot <c>await</c> inside a <c>lock</c>.
	///     </para>
	///     <para>
	///     So, we use the <c>async</c>-compatible <see cref="AsyncLock"/> instead:
	///     </para>
	///     <code>
	/// private readonly AsyncLock mMutex = new AsyncLock();
	/// public async Task DoStuffAsync()
	/// {
	///     using (await mMutex.LockAsync())
	///     {
	///         await Task.Delay(TimeSpan.FromSeconds(1));
	///     }
	/// }
	/// </code>
	/// </example>
	[DebuggerDisplay("Id = {Id}, Taken = {mTaken}")]
	[DebuggerTypeProxy(typeof(DebugView))]
	public sealed class AsyncLock
	{
		/// <summary>
		/// Indicates whether the lock is taken by a task.
		/// </summary>
		private bool mTaken;

		/// <summary>
		/// The queue of TCSs that other tasks are awaiting to acquire the lock.
		/// </summary>
		private readonly IAsyncWaitQueue<IDisposable> mQueue;

		/// <summary>
		/// The semi-unique identifier for this instance. This is 0 if the id has not yet been created.
		/// </summary>
		private int mId;

		/// <summary>
		/// The object used for mutual exclusion.
		/// </summary>
		private readonly object mMutex;

		/// <summary>
		/// Creates a new async-compatible mutual exclusion lock.
		/// </summary>
		public AsyncLock()
			: this(null) { }

		/// <summary>
		/// Creates a new async-compatible mutual exclusion lock using the specified wait queue.
		/// </summary>
		/// <param name="queue">
		/// The wait queue used to manage waiters.
		/// This may be <c>null</c> to use a default (FIFO) queue.
		/// </param>
		internal AsyncLock(IAsyncWaitQueue<IDisposable> queue)
		{
			mQueue = queue ?? new DefaultAsyncWaitQueue<IDisposable>();
			mMutex = new object();
		}

		/// <summary>
		/// Gets a semi-unique identifier for this asynchronous lock.
		/// </summary>
		public int Id => IdManager<AsyncLock>.GetId(ref mId);

		/// <summary>
		/// Asynchronously acquires the lock.
		/// Returns a disposable that releases the lock when disposed.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation token used to cancel the lock.
		/// If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).
		/// </param>
		/// <returns>A disposable that releases the lock when disposed.</returns>
		private Task<IDisposable> RequestLockAsync(CancellationToken cancellationToken)
		{
			lock (mMutex)
			{
				if (!mTaken)
				{
					// If the lock is available, take it immediately.
					mTaken = true;
					return Task.FromResult<IDisposable>(new Key(this));
				}

				// Wait for the lock to become available or cancellation.
				return mQueue.Enqueue(mMutex, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously acquires the lock.
		/// Returns a disposable that releases the lock when disposed.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation token used to cancel the lock.
		/// If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).
		/// </param>
		/// <returns>A disposable that releases the lock when disposed.</returns>
		public AwaitableDisposable<IDisposable> LockAsync(CancellationToken cancellationToken)
		{
			return new AwaitableDisposable<IDisposable>(RequestLockAsync(cancellationToken));
		}

		/// <summary>
		/// Asynchronously acquires the lock.
		/// Returns a disposable that releases the lock when disposed.
		/// </summary>
		/// <returns>A disposable that releases the lock when disposed.</returns>
		public AwaitableDisposable<IDisposable> LockAsync()
		{
			return LockAsync(CancellationToken.None);
		}

		/// <summary>
		/// Synchronously acquires the lock.
		/// Returns a disposable that releases the lock when disposed.
		/// This method may block the calling thread.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation token used to cancel the lock.
		/// If this is already set, then this method will attempt to take the lock immediately (succeeding if the lock is currently available).
		/// </param>
		public IDisposable Lock(CancellationToken cancellationToken)
		{
			return RequestLockAsync(cancellationToken).WaitAndUnwrapException();
		}

		/// <summary>
		/// Synchronously acquires the lock.
		/// Returns a disposable that releases the lock when disposed.
		/// This method may block the calling thread.
		/// </summary>
		public IDisposable Lock()
		{
			return Lock(CancellationToken.None);
		}

		/// <summary>
		/// Releases the lock.
		/// </summary>
		internal void ReleaseLock()
		{
			lock (mMutex)
			{
				if (mQueue.IsEmpty)
					mTaken = false;
				else
					mQueue.Dequeue(new Key(this));
			}
		}

		/// <summary>
		/// The disposable which releases the lock.
		/// </summary>
		private sealed class Key : SingleDisposable<AsyncLock>
		{
			/// <summary>
			/// Creates the key for a lock.
			/// </summary>
			/// <param name="asyncLock">The lock to release. May not be <c>null</c>.</param>
			public Key(AsyncLock asyncLock)
				: base(asyncLock) { }

			protected override void Dispose(AsyncLock context)
			{
				context.ReleaseLock();
			}
		}

		[DebuggerNonUserCode]
		private sealed class DebugView(AsyncLock mutex)
		{
			public int                          Id        => mutex.Id;
			public bool                         Taken     => mutex.mTaken;
			public IAsyncWaitQueue<IDisposable> WaitQueue => mutex.mQueue;
		}
	}

}
