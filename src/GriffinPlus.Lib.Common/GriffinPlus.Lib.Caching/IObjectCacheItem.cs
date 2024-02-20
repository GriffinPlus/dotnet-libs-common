///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;

namespace GriffinPlus.Lib.Caching;

/// <summary>
/// An item in an <see cref="IObjectCache"/>.
/// </summary>
public interface IObjectCacheItem : IDisposable, INotifyPropertyChanged
{
	/// <summary>
	/// Gets or sets the object associated with the cache item.
	/// </summary>
	object Value { get; set; }

	/// <summary>
	/// Gets or sets the object associated with the cache item
	/// (returns <c>null</c>, if the object is not in memory, yet, triggers loading the object and raises the
	/// <see cref="INotifyPropertyChanged.PropertyChanged"/> event).
	/// </summary>
	object ValueDelayed { get; set; }

	/// <summary>
	/// Gets the type of the object cache item.
	/// </summary>
	Type Type { get; }

	/// <summary>
	/// Gets a value indicating whether the value of the cache item is still in memory.
	/// </summary>
	bool IsValueInMemory { get; }

	/// <summary>
	/// Gets a value indicating whether the cache item has a value (not a null reference).
	/// </summary>
	bool HasValue { get; }

	/// <summary>
	/// Assigns the specified object cache item to the current one (the specified item is disposed at the end).
	/// </summary>
	/// <param name="item">Object cache item to assign.</param>
	void TakeOwnership(IObjectCacheItem item);

	/// <summary>
	/// Creates a duplicate of the current object cache item that refers to the same file as the current object cache item
	/// at the beginning, but as soon as it is changed a new file is created.
	/// </summary>
	/// <returns>A duplicate of the current object cache item.</returns>
	IObjectCacheItem Dupe();

	/// <summary>
	/// Drops the object by intent and removes the reference to it scheduling it for garbage collection.
	/// </summary>
	void DropObject();
}
