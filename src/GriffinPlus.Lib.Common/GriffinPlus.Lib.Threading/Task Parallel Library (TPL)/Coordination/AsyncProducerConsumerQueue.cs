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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading
{

	/// <summary>
	/// An async-compatible producer/consumer queue.
	/// </summary>
	/// <typeparam name="T">The type of elements contained in the queue.</typeparam>
	[DebuggerDisplay("Count = {mQueue.Count}, MaxCount = {mMaxCount}")]
	[DebuggerTypeProxy(typeof(AsyncProducerConsumerQueue<>.DebugView))]
	public sealed class AsyncProducerConsumerQueue<T>
	{
		/// <summary>
		/// The underlying queue.
		/// </summary>
		private readonly Queue<T> mQueue;

		/// <summary>
		/// The maximum number of elements allowed in the queue.
		/// </summary>
		private readonly int mMaxCount;

		/// <summary>
		/// The mutual-exclusion lock protecting <see cref="mQueue"/> and <see cref="mCompleted"/>.
		/// </summary>
		private readonly AsyncLock mMutex;

		/// <summary>
		/// A condition variable that is signaled when the queue is not full.
		/// </summary>
		private readonly AsyncConditionVariable mCompletedOrNotFull;

		/// <summary>
		/// A condition variable that is signaled when the queue is completed or not empty.
		/// </summary>
		private readonly AsyncConditionVariable mCompletedOrNotEmpty;

		/// <summary>
		/// Whether this producer/consumer queue has been marked complete for adding.
		/// </summary>
		private bool mCompleted;

		/// <summary>
		/// Creates a new async-compatible producer/consumer queue with the specified initial elements and a maximum element count.
		/// </summary>
		/// <param name="collection">
		/// The initial elements to place in the queue.
		/// This may be <c>null</c> to start with an empty collection.
		/// </param>
		/// <param name="maxCount">
		/// The maximum element count.
		/// This must be greater than zero, and greater than or equal to the number of elements in <paramref name="collection"/>.
		/// </param>
		public AsyncProducerConsumerQueue(IEnumerable<T> collection, int maxCount)
		{
			if (maxCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(maxCount), "The maximum count must be greater than zero.");

			mQueue = collection == null ? new Queue<T>() : new Queue<T>(collection);
			if (maxCount < mQueue.Count)
				throw new ArgumentException("The maximum count cannot be less than the number of elements in the collection.", nameof(maxCount));
			mMaxCount = maxCount;

			mMutex = new AsyncLock();
			mCompletedOrNotFull = new AsyncConditionVariable(mMutex);
			mCompletedOrNotEmpty = new AsyncConditionVariable(mMutex);
		}

		/// <summary>
		/// Creates a new async-compatible producer/consumer queue with the specified initial elements.
		/// </summary>
		/// <param name="collection">
		/// The initial elements to place in the queue.
		/// This may be <c>null</c> to start with an empty collection.
		/// </param>
		public AsyncProducerConsumerQueue(IEnumerable<T> collection)
			: this(collection, int.MaxValue) { }

		/// <summary>
		/// Creates a new async-compatible producer/consumer queue with a maximum element count.
		/// </summary>
		/// <param name="maxCount">
		/// The maximum element count.
		/// This must be greater than zero.
		/// </param>
		public AsyncProducerConsumerQueue(int maxCount)
			: this(null, maxCount) { }

		/// <summary>
		/// Creates a new async-compatible producer/consumer queue.
		/// </summary>
		public AsyncProducerConsumerQueue()
			: this(null, int.MaxValue) { }

		/// <summary>
		/// Get a value indicating whether the queue is empty.
		/// This property assumes that <see cref="mMutex"/> is already held.
		/// </summary>
		private bool Empty => mQueue.Count == 0;

		/// <summary>
		/// Whether the queue is full.
		/// This property assumes that the <see cref="mMutex"/> is already held.
		/// </summary>
		private bool Full => mQueue.Count == mMaxCount;

		/// <summary>
		/// Marks the producer/consumer queue as complete for adding.
		/// </summary>
		public void CompleteAdding()
		{
			using (mMutex.Lock())
			{
				mCompleted = true;
				mCompletedOrNotEmpty.NotifyAll();
				mCompletedOrNotFull.NotifyAll();
			}
		}

		/// <summary>
		/// Enqueues an item to the producer/consumer queue.
		/// Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding.
		/// </summary>
		/// <param name="item">The item to enqueue.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to abort the enqueue operation.</param>
		/// <param name="sync">
		/// <c>true</c> to run the method synchronously;
		/// <c>false</c> to run the method asynchronously.
		/// </param>
		private async Task DoEnqueueAsync(T item, CancellationToken cancellationToken, bool sync)
		{
			using (sync ? mMutex.Lock() : await mMutex.LockAsync().ConfigureAwait(false))
			{
				// Wait for the queue to be not full.
				while (Full && !mCompleted)
				{
					if (sync)
						// ReSharper disable once MethodHasAsyncOverload
						mCompletedOrNotFull.Wait(cancellationToken);
					else
						await mCompletedOrNotFull.WaitAsync(cancellationToken).ConfigureAwait(false);
				}

				// If the queue has been marked complete, then abort.
				if (mCompleted)
					throw new InvalidOperationException("Enqueue failed; the producer/consumer queue has completed adding.");

				mQueue.Enqueue(item);
				mCompletedOrNotEmpty.Notify();
			}
		}

		/// <summary>
		/// Enqueues an item to the producer/consumer queue.
		/// Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding.
		/// </summary>
		/// <param name="item">The item to enqueue.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to abort the enqueue operation.</param>
		/// <exception cref="InvalidOperationException">The producer/consumer queue has been marked complete for adding.</exception>
		public Task EnqueueAsync(T item, CancellationToken cancellationToken)
		{
			return DoEnqueueAsync(item, cancellationToken, false);
		}

		/// <summary>
		/// Enqueues an item to the producer/consumer queue.
		/// Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding.
		/// </summary>
		/// <param name="item">The item to enqueue.</param>
		/// <exception cref="InvalidOperationException">The producer/consumer queue has been marked complete for adding.</exception>
		public Task EnqueueAsync(T item)
		{
			return EnqueueAsync(item, CancellationToken.None);
		}

		/// <summary>
		/// Enqueues an item to the producer/consumer queue.
		/// This method may block the calling thread.
		/// Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding.
		/// </summary>
		/// <param name="item">The item to enqueue.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to abort the enqueue operation.</param>
		/// <exception cref="InvalidOperationException">The producer/consumer queue has been marked complete for adding.</exception>
		public void Enqueue(T item, CancellationToken cancellationToken)
		{
			DoEnqueueAsync(item, cancellationToken, true).WaitAndUnwrapException(CancellationToken.None);
		}

		/// <summary>
		/// Enqueues an item to the producer/consumer queue. This method may block the calling thread.
		/// Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding.
		/// </summary>
		/// <param name="item">The item to enqueue.</param>
		/// <exception cref="InvalidOperationException">The producer/consumer queue has been marked complete for adding.</exception>
		public void Enqueue(T item)
		{
			Enqueue(item, CancellationToken.None);
		}

		/// <summary>
		/// Waits until an item is available to dequeue.
		/// Returns <c>false</c> if the producer/consumer queue has completed adding and there are no more items.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to abort the asynchronous wait.</param>
		/// <param name="sync">
		/// <c>true</c> to run the method synchronously;
		/// <c>false</c> to run the method asynchronously.
		/// </param>
		private async Task<bool> DoOutputAvailableAsync(CancellationToken cancellationToken, bool sync)
		{
			using (sync ? mMutex.Lock() : await mMutex.LockAsync().ConfigureAwait(false))
			{
				while (Empty && !mCompleted)
				{
					if (sync)
						// ReSharper disable once MethodHasAsyncOverload
						mCompletedOrNotEmpty.Wait(cancellationToken);
					else
						await mCompletedOrNotEmpty.WaitAsync(cancellationToken).ConfigureAwait(false);
				}

				return !Empty;
			}
		}

		/// <summary>
		/// Asynchronously waits until an item is available to dequeue.
		/// Returns <c>false</c> if the producer/consumer queue has completed adding and there are no more items.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to abort the asynchronous wait.</param>
		public Task<bool> OutputAvailableAsync(CancellationToken cancellationToken)
		{
			return DoOutputAvailableAsync(cancellationToken, false);
		}

		/// <summary>
		/// Asynchronously waits until an item is available to dequeue.
		/// Returns <c>false</c> if the producer/consumer queue has completed adding and there are no more items.
		/// </summary>
		public Task<bool> OutputAvailableAsync()
		{
			return OutputAvailableAsync(CancellationToken.None);
		}

		/// <summary>
		/// Synchronously waits until an item is available to dequeue.
		/// Returns <c>false</c> if the producer/consumer queue has completed adding and there are no more items.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to abort the asynchronous wait.</param>
		public bool OutputAvailable(CancellationToken cancellationToken)
		{
			return DoOutputAvailableAsync(cancellationToken, true).WaitAndUnwrapException();
		}

		/// <summary>
		/// Synchronously waits until an item is available to dequeue.
		/// Returns <c>false</c> if the producer/consumer queue has completed adding and there are no more items.
		/// </summary>
		public bool OutputAvailable()
		{
			return OutputAvailable(CancellationToken.None);
		}

		/// <summary>
		/// Provides a (synchronous) consuming enumerable for items in the producer/consumer queue.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to abort the synchronous enumeration.</param>
		public IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken)
		{
			while (true)
			{
				Tuple<bool, T> result = TryDoDequeueAsync(cancellationToken, true).WaitAndUnwrapException();
				if (!result.Item1)
					yield break;

				yield return result.Item2;
			}
		}

		/// <summary>
		/// Provides a (synchronous) consuming enumerable for items in the producer/consumer queue.
		/// </summary>
		public IEnumerable<T> GetConsumingEnumerable()
		{
			return GetConsumingEnumerable(CancellationToken.None);
		}

		/// <summary>
		/// Attempts to dequeue an item from the producer/consumer queue.
		/// Returns <c>false</c> if the producer/consumer queue has completed adding and is empty.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to abort the dequeue operation.</param>
		/// <param name="sync">
		/// <c>true</c> to run the method synchronously;
		/// <c>false</c> to run the method asynchronously.
		/// </param>
		private async Task<Tuple<bool, T>> TryDoDequeueAsync(CancellationToken cancellationToken, bool sync)
		{
			using (sync ? mMutex.Lock() : await mMutex.LockAsync().ConfigureAwait(false))
			{
				while (Empty && !mCompleted)
				{
					if (sync)
						// ReSharper disable once MethodHasAsyncOverload
						mCompletedOrNotEmpty.Wait(cancellationToken);
					else
						await mCompletedOrNotEmpty.WaitAsync(cancellationToken).ConfigureAwait(false);
				}

				if (mCompleted && Empty)
					return Tuple.Create(false, default(T));

				T item = mQueue.Dequeue();
				mCompletedOrNotFull.Notify();
				return Tuple.Create(true, item);
			}
		}

		/// <summary>
		/// Dequeues an item from the producer/consumer queue.
		/// Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding and is empty.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to abort the dequeue operation.</param>
		/// <param name="sync">
		/// <c>true</c> to run the method synchronously;
		/// <c>false</c> to run the method asynchronously.
		/// </param>
		/// <exception cref="InvalidOperationException">The producer/consumer queue has been marked complete for adding and is empty.</exception>
		private async Task<T> DoDequeueAsync(CancellationToken cancellationToken, bool sync)
		{
			Tuple<bool, T> result = await TryDoDequeueAsync(cancellationToken, sync).ConfigureAwait(false);
			if (result.Item1)
				return result.Item2;
			throw new InvalidOperationException("Dequeue failed; the producer/consumer queue has completed adding and is empty.");
		}

		/// <summary>
		/// Dequeues an item from the producer/consumer queue.
		/// Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding and is empty.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to abort the dequeue operation.</param>
		/// <returns>The dequeued item.</returns>
		/// <exception cref="InvalidOperationException">The producer/consumer queue has been marked complete for adding and is empty.</exception>
		public Task<T> DequeueAsync(CancellationToken cancellationToken)
		{
			return DoDequeueAsync(cancellationToken, false);
		}

		/// <summary>
		/// Dequeues an item from the producer/consumer queue. Returns the dequeued item.
		/// Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding and is empty.
		/// </summary>
		/// <returns>The dequeued item.</returns>
		/// <exception cref="InvalidOperationException">The producer/consumer queue has been marked complete for adding and is empty.</exception>
		public Task<T> DequeueAsync()
		{
			return DequeueAsync(CancellationToken.None);
		}

		/// <summary>
		/// Dequeues an item from the producer/consumer queue.
		/// Returns the dequeued item.
		/// This method may block the calling thread.
		/// Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding and is empty.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to abort the dequeue operation.</param>
		/// <returns>The dequeued item.</returns>
		/// <exception cref="InvalidOperationException">The producer/consumer queue has been marked complete for adding and is empty.</exception>
		public T Dequeue(CancellationToken cancellationToken)
		{
			return DoDequeueAsync(cancellationToken, true).WaitAndUnwrapException();
		}

		/// <summary>
		/// Dequeues an item from the producer/consumer queue.
		/// Returns the dequeued item. This method may block the calling thread.
		/// Throws <see cref="InvalidOperationException"/> if the producer/consumer queue has completed adding and is empty.
		/// </summary>
		/// <returns>The dequeued item.</returns>
		/// <exception cref="InvalidOperationException">The producer/consumer queue has been marked complete for adding and is empty.</exception>
		public T Dequeue()
		{
			return Dequeue(CancellationToken.None);
		}

		[DebuggerNonUserCode]
		internal sealed class DebugView
		{
			private readonly AsyncProducerConsumerQueue<T> mQueue;

			public DebugView(AsyncProducerConsumerQueue<T> queue)
			{
				mQueue = queue;
			}

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public T[] Items => mQueue.mQueue.ToArray();
		}
	}

}
