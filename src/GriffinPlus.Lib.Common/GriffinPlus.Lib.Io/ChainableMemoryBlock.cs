///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;

namespace GriffinPlus.Lib.Io
{

	/// <summary>
	/// A block of memory that can be chained with others.
	/// </summary>
	public sealed class ChainableMemoryBlock : IDisposable
	{
		internal readonly ArrayPool<byte>      InternalPool;
		internal readonly byte[]               InternalBuffer;
		internal          int                  InternalLength;
		internal          ChainableMemoryBlock InternalNext;
		internal          bool                 ReleasedInternal;

		/// <summary>
		/// Initializes a new instance of the <see cref="ChainableMemoryBlock"/> class with the specified capacity.
		/// The buffer is allocated on the heap.
		/// </summary>
		/// <param name="capacity">Capacity of the memory block to create.</param>
		public ChainableMemoryBlock(int capacity) : this(capacity, null, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ChainableMemoryBlock"/> class with the specified capacity.
		/// The buffer is rented from the specified array pool.
		/// </summary>
		/// <param name="capacity">Capacity of the memory block to create.</param>
		/// <param name="pool">Array pool to rent the buffer from (<c>null</c> to use the regular heap).</param>
		/// <param name="clear"><c>true</c> to initialize the buffer with zeros; otherwise <c>false</c>.</param>
		public ChainableMemoryBlock(int capacity, ArrayPool<byte> pool, bool clear = false)
		{
			InternalPool = pool;
			InternalBuffer = InternalPool != null ? InternalPool.Rent(capacity) : new byte[capacity];
			InternalLength = 0;

			// clear the buffer, if it was retrieved from the pool
			// (not necessary for buffers allocated on the heap)
			if (InternalPool != null && clear)
				Array.Clear(InternalBuffer, 0, InternalBuffer.Length);
		}

		/// <summary>
		/// Releases the current block and all chained blocks returning rented buffers to the appropriate array pools, if necessary
		/// (same as <see cref="ReleaseChain"/>).
		/// </summary>
		public void Dispose() => ReleaseChain();

		/// <summary>
		/// Releases the current block returning the rented buffer to the appropriate array pool, if necessary.
		/// </summary>
		public void Release()
		{
			if (!ReleasedInternal)
			{
				InternalPool?.Return(InternalBuffer);
				ReleasedInternal = true;
			}
		}

		/// <summary>
		/// Releases the current block and all chained blocks returning rented buffers to the appropriate array pools, if necessary.
		/// </summary>
		public void ReleaseChain()
		{
			InternalNext?.ReleaseChain();
			Release();
		}

		/// <summary>
		/// Get the array pool the buffer was rented from
		/// (<c>null</c>, if the the buffer was allocated on the heap).
		/// </summary>
		public ArrayPool<byte> Pool => InternalPool;

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

		/// <summary>
		/// Gets all data stored in the current memory block and all linked memory blocks
		/// (limited to memory block chains with a maximum total length of <see cref="int.MaxValue"/>).
		/// </summary>
		/// <returns>Data stored in the chain of memory blocks.</returns>
		public byte[] GetChainData()
		{
			byte[] buffer = new byte[ChainLength];
			GetChainData(buffer, 0);
			return buffer;
		}

		/// <summary>
		/// Copies all data stored in the current memory block and all linked memory blocks into the specified buffer.
		/// </summary>
		/// <param name="buffer">Buffer to copy data into.</param>
		/// <param name="offset">Offset in the array to copy the data to.</param>
		private void GetChainData(byte[] buffer, int offset)
		{
			Array.Copy(InternalBuffer, 0, buffer, offset, InternalLength);
			InternalNext?.GetChainData(buffer, offset + InternalLength);
		}
	}

}
