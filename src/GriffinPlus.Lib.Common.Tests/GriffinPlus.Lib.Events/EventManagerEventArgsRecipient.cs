///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

namespace GriffinPlus.Lib.Events
{

	/// <summary>
	/// A test class incorporating an event handler the event manager should call in the tests.
	/// </summary>
	class EventManagerEventArgsRecipient
	{
		private readonly object                 mSync               = new();
		private readonly ManualResetEventSlim   mHandlerCalledEvent = new(false);
		private          SynchronizationContext mSynchronizationContext;
		private          string                 mData;

		/// <summary>
		/// The event handler that can be invoked by an event manager.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void Handler(object sender, EventManagerEventArgs e)
		{
			lock (mSync)
			{
				mSynchronizationContext = SynchronizationContext.Current;
				mData = e.MyString;
				mHandlerCalledEvent.Set();
			}
		}

		/// <summary>
		/// Gets the event that is signaled when the handler is called
		/// </summary>
		public ManualResetEventSlim HandlerCalledEvent => mHandlerCalledEvent;

		/// <summary>
		/// Gets the synchronization context of the thread that invoked the handler.
		/// </summary>
		public SynchronizationContext SynchronizationContext
		{
			get
			{
				lock (mSync)
				{
					return mSynchronizationContext;
				}
			}
		}

		/// <summary>
		/// Gets the data passed to the event handler.
		/// </summary>
		public string Data
		{
			get
			{
				lock (mSync)
				{
					return mData;
				}
			}
		}

		/// <summary>
		/// Resets the event recipient, so it can be re-used.
		/// </summary>
		public void Reset()
		{
			lock (mSync)
			{
				mData = null;
				mHandlerCalledEvent.Reset();
			}
		}
	}

}
