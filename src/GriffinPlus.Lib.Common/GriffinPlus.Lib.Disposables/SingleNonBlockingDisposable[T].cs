///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
//
// This file incorporates work covered by the following copyright and permission notice:
//
//     MIT License
//
//     Copyright (c) 2016-2018 Stephen Cleary
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in all
//     copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//     SOFTWARE.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

using GriffinPlus.Lib.Disposables.Internal;

namespace GriffinPlus.Lib.Disposables
{

	/// <summary>
	/// A base class for disposables that need exactly-once semantics in a thread-safe way.
	/// </summary>
	/// <typeparam name="T">
	/// The type of "context" for the derived disposable.
	/// Since the context should not be modified, strongly consider making this an immutable type.
	/// </typeparam>
	/// <remarks>
	/// If <see cref="Dispose()"/> is called multiple times, only the first call will execute the disposal code.
	/// Other calls to <see cref="Dispose()"/> will not wait for the disposal to complete.
	/// </remarks>
	public abstract class SingleNonBlockingDisposable<T> : IDisposable
	{
		/// <summary>
		/// The context.
		/// This is never <c>null</c>.
		/// This is empty if this instance has already been disposed (or is being disposed).
		/// </summary>
		private readonly BoundActionField<T> mContext;

		/// <summary>
		/// Initializes a disposable for the specified context.
		/// </summary>
		/// <param name="context">The context passed to <see cref="Dispose(T)"/>.</param>
		protected SingleNonBlockingDisposable(T context)
		{
			mContext = new BoundActionField<T>(Dispose, context);
		}

		/// <summary>
		/// Gets a value indicating whether this instance has been disposed (or is being disposed).
		/// </summary>
		public bool IsDisposed => mContext.IsEmpty;

		/// <summary>
		/// The actual disposal method, called only once from <see cref="Dispose()"/>.
		/// </summary>
		/// <param name="context">The context for the disposal operation.</param>
		protected abstract void Dispose(T context);

		/// <summary>
		/// Disposes this instance.
		/// </summary>
		/// <remarks>
		/// If <see cref="Dispose()"/> is called multiple times, only the first call will execute the disposal code.
		/// Other calls to <see cref="Dispose()"/> will not wait for the disposal to complete.
		/// </remarks>
		public void Dispose()
		{
			mContext.TryGetAndUnset()?.Invoke();
		}

		/// <summary>
		/// Attempts to update the stored context.
		/// This method returns <c>false</c> if this instance has already been disposed (or is being disposed).
		/// </summary>
		/// <param name="contextUpdater">
		/// The function used to update an existing context.
		/// This may be called more than once, if more than one thread attempts to simultaneously update the context.
		/// </param>
		protected bool TryUpdateContext(Func<T, T> contextUpdater)
		{
			return mContext.TryUpdateContext(contextUpdater);
		}
	}

}
