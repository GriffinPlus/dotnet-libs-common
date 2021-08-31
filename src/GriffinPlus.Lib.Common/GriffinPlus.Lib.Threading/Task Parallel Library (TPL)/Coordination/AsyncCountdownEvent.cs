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
	/// An async-compatible countdown event.
	/// </summary>
	[DebuggerDisplay("Id = {Id}, CurrentCount = {mCount}")]
	[DebuggerTypeProxy(typeof(DebugView))]
	public sealed class AsyncCountdownEvent
	{
		/// <summary>
		/// The underlying manual-reset event.
		/// </summary>
		private readonly AsyncManualResetEvent mManualResetEvent;

		/// <summary>
		/// The remaining count on this event.
		/// </summary>
		private long mCount;

		/// <summary>
		/// Creates an async-compatible countdown event.
		/// </summary>
		/// <param name="count">The number of signals this event will need before it becomes set.</param>
		public AsyncCountdownEvent(long count)
		{
			mManualResetEvent = new AsyncManualResetEvent(count == 0);
			mCount = count;
		}

		/// <summary>
		/// Gets a semi-unique identifier for this asynchronous countdown event.
		/// </summary>
		public int Id => mManualResetEvent.Id;

		/// <summary>
		/// Gets the current number of remaining signals before this event becomes set.
		/// This member is seldom used; code using this member has a high possibility of race conditions.
		/// </summary>
		public long CurrentCount
		{
			get
			{
				lock (mManualResetEvent) return mCount;
			}
		}

		/// <summary>
		/// Asynchronously waits for the count to reach zero.
		/// </summary>
		public Task WaitAsync()
		{
			return mManualResetEvent.WaitAsync();
		}

		/// <summary>
		/// Asynchronously waits for the count to reach zero.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation token used to cancel the wait.
		/// If this token is already canceled, this method will first check whether the event is set.
		/// </param>
		public Task WaitAsync(CancellationToken cancellationToken)
		{
			return mManualResetEvent.WaitAsync(cancellationToken);
		}

		/// <summary>
		/// Synchronously waits for the count to reach zero.
		/// This method may block the calling thread.
		/// </summary>
		public void Wait()
		{
			mManualResetEvent.Wait();
		}

		/// <summary>
		/// Synchronously waits for the count to reach zero.
		/// This method may block the calling thread.
		/// </summary>
		/// <param name="cancellationToken">
		/// The cancellation token used to cancel the wait.
		/// If this token is already canceled, this method will first check whether the event is set.
		/// </param>
		public void Wait(CancellationToken cancellationToken)
		{
			mManualResetEvent.Wait(cancellationToken);
		}

		/// <summary>
		/// Attempts to modify the current count by the specified amount.
		/// </summary>
		/// <param name="difference">The amount to change the current count.</param>
		/// <param name="add">
		/// <c>true</c> to add to the current count;
		/// <c>false</c> to subtract.
		/// </param>
		private void ModifyCount(long difference, bool add)
		{
			if (difference == 0)
				return;

			lock (mManualResetEvent)
			{
				long oldCount = mCount;

				checked
				{
					if (add) mCount += difference;
					else mCount -= difference;
				}

				if (oldCount == 0)
				{
					mManualResetEvent.Reset();
				}
				else if (mCount == 0)
				{
					mManualResetEvent.Set();
				}
				else if (oldCount < 0 && mCount > 0 || oldCount > 0 && mCount < 0)
				{
					mManualResetEvent.Set();
					mManualResetEvent.Reset();
				}
			}
		}

		/// <summary>
		/// Adds the specified value to the current count.
		/// </summary>
		/// <param name="addCount">The amount to change the current count.</param>
		public void AddCount(long addCount)
		{
			ModifyCount(addCount, true);
		}

		/// <summary>
		/// Adds one to the current count.
		/// </summary>
		public void AddCount()
		{
			AddCount(1);
		}

		/// <summary>
		/// Subtracts the specified value from the current count.
		/// </summary>
		/// <param name="signalCount">The amount to change the current count.</param>
		public void Signal(long signalCount)
		{
			ModifyCount(signalCount, false);
		}

		/// <summary>
		/// Subtracts one from the current count.
		/// </summary>
		public void Signal()
		{
			Signal(1);
		}

		// ReSharper disable UnusedMember.Local
		[DebuggerNonUserCode]
		private sealed class DebugView
		{
			private readonly AsyncCountdownEvent mCountdownEvent;

			public DebugView(AsyncCountdownEvent countdownEvent)
			{
				mCountdownEvent = countdownEvent;
			}

			public int                   Id                    => mCountdownEvent.Id;
			public long                  CurrentCount          => mCountdownEvent.CurrentCount;
			public AsyncManualResetEvent AsyncManualResetEvent => mCountdownEvent.mManualResetEvent;
		}
		// ReSharper restore UnusedMember.Local
	}

}
