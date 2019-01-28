///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/GriffinPlus/dotnet-libs-common)
//
// Copyright 2018-2019 Sascha Falk <sascha@falk-online.eu>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for
// the specific language governing permissions and limitations under the License.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Io
{
	/// <summary>
	/// A block of memory that can be chained with others.
	/// </summary>
	public class ChainableMemoryBlock
	{
		internal readonly byte[] mBuffer;
		internal int mLength;
		internal ChainableMemoryBlock mNext;

		/// <summary>
		/// Initializes a new instance of the <see cref="ChainableMemoryBlock"/> class.
		/// </summary>
		/// <param name="capacity">Capacity of the memory block to create.</param>
		public ChainableMemoryBlock(int capacity)
		{
			mBuffer = new byte[capacity];
			mLength = 0;
		}

		/// <summary>
		/// Gets the underlying buffer.
		/// </summary>
		public byte[] Buffer
		{
			get {
				return mBuffer;
			}
		}

		/// <summary>
		/// Gets the capacity of the memory block.
		/// </summary>
		public int Capacity
		{
			get {
				return mBuffer.Length;
			}
		}

		/// <summary>
		/// Gets or sets the length of the memory block (must not exceed the size of the underlying buffer).
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">The length exceeds the capacity of the memory block.</exception>
		public int Length
		{
			get {
				return mLength;
			}

			set
			{
				if (value < 0) {
					throw new ArgumentOutOfRangeException(nameof(value), "The length must be positive.");
				}

				if (value > mBuffer.Length) {
					throw new ArgumentOutOfRangeException(nameof(value), "The length to set exceeds the capacity of the memory block.");
				}

				mLength = value;
			}
		}

		/// <summary>
		/// Gets the length of the current memory block and all linked memory blocks.
		/// </summary>
		public long ChainLength
		{
			get
			{
				long totalLength = mLength;
				if (mNext != null) totalLength += mNext.ChainLength;
				return totalLength;
			}
		}

		/// <summary>
		/// Gets or sets the next memory block in the chain.
		/// </summary>
		public ChainableMemoryBlock Next
		{
			get { return mNext; }
			set { mNext = value; }
		}

	}
}
