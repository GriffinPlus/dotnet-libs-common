///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Collections
{

	/// <summary>
	/// A generic list of disposable objects which propagates the call to <see cref="IDisposable.Dispose"/> to all
	/// elements stored in the list (useful in conjunction with the <c>using</c> statement).
	/// </summary>
	/// <typeparam name="T">Type of the list item.</typeparam>
	public class DisposableList<T> : List<T>, IDisposable where T : IDisposable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DisposableList{T}"/> class that is empty and has the default initial capacity.
		/// </summary>
		public DisposableList() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="DisposableList{T}"/> class that is empty and has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">The number of elements that the new list can initially store.</param>
		/// <exception cref="ArgumentOutOfRangeException">The capacity is less than 0.</exception>
		public DisposableList(int capacity) : base(capacity) { }

		/// <summary>
		/// Initializes a new instance of the System.Collections.Generic.List`1 class that contains elements copied from the
		/// specified collection and has sufficient capacity to accommodate the number of elements copied.
		/// </summary>
		/// <param name="collection">The collection whose elements are copied to the new list.</param>
		/// <exception cref="ArgumentNullException">The collection is null.</exception>
		public DisposableList(IEnumerable<T> collection) : base(collection) { }

		/// <summary>
		/// Disposes all items in the list.
		/// </summary>
		public void Dispose()
		{
			foreach (T item in this)
			{
				item.Dispose();
			}
		}
	}

}
