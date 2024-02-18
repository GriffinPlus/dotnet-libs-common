///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;

namespace GriffinPlus.Lib
{

	/// <summary>
	/// A generic object pool that allows to re-use frequently used objects.
	/// </summary>
	/// <typeparam name="T">Type of the objects in the pool.</typeparam>
	public class ObjectPool<T> where T : class
	{
		private readonly ConcurrentBag<T> mObjects = [];
		private readonly Func<T>          mObjectCreator;
		private readonly Action<T>        mActionOnGet;
		private readonly Action<T>        mActionOnReturn;

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectPool{T}"/> class passing a creator function that
		/// creates new instances of pooled objects, if the pool is empty.
		/// </summary>
		/// <param name="creator">Function that creates a new object.</param>
		/// <param name="actionOnGet">Function to call before returning an object (may also be <c>null</c>).</param>
		/// <param name="actionOnReturn">Function to call before an object returns to the pool (may also be <c>null</c>).</param>
		public ObjectPool(Func<T> creator, Action<T> actionOnGet = null, Action<T> actionOnReturn = null)
		{
			mObjectCreator = creator ?? throw new ArgumentNullException(nameof(creator));
			mActionOnGet = actionOnGet;
			mActionOnReturn = actionOnReturn;
		}

		/// <summary>
		/// Gets an object from the pool, creates a new one, if the pool is empty.
		/// </summary>
		/// <returns>The requested object.</returns>
		public T Get()
		{
			if (!mObjects.TryTake(out T item))
				item = mObjectCreator();

			mActionOnGet?.Invoke(item);
			return item;
		}

		/// <summary>
		/// Returns the specified object to the pool.
		/// </summary>
		/// <param name="item">Object to return.</param>
		public void Return(T item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));
			mActionOnReturn?.Invoke(item);
			mObjects.Add(item);
		}
	}

}
