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

namespace GriffinPlus.Lib.Threading
{

	/// <summary>
	/// An async-compatible monitor.
	/// Note that the monitor is <b>not</b> recursive!
	/// </summary>
	[DebuggerDisplay("Id = {Id}, ConditionVariableId = {mConditionVariable.Id}")]
	public sealed class AsyncMonitor
	{
		/// <summary>
		/// The lock.
		/// </summary>
		private readonly AsyncLock mAsyncLock;

		/// <summary>
		/// The condition variable.
		/// </summary>
		private readonly AsyncConditionVariable mConditionVariable;

		/// <summary>
		/// Creates a new monitor.
		/// </summary>
		/// <param name="lockQueue">
		/// The wait queue used to manage waiters for the lock.
		/// This may be <c>null</c> to use a default (FIFO) queue.
		/// </param>
		/// <param name="conditionVariableQueue">
		/// The wait queue used to manage waiters for the signal.
		/// This may be <c>null</c> to use a default (FIFO) queue.
		/// </param>
		internal AsyncMonitor(IAsyncWaitQueue<IDisposable> lockQueue, IAsyncWaitQueue<object> conditionVariableQueue)
		{
			mAsyncLock = new AsyncLock(lockQueue);
			mConditionVariable = new AsyncConditionVariable(mAsyncLock, conditionVariableQueue);
		}

		/// <summary>
		/// Creates a new monitor.
		/// </summary>
		public AsyncMonitor()
			: this(null, null) { }

		/// <summary>
		/// Gets a semi-unique identifier for this monitor.
		/// </summary>
		public int Id => mAsyncLock.Id;

		/// <summary>
		/// Asynchronously enters the monitor.
		/// Returns a disposable that leaves the monitor when disposed.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation token used to cancel the enter.
		/// If this is already set, then this method will attempt to enter the monitor immediately (succeeding if the monitor is currently available).
		/// </param>
		/// <returns>A disposable that leaves the monitor when disposed.</returns>
		public AwaitableDisposable<IDisposable> EnterAsync(CancellationToken cancellationToken)
		{
			return mAsyncLock.LockAsync(cancellationToken);
		}

		/// <summary>
		/// Asynchronously enters the monitor.
		/// Returns a disposable that leaves the monitor when disposed.
		/// </summary>
		/// <returns>A disposable that leaves the monitor when disposed.</returns>
		public AwaitableDisposable<IDisposable> EnterAsync()
		{
			return EnterAsync(CancellationToken.None);
		}

		/// <summary>
		/// Synchronously enters the monitor.
		/// Returns a disposable that leaves the monitor when disposed.
		/// This method may block the calling thread.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation token used to cancel the enter.
		/// If this is already set, then this method will attempt to enter the monitor immediately (succeeding if the monitor is currently available).
		/// </param>
		public IDisposable Enter(CancellationToken cancellationToken)
		{
			return mAsyncLock.Lock(cancellationToken);
		}

		/// <summary>
		/// Asynchronously enters the monitor.
		/// Returns a disposable that leaves the monitor when disposed.
		/// This method may block the calling thread.
		/// </summary>
		public IDisposable Enter()
		{
			return Enter(CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously waits for a pulse signal on this monitor.
		/// The monitor MUST already be entered when calling this method, and it will still be entered when this method returns, even if the method is cancelled.
		/// This method internally will leave the monitor while waiting for a notification.
		/// </summary>
		/// <param name="cancellationToken">The cancellation signal used to cancel this wait.</param>
		public Task WaitAsync(CancellationToken cancellationToken)
		{
			return mConditionVariable.WaitAsync(cancellationToken);
		}

		/// <summary>
		/// Asynchronously waits for a pulse signal on this monitor.
		/// The monitor MUST already be entered when calling this method, and it will still be entered when this method returns.
		/// This method internally will leave the monitor while waiting for a notification.
		/// </summary>
		public Task WaitAsync()
		{
			return WaitAsync(CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously waits for a pulse signal on this monitor.
		/// This method may block the calling thread.
		/// The monitor MUST already be entered when calling this method, and it will still be entered when this method returns, even if the method is cancelled.
		/// This method internally will leave the monitor while waiting for a notification.
		/// </summary>
		/// <param name="cancellationToken">The cancellation signal used to cancel this wait.</param>
		public void Wait(CancellationToken cancellationToken)
		{
			mConditionVariable.Wait(cancellationToken);
		}

		/// <summary>
		/// Asynchronously waits for a pulse signal on this monitor.
		/// This method may block the calling thread. The monitor MUST already be entered when calling this method, and it will still be entered when this method
		/// returns.
		/// This method internally will leave the monitor while waiting for a notification.
		/// </summary>
		public void Wait()
		{
			Wait(CancellationToken.None);
		}

		/// <summary>
		/// Sends a signal to a single task waiting on this monitor.
		/// The monitor MUST already be entered when calling this method, and it will still be entered when this method returns.
		/// </summary>
		public void Pulse()
		{
			mConditionVariable.Notify();
		}

		/// <summary>
		/// Sends a signal to all tasks waiting on this monitor.
		/// The monitor MUST already be entered when calling this method, and it will still be entered when this method returns.
		/// </summary>
		public void PulseAll()
		{
			mConditionVariable.NotifyAll();
		}
	}

}
