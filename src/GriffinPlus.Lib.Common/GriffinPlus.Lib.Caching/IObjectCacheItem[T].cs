///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.ComponentModel;

namespace GriffinPlus.Lib.Caching
{

	/// <summary>
	/// An item in an <see cref="IObjectCache"/>.
	/// </summary>
	/// <typeparam name="T">Type of object stored in the item (it may also be a base type).</typeparam>
	public interface IObjectCacheItem<T> : IObjectCacheItem where T : class
	{
		/// <summary>
		/// Gets or sets the object associated with the cache item.
		/// </summary>
		new T Value { get; set; }

		/// <summary>
		/// Gets or sets the object associated with the cache item
		/// (returns <c>null</c>, if the object is not in memory, yet, triggers loading the object and raises the
		/// <see cref="INotifyPropertyChanged.PropertyChanged"/> event).
		/// </summary>
		new T ValueDelayed { get; set; }

		/// <summary>
		/// Duplicates the specified object cache item.
		/// </summary>
		/// <returns>A duplicate of the current object cache item.</returns>
		new IObjectCacheItem<T> Dupe();
	}

}
