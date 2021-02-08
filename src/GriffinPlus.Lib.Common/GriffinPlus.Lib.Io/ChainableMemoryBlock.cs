///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Io
{

	/// <summary>
	/// A block of memory that can be chained with others.
	/// </summary>
	public class ChainableMemoryBlock
	{
		internal readonly byte[]               InternalBuffer;
		internal          int                  InternalLength;
		internal          ChainableMemoryBlock InternalNext;

		/// <summary>
		/// Initializes a new instance of the <see cref="ChainableMemoryBlock"/> class.
		/// </summary>
		/// <param name="capacity">Capacity of the memory block to create.</param>
		public ChainableMemoryBlock(int capacity)
		{
			InternalBuffer = new byte[capacity];
			InternalLength = 0;
		}

		/// <summary>
		/// Gets the underlying buffer.
		/// </summary>
		public byte[] Buffer => InternalBuffer;

		/// <summary>
		/// Gets the capacity of the memory block.
		/// </summary>
		public int Capacity => InternalBuffer.Length;

		/// <summary>
		/// Gets or sets the length of the memory block (must not exceed the size of the underlying buffer).
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">The length exceeds the capacity of the memory block.</exception>
		public int Length
		{
			get => InternalLength;

			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "The length must be positive.");
				}

				if (value > InternalBuffer.Length)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "The length to set exceeds the capacity of the memory block.");
				}

				InternalLength = value;
			}
		}

		/// <summary>
		/// Gets the length of the current memory block and all linked memory blocks.
		/// </summary>
		public long ChainLength
		{
			get
			{
				long totalLength = InternalLength;
				if (InternalNext != null) totalLength += InternalNext.ChainLength;
				return totalLength;
			}
		}

		/// <summary>
		/// Gets or sets the next memory block in the chain.
		/// </summary>
		public ChainableMemoryBlock Next
		{
			get => InternalNext;
			set => InternalNext = value;
		}
	}

}
