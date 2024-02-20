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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading;

/// <summary>
/// Provides a context for asynchronous operations (thread-safe).
/// </summary>
/// <remarks>
/// <see cref="Execute()"/> may only be called once.
/// After <see cref="Execute()"/> returns, the async context should be disposed.
/// </remarks>
[DebuggerDisplay("Id = {Id}, OperationCount = {mOutstandingOperations}")]
[DebuggerTypeProxy(typeof(DebugView))]
public sealed partial class AsyncContext : IDisposable
{
	/// <summary>
	/// The queue holding the actions to run.
	/// </summary>
	private readonly TaskQueue mQueue;

	/// <summary>
	/// The <see cref="SynchronizationContext"/> for this <see cref="AsyncContext"/>.
	/// </summary>
	private readonly AsyncContextSynchronizationContext mSynchronizationContext;

	/// <summary>
	/// The <see cref="TaskScheduler"/> for this <see cref="AsyncContext"/>.
	/// </summary>
	private readonly AsyncContextTaskScheduler mTaskScheduler;

	/// <summary>
	/// The number of outstanding operations, including actions in the queue.
	/// </summary>
	private int mOutstandingOperations;

	/// <summary>
	/// Initializes a new instance of the <see cref="AsyncContext"/> class.
	/// This is an advanced operation; most people should use one of the static <c>Run</c> methods instead.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public AsyncContext()
	{
		mQueue = new TaskQueue();
		mSynchronizationContext = new AsyncContextSynchronizationContext(this);
		mTaskScheduler = new AsyncContextTaskScheduler(this);
		Factory = new TaskFactory(
			CancellationToken.None,
			TaskCreationOptions.HideScheduler,
			TaskContinuationOptions.HideScheduler,
			mTaskScheduler);
	}

	/// <summary>
	/// Gets a semi-unique identifier for this asynchronous context.
	/// This is the same identifier as the context's <see cref="TaskScheduler"/> id.
	/// </summary>
	public int Id => mTaskScheduler.Id;

	/// <summary>
	/// Increments the outstanding asynchronous operation count.
	/// </summary>
	private void OperationStarted()
	{
		Interlocked.Increment(ref mOutstandingOperations);
	}

	/// <summary>
	/// Decrements the outstanding asynchronous operation count.
	/// </summary>
	private void OperationCompleted()
	{
		int newCount = Interlocked.Decrement(ref mOutstandingOperations);
		if (newCount == 0)
		{
			mQueue.CompleteAdding();
		}
	}

	/// <summary>
	/// Queues a task for execution by <see cref="Execute"/>.
	/// If all tasks have been completed and the outstanding asynchronous operation count is zero, then this method has undefined behavior.
	/// </summary>
	/// <param name="task">The task to queue. May not be <c>null</c>.</param>
	/// <param name="propagateExceptions">
	/// A value indicating whether exceptions on this task should be propagated out of the main loop.
	/// </param>
	private void Enqueue(Task task, bool propagateExceptions)
	{
		OperationStarted();

		task.ContinueWith(
			_ => OperationCompleted(),
			CancellationToken.None,
			TaskContinuationOptions.ExecuteSynchronously,
			mTaskScheduler);

		mQueue.TryAdd(task, propagateExceptions);

		// If we fail to add to the queue, just drop the Task.
		// This is the same behavior as the TaskScheduler.FromCurrentSynchronizationContext(WinFormsSynchronizationContext).
	}

	/// <summary>
	/// Disposes all resources used by this class.
	/// This method should NOT be called, while <see cref="Execute"/> is executing.
	/// </summary>
	public void Dispose()
	{
		mQueue.Dispose();
	}

	/// <summary>
	/// Executes all queued actions.
	/// This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero.
	/// This method will unwrap and propagate errors from tasks that are supposed to propagate errors.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void Execute()
	{
		SynchronizationContextSwitcher.ApplyContext(
			mSynchronizationContext,
			() =>
			{
				IEnumerable<Tuple<Task, bool>> tasks = mQueue.GetConsumingEnumerable();
				foreach (Tuple<Task, bool> task in tasks)
				{
					mTaskScheduler.DoTryExecuteTask(task.Item1);

					// propagate exception if necessary
					if (task.Item2)
					{
						task.Item1.WaitAndUnwrapException();
					}
				}
			});
	}

	/// <summary>
	/// Queues a task for execution, and begins executing all tasks in the queue.
	/// This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero.
	/// This method will unwrap and propagate errors from the task.
	/// </summary>
	/// <param name="action">The action to execute. May not be <c>null</c>.</param>
	public static void Run(Action action)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));

		using var context = new AsyncContext();
		Task task = context.Factory.Run(action);
		context.Execute();
		task.WaitAndUnwrapException();
	}

	/// <summary>
	/// Queues a task for execution, and begins executing all tasks in the queue.
	/// This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero.
	/// Returns the result of the task. This method will unwrap and propagate errors from the task.
	/// </summary>
	/// <typeparam name="TResult">The result type of the task.</typeparam>
	/// <param name="action">The action to execute. May not be <c>null</c>.</param>
	public static TResult Run<TResult>(Func<TResult> action)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));

		using var context = new AsyncContext();
		Task<TResult> task = context.Factory.Run(action);
		context.Execute();
		return task.WaitAndUnwrapException();
	}

	/// <summary>
	/// Queues a task for execution, and begins executing all tasks in the queue.
	/// This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero.
	/// This method will unwrap and propagate errors from the task proxy.
	/// </summary>
	/// <param name="action">The action to execute. May not be <c>null</c>.</param>
	public static void Run(Func<Task> action)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));

		using var context = new AsyncContext();
		context.OperationStarted();
		Task task = context.Factory.Run(action)
			.ContinueWith(
				t =>
				{
					// ReSharper disable once AccessToDisposedClosure
					context.OperationCompleted();
					t.WaitAndUnwrapException();
				},
				CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously,
				context.mTaskScheduler);
		context.Execute();
		task.WaitAndUnwrapException();
	}

	/// <summary>
	/// Queues a task for execution, and begins executing all tasks in the queue.
	/// This method returns when all tasks have been completed and the outstanding asynchronous operation count is zero.
	/// Returns the result of the task proxy. This method will unwrap and propagate errors from the task proxy.
	/// </summary>
	/// <typeparam name="TResult">The result type of the task.</typeparam>
	/// <param name="action">The action to execute. May not be <c>null</c>.</param>
	public static TResult Run<TResult>(Func<Task<TResult>> action)
	{
		if (action == null)
			throw new ArgumentNullException(nameof(action));

		using var context = new AsyncContext();
		context.OperationStarted();
		Task<TResult> task = context.Factory.Run(action)
			.ContinueWith(
				t =>
				{
					// ReSharper disable once AccessToDisposedClosure
					context.OperationCompleted();
					return t.WaitAndUnwrapException();
				},
				CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously,
				context.mTaskScheduler);
		context.Execute();
		return task.WaitAndUnwrapException();
	}

	/// <summary>
	/// Gets the current <see cref="AsyncContext"/> for this thread,
	/// or <c>null</c> if this thread is not currently running in an <see cref="AsyncContext"/>.
	/// </summary>
	public static AsyncContext Current
	{
		get
		{
			var syncContext = SynchronizationContext.Current as AsyncContextSynchronizationContext;
			return syncContext?.Context;
		}
	}

	/// <summary>
	/// Gets the <see cref="SynchronizationContext"/> for this <see cref="AsyncContext"/>.
	/// From inside <see cref="Execute"/>, this value is always equal to <see cref="System.Threading.SynchronizationContext.Current"/>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public SynchronizationContext SynchronizationContext => mSynchronizationContext;

	/// <summary>
	/// Gets the <see cref="TaskScheduler"/> for this <see cref="AsyncContext"/>.
	/// From inside <see cref="Execute"/>, this value is always equal to <see cref="TaskScheduler.Current"/>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public TaskScheduler Scheduler => mTaskScheduler;

	/// <summary>
	/// Gets the <see cref="TaskFactory"/> for this <see cref="AsyncContext"/>.
	/// Note that this factory has the <see cref="TaskCreationOptions.HideScheduler"/> option set.
	/// Be careful with async delegates; you may need to call <see cref="M:System.Threading.SynchronizationContext.OperationStarted"/>
	/// and <see cref="M:System.Threading.SynchronizationContext.OperationCompleted"/> to prevent early termination of
	/// this <see cref="AsyncContext"/>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public TaskFactory Factory { get; }

	[DebuggerNonUserCode]
	internal sealed class DebugView(AsyncContext context)
	{
		public TaskScheduler TaskScheduler => context.mTaskScheduler;
	}
}
