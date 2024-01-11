///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Io
{

	/// <summary>
	/// Interface of Griffin+ streams basically interfacing the functionality of the <see cref="Stream"/> class
	/// plus span support on target frameworks where <see cref="Stream"/> does not support spans on its own.
	/// </summary>
	public interface IStream :
#if NETSTANDARD2_0 || NET461 || NET48
		IDisposable
#elif NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
		IDisposable,
		IAsyncDisposable
#else
#error Unhandled target framework.
#endif
	{
		/// <summary>
		/// Gets a value indicating whether the stream supports reading.
		/// </summary>
		/// <value><c>true</c> if the stream supports reading; otherwise, <c>false</c>.</value>
		bool CanRead { get; }

		/// <summary>
		/// Gets a value indicating whether the stream supports writing.
		/// </summary>
		/// <value><c>true</c> if the stream supports writing; otherwise, <c>false</c>.</value>
		bool CanWrite { get; }

		/// <summary>
		/// Gets a value indicating whether the stream supports seeking.
		/// </summary>
		bool CanSeek { get; }

		/// <summary>
		/// Gets a value that determines whether the current stream can time out.
		/// </summary>
		/// <value>A value that determines whether the current stream can time out.</value>
		bool CanTimeout { get; }

		/// <summary>
		/// Gets or sets a value, in milliseconds, that determines how long the stream will attempt to read before timing out.
		/// </summary>
		/// <value>A value, in milliseconds, that determines how long the stream will attempt to read before timing out.</value>
		/// <exception cref="InvalidOperationException">The stream does not support reading.</exception>
		int ReadTimeout { get; set; }

		/// <summary>
		/// Gets or sets a value, in milliseconds, that determines how long the stream will attempt to write before timing out.
		/// </summary>
		/// <value>A value, in milliseconds, that determines how long the stream will attempt to write before timing out.</value>
		/// <exception cref="InvalidOperationException">The stream does not support writing.</exception>
		int WriteTimeout { get; set; }

		/// <summary>
		/// Gets or sets the current position within the stream.
		/// </summary>
		/// <value>The current position within the stream.</value>
		/// <exception cref="ArgumentException">The position is out of bounds when trying to set it.</exception>
		/// <exception cref="NotSupportedException">The stream does not support seeking.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		long Position { get; set; }

		/// <summary>
		/// Gets the length of the current stream.
		/// </summary>
		/// <value>A long value representing the length of the stream in bytes.</value>
		/// <exception cref="NotSupportedException">The stream does not support seeking.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		long Length { get; }

		/// <summary>
		/// Begins an asynchronous read operation.
		/// Consider using <see cref="ReadAsync(byte[],int,int)"/> instead.
		/// </summary>
		/// <param name="buffer">The buffer to read the data into.</param>
		/// <param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing data read from the stream.</param>
		/// <param name="count">The maximum number of bytes to read.</param>
		/// <param name="callback">An optional asynchronous callback, to be called when the read is complete.</param>
		/// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
		/// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous read, which could still be pending.</returns>
		/// <exception cref="ArgumentException">One or more of the arguments is invalid.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="NotSupportedException">The stream does not support reading.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		IAsyncResult BeginRead(
			byte[]        buffer,
			int           offset,
			int           count,
			AsyncCallback callback,
			object        state);

		/// <summary>
		/// Begins an asynchronous write operation.
		/// Consider using <see cref="WriteAsync(byte[],int,int)"/> instead.
		/// </summary>
		/// <param name="buffer">The buffer to write data from.</param>
		/// <param name="offset">The byte offset in <paramref name="buffer"/> from which to begin writing.</param>
		/// <param name="count">The maximum number of bytes to write.</param>
		/// <param name="callback">An optional asynchronous callback, to be called when the write is complete.</param>
		/// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
		/// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous write, which could still be pending.</returns>
		/// <exception cref="ArgumentException">One or more of the arguments is invalid.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="NotSupportedException">The stream does not support writing.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		IAsyncResult BeginWrite(
			byte[]        buffer,
			int           offset,
			int           count,
			AsyncCallback callback,
			object        state);

		/// <summary>
		/// Closes the current stream and releases any resources (such as sockets and file handles) associated with the current stream.
		/// Instead of calling this method, ensure that the stream is properly disposed.
		/// </summary>
		void Close();

		/// <summary>
		/// Reads the bytes from the current stream and writes them to another stream.
		/// </summary>
		/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
		/// <exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
		/// <exception cref="NotSupportedException">The current stream does not support reading or the destination stream does not support writing.</exception>
		/// <exception cref="ObjectDisposedException">Either the current stream or <paramref name="destination"/> has been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		void CopyTo(Stream destination);

		/// <summary>
		/// Reads the bytes from the current stream and writes them to another stream, using a specified buffer size.
		/// </summary>
		/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
		/// <param name="bufferSize">The size of the buffer. This value must be greater than zero. The default size is 81920.</param>
		/// <exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> is negative or zero.</exception>
		/// <exception cref="NotSupportedException">The current stream does not support reading or the destination stream does not support writing.</exception>
		/// <exception cref="ObjectDisposedException">Either the current stream or <paramref name="destination"/> have been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		void CopyTo(Stream destination, int bufferSize);

		/// <summary>
		/// Asynchronously reads the bytes from the current stream and writes them to another stream,
		/// using a specified buffer size and cancellation token.
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
		/// <returns>A task that represents the asynchronous copy operation.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"> <paramref name="bufferSize"/> is negative or zero.</exception>
		/// <exception cref="ObjectDisposedException">Either the current stream or the destination stream is disposed.</exception>
		/// <exception cref="NotSupportedException">The current stream does not support reading or the destination stream does not support writing.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken);

#if NETSTANDARD2_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
		/// <summary>
		/// Asynchronously reads the bytes from the current stream and writes them to another stream,
		/// using a specified cancellation token.
		/// </summary>
		/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
		/// <param name="cancellationToken">
		/// The token to monitor for cancellation requests.
		/// The default value is <see cref="CancellationToken.None"/>.
		/// </param>
		/// <returns>A task that represents the asynchronous copy operation.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
		/// <exception cref="ObjectDisposedException">Either the current stream or the destination stream is disposed.</exception>
		/// <exception cref="NotSupportedException">The current stream does not support reading or the destination stream does not support writing.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		Task CopyToAsync(Stream destination, CancellationToken cancellationToken);
#elif NETSTANDARD2_0 || NET461 || NET48
		// This method is not supported by the Stream class.
#else
#error Unhandled target framework.
#endif

		/// <summary>
		/// Asynchronously reads the bytes from the current stream and writes them to another stream,
		/// using a specified buffer size.
		/// </summary>
		/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
		/// <param name="bufferSize">
		/// The size, in bytes, of the buffer.
		/// This value must be greater than zero.
		/// The default size is 81920.
		/// </param>
		/// <returns>A task that represents the asynchronous copy operation.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"> <paramref name="bufferSize"/> is negative or zero.</exception>
		/// <exception cref="ObjectDisposedException">Either the current stream or the destination stream is disposed.</exception>
		/// <exception cref="NotSupportedException">The current stream does not support reading or the destination stream does not support writing.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		Task CopyToAsync(Stream destination, int bufferSize);

		/// <summary>
		/// Asynchronously reads the bytes from the current stream and writes them to another stream.
		/// </summary>
		/// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
		/// <returns>A task that represents the asynchronous copy operation.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
		/// <exception cref="ObjectDisposedException">Either the current stream or the destination stream is disposed.</exception>
		/// <exception cref="NotSupportedException">The current stream does not support reading or the destination stream does not support writing.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		Task CopyToAsync(Stream destination);

		/// <summary>
		/// Waits for a pending asynchronous read to complete.
		/// Consider using <see cref="ReadAsync(byte[],int,int)"/> instead.
		/// </summary>
		/// <param name="asyncResult">A reference to the outstanding asynchronous I/O request.</param>
		/// <returns>
		/// The number of bytes read from the stream, between zero (0) and the number of bytes you requested. Streams return
		/// zero (0) only at the end of the stream, otherwise, they should block until at least one byte is available.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		/// A handle to the pending read operation is not available.
		/// -or-
		/// The pending operation does not support reading.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="asyncResult"/> did not originate from a <see cref="BeginRead"/> method on the current stream.
		/// </exception>
		/// <exception cref="IOException">The stream is closed or an internal error has occurred.</exception>
		int EndRead(IAsyncResult asyncResult);

		/// <summary>
		/// Waits for a pending asynchronous write to complete.
		/// Consider using <see cref="WriteAsync(byte[],int,int)"/> instead.
		/// </summary>
		/// <param name="asyncResult">A reference to the outstanding asynchronous I/O request.</param>
		/// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">
		/// A handle to the pending write operation is not available.
		/// -or-
		/// The pending operation does not support writing.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="asyncResult"/> did not originate from a <see cref="BeginWrite"/> method on the current stream.
		/// </exception>
		/// <exception cref="IOException">The stream is closed or an internal error has occurred.</exception>
		void EndWrite(IAsyncResult asyncResult);

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		void Flush();

		/// <summary>
		/// Asynchronously clears all buffers for this stream and causes any buffered data to be written to the underlying device.
		/// </summary>
		/// <returns>A task that represents the asynchronous flush operation.</returns>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		Task FlushAsync();

		/// <summary>
		/// Asynchronously clears all buffers for this stream, causes any buffered data to be written to the underlying device,
		/// and monitors cancellation requests.
		/// </summary>
		/// <param name="cancellationToken">
		/// The token to monitor for cancellation requests.
		/// The default value is <see cref="CancellationToken.None"/>.
		/// </param>
		/// <returns>A task that represents the asynchronous flush operation.</returns>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		Task FlushAsync(CancellationToken cancellationToken);

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
		/// <exception cref="NotSupportedException">The stream does not support reading.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		int Read(Span<byte> buffer);

		/// <summary>
		/// Reads a sequence of bytes from the stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">
		/// An array of bytes. When this method returns, the buffer contains the specified byte array with the values between
		/// <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes
		/// read from the current source.
		/// </param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>
		/// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many
		/// bytes are not currently available, or zero (0) if the end of the stream has been reached.
		/// </returns>
		/// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
		/// <exception cref="NotSupportedException">The stream does not support reading.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		int Read(byte[] buffer, int offset, int count);

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
		/// <exception cref="NotSupportedException">The stream does not support reading.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		ValueTask<int> ReadAsync(
			Memory<byte>      buffer,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously reads a sequence of bytes from the current stream and advances the position within the stream by the
		/// number of bytes read.
		/// </summary>
		/// <param name="buffer">The buffer to write the data into.</param>
		/// <param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing data from the stream.</param>
		/// <param name="count">The maximum number of bytes to read.</param>
		/// <returns>
		/// A task that represents the asynchronous read operation.
		/// The value contains the total number of bytes read into the buffer.
		/// The result value can be less than the number of bytes requested if the number of bytes currently available
		/// is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
		/// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.</exception>
		/// <exception cref="NotSupportedException">The stream does not support reading.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="InvalidOperationException">The stream is currently in use by a previous read operation.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		Task<int> ReadAsync(
			byte[] buffer,
			int    offset,
			int    count);

		/// <summary>
		/// Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the
		/// number of bytes read, and monitors cancellation requests.
		/// </summary>
		/// <param name="buffer">The buffer to write the data into.</param>
		/// <param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing data from the stream.</param>
		/// <param name="count">The maximum number of bytes to read.</param>
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
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
		/// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.</exception>
		/// <exception cref="NotSupportedException">The stream does not support reading.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="InvalidOperationException">The stream is currently in use by a previous read operation.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		Task<int> ReadAsync(
			byte[]            buffer,
			int               offset,
			int               count,
			CancellationToken cancellationToken);

		/// <summary>
		/// Reads a byte from the stream and advances the position within the stream by one byte,
		/// or returns -1 if at the end of the stream.
		/// </summary>
		/// <returns>
		/// The unsigned byte cast to an <see cref="int"/>;
		/// -1, if at the end of the stream.
		/// </returns>
		/// <exception cref="NotSupportedException">The stream does not support reading.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		int ReadByte();

		/// <summary>
		/// Sets the position within the current stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
		/// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
		/// <returns>The new position within the current stream.</returns>
		/// <exception cref="NotSupportedException">The stream does not support seeking.</exception>
		/// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		long Seek(long offset, SeekOrigin origin);

		/// <summary>
		/// Sets the length of the stream.
		/// </summary>
		/// <param name="length">The desired length of the current stream in bytes.</param>
		/// <exception cref="ArgumentOutOfRangeException">The specified length is negative or too large for the stream.</exception>
		/// <exception cref="NotSupportedException">
		/// The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output.
		/// </exception>
		/// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		void SetLength(long length);

		/// <summary>
		/// Writes a sequence of bytes to the current stream and advances the current position within this stream by the
		/// number of bytes written.
		/// </summary>
		/// <param name="buffer">A region of memory. This method copies the contents of this region to the current stream.</param>
		/// <exception cref="NotSupportedException">The stream does not support writing.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		void Write(ReadOnlySpan<byte> buffer);

		/// <summary>
		/// Writes a sequence of bytes to the stream and advances the position within this stream by the number of bytes written.
		/// </summary>
		/// <param name="buffer">The buffer to write to the stream.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
		/// <exception cref="NotSupportedException">The stream does not support writing.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		void Write(byte[] buffer, int offset, int count);

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
		/// <exception cref="NotSupportedException">The stream does not support writing.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);

		/// <summary>
		/// Asynchronously writes a sequence of bytes to the current stream and advances the current position within this stream
		/// by the number of bytes written.
		/// </summary>
		/// <param name="buffer">The buffer to write data from.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> from which to begin copying bytes to the stream.</param>
		/// <param name="count">The maximum number of bytes to write.</param>
		/// <returns>A task that represents the asynchronous write operation.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
		/// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.</exception>
		/// <exception cref="NotSupportedException">The stream does not support writing.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="InvalidOperationException">The stream is currently in use by a previous write operation.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		Task WriteAsync(
			byte[] buffer,
			int    offset,
			int    count);

		/// <summary>
		/// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream
		/// by the number of bytes written, and monitors cancellation requests.
		/// </summary>
		/// <param name="buffer">The buffer to write data from.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> from which to begin copying bytes to the stream.</param>
		/// <param name="count">The maximum number of bytes to write.</param>
		/// <param name="cancellationToken">
		/// The token to monitor for cancellation requests.
		/// The default value is <see cref="CancellationToken.None"/>.
		/// </param>
		/// <returns>A task that represents the asynchronous write operation.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
		/// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.</exception>
		/// <exception cref="NotSupportedException">The stream does not support writing.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="InvalidOperationException">The stream is currently in use by a previous write operation.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		Task WriteAsync(
			byte[]            buffer,
			int               offset,
			int               count,
			CancellationToken cancellationToken);

		/// <summary>
		/// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
		/// </summary>
		/// <param name="value">The byte to write to the stream.</param>
		/// <exception cref="NotSupportedException">The stream does not support writing.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		void WriteByte(byte value);

		/// <summary>
		/// Writes a sequence of bytes to the stream and advances the position within this stream by the number
		/// of bytes written.
		/// </summary>
		/// <param name="stream">Stream containing data to write.</param>
		/// <returns>Number of written bytes.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
		/// <exception cref="NotSupportedException">The current stream does not support writing or the source stream does not support reading.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		long Write(Stream stream);

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
		/// <exception cref="NotSupportedException">The current stream does not support writing or the source stream does not support reading.</exception>
		/// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		ValueTask<long> WriteAsync(Stream stream, CancellationToken cancellationToken);
	}

}
