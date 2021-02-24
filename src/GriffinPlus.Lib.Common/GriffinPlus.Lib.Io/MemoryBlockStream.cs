///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Io
{

	/// <summary>
	/// A stream with a linked list of memory blocks as backing store.
	/// </summary>
	/// <remarks>
	/// The <see cref="MemoryBlockStream"/> class creates streams that have memory as backing store instead of a
	/// disk or a network connection, just like the <see cref="MemoryStream"/> class. The major difference compared
	/// to the <see cref="MemoryStream"/> class is the way data is stored. The <see cref="MemoryStream"/> class works
	/// on a continuous block of memory while the <see cref="MemoryBlockStream"/> class makes use of a linked list of
	/// memory blocks.
	///
	/// If the amount of needed memory is known the <see cref="MemoryStream"/> class is the right choice since it can
	/// allocate the memory block in a single operation and work on it efficiently without walking a linked list. But
	/// if the amount of needed memory is not known the <see cref="MemoryBlockStream"/> class is a better choice since
	/// it can grow without copying any data while resizing.
	///
	/// Buffers can be allocated on demand on the heap or rented from an array pool to optimize allocation speed and
	/// reduce garbage collection pressure.
	/// </remarks>
	public class MemoryBlockStream : Stream
	{
		/// <summary>
		/// Default size of block in the stream.
		/// 80 kByte is small enough for the regular heap and avoids allocation on the large object heap.
		/// </summary>
		private const int DefaultBlockSize = 80 * 1024;

		private          long                 mPosition;
		private          long                 mLength;
		private          long                 mCapacity;
		private readonly int                  mBlockSize;
		private          long                 mCurrentBlockStartIndex;
		private readonly bool                 mCanSeek;
		private readonly ArrayPool<byte>      mArrayPool;
		private readonly bool                 mReleaseReadBlocks;
		private          ChainableMemoryBlock mFirstBlock;
		private          ChainableMemoryBlock mCurrentBlock;
		private          ChainableMemoryBlock mLastBlock;
		private          bool                 mDisposed;

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryBlockStream"/> class.
		/// Buffers are allocated on the heap.
		/// The block size defaults to 80 kByte.
		/// The stream is seekable and grows as data is written.
		/// </summary>
		public MemoryBlockStream() : this(DefaultBlockSize, null, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryBlockStream"/> class.
		/// Buffers are rented from the specified array pool.
		/// The block size defaults to 80 kByte.
		/// The stream is seekable and grows as data is written.
		/// </summary>
		/// <param name="pool">Array pool to use for allocating buffers.</param>
		/// <exception cref="ArgumentNullException"><paramref name="pool"/> is <c>null</c>.</exception>
		public MemoryBlockStream(ArrayPool<byte> pool) : this(DefaultBlockSize, pool, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryBlockStream"/> class with a specific block size.
		/// Buffers are allocated on the heap.
		/// The stream is seekable and grows as data is written.
		/// </summary>
		/// <param name="blockSize">Size of a block in the stream.</param>
		/// <exception cref="ArgumentException">The specified block size is less than or equal to 0.</exception>
		public MemoryBlockStream(int blockSize) : this(blockSize, null, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryBlockStream"/> class with a specific block size.
		/// Buffers are rented from the specified array pool.
		/// The stream can configured to be seekable or release buffers as data is read.
		/// </summary>
		/// <param name="blockSize">
		/// Size of a block in the stream.
		/// The actual block size may be greater than the specified size, if the buffer is rented from an array pool.
		/// </param>
		/// <param name="pool">Array pool to rent buffers from (<c>null</c> to allocate buffers on the heap).</param>
		/// <param name="releaseReadBlocks">
		/// <c>true</c> to release memory blocks that have been read (makes the stream unseekable);
		/// <c>false</c> to keep written memory blocks enabling seeking and changing the length of the stream.
		/// </param>
		/// <exception cref="ArgumentException">The specified block size is less than or equal to 0.</exception>
		public MemoryBlockStream(int blockSize, ArrayPool<byte> pool, bool releaseReadBlocks)
		{
			if (blockSize <= 0)
			{
				throw new ArgumentException(
					"The block size must be greater than 0.",
					nameof(blockSize));
			}

			mArrayPool = pool;
			mBlockSize = blockSize;
			mReleaseReadBlocks = releaseReadBlocks;
			mCanSeek = !releaseReadBlocks;
			mCurrentBlockStartIndex = 0;
			mCapacity = 0;
			mLength = 0;
			mPosition = 0;
		}

		/// <summary>
		/// Disposes the stream releasing the underlying memory block chain
		/// (returns rented buffers to their array pool, if necessary).
		/// </summary>
		/// <param name="disposing">
		/// <c>true</c> if the stream is being disposed;
		/// <c>false</c> if the stream is being finalized.
		/// </param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (mFirstBlock != null)
				{
					mFirstBlock?.ReleaseChain();
					mFirstBlock = null;
					mCurrentBlock = null;
					mLastBlock = null;
					mCurrentBlockStartIndex = 0;
					mCapacity = 0;
					mLength = 0;
					mPosition = 0;
				}

				mDisposed = true;
			}
		}

#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0
		/// <summary>
		/// Asynchronously disposes the stream releasing the underlying memory block chain
		/// (returns rented buffers to their array pool, if necessary).
		/// </summary>
		public override ValueTask DisposeAsync()
		{
			base.DisposeAsync();
			Dispose(true);
			return new ValueTask();
		}
#elif NETSTANDARD2_0 || NETCOREAPP2_1 || NET461
		// This method is not supported by the Stream class.
#else
#error Unhandled target framework.
#endif

		#endregion

		#region Stream Capabilities

		/// <summary>
		/// Gets a value indicating whether the stream supports reading (always true).
		/// </summary>
		public override bool CanRead => true;

		/// <summary>
		/// Gets a value indicating whether the stream supports writing (always true).
		/// </summary>
		public override bool CanWrite => true;

		/// <summary>
		/// Gets a value indicating whether the stream supports seeking.
		/// </summary>
		public override bool CanSeek => mCanSeek;

		#endregion

		#region Position

		/// <summary>
		/// Gets or sets the current position within the stream.
		/// </summary>
		/// <exception cref="ArgumentException">The position is out of bounds when trying to set it.</exception>
		/// <exception cref="NotSupportedException">Setting the position is not supported as the stream does not support seeking.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		public override long Position
		{
			get => mPosition;
			set => Seek(value, SeekOrigin.Begin);
		}

		#endregion

		#region Length

		/// <summary>
		/// Gets the length of the current stream.
		/// </summary>
		public override long Length => mLength;

		#endregion

		#region SetLength(long length)

		/// <summary>
		/// Sets the length of the stream.
		/// </summary>
		/// <param name="length">The desired length of the current stream in bytes.</param>
		/// <exception cref="ArgumentException">The specified length is negative.</exception>
		/// <exception cref="NotSupportedException">The stream does not support seeking.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <remarks>
		/// If the specified length is less than the current length of the stream, the stream is truncated. If the current position
		/// within the stream is past the end of the stream after the truncation, the <see cref="ReadByte"/> method returns -1, the
		/// 'Read' methods read zero bytes into the provided byte hash, and the 'Write' methods append specified bytes at the end of the
		/// stream, increasing its length. If the specified value is larger than the current capacity, the capacity is increased, and
		/// the current position within the stream is unchanged. If the length is increased, the contents of the stream between the old
		/// and the new length are initialized with zeros.
		/// </remarks>
		public override void SetLength(long length)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			if (!mCanSeek)
				throw new NotSupportedException("The stream does not support seeking.");

			if (length < 0)
				throw new ArgumentException("The length must not be negative.", nameof(length));

			if (length > mCapacity)
			{
				// requested size is greater than the current capacity of the stream
				// => enlarge buffer by adding a new memory block

				// determine the number of bytes to allocate additionally
				long additionallyNeededSpace = length - mCapacity;
				long lengthOfLastBlock = length - mCapacity;
				while (true)
				{
					var newBlock = new ChainableMemoryBlock(mBlockSize, mArrayPool, true); // initializes the buffer with zeros
					if (mCapacity == 0)
					{
						// no block in the chain, yet
						// => new block becomes the only block
						mFirstBlock = newBlock;
						mCurrentBlock = newBlock;
						mLastBlock = newBlock;
					}
					else
					{
						// at least one block is in the chain
						// => append new block
						mLastBlock.Length = mLastBlock.Capacity;
						mLastBlock.InternalNext = newBlock;
						mLastBlock = newBlock;
					}

					// adjust stream capacity
					mCapacity += newBlock.Capacity;

					// abort, if needed space has been allocated
					additionallyNeededSpace -= mBlockSize;
					if (additionallyNeededSpace <= 0) break;

					// adjust length of the last block, since another block is following
					lengthOfLastBlock -= newBlock.Capacity;
				}

				// adjust length (position is kept unchanged)
				Debug.Assert(lengthOfLastBlock <= int.MaxValue);
				mLastBlock.Length = (int)lengthOfLastBlock;
				mLength = length;
			}
			else
			{
				// requested size is less than (or equal to) the capacity
				if (length == 0)
				{
					// the stream will not contain any data after setting the length
					// => release all blocks and reset the stream
					mFirstBlock?.ReleaseChain();
					mFirstBlock = null;
					mCurrentBlock = null;
					mLastBlock = null;
					mCurrentBlockStartIndex = 0;
					mCapacity = 0;
					mLength = 0;
					mPosition = 0;
				}
				else
				{
					// the stream will contain less, but at least some data
					// => release memory blocks that are not needed any more
					long remaining = length;
					long lastBlockStartIndex = 0;
					mCapacity = 0;
					var block = mFirstBlock;
					while (true)
					{
						remaining -= Math.Min(remaining, block.InternalLength);
						if (remaining == 0) break;
						lastBlockStartIndex += block.InternalLength;
						mCapacity += block.InternalLength;
						block = block.InternalNext;
					}

					block.InternalNext?.ReleaseChain();
					mCapacity += block.Capacity;
					block.InternalNext = null;
					mLastBlock = block;
					mLength = length;

					// clear all bytes up to the end of the last block
					int bytesToClear = (int)(mLastBlock.Capacity - length + lastBlockStartIndex);
					Array.Clear(mLastBlock.InternalBuffer, (int)(length - lastBlockStartIndex), bytesToClear);
					mLastBlock.InternalLength = (int)(length - lastBlockStartIndex);

					// adjust stream position, if the position is out of bounds now
					if (mPosition >= mLength)
					{
						mPosition = mLength;
						mCurrentBlock = mLastBlock;
						mCurrentBlockStartIndex = lastBlockStartIndex;
					}
				}
			}
		}

		#endregion

		#region long Seek(long offset, SeekOrigin origin)

		/// <summary>
		/// Sets the current position within the stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the origin parameter.</param>
		/// <param name="origin">Indicates the reference point used to obtain the new position.</param>
		/// <returns>The new position within the stream.</returns>
		/// <exception cref="NotSupportedException">The stream does not support seeking.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		public override long Seek(long offset, SeekOrigin origin)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			if (!mCanSeek)
				throw new NotSupportedException("The stream does not support seeking.");

			if (origin == SeekOrigin.Begin)
			{
				if (offset < 0)
				{
					throw new ArgumentException(
						"Position must be positive when seeking from the beginning of the stream.",
						nameof(offset));
				}

				if (offset > mLength)
				{
					throw new ArgumentException(
						"Position exceeds the length of the stream.",
						nameof(offset));
				}

				mPosition = offset;
				mCurrentBlockStartIndex = 0;
				long remaining = mPosition;
				mCurrentBlock = mFirstBlock;

				while (mCurrentBlock != null)
				{
					remaining -= Math.Min(remaining, mCurrentBlock.InternalLength);
					if (remaining == 0) break;
					mCurrentBlockStartIndex += mCurrentBlock.InternalLength;
					mCurrentBlock = mCurrentBlock.InternalNext;
				}
			}
			else if (origin == SeekOrigin.Current)
			{
				if (offset < 0 && -offset > mPosition)
				{
					throw new ArgumentException(
						"The target position is before the start of the stream.",
						nameof(offset));
				}

				if (offset > 0 && offset > mLength - mPosition)
				{
					throw new ArgumentException(
						"The target position is after the end of the stream.",
						nameof(offset));
				}

				long position = mPosition + offset;
				Seek(position, SeekOrigin.Begin);
			}
			else if (origin == SeekOrigin.End)
			{
				if (offset > 0)
				{
					throw new ArgumentException(
						"Position must be negative when seeking from the end of the stream.",
						nameof(offset));
				}

				if (-offset > mLength)
				{
					throw new ArgumentException(
						"Position exceeds the start of the stream.",
						nameof(offset));
				}

				long position = mLength + offset;
				Seek(position, SeekOrigin.Begin);
			}
			else
			{
				throw new ArgumentException(
					"The specified seek origin is invalid.",
					nameof(origin));
			}

			return 0;
		}

		#endregion

		#region int Read(byte[] buffer, int offset, int count)

		/// <summary>
		/// Reads a sequence of bytes from the stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">Buffer receiving data from the stream.</param>
		/// <param name="offset">Offset in the buffer to start reading data to.</param>
		/// <param name="count">Number of bytes to read.</param>
		/// <returns>
		/// The total number of bytes read into the buffer. This can be less than the number of requested bytes,
		/// if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="offset"/> or <paramref name="count"/> is negative.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.
		/// </exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			if (count == 0)
				return 0;

			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));

			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be greater than or equal to 0.");

			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than or equal to 0.");

			if (offset >= buffer.Length)
				throw new ArgumentException("Offset exceeds the end of the buffer.", nameof(offset));

			if (offset + count > buffer.Length)
				throw new ArgumentException("The buffer's length is less than offset + count.", nameof(count));

			int bytesToRead = (int)Math.Min(mLength - mPosition, count);
			Debug.Assert(bytesToRead >= 0);

			int remaining = bytesToRead;
			while (remaining > 0)
			{
				// copy as many bytes as requested and possible
				int index = PrepareReadingBlock(remaining, out int bytesToCopy);
				Array.Copy(mCurrentBlock.InternalBuffer, index, buffer, offset, bytesToCopy);
				offset += bytesToCopy;
				remaining -= bytesToCopy;
				mPosition += bytesToCopy;
			}

			CompleteReadingBlock();
			return bytesToRead;
		}

		#endregion

		#region Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)

		/// <summary>
		/// Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the
		/// number of bytes read, and monitors cancellation requests.
		/// </summary>
		/// <param name="buffer">
		/// The buffer to write the data into.
		/// </param>
		/// <param name="offset">
		/// The byte offset in <paramref name="buffer"/> at which to begin writing data from the stream.
		/// </param>
		/// <param name="count">
		/// The maximum number of bytes to read.
		/// </param>
		/// <param name="cancellationToken">
		/// The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None"/>.
		/// </param>
		/// <returns>
		/// A task that represents the asynchronous read operation.
		/// The value contains the total number of bytes read into the buffer.
		/// The result value can be less than the number of bytes requested if the number of bytes currently available
		/// is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="offset"/> or <paramref name="count"/> is negative.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.
		/// </exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="InvalidOperationException">The stream is currently in use by a previous read operation.</exception>
		public override Task<int> ReadAsync(
			byte[]            buffer,
			int               offset,
			int               count,
			CancellationToken cancellationToken)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			// abort, if cancellation is pending
			if (cancellationToken.IsCancellationRequested)
				return Task.FromCanceled<int>(cancellationToken);

			// The stream is purely in memory, so offloading to another thread doesn't make sense
			// => complete synchronously
			return Task.FromResult(Read(buffer, offset, count));
		}

		#endregion

		#region int Read(Span<byte> buffer)

		/// <summary>
		/// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">
		/// A region of memory.
		/// When this method returns, the contents of this region are replaced by the bytes read from the current source.
		/// </param>
		/// <returns>
		/// The total number of bytes read into the buffer.
		/// This can be less than the number of bytes allocated in the buffer if that many bytes are not currently available,
		/// or zero (0) if the end of the stream has been reached.
		/// </returns>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		public
#if NETSTANDARD2_1 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0
			override
#elif !NETSTANDARD2_0 && !NET461
#error Unhandled target framework.
#endif
			int Read(Span<byte> buffer)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			int bytesToRead = (int)Math.Min(mLength - mPosition, buffer.Length);
			Debug.Assert(bytesToRead >= 0);

			// abort if there is nothing to do...
			if (bytesToRead == 0)
				return 0;

			int offset = 0;
			int remaining = bytesToRead;
			while (remaining > 0)
			{
				int index = PrepareReadingBlock(remaining, out int bytesToCopy);
				var source = new Span<byte>(mCurrentBlock.InternalBuffer, index, bytesToCopy);
				var destination = buffer.Slice(offset, buffer.Length - offset);
				source.CopyTo(destination);
				offset += bytesToCopy;
				remaining -= bytesToCopy;
				mPosition += bytesToCopy;
			}

			CompleteReadingBlock();
			return bytesToRead;
		}

		#endregion

		#region ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)

		/// <summary>
		/// Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the
		/// number of bytes read, and monitors cancellation requests.
		/// </summary>
		/// <param name="buffer">The region of memory to write the data into.</param>
		/// <param name="cancellationToken">
		/// The token to monitor for cancellation requests.
		/// The default value is <see cref="CancellationToken.None"/>.
		/// </param>
		/// <returns>
		/// A task that represents the asynchronous read operation.
		/// The value of its <see cref="ValueTask{T}.Result"/> property contains the total number
		/// of bytes read into the buffer. The result value can be less than the number of bytes allocated in the buffer
		/// if that many bytes are not currently available, or it can be 0 (zero) if the end of the stream has been reached.
		/// </returns>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1 || NET461
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public
#if NETSTANDARD2_1 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1
			override
#endif
			async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			// abort, if cancellation is pending
			cancellationToken.ThrowIfCancellationRequested();

			// The stream is purely in memory, so offloading to another thread doesn't make sense
			// => complete synchronously
			return Read(buffer.Span);
		}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#elif NET5_0
		public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			// abort, if cancellation is pending
			if (cancellationToken.IsCancellationRequested)
				return ValueTask.FromCanceled<int>(cancellationToken);

			// The stream is purely in memory, so offloading to another thread doesn't make sense
			// => complete synchronously
			return ValueTask.FromResult(Read(buffer.Span));
		}
#else
#error Unhandled target framework.
#endif

		#endregion

		#region int ReadByte()

		/// <summary>
		/// Reads a byte from the stream and advances the position within the stream by one byte,
		/// or returns -1 if at the end of the stream.
		/// </summary>
		/// <returns>
		/// The unsigned byte cast to an Int32;
		/// -1, if at the end of the stream.
		/// </returns>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		public override int ReadByte()
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			if (mPosition == mLength)
				return -1; // end of the stream

			// return byte and advance position of the stream
			int index = PrepareReadingBlock(1, out _);
			mPosition++;
			byte value = mCurrentBlock.InternalBuffer[index];
			CompleteReadingBlock();
			return value;
		}

		#endregion

		#region Internal Read Helpers

		/// <summary>
		/// Prepares reading data from the chain of buffers by adjusting the block under read and indices,
		/// so a copy operation can follow to retrieve data from the buffer.
		/// </summary>
		/// <param name="remaining">Number of bytes remaining to be read.</param>
		/// <param name="bytesToCopy">Receives the number of bytes to copy from the current block.</param>
		/// <returns>Index in the current block the block buffer to read starts at.</returns>
		private int PrepareReadingBlock(long remaining, out int bytesToCopy)
		{
			// get index in the current memory block
			int index = (int)(mPosition - mCurrentBlockStartIndex);

			// determine how many bytes can be read from the current memory block
			int bytesToEnd = mCurrentBlock.InternalLength - index;

			if (bytesToEnd == 0)
			{
				// memory block is at its end
				// => continue reading the next memory block
				if (mReleaseReadBlocks)
				{
					// if releasing read blocks is enabled, seeking is not supported
					// => we've read the first block in the chain
					Debug.Assert(mFirstBlock == mCurrentBlock);
					var nextBlock = mCurrentBlock.InternalNext;
					mCurrentBlock.Release();
					mCurrentBlock = nextBlock;
					mCurrentBlockStartIndex = mPosition;

					// as the first block in the chain has been released, the current one becomes the first one
					// (for consistency the stream capacity, length and position remain the same)
					mFirstBlock = mCurrentBlock;
				}
				else
				{
					mCurrentBlock = mCurrentBlock.InternalNext;
					mCurrentBlockStartIndex = mPosition;
				}

				// the caller should have ensured that there is enough data to read!
				Debug.Assert(mCurrentBlock != null);

				// update index in the current memory block
				index = (int)(mPosition - mCurrentBlockStartIndex);
			}

			// copy as many bytes as requested and possible
			bytesToCopy = (int)Math.Min(mCurrentBlock.InternalLength - index, remaining);
			return index;
		}

		/// <summary>
		/// Should be called after a read operation to release the current block, if it has been read completely.
		/// </summary>
		private void CompleteReadingBlock()
		{
			// abort, if the chain of blocks is already empty
			if (mCurrentBlock == null)
				return;

			// get index in the current memory block
			int index = (int)(mPosition - mCurrentBlockStartIndex);

			// determine how many bytes can be read from the current memory block
			int bytesToEnd = mCurrentBlock.InternalLength - index;

			if (bytesToEnd == 0)
			{
				// memory block is at its end
				// => skip to next memory block
				if (mReleaseReadBlocks)
				{
					// if releasing read blocks is enabled, seeking is not supported
					// => we've read the first block in the chain
					Debug.Assert(mFirstBlock == mCurrentBlock);
					var nextBlock = mCurrentBlock.InternalNext;
					mCurrentBlock.Release();
					mCurrentBlock = nextBlock;
					mCurrentBlockStartIndex = mPosition;

					// as the first block in the chain has been released, the current one becomes the first one
					// (for consistency the stream capacity, length and position remain the same)
					mFirstBlock = mCurrentBlock;
				}
				else
				{
					mCurrentBlock = mCurrentBlock.InternalNext;
					mCurrentBlockStartIndex = mPosition;
				}

				// if the chain is empty now, adjust the last block
				if (mFirstBlock == null) mLastBlock = null;
			}
		}

		#endregion

		#region void Write(byte[] buffer, int offset, int count)

		/// <summary>
		/// Writes a sequence of bytes to the stream and advances the position within this stream by the number of
		/// bytes written.
		/// </summary>
		/// <param name="buffer">The buffer to write to the stream.</param>
		/// <param name="offset">
		/// The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.
		/// </param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="offset"/> or <paramref name="count"/> is negative.
		/// </exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			if (count == 0)
				return;

			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));

			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be greater than or equal to 0.");

			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than or equal to 0.");

			if (offset >= buffer.Length)
				throw new ArgumentException("The offset exceeds the end of the buffer.", nameof(offset));

			if (offset + count > buffer.Length)
				throw new ArgumentException("The sum of offset + count is greater than the buffer's length.", nameof(count));

			// write data to the stream
			int bytesRemaining = count;
			while (bytesRemaining > 0)
			{
				// copy as many bytes as requested and possible
				int index = PrepareWritingBlock(out int bytesToEndOfBlock);
				int bytesToCopy = Math.Min(bytesToEndOfBlock, bytesRemaining);
				Array.Copy(buffer, offset, mCurrentBlock.InternalBuffer, index, bytesToCopy);
				offset += bytesToCopy;
				bytesRemaining -= bytesToCopy;
				mPosition += bytesToCopy;

				// update length of the memory block
				mCurrentBlock.InternalLength = Math.Max(mCurrentBlock.InternalLength, index + bytesToCopy);
			}

			mLength = Math.Max(mLength, mPosition);
		}

		#endregion

		#region void WriteByte(byte value)

		/// <summary>
		/// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
		/// </summary>
		/// <param name="value">The byte to write to the stream.</param>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		public override void WriteByte(byte value)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			int index = PrepareWritingBlock(out _);
			mCurrentBlock.InternalBuffer[index] = value;
			mCurrentBlock.InternalLength = Math.Max(mCurrentBlock.InternalLength, index + 1);
			mPosition++;
			mLength = Math.Max(mLength, mPosition);
		}

		#endregion

		#region void Write(ReadOnlySpan<byte> buffer)

		/// <summary>
		/// Writes a sequence of bytes to the current stream and advances the current position within this stream by the
		/// number of bytes written.
		/// </summary>
		/// <param name="buffer">A region of memory. This method copies the contents of this region to the current stream.</param>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		public
#if NETSTANDARD2_1 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0
			override
#elif !NETSTANDARD2_0 && !NET461
#error Unhandled target framework.
#endif
			void Write(ReadOnlySpan<byte> buffer)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			if (buffer.Length == 0)
				return;

			// write data to the stream
			int bytesRemaining = buffer.Length;
			int offset = 0;
			while (bytesRemaining > 0)
			{
				// copy as many bytes as requested and possible
				int index = PrepareWritingBlock(out int bytesToEndOfBlock);
				int bytesToCopy = Math.Min(bytesToEndOfBlock, bytesRemaining);
				var source = buffer.Slice(offset, bytesToCopy);
				var destination = new Span<byte>(mCurrentBlock.InternalBuffer, index, bytesToCopy);
				source.CopyTo(destination);
				bytesRemaining -= bytesToCopy;
				offset += bytesToCopy;
				mPosition += bytesToCopy;

				// update length of the memory block
				mCurrentBlock.InternalLength = Math.Max(mCurrentBlock.InternalLength, index + bytesToCopy);
			}

			mLength = Math.Max(mLength, mPosition);
		}

		#endregion

		#region Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)

		/// <summary>
		/// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream
		/// by the number of bytes written, and monitors cancellation requests.
		/// </summary>
		/// <param name="buffer">The buffer to write data from.</param>
		/// <param name="offset">
		/// The zero-based byte offset in <paramref name="buffer"/> from which to begin copying bytes to the stream.
		/// </param>
		/// <param name="count">The maximum number of bytes to write.</param>
		/// <param name="cancellationToken">
		/// The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None"/>.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="buffer"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="offset"/> or <paramref name="count"/> is negative.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.
		/// </exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="InvalidOperationException">The stream is currently in use by a previous write operation.</exception>
		/// <returns>A task that represents the asynchronous write operation.</returns>
		public override Task WriteAsync(
			byte[]            buffer,
			int               offset,
			int               count,
			CancellationToken cancellationToken)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			// abort, if cancellation is pending
			if (cancellationToken.IsCancellationRequested)
				return Task.FromCanceled<int>(cancellationToken);

			// The stream is purely in memory, so offloading to another thread doesn't make sense
			// => complete synchronously
			Write(buffer, offset, count);
			return Task.CompletedTask;
		}

		#endregion

		#region ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)

		/// <summary>
		/// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream
		/// by the number of bytes written, and monitors cancellation requests.
		/// </summary>
		/// <param name="buffer">The region of memory to write data from.</param>
		/// <param name="cancellationToken">
		/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
		/// </param>
		/// <returns>A task that represents the asynchronous write operation.</returns>
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1 || NET461
		public
#if NETSTANDARD2_1 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1
			override
#endif
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
			async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			// abort, if cancellation is pending
			cancellationToken.ThrowIfCancellationRequested();

			// The stream is purely in memory, so offloading to another thread doesn't make sense
			// => complete synchronously
			Write(buffer.Span);
		}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#elif NET5_0
		public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			// abort, if cancellation is pending
			if (cancellationToken.IsCancellationRequested)
				return ValueTask.FromCanceled(cancellationToken);

			// The stream is purely in memory, so offloading to another thread doesn't make sense
			// => complete synchronously
			Write(buffer.Span);
			return ValueTask.CompletedTask;
		}
#else
#error Unhandled target framework.
#endif

		#endregion

		#region long Write(Stream stream)

		/// <summary>
		/// Writes a sequence of bytes to the stream and advances the position within this stream by the number
		/// of bytes written.
		/// </summary>
		/// <param name="stream">Stream containing data to write.</param>
		/// <returns>Number of written bytes.</returns>
		/// <exception cref="ArgumentNullException">Specified stream is a null.</exception>
		public long Write(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));

			// try to determine the size of the stream
			long bytesInSourceStream = -1;
			if (stream.CanSeek)
			{
				bytesInSourceStream = stream.Length - stream.Position;
			}

			// abort, if the source stream is empty
			if (bytesInSourceStream == 0)
				return 0;

			// write data to the stream
			long count = 0;
			while (true)
			{
				// copy as many bytes as requested and possible
				int index = PrepareWritingBlock(out int bytesToEndOfBlock);
				int bytesRead = stream.Read(mCurrentBlock.InternalBuffer, index, bytesToEndOfBlock);
				mPosition += bytesRead;
				count += bytesRead;

				// update length of the memory block
				mCurrentBlock.InternalLength = Math.Max(mCurrentBlock.InternalLength, index + bytesRead);

				if (bytesRead < bytesToEndOfBlock) break;
			}

			mLength = Math.Max(mLength, mPosition);
			return count;
		}

		#endregion

		#region ValueTask<long> WriteAsync(Stream stream, CancellationToken cancellationToken)

		/// <summary>
		/// Writes a sequence of bytes to the stream and advances the position within this stream by the number
		/// of bytes written.
		/// </summary>
		/// <param name="stream">Stream containing data to write.</param>
		/// <param name="cancellationToken">
		/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
		/// </param>
		/// <returns>Number of written bytes.</returns>
		/// <exception cref="ArgumentNullException">Specified stream is a null.</exception>
		public async ValueTask<long> WriteAsync(Stream stream, CancellationToken cancellationToken = default)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));

			// try to determine the size of the stream
			long bytesInSourceStream = stream.CanSeek ? stream.Length - stream.Position : -1;

			// abort, if the source stream is empty
			if (bytesInSourceStream == 0)
				return 0;

			// write data to the stream
			long count = 0;
			while (true)
			{
				// copy as many bytes as requested and possible
				int index = PrepareWritingBlock(out int bytesToEndOfBlock);

				int bytesRead = await stream.ReadAsync(
						                mCurrentBlock.InternalBuffer,
						                index,
						                bytesToEndOfBlock,
						                cancellationToken)
					                .ConfigureAwait(false);

				mPosition += bytesRead;
				count += bytesRead;

				// update length of the memory block
				mCurrentBlock.InternalLength = Math.Max(mCurrentBlock.InternalLength, index + bytesRead);

				if (bytesRead < bytesToEndOfBlock) break;
			}

			mLength = Math.Max(mLength, mPosition);
			return count;
		}

		#endregion

		#region Internal Write Helpers

		/// <summary>
		/// Prepares writing data to the chain of buffers by adjusting the block under write and indices,
		/// so a copy operation can follow to copy data into the buffer.
		/// </summary>
		/// <param name="bytesToEndOfBlock">
		/// Receives the number of bytes from the current position to the end of the block to fill.
		/// </param>
		/// <returns>Index in the current block the block buffer to write starts at.</returns>
		private int PrepareWritingBlock(out int bytesToEndOfBlock)
		{
			// get index in the current memory block
			int index = (int)(mPosition - mCurrentBlockStartIndex);

			// determine how many bytes can be written to the current memory block
			bytesToEndOfBlock = 0;
			if (mCurrentBlock != null)
			{
				bytesToEndOfBlock = mCurrentBlock.InternalNext != null
					                    ? mCurrentBlock.InternalLength - index
					                    : mCurrentBlock.Capacity - index;
			}

			if (bytesToEndOfBlock == 0)
			{
				// memory block is at its end
				// => continue writing the next memory block
				if (mCurrentBlock?.InternalNext != null)
				{
					mCurrentBlock = mCurrentBlock.InternalNext;
					bytesToEndOfBlock = mCurrentBlock.InternalLength;
				}
				else
				{
					if (!AppendNewBuffer()) mCurrentBlock = mCurrentBlock.InternalNext;
					Debug.Assert(mCurrentBlock != null, nameof(mCurrentBlock) + " != null");
					bytesToEndOfBlock = mCurrentBlock.Capacity;
				}

				mCurrentBlockStartIndex = mPosition;
				index = 0;
			}

			return index;
		}

		/// <summary>
		/// Appends a new buffer to the end of the chain.
		/// </summary>
		/// <returns>
		/// true, if the first created new buffer is the first buffer;
		/// false, if the first created new buffer is not the first buffer.
		/// </returns>
		private bool AppendNewBuffer()
		{
			bool isFirstBuffer = mFirstBlock == null;

			if (mFirstBlock == null)
			{
				mCurrentBlock = new ChainableMemoryBlock(mBlockSize, mArrayPool, false);
				mFirstBlock = mCurrentBlock;
				mLastBlock = mCurrentBlock;
				mCapacity = mCurrentBlock.Capacity;
			}
			else
			{
				var block = new ChainableMemoryBlock(mBlockSize, mArrayPool, false);
				mLastBlock.InternalNext = block;
				mLastBlock = block;
				mCapacity += block.Capacity;
			}

			return isFirstBuffer;
		}

		#endregion

		#region void CopyTo(Stream destination, int bufferSize)

		/// <summary>
		/// Reads the bytes from the current stream and writes them to another stream, using a specified buffer size.
		/// </summary>
		/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
		/// <param name="bufferSize">The size of the buffer. This value must be greater than zero. The default size is 81920.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="destination"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="bufferSize"/> is negative or zero.
		/// </exception>
		/// <exception cref="NotSupportedException"><paramref name="destination"/> does not support writing.</exception>
		/// <exception cref="ObjectDisposedException">
		/// Either the current stream or <paramref name="destination"/> have been disposed.
		/// </exception>
		public
#if NETSTANDARD2_1 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0
			override
#elif NETSTANDARD2_0 || NET461
			new
#endif
			void CopyTo(Stream destination, int bufferSize)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			long bytesToRead = mLength - mPosition;
			Debug.Assert(bytesToRead >= 0);
			long remaining = bytesToRead;
			while (remaining > 0)
			{
				// copy as many bytes as requested and possible
				int index = PrepareReadingBlock(remaining, out int bytesToCopy);
				destination.Write(mCurrentBlock.InternalBuffer, index, bytesToCopy);
				remaining -= bytesToCopy;
				mPosition += bytesToCopy;
			}

			CompleteReadingBlock();
		}

		#endregion

		#region Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)

		/// <summary>
		/// Asynchronously reads the bytes from the current stream and writes them to another stream, using a specified
		/// buffer size and cancellation token.
		/// </summary>
		/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
		/// <param name="bufferSize">
		/// The size of the buffer (in bytes).
		/// This value must be greater than zero.
		/// The default size is 81920.
		/// </param>
		/// <param name="cancellationToken">
		/// The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None"/>.
		/// </param>
		/// <exception cref="ArgumentNullException"> <paramref name="destination"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"> <paramref name="bufferSize"/> is negative or zero.</exception>
		/// <exception cref="ObjectDisposedException">Either the current stream or the destination stream is disposed.</exception>
		/// <exception cref="NotSupportedException">The destination stream does not support writing.</exception>
		/// <returns>A task that represents the asynchronous copy operation.</returns>
		public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			// abort, if cancellation is pending
			cancellationToken.ThrowIfCancellationRequested();

			long bytesToRead = mLength - mPosition;
			Debug.Assert(bytesToRead >= 0);
			long remaining = bytesToRead;
			while (remaining > 0)
			{
				// copy as many bytes as requested and possible
				int index = PrepareReadingBlock(remaining, out int bytesToCopy);
				await destination.WriteAsync(mCurrentBlock.InternalBuffer, index, bytesToCopy, cancellationToken).ConfigureAwait(false);
				remaining -= bytesToCopy;
				mPosition += bytesToCopy;
			}

			CompleteReadingBlock();
		}

		#endregion

		#region Flushing

		/// <summary>
		/// Flushes the stream (does not do anything for this stream).
		/// </summary>
		public override void Flush()
		{
		}

		/// <summary>
		/// Flushes the stream asynchronously (does not do anything for this stream).
		/// </summary>
		/// <param name="cancellationToken">Cancellation token that can be signaled to cancel the operation.</param>
		public override Task FlushAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		#endregion

		#region Attaching / Detaching Buffers

		/// <summary>
		/// Appends a memory block or chain of memory blocks to the stream.
		/// </summary>
		/// <param name="buffer">Memory block to append to the stream.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="buffer"/> argument is <c>null</c>.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <remarks>
		/// The specified buffer must not be accessed directly after this operation.
		/// The stream takes care of returning buffers to their array pool, if necessary.
		/// </remarks>
		public void AppendBuffer(ChainableMemoryBlock buffer)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));

			if (mLastBlock != null)
			{
				// adjust capacity to exclude the remaining space of the last block, if the new blocks contain data
				// (in this case the free space in the last block cannot be used as more data is already following in the new blocks)
				if (buffer.Length > 0 || buffer.ChainLength > 0) // in most cases checking the first buffer should suffice and avoid walking the entire chain
				{
					mCapacity -= mLastBlock.Capacity - mLastBlock.InternalLength;
				}

				// the stream already contains data
				// => append blocks at the end
				mLastBlock.InternalNext = buffer;

				// adjust length and capacity
				var block = buffer;
				while (block != null)
				{
					mLength += block.InternalLength;
					mCapacity += block.InternalLength;
					mLastBlock = block;
					block = block.InternalNext;
				}

				// adjust the capacity of the now last block, since only the last block can be extended up to its capacity
				// (the blocks in between must not be changed in length, since this would insert/remove data in the middle of the stream!)
				if (mLastBlock != null)
				{
					mCapacity = mCapacity - mLastBlock.InternalLength + mLastBlock.Capacity;
				}
			}
			else
			{
				// stream is empty
				// => the buffer becomes the only buffer backing the stream

				// exchange buffer
				mCurrentBlock = buffer;
				mFirstBlock = buffer;

				// update administrative variables appropriately
				mCurrentBlockStartIndex = 0;
				mPosition = 0;

				// determine capacity and length of the buffer
				mFirstBlock = buffer;
				mCapacity = 0;
				mLength = 0;
				var block = mFirstBlock;
				while (block != null)
				{
					mLength += block.InternalLength;
					mCapacity += block.InternalLength;
					mLastBlock = block;
					block = block.InternalNext;
				}

				// adjust the capacity, since only the last block must be extended up to its capacity
				// (the blocks in between must not be changed in length, since this would insert/remove data in the middle of the stream!)
				if (mLastBlock != null)
				{
					mCapacity = mCapacity - mLastBlock.InternalLength + mLastBlock.Capacity;
				}
			}
		}

		/// <summary>
		/// Attaches a memory block or chain of memory blocks to the stream.
		/// </summary>
		/// <param name="buffer">Memory block to attach to the stream (null to clear the stream).</param>
		/// <exception cref="ArgumentNullException">The <paramref name="buffer"/> argument is <c>null</c>.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <remarks>
		/// This method allows you to exchange the underlying memory block buffer.
		/// The stream is reset, so the position is 0 after attaching the new buffer.
		/// The specified buffer must not be accessed directly after this operation.
		/// The stream takes care of returning buffers to their array pool, if necessary.
		/// </remarks>
		public void AttachBuffer(ChainableMemoryBlock buffer)
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			// exchange buffer
			mFirstBlock = buffer;

			// update administrative variables appropriately
			mCurrentBlock = mFirstBlock;
			mCurrentBlockStartIndex = 0;
			mPosition = 0;

			// determine capacity and length of the buffer
			mCapacity = 0;
			mLength = 0;
			var block = mFirstBlock;
			while (block != null)
			{
				mCapacity += block.InternalLength;
				mLength += block.InternalLength;
				mLastBlock = block;
				block = block.InternalNext;
			}

			// adjust the capacity, since only the last block must be extended up to its capacity
			// (the blocks in between must not be changed in length, since this would insert/remove data in the middle of the stream!)
			if (mLastBlock != null)
			{
				mCapacity = mCapacity - mLastBlock.InternalLength + mLastBlock.Capacity;
			}
		}

		/// <summary>
		/// Detaches the underlying memory block buffer from the stream.
		/// </summary>
		/// <returns>Underlying memory block buffer (can be a chained with other memory blocks).</returns>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <remarks>
		/// This method allows you to detach the underlying buffer from the stream and use it in another place.
		/// If blocks contain buffers that have been rented from an array pool, the returned block chain must
		/// be disposed to return buffers to the pool. The stream is empty afterwards.
		/// </remarks>
		public ChainableMemoryBlock DetachBuffer()
		{
			if (mDisposed)
				throw new ObjectDisposedException(nameof(MemoryBlockStream));

			var buffer = mFirstBlock;
			mFirstBlock = null;
			mCurrentBlock = null;
			mLastBlock = null;
			mCurrentBlockStartIndex = 0;
			mPosition = 0;
			mCapacity = 0;
			mLength = 0;
			return buffer;
		}

		#endregion

		#region Access Points for Unit Tests

		/// <summary>
		/// Gets a value indicating whether the stream releases read buffers.
		/// </summary>
		// ReSharper disable once ConvertToAutoPropertyWhenPossible
		internal bool ReleaseReadBlocks => mReleaseReadBlocks;

		#endregion
	}

}
