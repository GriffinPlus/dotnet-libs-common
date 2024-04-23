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

namespace GriffinPlus.Lib.Io;

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
public sealed class MemoryBlockStream : Stream, IMemoryBlockStream
{
	/// <summary>
	/// Default size of block in the stream.
	/// 80 kByte is small enough for the regular heap and avoids allocation on the large object heap.
	/// </summary>
	internal const int DefaultBlockSize = 80 * 1024;

	private          long                 mPosition;
	private          long                 mLength;
	private readonly int                  mBlockSize;
	private          long                 mCurrentBlockStartIndex;
	private          long                 mFirstBlockOffset;
	private readonly ArrayPool<byte>      mArrayPool;
	private          ChainableMemoryBlock mFirstBlock;
	private          ChainableMemoryBlock mCurrentBlock;
	private          ChainableMemoryBlock mLastBlock;
	private          bool                 mDisposed;

	#region Construction and Disposal

	/// <summary>
	/// Initializes a new instance of the <see cref="MemoryBlockStream"/> class.
	/// Buffers are allocated on the heap.
	/// The block size defaults to 80 kByte.
	/// The stream is seekable and grows as data is written.
	/// </summary>
	public MemoryBlockStream() : this(DefaultBlockSize, null, false) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="MemoryBlockStream"/> class.
	/// Buffers are rented from the specified array pool.
	/// The block size defaults to 80 kByte.
	/// The stream is seekable and grows as data is written.
	/// </summary>
	/// <param name="pool">Array pool to use for allocating buffers.</param>
	/// <exception cref="ArgumentNullException"><paramref name="pool"/> is <c>null</c>.</exception>
	public MemoryBlockStream(ArrayPool<byte> pool) : this(DefaultBlockSize, pool, false) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="MemoryBlockStream"/> class with a specific block size.
	/// Buffers are allocated on the heap.
	/// The stream is seekable and grows as data is written.
	/// </summary>
	/// <param name="blockSize">Size of a block in the stream.</param>
	/// <exception cref="ArgumentException">The specified block size is less than or equal to 0.</exception>
	public MemoryBlockStream(int blockSize) : this(blockSize, null, false) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="MemoryBlockStream"/> class with a specific block size.
	/// Buffers can be allocated on the heap or rented from the specified array pool.
	/// The stream can be configured to release buffers as data is read (makes the stream unseekable).
	/// </summary>
	/// <param name="blockSize">
	/// Size of a block in the stream.
	/// The actual block size may be greater than the specified size, if the buffer is rented from an array pool.
	/// </param>
	/// <param name="pool">
	/// Array pool to rent buffers from (<c>null</c> to allocate buffers on the heap).
	/// </param>
	/// <param name="releasesReadBlocks">
	/// <c>true</c> to release memory blocks that have been read (makes the stream unseekable);
	/// <c>false</c> to keep written memory blocks enabling seeking and changing the length of the stream.
	/// </param>
	/// <exception cref="ArgumentException">The specified block size is less than or equal to 0.</exception>
	public MemoryBlockStream(
		int             blockSize          = DefaultBlockSize,
		ArrayPool<byte> pool               = null,
		bool            releasesReadBlocks = false)
	{
		if (blockSize <= 0)
		{
			throw new ArgumentException(
				"The block size must be greater than 0.",
				nameof(blockSize));
		}

		mArrayPool = pool;
		mBlockSize = blockSize;
		ReleasesReadBlocks = releasesReadBlocks;
		CanSeek = !releasesReadBlocks;
		mFirstBlockOffset = 0;
		mCurrentBlockStartIndex = 0;
		mLength = 0;
		mPosition = 0;
	}

	/// <summary>
	/// Disposes the stream releasing the underlying memory-block chain
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
			DisposeInternal();
		}
	}

#if NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
	/// <summary>
	/// Asynchronously disposes the stream releasing the underlying memory-block chain
	/// (returns rented buffers to their array pool, if necessary).
	/// </summary>
	public override ValueTask DisposeAsync()
	{
		DisposeInternal();
		return default;
	}
#elif NETSTANDARD2_0 || NET461 || NET48
	// This method is not supported by the Stream class.
#else
#error Unhandled target framework.
#endif

	/// <summary>
	/// Disposes the stream releasing the underlying memory-block chain
	/// (returns rented buffers to their array pool, if necessary; for internal use only).
	/// </summary>
	private void DisposeInternal()
	{
		if (mFirstBlock != null)
		{
			mFirstBlock?.ReleaseChain();
			mFirstBlock = null;
			mCurrentBlock = null;
			mLastBlock = null;
			mFirstBlockOffset = 0;
			mCurrentBlockStartIndex = 0;
			mLength = 0;
			mPosition = 0;
		}

		mDisposed = true;
	}

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
	public override bool CanSeek { get; }

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
	// ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
	public override long Length => mLength;

	#endregion

	#region ReleasesReadBlocks

	/// <summary>
	/// Gets a value indicating whether the stream releases read buffers.
	/// </summary>
	public bool ReleasesReadBlocks { get; }

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
		if (length < 0)
			throw new ArgumentException("The length must not be negative.", nameof(length));

		if (!CanSeek)
			throw new NotSupportedException("The stream does not support seeking.");

		if (mDisposed)
			throw new ObjectDisposedException(nameof(MemoryBlockStream));

		// determine the capacity of the chain of memory blocks backing the stream
		long capacity = mLastBlock != null
			                ? mLength + mLastBlock.Capacity - mLastBlock.Length
			                : 0;

		if (length > capacity)
		{
			// requested size is greater than the current capacity of the stream
			// => enlarge buffer by adding new memory blocks

			// determine the number of bytes to allocate additionally
			long additionallyNeededSpace = length - capacity;
			long lengthOfLastBlock = length - capacity;
			while (true)
			{
				ChainableMemoryBlock newBlock = ChainableMemoryBlock.GetPooled(mBlockSize, mArrayPool, true); // initializes the buffer with zeros
				if (mFirstBlock == null)
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
					Debug.Assert(mLastBlock != null, nameof(mLastBlock) + " != null");
					mLastBlock.Length = mLastBlock.Capacity;
					mLastBlock.Next = newBlock;
					mLastBlock = newBlock;
				}

				// abort, if needed space has been allocated
				additionallyNeededSpace -= newBlock.Capacity;
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
				mFirstBlockOffset = 0;
				mCurrentBlockStartIndex = 0;
				mLength = 0;
				mPosition = 0;
			}
			else
			{
				// the stream will contain data up to the capacity of the stream
				// (the stream might even get longer if the capacity allows that)
				// => release memory blocks that are not needed any more
				long remaining = length;
				long lastBlockStartIndex = 0;
				ChainableMemoryBlock block = mFirstBlock;
				while (true)
				{
					Debug.Assert(block != null, nameof(block) + " != null");
					remaining -= Math.Min(remaining, block.Next != null ? block.Length : block.Capacity);
					if (remaining == 0) break;
					lastBlockStartIndex += block.Length;
					block = block.Next;
				}

				block.Next?.ReleaseChain();
				block.Next = null;
				mLastBlock = block;
				mLength = length;

				// clear all bytes up to the end of the last block
				int startIndex = Math.Min(mLastBlock.Length, (int)(length - lastBlockStartIndex));
				int bytesToClear = (mLastBlock.Capacity - startIndex);
				Array.Clear(mLastBlock.Buffer, startIndex, bytesToClear);
				mLastBlock.Length = (int)(length - lastBlockStartIndex);

				// adjust stream position, if the position is out of bounds now
				if (mPosition < mLength) return;
				mPosition = mLength;
				mCurrentBlock = mLastBlock;
				mCurrentBlockStartIndex = lastBlockStartIndex;
				Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);
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

		if (!CanSeek)
			throw new NotSupportedException("The stream does not support seeking.");

		switch (origin)
		{
			case SeekOrigin.Begin when offset < 0:
				throw new ArgumentException(
					"Position must be positive when seeking from the beginning of the stream.",
					nameof(offset));

			case SeekOrigin.Begin when offset > mLength:
				throw new ArgumentException(
					"Position exceeds the length of the stream.",
					nameof(offset));

			case SeekOrigin.Begin:
			{
				mPosition = offset;
				mCurrentBlockStartIndex = 0;
				long remaining = mPosition;
				mCurrentBlock = mFirstBlock;

				while (mCurrentBlock != null)
				{
					remaining -= Math.Min(remaining, mCurrentBlock.Length);
					if (remaining == 0) break;
					mCurrentBlockStartIndex += mCurrentBlock.Length;
					mCurrentBlock = mCurrentBlock.Next;
					Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);
				}
				break;
			}

			case SeekOrigin.Current when offset < 0 && -offset > mPosition:
				throw new ArgumentException(
					"The target position is before the start of the stream.",
					nameof(offset));

			case SeekOrigin.Current when offset > 0 && offset > mLength - mPosition:
				throw new ArgumentException(
					"The target position is after the end of the stream.",
					nameof(offset));

			case SeekOrigin.Current:
			{
				mPosition += offset;
				mCurrentBlockStartIndex = 0;
				long remaining = mPosition;
				mCurrentBlock = mFirstBlock;

				while (mCurrentBlock != null)
				{
					remaining -= Math.Min(remaining, mCurrentBlock.Length);
					if (remaining == 0) break;
					mCurrentBlockStartIndex += mCurrentBlock.Length;
					mCurrentBlock = mCurrentBlock.Next;
					Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);
				}
				break;
			}

			case SeekOrigin.End when offset > 0:
				throw new ArgumentException(
					"Position must be negative when seeking from the end of the stream.",
					nameof(offset));

			case SeekOrigin.End when -offset > mLength:
				throw new ArgumentException(
					"Position exceeds the start of the stream.",
					nameof(offset));

			case SeekOrigin.End:
			{
				if (mLength > 0)
				{
					long targetPosition = mLength + offset;
					mPosition = mLength + offset;
					mCurrentBlock = mLastBlock;
					mCurrentBlockStartIndex = mLength - mCurrentBlock.Length;
					Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);
					while (mPosition != targetPosition)
					{
						if (targetPosition > mCurrentBlockStartIndex)
						{
							mPosition -= mCurrentBlock.Length;
							mCurrentBlock = mCurrentBlock.Previous;
							mCurrentBlockStartIndex = mPosition - mCurrentBlock.Length;
							Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);
						}
						else
						{
							mPosition = targetPosition;
						}
					}
				}
				break;
			}

			default:
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
		if (buffer == null)
			throw new ArgumentNullException(nameof(buffer));

		if (offset < 0)
			throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be greater than or equal to 0.");

		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than or equal to 0.");

		if (offset + count > buffer.Length)
			throw new ArgumentException("The buffer's length is less than offset + count.", nameof(count));

		return ReadInternal(buffer, offset, count);
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
	/// The token to monitor for cancellation requests.
	/// The default value is <see cref="CancellationToken.None"/>.
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
		CancellationToken cancellationToken) // overload without CancellationToken is defined in base class
	{
		if (buffer == null)
			throw new ArgumentNullException(nameof(buffer));

		if (offset < 0)
			throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be greater than or equal to 0.");

		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than or equal to 0.");

		if (offset + count > buffer.Length)
			throw new ArgumentException("The buffer's length is less than offset + count.", nameof(count));

		// abort, if cancellation is pending
		return cancellationToken.IsCancellationRequested
			       ? Task.FromException<int>(new OperationCanceledException(cancellationToken))
			       : Task.FromResult(ReadInternal(buffer, offset, count));
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
#if NETSTANDARD2_0 || NET461 || NET48
#elif NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
		override
#else
#error Unhandled target framework.
#endif
		int Read(Span<byte> buffer)
	{
		return ReadInternal(buffer);
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
	public
#if NETSTANDARD2_0 || NET461 || NET48
#elif NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
		override
#else
#error Unhandled target framework.
#endif
		ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
	{
		// abort, if cancellation is pending
		return cancellationToken.IsCancellationRequested
			       ? new ValueTask<int>(Task.FromException<int>(new OperationCanceledException(cancellationToken)))
			       : new ValueTask<int>(ReadInternal(buffer.Span));
	}

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
		int index = PrepareReadingBlock(1, out int _);
		mPosition++;
		byte value = mCurrentBlock.Buffer[index];
		return value;
	}

	#endregion

	#region Internal Read Helpers

	/// <summary>
	/// Common implementation for reading into an array of <see cref="byte"/>.
	/// </summary>
	/// <param name="buffer">Array to put read data into.</param>
	/// <param name="offset">Offset in the array to start at.</param>
	/// <param name="count">Maximum number of bytes to read.</param>
	/// <returns>Number of read bytes; 0, if the end of the stream is reached.</returns>
	private int ReadInternal(byte[] buffer, int offset, int count)
	{
		if (mDisposed)
			throw new ObjectDisposedException(nameof(MemoryBlockStream));

		int bytesToRead = (int)Math.Min(mLength - mPosition, count);
		Debug.Assert(bytesToRead >= 0);

		// abort if there is nothing to do...
		if (bytesToRead == 0)
			return 0;

		int remaining = bytesToRead;
		while (remaining > 0)
		{
			// copy as many bytes as requested and possible
			int index = PrepareReadingBlock(remaining, out int bytesToCopy);
			Array.Copy(mCurrentBlock.Buffer, index, buffer, offset, bytesToCopy);
			offset += bytesToCopy;
			remaining -= bytesToCopy;
			mPosition += bytesToCopy;
		}

		return bytesToRead;
	}

	/// <summary>
	/// Common implementation for reading into a <see cref="Span{T}"/>.
	/// </summary>
	/// <param name="buffer">Buffer to put read data into.</param>
	/// <returns>Number of read bytes; 0, if the end of the stream is reached.</returns>
	private int ReadInternal(Span<byte> buffer)
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
			var source = new Span<byte>(mCurrentBlock.Buffer, index, bytesToCopy);
			Span<byte> destination = buffer.Slice(offset, buffer.Length - offset);
			source.CopyTo(destination);
			offset += bytesToCopy;
			remaining -= bytesToCopy;
			mPosition += bytesToCopy;
		}

		return bytesToRead;
	}

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
		int bytesToEnd = mCurrentBlock.Length - index;

		if (bytesToEnd == 0)
		{
			// memory block is at its end
			// => continue reading the next memory block
			if (ReleasesReadBlocks)
			{
				// if releasing read blocks is enabled, seeking is not supported
				// => we've read the first block in the chain
				Debug.Assert(mFirstBlock == mCurrentBlock);
				ChainableMemoryBlock nextBlock = mCurrentBlock.Next;
				mFirstBlockOffset += mCurrentBlock.Length;
				mCurrentBlock.Next = null;
				mCurrentBlock.Release();
				mCurrentBlock = nextBlock;
				mCurrentBlockStartIndex = mPosition;
				Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);

				// as the first block in the chain has been released, the current one becomes the first one
				// (for consistency the stream length and position remain the same)
				mFirstBlock = mCurrentBlock;
			}
			else
			{
				mCurrentBlock = mCurrentBlock.Next;
				mCurrentBlockStartIndex = mPosition;
				Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);
			}

			// the caller should have ensured that there is enough data to read!
			Debug.Assert(mCurrentBlock != null);

			// update index in the current memory block
			index = (int)(mPosition - mCurrentBlockStartIndex);
		}

		// copy as many bytes as requested and possible
		bytesToCopy = (int)Math.Min(mCurrentBlock.Length - index, remaining);
		return index;
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
		if (buffer == null)
			throw new ArgumentNullException(nameof(buffer));

		if (offset < 0)
			throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be greater than or equal to 0.");

		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than or equal to 0.");

		if (count > 0 && offset >= buffer.Length)
			throw new ArgumentException("The offset exceeds the end of the buffer.", nameof(offset));

		if (offset + count > buffer.Length)
			throw new ArgumentException("The sum of offset + count is greater than the buffer's length.", nameof(count));

		WriteInternal(buffer, offset, count);
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
	/// The token to monitor for cancellation requests.
	/// The default value is <see cref="CancellationToken.None"/>.
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
		CancellationToken cancellationToken) // overload without CancellationToken is defined in base class
	{
		if (buffer == null)
			throw new ArgumentNullException(nameof(buffer));

		if (offset < 0)
			throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be greater than or equal to 0.");

		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than or equal to 0.");

		if (count > 0 && offset >= buffer.Length)
			throw new ArgumentException("The offset exceeds the end of the buffer.", nameof(offset));

		if (offset + count > buffer.Length)
			throw new ArgumentException("The sum of offset + count is greater than the buffer's length.", nameof(count));

		// abort, if cancellation is pending
		if (cancellationToken.IsCancellationRequested)
			return Task.FromException(new OperationCanceledException(cancellationToken));

		WriteInternal(buffer, offset, count);
		return Task.CompletedTask;
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
#if NETSTANDARD2_0 || NET461 || NET48
#elif NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
		override
#else
#error Unhandled target framework.
#endif
		void Write(ReadOnlySpan<byte> buffer)
	{
		WriteInternal(buffer);
	}

	#endregion

	#region ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)

	/// <summary>
	/// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream
	/// by the number of bytes written, and monitors cancellation requests.
	/// </summary>
	/// <param name="buffer">The region of memory to write data from.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests.
	/// The default value is <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A task that represents the asynchronous write operation.</returns>
	public
#if NETSTANDARD2_0 || NET461 || NET48
#elif NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
		override
#else
#error Unhandled target framework.
#endif
		ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
	{
		// abort, if cancellation is pending
		if (cancellationToken.IsCancellationRequested)
			return new ValueTask(Task.FromException<int>(new OperationCanceledException(cancellationToken)));

		WriteInternal(buffer.Span);
		return default;
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
		int index = PrepareWritingBlock(out int _);
		mCurrentBlock.Buffer[index] = value;
		mCurrentBlock.Length = Math.Max(mCurrentBlock.Length, index + 1);
		mPosition++;
		mLength = Math.Max(mLength, mPosition);
	}

	#endregion

	#region long Write(Stream stream)

	/// <summary>
	/// Writes a sequence of bytes to the stream and advances the position within this stream by the number
	/// of bytes written.
	/// </summary>
	/// <param name="stream">Stream containing data to write.</param>
	/// <returns>Number of written bytes.</returns>
	/// <exception cref="ArgumentNullException">Specified stream is a null.</exception>
	/// <exception cref="NotSupportedException">The source stream does not support reading.</exception>
	public long Write(Stream stream)
	{
		if (stream == null)
			throw new ArgumentNullException(nameof(stream));

		if (!stream.CanRead)
			throw new NotSupportedException("The source stream does not support reading.");

		// abort, if the source stream is empty
		long bytesInSourceStream = stream.CanSeek ? stream.Length - stream.Position : -1;
		if (bytesInSourceStream == 0)
			return 0;

		// abort, if the stream has been disposed
		if (mDisposed)
			throw new ObjectDisposedException(nameof(MemoryBlockStream));

		// read stream into a new blocks that can be appended to the current stream later on
		// (avoid blocking the current stream as long as the read operation is in progress)
		long count = 0;
		ChainableMemoryBlock firstBlock = null;
		ChainableMemoryBlock previousBlock = null;
		try
		{
			while (true)
			{
				// allocate a new block
				ChainableMemoryBlock block = ChainableMemoryBlock.GetPooled(mBlockSize, mArrayPool);
				firstBlock ??= block;
				if (previousBlock != null) previousBlock.Next = block;

				// read data into the block
				int bytesRead = stream.Read(block.Buffer, 0, block.Capacity);
				if (bytesRead == 0)
				{
					if (firstBlock == block) firstBlock = null;
					if (previousBlock != null) previousBlock.Next = null;
					block.Release();
					break;
				}

				count += bytesRead;
				block.Length = bytesRead;
				previousBlock = block;
			}

			// inject the buffer at the current position
			InjectBufferAtCurrentPosition(firstBlock, true, true);
		}
		catch (Exception)
		{
			firstBlock?.ReleaseChain();
			throw;
		}

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
	/// The token to monitor for cancellation requests.
	/// The default value is <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>Number of written bytes.</returns>
	/// <exception cref="ArgumentNullException">Specified stream is a null.</exception>
	/// <exception cref="NotSupportedException">The source stream does not support reading.</exception>
	public async ValueTask<long> WriteAsync(Stream stream, CancellationToken cancellationToken = default)
	{
		if (stream == null)
			throw new ArgumentNullException(nameof(stream));

		if (!stream.CanRead)
			throw new NotSupportedException("The source stream does not support reading.");

		// abort, if cancellation is pending
		cancellationToken.ThrowIfCancellationRequested();

		// abort, if the source stream is empty
		long bytesInSourceStream = stream.CanSeek ? stream.Length - stream.Position : -1;
		if (bytesInSourceStream == 0)
			return 0;

		// abort, if the stream has been disposed
		if (mDisposed)
			throw new ObjectDisposedException(nameof(MemoryBlockStream));

		// read stream into a new blocks that can be appended to the current stream later on
		// (avoid blocking the current stream as long as the read operation is in progress)
		long count = 0;
		ChainableMemoryBlock firstBlock = null;
		ChainableMemoryBlock previousBlock = null;
		try
		{
			while (true)
			{
				// allocate a new block
				ChainableMemoryBlock block = ChainableMemoryBlock.GetPooled(mBlockSize, mArrayPool);
				firstBlock ??= block;
				if (previousBlock != null) previousBlock.Next = block;

				// read data into the block
				int bytesRead = await stream.ReadAsync(
						                block.Buffer,
						                0,
						                block.Capacity,
						                cancellationToken)
					                .ConfigureAwait(false);

				if (bytesRead == 0)
				{
					if (firstBlock == block) firstBlock = null;
					if (previousBlock != null) previousBlock.Next = null;
					block.Release();
					break;
				}

				count += bytesRead;
				block.Length = bytesRead;
				previousBlock = block;
			}

			// inject the buffer at the current position
			await InjectBufferAtCurrentPositionAsync(firstBlock, true, true, cancellationToken).ConfigureAwait(false);
		}
		catch (Exception)
		{
			firstBlock?.ReleaseChain();
			throw;
		}

		return count;
	}

	#endregion

	#region Internal Write Helpers

	/// <summary>
	/// Writes the specified buffer into the stream.
	/// </summary>
	/// <param name="buffer">Array containing data to write.</param>
	/// <param name="offset">Offset in the array to start at.</param>
	/// <param name="count">Number of bytes to write.</param>
	private void WriteInternal(byte[] buffer, int offset, int count)
	{
		if (mDisposed)
			throw new ObjectDisposedException(nameof(MemoryBlockStream));

		// write data to the stream
		int bytesRemaining = count;
		while (bytesRemaining > 0)
		{
			// copy as many bytes as requested and possible
			int index = PrepareWritingBlock(out int bytesToEndOfBlock);
			int bytesToCopy = Math.Min(bytesToEndOfBlock, bytesRemaining);
			Array.Copy(buffer, offset, mCurrentBlock.Buffer, index, bytesToCopy);
			offset += bytesToCopy;
			bytesRemaining -= bytesToCopy;
			mPosition += bytesToCopy;

			// update length of the memory block
			mCurrentBlock.Length = Math.Max(mCurrentBlock.Length, index + bytesToCopy);
		}

		mLength = Math.Max(mLength, mPosition);
	}

	/// <summary>
	/// Writes the specified buffer into the stream.
	/// </summary>
	/// <param name="buffer">Buffer containing data to write.</param>
	private void WriteInternal(ReadOnlySpan<byte> buffer)
	{
		if (mDisposed)
			throw new ObjectDisposedException(nameof(MemoryBlockStream));

		// write data to the stream
		int bytesRemaining = buffer.Length;
		int offset = 0;
		while (bytesRemaining > 0)
		{
			// copy as many bytes as requested and possible
			int index = PrepareWritingBlock(out int bytesToEndOfBlock);
			int bytesToCopy = Math.Min(bytesToEndOfBlock, bytesRemaining);
			ReadOnlySpan<byte> source = buffer.Slice(offset, bytesToCopy);
			var destination = new Span<byte>(mCurrentBlock.Buffer, index, bytesToCopy);
			source.CopyTo(destination);
			bytesRemaining -= bytesToCopy;
			offset += bytesToCopy;
			mPosition += bytesToCopy;

			// update length of the memory block
			mCurrentBlock.Length = Math.Max(mCurrentBlock.Length, index + bytesToCopy);
		}

		mLength = Math.Max(mLength, mPosition);
	}

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
			bytesToEndOfBlock = mCurrentBlock.Next != null
				                    ? mCurrentBlock.Length - index
				                    : mCurrentBlock.Capacity - index;
		}

		if (bytesToEndOfBlock != 0)
			return index;

		// memory block is at its end
		// => continue writing the next memory block
		if (mCurrentBlock?.Next != null)
		{
			mCurrentBlock = mCurrentBlock.Next;
			bytesToEndOfBlock = mCurrentBlock.Length;
		}
		else
		{
			if (!AppendNewBuffer()) mCurrentBlock = mCurrentBlock!.Next;
			Debug.Assert(mCurrentBlock != null, nameof(mCurrentBlock) + " != null");
			bytesToEndOfBlock = mCurrentBlock.Capacity;
		}

		mCurrentBlockStartIndex = mPosition;
		Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);
		index = 0;

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

		ChainableMemoryBlock block = ChainableMemoryBlock.GetPooled(mBlockSize, mArrayPool, false);

		if (mFirstBlock == null)
		{
			mCurrentBlock = block;
			mFirstBlock = block;
		}
		else
		{
			mLastBlock.Next = block;
		}

		mLastBlock = block;

		return isFirstBuffer;
	}

	#endregion

	#region void CopyTo(Stream destination, int bufferSize)

	/// <summary>
	/// Reads the bytes from the current stream and writes them to another stream, using a specified buffer size.
	/// Blocks until the operation has completed.
	/// </summary>
	/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
	/// <param name="bufferSize">The size of the buffer. This value must be greater than zero. The default size is 81920.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="destination"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="bufferSize"/> is negative or zero.
	/// </exception>
	/// <exception cref="NotSupportedException"><paramref name="destination"/> does not support writing.</exception>
	/// <exception cref="ObjectDisposedException">
	/// Either the current stream or <paramref name="destination"/> have been disposed.
	/// </exception>
	public
#if NETSTANDARD2_0 || NET461 || NET48
		new
#elif NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
		override
#else
#error Unhandled target framework.
#endif
		void CopyTo(Stream destination, int bufferSize)
	{
		if (destination == null)
			throw new ArgumentNullException(nameof(destination));

		if (bufferSize <= 0)
			throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, "The buffer size must not be negative or zero.");

		if (!destination.CanWrite)
			throw new NotSupportedException("The destination stream does not support writing.");

		if (mDisposed)
			throw new ObjectDisposedException(nameof(MemoryBlockStream));

		long bytesToRead = mLength - mPosition;
		Debug.Assert(bytesToRead >= 0);
		long remaining = bytesToRead;
		while (remaining > 0)
		{
			// copy as many bytes as requested and possible
			int index = PrepareReadingBlock(remaining, out int bytesToCopy);
			destination.Write(mCurrentBlock.Buffer, index, bytesToCopy);
			remaining -= bytesToCopy;
			mPosition += bytesToCopy;
		}
	}

	#endregion

	#region Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)

	/// <summary>
	/// Asynchronously reads the bytes from the current stream and writes them to another stream, using a specified
	/// buffer size and cancellation token. Blocks until the operation has completed.
	/// </summary>
	/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
	/// <param name="bufferSize">
	/// The size of the buffer (in bytes).
	/// This value must be greater than zero.
	/// The default size is 81920.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests.
	/// The default value is <see cref="CancellationToken.None"/>.
	/// </param>
	/// <exception cref="ArgumentNullException"> <paramref name="destination"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentOutOfRangeException"> <paramref name="bufferSize"/> is negative or zero.</exception>
	/// <exception cref="ObjectDisposedException">Either the current stream or the destination stream is disposed.</exception>
	/// <exception cref="NotSupportedException">The destination stream does not support writing.</exception>
	/// <returns>A task that represents the asynchronous copy operation.</returns>
	public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
	{
		if (destination == null)
			throw new ArgumentNullException(nameof(destination));

		if (bufferSize <= 0)
			throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, "The buffer size must not be negative or zero.");

		if (!destination.CanWrite)
			throw new NotSupportedException("The destination stream does not support writing.");

		// abort, if cancellation is pending
		cancellationToken.ThrowIfCancellationRequested();

		if (mDisposed)
			throw new ObjectDisposedException(nameof(MemoryBlockStream));

		long bytesToRead = mLength - mPosition;
		Debug.Assert(bytesToRead >= 0);
		long remaining = bytesToRead;
		while (remaining > 0)
		{
			// copy as many bytes as requested and possible
			int index = PrepareReadingBlock(remaining, out int bytesToCopy);
			await destination.WriteAsync(mCurrentBlock.Buffer, index, bytesToCopy, cancellationToken).ConfigureAwait(false);
			remaining -= bytesToCopy;
			mPosition += bytesToCopy;
		}
	}

	#endregion

	#region Flushing

	/// <summary>
	/// Flushes the stream (does not do anything for this stream).
	/// </summary>
	public override void Flush() { }

	/// <summary>
	/// Flushes the stream asynchronously (does not do anything for this stream).
	/// </summary>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests.
	/// The default value is <see cref="CancellationToken.None"/>.
	/// </param>
	public override Task FlushAsync(CancellationToken cancellationToken) // overload without CancellationToken is defined in base class
	{
		return Task.CompletedTask;
	}

	#endregion

	#region Appending Buffers

	/// <summary>
	/// Appends a memory block or chain of memory blocks to the stream.
	/// </summary>
	/// <param name="buffer">Memory block to append to the stream.</param>
	/// <exception cref="ArgumentNullException">The <paramref name="buffer"/> argument is <c>null</c>.</exception>
	/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
	/// <remarks>
	/// The specified buffer must not be directly accessed after this operation.
	/// The stream takes care of returning buffers to their array pool, if necessary.
	/// </remarks>
	public void AppendBuffer(ChainableMemoryBlock buffer)
	{
		if (buffer == null)
			throw new ArgumentNullException(nameof(buffer));

		AppendBuffer_Internal(buffer);
	}

	/// <summary>
	/// Appends a memory block or chain of memory blocks to the stream.
	/// </summary>
	/// <param name="buffer">Memory block to append to the stream.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests.
	/// The default value is <see cref="CancellationToken.None"/>.
	/// </param>
	/// <exception cref="ArgumentNullException">The <paramref name="buffer"/> argument is <c>null</c>.</exception>
	/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
	/// <remarks>
	/// The specified buffer must not be directly accessed after this operation.
	/// The stream takes care of returning buffers to their array pool, if necessary.
	/// </remarks>
	public Task AppendBufferAsync(ChainableMemoryBlock buffer, CancellationToken cancellationToken = default)
	{
		if (buffer == null)
			throw new ArgumentNullException(nameof(buffer));

		// abort, if cancellation is pending
		if (cancellationToken.IsCancellationRequested)
			return Task.FromException(new OperationCanceledException(cancellationToken));

		AppendBuffer_Internal(buffer);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Appends a memory block or chain of memory blocks to the stream (for internal use).
	/// </summary>
	/// <param name="buffer">Memory block to append to the stream.</param>
	/// <exception cref="ArgumentNullException">The <paramref name="buffer"/> argument is <c>null</c>.</exception>
	/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
	/// <remarks>
	/// The specified buffer must not be directly accessed after this operation.
	/// The stream takes care of returning buffers to their array pool, if necessary.
	/// </remarks>
	private void AppendBuffer_Internal(ChainableMemoryBlock buffer)
	{
		if (mDisposed)
			throw new ObjectDisposedException(nameof(MemoryBlockStream));

		if (mLastBlock != null)
		{
			// the stream already contains data
			// => append blocks at the end
			mLastBlock.Next = buffer;

			// adjust length
			ChainableMemoryBlock block = buffer;
			while (block != null)
			{
				mLength += block.Length;
				mLastBlock = block;
				block = block.Next;
			}
		}
		else
		{
			// stream has no backing buffer
			// => the buffer becomes the only buffer backing the stream

			// exchange buffer
			mCurrentBlock = buffer;
			mFirstBlock = buffer;

			// update administrative variables appropriately
			mCurrentBlockStartIndex = mPosition;
			Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);

			// adjust length of the buffer
			ChainableMemoryBlock block = mFirstBlock;
			while (block != null)
			{
				mLength += block.Length;
				mLastBlock = block;
				block = block.Next;
			}
		}
	}

	#endregion

	#region Attaching Buffers

	/// <summary>
	/// Attaches a memory block or chain of memory blocks to the stream.
	/// </summary>
	/// <param name="buffer">Memory block to attach to the stream (null to clear the stream).</param>
	/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
	/// <remarks>
	/// This method allows you to exchange the underlying memory block buffer.
	/// The stream is reset, so the position is 0 after attaching the new buffer.
	/// The specified buffer must not be directly accessed after this operation.
	/// The stream takes care of returning buffers to their array pool, if necessary.
	/// </remarks>
	public void AttachBuffer(ChainableMemoryBlock buffer)
	{
		if (buffer?.Previous != null)
			throw new ArgumentException("The specified block must not have a predecessor.", nameof(buffer));

		AttachBuffer_Internal(buffer);
	}

	/// <summary>
	/// Attaches a memory block or chain of memory blocks to the stream.
	/// </summary>
	/// <param name="buffer">Memory block to attach to the stream (null to clear the stream).</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests.
	/// The default value is <see cref="CancellationToken.None"/>.
	/// </param>
	/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
	/// <remarks>
	/// This method allows you to exchange the underlying memory block buffer.
	/// The stream is reset, so the position is 0 after attaching the new buffer.
	/// The specified buffer must not be directly accessed after this operation.
	/// The stream takes care of returning buffers to their array pool, if necessary.
	/// </remarks>
	public Task AttachBufferAsync(ChainableMemoryBlock buffer, CancellationToken cancellationToken = default)
	{
		if (buffer?.Previous != null)
			throw new ArgumentException("The specified block must not have a predecessor.", nameof(buffer));

		// abort, if cancellation is pending
		if (cancellationToken.IsCancellationRequested)
			return Task.FromException(new OperationCanceledException(cancellationToken));

		AttachBuffer_Internal(buffer);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Attaches a memory block or chain of memory blocks to the stream (for internal use).
	/// </summary>
	/// <param name="buffer">Memory block to attach to the stream (null to clear the stream).</param>
	/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
	/// <remarks>
	/// This method allows you to exchange the underlying memory block buffer.
	/// The stream is reset, so the position is 0 after attaching the new buffer.
	/// The specified buffer must not be directly accessed after this operation.
	/// The stream takes care of returning buffers to their array pool, if necessary.
	/// </remarks>
	private void AttachBuffer_Internal(ChainableMemoryBlock buffer)
	{
		if (mDisposed)
			throw new ObjectDisposedException(nameof(MemoryBlockStream));

		// release old chain of blocks backing the stream
		mFirstBlock?.ReleaseChain();

		// exchange buffer
		mFirstBlock = buffer;
		mFirstBlockOffset = 0;
		mCurrentBlock = mFirstBlock;
		mCurrentBlockStartIndex = 0;
		mPosition = 0;

		// determine length of the buffer
		mLength = 0;
		ChainableMemoryBlock block = mFirstBlock;
		while (block != null)
		{
			mLength += block.Length;
			mLastBlock = block;
			block = block.Next;
		}
	}

	#endregion

	#region Detaching Buffers

	/// <summary>
	/// Detaches the underlying memory block buffer from the stream.
	/// </summary>
	/// <returns>Underlying memory block buffer (can be a chained with other memory blocks).</returns>
	/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
	/// <remarks>
	/// This method allows you to detach the underlying buffer from the stream and use it in another place.
	/// If blocks contain buffers that have been rented from an array pool, the returned memory-block chain must
	/// be disposed to return buffers to the pool. The stream is empty afterward.
	/// </remarks>
	public ChainableMemoryBlock DetachBuffer()
	{
		return DetachBuffer_Internal();
	}

	/// <summary>
	/// Detaches the underlying memory block buffer from the stream.
	/// </summary>
	/// <returns>Underlying memory block buffer (can be a chained with other memory blocks).</returns>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests.
	/// The default value is <see cref="CancellationToken.None"/>.
	/// </param>
	/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
	/// <remarks>
	/// This method allows you to detach the underlying buffer from the stream and use it in another place.
	/// If blocks contain buffers that have been rented from an array pool, the returned memory-block chain must
	/// be disposed to return buffers to the pool. The stream is empty afterward.
	/// </remarks>
	public Task<ChainableMemoryBlock> DetachBufferAsync(CancellationToken cancellationToken = default)
	{
		return cancellationToken.IsCancellationRequested
			       ? Task.FromException<ChainableMemoryBlock>(new OperationCanceledException(cancellationToken))
			       : Task.FromResult(DetachBuffer_Internal());
	}

	/// <summary>
	/// Detaches the underlying memory block buffer from the stream (for internal use).
	/// </summary>
	/// <returns>Underlying memory block buffer (can be a chained with other memory blocks).</returns>
	/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
	/// <remarks>
	/// This method allows you to detach the underlying buffer from the stream and use it in another place.
	/// If blocks contain buffers that have been rented from an array pool, the returned memory-block chain must
	/// be disposed to return buffers to the pool. The stream is empty afterward.
	/// </remarks>
	private ChainableMemoryBlock DetachBuffer_Internal()
	{
		if (mDisposed)
			throw new ObjectDisposedException(nameof(MemoryBlockStream));

		ChainableMemoryBlock buffer = mFirstBlock;
		mFirstBlock = null;
		mCurrentBlock = null;
		mLastBlock = null;
		mFirstBlockOffset = 0;
		mCurrentBlockStartIndex = 0;
		mPosition = 0;
		mLength = 0;
		return buffer;
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Injects the specified memory block or chain of memory blocks at the current position of the chain of memory blocks
	/// backing the stream. Optionally overwrites exiting stream data.
	/// </summary>
	/// <param name="buffer">Memory block to inject into the stream.</param>
	/// <param name="overwrite">
	/// <c>true</c> to overwrite existing data in the stream;
	/// <c>false</c> to insert the specified memory blocks at the current position.
	/// </param>
	/// <param name="advancePosition">
	/// <c>true</c> to advance the position of the stream to the end of the injected memory block;
	/// <c>false</c> to keep the position of the stream at its position.
	/// </param>
	/// <exception cref="ArgumentNullException">The <paramref name="buffer"/> argument is <c>null</c>.</exception>
	/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
	/// <remarks>
	/// The specified buffer must not be directly accessed after this operation.
	/// The stream takes care of returning buffers to their array pool, if necessary.
	/// </remarks>
	internal void InjectBufferAtCurrentPosition(ChainableMemoryBlock buffer, bool overwrite, bool advancePosition)
	{
		if (buffer == null)
			throw new ArgumentNullException(nameof(buffer));

		if (buffer.Previous != null)
			throw new ArgumentException("The specified block must not have a predecessor.", nameof(buffer));

		InjectBufferAtCurrentPosition_Internal(buffer, overwrite, advancePosition);
	}

	/// <summary>
	/// Injects the specified memory block or chain of memory blocks at the current position of the chain of memory blocks
	/// backing the stream. Optionally overwrites exiting stream data.
	/// </summary>
	/// <param name="buffer">Memory block to inject into the stream.</param>
	/// <param name="overwrite">
	/// <c>true</c> to overwrite existing data in the stream;
	/// <c>false</c> to insert the specified memory blocks at the current position.
	/// </param>
	/// <param name="advancePosition">
	/// <c>true</c> to advance the position of the stream to the end of the injected memory block;
	/// <c>false</c> to keep the position of the stream at its position.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests.
	/// The default value is <see cref="CancellationToken.None"/>.
	/// </param>
	/// <exception cref="ArgumentNullException">The <paramref name="buffer"/> argument is <c>null</c>.</exception>
	/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
	/// <remarks>
	/// The specified buffer must not be directly accessed after this operation.
	/// The stream takes care of returning buffers to their array pool, if necessary.
	/// </remarks>
	internal Task InjectBufferAtCurrentPositionAsync(
		ChainableMemoryBlock buffer,
		bool                 overwrite,
		bool                 advancePosition,
		CancellationToken    cancellationToken)
	{
		if (buffer == null)
			throw new ArgumentNullException(nameof(buffer));

		if (buffer.Previous != null)
			throw new ArgumentException("The specified block must not have a predecessor.", nameof(buffer));

		// abort, if cancellation is pending
		if (cancellationToken.IsCancellationRequested)
			return Task.FromException(new OperationCanceledException(cancellationToken));

		InjectBufferAtCurrentPosition_Internal(buffer, overwrite, advancePosition);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Injects the specified memory block or chain of memory blocks at the current position of the chain of memory blocks
	/// backing the stream. Optionally overwrites exiting stream data (for internal use).
	/// </summary>
	/// <param name="buffer">Memory block to inject into the stream.</param>
	/// <param name="overwrite">
	/// <c>true</c> to overwrite existing data in the stream;
	/// <c>false</c> to insert the specified memory blocks at the current position.
	/// </param>
	/// <param name="advancePosition">
	/// <c>true</c> to advance the position of the stream to the end of the injected memory block;
	/// <c>false</c> to keep the position of the stream at its position.
	/// </param>
	/// <exception cref="ArgumentNullException">The <paramref name="buffer"/> argument is <c>null</c>.</exception>
	/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
	/// <remarks>
	/// The specified buffer must not be directly accessed after this operation.
	/// The stream takes care of returning buffers to their array pool, if necessary.
	/// </remarks>
	private void InjectBufferAtCurrentPosition_Internal(ChainableMemoryBlock buffer, bool overwrite, bool advancePosition)
	{
		if (mDisposed)
			throw new ObjectDisposedException(nameof(MemoryBlockStream));

		if (mLastBlock != null)
		{
			if (mPosition == mLength)
			{
				// --------------------------------------------------------------------------------------------------------------
				// the current position is at the end of the stream
				// => the specified chain of memory blocks can simply be appended to the chain of memory blocks backing the stream
				// --------------------------------------------------------------------------------------------------------------

				// the stream already contains data
				// => append blocks at the end
				mLastBlock.Next = buffer;

				// adjust length
				ChainableMemoryBlock block = buffer;
				while (block != null)
				{
					mLength += block.Length;
					mLastBlock = block;
					block = block.Next;
				}

				// advance the position to the end of the stream, if requested
				if (!advancePosition) return;
				mPosition = mLength;
				mCurrentBlock = mLastBlock;
				mCurrentBlockStartIndex = mLength - mCurrentBlock.Length;
				Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);
			}
			else
			{
				// --------------------------------------------------------------------------------------------------------------
				// the current position is within the stream
				// => data after the current stream position must be moved/overwritten
				// --------------------------------------------------------------------------------------------------------------

				// adjust current block, if the position is at the end of the block
				int indexOfPositionInCurrentBlock = (int)(mPosition - mCurrentBlockStartIndex);
				if (indexOfPositionInCurrentBlock == mCurrentBlock.Length)
				{
					// position is at the end of the current block
					// => adjust current block before proceeding
					Debug.Assert(mCurrentBlock.Next != null); // position is within the stream, so a block should be following
					mCurrentBlockStartIndex += mCurrentBlock.Length;
					mCurrentBlock = mCurrentBlock.Next;
					indexOfPositionInCurrentBlock = (int)(mPosition - mCurrentBlockStartIndex);
					Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);
				}

				// the specified buffers contain data that should be inserted at the current position of the stream
				ChainableMemoryBlock endOfChainToInsert = buffer.GetEndOfChain(out long chainToInsertLength);
				if (indexOfPositionInCurrentBlock == 0)
				{
					// stream position is at the first byte in the current block
					// => the specified chain of blocks can be inserted before the current block, no copying needed
					ChainableMemoryBlock block = mCurrentBlock;
					if (mCurrentBlock.Previous != null)
						mCurrentBlock.Previous.Next = buffer;
					else
						mFirstBlock = buffer;

					// the old current block now follows the last inserted block
					endOfChainToInsert.Next = block;

					if (overwrite)
					{
						// overwriting has been requested
						// => remove same amount of existing data
						RemoveDataFromChain(block, chainToInsertLength);
						mLength = Math.Max(mLength, mPosition + chainToInsertLength);
						if (endOfChainToInsert.Next == null)
							mLastBlock = endOfChainToInsert;
					}
					else
					{
						// inserting has been requested
						// => leave the backing chain of blocks as they are now and adjust the length only
						mLength += chainToInsertLength;
					}

					// advance the position to the end of the inserted/overwritten part, if requested
					if (advancePosition)
					{
						mPosition += chainToInsertLength;
						mCurrentBlock = endOfChainToInsert;
						mCurrentBlockStartIndex += chainToInsertLength - endOfChainToInsert.Length;
						Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);
					}
					else
					{
						mCurrentBlock = buffer;
						Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);
					}
				}
				else
				{
					// stream position is not at the first byte in the current block
					// => check whether data must be moved from the current block to the end of the inserted chain of blocks
					int bytesToEndOfCurrentBlock = mCurrentBlock.Length - indexOfPositionInCurrentBlock;
					if (overwrite)
					{
						if (chainToInsertLength >= bytesToEndOfCurrentBlock)
						{
							// new data should overwrite existing data and there is enough new data to overwrite existing data in the current block
							// => no need to move existing data to the end of the inserted blocks, just cut the current block
							mCurrentBlock.Length -= bytesToEndOfCurrentBlock;
							ChainableMemoryBlock block = mCurrentBlock.Next;
							mCurrentBlock.Next = buffer;

							// the old current block now follows the last inserted block
							endOfChainToInsert.Next = block;

							// remove same amount of existing data as has been inserted
							if (block != null)
								RemoveDataFromChain(block, chainToInsertLength - bytesToEndOfCurrentBlock);

							// adjust the last block
							if (endOfChainToInsert.Next == null)
								mLastBlock = endOfChainToInsert;

							// adjust the length of the stream
							mLength = Math.Max(mLength, mPosition + chainToInsertLength);

							// advance the position to the end of the inserted part, if requested
							if (!advancePosition) return;
							mPosition += chainToInsertLength;
							mCurrentBlock = endOfChainToInsert;
							mCurrentBlockStartIndex += indexOfPositionInCurrentBlock + chainToInsertLength - endOfChainToInsert.Length;
							Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);
						}
						else
						{
							// there is not enough data to overwrite existing data from the current position to the end of the block
							// => the current block still contains data to keep...
							ChainableMemoryBlock block = buffer;
							int offset = indexOfPositionInCurrentBlock;
							while (block != null)
							{
								Array.Copy(
									block.Buffer,
									0,
									mCurrentBlock.Buffer,
									offset,
									block.Length);

								offset += block.Length;
								ChainableMemoryBlock next = block.Next;
								block.Next = null;
								Debug.Assert(block.Previous == null);
								Debug.Assert(block.Next == null);
								block.Release();
								block = next;
							}

							// advance the position to the end of the inserted part, if requested
							if (advancePosition)
							{
								mPosition += chainToInsertLength;
								// mCurrentBlock and mCurrentBlockStartIndex remains the same
							}
						}
					}
					else
					{
						ChainableMemoryBlock adjustedEndOfChainToInsert = endOfChainToInsert; // can be enlarged to contain moved data
						int lengthOfEndOfChainToInsert = endOfChainToInsert.Length;
						if (bytesToEndOfCurrentBlock > 0)
						{
							// there is some data to move from the current block to the end of the inserted chain of blocks
							// => ensure the last block contains enough space to store the data to move, append a new block, if necessary
							int unusedSpace = adjustedEndOfChainToInsert.Capacity - adjustedEndOfChainToInsert.Length;
							if (bytesToEndOfCurrentBlock > unusedSpace)
							{
								// the data to move does not fit into the last block of the inserted chain of blocks
								// => append new block(s) for it
								int remainingBytesToAllocate = bytesToEndOfCurrentBlock - unusedSpace;
								while (remainingBytesToAllocate > 0)
								{
									ChainableMemoryBlock block = ChainableMemoryBlock.GetPooled(mBlockSize, mArrayPool);
									adjustedEndOfChainToInsert.Next = block;
									adjustedEndOfChainToInsert = block;
									remainingBytesToAllocate -= block.Capacity;
								}
							}

							// copy data to move into the block(s)
							ChainableMemoryBlock blockReceivingMovedData = endOfChainToInsert;
							int remainingBytesToCopy = bytesToEndOfCurrentBlock;
							int offset = indexOfPositionInCurrentBlock;
							while (blockReceivingMovedData != null)
							{
								int bytesToCopy = Math.Min(remainingBytesToCopy, blockReceivingMovedData.Capacity - blockReceivingMovedData.Length);
								if (bytesToCopy > 0)
								{
									Array.Copy(
										mCurrentBlock.Buffer,
										offset,
										blockReceivingMovedData.Buffer,
										blockReceivingMovedData.Length,
										bytesToCopy);

									blockReceivingMovedData.Length += bytesToCopy;
									remainingBytesToCopy -= bytesToCopy;
									offset += bytesToCopy;
								}

								blockReceivingMovedData = blockReceivingMovedData.Next;
							}

							// all data should have been copied now
							Debug.Assert(remainingBytesToCopy == 0);
						}

						// adjust reference to the last block of the chain backing the stream, if the current block is the last block
						// (in this case the end of the chain of blocks to insert becomes the last block)
						if (mCurrentBlock.Next == null)
							mLastBlock = adjustedEndOfChainToInsert;

						// cut current block and link the chain of memory blocks with it
						mCurrentBlock.Length = indexOfPositionInCurrentBlock;
						adjustedEndOfChainToInsert.Next = mCurrentBlock.Next;
						mCurrentBlock.Next = buffer;

						// adjust the length of the stream
						mLength += chainToInsertLength;

						// advance the position to the end of the inserted part, if requested
						if (!advancePosition) return;
						mPosition += chainToInsertLength;
						mCurrentBlock = endOfChainToInsert;
						mCurrentBlockStartIndex += indexOfPositionInCurrentBlock + chainToInsertLength - lengthOfEndOfChainToInsert;
						Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);
					}
				}
			}
		}
		else
		{
			// stream has no backing buffer
			// => the buffer becomes the only buffer backing the stream

			// exchange buffer
			mCurrentBlock = buffer;
			mFirstBlock = buffer;

			// update administrative variables appropriately
			mCurrentBlockStartIndex = mPosition;
			Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);

			// adjust length of the buffer
			ChainableMemoryBlock block = mFirstBlock;
			while (block != null)
			{
				mLength += block.Length;
				mLastBlock = block;
				block = block.Next;
			}

			// advance the position to the end of the stream, if requested
			if (!advancePosition) return;
			Debug.Assert(mLastBlock != null, nameof(mLastBlock) + " != null");
			mCurrentBlock = mLastBlock;
			mPosition = mLength;
			mCurrentBlockStartIndex = mLength - mCurrentBlock.Length;
			Debug.Assert(mCurrentBlockStartIndex == mFirstBlockOffset + mCurrentBlock.IndexOfFirstByteInBlock);
		}
	}

	/// <summary>
	/// Removes the specified number of bytes starting at byte 0 of the specified memory block.
	/// Removes multiple blocks from the chain of blocks, if necessary.
	/// </summary>
	/// <param name="block">First block of the chain of blocks to remove data from.</param>
	/// <param name="length">Number of bytes to remove.</param>
	internal static void RemoveDataFromChain(ChainableMemoryBlock block, long length)
	{
		long remainingBytesToRemove = length;
		while (block != null && remainingBytesToRemove > 0)
		{
			int bytesToRemove = (int)Math.Min(block.Length, remainingBytesToRemove);
			ChainableMemoryBlock next = block.Next;
			ChainableMemoryBlock previous = block.Previous;
			if (bytesToRemove == block.Length)
			{
				// the block does not contain any data afterward
				// => complete block can be removed
				if (previous != null)
				{
					previous.Next = next;
					block.Next = null;
				}
				else if (next != null)
				{
					// block is first block of chain, setting previous reference is sufficient
					next.Previous = null;
				}

				Debug.Assert(block.Previous == null);
				Debug.Assert(block.Next == null);
				block.Release();
			}
			else
			{
				// the block still contains some data afterward
				// => adjust block buffer
				Debug.Assert(block.Length > 0);
				Debug.Assert(bytesToRemove > 0);
				Array.Copy(
					block.Buffer,
					bytesToRemove,
					block.Buffer,
					0,
					block.Length - bytesToRemove);

				block.Length -= bytesToRemove;
			}

			block = next;
			remainingBytesToRemove -= bytesToRemove;
		}
	}

	#endregion
}
