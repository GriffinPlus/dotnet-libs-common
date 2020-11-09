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
	/// Provides extension methods for wait queues.
	/// </summary>
	internal static class AsyncWaitQueueExtensions
	{
		/// <summary>
		/// Creates a new entry and queues it to this wait queue.
		/// If the cancellation token is already canceled, this method immediately returns a canceled task without modifying the wait queue.
		/// </summary>
		/// <param name="this">The wait queue.</param>
		/// <param name="mutex">A synchronization object taken while cancelling the entry.</param>
		/// <param name="token">The token used to cancel the wait.</param>
		/// <returns>The queued task.</returns>
		public static Task<T> Enqueue<T>(this IAsyncWaitQueue<T> @this, object mutex, CancellationToken token)
		{
			if (token.IsCancellationRequested)
				return Task.FromCanceled<T>(token);

			var task = @this.Enqueue();
			if (!token.CanBeCanceled)
				return task;

			var registration = token.Register(() =>
			{
				lock (mutex) @this.TryCancel(task, token);
			}, useSynchronizationContext: false);

			task.ContinueWith(_ => registration.Dispose(),
				CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.Default);

			return task;
		}
	}
}
