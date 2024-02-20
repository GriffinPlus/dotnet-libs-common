///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Caching;

/// <summary>
/// A dummy object cache providing no caching at all
/// (just for interface compatibility with other cache implementations).
/// </summary>
public class DummyObjectCache : IObjectCache
{
	/// <summary>
	/// Puts an object into the cache.
	/// </summary>
	/// <typeparam name="T">Type of the object to put into the cache (it may also be it's base type).</typeparam>
	/// <param name="obj">Object to put into the cache.</param>
	/// <returns>Cache item keeping track of the object.</returns>
	IObjectCacheItem<T> IObjectCache.Set<T>(T obj)
	{
		return Set(obj);
	}

	/// <summary>
	/// Puts an object into the cache.
	/// </summary>
	/// <typeparam name="T">Type of the object to put into the cache (it may also be it's base type).</typeparam>
	/// <param name="obj">Object to put into the cache.</param>
	/// <returns>Cache item keeping track of the object.</returns>
	public static DummyObjectCacheItem<T> Set<T>(T obj) where T : class
	{
		return new DummyObjectCacheItem<T>(obj);
	}
}
