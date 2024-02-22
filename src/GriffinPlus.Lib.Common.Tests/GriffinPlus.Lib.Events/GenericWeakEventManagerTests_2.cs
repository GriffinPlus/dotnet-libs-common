///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GriffinPlus.Lib.Threading;

using Xunit;

namespace GriffinPlus.Lib.Events;

/// <summary>
/// Unit tests targeting the <see cref="GenericWeakEventManager{TArg1,TArg2}"/> class.
/// </summary>
[Collection(nameof(NoParallelizationCollection))]
public class GenericWeakEventManagerTests_2 : IDisposable
{
	private const string EventName = "MyEvent";

	private AsyncContextThread mThread;

	/// <summary>
	/// Initializes an instance the <see cref="GenericWeakEventManagerTests_2"/> class performing common initialization before running a test.
	/// </summary>
	public GenericWeakEventManagerTests_2()
	{
		mThread = new AsyncContextThread();
	}


	/// <summary>
	/// Cleans up.
	/// </summary>
	public void Dispose()
	{
		if (mThread == null) return;
		mThread.Dispose();
		mThread = null;
	}


	/// <summary>
	/// Tests registering, firing and unregistering an event without using a synchronization context.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Complete_WithoutSynchronizationContext(bool scheduleAlways)
	{
		var recipient = new EventManagerEventRecipient();

		const string testData1 = "Arg 1";
		const string testData2 = "Arg 2";

		// register event handler
		int regCount = GenericWeakEventManager<string, string>.RegisterEventHandler(
			this,
			EventName,
			recipient.Handler,
			null,
			scheduleAlways);
		Assert.Equal(1, regCount);

		// check whether the handler is registered
		Assert.True(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName));
		Assert.True(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName, recipient.Handler));

		// fire event
		GenericWeakEventManager<string, string>.FireEvent(
			this,
			EventName,
			testData1,
			testData2);

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
			Assert.True(recipient.HandlerCalledEvent.IsSet, "Handler was not invoked directly");
			Assert.Same(SynchronizationContext.Current, recipient.SynchronizationContext);
		}

		// the event should have received the expected test data
		Assert.Equal(testData1, recipient.Arg1);
		Assert.Equal(testData2, recipient.Arg2);
		Assert.Null(recipient.Arg3);
		Assert.Null(recipient.Arg4);
		Assert.Null(recipient.Arg5);
		Assert.Null(recipient.Arg6);
		Assert.Null(recipient.Arg7);
		Assert.Null(recipient.Arg8);

		// unregister event handler
		regCount = GenericWeakEventManager<string, string>.UnregisterEventHandler(this, EventName, recipient.Handler);
		Assert.Equal(0, regCount);

		// check whether the handler is not registered anymore
		Assert.False(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName));
		Assert.False(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName, recipient.Handler));
	}


	/// <summary>
	/// Tests registering with firing immediately and unregistering an event without using a synchronization context.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Complete_WithoutSynchronizationContext_FireImmediately(bool scheduleAlways)
	{
		var recipient = new EventManagerEventRecipient();

		const string testData1 = "Arg 1";
		const string testData2 = "Arg 2";

		int regCount;
		if (scheduleAlways)
		{
			// register event handler and fire it immediately
			regCount = GenericWeakEventManager<string, string>.RegisterEventHandler(
				this,
				EventName,
				recipient.Handler,
				null,
				true,
				true,
				testData1,
				testData2);
			Assert.Equal(1, regCount);

			// handler is called asynchronously
			// => wait for the handler to be called and continue
			Assert.True(recipient.HandlerCalledEvent.Wait(1000));
			Assert.Null(recipient.SynchronizationContext); // synchronization context should be null for thread pool threads
		}
		else
		{
			// register event handler and fire it immediately
			regCount = GenericWeakEventManager<string, string>.RegisterEventHandler(
				this,
				EventName,
				recipient.Handler,
				null,
				false,
				true,
				testData1,
				testData2);
			Assert.Equal(1, regCount);

			// handler is called synchronously
			Assert.True(recipient.HandlerCalledEvent.IsSet, "Handler was not invoked directly");
			Assert.Same(SynchronizationContext.Current, recipient.SynchronizationContext);
		}

		// the event should have received the expected test data
		Assert.Equal(testData1, recipient.Arg1);
		Assert.Equal(testData2, recipient.Arg2);
		Assert.Null(recipient.Arg3);
		Assert.Null(recipient.Arg4);
		Assert.Null(recipient.Arg5);
		Assert.Null(recipient.Arg6);
		Assert.Null(recipient.Arg7);
		Assert.Null(recipient.Arg8);

		// check whether the handler is registered
		Assert.True(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName));
		Assert.True(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName, recipient.Handler));

		// unregister event handler
		regCount = GenericWeakEventManager<string, string>.UnregisterEventHandler(this, EventName, recipient.Handler);
		Assert.Equal(0, regCount);

		// check whether the handler is not registered anymore
		Assert.False(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName));
		Assert.False(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName, recipient.Handler));
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
		var recipient = new EventManagerEventRecipient();

		const string testData1 = "Arg 1";
		const string testData2 = "Arg 2";

		// register event handler
		await mThread.Factory.Run(
			() =>
			{
				Assert.NotNull(SynchronizationContext.Current);
				int regCount1 = GenericWeakEventManager<string, string>.RegisterEventHandler(
					this,
					EventName,
					recipient.Handler,
					SynchronizationContext.Current,
					scheduleAlways);
				Assert.Equal(1, regCount1);
			});

		// check whether the handler is registered
		Assert.True(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName));
		Assert.True(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName, recipient.Handler));

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

						GenericWeakEventManager<string, string>.FireEvent(
							this,
							EventName,
							testData1,
							testData2);

						Assert.False(recipient.HandlerCalledEvent.IsSet, "Handler was invoked directly, should have been scheduled.");
					});

				Assert.True(recipient.HandlerCalledEvent.Wait(1000));
				Assert.Same(mThread.Context.SynchronizationContext, recipient.SynchronizationContext);
				Assert.Equal(testData1, recipient.Arg1);
				Assert.Equal(testData2, recipient.Arg2);
				Assert.Null(recipient.Arg3);
				Assert.Null(recipient.Arg4);
				Assert.Null(recipient.Arg5);
				Assert.Null(recipient.Arg6);
				Assert.Null(recipient.Arg7);
				Assert.Null(recipient.Arg8);
			}
			else
			{
				// let the thread that registered the event fire the event
				// => handler should be invoked directly in the same thread
				await mThread.Factory.Run(
					() =>
					{
						Assert.NotNull(SynchronizationContext.Current);

						GenericWeakEventManager<string, string>.FireEvent(
							this,
							EventName,
							testData1,
							testData2);

						Assert.True(recipient.HandlerCalledEvent.IsSet, "Handler was not invoked directly");
						Assert.Same(SynchronizationContext.Current, recipient.SynchronizationContext);
						Assert.Equal(testData1, recipient.Arg1);
						Assert.Equal(testData2, recipient.Arg2);
						Assert.Null(recipient.Arg3);
						Assert.Null(recipient.Arg4);
						Assert.Null(recipient.Arg5);
						Assert.Null(recipient.Arg6);
						Assert.Null(recipient.Arg7);
						Assert.Null(recipient.Arg8);
					});
			}
		}
		else
		{
			// let the executing thread fire the event (other thread than the one that registered the handler)
			// => handler should be invoked using the synchronization context of the thread that registered the handler
			GenericWeakEventManager<string, string>.FireEvent(
				this,
				EventName,
				testData1,
				testData2);

			Assert.True(recipient.HandlerCalledEvent.Wait(1000));
			Assert.Same(mThread.Context.SynchronizationContext, recipient.SynchronizationContext);
			Assert.Equal(testData1, recipient.Arg1);
			Assert.Equal(testData2, recipient.Arg2);
			Assert.Null(recipient.Arg3);
			Assert.Null(recipient.Arg4);
			Assert.Null(recipient.Arg5);
			Assert.Null(recipient.Arg6);
			Assert.Null(recipient.Arg7);
			Assert.Null(recipient.Arg8);
		}

		// unregister event handler
		int regCount2 = GenericWeakEventManager<string, string>.UnregisterEventHandler(this, EventName, recipient.Handler);
		Assert.Equal(0, regCount2);

		// check whether the handler is not registered
		Assert.False(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName));
		Assert.False(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName, recipient.Handler));
	}


	/// <summary>
	/// Tests registering with firing immediately and unregistering an event using a synchronization context.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task Complete_WithSynchronizationContext_FireImmediately(bool scheduleAlways)
	{
		var recipient = new EventManagerEventRecipient();

		const string testData1 = "Arg 1";
		const string testData2 = "Arg 2";

		// register event handler and let it fire immediately
		if (scheduleAlways)
		{
			// the handler should be scheduled to run after registering the event
			// (no direct call as part of the registration process)
			await mThread.Factory.Run(
				() =>
				{
					Assert.NotNull(SynchronizationContext.Current);
					int regCount1 = GenericWeakEventManager<string, string>.RegisterEventHandler(
						this,
						EventName,
						recipient.Handler,
						SynchronizationContext.Current,
						true,
						true,
						testData1,
						testData2);
					Assert.Equal(1, regCount1);
					Assert.False(recipient.HandlerCalledEvent.IsSet, "Handler was invoked directly, should have been scheduled.");
				});

			Assert.True(recipient.HandlerCalledEvent.Wait(1000));
			Assert.Same(mThread.Context.SynchronizationContext, recipient.SynchronizationContext);
			Assert.Equal(testData1, recipient.Arg1);
			Assert.Equal(testData2, recipient.Arg2);
			Assert.Null(recipient.Arg3);
			Assert.Null(recipient.Arg4);
			Assert.Null(recipient.Arg5);
			Assert.Null(recipient.Arg6);
			Assert.Null(recipient.Arg7);
			Assert.Null(recipient.Arg8);
		}
		else
		{
			// the handler should be called directly as part of the registration process
			await mThread.Factory.Run(
				() =>
				{
					Assert.NotNull(SynchronizationContext.Current);
					int regCount1 = GenericWeakEventManager<string, string>.RegisterEventHandler(
						this,
						EventName,
						recipient.Handler,
						SynchronizationContext.Current,
						false,
						true,
						testData1,
						testData2);
					Assert.Equal(1, regCount1);
					Assert.True(recipient.HandlerCalledEvent.IsSet, "Handler was not invoked directly");
					Assert.Same(SynchronizationContext.Current, recipient.SynchronizationContext);
					Assert.Equal(testData1, recipient.Arg1);
					Assert.Equal(testData2, recipient.Arg2);
					Assert.Null(recipient.Arg3);
					Assert.Null(recipient.Arg4);
					Assert.Null(recipient.Arg5);
					Assert.Null(recipient.Arg6);
					Assert.Null(recipient.Arg7);
					Assert.Null(recipient.Arg8);
				});
		}

		// check whether the handler is registered
		Assert.True(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName));
		Assert.True(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName, recipient.Handler));

		// unregister event handler
		int regCount2 = GenericWeakEventManager<string, string>.UnregisterEventHandler(this, EventName, recipient.Handler);
		Assert.Equal(0, regCount2);

		// check whether the handler is not registered
		Assert.False(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName));
		Assert.False(GenericWeakEventManager<string, string>.IsHandlerRegistered(this, EventName, recipient.Handler));
	}


	/// <summary>
	/// Tests getting a multicast delegate executing the registered event handlers for a specific event.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void GetEventCallers_WithoutSynchronizationContext(bool scheduleAlways)
	{
		var recipient1 = new EventManagerEventRecipient();
		var recipient2 = new EventManagerEventRecipient();

		const string testData11 = "Handler 1, Arg 1";
		const string testData12 = "Handler 1, Arg 2";
		const string testData21 = "Handler 2, Arg 1";
		const string testData22 = "Handler 2, Arg 2";

		// register event handlers
		GenericWeakEventManager<string, string>.RegisterEventHandler(this, EventName, recipient1.Handler, null, scheduleAlways);
		GenericWeakEventManager<string, string>.RegisterEventHandler(this, EventName, recipient2.Handler, null, scheduleAlways);

		// get event callers
		Action<string, string> callers = GenericWeakEventManager<string, string>.GetEventCallers(this, EventName);
		Assert.NotNull(callers);
		Action<string, string>[] delegates = callers.GetInvocationList().Cast<Action<string, string>>().ToArray();
		Assert.Equal(2, delegates.Length);

		// call handlers
		if (scheduleAlways)
		{
			delegates[0](testData11, testData12);
			delegates[1](testData21, testData22);

			Assert.True(recipient1.HandlerCalledEvent.Wait(1000));
			Assert.True(recipient2.HandlerCalledEvent.Wait(1000));
			Assert.Null(recipient1.SynchronizationContext);
			Assert.Null(recipient2.SynchronizationContext);

			Assert.Equal(testData11, recipient1.Arg1);
			Assert.Equal(testData12, recipient1.Arg2);
			Assert.Null(recipient1.Arg3);
			Assert.Null(recipient1.Arg4);
			Assert.Null(recipient1.Arg5);
			Assert.Null(recipient1.Arg6);
			Assert.Null(recipient1.Arg7);
			Assert.Null(recipient1.Arg8);

			Assert.Equal(testData21, recipient2.Arg1);
			Assert.Equal(testData22, recipient2.Arg2);
			Assert.Null(recipient2.Arg3);
			Assert.Null(recipient2.Arg4);
			Assert.Null(recipient2.Arg5);
			Assert.Null(recipient2.Arg6);
			Assert.Null(recipient2.Arg7);
			Assert.Null(recipient2.Arg8);
		}
		else
		{
			// the handlers should be called directly
			delegates[0](testData11, testData12);
			Assert.True(recipient1.HandlerCalledEvent.IsSet, "Handler was not invoked directly");

			Assert.Equal(testData11, recipient1.Arg1);
			Assert.Equal(testData12, recipient1.Arg2);
			Assert.Null(recipient1.Arg3);
			Assert.Null(recipient1.Arg4);
			Assert.Null(recipient1.Arg5);
			Assert.Null(recipient1.Arg6);
			Assert.Null(recipient1.Arg7);
			Assert.Null(recipient1.Arg8);

			delegates[1](testData21, testData22);
			Assert.True(recipient2.HandlerCalledEvent.IsSet, "Handler was not invoked directly");
			Assert.Equal(testData21, recipient2.Arg1);
			Assert.Equal(testData22, recipient2.Arg2);
			Assert.Null(recipient2.Arg3);
			Assert.Null(recipient2.Arg4);
			Assert.Null(recipient2.Arg5);
			Assert.Null(recipient2.Arg6);
			Assert.Null(recipient2.Arg7);
			Assert.Null(recipient2.Arg8);
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
	public async Task GetEventCallers_WithSynchronizationContext(bool scheduleAlways, bool fireOnSameThread)
	{
		var recipient1 = new EventManagerEventRecipient();
		var recipient2 = new EventManagerEventRecipient();

		const string testData11 = "Handler 1, Arg 1";
		const string testData12 = "Handler 1, Arg 2";
		const string testData21 = "Handler 2, Arg 1";
		const string testData22 = "Handler 2, Arg 2";

		// register handler 1 only, but do not trigger firing immediately
		await mThread.Factory.Run(
			() =>
			{
				Assert.NotNull(SynchronizationContext.Current);

				GenericWeakEventManager<string, string>.RegisterEventHandler(
					this,
					EventName,
					recipient1.Handler,
					SynchronizationContext.Current,
					scheduleAlways);

				Assert.False(recipient1.HandlerCalledEvent.IsSet, "Event handler was called unexpectedly.");
			});

		// handler 1 should not be called immediately
		Assert.False(recipient1.HandlerCalledEvent.Wait(1000), "Event handler was scheduled to be called unexpectedly.");
		Assert.Null(recipient1.Arg1);
		Assert.Null(recipient1.Arg2);
		Assert.Null(recipient1.Arg3);
		Assert.Null(recipient1.Arg4);
		Assert.Null(recipient1.Arg5);
		Assert.Null(recipient1.Arg6);
		Assert.Null(recipient1.Arg7);
		Assert.Null(recipient1.Arg8);

		if (scheduleAlways)
		{
			// register handler 2 and trigger firing immediately
			await mThread.Factory.Run(
				() =>
				{
					Assert.NotNull(SynchronizationContext.Current);

					GenericWeakEventManager<string, string>.RegisterEventHandler(
						this,
						EventName,
						recipient2.Handler,
						SynchronizationContext.Current,
						true,
						true,
						testData21,
						testData22);

					Assert.False(recipient2.HandlerCalledEvent.IsSet, "Event handler was called immediately, should have been scheduled to be executed...");
				});

			// handler 2 should have been called after some time
			Assert.True(recipient2.HandlerCalledEvent.Wait(1000), "The event was not called asynchronously.");
			Assert.Same(mThread.Context.SynchronizationContext, recipient2.SynchronizationContext);
			Assert.Equal(testData21, recipient2.Arg1);
			Assert.Equal(testData22, recipient2.Arg2);
			Assert.Null(recipient2.Arg3);
			Assert.Null(recipient2.Arg4);
			Assert.Null(recipient2.Arg5);
			Assert.Null(recipient2.Arg6);
			Assert.Null(recipient2.Arg7);
			Assert.Null(recipient2.Arg8);
		}
		else
		{
			// register handler 2 and trigger firing immediately
			await mThread.Factory.Run(
				() =>
				{
					Assert.NotNull(SynchronizationContext.Current);

					GenericWeakEventManager<string, string>.RegisterEventHandler(
						this,
						EventName,
						recipient2.Handler,
						SynchronizationContext.Current,
						false,
						true,
						testData21,
						testData22);

					Assert.True(recipient2.HandlerCalledEvent.IsSet, "Event handler should have been called immediately.");
				});

			// handler 2 should have been called after some time
			Assert.Same(mThread.Context.SynchronizationContext, recipient2.SynchronizationContext);
			Assert.Equal(testData21, recipient2.Arg1);
			Assert.Equal(testData22, recipient2.Arg2);
			Assert.Null(recipient2.Arg3);
			Assert.Null(recipient2.Arg4);
			Assert.Null(recipient2.Arg5);
			Assert.Null(recipient2.Arg6);
			Assert.Null(recipient2.Arg7);
			Assert.Null(recipient2.Arg8);
		}

		// get delegates invoking the event handlers
		Action<string, string> callers = GenericWeakEventManager<string, string>.GetEventCallers(this, EventName);
		Assert.NotNull(callers);
		Action<string, string>[] delegates = callers.GetInvocationList().Cast<Action<string, string>>().ToArray();
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
						delegates[0](testData11, testData12);
						delegates[1](testData21, testData22);
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
						delegates[0](testData11, testData12);
						delegates[1](testData21, testData22);
						Assert.True(recipient1.HandlerCalledEvent.IsSet, "Event handler should have been called directly.");
						Assert.True(recipient2.HandlerCalledEvent.IsSet, "Event handler should have been called directly.");
					});
			}
		}
		else
		{
			// call handlers on the current thread (different from the thread registering the event)
			// => handlers should be called in the context of the thread registering the event
			delegates[0](testData11, testData12);
			delegates[1](testData21, testData22);
			Assert.True(recipient1.HandlerCalledEvent.Wait(1000), "The event was not called asynchronously.");
			Assert.True(recipient2.HandlerCalledEvent.Wait(1000), "The event was not called asynchronously.");
		}

		// the handlers should have run in the context of the thread that registered them
		Assert.Same(mThread.Context.SynchronizationContext, recipient1.SynchronizationContext);
		Assert.Same(mThread.Context.SynchronizationContext, recipient2.SynchronizationContext);

		Assert.Equal(testData11, recipient1.Arg1);
		Assert.Equal(testData12, recipient1.Arg2);
		Assert.Null(recipient1.Arg3);
		Assert.Null(recipient1.Arg4);
		Assert.Null(recipient1.Arg5);
		Assert.Null(recipient1.Arg6);
		Assert.Null(recipient1.Arg7);
		Assert.Null(recipient1.Arg8);

		Assert.Equal(testData21, recipient2.Arg1);
		Assert.Equal(testData22, recipient2.Arg2);
		Assert.Null(recipient2.Arg3);
		Assert.Null(recipient2.Arg4);
		Assert.Null(recipient2.Arg5);
		Assert.Null(recipient2.Arg6);
		Assert.Null(recipient2.Arg7);
		Assert.Null(recipient2.Arg8);
	}


	/// <summary>
	/// Checks whether the event manager detects and cleans up objects that have registered events, but have been garbage collected.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void EnsureEventProvidersAreCollectable(bool scheduleAlways)
	{
		var recipient = new EventManagerEventRecipient();

		// register an event handler to a dummy event provider object
		// (must not be done in the same method to allow the object to be collected in the next step)
		WeakReference weakReferenceProvider = new Func<WeakReference>(
			() =>
			{
				object provider = new();

				int regCount = GenericWeakEventManager<string, string>.RegisterEventHandler(
					provider,
					EventName,
					recipient.Handler,
					null,
					scheduleAlways);

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
				var recipient = new EventManagerEventRecipient();
				int regCount = GenericWeakEventManager<string, string>.RegisterEventHandler(provider, EventName, recipient.Handler, null, scheduleAlways);
				Assert.Equal(1, regCount);
				return new WeakReference(recipient);
			}).Invoke();

		// kick event recipient out of memory
		GC.Collect();

		// the event recipient should now be collected
		Assert.False(recipientWeakReference.IsAlive);
	}
}
