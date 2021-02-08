///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GriffinPlus.Lib.Threading
{

	/// <summary>
	/// Provides an enumerator that keeps a monitor synchronized object locked during enumeration.
	/// </summary>
	public class MonitorSynchronizedEnumerator<T> : IEnumerator<T>
	{
		private readonly IEnumerator<T> mInner;
		private readonly object         mSync;
		private          bool           mDisposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="MonitorSynchronizedEnumerator{T}"/> class.
		/// </summary>
		/// <param name="inner">Inner enumerator (unsynchronized).</param>
		/// <param name="sync">Synchronization object to use for locking the enumerated collection.</param>
		public MonitorSynchronizedEnumerator(IEnumerator<T> inner, object sync)
		{
			mInner = inner;
			mSync = sync;
			mDisposed = false;
			Monitor.Enter(mSync);
		}

		/// <summary>
		/// Disposes the enumerator releasing the lock acquired in the constructor.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// Disposes the enumerator releasing the lock acquired in the constructor.
		/// </summary>
		/// <param name="disposing">
		/// true to release unmanaged resources and dispose other managed objects as well;
		/// false to release unmanaged resources only.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (!mDisposed)
			{
				if (disposing)
				{
					Monitor.Exit(mSync);
					mDisposed = true;
					GC.SuppressFinalize(this);
				}
				else
				{
					// enumerator is finalized, it was not disposed explicitly before
					Debug.Fail($"The {typeof(MonitorSynchronizedEnumerator<T>).FullName} was not disposed. The collection the enumerator is associated with was not unblocked!");
				}
			}
		}

#if DEBUG
		/// <summary>
		/// Finalizes the current instance.
		/// </summary>
		~MonitorSynchronizedEnumerator()
		{
			Dispose(false);
		}
#endif

		/// <summary>
		/// Move enumerator to the next element.
		/// </summary>
		/// <returns>true if the enumerator was successfully moved, false if the enumerator is at the end of the collection.</returns>
		public bool MoveNext()
		{
			return mInner.MoveNext();
		}

		/// <summary>
		/// Resets the enumerator to the beginning of the collection.
		/// </summary>
		public void Reset()
		{
			mInner.Reset();
		}

		/// <summary>
		/// Gets the element the enumerator points to.
		/// </summary>
		public T Current => mInner.Current;

		/// <summary>
		/// Gets the element the enumerator points to.
		/// </summary>
		object IEnumerator.Current => Current;
	}

}
