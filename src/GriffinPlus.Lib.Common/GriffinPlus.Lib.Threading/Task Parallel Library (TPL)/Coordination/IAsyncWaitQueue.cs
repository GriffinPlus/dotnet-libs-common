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

using System.Threading;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading
{
	/// <summary>
	/// A collection of cancelable <see cref="TaskCompletionSource{T}"/> instances.
	/// Implementations must assume the caller is holding a lock.
	/// </summary>
	/// <typeparam name="T">The type of the results. If this isn't needed, use <see cref="System.Object"/>.</typeparam>
	internal interface IAsyncWaitQueue<T>
	{
		/// <summary>
		/// Gets whether the queue is empty.
		/// </summary>
		bool IsEmpty { get; }

		/// <summary>
		/// Creates a new entry and queues it to this wait queue.
		/// The returned task must support both synchronous and asynchronous waits.
		/// </summary>
		/// <returns>The queued task.</returns>
		Task<T> Enqueue();

		/// <summary>
		/// Removes a single entry in the wait queue and completes it.
		/// This method may only be called if <see cref="IsEmpty"/> is <c>false</c>.
		/// The task continuations for the completed task must be executed asynchronously.
		/// </summary>
		/// <param name="result">The result used to complete the wait queue entry. If this isn't needed, use <c>default(T)</c>.</param>
		void Dequeue(T result = default(T));

		/// <summary>
		/// Removes all entries in the wait queue and completes them.
		/// The task continuations for the completed tasks must be executed asynchronously.
		/// </summary>
		/// <param name="result">The result used to complete the wait queue entries. If this isn't needed, use <c>default(T)</c>.</param>
		void DequeueAll(T result = default(T));

		/// <summary>
		/// Attempts to remove an entry from the wait queue and cancels it.
		/// The task continuations for the completed task must be executed asynchronously.
		/// </summary>
		/// <param name="task">The task to cancel.</param>
		/// <param name="cancellationToken">The cancellation token to use to cancel the task.</param>
		bool TryCancel(Task task, CancellationToken cancellationToken);

		/// <summary>
		/// Removes all entries from the wait queue and cancels them.
		/// The task continuations for the completed tasks must be executed asynchronously.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token to use to cancel the tasks.</param>
		void CancelAll(CancellationToken cancellationToken);
	}
}
