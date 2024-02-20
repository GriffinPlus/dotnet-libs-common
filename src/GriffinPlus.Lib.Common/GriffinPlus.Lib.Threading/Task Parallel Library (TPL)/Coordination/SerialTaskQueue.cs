///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading;

/// <summary>
/// A queue that ensures that synchronous/asynchronous actions/functions are executed one after the other
/// using the Task Parallel Library (TPL).
/// </summary>
public sealed class SerialTaskQueue
{
	private readonly object              mMutex    = new();
	private readonly WeakReference<Task> mLastTask = new(null);

	/// <summary>
	/// Initializes a new instance of the <see cref="SerialTaskQueue"/> class.
	/// </summary>
	public SerialTaskQueue()
	{
		SynchronizationContext = new SerialTaskQueueSynchronizationContext(this);
	}

	/// <summary>
	/// Gets the synchronization context that can be used to dispatch asynchronous messages using the queue.
	/// </summary>
	public SerialTaskQueueSynchronizationContext SynchronizationContext { get; }

	/// <summary>
	/// Enqueues a synchronous action for execution.
	/// The action is always executed by a TPL thread.
	/// </summary>
	/// <param name="action">Synchronous action to schedule for execution.</param>
	/// <returns>A <see cref="Task"/> identifying the scheduled action.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
	public Task Enqueue(Action action)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));

		lock (mMutex)
		{
			// Schedule executing the specified function on a TPL thread.
			// Ensure that the function is _NOT_ executed by the current thread to avoid deadlocks that can occur
			// if the executing thread holds synchronization objects.
			Task resultTask = mLastTask.TryGetTarget(out Task lastTask)
				                  ? lastTask.ContinueWith(
					                  (_, state) => ((Action)state)(),
					                  action,
					                  TaskContinuationOptions.RunContinuationsAsynchronously)
				                  : Task.Run(action);

			mLastTask.SetTarget(resultTask);

			return resultTask;
		}
	}

	/// <summary>
	/// Enqueues a synchronous function for execution.
	/// The function is always executed by a TPL thread.
	/// </summary>
	/// <typeparam name="TResult">Result type of the function to schedule for execution.</typeparam>
	/// <param name="function">Synchronous function to schedule for execution.</param>
	/// <returns>A <see cref="Task"/> identifying the scheduled function.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="function"/> is <c>null</c>.</exception>
	public Task<TResult> Enqueue<TResult>(Func<TResult> function)
	{
		if (function == null) throw new ArgumentNullException(nameof(function));

		lock (mMutex)
		{
			// Schedule executing the specified function on a TPL thread.
			// Ensure that the function is _NOT_ executed by the current thread to avoid deadlocks that can occur
			// if the executing thread holds synchronization objects.
			Task<TResult> resultTask = mLastTask.TryGetTarget(out Task lastTask)
				                           ? lastTask.ContinueWith(
					                           (_, state) => ((Func<TResult>)state)(),
					                           function,
					                           TaskContinuationOptions.RunContinuationsAsynchronously)
				                           : Task.Run(function);

			mLastTask.SetTarget(resultTask);

			return resultTask;
		}
	}

	/// <summary>
	/// Enqueues an asynchronous action for execution.
	/// The action is always executed by a TPL thread.
	/// </summary>
	/// <param name="asyncAction">Asynchronous action to schedule for execution.</param>
	/// <returns>A <see cref="Task"/> identifying the scheduled action.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
	public Task Enqueue(Func<Task> asyncAction)
	{
		if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

		lock (mMutex)
		{
			// Schedule executing the specified action on a TPL thread.
			// Ensure that the function is _NOT_ executed by the current thread to avoid deadlocks that can occur
			// if the executing thread holds synchronization objects.
			Task resultTask = mLastTask.TryGetTarget(out Task lastTask)
				                  ? lastTask.ContinueWith(
						                  (_, state) => ((Func<Task>)state)(),
						                  asyncAction,
						                  TaskContinuationOptions.RunContinuationsAsynchronously)
					                  .Unwrap()
				                  : Task.Run(asyncAction);

			mLastTask.SetTarget(resultTask);

			return resultTask;
		}
	}

	/// <summary>
	/// Enqueues an asynchronous function for execution.
	/// The function is always executed by a TPL thread.
	/// </summary>
	/// <typeparam name="TResult">Result type of the function to schedule for execution.</typeparam>
	/// <param name="asyncFunction">Asynchronous function to schedule for execution.</param>
	/// <returns>A <see cref="Task"/> identifying the scheduled function.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="asyncFunction"/> is <c>null</c>.</exception>
	public Task<TResult> Enqueue<TResult>(Func<Task<TResult>> asyncFunction)
	{
		if (asyncFunction == null) throw new ArgumentNullException(nameof(asyncFunction));

		lock (mMutex)
		{
			// Schedule executing the specified function on a TPL thread.
			// Ensure that the function is _NOT_ executed by the current thread to avoid deadlocks that can occur
			// if the executing thread holds synchronization objects.
			Task<TResult> resultTask = mLastTask.TryGetTarget(out Task lastTask)
				                           ? lastTask.ContinueWith(
						                           (_, state) => ((Func<Task<TResult>>)state)(),
						                           asyncFunction,
						                           TaskContinuationOptions.RunContinuationsAsynchronously)
					                           .Unwrap()
				                           : Task.Run(asyncFunction);

			mLastTask.SetTarget(resultTask);

			return resultTask;
		}
	}
}
