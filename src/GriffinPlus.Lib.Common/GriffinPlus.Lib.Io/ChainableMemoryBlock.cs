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
		private readonly ArrayPool<byte>      mPool;
		private readonly byte[]               mBuffer;
		private          int                  mLength;
		private          ChainableMemoryBlock mPreviousBlock;
		private          ChainableMemoryBlock mNextBlock;
		private          bool                 mReleased;

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
			mPool = pool;
			mBuffer = mPool != null ? mPool.Rent(capacity) : new byte[capacity];
			mLength = 0;

			// clear the buffer, if it was retrieved from the pool
			// (not necessary for buffers allocated on the heap)
			if (mPool != null && clear)
				Array.Clear(mBuffer, 0, mBuffer.Length);
		}

		/// <summary>
		/// Releases the current block and all chained blocks returning rented buffers to the appropriate array pools, if necessary
		/// (same as <see cref="ReleaseChain"/>).
		/// </summary>
		public void Dispose()
		{
			ReleaseChain();
		}

		/// <summary>
		/// Releases the current block returning the rented buffer to the appropriate array pool, if necessary.
		/// </summary>
		public void Release()
		{
			if (!mReleased)
			{
				mPool?.Return(mBuffer);
				mReleased = true;
			}
		}

		/// <summary>
		/// Releases the current block and all chained blocks returning rented buffers to the appropriate array pools, if necessary.
		/// </summary>
		public void ReleaseChain()
		{
			mNextBlock?.ReleaseChain();
			Release();
		}

		/// <summary>
		/// Get the array pool the buffer was rented from
		/// (<c>null</c>, if the the buffer was allocated on the heap).
		/// </summary>
		public ArrayPool<byte> Pool => mPool;

		/// <summary>
		/// Gets the underlying buffer.
		/// </summary>
		public byte[] Buffer => mBuffer;

		/// <summary>
		/// Gets the capacity of the memory block.
		/// </summary>
		public int Capacity => mBuffer.Length;

		/// <summary>
		/// Gets or sets the length of the memory block (must not exceed the size of the underlying buffer).
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">The length exceeds the capacity of the memory block.</exception>
		public int Length
		{
			get => mLength;

			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "The length must be positive.");
				}

				if (value > mBuffer.Length)
				{
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
				long length = 0;
				var current = this;
				while (current != null)
				{
					length += current.mLength;
					current = current.mNextBlock;
				}

				return length;
			}
		}

		/// <summary>
		/// Gets or sets the previous memory block in the chain
		/// (<c>null</c> if this is the first block in the chain).
		/// </summary>
		public ChainableMemoryBlock Previous
		{
			get => mPreviousBlock;
			set
			{
				if (mPreviousBlock != null)
					mPreviousBlock.mNextBlock = null;

				if (value != null)
				{
					if (value.mNextBlock != null)
						value.mNextBlock.mPreviousBlock = null;

					mPreviousBlock = value;
					mPreviousBlock.mNextBlock = this;
				}
				else
				{
					mPreviousBlock = null;
				}
			}
		}

		/// <summary>
		/// Gets or sets the next memory block in the chain
		/// (<c>null</c> if this is the last block in the chain).
		/// </summary>
		public ChainableMemoryBlock Next
		{
			get => mNextBlock;
			set
			{
				if (mNextBlock != null)
					mNextBlock.mPreviousBlock = null;

				if (value != null)
				{
					if (value.mPreviousBlock != null)
						value.mPreviousBlock.mNextBlock = null;

					mNextBlock = value;
					mNextBlock.mPreviousBlock = this;
				}
				else
				{
					mNextBlock = null;
				}
			}
		}

		/// <summary>
		/// Gets the block at the start of the chain.
		/// </summary>
		public ChainableMemoryBlock GetStartOfChain()
		{
			var current = this;
			var first = this;
			while (current != null)
			{
				first = current;
				current = current.mPreviousBlock;
			}

			return first;
		}

		/// <summary>
		/// Gets the block at the start of the chain and the accumulated length of the current block and all preceding blocks.
		/// </summary>
		/// <param name="length">Receives the accumulated length of the current block and all preceding blocks.</param>
		public ChainableMemoryBlock GetStartOfChain(out long length)
		{
			length = 0;
			var current = this;
			var first = this;
			while (current != null)
			{
				first = current;
				length += current.mLength;
				current = current.mPreviousBlock;
			}

			return first;
		}

		/// <summary>
		/// Gets the block at the end of the chain.
		/// </summary>
		public ChainableMemoryBlock GetEndOfChain()
		{
			var current = this;
			var last = this;
			while (current != null)
			{
				last = current;
				current = current.mNextBlock;
			}

			return last;
		}

		/// <summary>
		/// Gets the block at the end of the chain and the accumulated length of the current block and all following blocks.
		/// </summary>
		/// <param name="length">Receives the accumulated length of the current block and all following blocks.</param>
		public ChainableMemoryBlock GetEndOfChain(out long length)
		{
			length = 0;
			var current = this;
			var last = this;
			while (current != null)
			{
				last = current;
				length += current.mLength;
				current = current.mNextBlock;
			}

			return last;
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
			Array.Copy(mBuffer, 0, buffer, offset, mLength);
			mNextBlock?.GetChainData(buffer, offset + mLength);
		}

		/// <summary>
		/// Gets the index of the first byte in the current block in the chain of blocks (for internal use only).
		/// </summary>
		/// <returns>Index of the first byte in the current block in the chain of blocks.</returns>
		internal long IndexOfFirstByteInBlock
		{
			get
			{
				GetStartOfChain(out long length);
				length -= mLength;
				return length;
			}
		}
	}

}
