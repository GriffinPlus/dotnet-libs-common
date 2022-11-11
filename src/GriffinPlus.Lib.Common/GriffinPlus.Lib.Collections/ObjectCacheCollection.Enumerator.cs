///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Collections
{

	partial class ObjectCacheCollection<T>
	{
		/// <summary>
		/// An enumerator iterating over a <see cref="ObjectCacheCollection{T}"/>.
		/// </summary>
		public class Enumerator : IEnumerator<T>
		{
			private readonly ObjectCacheCollection<T> mCollection;
			private          int                      mCurrentIndex = -1;
			private          T[]                      mCurrentPage;
			private          int                      mCurrentPageIndex = -1;

			/// <summary>
			/// Initializes a new instance of the <see cref="Enumerator"/> class.
			/// </summary>
			/// <param name="collection">Collection to enumerate.</param>
			internal Enumerator(ObjectCacheCollection<T> collection)
			{
				mCollection = collection;
			}

			/// <summary>
			/// Disposes the enumerator.
			/// </summary>
			public void Dispose() { }

			/// <summary>
			/// Gets the element in the collection at the current position of the enumerator.
			/// </summary>
			public T Current
			{
				get
				{
					if (mCollection.mItemPages != null)
					{
						int pageIndex = mCurrentIndex / mCollection.mPageSize;
						int itemIndex = mCurrentIndex - pageIndex * mCollection.mPageSize;
						return mCurrentPage[itemIndex];
					}

					return mCollection[mCurrentIndex];
				}
			}

			/// <summary>
			/// Gets the element in the collection at the current position of the enumerator.
			/// </summary>
			object IEnumerator.Current => Current;

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>
			/// <c>true</c> if the enumerator was successfully advanced to the next element;
			/// <c>false</c> if the enumerator has reached the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				if (mCurrentIndex + 1 < mCollection.mCount)
				{
					mCurrentIndex++;

					if (mCollection.mItemPages != null)
					{
						int pageIndex = mCurrentIndex / mCollection.mPageSize;
						if (pageIndex != mCurrentPageIndex)
						{
							mCurrentPageIndex = pageIndex;
							mCurrentPage = mCollection.mItemPages[mCurrentPageIndex].Value;
						}
					}

					return true;
				}

				return false;
			}

			/// <summary>
			/// Sets the enumerator to its initial position.
			/// </summary>
			public void Reset()
			{
				mCurrentIndex = -1;
			}
		}
	}

}
