///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GriffinPlus.Lib.Threading;

using Xunit;

namespace GriffinPlus.Lib.Events
{

	/// <summary>
	/// Unit tests targeting the <see cref="WeakEventManager{T}"/> class.
	/// </summary>
	[Collection(nameof(NoParallelizationCollection))]
	public class WeakEventManagerTests : IDisposable
	{
		private const string EventName = "MyEvent";

		private AsyncContextThread mThread;

		/// <summary>
		/// Initializes an instance the <see cref="WeakEventManagerTests"/> class performing common initialization before running a test.
		/// </summary>
		public WeakEventManagerTests()
		{
			mThread = new AsyncContextThread();
		}


		/// <summary>
		/// Cleans up.
		/// </summary>
		public void Dispose()
		{
			if (mThread != null)
			{
				mThread.Dispose();
				mThread = null;
			}
		}


		/// <summary>
		/// Tests registering, firing and unregistering an event without using a synchronization context.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Complete_WithoutSynchronizationContext(bool scheduleAlways)
		{
			var recipient = new EventManagerEventArgsRecipient();

			// register event handler
			int regCount = WeakEventManager<EventManagerEventArgs>.RegisterEventHandler(
				this,
				EventName,
				recipient.Handler,
				null,
				scheduleAlways);
			Assert.Equal(1, regCount);

			// check whether the handler is registered
			Assert.True(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName));
			Assert.True(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName, recipient.Handler));

			// fire event
			string testData = Guid.NewGuid().ToString("D");
			WeakEventManager<EventManagerEventArgs>.FireEvent(this, EventName, this, new EventManagerEventArgs(testData));
			if (scheduleAlways)
			{
				// handler is called asynchronously
				// => wait for the handler to be called and continue
				Assert.True(recipient.HandlerCalledEvent.Wait(1000));
				Assert.Null(recipient.SynchronizationContext); // synchronization context should be null for thread pool threads
			}
			else
			{
				// handler is called synchronously
				Assert.NotNull(recipient.SynchronizationContext);
				Assert.Same(SynchronizationContext.Current, recipient.SynchronizationContext);
			}

			// the event should have received the expected test data
			Assert.Equal(testData, recipient.Data);

			// unregister event handler
			regCount = WeakEventManager<EventManagerEventArgs>.UnregisterEventHandler(this, EventName, recipient.Handler);
			Assert.Equal(0, regCount);

			// check whether the handler is not registered anymore
			Assert.False(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName));
			Assert.False(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName, recipient.Handler));
		}


		/// <summary>
		/// Tests registering with firing immediately and unregistering an event without using a synchronization context.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Complete_WithoutSynchronizationContext_FireImmediately(bool scheduleAlways)
		{
			var recipient = new EventManagerEventArgsRecipient();

			int regCount;
			string testData = Guid.NewGuid().ToString("D");
			if (scheduleAlways)
			{
				// register event handler and fire it immediately
				regCount = WeakEventManager<EventManagerEventArgs>.RegisterEventHandler(
					this,
					EventName,
					recipient.Handler,
					null,
					true,
					true,
					this,
					new EventManagerEventArgs(testData));
				Assert.Equal(1, regCount);

				// handler is called asynchronously
				// => wait for the handler to be called and continue
				Assert.True(recipient.HandlerCalledEvent.Wait(1000));
				Assert.Null(recipient.SynchronizationContext); // synchronization context should be null for thread pool threads
			}
			else
			{
				// register event handler and fire it immediately
				regCount = WeakEventManager<EventManagerEventArgs>.RegisterEventHandler(
					this,
					EventName,
					recipient.Handler,
					null,
					false,
					true,
					this,
					new EventManagerEventArgs(testData));
				Assert.Equal(1, regCount);

				// handler is called synchronously
				Assert.NotNull(recipient.SynchronizationContext);
				Assert.Same(SynchronizationContext.Current, recipient.SynchronizationContext);
			}

			// the event should have received the expected test data
			Assert.Equal(testData, recipient.Data);

			// check whether the handler is registered
			Assert.True(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName));
			Assert.True(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName, recipient.Handler));

			// unregister event handler
			regCount = WeakEventManager<EventManagerEventArgs>.UnregisterEventHandler(this, EventName, recipient.Handler);
			Assert.Equal(0, regCount);

			// check whether the handler is not registered anymore
			Assert.False(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName));
			Assert.False(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName, recipient.Handler));
		}


		/// <summary>
		/// Tests registering, firing and unregistering an event using a synchronization context.
		/// </summary>
		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public async Task Complete_WithSynchronizationContext(bool scheduleAlways, bool fireOnSameThread)
		{
			var recipient = new EventManagerEventArgsRecipient();

			// register event handler
			await mThread.Factory.Run(
				() =>
				{
					Assert.NotNull(SynchronizationContext.Current);
					int regCount1 = WeakEventManager<EventManagerEventArgs>.RegisterEventHandler(
						this,
						EventName,
						recipient.Handler,
						SynchronizationContext.Current,
						scheduleAlways);
					Assert.Equal(1, regCount1);
				});

			// check whether the handler is registered
			Assert.True(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName));
			Assert.True(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName, recipient.Handler));

			string testData = Guid.NewGuid().ToString("D");
			if (fireOnSameThread)
			{
				if (scheduleAlways)
				{
					// let the thread that registered the event fire the event
					// => handler should be scheduled to run in the same thread (decoupling)
					await mThread.Factory.Run(
						() =>
						{
							Assert.NotNull(SynchronizationContext.Current);
							WeakEventManager<EventManagerEventArgs>.FireEvent(this, EventName, this, new EventManagerEventArgs(testData));
							Assert.False(recipient.HandlerCalledEvent.IsSet, "Handler was invoked directly, should have been scheduled.");
						});

					Assert.True(recipient.HandlerCalledEvent.Wait(1000));
					Assert.Same(mThread.Context.SynchronizationContext, recipient.SynchronizationContext);
					Assert.Equal(testData, recipient.Data);
				}
				else
				{
					// let the thread that registered the event fire the event
					// => handler should be invoked directly in the same thread
					await mThread.Factory.Run(
						() =>
						{
							Assert.NotNull(SynchronizationContext.Current);
							WeakEventManager<EventManagerEventArgs>.FireEvent(this, EventName, this, new EventManagerEventArgs(testData));
							Assert.True(recipient.HandlerCalledEvent.IsSet, "Handler was not invoked directly");
							Assert.Same(SynchronizationContext.Current, recipient.SynchronizationContext);
							Assert.Equal(testData, recipient.Data);
						});
				}
			}
			else
			{
				// let the executing thread fire the event (other thread than the one that registered the handler)
				// => handler should be invoked using the synchronization context of the thread that registered the handler
				WeakEventManager<EventManagerEventArgs>.FireEvent(this, EventName, this, new EventManagerEventArgs(testData));
				Assert.True(recipient.HandlerCalledEvent.Wait(1000));
				Assert.Same(mThread.Context.SynchronizationContext, recipient.SynchronizationContext);
				Assert.Equal(testData, recipient.Data);
			}

			// unregister event handler
			int regCount2 = WeakEventManager<EventManagerEventArgs>.UnregisterEventHandler(this, EventName, recipient.Handler);
			Assert.Equal(0, regCount2);

			// check whether the handler is not registered
			Assert.False(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName));
			Assert.False(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName, recipient.Handler));
		}


		/// <summary>
		/// Tests registering with firing immediately and unregistering an event using a synchronization context.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task Complete_WithSynchronizationContext_FireImmediately(bool scheduleAlways)
		{
			var recipient = new EventManagerEventArgsRecipient();

			// register event handler and let it fire immediately
			string testData = Guid.NewGuid().ToString("D");
			if (scheduleAlways)
			{
				// the handler should be scheduled to run after registering the event
				// (no direct call as part of the registration process)
				await mThread.Factory.Run(
					() =>
					{
						Assert.NotNull(SynchronizationContext.Current);
						int regCount1 = WeakEventManager<EventManagerEventArgs>.RegisterEventHandler(
							this,
							EventName,
							recipient.Handler,
							SynchronizationContext.Current,
							true,
							true,
							this,
							new EventManagerEventArgs(testData));
						Assert.Equal(1, regCount1);
						Assert.False(recipient.HandlerCalledEvent.IsSet, "Handler was invoked directly, should have been scheduled.");
					});

				Assert.True(recipient.HandlerCalledEvent.Wait(1000));
				Assert.Same(mThread.Context.SynchronizationContext, recipient.SynchronizationContext);
				Assert.Equal(testData, recipient.Data);
			}
			else
			{
				// the handler should be called directly as part of the registration process
				await mThread.Factory.Run(
					() =>
					{
						Assert.NotNull(SynchronizationContext.Current);
						int regCount1 = WeakEventManager<EventManagerEventArgs>.RegisterEventHandler(
							this,
							EventName,
							recipient.Handler,
							SynchronizationContext.Current,
							false,
							true,
							this,
							new EventManagerEventArgs(testData));
						Assert.Equal(1, regCount1);
						Assert.True(recipient.HandlerCalledEvent.IsSet, "Handler was not invoked directly");
						Assert.Same(SynchronizationContext.Current, recipient.SynchronizationContext);
						Assert.Equal(testData, recipient.Data);
					});
			}

			// check whether the handler is registered
			Assert.True(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName));
			Assert.True(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName, recipient.Handler));

			// unregister event handler
			int regCount2 = WeakEventManager<EventManagerEventArgs>.UnregisterEventHandler(this, EventName, recipient.Handler);
			Assert.Equal(0, regCount2);

			// check whether the handler is not registered
			Assert.False(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName));
			Assert.False(WeakEventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EventName, recipient.Handler));
		}


		/// <summary>
		/// Tests getting a multicast delegate executing the registered event handlers for a specific event.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void GetEventCallers_WithoutSynchronizationContext(bool scheduleAlways)
		{
			var recipient1 = new EventManagerEventArgsRecipient();
			var recipient2 = new EventManagerEventArgsRecipient();

			// register event handlers
			WeakEventManager<EventManagerEventArgs>.RegisterEventHandler(this, EventName, recipient1.Handler, null, scheduleAlways);
			WeakEventManager<EventManagerEventArgs>.RegisterEventHandler(this, EventName, recipient2.Handler, null, scheduleAlways);

			// get event callers
			EventHandler<EventManagerEventArgs> callers = WeakEventManager<EventManagerEventArgs>.GetEventCallers(this, EventName);
			Assert.NotNull(callers);
			EventHandler<EventManagerEventArgs>[] delegates = callers.GetInvocationList().Cast<EventHandler<EventManagerEventArgs>>().ToArray();
			Assert.Equal(2, delegates.Length);

			// call handlers
			if (scheduleAlways)
			{
				delegates[0](this, new EventManagerEventArgs("Test1"));
				delegates[1](this, new EventManagerEventArgs("Test2"));
				Assert.True(recipient1.HandlerCalledEvent.Wait(1000));
				Assert.True(recipient2.HandlerCalledEvent.Wait(1000));
				Assert.Null(recipient1.SynchronizationContext);
				Assert.Null(recipient2.SynchronizationContext);
				Assert.Equal("Test1", recipient1.Data);
				Assert.Equal("Test2", recipient2.Data);
			}
			else
			{
				// the handlers should be called directly
				delegates[0](this, new EventManagerEventArgs("Test1"));
				Assert.True(recipient1.HandlerCalledEvent.IsSet, "Handler was not invoked directly");
				Assert.Equal("Test1", recipient1.Data);

				delegates[1](this, new EventManagerEventArgs("Test2"));
				Assert.True(recipient2.HandlerCalledEvent.IsSet, "Handler was not invoked directly");
				Assert.Equal("Test2", recipient2.Data);
			}
		}


		/// <summary>
		/// Tests getting a multicast delegate executing the registered event handlers for a specific event.
		/// </summary>
		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[SuppressMessage("ReSharper", "RedundantAssignment")]
		public async Task GetEventCallers_WithSynchronizationContext(bool scheduleAlways, bool fireOnSameThread)
		{
			var recipient1 = new EventManagerEventArgsRecipient();
			var recipient2 = new EventManagerEventArgsRecipient();

			// register handler 1 only, but do not trigger firing immediately
			await mThread.Factory.Run(
				() =>
				{
					Assert.NotNull(SynchronizationContext.Current);

					WeakEventManager<EventManagerEventArgs>.RegisterEventHandler(
						this,
						EventName,
						recipient1.Handler,
						SynchronizationContext.Current,
						scheduleAlways);

					Assert.False(recipient1.HandlerCalledEvent.IsSet, "Event handler was called unexpectedly.");
				});

			// handler 1 should not be called immediately
			Assert.False(recipient1.HandlerCalledEvent.Wait(1000), "Event handler was scheduled to be called unexpectedly.");
			Assert.Null(recipient1.Data);

			if (scheduleAlways)
			{
				// register handler 2 and trigger firing immediately
				await mThread.Factory.Run(
					() =>
					{
						Assert.NotNull(SynchronizationContext.Current);

						WeakEventManager<EventManagerEventArgs>.RegisterEventHandler(
							this,
							EventName,
							recipient2.Handler,
							SynchronizationContext.Current,
							true,
							true,
							this,
							new EventManagerEventArgs("Test2"));

						Assert.False(recipient2.HandlerCalledEvent.IsSet, "Event handler was called immediately, should have been scheduled to be executed...");
					});

				// handler 2 should have been called after some time
				Assert.True(recipient2.HandlerCalledEvent.Wait(1000), "The event was not called asynchronously.");
				Assert.Same(mThread.Context.SynchronizationContext, recipient2.SynchronizationContext);
				Assert.Equal("Test2", recipient2.Data);
			}
			else
			{
				// register handler 2 and trigger firing immediately
				await mThread.Factory.Run(
					() =>
					{
						Assert.NotNull(SynchronizationContext.Current);

						WeakEventManager<EventManagerEventArgs>.RegisterEventHandler(
							this,
							EventName,
							recipient2.Handler,
							SynchronizationContext.Current,
							false,
							true,
							this,
							new EventManagerEventArgs("Test2"));

						Assert.True(recipient2.HandlerCalledEvent.IsSet, "Event handler should have been called immediately.");
					});

				// handler 2 should have been called after some time
				Assert.Same(mThread.Context.SynchronizationContext, recipient2.SynchronizationContext);
				Assert.Equal("Test2", recipient2.Data);
			}

			// get delegates invoking the event handlers
			EventHandler<EventManagerEventArgs> callers = WeakEventManager<EventManagerEventArgs>.GetEventCallers(this, EventName);
			Assert.NotNull(callers);
			EventHandler<EventManagerEventArgs>[] delegates = callers.GetInvocationList().Cast<EventHandler<EventManagerEventArgs>>().ToArray();
			Assert.Equal(2, delegates.Length);

			// reset event handler data
			recipient1.Reset();
			recipient2.Reset();

			// call handlers
			if (fireOnSameThread)
			{
				// call handlers in the context of the thread that registered the event
				if (scheduleAlways)
				{
					// registering thread and firing thread are the same
					// => handler should be scheduled anyway
					await mThread.Factory.Run(
						() =>
						{
							delegates[0](this, new EventManagerEventArgs("Test1"));
							delegates[1](this, new EventManagerEventArgs("Test2"));
							Assert.False(recipient1.HandlerCalledEvent.IsSet, "Event handler was called unexpectedly.");
							Assert.False(recipient2.HandlerCalledEvent.IsSet, "Event handler was called unexpectedly.");
						});

					Assert.True(recipient1.HandlerCalledEvent.Wait(1000), "The event was not called asynchronously.");
					Assert.True(recipient2.HandlerCalledEvent.Wait(1000), "The event was not called asynchronously.");
				}
				else
				{
					// registering thread and firing thread are the same
					// => handler should be called directly
					await mThread.Factory.Run(
						() =>
						{
							delegates[0](this, new EventManagerEventArgs("Test1"));
							delegates[1](this, new EventManagerEventArgs("Test2"));
							Assert.True(recipient1.HandlerCalledEvent.IsSet, "Event handler should have been called directly.");
							Assert.True(recipient2.HandlerCalledEvent.IsSet, "Event handler should have been called directly.");
						});
				}
			}
			else
			{
				// call handlers on the current thread (different from the thread registering the event)
				// => handlers should be called in the context of the thread registering the event
				delegates[0](this, new EventManagerEventArgs("Test1"));
				delegates[1](this, new EventManagerEventArgs("Test2"));
				Assert.True(recipient1.HandlerCalledEvent.Wait(1000), "The event was not called asynchronously.");
				Assert.True(recipient2.HandlerCalledEvent.Wait(1000), "The event was not called asynchronously.");
			}

			// the handlers should have run in the context of the thread that registered them
			Assert.Same(mThread.Context.SynchronizationContext, recipient1.SynchronizationContext);
			Assert.Same(mThread.Context.SynchronizationContext, recipient2.SynchronizationContext);
			Assert.Equal("Test1", recipient1.Data);
			Assert.Equal("Test2", recipient2.Data);
		}


		/// <summary>
		/// Checks whether the event manager detects and cleans up objects that have registered events, but have been garbage collected.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void EnsureEventProvidersAreCollectable(bool scheduleAlways)
		{
			var recipient = new EventManagerEventArgsRecipient();

			// register an event handler to a dummy event provider object
			// (must not be done in the same method to allow the object to be collected in the next step)
			WeakReference weakReferenceProvider = new Func<WeakReference>(
				() =>
				{
					object provider = new();
					int regCount = WeakEventManager<EventManagerEventArgs>.RegisterEventHandler(provider, EventName, recipient.Handler, null, scheduleAlways);
					Assert.Equal(1, regCount);
					return new WeakReference(provider);
				}).Invoke();

			// kick object out of memory
			GC.Collect();

			// the event provider should now be collected
			Assert.False(weakReferenceProvider.IsAlive);
		}


		/// <summary>
		/// Checks whether the event manager detects and cleans up objects that have registered event handlers, but have been garbage collected.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void EnsureEventRecipientsAreCollectable(bool scheduleAlways)
		{
			// create an event object and register its event handler with the event manager
			object provider = new();
			WeakReference recipientWeakReference = new Func<WeakReference>(
				() =>
				{
					var recipient = new EventManagerEventArgsRecipient();
					int regCount = WeakEventManager<EventManagerEventArgs>.RegisterEventHandler(provider, EventName, recipient.Handler, null, scheduleAlways);
					Assert.Equal(1, regCount);
					return new WeakReference(recipient);
				}).Invoke();

			// kick event recipient out of memory
			GC.Collect();

			// the event recipient should now be collected
			Assert.False(recipientWeakReference.IsAlive);
		}
	}

}
