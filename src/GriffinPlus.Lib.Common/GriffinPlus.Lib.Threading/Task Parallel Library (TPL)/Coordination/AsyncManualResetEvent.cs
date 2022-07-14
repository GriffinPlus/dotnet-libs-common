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
	/// An async-compatible manual-reset event.
	/// </summary>
	[DebuggerDisplay("Id = {Id}, IsSet = {GetStateForDebugger}")]
	[DebuggerTypeProxy(typeof(DebugView))]
	public sealed class AsyncManualResetEvent
	{
		/// <summary>
		/// The object used for synchronization.
		/// </summary>
		private readonly object mMutex = new object();

		/// <summary>
		/// The current state of the event.
		/// </summary>
		private TaskCompletionSource<object> mTcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();

		/// <summary>
		/// The semi-unique identifier for this instance.
		/// This is 0 if the id has not yet been created.
		/// </summary>
		private int mId;

		[DebuggerNonUserCode]
		// ReSharper disable once InconsistentlySynchronizedField
		private bool GetStateForDebugger => mTcs.Task.IsCompleted; // no need for synchronization as mTcs is always initialized and replaced atomically

		/// <summary>
		/// Creates an async-compatible manual-reset event.
		/// </summary>
		/// <param name="set">
		/// <c>true</c> to create a manual-reset event that is initially set;
		/// <c>false</c> to create a manual-reset event that is initially unset.
		/// </param>
		public AsyncManualResetEvent(bool set)
		{
			if (set) mTcs.TrySetResult(null);
		}

		/// <summary>
		/// Creates an async-compatible manual-reset event that is initially unset.
		/// </summary>
		public AsyncManualResetEvent()
			: this(false) { }

		/// <summary>
		/// Gets a semi-unique identifier for this asynchronous manual-reset event.
		/// </summary>
		public int Id => IdManager<AsyncManualResetEvent>.GetId(ref mId);

		/// <summary>
		/// Whether this event is currently set.
		/// This member is seldom used; code using this member has a high possibility of race conditions.
		/// </summary>
		public bool IsSet
		{
			get
			{
				lock (mMutex) return mTcs.Task.IsCompleted;
			}
		}

		/// <summary>
		/// Asynchronously waits for this event to be set.
		/// </summary>
		public Task WaitAsync()
		{
			lock (mMutex)
			{
				return mTcs.Task;
			}
		}

		/// <summary>
		/// Asynchronously waits for this event to be set.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation token used to cancel the wait.
		/// If this token is already canceled, this method will first check whether the event is set.
		/// </param>
		public Task WaitAsync(CancellationToken cancellationToken)
		{
			Task waitTask;
			lock (mMutex) waitTask = mTcs.Task;
			if (waitTask.IsCompleted) return waitTask;
			return waitTask.WaitAsync(cancellationToken);
		}

		/// <summary>
		/// Synchronously waits for this event to be set.
		/// This method may block the calling thread.
		/// </summary>
		public void Wait()
		{
			WaitAsync().WaitAndUnwrapException();
		}

		/// <summary>
		/// Synchronously waits for this event to be set.
		/// This method may block the calling thread.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation token used to cancel the wait.
		/// If this token is already canceled, this method will first check whether the event is set.
		/// </param>
		public void Wait(CancellationToken cancellationToken)
		{
			Task waitTask;
			lock (mMutex) waitTask = mTcs.Task;
			if (waitTask.IsCompleted) return;
			waitTask.WaitAndUnwrapException(cancellationToken);
		}

		/// <summary>
		/// Sets the event, atomically completing every task returned by <see cref="WaitAsync()"/>.
		/// If the event is already set, this method does nothing.
		/// </summary>
		public void Set()
		{
			lock (mMutex)
			{
				mTcs.TrySetResult(null);
			}
		}

		/// <summary>
		/// Resets the event.
		/// If the event is already reset, this method does nothing.
		/// </summary>
		public void Reset()
		{
			lock (mMutex)
			{
				if (mTcs.Task.IsCompleted)
					mTcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
			}
		}

		// ReSharper disable UnusedMember.Local
		[DebuggerNonUserCode]
		private sealed class DebugView
		{
			private readonly AsyncManualResetEvent mManualResetEvent;

			public DebugView(AsyncManualResetEvent manualResetEvent)
			{
				mManualResetEvent = manualResetEvent;
			}

			public int  Id          => mManualResetEvent.Id;
			public bool IsSet       => mManualResetEvent.GetStateForDebugger;
			public Task CurrentTask => mManualResetEvent.mTcs.Task;
		}
		// ReSharper restore UnusedMember.Local
	}

}
