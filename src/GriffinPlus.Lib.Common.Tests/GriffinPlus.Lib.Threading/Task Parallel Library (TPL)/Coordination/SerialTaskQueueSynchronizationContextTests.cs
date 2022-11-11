///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using Xunit;

// ReSharper disable AccessToDisposedClosure

namespace GriffinPlus.Lib.Threading
{

	/// <summary>
	/// Tests targeting the <see cref="SerialTaskQueueSynchronizationContext"/> class.
	/// </summary>
	[Collection(nameof(NoParallelizationCollection))]
	public class SerialTaskQueueSynchronizationContextTests
	{
		/// <summary>
		/// Timeout when waiting for a task to complete (in ms).
		/// </summary>
		private const int Timeout = 1000;

		#region Creating

		/// <summary>
		/// Tests creating a <see cref="SerialTaskQueueSynchronizationContext"/> as part of the <see cref="SerialTaskQueue"/> class.
		/// </summary>
		[Fact]
		public void Create()
		{
			var queue = new SerialTaskQueue();
			Assert.IsType<SerialTaskQueueSynchronizationContext>(queue.SynchronizationContext);
		}

		#endregion

		#region void Post(SendOrPostCallback callback, object state)

		/// <summary>
		/// Tests the <see cref="SerialTaskQueueSynchronizationContext.Post"/> method.
		/// A single callback is scheduled for execution.
		/// </summary>
		[Fact]
		public void Post_SingleInvocation()
		{
			using (var cts = new CancellationTokenSource(Timeout))
			{
				var queue = new SerialTaskQueue();
				var startProcessingEvent = new AsyncManualResetEvent(false);
				var processingFinishedEvent = new AsyncManualResetEvent(false);
				object obj = new object();

				queue.SynchronizationContext.Post(
					state =>
					{
						Assert.Same(obj, state);
						startProcessingEvent.Wait(cts.Token);
						processingFinishedEvent.Set();
					},
					obj);

				Assert.False(processingFinishedEvent.IsSet);
				startProcessingEvent.Set();
				processingFinishedEvent.Wait(cts.Token);
				Assert.True(processingFinishedEvent.IsSet);
			}
		}

		/// <summary>
		/// Tests the <see cref="SerialTaskQueueSynchronizationContext.Post"/> method.
		/// Multiple callbacks are scheduled for execution.
		/// </summary>
		[Fact]
		public void Post_MultipleInvocations()
		{
			const int iterations = 1000;

			using (var cts = new CancellationTokenSource(Timeout))
			{
				var queue = new SerialTaskQueue();
				var startProcessingEvent = new AsyncManualResetEvent(false);
				var processingFinishedEvent = new AsyncManualResetEvent(false);
				int invocationCounter = 0;
				var expectedOrder = new List<int>();
				var actualOrder = new List<int>();
				object obj = new object();
				for (int i = 0; i < iterations; i++)
				{
					int value = i;
					expectedOrder.Add(value);

					queue.SynchronizationContext.Post(
						state =>
						{
							Assert.Same(obj, state);
							startProcessingEvent.Wait(cts.Token);
							actualOrder.Add(value);
							// ReSharper disable once AccessToModifiedClosure
							if (Interlocked.Increment(ref invocationCounter) == iterations)
								processingFinishedEvent.Set();
						},
						obj);
				}

				// give the first scheduled callback a chance to start execution
				// (the resulting order list should still be empty as execution is stopped at the processing event)
				Thread.Sleep(1);
				Assert.Empty(actualOrder);

				// start processing and wait for it to complete
				// (the resulting order list should now equal the expected order list)
				startProcessingEvent.Set();
				processingFinishedEvent.Wait(cts.Token);
				Assert.Equal(expectedOrder, actualOrder);
			}
		}

		/// <summary>
		/// Tests the <see cref="SerialTaskQueueSynchronizationContext.Post"/> method.
		/// The method should throw an <see cref="ArgumentNullException"/> if the specified callback is <c>null</c>.
		/// </summary>
		[Fact]
		public void Post_CallbackIsNull()
		{
			var queue = new SerialTaskQueue();
			// ReSharper disable once AssignNullToNotNullAttribute
			var exception = Assert.Throws<ArgumentNullException>(() => queue.SynchronizationContext.Post(null, null));
			Assert.Equal("callback", exception.ParamName);
		}

		#endregion

		#region void Send(SendOrPostCallback callback, object state)

		/// <summary>
		/// Tests the <see cref="SerialTaskQueueSynchronizationContext.Send"/> method.
		/// A single callback is scheduled for execution.
		/// </summary>
		[Fact]
		public void Send_SingleInvocation()
		{
			var queue = new SerialTaskQueue();
			bool finished = false;
			object obj = new object();

			queue.SynchronizationContext.Send(
				state =>
				{
					Assert.Same(obj, state);
					Thread.Sleep(10);
					finished = true;
				},
				obj);

			Assert.True(finished);
		}

		/// <summary>
		/// Tests the <see cref="SerialTaskQueueSynchronizationContext.Send"/> method.
		/// Multiple callbacks are scheduled for execution.
		/// </summary>
		[Fact]
		public void Send_MultipleInvocations()
		{
			const int iterations = 1000;

			var queue = new SerialTaskQueue();
			var expectedOrder = new List<int>();
			var actualOrder = new List<int>();
			object obj = new object();
			for (int i = 0; i < iterations; i++)
			{
				int value = i;
				expectedOrder.Add(value);

				queue.SynchronizationContext.Send(
					state =>
					{
						Assert.Same(obj, state);
						actualOrder.Add(value);
					},
					obj);
			}

			// the resulting order list should now equal the expected order list
			Assert.Equal(expectedOrder, actualOrder);
		}

		/// <summary>
		/// Tests the <see cref="SerialTaskQueueSynchronizationContext.Send"/> method.
		/// The method should throw an <see cref="ArgumentNullException"/> if the specified callback is <c>null</c>.
		/// </summary>
		[Fact]
		public void Send_CallbackIsNull()
		{
			var queue = new SerialTaskQueue();
			// ReSharper disable once AssignNullToNotNullAttribute
			var exception = Assert.Throws<ArgumentNullException>(() => queue.SynchronizationContext.Send(null, null));
			Assert.Equal("callback", exception.ParamName);
		}

		#endregion

		#region SynchronizationContext CreateCopy()

		/// <summary>
		/// Tests the <see cref="SerialTaskQueueSynchronizationContext.CreateCopy"/> method.
		/// The copy should refer to the same <see cref="SerialTaskQueue"/> instance, have the same hash code and
		/// equal to the original synchronization context.
		/// </summary>
		[Fact]
		public void CreateCopy()
		{
			var queue = new SerialTaskQueue();
			SerialTaskQueueSynchronizationContext context = queue.SynchronizationContext;
			var copy = (SerialTaskQueueSynchronizationContext)context.CreateCopy();
			Assert.Same(context.Queue, copy.Queue);
			Assert.Equal(context.GetHashCode(), copy.GetHashCode());
			Assert.Equal(context, copy);
		}

		#endregion

		#region int GetHashCode()

		/// <summary>
		/// Tests the <see cref="SerialTaskQueueSynchronizationContext.GetHashCode"/> method.
		/// Two <see cref="SerialTaskQueue"/> instances should have different hash codes.
		/// </summary>
		[Fact]
		public void GetHashCode_()
		{
			var queue1 = new SerialTaskQueue();
			var queue2 = new SerialTaskQueue();
			int hashCode1 = queue1.SynchronizationContext.GetHashCode();
			int hashCode2 = queue2.SynchronizationContext.GetHashCode();
			Assert.NotEqual(hashCode1, hashCode2);
		}

		#endregion

		#region bool Equals(object obj)

		public static IEnumerable<object[]> Equals_TestData
		{
			get
			{
				SerialTaskQueueSynchronizationContext context = new SerialTaskQueue().SynchronizationContext;

				// object to compare with is the same synchronization context
				yield return new object[]
				{
					context,
					context,
					true
				};

				// object to compare with is a copy of the synchronization context
				yield return new object[]
				{
					context,
					context.CreateCopy(),
					true
				};

				// object to compare with is a synchronization context of the same type, but attached to a different queue
				yield return new object[]
				{
					context,
					new SerialTaskQueue().SynchronizationContext,
					false
				};

				// object to compare with is a synchronization context of a different type
				yield return new object[]
				{
					context,
					new SynchronizationContext(),
					false
				};

				// object to compare with is null
				yield return new object[]
				{
					context,
					null,
					false
				};
			}
		}

		/// <summary>
		/// Tests the <see cref="SerialTaskQueueSynchronizationContext.Equals(object)"/> method.
		/// </summary>
		[Theory]
		[MemberData(nameof(Equals_TestData))]
		public void Equals_(
			SerialTaskQueueSynchronizationContext context,
			object                                obj,
			bool                                  expected)
		{
			Assert.Equal(expected, context.Equals(obj));
		}

		#endregion
	}

}
