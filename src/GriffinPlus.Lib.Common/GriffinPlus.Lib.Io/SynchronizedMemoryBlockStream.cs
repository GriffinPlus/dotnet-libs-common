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
	/// Initializes a new instance of the <see cref="SynchronizedMemoryBlockStream"/> class.<br/>
	/// Buffers are allocated on the heap.<br/>
	/// The block size defaults to 80 kByte.<br/>
	/// The stream is seekable and grows as data is written.
	/// </summary>
	public SynchronizedMemoryBlockStream() : this(MemoryBlockStream.DefaultBlockSize, null, false) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="SynchronizedMemoryBlockStream"/> class.<br/>
	/// Buffers are rented from the specified array pool.<br/>
	/// The block size defaults to 80 kByte.<br/>
	/// The stream is seekable and grows as data is written.
	/// </summary>
	/// <param name="pool">Array pool to use for allocating buffers.</param>
	/// <exception cref="ArgumentNullException"><paramref name="pool"/> is <see langword="null"/>.</exception>
	public SynchronizedMemoryBlockStream(ArrayPool<byte> pool) : this(MemoryBlockStream.DefaultBlockSize, pool, false) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="SynchronizedMemoryBlockStream"/> class with a specific block size.<br/>
	/// Buffers are allocated on the heap.<br/>
	/// The stream is seekable and grows as data is written.
	/// </summary>
	/// <param name="blockSize">Size of a block in the stream.</param>
	/// <exception cref="ArgumentException">The specified block size is less than or equal to 0.</exception>
	public SynchronizedMemoryBlockStream(int blockSize) : this(blockSize, null, false) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="SynchronizedMemoryBlockStream"/> class with a specific block size.<br/>
	/// Buffers can be allocated on the heap or rented from the specified array pool.<br/>
	/// The stream can be configured to release buffers as data is read (makes the stream unseekable).
	/// </summary>
	/// <param name="blockSize">
	/// Size of a block in the stream.
	/// The actual block size may be greater than the specified size, if the buffer is rented from an array pool.
	/// </param>
	/// <param name="pool">
	/// Array pool to rent buffers from (<see langword="null"/> to allocate buffers on the heap).
	/// </param>
	/// <param name="releaseReadBlocks">
	/// <see langword="true"/> to release memory blocks that have been read (makes the stream unseekable);<br/>
	/// <see langword="false"/> to keep written memory blocks enabling seeking and changing the length of the stream.
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

	/// <inheritdoc cref="MemoryBlockStream.Dispose(bool)"/>
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
	/// <inheritdoc cref="MemoryBlockStream.DisposeAsync()"/>
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

	/// <inheritdoc cref="MemoryBlockStream.CanRead"/>
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

	/// <inheritdoc cref="MemoryBlockStream.CanWrite"/>
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

	/// <inheritdoc cref="MemoryBlockStream.CanSeek"/>
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

	#region long Position

	/// <inheritdoc cref="MemoryBlockStream.Position"/>
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

	#region long Length

	/// <inheritdoc cref="MemoryBlockStream.Length"/>
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

	#region bool ReleasesReadBlocks

	/// <inheritdoc cref="MemoryBlockStream.ReleasesReadBlocks"/>
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

	#region void SetLength(long length)

	/// <inheritdoc cref="MemoryBlockStream.SetLength(long)"/>
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

	/// <inheritdoc cref="MemoryBlockStream.Seek(long,SeekOrigin)"/>
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

	/// <inheritdoc cref="MemoryBlockStream.Read(byte[], int, int)"/>
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

	/// <inheritdoc cref="MemoryBlockStream.ReadAsync(byte[], int, int, CancellationToken)"/>
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

	/// <inheritdoc cref="MemoryBlockStream.Read(Span{byte})"/>
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

	/// <inheritdoc cref="MemoryBlockStream.ReadAsync(Memory{byte}, CancellationToken)"/>
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

	/// <inheritdoc cref="MemoryBlockStream.ReadByte()"/>
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

	/// <inheritdoc cref="MemoryBlockStream.Write(byte[], int, int)"/>
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

	/// <inheritdoc cref="MemoryBlockStream.WriteAsync(byte[], int, int, CancellationToken)"/>
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

	/// <inheritdoc cref="MemoryBlockStream.Write(ReadOnlySpan{byte})"/>
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

	/// <inheritdoc cref="MemoryBlockStream.WriteAsync(ReadOnlyMemory{byte}, CancellationToken)"/>
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

	/// <inheritdoc cref="MemoryBlockStream.WriteByte(byte)"/>
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

	/// <inheritdoc cref="MemoryBlockStream.Write(Stream)"/>
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

	/// <inheritdoc cref="MemoryBlockStream.WriteAsync(Stream, CancellationToken)"/>
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

	/// <inheritdoc cref="MemoryBlockStream.CopyTo(Stream, int)"/>
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

	/// <inheritdoc cref="MemoryBlockStream.CopyToAsync(Stream, int, CancellationToken)"/>
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

	/// <inheritdoc cref="MemoryBlockStream.Flush()"/>
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

	/// <inheritdoc cref="MemoryBlockStream.FlushAsync(CancellationToken)"/>
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

	/// <inheritdoc/>
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

	/// <inheritdoc/>
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

	/// <inheritdoc/>
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

	/// <inheritdoc/>
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

	/// <inheritdoc/>
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

	/// <inheritdoc/>
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
