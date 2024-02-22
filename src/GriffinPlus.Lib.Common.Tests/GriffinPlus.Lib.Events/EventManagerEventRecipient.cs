///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

namespace GriffinPlus.Lib.Events;

/// <summary>
/// A test class incorporating an event handler the event manager should call in the tests.
/// </summary>
class EventManagerEventRecipient
{
	private readonly object                 mSync = new();
	private          SynchronizationContext mSynchronizationContext;
	private          string                 mData;
	private          string                 mArg1;
	private          string                 mArg2;
	private          string                 mArg3;
	private          string                 mArg4;
	private          string                 mArg5;
	private          string                 mArg6;
	private          string                 mArg7;
	private          string                 mArg8;

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
			mData = e.MyData;
			HandlerCalledEvent.Set();
		}
	}

	/// <summary>
	/// The event handler that can be invoked by an event manager.
	/// </summary>
	/// <param name="arg1">The event argument.</param>
	public void Handler(string arg1)
	{
		lock (mSync)
		{
			mSynchronizationContext = SynchronizationContext.Current;
			mArg1 = arg1;
			HandlerCalledEvent.Set();
		}
	}

	/// <summary>
	/// The event handler that can be invoked by an event manager.
	/// </summary>
	/// <param name="arg1">First event argument.</param>
	/// <param name="arg2">Second event argument.</param>
	public void Handler(string arg1, string arg2)
	{
		lock (mSync)
		{
			mSynchronizationContext = SynchronizationContext.Current;
			mArg1 = arg1;
			mArg2 = arg2;
			HandlerCalledEvent.Set();
		}
	}

	/// <summary>
	/// The event handler that can be invoked by an event manager.
	/// </summary>
	/// <param name="arg1">First event argument.</param>
	/// <param name="arg2">Second event argument.</param>
	/// <param name="arg3">Third event argument.</param>
	public void Handler(string arg1, string arg2, string arg3)
	{
		lock (mSync)
		{
			mSynchronizationContext = SynchronizationContext.Current;
			mArg1 = arg1;
			mArg2 = arg2;
			mArg3 = arg3;
			HandlerCalledEvent.Set();
		}
	}

	/// <summary>
	/// The event handler that can be invoked by an event manager.
	/// </summary>
	/// <param name="arg1">First event argument.</param>
	/// <param name="arg2">Second event argument.</param>
	/// <param name="arg3">Third event argument.</param>
	/// <param name="arg4">Fourth event argument.</param>
	public void Handler(
		string arg1,
		string arg2,
		string arg3,
		string arg4)
	{
		lock (mSync)
		{
			mSynchronizationContext = SynchronizationContext.Current;
			mArg1 = arg1;
			mArg2 = arg2;
			mArg3 = arg3;
			mArg4 = arg4;
			HandlerCalledEvent.Set();
		}
	}

	/// <summary>
	/// The event handler that can be invoked by an event manager.
	/// </summary>
	/// <param name="arg1">First event argument.</param>
	/// <param name="arg2">Second event argument.</param>
	/// <param name="arg3">Third event argument.</param>
	/// <param name="arg4">Fourth event argument.</param>
	/// <param name="arg5">Fifth event argument.</param>
	public void Handler(
		string arg1,
		string arg2,
		string arg3,
		string arg4,
		string arg5)
	{
		lock (mSync)
		{
			mSynchronizationContext = SynchronizationContext.Current;
			mArg1 = arg1;
			mArg2 = arg2;
			mArg3 = arg3;
			mArg4 = arg4;
			mArg5 = arg5;
			HandlerCalledEvent.Set();
		}
	}

	/// <summary>
	/// The event handler that can be invoked by an event manager.
	/// </summary>
	/// <param name="arg1">First event argument.</param>
	/// <param name="arg2">Second event argument.</param>
	/// <param name="arg3">Third event argument.</param>
	/// <param name="arg4">Fourth event argument.</param>
	/// <param name="arg5">Fifth event argument.</param>
	/// <param name="arg6">Sixth event argument.</param>
	public void Handler(
		string arg1,
		string arg2,
		string arg3,
		string arg4,
		string arg5,
		string arg6)
	{
		lock (mSync)
		{
			mSynchronizationContext = SynchronizationContext.Current;
			mArg1 = arg1;
			mArg2 = arg2;
			mArg3 = arg3;
			mArg4 = arg4;
			mArg5 = arg5;
			mArg6 = arg6;
			HandlerCalledEvent.Set();
		}
	}

	/// <summary>
	/// The event handler that can be invoked by an event manager.
	/// </summary>
	/// <param name="arg1">First event argument.</param>
	/// <param name="arg2">Second event argument.</param>
	/// <param name="arg3">Third event argument.</param>
	/// <param name="arg4">Fourth event argument.</param>
	/// <param name="arg5">Fifth event argument.</param>
	/// <param name="arg6">Sixth event argument.</param>
	/// <param name="arg7">Seventh event argument.</param>
	public void Handler(
		string arg1,
		string arg2,
		string arg3,
		string arg4,
		string arg5,
		string arg6,
		string arg7)
	{
		lock (mSync)
		{
			mSynchronizationContext = SynchronizationContext.Current;
			mArg1 = arg1;
			mArg2 = arg2;
			mArg3 = arg3;
			mArg4 = arg4;
			mArg5 = arg5;
			mArg6 = arg6;
			mArg7 = arg7;
			HandlerCalledEvent.Set();
		}
	}

	/// <summary>
	/// The event handler that can be invoked by an event manager.
	/// </summary>
	/// <param name="arg1">First event argument.</param>
	/// <param name="arg2">Second event argument.</param>
	/// <param name="arg3">Third event argument.</param>
	/// <param name="arg4">Fourth event argument.</param>
	/// <param name="arg5">Fifth event argument.</param>
	/// <param name="arg6">Sixth event argument.</param>
	/// <param name="arg7">Seventh event argument.</param>
	/// <param name="arg8">Eighth event argument.</param>
	public void Handler(
		string arg1,
		string arg2,
		string arg3,
		string arg4,
		string arg5,
		string arg6,
		string arg7,
		string arg8)
	{
		lock (mSync)
		{
			mSynchronizationContext = SynchronizationContext.Current;
			mArg1 = arg1;
			mArg2 = arg2;
			mArg3 = arg3;
			mArg4 = arg4;
			mArg5 = arg5;
			mArg6 = arg6;
			mArg7 = arg7;
			mArg8 = arg8;
			HandlerCalledEvent.Set();
		}
	}

	/// <summary>
	/// Gets the event that is signaled when the handler is called
	/// </summary>
	public ManualResetEventSlim HandlerCalledEvent { get; } = new(false);

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
	/// Gets the argument passed to the event handler.
	/// </summary>
	public string Data
	{
		get
		{
			lock (mSync) { return mData; }
		}
	}

	/// <summary>
	/// Gets the first argument passed to the event handler.
	/// </summary>
	public string Arg1
	{
		get
		{
			lock (mSync) { return mArg1; }
		}
	}

	/// <summary>
	/// Gets the second argument passed to the event handler.
	/// </summary>
	public string Arg2
	{
		get
		{
			lock (mSync) { return mArg2; }
		}
	}

	/// <summary>
	/// Gets the third argument passed to the event handler.
	/// </summary>
	public string Arg3
	{
		get
		{
			lock (mSync) { return mArg3; }
		}
	}

	/// <summary>
	/// Gets the fourth argument passed to the event handler.
	/// </summary>
	public string Arg4
	{
		get
		{
			lock (mSync) { return mArg4; }
		}
	}

	/// <summary>
	/// Gets the fifth argument passed to the event handler.
	/// </summary>
	public string Arg5
	{
		get
		{
			lock (mSync) { return mArg5; }
		}
	}

	/// <summary>
	/// Gets the sixth argument passed to the event handler.
	/// </summary>
	public string Arg6
	{
		get
		{
			lock (mSync) { return mArg6; }
		}
	}

	/// <summary>
	/// Gets the seventh argument passed to the event handler.
	/// </summary>
	public string Arg7
	{
		get
		{
			lock (mSync) { return mArg7; }
		}
	}

	/// <summary>
	/// Gets the eighth argument passed to the event handler.
	/// </summary>
	public string Arg8
	{
		get
		{
			lock (mSync) { return mArg8; }
		}
	}

	/// <summary>
	/// Resets the event recipient, so it can be re-used.
	/// </summary>
	public void Reset()
	{
		lock (mSync)
		{
			mArg1 = null;
			mArg2 = null;
			mArg3 = null;
			mArg4 = null;
			mArg5 = null;
			mArg6 = null;
			mArg7 = null;
			mArg8 = null;
			HandlerCalledEvent.Reset();
		}
	}
}
