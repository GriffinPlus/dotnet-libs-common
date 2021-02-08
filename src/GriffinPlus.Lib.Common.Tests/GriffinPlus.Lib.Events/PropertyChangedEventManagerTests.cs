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

namespace GriffinPlus.Lib.Events
{

	/// <summary>
	/// Unit tests targeting the <see cref="PropertyChangedEventManager"/> class.
	/// </summary>
	public class PropertyChangedEventManagerTests : IDisposable
	{
		private const string PropertyName = "MyProperty";

		private AsyncContextThread mThread;

		public class TestEventRecipient
		{
			public string MyString { get; set; }

			public void EH_MyEvent(object sender, EventManagerEventArgs e)
			{
				MyString = e.MyString;
			}
		}

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
			SynchronizationContext handlerThreadSynchronizationContext = null;
			string changedPropertyName = null;
			var handlerCalledEvent = new ManualResetEventSlim(false);

			// the event handler
			void Handler(object sender, PropertyChangedEventArgs e)
			{
				handlerThreadSynchronizationContext = SynchronizationContext.Current;
				changedPropertyName = e.PropertyName;
				handlerCalledEvent.Set();
			}

			// register event handler
			int regCount = PropertyChangedEventManager.RegisterEventHandler(this, Handler, null, scheduleAlways);
			Assert.Equal(1, regCount);

			// check whether the handler is registered
			Assert.True(PropertyChangedEventManager.IsHandlerRegistered(this));

			// fire event
			PropertyChangedEventManager.FireEvent(this, PropertyName);

			if (scheduleAlways)
			{
				// handler is called asynchronously
				// => wait for the handler to be called and continue
				Assert.True(handlerCalledEvent.Wait(60000));
				Assert.Null(handlerThreadSynchronizationContext); // synchronization context should be null for thread pool threads
			}
			else
			{
				// handler is called synchronously
				Assert.NotNull(handlerThreadSynchronizationContext);
				Assert.Same(SynchronizationContext.Current, handlerThreadSynchronizationContext);
			}

			// the event should have received the property name
			Assert.Equal(PropertyName, changedPropertyName);

			// unregister event handler
			regCount = PropertyChangedEventManager.UnregisterEventHandler(this, Handler);
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
			SynchronizationContext handlerThreadSynchronizationContext = null;
			string changedPropertyName = null;
			var handlerCalledEvent = new ManualResetEventSlim(false);

			// the event handler
			void Handler(object sender, PropertyChangedEventArgs e)
			{
				handlerThreadSynchronizationContext = SynchronizationContext.Current;
				changedPropertyName = e.PropertyName;
				handlerCalledEvent.Set();
			}

			int regCount;
			if (scheduleAlways)
			{
				// register event handler and fire it immediately
				regCount = PropertyChangedEventManager.RegisterEventHandler(
					this,
					Handler,
					null,
					true,
					true,
					this,
					PropertyName);
				Assert.Equal(1, regCount);

				// handler is called asynchronously
				// => wait for the handler to be called and continue
				Assert.True(handlerCalledEvent.Wait(60000));
				Assert.Null(handlerThreadSynchronizationContext); // synchronization context should be null for thread pool threads
			}
			else
			{
				// register event handler and fire it immediately
				regCount = PropertyChangedEventManager.RegisterEventHandler(
					this,
					Handler,
					null,
					false,
					true,
					this,
					PropertyName);
				Assert.Equal(1, regCount);

				// handler is called synchronously
				Assert.NotNull(handlerThreadSynchronizationContext);
				Assert.Same(SynchronizationContext.Current, handlerThreadSynchronizationContext);
			}

			// the event should have received the property name
			Assert.Equal(PropertyName, changedPropertyName);

			// check whether the handler is registered
			Assert.True(PropertyChangedEventManager.IsHandlerRegistered(this));

			// unregister event handler
			regCount = PropertyChangedEventManager.UnregisterEventHandler(this, Handler);
			Assert.Equal(0, regCount);

			// check whether the handler is not registered any more
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
			SynchronizationContext handlerThreadSynchronizationContext = null;
			string changedPropertyName = null;
			var handlerCalledEvent = new ManualResetEventSlim();

			// the event handler that is expected to be called
			void Handler(object sender, PropertyChangedEventArgs e)
			{
				handlerThreadSynchronizationContext = SynchronizationContext.Current;
				changedPropertyName = e.PropertyName;
				handlerCalledEvent.Set();
			}

			// register event handler
			await mThread.Factory.Run(
				() =>
				{
					Assert.NotNull(SynchronizationContext.Current);
					int regCount1 = PropertyChangedEventManager.RegisterEventHandler(this, Handler, SynchronizationContext.Current, scheduleAlways);
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
							Assert.False(handlerCalledEvent.IsSet, "Handler was invoked directly, should have been scheduled.");
						});

					Assert.True(handlerCalledEvent.Wait(60000));
					Assert.Same(mThread.Context.SynchronizationContext, handlerThreadSynchronizationContext);
					Assert.Equal(PropertyName, changedPropertyName);
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
							Assert.True(handlerCalledEvent.IsSet, "Handler was not invoked directly");
							Assert.Same(SynchronizationContext.Current, handlerThreadSynchronizationContext);
							Assert.Equal(PropertyName, changedPropertyName);
						});
				}
			}
			else
			{
				// let the executing thread fire the event (other thread than the one that registered the handler)
				// => handler should be invoked using the synchronization context of the thread that registered the handler
				PropertyChangedEventManager.FireEvent(this, PropertyName);
				Assert.True(handlerCalledEvent.Wait(60000));
				Assert.Same(mThread.Context.SynchronizationContext, handlerThreadSynchronizationContext);
				Assert.Equal(PropertyName, changedPropertyName);
			}

			// unregister event handler
			int regCount2 = PropertyChangedEventManager.UnregisterEventHandler(this, Handler);
			Assert.Equal(0, regCount2);

			// check whether the handler is not registered any more
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
			SynchronizationContext handlerThreadSynchronizationContext = null;
			string changedPropertyName = null;
			var handlerCalledEvent = new ManualResetEventSlim();

			// the event handler that is expected to be called
			void Handler(object sender, PropertyChangedEventArgs e)
			{
				handlerThreadSynchronizationContext = SynchronizationContext.Current;
				changedPropertyName = e.PropertyName;
				handlerCalledEvent.Set();
			}

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
							Handler,
							SynchronizationContext.Current,
							true,
							true,
							this,
							PropertyName);
						Assert.Equal(1, regCount1);
						Assert.False(handlerCalledEvent.IsSet, "Handler was invoked directly, should have been scheduled.");
					});

				Assert.True(handlerCalledEvent.Wait(60000));
				Assert.Same(mThread.Context.SynchronizationContext, handlerThreadSynchronizationContext);
				Assert.Equal(PropertyName, changedPropertyName);
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
							Handler,
							SynchronizationContext.Current,
							false,
							true,
							this,
							PropertyName);
						Assert.Equal(1, regCount1);
						Assert.True(handlerCalledEvent.IsSet, "Handler was not invoked directly");
						Assert.Same(SynchronizationContext.Current, handlerThreadSynchronizationContext);
						Assert.Equal(PropertyName, changedPropertyName);
					});
			}

			// check whether the handler is registered
			Assert.True(PropertyChangedEventManager.IsHandlerRegistered(this));

			// unregister event handler
			int regCount2 = PropertyChangedEventManager.UnregisterEventHandler(this, Handler);
			Assert.Equal(0, regCount2);

			// check whether the handler is not registered any more
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
			SynchronizationContext handlerThreadSynchronizationContext1 = null;
			SynchronizationContext handlerThreadSynchronizationContext2 = null;
			string changedPropertyName1 = null;
			string changedPropertyName2 = null;
			var handlerCalledEvent1 = new ManualResetEventSlim(false);
			var handlerCalledEvent2 = new ManualResetEventSlim(false);

			// event handler 1
			void Handler1(object sender, PropertyChangedEventArgs e)
			{
				handlerThreadSynchronizationContext1 = SynchronizationContext.Current;
				changedPropertyName1 = e.PropertyName;
				handlerCalledEvent1.Set();
			}

			// event handler 2
			void Handler2(object sender, PropertyChangedEventArgs e)
			{
				handlerThreadSynchronizationContext2 = SynchronizationContext.Current;
				changedPropertyName2 = e.PropertyName;
				handlerCalledEvent2.Set();
			}

			// register event handlers
			PropertyChangedEventManager.RegisterEventHandler(this, Handler1, null, scheduleAlways);
			PropertyChangedEventManager.RegisterEventHandler(this, Handler2, null, scheduleAlways);

			// get event callers
			var callers = PropertyChangedEventManager.GetEventCallers(this);
			Assert.NotNull(callers);
			var delegates = callers.GetInvocationList().Cast<PropertyChangedEventHandler>().ToArray();
			Assert.Equal(2, delegates.Length);

			// call handlers
			if (scheduleAlways)
			{
				delegates[0](this, new PropertyChangedEventArgs("Test1"));
				delegates[1](this, new PropertyChangedEventArgs("Test2"));
				Assert.True(handlerCalledEvent1.Wait(60000));
				Assert.True(handlerCalledEvent2.Wait(60000));
				Assert.Null(handlerThreadSynchronizationContext1);
				Assert.Null(handlerThreadSynchronizationContext2);
				Assert.Equal("Test1", changedPropertyName1);
				Assert.Equal("Test2", changedPropertyName2);
			}
			else
			{
				// the handlers should be called directly
				delegates[0](this, new PropertyChangedEventArgs("Test1"));
				Assert.True(handlerCalledEvent1.IsSet, "Handler was not invoked directly");
				Assert.Equal("Test1", changedPropertyName1);

				delegates[1](this, new PropertyChangedEventArgs("Test2"));
				Assert.True(handlerCalledEvent2.IsSet, "Handler was not invoked directly");
				Assert.Equal("Test2", changedPropertyName2);
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
			SynchronizationContext handlerThreadSynchronizationContext1 = null;
			SynchronizationContext handlerThreadSynchronizationContext2 = null;
			string changedPropertyName1 = null;
			string changedPropertyName2 = null;
			var handlerCalledEvent1 = new ManualResetEventSlim(false);
			var handlerCalledEvent2 = new ManualResetEventSlim(false);

			// event handler 1
			void Handler1(object sender, PropertyChangedEventArgs e)
			{
				handlerThreadSynchronizationContext1 = SynchronizationContext.Current;
				changedPropertyName1 = e.PropertyName;
				handlerCalledEvent1.Set();
			}

			// event handler 2
			void Handler2(object sender, PropertyChangedEventArgs e)
			{
				handlerThreadSynchronizationContext2 = SynchronizationContext.Current;
				changedPropertyName2 = e.PropertyName;
				handlerCalledEvent2.Set();
			}

			// register handler 1 only, but do not trigger firing immediately
			await mThread.Factory.Run(
				() =>
				{
					Assert.NotNull(SynchronizationContext.Current);

					PropertyChangedEventManager.RegisterEventHandler(
						this,
						Handler1,
						SynchronizationContext.Current,
						scheduleAlways);

					Assert.False(handlerCalledEvent1.IsSet, "Event handler was called unexpectedly.");
				});

			// handler 1 should not be called immediately
			Assert.False(handlerCalledEvent1.Wait(60000), "Event handler was scheduled to be called unexpectedly.");
			Assert.Null(changedPropertyName1);

			if (scheduleAlways)
			{
				// register handler 2 and trigger firing immediately
				await mThread.Factory.Run(
					() =>
					{
						Assert.NotNull(SynchronizationContext.Current);

						PropertyChangedEventManager.RegisterEventHandler(
							this,
							Handler2,
							SynchronizationContext.Current,
							true,
							true,
							this,
							"Test2");

						Assert.False(handlerCalledEvent2.IsSet, "Event handler was called immediately, should have been scheduled to be executed...");
					});

				// handler 2 should have been called after some time
				Assert.True(handlerCalledEvent2.Wait(60000), "The event was not called asynchronously.");
				Assert.Same(mThread.Context.SynchronizationContext, handlerThreadSynchronizationContext2);
				Assert.Equal("Test2", changedPropertyName2);
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
							Handler2,
							SynchronizationContext.Current,
							false,
							true,
							this,
							"Test2");

						Assert.True(handlerCalledEvent2.IsSet, "Event handler should have been called immediately.");
					});

				// handler 2 should have been called after some time
				Assert.Same(mThread.Context.SynchronizationContext, handlerThreadSynchronizationContext2);
				Assert.Equal("Test2", changedPropertyName2);
			}

			// get delegates invoking the event handlers
			var callers = PropertyChangedEventManager.GetEventCallers(this);
			Assert.NotNull(callers);
			var delegates = callers.GetInvocationList().Cast<PropertyChangedEventHandler>().ToArray();
			Assert.Equal(2, delegates.Length);

			// call handlers
			handlerCalledEvent1.Reset();
			handlerCalledEvent2.Reset();
			changedPropertyName1 = changedPropertyName2 = null;
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
							Assert.False(handlerCalledEvent1.IsSet, "Event handler was called unexpectedly.");
							Assert.False(handlerCalledEvent2.IsSet, "Event handler was called unexpectedly.");
						});

					Assert.True(handlerCalledEvent1.Wait(60000), "The event was not called asynchronously.");
					Assert.True(handlerCalledEvent2.Wait(60000), "The event was not called asynchronously.");
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
							Assert.True(handlerCalledEvent1.IsSet, "Event handler should have been called directly.");
							Assert.True(handlerCalledEvent2.IsSet, "Event handler should have been called directly.");
						});
				}
			}
			else
			{
				// call handlers on the current thread (different from the thread registering the event)
				// => handlers should be called in the context of the thread registering the event
				delegates[0](this, new PropertyChangedEventArgs("Test1"));
				delegates[1](this, new PropertyChangedEventArgs("Test2"));
				Assert.True(handlerCalledEvent1.Wait(60000), "The event was not called asynchronously.");
				Assert.True(handlerCalledEvent2.Wait(60000), "The event was not called asynchronously.");
			}

			// the handlers should have run in the context of the thread that registered them
			Assert.Same(mThread.Context.SynchronizationContext, handlerThreadSynchronizationContext1);
			Assert.Same(mThread.Context.SynchronizationContext, handlerThreadSynchronizationContext2);
			Assert.Equal("Test1", changedPropertyName1);
			Assert.Equal("Test2", changedPropertyName2);
		}

		/// <summary>
		/// Checks whether the event manager detects and cleans up objects that have registered events, but have been garbage collected.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void EnsureEventProvidersAreCollectable(bool scheduleAlways)
		{
			// the event handler
			void Handler(object sender, PropertyChangedEventArgs e)
			{
			}

			// register an event handler to a dummy event provider object
			// (must not be done in the same method to allow the object to be collected in the next step)
			var weakReferenceProvider = new Func<WeakReference>(
				() =>
				{
					var provider = new object();
					int regCount = PropertyChangedEventManager.RegisterEventHandler(provider, Handler, null, scheduleAlways);
					Assert.Equal(1, regCount);
					return new WeakReference(provider);
				}).Invoke();

			// kick object out of memory
			GC.Collect();

			// the event provider should now be collected
			Assert.False(weakReferenceProvider.IsAlive);
		}
	}

}
