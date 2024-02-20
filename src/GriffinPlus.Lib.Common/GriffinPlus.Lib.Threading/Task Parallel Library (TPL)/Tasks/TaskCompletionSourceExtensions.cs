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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading;

/// <summary>
/// Provides extension methods for <see cref="TaskCompletionSource{TResult}"/>.
/// </summary>
public static class TaskCompletionSourceExtensions
{
	private const int MinWorkerThreads = 4;

	/// <summary>
	/// Initializes the <see cref="DefaultAsyncWaitQueue{T}"/> class.
	/// </summary>
	static TaskCompletionSourceExtensions()
	{
		// Increase the number of threads in the .NET thread pool on machines with only few CPUs.
		// The AsyncEx synchronization primitives in this library need a thread pool thread to execute continuations.
		// If there is no thread in the pool the application will hang for some time until the thread pool creates another
		// thread to solve the issue. Increasing the number of threads to a higher value than the number of CPUs comes at
		// the cost of some performance, so the actual issue should be fixed and this workaround should be eliminated to
		// restore the original behavior.
		ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionPortThreads);
		if (minWorkerThreads < MinWorkerThreads) ThreadPool.SetMinThreads(MinWorkerThreads, minCompletionPortThreads);
	}

	/// <summary>
	/// Attempts to complete a <see cref="TaskCompletionSource{TResult}"/>,
	/// propagating the completion of <paramref name="task"/>.
	/// </summary>
	/// <typeparam name="TResult">The type of the result of the target asynchronous operation.</typeparam>
	/// <typeparam name="TSourceResult">The type of the result of the source asynchronous operation.</typeparam>
	/// <param name="this">The task completion source. May not be <c>null</c>.</param>
	/// <param name="task">The task. May not be <c>null</c>.</param>
	/// <returns>
	/// <c>true</c> if this method completed the task completion source;
	/// <c>false</c> if it was already completed.
	/// </returns>
	public static bool TryCompleteFromCompletedTask<TResult, TSourceResult>(
		this TaskCompletionSource<TResult> @this,
		Task<TSourceResult>                task) where TSourceResult : TResult
	{
		if (@this == null) throw new ArgumentNullException(nameof(@this));
		if (task == null) throw new ArgumentNullException(nameof(task));

		if (task.IsFaulted)
		{
			Debug.Assert(task.Exception != null, "task.Exception != null");
			return @this.TrySetException(task.Exception.InnerExceptions);
		}

		// ReSharper disable once InvertIf
		if (task.IsCanceled)
		{
			try
			{
				task.WaitAndUnwrapException();
			}
			catch (OperationCanceledException exception)
			{
				CancellationToken token = exception.CancellationToken;
				return token.IsCancellationRequested ? @this.TrySetCanceled(token) : @this.TrySetCanceled();
			}
		}

		return @this.TrySetResult(task.Result);
	}

	/// <summary>
	/// Attempts to complete a <see cref="TaskCompletionSource{TResult}"/>, propagating the completion of <paramref name="task"/>,
	/// but using the result value from <paramref name="resultFunc"/> if the task completed successfully.
	/// </summary>
	/// <typeparam name="TResult">The type of the result of the target asynchronous operation.</typeparam>
	/// <param name="this">The task completion source. May not be <c>null</c>.</param>
	/// <param name="task">The task. May not be <c>null</c>.</param>
	/// <param name="resultFunc">
	/// A delegate that returns the result with which to complete the task completion source, if the task completed successfully.
	/// May not be <c>null</c>.
	/// </param>
	/// <returns>
	/// <c>true</c> if this method completed the task completion source;
	/// <c>false</c> if it was already completed.
	/// </returns>
	public static bool TryCompleteFromCompletedTask<TResult>(
		this TaskCompletionSource<TResult> @this,
		Task                               task,
		Func<TResult>                      resultFunc)
	{
		if (@this == null) throw new ArgumentNullException(nameof(@this));
		if (task == null) throw new ArgumentNullException(nameof(task));
		if (resultFunc == null) throw new ArgumentNullException(nameof(resultFunc));

		if (task.IsFaulted)
		{
			Debug.Assert(task.Exception != null, "task.Exception != null");
			return @this.TrySetException(task.Exception.InnerExceptions);
		}

		// ReSharper disable once InvertIf
		if (task.IsCanceled)
		{
			try
			{
				task.WaitAndUnwrapException();
			}
			catch (OperationCanceledException exception)
			{
				CancellationToken token = exception.CancellationToken;
				return token.IsCancellationRequested ? @this.TrySetCanceled(token) : @this.TrySetCanceled();
			}
		}

		return @this.TrySetResult(resultFunc());
	}

	/// <summary>
	/// Creates a new TCS for use with async code, and which forces its continuations to execute asynchronously.
	/// </summary>
	/// <typeparam name="TResult">The type of the result of the TCS.</typeparam>
	public static TaskCompletionSource<TResult> CreateAsyncTaskSource<TResult>()
	{
		return new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
	}
}
