///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GriffinPlus.Lib.Threading;

using Xunit;

namespace GriffinPlus.Lib.Events;

/// <summary>
/// Unit tests targeting the <see cref="PropertyChangedEventManager"/> class.
/// </summary>
[Collection(nameof(NoParallelizationCollection))]
public class PropertyChangedEventManagerTests : IDisposable
{
	private const string PropertyName = "MyProperty";

	private AsyncContextThread mThread;

	/// <summary>
	/// Initializes an instance the <see cref="PropertyChangedEventManagerTests"/> class performing common initialization before running a test.
	/// </summary>
	public PropertyChangedEventManagerTests()
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
		var recipient = new PropertyChangedEventRecipient();

		// register event handler
		int regCount = PropertyChangedEventManager.RegisterEventHandler(this, recipient.Handler, null, scheduleAlways);
		Assert.Equal(1, regCount);

		// check whether the handler is registered
		Assert.True(PropertyChangedEventManager.IsHandlerRegistered(this));

		// fire event
		PropertyChangedEventManager.FireEvent(this, PropertyName);

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

		// the event should have received the property name
		Assert.Equal(PropertyName, recipient.ChangedPropertyName);

		// unregister event handler
		regCount = PropertyChangedEventManager.UnregisterEventHandler(this, recipient.Handler);
		Assert.Equal(0, regCount);

		// check whether the handler is not registered
		Assert.False(PropertyChangedEventManager.IsHandlerRegistered(this));
	}

	/// <summary>
	/// Tests registering with firing immediately and unregistering an event without using a synchronization context.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Complete_WithoutSynchronizationContext_FireImmediately(bool scheduleAlways)
	{
		var recipient = new PropertyChangedEventRecipient();

		int regCount;
		if (scheduleAlways)
		{
			// register event handler and fire it immediately
			regCount = PropertyChangedEventManager.RegisterEventHandler(
				this,
				recipient.Handler,
				null,
				true,
				true,
				this,
				PropertyName);
			Assert.Equal(1, regCount);

			// handler is called asynchronously
			// => wait for the handler to be called and continue
			Assert.True(recipient.HandlerCalledEvent.Wait(1000));
			Assert.Null(recipient.SynchronizationContext); // synchronization context should be null for thread pool threads
		}
		else
		{
			// register event handler and fire it immediately
			regCount = PropertyChangedEventManager.RegisterEventHandler(
				this,
				recipient.Handler,
				null,
				false,
				true,
				this,
				PropertyName);
			Assert.Equal(1, regCount);

			// handler is called synchronously
			Assert.True(recipient.HandlerCalledEvent.IsSet, "Handler was not invoked directly");
			Assert.Same(SynchronizationContext.Current, recipient.SynchronizationContext);
		}

		// the event should have received the property name
		Assert.Equal(PropertyName, recipient.ChangedPropertyName);

		// check whether the handler is registered
		Assert.True(PropertyChangedEventManager.IsHandlerRegistered(this));

		// unregister event handler
		regCount = PropertyChangedEventManager.UnregisterEventHandler(this, recipient.Handler);
		Assert.Equal(0, regCount);

		// check whether the handler is not registered anymore
		Assert.False(PropertyChangedEventManager.IsHandlerRegistered(this));
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
		var recipient = new PropertyChangedEventRecipient();

		// register event handler
		await mThread.Factory.Run(
			() =>
			{
				Assert.NotNull(SynchronizationContext.Current);
				int regCount1 = PropertyChangedEventManager.RegisterEventHandler(
					this,
					recipient.Handler,
					SynchronizationContext.Current,
					scheduleAlways);
				Assert.Equal(1, regCount1);
			});

		// check whether the handler is registered
		Assert.True(PropertyChangedEventManager.IsHandlerRegistered(this));

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
						PropertyChangedEventManager.FireEvent(this, PropertyName);
						Assert.False(recipient.HandlerCalledEvent.IsSet, "Handler was invoked directly, should have been scheduled.");
					});

				Assert.True(recipient.HandlerCalledEvent.Wait(1000));
				Assert.Same(mThread.Context.SynchronizationContext, recipient.SynchronizationContext);
				Assert.Equal(PropertyName, recipient.ChangedPropertyName);
			}
			else
			{
				// let the thread that registered the event fire the event
				// => handler should be invoked directly in the same thread
				await mThread.Factory.Run(
					() =>
					{
						Assert.NotNull(SynchronizationContext.Current);
						PropertyChangedEventManager.FireEvent(this, PropertyName);
						Assert.True(recipient.HandlerCalledEvent.IsSet, "Handler was not invoked directly");
						Assert.Same(SynchronizationContext.Current, recipient.SynchronizationContext);
						Assert.Equal(PropertyName, recipient.ChangedPropertyName);
					});
			}
		}
		else
		{
			// let the executing thread fire the event (other thread than the one that registered the handler)
			// => handler should be invoked using the synchronization context of the thread that registered the handler
			PropertyChangedEventManager.FireEvent(this, PropertyName);
			Assert.True(recipient.HandlerCalledEvent.Wait(1000));
			Assert.Same(mThread.Context.SynchronizationContext, recipient.SynchronizationContext);
			Assert.Equal(PropertyName, recipient.ChangedPropertyName);
		}

		// unregister event handler
		int regCount2 = PropertyChangedEventManager.UnregisterEventHandler(this, recipient.Handler);
		Assert.Equal(0, regCount2);

		// check whether the handler is not registered anymore
		Assert.False(PropertyChangedEventManager.IsHandlerRegistered(this));
	}

	/// <summary>
	/// Tests registering with firing immediately and unregistering an event using a synchronization context.
	/// The event handler is called in the context of another thread.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task Complete_WithSynchronizationContext_FireImmediately(bool scheduleAlways)
	{
		var recipient = new PropertyChangedEventRecipient();

		if (scheduleAlways)
		{
			// the handler should be scheduled to run after registering the event
			// (no direct call as part of the registration process)
			await mThread.Factory.Run(
				() =>
				{
					Assert.NotNull(SynchronizationContext.Current);
					int regCount1 = PropertyChangedEventManager.RegisterEventHandler(
						this,
						recipient.Handler,
						SynchronizationContext.Current,
						true,
						true,
						this,
						PropertyName);
					Assert.Equal(1, regCount1);
					Assert.False(recipient.HandlerCalledEvent.IsSet, "Handler was invoked directly, should have been scheduled.");
				});

			Assert.True(recipient.HandlerCalledEvent.Wait(1000));
			Assert.Same(mThread.Context.SynchronizationContext, recipient.SynchronizationContext);
			Assert.Equal(PropertyName, recipient.ChangedPropertyName);
		}
		else
		{
			// the handler should be called directly as part of the registration process
			await mThread.Factory.Run(
				() =>
				{
					Assert.NotNull(SynchronizationContext.Current);
					int regCount1 = PropertyChangedEventManager.RegisterEventHandler(
						this,
						recipient.Handler,
						SynchronizationContext.Current,
						false,
						true,
						this,
						PropertyName);
					Assert.Equal(1, regCount1);
					Assert.True(recipient.HandlerCalledEvent.IsSet, "Handler was not invoked directly");
					Assert.Same(SynchronizationContext.Current, recipient.SynchronizationContext);
					Assert.Equal(PropertyName, recipient.ChangedPropertyName);
				});
		}

		// check whether the handler is registered
		Assert.True(PropertyChangedEventManager.IsHandlerRegistered(this));

		// unregister event handler
		int regCount2 = PropertyChangedEventManager.UnregisterEventHandler(this, recipient.Handler);
		Assert.Equal(0, regCount2);

		// check whether the handler is not registered anymore
		Assert.False(PropertyChangedEventManager.IsHandlerRegistered(this));
	}

	/// <summary>
	/// Tests getting a multicast delegate executing the registered event handlers for a specific event.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void GetEventCallers_WithoutSynchronizationContext(bool scheduleAlways)
	{
		var recipient1 = new PropertyChangedEventRecipient();
		var recipient2 = new PropertyChangedEventRecipient();

		// register event handlers
		PropertyChangedEventManager.RegisterEventHandler(this, recipient1.Handler, null, scheduleAlways);
		PropertyChangedEventManager.RegisterEventHandler(this, recipient2.Handler, null, scheduleAlways);

		// get event callers
		PropertyChangedEventHandler callers = PropertyChangedEventManager.GetEventCallers(this);
		Assert.NotNull(callers);
		PropertyChangedEventHandler[] delegates = callers.GetInvocationList().Cast<PropertyChangedEventHandler>().ToArray();
		Assert.Equal(2, delegates.Length);

		// call handlers
		if (scheduleAlways)
		{
			delegates[0](this, new PropertyChangedEventArgs("Test1"));
			delegates[1](this, new PropertyChangedEventArgs("Test2"));
			Assert.True(recipient1.HandlerCalledEvent.Wait(1000));
			Assert.True(recipient2.HandlerCalledEvent.Wait(1000));
			Assert.Null(recipient1.SynchronizationContext);
			Assert.Null(recipient2.SynchronizationContext);
			Assert.Equal("Test1", recipient1.ChangedPropertyName);
			Assert.Equal("Test2", recipient2.ChangedPropertyName);
		}
		else
		{
			// the handlers should be called directly
			delegates[0](this, new PropertyChangedEventArgs("Test1"));
			Assert.True(recipient1.HandlerCalledEvent.IsSet, "Handler was not invoked directly");
			Assert.Equal("Test1", recipient1.ChangedPropertyName);

			delegates[1](this, new PropertyChangedEventArgs("Test2"));
			Assert.True(recipient2.HandlerCalledEvent.IsSet, "Handler was not invoked directly");
			Assert.Equal("Test2", recipient2.ChangedPropertyName);
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
		var recipient1 = new PropertyChangedEventRecipient();
		var recipient2 = new PropertyChangedEventRecipient();

		// register handler 1 only, but do not trigger firing immediately
		await mThread.Factory.Run(
			() =>
			{
				Assert.NotNull(SynchronizationContext.Current);

				PropertyChangedEventManager.RegisterEventHandler(
					this,
					recipient1.Handler,
					SynchronizationContext.Current,
					scheduleAlways);

				Assert.False(recipient1.HandlerCalledEvent.IsSet, "Event handler was called unexpectedly.");
			});

		// handler 1 should not be called immediately
		Assert.False(recipient1.HandlerCalledEvent.Wait(1000), "Event handler was scheduled to be called unexpectedly.");
		Assert.Null(recipient1.ChangedPropertyName);

		if (scheduleAlways)
		{
			// register handler 2 and trigger firing immediately
			await mThread.Factory.Run(
				() =>
				{
					Assert.NotNull(SynchronizationContext.Current);

					PropertyChangedEventManager.RegisterEventHandler(
						this,
						recipient2.Handler,
						SynchronizationContext.Current,
						true,
						true,
						this,
						"Test2");

					Assert.False(recipient2.HandlerCalledEvent.IsSet, "Event handler was called immediately, should have been scheduled to be executed...");
				});

			// handler 2 should have been called after some time
			Assert.True(recipient2.HandlerCalledEvent.Wait(1000), "The event was not called asynchronously.");
			Assert.Same(mThread.Context.SynchronizationContext, recipient2.SynchronizationContext);
			Assert.Equal("Test2", recipient2.ChangedPropertyName);
		}
		else
		{
			// register handler 2 and trigger firing immediately
			await mThread.Factory.Run(
				() =>
				{
					Assert.NotNull(SynchronizationContext.Current);

					PropertyChangedEventManager.RegisterEventHandler(
						this,
						recipient2.Handler,
						SynchronizationContext.Current,
						false,
						true,
						this,
						"Test2");

					Assert.True(recipient2.HandlerCalledEvent.IsSet, "Event handler should have been called immediately.");
				});

			// handler 2 should have been called after some time
			Assert.Same(mThread.Context.SynchronizationContext, recipient2.SynchronizationContext);
			Assert.Equal("Test2", recipient2.ChangedPropertyName);
		}

		// get delegates invoking the event handlers
		PropertyChangedEventHandler callers = PropertyChangedEventManager.GetEventCallers(this);
		Assert.NotNull(callers);
		PropertyChangedEventHandler[] delegates = callers.GetInvocationList().Cast<PropertyChangedEventHandler>().ToArray();
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
						delegates[0](this, new PropertyChangedEventArgs("Test1"));
						delegates[1](this, new PropertyChangedEventArgs("Test2"));
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
						delegates[0](this, new PropertyChangedEventArgs("Test1"));
						delegates[1](this, new PropertyChangedEventArgs("Test2"));
						Assert.True(recipient1.HandlerCalledEvent.IsSet, "Event handler should have been called directly.");
						Assert.True(recipient2.HandlerCalledEvent.IsSet, "Event handler should have been called directly.");
					});
			}
		}
		else
		{
			// call handlers on the current thread (different from the thread registering the event)
			// => handlers should be called in the context of the thread registering the event
			delegates[0](this, new PropertyChangedEventArgs("Test1"));
			delegates[1](this, new PropertyChangedEventArgs("Test2"));
			Assert.True(recipient1.HandlerCalledEvent.Wait(1000), "The event was not called asynchronously.");
			Assert.True(recipient2.HandlerCalledEvent.Wait(1000), "The event was not called asynchronously.");
		}

		// the handlers should have run in the context of the thread that registered them
		Assert.Same(mThread.Context.SynchronizationContext, recipient1.SynchronizationContext);
		Assert.Same(mThread.Context.SynchronizationContext, recipient2.SynchronizationContext);
		Assert.Equal("Test1", recipient1.ChangedPropertyName);
		Assert.Equal("Test2", recipient2.ChangedPropertyName);
	}

	/// <summary>
	/// Checks whether the event manager detects and cleans up objects that have registered events, but have been garbage collected.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void EnsureEventProvidersAreCollectable(bool scheduleAlways)
	{
		var recipient = new PropertyChangedEventRecipient();

		// register an event handler to a dummy event provider object
		// (must not be done in the same method to allow the object to be collected in the next step)
		WeakReference weakReferenceProvider = new Func<WeakReference>(
			() =>
			{
				object provider = new();
				int regCount = PropertyChangedEventManager.RegisterEventHandler(provider, recipient.Handler, null, scheduleAlways);
				Assert.Equal(1, regCount);
				return new WeakReference(provider);
			}).Invoke();

		// kick object out of memory
		GC.Collect();

		// the event provider should now be collected
		Assert.False(weakReferenceProvider.IsAlive);
	}
}
