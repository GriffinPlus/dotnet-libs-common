///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
//
// This file incorporates work covered by the following copyright and permission notice:
//
//     MIT License
//
//     Copyright (c) 2014-2018 Stephen Cleary
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
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading
{
	/// <summary>
	/// Provides extension methods for the <see cref="Task"/> and <see cref="Task{T}"/> types.
	/// </summary>
	public static class TaskExtensions
	{
		#region Waiting for Task (Synchronous)

		/// <summary>
		/// Waits for the task to complete, unwrapping any exceptions.
		/// </summary>
		/// <param name="task">The task. May not be <c>null</c>.</param>
		public static void WaitAndUnwrapException(this Task task)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));
			task.GetAwaiter().GetResult();
		}

		/// <summary>
		/// Waits for the task to complete, unwrapping any exceptions.
		/// </summary>
		/// <param name="task">The task. May not be <c>null</c>.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
		/// <exception cref="OperationCanceledException">
		/// The <paramref name="cancellationToken"/> was cancelled before the <paramref name="task"/> completed,
		/// or the <paramref name="task"/> raised an <see cref="OperationCanceledException"/>.
		/// </exception>
		public static void WaitAndUnwrapException(this Task task, CancellationToken cancellationToken)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));

			try
			{
				task.Wait(cancellationToken);
			}
			catch (AggregateException ex)
			{
				throw ExceptionHelpers.PrepareForRethrow(ex.InnerException);
			}
		}

		/// <summary>
		/// Waits for the task to complete, unwrapping any exceptions.
		/// </summary>
		/// <typeparam name="TResult">The type of the result of the task.</typeparam>
		/// <param name="task">The task. May not be <c>null</c>.</param>
		/// <returns>The result of the task.</returns>
		public static TResult WaitAndUnwrapException<TResult>(this Task<TResult> task)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));
			return task.GetAwaiter().GetResult();
		}

		/// <summary>
		/// Waits for the task to complete, unwrapping any exceptions.
		/// </summary>
		/// <typeparam name="TResult">The type of the result of the task.</typeparam>
		/// <param name="task">The task. May not be <c>null</c>.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
		/// <returns>The result of the task.</returns>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was cancelled before the <paramref name="task"/> completed, or the <paramref name="task"/> raised an <see cref="OperationCanceledException"/>.</exception>
		public static TResult WaitAndUnwrapException<TResult>(this Task<TResult> task, CancellationToken cancellationToken)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));

			try
			{
				task.Wait(cancellationToken);
				return task.Result;
			}
			catch (AggregateException ex)
			{
				throw ExceptionHelpers.PrepareForRethrow(ex.InnerException);
			}
		}

		/// <summary>
		/// Waits for the task to complete, but does not raise task exceptions. The task exception (if any) is unobserved.
		/// </summary>
		/// <param name="task">The task. May not be <c>null</c>.</param>
		public static void WaitWithoutException(this Task task)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));

			try
			{
				task.Wait();
			}
			catch (AggregateException)
			{
			}
		}

		/// <summary>
		/// Waits for the task to complete, but does not raise task exceptions. The task exception (if any) is unobserved.
		/// </summary>
		/// <param name="task">The task. May not be <c>null</c>.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was cancelled before the <paramref name="task"/> completed.</exception>
		public static void WaitWithoutException(this Task task, CancellationToken cancellationToken)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));

			try
			{
				task.Wait(cancellationToken);
			}
			catch (AggregateException)
			{
				cancellationToken.ThrowIfCancellationRequested();
			}
		}

		#endregion

		#region Waiting for Task (Asynchronous)

		/// <summary>
		/// Asynchronously waits for the task to complete, or for the cancellation token to be canceled.
		/// </summary>
		/// <param name="this">The task to wait for. May not be <c>null</c>.</param>
		/// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
		public static Task WaitAsync(this Task @this, CancellationToken cancellationToken)
		{
			if (@this == null) throw new ArgumentNullException(nameof(@this));
			if (!cancellationToken.CanBeCanceled) return @this;
			if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
			return DoWaitAsync(@this, cancellationToken);
		}

		private static async Task DoWaitAsync(Task task, CancellationToken cancellationToken)
		{
			using (var cancelTaskSource = new CancellationTokenTaskSource<object>(cancellationToken))
			{
				await await Task.WhenAny(task, cancelTaskSource.Task).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Asynchronously waits for the task to complete, or for the cancellation token to be canceled.
		/// </summary>
		/// <typeparam name="TResult">The type of the task result.</typeparam>
		/// <param name="this">The task to wait for. May not be <c>null</c>.</param>
		/// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
		public static Task<TResult> WaitAsync<TResult>(this Task<TResult> @this, CancellationToken cancellationToken)
		{
			if (@this == null) throw new ArgumentNullException(nameof(@this));
			if (!cancellationToken.CanBeCanceled) return @this;
			if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<TResult>(cancellationToken);
			return DoWaitAsync(@this, cancellationToken);
		}

		private static async Task<TResult> DoWaitAsync<TResult>(Task<TResult> task, CancellationToken cancellationToken)
		{
			using (var cancelTaskSource = new CancellationTokenTaskSource<TResult>(cancellationToken))
			{
				return await await Task.WhenAny(task, cancelTaskSource.Task).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Asynchronously waits for any of the source tasks to complete, or for the cancellation token to be canceled.
		/// </summary>
		/// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
		/// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
		public static Task<Task> WhenAny(this IEnumerable<Task> @this, CancellationToken cancellationToken)
		{
			if (@this == null) throw new ArgumentNullException(nameof(@this));
			return Task.WhenAny(@this).WaitAsync(cancellationToken);
		}

		/// <summary>
		/// Asynchronously waits for any of the source tasks to complete.
		/// </summary>
		/// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
		public static Task<Task> WhenAny(this IEnumerable<Task> @this)
		{
			if (@this == null) throw new ArgumentNullException(nameof(@this));
			return Task.WhenAny(@this);
		}

		/// <summary>
		/// Asynchronously waits for any of the source tasks to complete, or for the cancellation token to be canceled.
		/// </summary>
		/// <typeparam name="TResult">The type of the task results.</typeparam>
		/// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
		/// <param name="cancellationToken">The cancellation token that cancels the wait.</param>
		public static Task<Task<TResult>> WhenAny<TResult>(this IEnumerable<Task<TResult>> @this, CancellationToken cancellationToken)
		{
			if (@this == null) throw new ArgumentNullException(nameof(@this));
			return Task.WhenAny(@this).WaitAsync(cancellationToken);
		}

		/// <summary>
		/// Asynchronously waits for any of the source tasks to complete.
		/// </summary>
		/// <typeparam name="TResult">The type of the task results.</typeparam>
		/// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
		public static Task<Task<TResult>> WhenAny<TResult>(this IEnumerable<Task<TResult>> @this)
		{
			if (@this == null) throw new ArgumentNullException(nameof(@this));
			return Task.WhenAny(@this);
		}

		/// <summary>
		/// Asynchronously waits for all of the source tasks to complete.
		/// </summary>
		/// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
		public static Task WhenAll(this IEnumerable<Task> @this)
		{
			if (@this == null) throw new ArgumentNullException(nameof(@this));
			return Task.WhenAll(@this);
		}

		/// <summary>
		/// Asynchronously waits for all of the source tasks to complete.
		/// </summary>
		/// <typeparam name="TResult">The type of the task results.</typeparam>
		/// <param name="this">The tasks to wait for. May not be <c>null</c>.</param>
		public static Task<TResult[]> WhenAll<TResult>(this IEnumerable<Task<TResult>> @this)
		{
			if (@this == null) throw new ArgumentNullException(nameof(@this));
			return Task.WhenAll(@this);
		}

		#endregion

		#region Ignoring Task Completion

		/// <summary>
		/// DANGEROUS! Ignores the completion of this task. Also ignores exceptions.
		/// </summary>
		/// <param name="this">The task to ignore.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static async void Ignore(this Task @this)
		{
			try
			{
				await @this.ConfigureAwait(false);
			}
			catch
			{
				// ignored
			}
		}

		/// <summary>
		/// DANGEROUS! Ignores the completion and results of this task. Also ignores exceptions.
		/// </summary>
		/// <param name="this">The task to ignore.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static async void Ignore<T>(this Task<T> @this)
		{
			try
			{
				await @this.ConfigureAwait(false);
			}
			catch
			{
				// ignored
			}
		}

		#endregion

		#region Order by Completion

		/// <summary>
		/// Creates a new collection of tasks that complete in order.
		/// </summary>
		/// <typeparam name="T">The type of the results of the tasks.</typeparam>
		/// <param name="this">The tasks to order by completion. May not be <c>null</c>.</param>
		public static List<Task<T>> OrderByCompletion<T>(this IEnumerable<Task<T>> @this)
		{
			if (@this == null) throw new ArgumentNullException(nameof(@this));

			// This is a combination of Jon Skeet's approach and Stephen Toub's approach:
			//  http://msmvps.com/blogs/jon_skeet/archive/2012/01/16/eduasync-part-19-ordering-by-completion-ahead-of-time.aspx
			//  http://blogs.msdn.com/b/pfxteam/archive/2012/08/02/processing-tasks-as-they-complete.aspx

			// Reify the source task sequence. TODO: better reification.
			var taskArray = @this.ToArray();

			// Allocate a TCS array and an array of the resulting tasks.
			var numTasks = taskArray.Length;
			var tcs = new TaskCompletionSource<T>[numTasks];
			var ret = new List<Task<T>>(numTasks);

			// As each task completes, complete the next tcs.
			var lastIndex = -1;
			Action<Task<T>> continuation = task =>
			{
				var index = Interlocked.Increment(ref lastIndex);
				tcs[index].TryCompleteFromCompletedTask(task);
			};

			// Fill out the arrays and attach the continuations.
			for (var i = 0; i != numTasks; ++i)
			{
				tcs[i] = new TaskCompletionSource<T>();
				ret.Add(tcs[i].Task);

				taskArray[i].ContinueWith(
					continuation,
					CancellationToken.None,
					TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.DenyChildAttach,
					TaskScheduler.Default);
			}

			return ret;
		}

		/// <summary>
		/// Creates a new collection of tasks that complete in order.
		/// </summary>
		/// <param name="this">The tasks to order by completion. May not be <c>null</c>.</param>
		public static List<Task> OrderByCompletion(this IEnumerable<Task> @this)
		{
			if (@this == null) throw new ArgumentNullException(nameof(@this));

			// This is a combination of Jon Skeet's approach and Stephen Toub's approach:
			//  http://msmvps.com/blogs/jon_skeet/archive/2012/01/16/eduasync-part-19-ordering-by-completion-ahead-of-time.aspx
			//  http://blogs.msdn.com/b/pfxteam/archive/2012/08/02/processing-tasks-as-they-complete.aspx

			// Reify the source task sequence. TODO: better reification.
			var taskArray = @this.ToArray();

			// Allocate a TCS array and an array of the resulting tasks.
			var numTasks = taskArray.Length;
			var tcs = new TaskCompletionSource<object>[numTasks];
			var ret = new List<Task>(numTasks);

			// As each task completes, complete the next tcs.
			var lastIndex = -1;
			// ReSharper disable once ConvertToLocalFunction
			Action<Task> continuation = task =>
			{
				var index = Interlocked.Increment(ref lastIndex);
				tcs[index].TryCompleteFromCompletedTask(task, NullResultFunc);
			};

			// Fill out the arrays and attach the continuations.
			for (var i = 0; i != numTasks; ++i)
			{
				tcs[i] = new TaskCompletionSource<object>();
				ret.Add(tcs[i].Task);
				taskArray[i].ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.DenyChildAttach, TaskScheduler.Default);
			}

			return ret;
		}

		private static Func<object> NullResultFunc { get; } = () => null;

		#endregion
	}
}
