///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

// ReSharper disable AccessToDisposedClosure

namespace GriffinPlus.Lib.Threading
{

	/// <summary>
	/// Tests targeting the <see cref="SerialTaskQueue"/> class.
	/// </summary>
	[Collection(nameof(NoParallelizationCollection))]
	public class SerialTaskQueueTests
	{
		/// <summary>
		/// Timeout when waiting for a task to complete (in ms).
		/// </summary>
		private const int Timeout = 1000;

		#region SerialTaskQueue()

		/// <summary>
		/// Tests the <see cref="SerialTaskQueue()"/> constructor.
		/// </summary>
		[Fact]
		public void Create()
		{
			var queue = new SerialTaskQueue();
			Assert.IsType<SerialTaskQueueSynchronizationContext>(queue.SynchronizationContext);
		}

		#endregion

		#region Task Enqueue(Action action)

		/// <summary>
		/// Tests the <see cref="SerialTaskQueue.Enqueue(Action)"/> method.
		/// A single callback is scheduled for execution.
		/// </summary>
		[Fact]
		public async Task Enqueue_Synchronous_Action_SingleInvocation()
		{
			using var cts = new CancellationTokenSource(Timeout);
			var queue = new SerialTaskQueue();
			var startProcessingEvent = new AsyncManualResetEvent(false);
			bool finished = false;

			Task task = queue.Enqueue(
				() =>
				{
					startProcessingEvent.Wait(cts.Token);
					finished = true;
				});

			Assert.False(task.IsCompleted);
			Assert.False(finished);
			startProcessingEvent.Set();
			await task;
			Assert.True(task.IsCompleted);
			Assert.True(finished);
		}

		/// <summary>
		/// Tests the <see cref="SerialTaskQueue.Enqueue(Action)"/> method.
		/// Multiple callbacks are scheduled for execution.
		/// </summary>
		[Fact]
		public async Task Enqueue_Synchronous_Action_MultipleInvocations()
		{
			// Create a new queue and enqueue multiple invocations of a callback adding the iteration counter value to
			// a list. At the end the list should contain the numbers in ascending order. The first callback should
			// delay the execution, so all tasks should not have been completed, yet.
			using var cts = new CancellationTokenSource(Timeout);
			var queue = new SerialTaskQueue();
			var startProcessingEvent = new AsyncManualResetEvent(false);
			var tasks = new List<Task>();
			var expectedOrder = new List<int>();
			var actualOrder = new List<int>();
			for (int i = 0; i < 1000; i++)
			{
				int value = i;
				expectedOrder.Add(value);
				tasks.Add(
					queue.Enqueue(
						() =>
						{
							startProcessingEvent.Wait(cts.Token);
							actualOrder.Add(value);
						}));
			}

			Assert.All(tasks, task => Assert.False(task.IsCompleted));
			startProcessingEvent.Set();
			await Task.WhenAll(tasks);
			Assert.All(tasks, task => Assert.True(task.IsCompleted && !task.IsCanceled && !task.IsFaulted));
			Assert.Equal(expectedOrder, actualOrder);
		}

		/// <summary>
		/// Tests the <see cref="SerialTaskQueue.Enqueue(Action)"/> method.
		/// The method should throw an <see cref="ArgumentNullException"/> if the specified action is <c>null</c>.
		/// </summary>
		[Fact]
		public async Task Enqueue_Synchronous_Action_ActionIsNull()
		{
			var queue = new SerialTaskQueue();
			var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await queue.Enqueue((Action)null));
			Assert.Equal("action", exception.ParamName);
		}

		#endregion

		#region Task<TResult> Enqueue<TResult>(Func<TResult> function)

		/// <summary>
		/// Tests the <see cref="SerialTaskQueue.Enqueue{TResult}(Func{TResult})"/> method.
		/// A single callback is scheduled for execution.
		/// </summary>
		[Fact]
		public async Task Enqueue_Synchronous_Function_SingleInvocation()
		{
			const int expectedResult = 42;

			using var cts = new CancellationTokenSource(Timeout);
			var queue = new SerialTaskQueue();
			var startProcessingEvent = new AsyncManualResetEvent(false);
			bool finished = false;

			Task<int> task = queue.Enqueue(
				() =>
				{
					startProcessingEvent.Wait(cts.Token);
					finished = true;
					return expectedResult;
				});

			Assert.False(task.IsCompleted);
			Assert.False(finished);
			startProcessingEvent.Set();
			int result = await task;
			Assert.Equal(expectedResult, result);
			Assert.True(task.IsCompleted);
			Assert.True(finished);
		}

		/// <summary>
		/// Tests the <see cref="SerialTaskQueue.Enqueue{TResult}(Func{TResult})"/> method.
		/// Multiple callbacks are scheduled for execution.
		/// </summary>
		[Fact]
		public async Task Enqueue_Synchronous_Function_MultipleInvocations()
		{
			// Create a new queue and enqueue multiple invocations of a callback adding the iteration counter value to
			// a list. At the end the list should contain the numbers in ascending order. The first callback should
			// delay the execution, so all tasks should not have been completed, yet.
			using var cts = new CancellationTokenSource(Timeout);
			var queue = new SerialTaskQueue();
			var startProcessingEvent = new AsyncManualResetEvent(false);
			var tasks = new List<Task<int>>();
			var expectedOrder = new List<int>();
			var actualOrder = new List<int>();
			for (int i = 0; i < 1000; i++)
			{
				int value = i;
				expectedOrder.Add(value);
				tasks.Add(
					queue.Enqueue(
						() =>
						{
							startProcessingEvent.Wait(cts.Token);
							actualOrder.Add(value);
							return value;
						}));
			}

			Assert.All(tasks, task => Assert.False(task.IsCompleted));
			startProcessingEvent.Set();
			await Task.WhenAll(tasks);
			Assert.All(tasks, task => Assert.True(task.IsCompleted && !task.IsCanceled && !task.IsFaulted));
			Assert.Equal(expectedOrder, actualOrder);
			Assert.Equal(expectedOrder, tasks.Select(x => x.Result));
		}

		/// <summary>
		/// Tests the <see cref="SerialTaskQueue.Enqueue{TResult}(Func{TResult})"/> method.
		/// The method should throw an <see cref="ArgumentNullException"/> if the specified action is <c>null</c>.
		/// </summary>
		[Fact]
		public async Task Enqueue_Synchronous_Function_ActionIsNull()
		{
			var queue = new SerialTaskQueue();
			var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await queue.Enqueue((Func<int>)null));
			Assert.Equal("function", exception.ParamName);
		}

		#endregion

		#region Task<Task> Enqueue(Func<Task> action)

		/// <summary>
		/// Tests the <see cref="SerialTaskQueue.Enqueue(Func{Task})"/> method.
		/// A single callback is scheduled for execution.
		/// </summary>
		[Fact]
		public async Task Enqueue_Asynchronous_Action_SingleInvocation()
		{
			using var cts = new CancellationTokenSource(Timeout);
			var queue = new SerialTaskQueue();
			var startProcessingEvent = new AsyncManualResetEvent(false);
			bool finished = false;

			Task task = queue.Enqueue(
				async () =>
				{
					await startProcessingEvent.WaitAsync(cts.Token).ConfigureAwait(false);
					finished = true;
				});

			Assert.False(task.IsCompleted);
			Assert.False(finished);
			startProcessingEvent.Set();
			await task;
			Assert.True(task.IsCompleted);
			Assert.True(finished);
		}

		/// <summary>
		/// Tests the <see cref="SerialTaskQueue.Enqueue(Func{Task})"/> method.
		/// Multiple callbacks are scheduled for execution.
		/// </summary>
		[Fact]
		public async Task Enqueue_Asynchronous_Action_MultipleInvocations()
		{
			// Create a new queue and enqueue multiple invocations of a callback adding the iteration counter value to
			// a list. At the end the list should contain the numbers in ascending order. The first callback should
			// delay the execution, so all tasks should not have been completed, yet.
			using var cts = new CancellationTokenSource(Timeout);
			var queue = new SerialTaskQueue();
			var startProcessingEvent = new AsyncManualResetEvent(false);
			var tasks = new List<Task>();
			var expectedOrder = new List<int>();
			var actualOrder = new List<int>();
			for (int i = 0; i < 1000; i++)
			{
				int value = i;
				expectedOrder.Add(value);
				tasks.Add(
					queue.Enqueue(
						async () =>
						{
							await startProcessingEvent.WaitAsync(cts.Token).ConfigureAwait(false);
							actualOrder.Add(value);
						}));
			}

			Assert.All(tasks, task => Assert.False(task.IsCompleted));
			startProcessingEvent.Set();
			await Task.WhenAll(tasks);
			Assert.All(tasks, task => Assert.True(task.IsCompleted));
			Assert.Equal(expectedOrder, actualOrder);
		}

		/// <summary>
		/// Tests the <see cref="SerialTaskQueue.Enqueue(Func{Task})"/> method.
		/// The method should throw an <see cref="ArgumentNullException"/> if the specified action is <c>null</c>.
		/// </summary>
		[Fact]
		public async Task Enqueue_Asynchronous_Action_ActionIsNull()
		{
			var queue = new SerialTaskQueue();
			var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await queue.Enqueue(null));
			Assert.Equal("asyncAction", exception.ParamName);
		}

		#endregion

		#region Task<TResult> Enqueue<TResult>(Func<Task<TResult>> function)

		/// <summary>
		/// Tests the <see cref="SerialTaskQueue.Enqueue{TResult}(Func{Task{TResult}})"/> method.
		/// A single callback is scheduled for execution.
		/// </summary>
		[Fact]
		public async Task Enqueue_Asynchronous_Func_SingleInvocation()
		{
			const int expectedResult = 42;

			using var startProcessingEvent = new ManualResetEventSlim();
			var queue = new SerialTaskQueue();
			bool finished = false;

			Task<int> task = queue.Enqueue(
				() =>
				{
					Assert.True(startProcessingEvent.Wait(Timeout));
					finished = true;
					return expectedResult;
				});

			Assert.False(task.IsCompleted);
			Assert.False(finished);
			startProcessingEvent.Set();
			int result = await task;
			Assert.Equal(expectedResult, result);
			Assert.True(task.IsCompleted);
			Assert.True(finished);
		}

		/// <summary>
		/// Tests the <see cref="SerialTaskQueue.Enqueue{TResult}(Func{Task{TResult}})"/> method.
		/// Multiple callbacks are scheduled for execution.
		/// </summary>
		[Fact]
		public async Task Enqueue_Asynchronous_Function_MultipleInvocations()
		{
			// Create a new queue and enqueue multiple invocations of a callback adding the iteration counter value to
			// a list. At the end the list should contain the numbers in ascending order. The first callback should
			// delay the execution, so all tasks should not have been completed, yet.
			using var cts = new CancellationTokenSource(Timeout);
			var queue = new SerialTaskQueue();
			var startProcessingEvent = new AsyncManualResetEvent(false);
			var tasks = new List<Task<int>>();
			var expectedOrder = new List<int>();
			var actualOrder = new List<int>();
			for (int i = 0; i < 1000; i++)
			{
				int value = i;
				expectedOrder.Add(value);
				tasks.Add(
					queue.Enqueue(
						async () =>
						{
							await startProcessingEvent.WaitAsync(cts.Token).ConfigureAwait(false);
							actualOrder.Add(value);
							return value;
						}));
			}

			Assert.All(tasks, task => Assert.False(task.IsCompleted));
			startProcessingEvent.Set();
			await Task.WhenAll(tasks);
			Assert.All(tasks, task => Assert.True(task.IsCompleted));
			Assert.Equal(expectedOrder, actualOrder);
			Assert.Equal(expectedOrder, tasks.Select(x => x.Result));
		}

		/// <summary>
		/// Tests the <see cref="SerialTaskQueue.Enqueue{TResult}(Func{Task{TResult}})"/> method.
		/// The method should throw an <see cref="ArgumentNullException"/> if the specified function is <c>null</c>.
		/// </summary>
		[Fact]
		public async Task Enqueue_Asynchronous_function_ActionIsNull()
		{
			var queue = new SerialTaskQueue();
			var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await queue.Enqueue((Func<Task<int>>)null));
			Assert.Equal("asyncFunction", exception.ParamName);
		}

		#endregion
	}

}
