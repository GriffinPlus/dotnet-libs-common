///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using GriffinPlus.Lib.Threading;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GriffinPlus.Lib.Events
{
	/// <summary>
	/// Unit tests targetting the <see cref="EventManager{T}"/> class.
	/// </summary>
	public class EventManagerTests : IDisposable
	{
		private const string EVENT_NAME = "MyEvent";
		
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
		/// Initializes an instance the <see cref="EventManagerTests"/> class performing common initialization before running a test.
		/// </summary>
		public EventManagerTests()
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
		/// All operations are performed on the same thread.
		/// </summary>
		[Fact]
		public void Complete_WithoutSynchronizationContext()
		{
			// the event handler
			string eventData = null;
			EventHandler<EventManagerEventArgs> handler = (sender, e) => { 
				eventData = e.MyString;
			};

			// register event handler
			int regCount = EventManager<EventManagerEventArgs>.RegisterEventHandler(this, EVENT_NAME, handler, null);
			Assert.Equal(1, regCount);

			// check whether the handler is registered
			Assert.True(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME));
			Assert.True(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME, handler));

			// fire event (handler is called synchronously)
			string testData = Guid.NewGuid().ToString("D");
			EventManager<EventManagerEventArgs>.FireEvent(this, EVENT_NAME, this, new EventManagerEventArgs(testData));
			Assert.Equal(testData, eventData);

			// unregister event handler
			regCount = EventManager<EventManagerEventArgs>.UnregisterEventHandler(this, EVENT_NAME, handler);
			Assert.Equal(0, regCount);

			// check whether the handler is not registered
			Assert.False(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME));
			Assert.False(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME, handler));
		}


		/// <summary>
		/// Tests registering with firing immediately and unregistering an event without using a synchronization context.
		/// All operations are performed on the same thread.
		/// </summary>
		[Fact]
		public void Complete_WithoutSynchronizationContext_FireImmediately()
		{
			// the event handler
			string eventData = null;
			EventHandler<EventManagerEventArgs> handler = (sender, e) => { 
				eventData = e.MyString;
			};

			// register event handler and fire it immediately
			string testData = Guid.NewGuid().ToString("D");
			int regCount = EventManager<EventManagerEventArgs>.RegisterEventHandler(this, EVENT_NAME, handler, null, true, this, new EventManagerEventArgs(testData));
			Assert.Equal(1, regCount);
			Assert.Equal(testData, eventData);

			// check whether the handler is registered
			Assert.True(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME));
			Assert.True(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME, handler));

			// unregister event handler
			regCount = EventManager<EventManagerEventArgs>.UnregisterEventHandler(this, EVENT_NAME, handler);
			Assert.Equal(0, regCount);

			// check whether the handler is not registered
			Assert.False(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME));
			Assert.False(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME, handler));
		}


		/// <summary>
		/// Tests registering, firing and unregistering an event using a synchronization context.
		/// The event handler is called in the context of another thread.
		/// </summary>
		[Fact]
		public async Task Complete_WithSynchronizationContext()
		{
			// the event handler
			string eventData = null;
			ManualResetEventSlim gotEventData = new ManualResetEventSlim();
			EventHandler<EventManagerEventArgs> handler = (sender, e) => { 
				eventData = e.MyString;
				gotEventData.Set();
			};

			// register event handler
			await mThread.Factory.Run(() => {
				Assert.NotNull(SynchronizationContext.Current);
				int regCount1 = EventManager<EventManagerEventArgs>.RegisterEventHandler(this, EVENT_NAME, handler, SynchronizationContext.Current);
				Assert.Equal(1, regCount1);
			});

			// fire event (handler is called asynchronously)
			string testData = Guid.NewGuid().ToString("D");
			EventManager<EventManagerEventArgs>.FireEvent(this, EVENT_NAME, this, new EventManagerEventArgs(testData));
			Assert.True(gotEventData.Wait(200), "The event was not called asynchronously.");
			Assert.Equal(testData, eventData);

			// check whether the handler is registered
			Assert.True(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME));
			Assert.True(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME, handler));

			// unregister event handler
			int regCount2 = EventManager<EventManagerEventArgs>.UnregisterEventHandler(this, EVENT_NAME, handler);
			Assert.Equal(0, regCount2);

			// check whether the handler is not registered
			Assert.False(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME));
			Assert.False(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME, handler));
		}


		/// <summary>
		/// Tests registering with firing immediately and unregistering an event using a synchronization context.
		/// The event handler is called in the context of another thread.
		/// </summary>
		[Fact]
		public async Task Complete_WithSynchronizationContext_FireImmediately()
		{
			// the event handler
			string eventData = null;
			ManualResetEventSlim gotEventData = new ManualResetEventSlim();
			EventHandler<EventManagerEventArgs> handler = (sender, e) => { 
				eventData = e.MyString;
				gotEventData.Set();
			};

			// register event handler and let it fire immediately
			string testData = Guid.NewGuid().ToString("D");
			await mThread.Factory.Run(() => {
				Assert.NotNull(SynchronizationContext.Current);
				int regCount1 = EventManager<EventManagerEventArgs>.RegisterEventHandler(this, EVENT_NAME, handler, SynchronizationContext.Current, true, this, new EventManagerEventArgs(testData));
				Assert.Equal(1, regCount1);
			});
			
			// check whether the event was fired asynchronously
			Assert.True(gotEventData.Wait(200), "The event was not called asynchronously.");
			Assert.Equal(testData, eventData);

			// check whether the handler is registered
			Assert.True(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME));
			Assert.True(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME, handler));

			// unregister event handler
			int regCount2 = EventManager<EventManagerEventArgs>.UnregisterEventHandler(this, EVENT_NAME, handler);
			Assert.Equal(0, regCount2);

			// check whether the handler is not registered
			Assert.False(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME));
			Assert.False(EventManager<EventManagerEventArgs>.IsHandlerRegistered(this, EVENT_NAME, handler));
		}


		/// <summary>
		/// Tests getting a multicast delegate executing the registered event handlers for a specific event.
		/// </summary>
		[Fact]
		public void GetEventCallers_WithoutSynchronizationContext()
		{
			// the event handlers
			string eventData1 = null;
			string eventData2 = null;
			EventHandler<EventManagerEventArgs> handler1 = (sender, e) => { eventData1 = e.MyString; };
			EventHandler<EventManagerEventArgs> handler2 = (sender, e) => { eventData2 = e.MyString; };

			// register event handlers
			EventManager<EventManagerEventArgs>.RegisterEventHandler(this, EVENT_NAME, handler1, null);
			EventManager<EventManagerEventArgs>.RegisterEventHandler(this, EVENT_NAME, handler2, null);

			var callers = EventManager<EventManagerEventArgs>.GetEventCallers(this, EVENT_NAME);
			Assert.NotNull(callers);
			var delegates = callers.GetInvocationList().Cast<EventHandler<EventManagerEventArgs>>().ToArray();
			Assert.Equal(2, delegates.Length);

			// call handlers
			delegates[0](this, new EventManagerEventArgs("Test1"));
			delegates[1](this, new EventManagerEventArgs("Test2"));
			Assert.Equal("Test1", eventData1);
			Assert.Equal("Test2", eventData2);
		}


		/// <summary>
		/// Tests getting a multicast delegate executing the registered event handlers for a specific event.
		/// </summary>
		[Fact]
		public async Task GetEventCallers_WithSynchronizationContext()
		{
			// the event handlers
			string eventData1 = null;
			string eventData2 = null;
			ManualResetEventSlim gotEventData1 = new ManualResetEventSlim();
			ManualResetEventSlim gotEventData2 = new ManualResetEventSlim();
			EventHandler<EventManagerEventArgs> handler1 = (sender, e) => { eventData1 = e.MyString; gotEventData1.Set(); };
			EventHandler<EventManagerEventArgs> handler2 = (sender, e) => { eventData2 = e.MyString; gotEventData2.Set(); };

			// register event handlers:
			// - register handler1 only, but do not trigger firing immediately
			// - register handler2 and trigger firing immediately
			await mThread.Factory.Run(() => {
				Assert.NotNull(SynchronizationContext.Current);
				EventManager<EventManagerEventArgs>.RegisterEventHandler(this, EVENT_NAME, handler1, SynchronizationContext.Current);
				Assert.False(gotEventData1.IsSet, "Event handler was called unexpectedly.");
				EventManager<EventManagerEventArgs>.RegisterEventHandler(this, EVENT_NAME, handler2, SynchronizationContext.Current, true, this, new EventManagerEventArgs("Test2"));
				Assert.False(gotEventData1.IsSet, "Event handler was called immediately, should have been scheduled to be executed...");
			});

			// only handler2 should have been called after some time
			Assert.False(gotEventData1.Wait(200), "The event was called unexpectedly.");
			Assert.True(gotEventData2.Wait(200), "The event was not called asynchronously.");
			Assert.Null(eventData1);
			Assert.Equal("Test2", eventData2);

			// get delegates invoking the event handlers
			var callers = EventManager<EventManagerEventArgs>.GetEventCallers(this, EVENT_NAME);
			Assert.NotNull(callers);
			var delegates = callers.GetInvocationList().Cast<EventHandler<EventManagerEventArgs>>().ToArray();
			Assert.Equal(2, delegates.Length);

			// call handlers
			gotEventData1.Reset();
			gotEventData2.Reset();
			eventData1 = eventData2 = null;
			delegates[0](this, new EventManagerEventArgs("Test1"));
			delegates[1](this, new EventManagerEventArgs("Test2"));
			Assert.True(gotEventData1.Wait(200), "The event was not called asynchronously.");
			Assert.True(gotEventData2.Wait(200), "The event was not called asynchronously.");
			Assert.Equal("Test1", eventData1);
			Assert.Equal("Test2", eventData2);
		}


		/// <summary>
		/// Checks whether the event manager detects and cleans up objects that have registered events, but have been garbage collected.
		/// </summary>
		[Fact]
		public void EnsureEventProvidersAreCollectable()
		{
			// the event handler
			string eventData = null;
			EventHandler<EventManagerEventArgs> handler = (sender, e) => { 
				eventData = e.MyString;
			};

			// register an event handler to a dummy event provider object
			// (must not be done in the same method to allow the object to be collected in the next step)
			WeakReference wrefProvider = new Func<WeakReference>(() =>
			{
				object provider = new object();
				int regCount = EventManager<EventManagerEventArgs>.RegisterEventHandler(provider, EVENT_NAME, handler, null);
				Assert.Equal(1, regCount);
				return new WeakReference(provider);
			}).Invoke();

			// kick object out of memory
			GC.Collect();

			// the event provider should now be collected
			Assert.False(wrefProvider.IsAlive);
		}

	}
}
