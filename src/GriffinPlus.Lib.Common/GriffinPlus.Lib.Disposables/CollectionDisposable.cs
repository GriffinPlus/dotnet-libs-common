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
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GriffinPlus.Lib.Disposables
{

	/// <summary>
	/// Disposes a collection of disposables.
	/// </summary>
	public sealed class CollectionDisposable : SingleDisposable<ImmutableQueue<IDisposable>>
	{
		/// <summary>
		/// Creates a disposable that disposes a collection of disposables.
		/// </summary>
		/// <param name="disposables">The disposables to dispose.</param>
		public CollectionDisposable(params IDisposable[] disposables)
			: this((IEnumerable<IDisposable>)disposables) { }

		/// <summary>
		/// Creates a disposable that disposes a collection of disposables.
		/// </summary>
		/// <param name="disposables">The disposables to dispose.</param>
		public CollectionDisposable(IEnumerable<IDisposable> disposables)
			: base(ImmutableQueue.CreateRange(disposables)) { }

		/// <inheritdoc/>
		protected override void Dispose(ImmutableQueue<IDisposable> context)
		{
			foreach (IDisposable disposable in context)
			{
				disposable.Dispose();
			}
		}

		/// <summary>
		/// Adds a disposable to the collection of disposables.
		/// If this instance is already disposed or disposing, then <paramref name="disposable"/> is disposed immediately.
		/// </summary>
		/// <param name="disposable">The disposable to add to our collection.</param>
		public void Add(IDisposable disposable)
		{
			// ReSharper disable once AccessToDisposedClosure
			if (!TryUpdateContext(x => x.Enqueue(disposable)))
				disposable.Dispose();
		}

		/// <summary>
		/// Creates a disposable that disposes a collection of disposables.
		/// </summary>
		/// <param name="disposables">The disposables to dispose.</param>
		public static CollectionDisposable Create(params IDisposable[] disposables)
		{
			return new CollectionDisposable(disposables);
		}

		/// <summary>
		/// Creates a disposable that disposes a collection of disposables.
		/// </summary>
		/// <param name="disposables">The disposables to dispose.</param>
		public static CollectionDisposable Create(IEnumerable<IDisposable> disposables)
		{
			return new CollectionDisposable(disposables);
		}
	}

}
