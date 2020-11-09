///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace GriffinPlus.Lib.Io
{
	/// <summary>
	/// Stream whose backing store is a linked list of memory blocks.
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
	/// </remarks>
	public class MemoryBlockStream : Stream
	{
		/// <summary>
		/// Default size of block in the stream.
		/// 64kByte is small enough for the regular heap and avoids allocation from the large object heap.
		/// </summary>
		private const int DefaultBlockSize = 64 * 1024;

		private long mPosition;
		private long mLength;
		private long mCapacity;
		private int mMinBlockSize;
		private long mCurrentBlockStartIndex;
		private ChainableMemoryBlock mFirstBlock;
		private ChainableMemoryBlock mCurrentBlock;
		private ChainableMemoryBlock mLastBlock;

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryBlockStream"/> class.
		/// </summary>
		/// <remarks>
		/// The minimum memory block size defaults to 64 kByte.
		/// </remarks>
		public MemoryBlockStream()
		{
			mMinBlockSize = DefaultBlockSize;
			mCurrentBlockStartIndex = 0;
			mCapacity = 0;
			mLength = 0;
			mPosition = 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryBlockStream"/> class setting the minimum size of blocks to use.
		/// </summary>
		/// <param name="minBlockSize">
		/// Minimum size of a memory block
		/// (can be greater if a block of data larger than the minimum size is written at once).
		/// </param>
		/// <exception cref="ArgumentException">The specified minimum memory block size is less than or equal to 0.</exception>
		public MemoryBlockStream(int minBlockSize)
		{
			if (minBlockSize <= 0)
			{
				throw new ArgumentException(
					"The minimum block length must be greater than 0.",
					nameof(minBlockSize));
			}

			mMinBlockSize = minBlockSize;
			mCurrentBlockStartIndex = 0;
			mCapacity = 0;
			mLength = 0;
			mPosition = 0;
		}

		#region Stream Capabilities

		/// <summary>
		/// Gets a value indicating whether the stream supports reading (always true).
		/// </summary>
		public override bool CanRead
		{
			get { return true; }
		}

		/// <summary>
		/// Gets a value indicating whether the stream supports writing (always true).
		/// </summary>
		public override bool CanWrite
		{
			get { return true; }
		}

		/// <summary>
		/// Gets a value indicating whether the stream supports seeking (always true).
		/// </summary>
		public override bool CanSeek
		{
			get { return true; }
		}

		#endregion

		#region Position/Length

		/// <summary>
		/// Gets or sets the current position within the stream.
		/// </summary>
		/// <exception cref="ArgumentException">The position is out of bounds when trying to set it.</exception>
		public override long Position
		{
			get { return mPosition; }
			set { Seek(value, SeekOrigin.Begin); }
		}

		/// <summary>
		/// Gets the length of the current stream.
		/// </summary>
		public override long Length
		{
			get { return mLength; }
		}

		/// <summary>
		/// Sets the length of the stream.
		/// </summary>
		/// <param name="length">The desired length of the current stream in bytes.</param>
		/// <exception cref="ArgumentException">The specified length is negative.</exception>
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
			{
				throw new ArgumentException(
					"The length must not be negative.",
					nameof(length));
			}

			if (length > mCapacity)
			{
				// requested size is greater than the current capacity of the stream
				// => enlarge buffer by adding a new memory block

				// determine the number of bytes to allocate additionally
				long additionallyNeededSpace = length - mCapacity;
				long lengthOfLastBlock = length - mCapacity;
				while (true)
				{
					int blockSize = (int)Math.Min(additionallyNeededSpace, int.MaxValue);
					blockSize = Math.Max(blockSize, mMinBlockSize);
					additionallyNeededSpace -= blockSize;

					ChainableMemoryBlock newBlock = new ChainableMemoryBlock(blockSize);      // inits memory with zeros automatically
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
						mLastBlock.mNext = newBlock;
						mLastBlock = newBlock;
					}

					// adjust stream capacity
					mCapacity += newBlock.Capacity;

					// abort, if needed space has been allocated
					if (additionallyNeededSpace == 0) break;

					// adjust length of the last block, since another block is following
					lengthOfLastBlock -= newBlock.Capacity;
				};

				// overwrite memory from the current position to the end of the memory block
				// (other memory blocks are inited with zeros anyway...)
				int bytesToClear = (int)(mCurrentBlock.Capacity - mPosition + mCurrentBlockStartIndex);
				Array.Clear(mCurrentBlock.mBuffer, (int)(mPosition - mCurrentBlockStartIndex), bytesToClear);

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
					// => release all memory blocks
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
					ChainableMemoryBlock block = mFirstBlock;
					while (true)
					{
						remaining -= Math.Min(remaining, block.mLength);
						if (remaining == 0) break;
						lastBlockStartIndex += block.mLength;
						mCapacity += block.mLength;
						block = block.mNext;
					}
					mCapacity += block.Capacity;
					block.mNext = null;
					mLastBlock = block;
					mLength = length;

					// clear all bytes up to the end of the last block
					int bytesToClear = (int)(mLastBlock.Capacity - length + lastBlockStartIndex);
					Array.Clear(mLastBlock.mBuffer, (int)(length - lastBlockStartIndex), bytesToClear);
					mLastBlock.mLength = (int)(length - lastBlockStartIndex);

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

		/// <summary>
		/// Sets the current position within the stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the origin parameter.</param>
		/// <param name="origin">Indicates the reference point used to obtain the new position.</param>
		/// <returns>The new position within the stream.</returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
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

				while (true)
				{
					remaining -= Math.Min(remaining, mCurrentBlock.mLength);
					if (remaining == 0) break;
					mCurrentBlockStartIndex += mCurrentBlock.mLength;
					mCurrentBlock = mCurrentBlock.mNext;
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

				if (offset < mLength)
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

		#region Reading

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
		/// <exception cref="ArgumentException">The specified range is out of bounds.</exception>
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (count == 0) return 0;
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			if (offset < 0)
			{
				throw new ArgumentException(
					"Offset must be greater than or equal to 0.",
					nameof(offset));
			}
			
			if (count < 0)
			{
				throw new ArgumentException(
					"Count must be greater than or equal to 0.",
					nameof(count));
			}

			if (offset >= buffer.Length)
			{
				throw new ArgumentException(
					"Offset exceeds the end of the buffer.",
					nameof(offset));
			}

			if (offset + count > buffer.Length)
			{
				throw new ArgumentException(
					"The buffer's length is less than offset + count.",
					nameof(count));
			}

			int bytesToRead = (int)Math.Min(mLength - mPosition, count);
			Debug.Assert(bytesToRead >= 0);
			ReadInternal(buffer, offset, bytesToRead);

			return bytesToRead;
		}

		/// <summary>
		/// Copies a sequence of bytes from the memory block chain to the provided buffer and advances the
		/// position appropriately.
		/// </summary>
		/// <param name="buffer">Buffer to copy read data to.</param>
		/// <param name="offset">Offset in the buffer to start at.</param>
		/// <param name="bytesToRead">Number of bytes to copy.</param>
		private void ReadInternal(byte[] buffer, int offset, int bytesToRead)
		{
			// abort if there is nothing to do...
			if (bytesToRead == 0) return;

			while (bytesToRead > 0)
			{
				// get index in the current memory block
				int index = (int)(mPosition - mCurrentBlockStartIndex);

				// determine how many bytes can be read from the current memory block
				int bytesToEnd = mCurrentBlock.mLength - index;

				if (bytesToEnd == 0)
				{
					// memory block is at its end
					// => continue reading the next memory block
					mCurrentBlock = mCurrentBlock.mNext;
					mCurrentBlockStartIndex = mPosition;
					Debug.Assert((mCurrentBlock != null)); // the caller should have ensured that there is enough data to read!

					// update index in the current memory block
					index = (int)(mPosition - mCurrentBlockStartIndex);
				}

				// copy as many bytes as requested and possible
				int bytesToCopy = Math.Min(mCurrentBlock.mLength - index, bytesToRead);
				Array.Copy(mCurrentBlock.mBuffer, index, buffer, offset, bytesToCopy);
				offset += bytesToCopy;
				bytesToRead -= bytesToCopy;
				mPosition += bytesToCopy;
			}
		}

		/// <summary>
		/// Reads a sequence of bytes from the stream and advances the position within the stream by the number
		/// of bytes read.
		/// </summary>
		/// <param name="pBuffer">Buffer receiving data from the stream.</param>
		/// <param name="count">Number of bytes to read.</param>
		/// <returns>
		/// The total number of bytes read into the buffer. This can be less than the number of requested bytes,
		/// if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
		/// </returns>
		public int Read(IntPtr pBuffer, int count)
		{
			if (count == 0) return 0;

			if (pBuffer == IntPtr.Zero && count > 0)
			{
				throw new ArgumentNullException(
					"The specified buffer must not be a null pointer, if 'count' is greater than 0.",
					nameof(pBuffer));
			}

			if (count < 0)
			{
				throw new ArgumentException(
					"Count must be greater than or equal to 0.",
					nameof(count));
			}

			int bytesToRead = (int)Math.Min(mLength - mPosition, count);
			Debug.Assert(bytesToRead >= 0);
			ReadInternal(pBuffer, bytesToRead);

			return bytesToRead;
		}

		/// <summary>
		/// Copies a sequence of bytes from the memory block chain to the provided buffer and advances the
		/// position appropriately.
		/// </summary>
		/// <param name="pBuffer">Buffer to copy read data to.</param>
		/// <param name="bytesToRead">Number of bytes to copy.</param>
		private void ReadInternal(IntPtr pBuffer, int bytesToRead)
		{
			// abort if there is nothing to do...
			if (bytesToRead == 0) return;

			while (bytesToRead > 0)
			{
				// get index in the current memory block
				int index = (int)(mPosition - mCurrentBlockStartIndex);

				// determine how many bytes can be read from the current memory block
				int bytesToEnd = mCurrentBlock.mLength - index;

				if (bytesToEnd == 0)
				{
					// memory block is at its end
					// => continue reading the next memory block
					mCurrentBlock = mCurrentBlock.mNext;
					mCurrentBlockStartIndex = mPosition;

					// the caller should have ensured that there is enough data to read!
					Debug.Assert((mCurrentBlock != null)); 

					// update index in the current memory block
					index = (int)(mPosition - mCurrentBlockStartIndex);
				}

				// copy as many bytes as requested and possible
				int bytesToCopy = Math.Min(mCurrentBlock.mLength - index, bytesToRead);
				Marshal.Copy(mCurrentBlock.mBuffer, index, pBuffer, bytesToRead);
				bytesToRead -= bytesToCopy;
				mPosition += bytesToCopy;
			}
		}

		/// <summary>
		/// Reads a byte from the stream and advances the position within the stream by one byte,
		/// or returns -1 if at the end of the stream.
		/// </summary>
		/// <returns>
		/// The unsigned byte cast to an Int32;
		/// -1, if at the end of the stream.
		/// </returns>
		public override int ReadByte()
		{
			if (mPosition == mLength) {
				return -1; // end of the stream
			}

			return ReadByteInternal();
		}

		/// <summary>
		/// Reads a byte from the stream and advances the position within the stream by one byte.
		/// </summary>
		/// <returns>The value of the read byte.</returns>
		private int ReadByteInternal()
		{
			while (true)
			{
				// get index in the current memory block
				int index = (int)(mPosition - mCurrentBlockStartIndex);

				// determine how many bytes can be read from the current memory block
				int bytesToEnd = (int)(mCurrentBlock.mLength - index);

				if (bytesToEnd == 0)
				{
					// memory block is at its end
					// => continue reading the next memory block
					mCurrentBlock = mCurrentBlock.mNext;
					mCurrentBlockStartIndex = mPosition;

					// the caller should have ensured that there is enough data to read!
					Debug.Assert((mCurrentBlock != null));

					// update index in the current memory block
					index = (int)(mPosition - mCurrentBlockStartIndex);
				}

				// return byte and advance position of the stream
				if (mCurrentBlock.mLength - index > 0)
				{
					mPosition++;
					return mCurrentBlock.mBuffer[index];
				}
			}
		}

		#endregion

		#region Writing

		/// <summary>
		/// Writes a sequence of bytes to the stream and advances the position within this stream by the
		/// number of bytes written.
		/// </summary>
		/// <param name="buffer">Buffer containing data to write to the stream.</param>
		/// <param name="offset">Offset in the buffer to start writing data from.</param>
		/// <param name="count">Number of bytes to write.</param>
		/// <exception cref="ArgumentException">The specified range is out of bounds.</exception>
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (count == 0) return;
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			
			if (offset < 0)
			{
				throw new ArgumentException(
					"Offset must be greater than or equal to 0.",
					nameof(offset));
			}

			if (count < 0)
			{
				throw new ArgumentException(
					"Count must be greater than or equal to 0.",
					nameof(count));
			}

			if (offset >= buffer.Length)
			{
				throw new ArgumentException(
					"Offset exceeds the end of the buffer.",
					nameof(offset));
			}

			if (offset + count > buffer.Length)
			{
				throw new ArgumentException(
					"The buffer's length is less than offset + count.",
					nameof(count));
			}

			// write data to the stream
			WriteInternal(buffer, offset, count);
		}

		/// <summary>
		/// Writes a sequence of bytes to the stream and advances the position within this stream by the
		/// number of bytes written.
		/// </summary>
		/// <param name="buffer">Buffer containing data to write to the stream.</param>
		/// <param name="offset">Offset in the buffer to start writing data from.</param>
		/// <param name="count">Number of bytes to write.</param>
		private void WriteInternal(byte[] buffer, int offset, int count)
		{
			int bytesRemaining = count;
			while (bytesRemaining > 0)
			{
				// get index in the current memory block
				int index = (int)(mPosition - mCurrentBlockStartIndex);

				// determine how many bytes can be written to the current memory block
				int bytesToEnd = 0;
				if (mCurrentBlock != null)
				{
					bytesToEnd = mCurrentBlock.mNext != null ?
						mCurrentBlock.mLength - index :
						mCurrentBlock.Capacity - index;
				}

				if (bytesToEnd == 0)
				{
					// memory block is at its end
					// => continue writing the next memory block
					if (mCurrentBlock?.mNext != null)
					{
						mCurrentBlock = mCurrentBlock.mNext;
						bytesToEnd = mCurrentBlock.mLength;
					}
					else
					{
						if (!AppendNewBuffer(bytesRemaining)) mCurrentBlock = mCurrentBlock.mNext;
						bytesToEnd = mCurrentBlock.Capacity;
					}

					mCurrentBlockStartIndex = mPosition;
					index = 0;
				}

				// copy as many bytes as requested and possible
				int bytesToCopy = Math.Min(bytesToEnd, bytesRemaining);
				Array.Copy(buffer, offset, mCurrentBlock.mBuffer, index, bytesToCopy);
				offset += bytesToCopy;
				bytesRemaining -= bytesToCopy;
				mPosition += bytesToCopy;

				// update length of the memory block
				mCurrentBlock.mLength = Math.Max(mCurrentBlock.mLength, index + bytesToCopy);
			}

			mLength = Math.Max(mLength, mPosition);
		}

		/// <summary>
		/// Writes a sequence of bytes to the stream and advances the position within this stream by the number of bytes written.
		/// </summary>
		/// <param name="pBuffer">Buffer containing data to write to the stream.</param>
		/// <param name="count">Number of bytes to write.</param>
		/// <exception cref="ArgumentException">
		/// Buffer is null and count is not 0 -or-\n
		/// count is less than 0.
		/// </exception>
		public void Write(IntPtr pBuffer, int count)
		{
			if (count == 0) return;

			if (pBuffer == IntPtr.Zero && count > 0)
			{
				throw new ArgumentException(
					"The specified buffer must not be a null pointer, if 'count' is greater than 0.",
					nameof(pBuffer));
			}

			if (count < 0)
			{
				throw new ArgumentException(
					"Count must be greater than or equal to 0.",
					nameof(count));
			}

			// write data to the stream
			WriteInternal(pBuffer, count);
		}

		/// <summary>
		/// Writes a sequence of bytes to the stream and advances the position within this stream by the number
		/// of bytes written.
		/// </summary>
		/// <param name="pBuffer">Buffer containing data to write to the stream.</param>
		/// <param name="count">Number of bytes to write.</param>
		private void WriteInternal(IntPtr pBuffer, int count)
		{
			int bytesRemaining = count;
			while (bytesRemaining > 0)
			{
				// get index in the current memory block
				int index = (int)(mPosition - mCurrentBlockStartIndex);

				// determine how many bytes can be written to the current memory block
				int bytesToEnd = 0;
				if (mCurrentBlock != null)
				{
					bytesToEnd = mCurrentBlock.mNext != null ?
						mCurrentBlock.mLength - index :
						mCurrentBlock.Capacity - index;
				}

				if (bytesToEnd == 0)
				{
					// memory block is at its end
					// => continue writing the next memory block
					if (mCurrentBlock?.mNext != null)
					{
						mCurrentBlock = mCurrentBlock.mNext;
						bytesToEnd = mCurrentBlock.mLength;
					}
					else
					{
						if (!AppendNewBuffer(bytesRemaining)) mCurrentBlock = mCurrentBlock.mNext;
						bytesToEnd = mCurrentBlock.Capacity;
					}

					mCurrentBlockStartIndex = mPosition;
					index = 0;
				}

				// copy as many bytes as requested and possible
				int bytesToCopy = Math.Min(bytesToEnd, bytesRemaining);
				Marshal.Copy(pBuffer, mCurrentBlock.mBuffer, index, bytesToCopy);
				pBuffer += bytesToCopy;
				bytesRemaining -= bytesToCopy;
				mPosition += bytesToCopy;

				// update length of the memory block
				mCurrentBlock.mLength = Math.Max(mCurrentBlock.mLength, index + bytesToCopy);
			}

			mLength = Math.Max(mLength, mPosition);
		}

		/// <summary>
		/// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
		/// </summary>
		/// <param name="value">The byte to write to the stream.</param>
		public override void WriteByte(byte value)
		{
			while (true)
			{
				// get index in the current memory block
				int index = (int)(mPosition - mCurrentBlockStartIndex);

				// determine how many bytes can be written to the current memory block
				int bytesToEnd = 0;
				if (mCurrentBlock != null)
				{
					bytesToEnd = mCurrentBlock.mNext != null ?
						mCurrentBlock.mLength - index :
						mCurrentBlock.Capacity - index;
				}

				if (bytesToEnd == 0)
				{
					// memory block is at its end
					// => continue writing the next memory block
					if (mCurrentBlock?.mNext != null)
					{
						mCurrentBlock = mCurrentBlock.mNext;
						bytesToEnd = mCurrentBlock.mLength;
					}
					else
					{
						if (!AppendNewBuffer(1)) mCurrentBlock = mCurrentBlock.mNext;
						bytesToEnd = mCurrentBlock.Capacity;
					}

					mCurrentBlockStartIndex = mPosition;
					index = 0;
				}

				// copy as many bytes as requested and possible
				if (bytesToEnd > 0)
				{
					mCurrentBlock.mBuffer[index] = value;
					mCurrentBlock.mLength = Math.Max(mCurrentBlock.mLength, index + 1);
					mPosition++;
					mLength = Math.Max(mLength, mPosition);
					return;
				}
			}
		}

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

			long count = 0;
			while (true)
			{
				// get index in the current memory block
				int index = (int)(mPosition - mCurrentBlockStartIndex);

				// determine how many bytes can be written to the current memory block
				int bytesToEnd = 0;
				if (mCurrentBlock != null)
				{
					bytesToEnd = mCurrentBlock.mNext != null ?
						mCurrentBlock.mLength - index :
						mCurrentBlock.Capacity - index;
				}

				if (bytesToEnd == 0)
				{
					// memory block is at its end
					// => continue writing the next memory block
					if (mCurrentBlock?.mNext != null)
					{
						mCurrentBlock = mCurrentBlock.mNext;
						bytesToEnd = mCurrentBlock.mLength;
					}
					else
					{
						if (!AppendNewBuffer(bytesInSourceStream - count)) mCurrentBlock = mCurrentBlock.mNext;
						bytesToEnd = mCurrentBlock.Capacity;
					}

					mCurrentBlockStartIndex = mPosition;
					index = 0;
				}

				// copy as many bytes as requested and possible
				int bytesRead = stream.Read(mCurrentBlock.mBuffer, index, bytesToEnd);
				mPosition += bytesRead;
				count += bytesRead;

				// update length of the memory block
				mCurrentBlock.mLength = Math.Max(mCurrentBlock.mLength, index + bytesRead);
				mLength = Math.Max(mLength, mPosition);

				// abort, if stream is at its end
				if (bytesRead < bytesToEnd) break;
			}

			return count;
		}

		/// <summary>
		/// Does not do anything for this stream (for interface compatibility only).
		/// </summary>
		public override void Flush()
		{

		}

		/// <summary>
		/// Appends a new buffer to the end of the chain.
		/// </summary>
		/// <param name="requestedSize">Number of bytes the buffer should contain at least.</param>
		/// <returns>
		/// true, if the new buffer is the first buffer;
		/// false, if the buffer is not the first buffer.
		/// </returns>
		private bool AppendNewBuffer(long requestedSize)
		{
			bool isFirstBuffer = mCapacity == 0;

			while (requestedSize > 0)
			{
				int blockSize = (int)Math.Min(requestedSize, int.MaxValue);
				blockSize = Math.Max(blockSize, mMinBlockSize);
				requestedSize -= blockSize;

				if (mCapacity == 0)
				{
					Debug.Assert(mCurrentBlock == null);
					mCurrentBlock = new ChainableMemoryBlock(blockSize);
					mFirstBlock = mCurrentBlock;
					mLastBlock = mCurrentBlock;
					mCapacity = mCurrentBlock.Capacity;
				}
				else
				{
					ChainableMemoryBlock block = new ChainableMemoryBlock(blockSize);
					mLastBlock.mNext = block;
					mLastBlock = block;
					mCapacity += block.Capacity;
				}
			}

			return isFirstBuffer;
		}

		/// <summary>
		/// Attaches a memory block or chain of memory blocks to the stream.
		/// </summary>
		/// <param name="buffer">Memory block to attach to the stream.</param>
		/// <remarks>
		/// This method allows you to exchange the underlying memory block buffer.
		/// The stream is reset, so the position is 0 after attaching the new buffer.
		/// </remarks>
		public void AttachBuffer(ChainableMemoryBlock buffer)
		{
			// exchange buffer
			mCurrentBlock = buffer;
			mFirstBlock = buffer;

			// update administrative variables appropriately
			mCurrentBlockStartIndex = 0;
			mPosition = 0;

			// determine capacity and length of the buffer
			mCapacity = 0;
			mLength = 0;
			ChainableMemoryBlock block = mFirstBlock;
			while (block != null)
			{
				mCapacity += block.mLength;
				mLength += block.mLength;
				mLastBlock = block;
				block = block.mNext;
			}

			// adjust the capacity, since only the last block must be extended up to its capacity
			// (the blocks in between must not be changed in length, since this would insert/remove data in the middle of the stream!)
			if (mLastBlock != null) {
				mCapacity = mCapacity - mLastBlock.mLength + mLastBlock.Capacity;
			}
		}

		/// <summary>
		/// Detaches the underlying memory block buffer from the stream.
		/// </summary>
		/// <returns>Underlying memory block buffer (can be a chained with other memory blocks).</returns>
		/// <remarks>
		/// This method allows you to detach the underlying buffer from the stream and use it in another place.
		/// The stream is empty afterwards.
		/// </remarks>
		public ChainableMemoryBlock DetachBuffer()
		{
			ChainableMemoryBlock buffer = mFirstBlock;
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
	}
}
