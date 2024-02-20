///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Io;

/// <summary>
/// Interface of <see cref="MemoryBlockStream"/> and <see cref="SynchronizedMemoryBlockStream"/>.
/// </summary>
public interface IMemoryBlockStream : IStream
{
	/// <summary>
	/// Gets a value indicating whether the stream releases read buffers.
	/// </summary>
	bool ReleasesReadBlocks { get; }

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
	void AppendBuffer(ChainableMemoryBlock buffer);

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
	Task AppendBufferAsync(ChainableMemoryBlock buffer, CancellationToken cancellationToken = default);

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
	void AttachBuffer(ChainableMemoryBlock buffer);

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
	Task AttachBufferAsync(ChainableMemoryBlock buffer, CancellationToken cancellationToken = default);

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
	ChainableMemoryBlock DetachBuffer();

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
	Task<ChainableMemoryBlock> DetachBufferAsync(CancellationToken cancellationToken = default);
}
