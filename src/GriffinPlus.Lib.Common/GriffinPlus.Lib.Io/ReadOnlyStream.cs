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
/// A stream that wraps another stream masking its capability to write.
/// </summary>
public sealed class ReadOnlyStream : Stream
{
	private readonly Stream mStream;
	private readonly bool   mLeaveOpen;

	/// <summary>
	/// Initializes a new instance of the <see cref="ReadOnlyStream"/> class.<br/>
	/// The created stream will close/dispose the specified stream when it is closed/disposed on its own.
	/// </summary>
	/// <param name="stream">Stream to wrap.</param>
	/// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
	public ReadOnlyStream(Stream stream) :
		this(stream, false) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="ReadOnlyStream"/> class.<br/>
	/// Allows to specify whether the created stream will close/dispose the specified stream when it is closed/disposed on its own.
	/// </summary>
	/// <param name="stream">Stream to wrap.</param>
	/// <param name="leaveOpen">
	/// <see langword="true"/> to close/dispose <paramref name="stream"/> when the stream is closed/disposed on its own;<br/>
	/// <see langword="false"/> to keep <paramref name="stream"/> alive when the stream is closed/disposed.
	/// </param>
	/// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
	public ReadOnlyStream(Stream stream, bool leaveOpen)
	{
		mStream = stream ?? throw new ArgumentNullException(nameof(stream));
		mLeaveOpen = leaveOpen;
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	protected override void Dispose(bool disposing)
	{
		if (!mLeaveOpen)
			mStream.Dispose();
	}

#if NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
	/// </summary>
	/// <returns>A task that represents the asynchronous dispose operation.</returns>
	public override ValueTask DisposeAsync()
	{
		return mLeaveOpen ? default : mStream.DisposeAsync();
	}
#elif NETSTANDARD2_0 || NET461 || NET48
	// These frameworks do not support IAsyncDisposable
#else
#error Unhandled target framework.
#endif

	/// <inheritdoc cref="IStream.CanRead"/>
	public override bool CanRead => mStream.CanRead;

	/// <inheritdoc cref="IStream.CanSeek"/>
	public override bool CanSeek => mStream.CanSeek;

	/// <inheritdoc cref="IStream.CanTimeout"/>
	public override bool CanTimeout => mStream.CanTimeout;

	/// <inheritdoc cref="IStream.CanWrite"/>
	public override bool CanWrite => false;

	/// <inheritdoc cref="IStream.Length"/>
	public override long Length => mStream.Length;

	/// <inheritdoc cref="IStream.Position"/>
	public override long Position
	{
		get => mStream.Position;
		set => mStream.Position = value;
	}

	/// <inheritdoc cref="IStream.ReadTimeout"/>
	public override int ReadTimeout
	{
		get => mStream.ReadTimeout;
		set => mStream.ReadTimeout = value;
	}

	/// <inheritdoc cref="IStream.WriteTimeout"/>
	public override int WriteTimeout
	{
		get => throw new InvalidOperationException("The stream does not support writing.");
		set => throw new InvalidOperationException("The stream does not support writing.");
	}

	/// <inheritdoc cref="IStream.BeginRead"/>
	public override IAsyncResult BeginRead(
		byte[]        buffer,
		int           offset,
		int           count,
		AsyncCallback callback,
		object        state)
	{
		return mStream.BeginRead(buffer, offset, count, callback, state);
	}

	/// <inheritdoc cref="IStream.BeginWrite"/>
	public override IAsyncResult BeginWrite(
		byte[]        buffer,
		int           offset,
		int           count,
		AsyncCallback callback,
		object        state)
	{
		throw new NotSupportedException("The stream does not support writing.");
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
		mStream.CopyTo(destination, bufferSize);
	}
#elif NETSTANDARD2_0 || NET461 || NET48
	// This method is not supported on these frameworks.
#else
#error Unhandled target framework.
#endif

	/// <inheritdoc cref="IStream.CopyToAsync(Stream,int,CancellationToken)"/>
	public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
	{
		return mStream.CopyToAsync(destination, bufferSize, cancellationToken);
	}

	/// <inheritdoc cref="IStream.EndRead(IAsyncResult)"/>
	public override int EndRead(IAsyncResult asyncResult)
	{
		return mStream.EndRead(asyncResult);
	}

	/// <inheritdoc cref="IStream.EndWrite(IAsyncResult)"/>
	public override void EndWrite(IAsyncResult asyncResult)
	{
		throw new NotSupportedException("The stream does not support writing.");
	}

	/// <inheritdoc cref="IStream.Flush()"/>
	public override void Flush()
	{
		mStream.Flush();
	}

	/// <inheritdoc cref="IStream.FlushAsync(CancellationToken)"/>
	public override Task FlushAsync(CancellationToken cancellationToken)
	{
		return mStream.FlushAsync(cancellationToken);
	}

#if NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
	/// <inheritdoc cref="IStream.Read(Span{byte})"/>
	public override int Read(Span<byte> buffer)
	{
		return mStream.Read(buffer);
	}
#elif NETSTANDARD2_0 || NET461 || NET48
	// This method is not supported on these frameworks.
#else
#error Unhandled target framework.
#endif

	/// <inheritdoc cref="IStream.Read(byte[], int, int)"/>
	public override int Read(byte[] buffer, int offset, int count)
	{
		return mStream.Read(buffer, offset, count);
	}

#if NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
	/// <inheritdoc cref="IStream.ReadAsync(Memory{byte},CancellationToken)"/>
	public override ValueTask<int> ReadAsync(
		Memory<byte>      buffer,
		CancellationToken cancellationToken = default)
	{
		return mStream.ReadAsync(buffer, cancellationToken);
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
		return mStream.ReadAsync(buffer, offset, count, cancellationToken);
	}

	/// <inheritdoc cref="IStream.ReadByte()"/>
	public override int ReadByte()
	{
		return mStream.ReadByte();
	}

	/// <inheritdoc cref="IStream.Seek(long,SeekOrigin)"/>
	public override long Seek(long offset, SeekOrigin origin)
	{
		return mStream.Seek(offset, origin);
	}

	/// <inheritdoc cref="IStream.SetLength(long)"/>
	public override void SetLength(long length)
	{
		throw new NotSupportedException("The stream does not support writing.");
	}

#if NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
	/// <inheritdoc cref="IStream.Write(ReadOnlySpan{byte})"/>
	public override void Write(ReadOnlySpan<byte> buffer)
	{
		throw new NotSupportedException("The stream does not support writing.");
	}
#elif NETSTANDARD2_0 || NET461 || NET48
	// This method is not supported on these frameworks.
#else
#error Unhandled target framework.
#endif

	/// <inheritdoc cref="IStream.Write(byte[], int, int)"/>
	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException("The stream does not support writing.");
	}

#if NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
	/// <inheritdoc cref="IStream.WriteAsync(ReadOnlyMemory{byte}, CancellationToken)"/>
	public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
	{
		throw new NotSupportedException("The stream does not support writing.");
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
		throw new NotSupportedException("The stream does not support writing.");
	}

	/// <inheritdoc cref="IStream.WriteByte(byte)"/>
	public override void WriteByte(byte value)
	{
		throw new NotSupportedException("The stream does not support writing.");
	}
}
