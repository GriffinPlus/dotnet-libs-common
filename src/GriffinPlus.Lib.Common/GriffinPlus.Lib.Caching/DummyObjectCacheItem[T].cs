///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

using GriffinPlus.Lib.Events;

namespace GriffinPlus.Lib.Caching;

/// <summary>
/// An item in the <see cref="DummyObjectCache"/>.
/// </summary>
/// <typeparam name="T">Type of object stored in the item.</typeparam>
public class DummyObjectCacheItem<T> : IObjectCacheItem<T> where T : class
{
	private T mValue;

	/// <summary>
	/// Occurs when a property changes.
	/// The event is raised using the synchronization context of the thread registering the event, if possible.
	/// Otherwise, the event is raised by a worker thread.
	/// </summary>
	public event PropertyChangedEventHandler PropertyChanged
	{
		add => PropertyChangedEventManager.RegisterEventHandler(this, value, SynchronizationContext.Current, true);
		remove => PropertyChangedEventManager.UnregisterEventHandler(this, value);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DummyObjectCacheItem{T}"/> class.
	/// </summary>
	/// <param name="obj">Object to keep in the item.</param>
	internal DummyObjectCacheItem(T obj)
	{
		mValue = obj;
	}

	/// <summary>
	/// Disposes the current object cache item (does not do anything, just for interface compatibility).
	/// </summary>
	public void Dispose() { }

	/// <summary>
	/// Gets or sets the object associated with the cache item.
	/// </summary>
	public T Value
	{
		get => mValue;
		set
		{
			T oldValue = mValue;
			if (mValue == value) return;
			mValue = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(ValueDelayed));
			if ((oldValue == null && mValue != null) || (oldValue != null && mValue == null))
			{
				OnPropertyChanged(nameof(HasValue));
			}
		}
	}

	/// <summary>
	/// Gets or sets the object associated with the cache item.
	/// </summary>
	object IObjectCacheItem.Value
	{
		get => Value;
		set => Value = (T)value;
	}

	/// <summary>
	/// Gets or sets the object associated with the cache item
	/// (delaying is not supported by this class, since objects are not swapped out).
	/// </summary>
	public T ValueDelayed
	{
		get => Value;
		set => Value = value;
	}

	/// <summary>
	/// Gets or sets the object associated with the cache item
	/// (delaying is not supported by this class, since objects are not swapped out).
	/// </summary>
	object IObjectCacheItem.ValueDelayed
	{
		get => ValueDelayed;
		set => ValueDelayed = (T)value;
	}

	/// <summary>
	/// Gets a value indicating whether the value of the cache item is still in memory.
	/// </summary>
	public bool IsValueInMemory => mValue != null;

	/// <summary>
	/// Gets a value indicating whether the target has a value (not a null reference).
	/// </summary>
	public bool HasValue => mValue != null;

	/// <summary>
	/// Gets the type of the object cache item.
	/// </summary>
	public Type Type => typeof(T);

	/// <summary>
	/// Assigns the specified object cache item to the current one (the specified item is disposed at the end).
	/// </summary>
	/// <param name="item">Object cache item to assign.</param>
	public void TakeOwnership(IObjectCacheItem item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));

		if (item is not DummyObjectCacheItem<T> other)
			throw new ArgumentException("The item to assign does not have the same type as the current item.");

		Value = other.mValue;
	}

	/// <summary>
	/// Creates a duplicate of the current object cache item that refers to the same file as the current object cache item
	/// at the beginning, but as soon as it is changed a new file is created.
	/// </summary>
	/// <returns>Duplicate of the current object cache item.</returns>
	IObjectCacheItem IObjectCacheItem.Dupe()
	{
		return Dupe();
	}

	/// <summary>
	/// Creates a duplicate of the current object cache item that refers to the same file as the current object cache item
	/// at the beginning, but as soon as it is changed a new file is created.
	/// </summary>
	/// <returns>Duplicate of the current object cache item.</returns>
	IObjectCacheItem<T> IObjectCacheItem<T>.Dupe()
	{
		return Dupe();
	}

	/// <summary>
	/// Creates a duplicate of the current object cache item that refers to the same file as the current object cache item
	/// at the beginning, but as soon as it is changed a new file is created.
	/// </summary>
	/// <returns>Duplicate of the current object cache item.</returns>
	public DummyObjectCacheItem<T> Dupe()
	{
		return new DummyObjectCacheItem<T>(mValue);
	}

	/// <summary>
	/// Drops the object by intent and removes the reference to it scheduling it for garbage collection
	/// (does not do anything in the dummy object cache).
	/// </summary>
	public void DropObject() { }

	/// <summary>
	/// Raises the <see cref="PropertyChanged"/> event.
	/// </summary>
	/// <param name="name">Name of the property that has changed.</param>
	private void OnPropertyChanged([CallerMemberName] string name = null)
	{
		PropertyChangedEventManager.FireEvent(this, name);
	}
}
