///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GriffinPlus.Lib.Threading;

using Xunit;

namespace GriffinPlus.Lib.Events
{

	/// <summary>
	/// Unit tests targetting the <see cref="PropertyChangedEventManager"/> class.
	/// </summary>
	public class PropertyChangedEventManagerTests : IDisposable
	{
		private const string PROPERTY_NAME = "MyProperty";

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
		/// All operations are performed on the same thread.
		/// </summary>
		[Fact]
		public void Complete_WithoutSynchronizationContext()
		{
			// the event handler
			string changedPropertyName = null;
			PropertyChangedEventHandler handler = (sender, e) =>
			{
				changedPropertyName = e.PropertyName;
			};

			// register event handler
			int regCount = PropertyChangedEventManager.RegisterEventHandler(this, handler, null);
			Assert.Equal(1, regCount);

			// check whether the handler is registered
			Assert.True(PropertyChangedEventManager.IsHandlerRegistered(this));

			// fire event (handler is called synchronously)
			PropertyChangedEventManager.FireEvent(this, PROPERTY_NAME);
			Assert.Equal(PROPERTY_NAME, changedPropertyName);

			// unregister event handler
			regCount = PropertyChangedEventManager.UnregisterEventHandler(this, handler);
			Assert.Equal(0, regCount);

			// check whether the handler is not registered any more
			Assert.False(PropertyChangedEventManager.IsHandlerRegistered(this));
		}


		/// <summary>
		/// Tests registering with firing immediately and unregistering an event without using a synchronization context.
		/// All operations are performed on the same thread.
		/// </summary>
		[Fact]
		public void Complete_WithoutSynchronizationContext_FireImmediately()
		{
			// the event handler
			string changedPropertyName = null;
			PropertyChangedEventHandler handler = (sender, e) =>
			{
				changedPropertyName = e.PropertyName;
			};

			// register event handler and fire it immediately
			int regCount = PropertyChangedEventManager.RegisterEventHandler(this, handler, null, true, this, PROPERTY_NAME);
			Assert.Equal(1, regCount);
			Assert.Equal(PROPERTY_NAME, changedPropertyName);

			// check whether the handler is registered
			Assert.True(PropertyChangedEventManager.IsHandlerRegistered(this));

			// unregister event handler
			regCount = PropertyChangedEventManager.UnregisterEventHandler(this, handler);
			Assert.Equal(0, regCount);

			// check whether the handler is not registered any more
			Assert.False(PropertyChangedEventManager.IsHandlerRegistered(this));
		}


		/// <summary>
		/// Tests registering, firing and unregistering an event using a synchronization context.
		/// The event handler is called in the context of another thread.
		/// </summary>
		[Fact]
		public async Task Complete_WithSynchronizationContext()
		{
			// the event handler
			string changedPropertyName = null;
			var gotEventData = new ManualResetEventSlim();
			PropertyChangedEventHandler handler = (sender, e) =>
			{
				changedPropertyName = e.PropertyName;
				gotEventData.Set();
			};

			// register event handler
			await mThread.Factory.Run(
				() =>
				{
					Assert.NotNull(SynchronizationContext.Current);
					int regCount1 = PropertyChangedEventManager.RegisterEventHandler(this, handler, SynchronizationContext.Current);
					Assert.Equal(1, regCount1);
				});

			// check whether the handler is registered
			Assert.True(PropertyChangedEventManager.IsHandlerRegistered(this));

			// fire event (handler is called asynchronously)
			PropertyChangedEventManager.FireEvent(this, PROPERTY_NAME);
			Assert.True(gotEventData.Wait(200), "The event was not called asynchronously.");
			Assert.Equal(PROPERTY_NAME, changedPropertyName);

			// unregister event handler
			int regCount2 = PropertyChangedEventManager.UnregisterEventHandler(this, handler);
			Assert.Equal(0, regCount2);

			// check whether the handler is not registered any more
			Assert.False(PropertyChangedEventManager.IsHandlerRegistered(this));
		}


		/// <summary>
		/// Tests registering with firing immediately and unregistering an event using a synchronization context.
		/// The event handler is called in the context of another thread.
		/// </summary>
		[Fact]
		public async Task Complete_WithSynchronizationContext_FireImmediately()
		{
			// the event handler
			string changedPropertyName = null;
			var gotEventData = new ManualResetEventSlim();
			PropertyChangedEventHandler handler = (sender, e) =>
			{
				changedPropertyName = e.PropertyName;
				gotEventData.Set();
			};

			// register event handler and let it fire immediately
			string testData = Guid.NewGuid().ToString("D");
			await mThread.Factory.Run(
				() =>
				{
					Assert.NotNull(SynchronizationContext.Current);
					int regCount1 = PropertyChangedEventManager.RegisterEventHandler(this, handler, SynchronizationContext.Current, true, this, PROPERTY_NAME);
					Assert.Equal(1, regCount1);
				});

			// check whether the event was fired asynchronously
			Assert.True(gotEventData.Wait(200), "The event was not called asynchronously.");
			Assert.Equal(PROPERTY_NAME, changedPropertyName);

			// check whether the handler is registered
			Assert.True(PropertyChangedEventManager.IsHandlerRegistered(this));

			// unregister event handler
			int regCount2 = PropertyChangedEventManager.UnregisterEventHandler(this, handler);
			Assert.Equal(0, regCount2);

			// check whether the handler is not registered any more
			Assert.False(PropertyChangedEventManager.IsHandlerRegistered(this));
		}


		/// <summary>
		/// Tests getting a multicast delegate executing the registered event handlers for a specific event.
		/// </summary>
		[Fact]
		public void GetEventCallers_WithoutSynchronizationContext()
		{
			// the event handlers
			string changedPropertyName1 = null;
			string changedPropertyName2 = null;
			PropertyChangedEventHandler handler1 = (sender, e) => { changedPropertyName1 = e.PropertyName; };
			PropertyChangedEventHandler handler2 = (sender, e) => { changedPropertyName2 = e.PropertyName; };

			// register event handlers
			PropertyChangedEventManager.RegisterEventHandler(this, handler1, null);
			PropertyChangedEventManager.RegisterEventHandler(this, handler2, null);

			var callers = PropertyChangedEventManager.GetEventCallers(this);
			Assert.NotNull(callers);
			var delegates = callers.GetInvocationList().Cast<PropertyChangedEventHandler>().ToArray();
			Assert.Equal(2, delegates.Length);

			// call handlers
			delegates[0](this, new PropertyChangedEventArgs("Test1"));
			delegates[1](this, new PropertyChangedEventArgs("Test2"));
			Assert.Equal("Test1", changedPropertyName1);
			Assert.Equal("Test2", changedPropertyName2);
		}


		/// <summary>
		/// Tests getting a multicast delegate executing the registered event handlers for a specific event.
		/// </summary>
		[Fact]
		public async Task GetEventCallers_WithSynchronizationContext()
		{
			// the event handlers
			string changedPropertyName1 = null;
			string changedPropertyName2 = null;
			var gotEventData1 = new ManualResetEventSlim();
			var gotEventData2 = new ManualResetEventSlim();
			PropertyChangedEventHandler handler1 = (sender, e) =>
			{
				changedPropertyName1 = e.PropertyName;
				gotEventData1.Set();
			};
			PropertyChangedEventHandler handler2 = (sender, e) =>
			{
				changedPropertyName2 = e.PropertyName;
				gotEventData2.Set();
			};

			// register event handlers:
			// - register handler1 only, but do not trigger firing immediately
			// - register handler2 and trigger firing immediately
			await mThread.Factory.Run(
				() =>
				{
					Assert.NotNull(SynchronizationContext.Current);
					PropertyChangedEventManager.RegisterEventHandler(this, handler1, SynchronizationContext.Current);
					Assert.False(gotEventData1.IsSet, "Event handler was called unexpectedly.");
					PropertyChangedEventManager.RegisterEventHandler(this, handler2, SynchronizationContext.Current, true, this, "Test2");
					Assert.False(gotEventData1.IsSet, "Event handler was called immediately, should have been scheduled to be executed...");
				});

			// only handler2 should have been called after some time
			Assert.False(gotEventData1.Wait(200), "The event was called unexpectedly.");
			Assert.True(gotEventData2.Wait(200), "The event was not called asynchronously.");
			Assert.Null(changedPropertyName1);
			Assert.Equal("Test2", changedPropertyName2);

			// get delegates invoking the event handlers
			var callers = PropertyChangedEventManager.GetEventCallers(this);
			Assert.NotNull(callers);
			var delegates = callers.GetInvocationList().Cast<PropertyChangedEventHandler>().ToArray();
			Assert.Equal(2, delegates.Length);

			// call handlers
			gotEventData1.Reset();
			gotEventData2.Reset();
			changedPropertyName1 = changedPropertyName2 = null;
			delegates[0](this, new PropertyChangedEventArgs("Test1"));
			delegates[1](this, new PropertyChangedEventArgs("Test2"));
			Assert.True(gotEventData1.Wait(200), "The event was not called asynchronously.");
			Assert.True(gotEventData2.Wait(200), "The event was not called asynchronously.");
			Assert.Equal("Test1", changedPropertyName1);
			Assert.Equal("Test2", changedPropertyName2);
		}


		/// <summary>
		/// Checks whether the event manager detects and cleans up objects that have registered events, but have been garbage collected.
		/// </summary>
		[Fact]
		public void EnsureEventProvidersAreCollectable()
		{
			// the event handler
			string changedPropertyName = null;
			PropertyChangedEventHandler handler = (sender, e) => { changedPropertyName = e.PropertyName; };

			// register an event handler to a dummy event provider object
			// (must not be done in the same method to allow the object to be collected in the next step)
			var wrefProvider = new Func<WeakReference>(
				() =>
				{
					var provider = new object();
					int regCount = PropertyChangedEventManager.RegisterEventHandler(provider, handler, null);
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
