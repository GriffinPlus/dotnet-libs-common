///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading;

/// <summary>
/// A <see cref="SynchronizationContext"/> that can be used to dispatch messages using a <see cref="SerialTaskQueue"/>.
/// </summary>
public sealed class SerialTaskQueueSynchronizationContext : SynchronizationContext
{
	/// <summary>
	/// Initializes the <see cref="SerialTaskQueueSynchronizationContext"/> class.
	/// </summary>
	static SerialTaskQueueSynchronizationContext()
	{
		// the synchronization context is serializing asynchronous messages
		// => register it at the synchronization context information class
		SynchronizationContextInfo.RegisterSerializingContext<SerialTaskQueueSynchronizationContext>();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SerialTaskQueueSynchronizationContext"/> class.
	/// </summary>
	/// <param name="queue">The <see cref="SerialTaskQueue"/> the synchronization context should use to dispatch messages.</param>
	internal SerialTaskQueueSynchronizationContext(SerialTaskQueue queue)
	{
		Queue = queue;
	}

	/// <summary>
	/// Gets the <see cref="SerialTaskQueue"/> the synchronization context uses to dispatch messages.
	/// </summary>
	public SerialTaskQueue Queue { get; }

	/// <summary>
	/// Dispatches an asynchronous message to a TPL thread using the associated <see cref="SerialTaskQueue"/>.
	/// </summary>
	/// <param name="callback">The <see cref="SendOrPostCallback"/> delegate to call. May not be <c>null</c>.</param>
	/// <param name="state">The object passed to the delegate.</param>
	/// <exception cref="ArgumentNullException"><paramref name="callback"/> is <c>null</c>.</exception>
	public override void Post(SendOrPostCallback callback, object state)
	{
		if (callback == null) throw new ArgumentNullException(nameof(callback));
		Queue.Enqueue(() => callback(state));
	}

	/// <summary>
	/// Dispatches an asynchronous message to a TPL thread using the associated <see cref="SerialTaskQueue"/> and waits for it to complete.
	/// </summary>
	/// <param name="callback">The <see cref="SendOrPostCallback"/> delegate to call. May not be <c>null</c>.</param>
	/// <param name="state">The object passed to the delegate.</param>
	/// <exception cref="ArgumentNullException"><paramref name="callback"/> is <c>null</c>.</exception>
	public override void Send(SendOrPostCallback callback, object state)
	{
		if (callback == null) throw new ArgumentNullException(nameof(callback));
		Task task = Queue.Enqueue(() => callback(state));
		task.WaitAndUnwrapException();
	}

	/// <summary>
	/// Creates a copy of the synchronization context.
	/// </summary>
	/// <returns>A new <see cref="SynchronizationContext"/> object.</returns>
	public override SynchronizationContext CreateCopy()
	{
		return new SerialTaskQueueSynchronizationContext(Queue);
	}

	/// <summary>
	/// Gets a hash code for this instance.
	/// </summary>
	/// <returns>A hash code for this instance.</returns>
	public override int GetHashCode()
	{
		return Queue.GetHashCode();
	}

	/// <summary>
	/// Determines whether the specified object is equal to this instance.
	/// It is considered equal if it refers to the same underlying <see cref="SerialTaskQueue"/> as this instance.
	/// </summary>
	/// <param name="obj">The object to compare with this instance.</param>
	/// <returns>
	/// <c>true</c> if the specified object is equal to this instance;
	/// otherwise <c>false</c>.
	/// </returns>
	public override bool Equals(object obj)
	{
		return obj is SerialTaskQueueSynchronizationContext other && ReferenceEquals(Queue, other.Queue);
	}
}
