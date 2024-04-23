///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Io;

/// <summary>
/// A stream with a linked list of memory blocks as backing store.
/// This stream provides a thread-safe wrapper around the <see cref="MemoryBlockStream"/>.
/// </summary>
public sealed class SynchronizedMemoryBlockStream : Stream, IMemoryBlockStream
{
	private readonly MemoryBlockStream mStream;
	private readonly SemaphoreSlim     mLock;

	#region Construction and Disposal

	/// <summary>
	/// Initializes a new instance of the <see cref="SynchronizedMemoryBlockStream"/> class.
	/// Buffers are allocated on the heap.
	/// The block size defaults to 80 kByte.
	/// The stream is seekable and grows as data is written.
	/// </summary>
	public SynchronizedMemoryBlockStream() : this(MemoryBlockStream.DefaultBlockSize, null, false) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="SynchronizedMemoryBlockStream"/> class.
	/// Buffers are rented from the specified array pool.
	/// The block size defaults to 80 kByte.
	/// The stream is seekable and grows as data is written.
	/// </summary>
	/// <param name="pool">Array pool to use for allocating buffers.</param>
	/// <exception cref="ArgumentNullException"><paramref name="pool"/> is <c>null</c>.</exception>
	public SynchronizedMemoryBlockStream(ArrayPool<byte> pool) : this(MemoryBlockStream.DefaultBlockSize, pool, false) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="SynchronizedMemoryBlockStream"/> class with a specific block size.
	/// Buffers are allocated on the heap.
	/// The stream is seekable and grows as data is written.
	/// </summary>
	/// <param name="blockSize">Size of a block in the stream.</param>
	/// <exception cref="ArgumentException">The specified block size is less than or equal to 0.</exception>
	public SynchronizedMemoryBlockStream(int blockSize) : this(blockSize, null, false) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="SynchronizedMemoryBlockStream"/> class with a specific block size.
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
	/// <param name="releaseReadBlocks">
	/// <c>true</c> to release memory blocks that have been read (makes the stream unseekable);
	/// <c>false</c> to keep written memory blocks enabling seeking and changing the length of the stream.
	/// </param>
	/// <exception cref="ArgumentException">The specified block size is less than or equal to 0.</exception>
	public SynchronizedMemoryBlockStream(
		int             blockSize         = MemoryBlockStream.DefaultBlockSize,
		ArrayPool<byte> pool              = null,
		bool            releaseReadBlocks = false)
	{
		mStream = new MemoryBlockStream(blockSize, pool, releaseReadBlocks);
		mLock = new SemaphoreSlim(1);
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
		if (!disposing)
			return;

		try
		{
			mLock.Wait();
			mStream.Dispose();
		}
		finally
		{
			mLock.Release();
		}
	}

#if NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
	/// <summary>
	/// Asynchronously disposes the stream releasing the underlying memory-block chain
	/// (returns rented buffers to their array pool, if necessary).
	/// </summary>
	public override async ValueTask DisposeAsync()
	{
		try
		{
			await mLock.WaitAsync().ConfigureAwait(false);
			await mStream.DisposeAsync().ConfigureAwait(false);
		}
		finally
		{
			mLock.Release();
		}
	}
#elif NETSTANDARD2_0 || NET461 || NET48
	// This method is not supported by the Stream class.
#else
#error Unhandled target framework.
#endif

	#endregion

	#region Stream Capabilities

	/// <summary>
	/// Gets a value indicating whether the stream supports reading (always true).
	/// </summary>
	public override bool CanRead
	{
		get
		{
			try
			{
				mLock.Wait();
				return mStream.CanRead;
			}
			finally
			{
				mLock.Release();
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the stream supports writing (always true).
	/// </summary>
	public override bool CanWrite
	{
		get
		{
			try
			{
				mLock.Wait();
				return mStream.CanWrite;
			}
			finally
			{
				mLock.Release();
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the stream supports seeking.
	/// </summary>
	public override bool CanSeek
	{
		get
		{
			try
			{
				mLock.Wait();
				return mStream.CanSeek;
			}
			finally
			{
				mLock.Release();
			}
		}
	}

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
		get
		{
			try
			{
				mLock.Wait();
				return mStream.Position;
			}
			finally
			{
				mLock.Release();
			}
		}
		set
		{
			try
			{
				mLock.Wait();
				mStream.Position = value;
			}
			finally
			{
				mLock.Release();
			}
		}
	}

	#endregion

	#region Length

	/// <summary>
	/// Gets the length of the current stream.
	/// </summary>
	public override long Length
	{
		get
		{
			try
			{
				mLock.Wait();
				return mStream.Length;
			}
			finally
			{
				mLock.Release();
			}
		}
	}

	#endregion

	#region ReleasesReadBlocks

	/// <summary>
	/// Gets a value indicating whether the stream releases read buffers.
	/// </summary>
	public bool ReleasesReadBlocks
	{
		get
		{
			try
			{
				mLock.Wait();
				return mStream.ReleasesReadBlocks;
			}
			finally
			{
				mLock.Release();
			}
		}
	}

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
		try
		{
			mLock.Wait();
			mStream.SetLength(length);
		}
		finally
		{
			mLock.Release();
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
		try
		{
			mLock.Wait();
			return mStream.Seek(offset, origin);
		}
		finally
		{
			mLock.Release();
		}
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
		try
		{
			mLock.Wait();
			return mStream.Read(buffer, offset, count);
		}
		finally
		{
			mLock.Release();
		}
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
	public override async Task<int> ReadAsync(
		byte[]            buffer,
		int               offset,
		int               count,
		CancellationToken cancellationToken) // overload without CancellationToken is defined in base class
	{
		try
		{
			await mLock.WaitAsync(cancellationToken).ConfigureAwait(false);
			return await mStream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			mLock.Release();
		}
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
#elif NETSTANDARD2_1 || NETCOREAPP3_0 || NET5_0 || NET6_0 || NET7_0 || NET8_0
		override
#else
#error Unhandled target framework.
#endif
		int Read(Span<byte> buffer)
	{
		try
		{
			mLock.Wait();
			return mStream.Read(buffer);
		}
		finally
		{
			mLock.Release();
		}
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
		async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
	{
		try
		{
			await mLock.WaitAsync(cancellationToken).ConfigureAwait(false);
			return await mStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			mLock.Release();
		}
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
		try
		{
			mLock.Wait();
			return mStream.ReadByte();
		}
		finally
		{
			mLock.Release();
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
		try
		{
			mLock.Wait();
			mStream.Write(buffer, offset, count);
		}
		finally
		{
			mLock.Release();
		}
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
	public override async Task WriteAsync(
		byte[]            buffer,
		int               offset,
		int               count,
		CancellationToken cancellationToken) // overload without CancellationToken is defined in base class
	{
		try
		{
			await mLock.WaitAsync(cancellationToken).ConfigureAwait(false);
			await mStream.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			mLock.Release();
		}
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
		try
		{
			mLock.Wait();
			mStream.Write(buffer);
		}
		finally
		{
			mLock.Release();
		}
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
		async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
	{
		try
		{
			await mLock.WaitAsync(cancellationToken).ConfigureAwait(false);
			await mStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			mLock.Release();
		}
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
		try
		{
			mLock.Wait();
			mStream.WriteByte(value);
		}
		finally
		{
			mLock.Release();
		}
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
		try
		{
			mLock.Wait();
			return mStream.Write(stream);
		}
		finally
		{
			mLock.Release();
		}
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
		try
		{
			await mLock.WaitAsync(cancellationToken).ConfigureAwait(false);
			return await mStream.WriteAsync(stream, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			mLock.Release();
		}
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
		try
		{
			mLock.Wait();
			mStream.CopyTo(destination, bufferSize);
		}
		finally
		{
			mLock.Release();
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
		try
		{
			await mLock.WaitAsync(cancellationToken).ConfigureAwait(false);
			await mStream.CopyToAsync(destination, bufferSize, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			mLock.Release();
		}
	}

	#endregion

	#region Flushing

	/// <summary>
	/// Flushes the stream (does not do anything for this stream).
	/// </summary>
	public override void Flush()
	{
		try
		{
			mLock.Wait();
			mStream.Flush();
		}
		finally
		{
			mLock.Release();
		}
	}

	/// <summary>
	/// Flushes the stream asynchronously (does not do anything for this stream).
	/// </summary>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests.
	/// The default value is <see cref="CancellationToken.None"/>.
	/// </param>
	public override async Task FlushAsync(CancellationToken cancellationToken) // overload without CancellationToken is defined in base class
	{
		try
		{
			await mLock.WaitAsync(cancellationToken).ConfigureAwait(false);
			await mStream.FlushAsync(cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			mLock.Release();
		}
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
		try
		{
			mLock.Wait();
			mStream.AppendBuffer(buffer);
		}
		finally
		{
			mLock.Release();
		}
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
	public async Task AppendBufferAsync(ChainableMemoryBlock buffer, CancellationToken cancellationToken = default)
	{
		try
		{
			await mLock.WaitAsync(cancellationToken).ConfigureAwait(false);
			await mStream.AppendBufferAsync(buffer, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			mLock.Release();
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
		try
		{
			mLock.Wait();
			mStream.AttachBuffer(buffer);
		}
		finally
		{
			mLock.Release();
		}
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
	public async Task AttachBufferAsync(ChainableMemoryBlock buffer, CancellationToken cancellationToken = default)
	{
		try
		{
			await mLock.WaitAsync(cancellationToken).ConfigureAwait(false);
			await mStream.AttachBufferAsync(buffer, cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			mLock.Release();
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
		try
		{
			mLock.Wait();
			return mStream.DetachBuffer();
		}
		finally
		{
			mLock.Release();
		}
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
	public async Task<ChainableMemoryBlock> DetachBufferAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await mLock.WaitAsync(cancellationToken).ConfigureAwait(false);
			return await mStream.DetachBufferAsync(cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			mLock.Release();
		}
	}

	#endregion
}
