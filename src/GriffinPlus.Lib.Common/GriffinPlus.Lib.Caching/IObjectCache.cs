///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Caching;

/// <summary>
/// Interface of an object cache that caches objects in a backing storage allowing to free memory occupied by
/// rarely used objects without entirely discarding them.
/// </summary>
public interface IObjectCache
{
	/// <summary>
	/// Puts an object into the cache.
	/// </summary>
	/// <typeparam name="T">Type of the object to put into the cache (it may also be a base type).</typeparam>
	/// <param name="obj">Object to put into the cache.</param>
	/// <returns>Cache item keeping track of the object.</returns>
	IObjectCacheItem<T> Set<T>(T obj) where T : class;
}
