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

namespace GriffinPlus.Lib.Threading;

/// <summary>
/// An async-compatible semaphore. Alternatively, you could use <c>SemaphoreSlim</c>.
/// </summary>
[DebuggerDisplay("Id = {Id}, CurrentCount = {mCount}")]
[DebuggerTypeProxy(typeof(DebugView))]
public sealed class AsyncSemaphore
{
	/// <summary>
	/// The queue of TCSs that other tasks are awaiting to acquire the semaphore.
	/// </summary>
	private readonly IAsyncWaitQueue<object> mQueue;

	/// <summary>
	/// The number of waits that will be immediately granted.
	/// </summary>
	private long mCount;

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
	/// Creates a new async-compatible semaphore with the specified initial count.
	/// </summary>
	/// <param name="initialCount">
	/// The initial count for this semaphore.
	/// This must be greater than or equal to zero.
	/// </param>
	/// <param name="queue">
	/// The wait queue used to manage waiters.
	/// This may be <c>null</c> to use a default (FIFO) queue.
	/// </param>
	internal AsyncSemaphore(long initialCount, IAsyncWaitQueue<object> queue)
	{
		mQueue = queue ?? new DefaultAsyncWaitQueue<object>();
		mCount = initialCount;
		mMutex = new object();
	}

	/// <summary>
	/// Creates a new async-compatible semaphore with the specified initial count.
	/// </summary>
	/// <param name="initialCount">
	/// The initial count for this semaphore.
	/// This must be greater than or equal to zero.
	/// </param>
	public AsyncSemaphore(long initialCount)
		: this(initialCount, null) { }

	/// <summary>
	/// Gets a semi-unique identifier for this asynchronous semaphore.
	/// </summary>
	public int Id => IdManager<AsyncSemaphore>.GetId(ref mId);

	/// <summary>
	/// Gets the number of slots currently available on this semaphore.
	/// This member is seldom used; code using this member has a high possibility of race conditions.
	/// </summary>
	public long CurrentCount
	{
		get
		{
			lock (mMutex) return mCount;
		}
	}

	/// <summary>
	/// Asynchronously waits for a slot in the semaphore to be available.
	/// </summary>
	/// <param name="cancellationToken">
	/// The cancellation token used to cancel the wait.
	/// If this is already set, then this method will attempt to take the slot immediately (succeeding if a slot is currently available).
	/// </param>
	public Task WaitAsync(CancellationToken cancellationToken)
	{
		Task task;
		lock (mMutex)
		{
			// If the semaphore is available, take it immediately and return.
			if (mCount != 0)
			{
				--mCount;
				task = TaskConstants.Completed;
			}
			else
			{
				// Wait for the semaphore to become available or cancellation.
				task = mQueue.Enqueue(mMutex, cancellationToken);
			}
		}

		return task;
	}

	/// <summary>
	/// Asynchronously waits for a slot in the semaphore to be available.
	/// </summary>
	public Task WaitAsync()
	{
		return WaitAsync(CancellationToken.None);
	}

	/// <summary>
	/// Synchronously waits for a slot in the semaphore to be available.
	/// This method may block the calling thread.
	/// </summary>
	/// <param name="cancellationToken">
	/// The cancellation token used to cancel the wait.
	/// If this is already set, then this method will attempt to take the slot immediately (succeeding if a slot is currently available).
	/// </param>
	public void Wait(CancellationToken cancellationToken)
	{
		WaitAsync(cancellationToken).WaitAndUnwrapException(CancellationToken.None);
	}

	/// <summary>
	/// Synchronously waits for a slot in the semaphore to be available.
	/// This method may block the calling thread.
	/// </summary>
	public void Wait()
	{
		Wait(CancellationToken.None);
	}

	/// <summary>
	/// Releases the semaphore.
	/// </summary>
	public void Release(long releaseCount)
	{
		if (releaseCount == 0)
			return;

		lock (mMutex)
		{
			checked
			{
				long unused = mCount + releaseCount;
			}

			while (releaseCount != 0 && !mQueue.IsEmpty)
			{
				mQueue.Dequeue();
				--releaseCount;
			}

			mCount += releaseCount;
		}
	}

	/// <summary>
	/// Releases the semaphore.
	/// </summary>
	public void Release()
	{
		Release(1);
	}

	private async Task<IDisposable> DoLockAsync(CancellationToken cancellationToken)
	{
		await WaitAsync(cancellationToken).ConfigureAwait(false);
		return AnonymousDisposable.Create(Release);
	}

	/// <summary>
	/// Asynchronously waits on the semaphore, and returns a disposable that releases the semaphore when disposed, thus treating this semaphore as a
	/// "multi-lock".
	/// </summary>
	/// <param name="cancellationToken">
	/// The cancellation token used to cancel the wait.
	/// If this is already set, then this method will attempt to take the slot immediately (succeeding if a slot is currently available).
	/// </param>
	public AwaitableDisposable<IDisposable> LockAsync(CancellationToken cancellationToken)
	{
		return new AwaitableDisposable<IDisposable>(DoLockAsync(cancellationToken));
	}

	/// <summary>
	/// Asynchronously waits on the semaphore, and returns a disposable that releases the semaphore when disposed, thus treating this semaphore as a
	/// "multi-lock".
	/// </summary>
	public AwaitableDisposable<IDisposable> LockAsync()
	{
		return LockAsync(CancellationToken.None);
	}

	/// <summary>
	/// Synchronously waits on the semaphore, and returns a disposable that releases the semaphore when disposed, thus treating this semaphore as a
	/// "multi-lock".
	/// </summary>
	/// <param name="cancellationToken">
	/// The cancellation token used to cancel the wait.
	/// If this is already set, then this method will attempt to take the slot immediately (succeeding if a slot is currently available).
	/// </param>
	public IDisposable Lock(CancellationToken cancellationToken)
	{
		Wait(cancellationToken);
		return AnonymousDisposable.Create(Release);
	}

	/// <summary>
	/// Synchronously waits on the semaphore, and returns a disposable that releases the semaphore when disposed, thus treating this semaphore as a
	/// "multi-lock".
	/// </summary>
	public IDisposable Lock()
	{
		return Lock(CancellationToken.None);
	}

	[DebuggerNonUserCode]
	private sealed class DebugView(AsyncSemaphore semaphore)
	{
		// ReSharper disable UnusedMember.Local
		public int  Id           => semaphore.Id;
		public long CurrentCount => semaphore.mCount;

		public IAsyncWaitQueue<object> WaitQueue => semaphore.mQueue;
		// ReSharper restore UnusedMember.Local
	}
}
