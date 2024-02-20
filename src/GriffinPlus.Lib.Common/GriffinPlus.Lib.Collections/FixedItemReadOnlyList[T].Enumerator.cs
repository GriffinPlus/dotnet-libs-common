///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Collections;

partial class FixedItemReadOnlyList<T>
{
	/// <summary>
	/// Enumerator of the <see cref="FixedItemReadOnlyList{T}"/> class.
	/// </summary>
	public class Enumerator : IEnumerator<T>
	{
		private readonly FixedItemReadOnlyList<T> mList;
		private          int                      mItemIndex;

		/// <summary>
		/// Initializes a new instance of the <see cref="Enumerator"/> class.
		/// </summary>
		/// <param name="list">The collection the enumerator should iterate over.</param>
		internal Enumerator(FixedItemReadOnlyList<T> list)
		{
			mList = list;
			mItemIndex = -1;
		}

		/// <summary>
		/// Disposes the enumerator.
		/// </summary>
		public void Dispose() { }

		/// <summary>
		/// Moves the enumerator to the next item.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the enumerator was successfully moved;
		/// <c>false</c> if the enumerator has reached the end of the collection.
		/// </returns>
		public bool MoveNext()
		{
			if (mItemIndex + 1 >= mList.Count) return false;
			mItemIndex++;
			return true;
		}

		/// <summary>
		/// Resets the enumerator to the beginning of the collection.
		/// </summary>
		public void Reset()
		{
			mItemIndex = -1;
		}

		/// <summary>
		/// Gets the current item the enumerator points to.
		/// </summary>
		public T Current => mList.mItem;

		/// <summary>
		/// Gets the current item the enumerator points to.
		/// </summary>
		object IEnumerator.Current => Current;
	}
}
