///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Io;

/// <summary>
/// A <see cref="MemoryStream"/> for testing purposes only.<br/>
/// It allows to define its capabilities/properties:<br/>
/// - <see cref="Stream.CanRead"/><br/>
/// - <see cref="Stream.CanSeek"/><br/>
/// - <see cref="Stream.CanTimeout"/><br/>
/// - <see cref="Stream.CanWrite"/><br/>
/// - <see cref="Stream.ReadTimeout"/><br/>
/// - <see cref="Stream.WriteTimeout"/>
/// </summary>
public sealed class MockMemoryStream : MemoryStream
{
	private int mReadTimeout;
	private int mWriteTimeout;

	/// <summary>
	/// Initializes a new instance of the <see cref="MockMemoryStream"/> class.
	/// </summary>
	public MockMemoryStream(
		bool canRead,
		bool canWrite,
		bool canSeek,
		bool canTimeout,
		int  readTimeout,
		int  writeTimeout)
	{
		CanRead = canRead;
		CanWrite = canWrite;
		CanSeek = canSeek;
		CanTimeout = canTimeout;
		mReadTimeout = readTimeout;
		mWriteTimeout = writeTimeout;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MockMemoryStream"/> class with predefined data.
	/// </summary>
	public MockMemoryStream(
		bool   canRead,
		bool   canWrite,
		bool   canSeek,
		bool   canTimeout,
		int    readTimeout,
		int    writeTimeout,
		byte[] buffer) : base(buffer, true)
	{
		CanRead = canRead;
		CanWrite = canWrite;
		CanSeek = canSeek;
		CanTimeout = canTimeout;
		mReadTimeout = readTimeout;
		mWriteTimeout = writeTimeout;
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	protected override void Dispose(bool disposing)
	{
		IsDisposed = true;
	}

	/// <summary>
	/// Gets a value indicating whether the stream was closed/disposed.
	/// </summary>
	public bool IsDisposed { get; private set; }

	/// <summary>
	/// Gets a value indicating how often the stream was flushed.
	/// </summary>
	public int FlushCounter { get; private set; }

	/// <inheritdoc cref="IStream.CanRead"/>
	public override bool CanRead { get; }

	/// <inheritdoc cref="IStream.CanSeek"/>
	public override bool CanSeek { get; }

	/// <inheritdoc cref="IStream.CanTimeout"/>
	public override bool CanTimeout { get; }

	/// <inheritdoc cref="IStream.CanWrite"/>
	public override bool CanWrite { get; }

	/// <inheritdoc cref="IStream.Length"/>
	public override long Length
	{
		get
		{
			if (!CanSeek) throw new NotSupportedException("The stream does not support seeking.");
			return base.Length;
		}
	}

	/// <inheritdoc cref="IStream.Position"/>
	public override long Position
	{
		get
		{
			if (!CanSeek) throw new NotSupportedException("The stream does not support seeking.");
			return base.Position;
		}

		set
		{
			if (!CanSeek) throw new NotSupportedException("The stream does not support seeking.");
			base.Position = value;
		}
	}

	/// <inheritdoc cref="IStream.ReadTimeout"/>
	public override int ReadTimeout
	{
		get
		{
			if (!CanRead || !CanTimeout) throw new InvalidOperationException("The stream does not support both timeouts and reading.");
			return mReadTimeout;
		}

		set
		{
			if (!CanRead || !CanTimeout) throw new InvalidOperationException("The stream does not support both timeouts and reading.");
			mReadTimeout = value;
		}
	}

	/// <inheritdoc cref="IStream.WriteTimeout"/>
	public override int WriteTimeout
	{
		get
		{
			if (!CanWrite || !CanTimeout) throw new InvalidOperationException("The stream does not support both timeouts and writing.");
			return mWriteTimeout;
		}

		set
		{
			if (!CanWrite || !CanTimeout) throw new InvalidOperationException("The stream does not support both timeouts and writing.");
			mWriteTimeout = value;
		}
	}

	/// <inheritdoc cref="IStream.BeginRead"/>
	public override IAsyncResult BeginRead(
		byte[]        buffer,
		int           offset,
		int           count,
		AsyncCallback callback,
		object        state)
	{
		if (!CanRead) throw new NotSupportedException("The stream does not support reading.");
		return base.BeginRead(buffer, offset, count, callback, state);
	}

	/// <inheritdoc cref="IStream.BeginWrite"/>
	public override IAsyncResult BeginWrite(
		byte[]        buffer,
		int           offset,
		int           count,
		AsyncCallback callback,
		object        state)
	{
		if (!CanWrite) throw new NotSupportedException("The stream does not support writing.");
		return base.BeginWrite(buffer, offset, count, callback, state);
	}

	/// <inheritdoc cref="IStream.Close"/>
	public override void Close()
	{
		Dispose(true);
	}

#if NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
	/// <inheritdoc cref="IStream.CopyTo(Stream,int)"/>
	public override void CopyTo(Stream destination, int bufferSize)
	{
		if (!CanRead) throw new NotSupportedException("The stream does not support reading.");
		base.CopyTo(destination, bufferSize);
	}
#elif NETSTANDARD2_0 || NET461 || NET48
	// This method is not supported on these frameworks.
#else
#error Unhandled target framework.
#endif

	/// <inheritdoc cref="IStream.CopyToAsync(Stream,int,CancellationToken)"/>
	public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
	{
		if (!CanRead) throw new NotSupportedException("The stream does not support reading.");
		return base.CopyToAsync(destination, bufferSize, cancellationToken);
	}

	/// <inheritdoc cref="IStream.Flush()"/>
	public override void Flush()
	{
		FlushCounter++;
	}

	/// <inheritdoc cref="IStream.FlushAsync(CancellationToken)"/>
	public override Task FlushAsync(CancellationToken cancellationToken)
	{
		FlushCounter++;
		return Task.CompletedTask;
	}

#if NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
	/// <inheritdoc cref="IStream.Read(Span{byte})"/>
	public override int Read(Span<byte> buffer)
	{
		if (!CanRead) throw new NotSupportedException("The stream does not support reading.");
		return base.Read(buffer);
	}
#elif NETSTANDARD2_0 || NET461 || NET48
	// This method is not supported on these frameworks.
#else
#error Unhandled target framework.
#endif

	/// <inheritdoc cref="IStream.Read(byte[], int, int)"/>
	public override int Read(byte[] buffer, int offset, int count)
	{
		if (!CanRead) throw new NotSupportedException("The stream does not support reading.");
		return base.Read(buffer, offset, count);
	}

#if NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
	/// <inheritdoc cref="IStream.ReadAsync(Memory{byte},CancellationToken)"/>
	public override ValueTask<int> ReadAsync(
		Memory<byte>      buffer,
		CancellationToken cancellationToken = default)
	{
		if (!CanRead) throw new NotSupportedException("The stream does not support reading.");
		return base.ReadAsync(buffer, cancellationToken);
	}
#elif NETSTANDARD2_0 || NET461 || NET48
	// This method is not supported on these frameworks.
#else
#error Unhandled target framework.
#endif

	/// <inheritdoc cref="IStream.ReadAsync(byte[],int,int,CancellationToken)"/>
	public override Task<int> ReadAsync(
		byte[]            buffer,
		int               offset,
		int               count,
		CancellationToken cancellationToken)
	{
		if (!CanRead) throw new NotSupportedException("The stream does not support reading.");
		return base.ReadAsync(buffer, offset, count, cancellationToken);
	}

	/// <inheritdoc cref="IStream.ReadByte()"/>
	public override int ReadByte()
	{
		if (!CanRead) throw new NotSupportedException("The stream does not support reading.");
		return base.ReadByte();
	}

	/// <inheritdoc cref="IStream.Seek(long,SeekOrigin)"/>
	public override long Seek(long offset, SeekOrigin origin)
	{
		if (!CanSeek) throw new NotSupportedException("The stream does not support seeking.");
		return base.Seek(offset, origin);
	}

	/// <inheritdoc cref="IStream.SetLength(long)"/>
	public override void SetLength(long length)
	{
		if (!CanWrite) throw new NotSupportedException("The stream does not support writing.");
		if (!CanSeek) throw new NotSupportedException("The stream does not support seeking.");
		base.SetLength(length);
	}

#if NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
	/// <inheritdoc cref="IStream.Write(ReadOnlySpan{byte})"/>
	public override void Write(ReadOnlySpan<byte> buffer)
	{
		if (!CanWrite) throw new NotSupportedException("The stream does not support writing.");
		base.Write(buffer);
	}
#elif NETSTANDARD2_0 || NET461 || NET48
	// This method is not supported on these frameworks.
#else
#error Unhandled target framework.
#endif

	/// <inheritdoc cref="IStream.Write(byte[], int, int)"/>
	public override void Write(byte[] buffer, int offset, int count)
	{
		if (!CanWrite) throw new NotSupportedException("The stream does not support writing.");
		base.Write(buffer, offset, count);
	}

#if NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
	/// <inheritdoc cref="IStream.WriteAsync(ReadOnlyMemory{byte}, CancellationToken)"/>
	public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
	{
		if (!CanWrite) throw new NotSupportedException("The stream does not support writing.");
		return base.WriteAsync(buffer, cancellationToken);
	}
#elif NETSTANDARD2_0 || NET461 || NET48
	// This method is not supported on these frameworks.
#else
#error Unhandled target framework.
#endif

	/// <inheritdoc cref="IStream.WriteAsync(byte[], int, int, CancellationToken)"/>
	public override Task WriteAsync(
		byte[]            buffer,
		int               offset,
		int               count,
		CancellationToken cancellationToken)
	{
		if (!CanWrite) throw new NotSupportedException("The stream does not support writing.");
		return base.WriteAsync(buffer, offset, count, cancellationToken);
	}

	/// <inheritdoc cref="IStream.WriteByte(byte)"/>
	public override void WriteByte(byte value)
	{
		if (!CanWrite) throw new NotSupportedException("The stream does not support writing.");
		base.WriteByte(value);
	}
}
