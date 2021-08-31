///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace GriffinPlus.Lib.Io
{

	/// <summary>
	/// Unit tests targeting the <see cref="MemoryBlockStream"/> class.
	/// </summary>
	public abstract class MemoryBlockStreamTestsBase : IDisposable
	{
		/// <summary>
		/// The size of of test data sets tests juggle with.
		/// </summary>
		protected const int TestDataSize = 256 * 1024;

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryBlockStreamTestsBase"/> class.
		/// </summary>
		/// <param name="synchronized"><c>true</c> if the stream is synchronized; otherwise <c>false</c>.</param>
		/// <param name="usePool"><c>true</c> if the stream uses buffer pooling; otherwise <c>false</c>.</param>
		protected MemoryBlockStreamTestsBase(bool synchronized, bool usePool)
		{
			StreamIsSynchronized = synchronized;
			if (usePool) BufferPool = new ArrayPoolMock();
		}

		/// <summary>
		/// Test Teardown.
		/// </summary>
		public void Dispose()
		{
			// ensure that the stream has returned rented buffers to the pool
			EnsureBuffersHaveBeenReturned(0);
		}

		#region Test Specific Overrides

		/// <summary>
		/// Creates the <see cref="MemoryBlockStream"/> to test.
		/// </summary>
		/// <param name="minimumBlockSize">Minimum size of a memory block in the stream (in bytes).</param>
		/// <returns>The stream to test.</returns>
		protected abstract MemoryBlockStream CreateStreamToTest(int minimumBlockSize = -1);

		/// <summary>
		/// Gets a value indicating whether the stream can seek.
		/// </summary>
		protected abstract bool StreamCanSeek { get; }

		/// <summary>
		/// Gets the expected size of a memory block in the stream.
		/// </summary>
		protected abstract int StreamMemoryBlockSize { get; }

		/// <summary>
		/// Gets a value indicating whether the stream is synchronized.
		/// </summary>
		protected bool StreamIsSynchronized { get; }

		/// <summary>
		/// Gets the array pool used by the stream, if the stream uses pooled buffers.
		/// </summary>
		internal ArrayPoolMock BufferPool { get; }

		#endregion

		#region Stream Construction

		/// <summary>
		/// Creates a new stream and checks its properties.
		/// </summary>
		[Fact]
		public void CreateNewStream()
		{
			// create a new stream
			var stream = CreateStreamToTest();

			// check capabilities of the stream
			Assert.True(stream.CanRead);
			Assert.True(stream.CanWrite);
			Assert.Equal(StreamCanSeek, stream.CanSeek);
			Assert.Equal(StreamIsSynchronized, stream.IsSynchronized);

			// check position and length of the stream
			Assert.Equal(0, stream.Position);
			Assert.Equal(0, stream.Length);

			// the stream does not support timeouts
			Assert.Throws<InvalidOperationException>(() => stream.ReadTimeout);
			Assert.Throws<InvalidOperationException>(() => stream.WriteTimeout);

			// detach internal buffer and check that there is no buffer, yet
			var buffer = stream.DetachBuffer();
			Assert.Null(buffer);
		}

		#endregion

		#region void Flush()

		/// <summary>
		/// Tests flushing the stream using <see cref="MemoryBlockStream.Flush"/>
		/// (should not do anything with the stream as it's purely backed by memory).
		/// </summary>
		[Fact]
		public void Flush()
		{
			var stream = CreateStreamToTest();
			stream.Flush();
		}

		#endregion

		#region Task FlushAsync()

		/// <summary>
		/// Tests flushing the stream using <see cref="MemoryBlockStream.FlushAsync(CancellationToken)"/>
		/// (should not do anything with the stream as it's purely backed by memory).
		/// </summary>
		[Fact]
		public async Task FlushAsync()
		{
			var stream = CreateStreamToTest();
			await stream.FlushAsync(CancellationToken.None).ConfigureAwait(false);
		}

		#endregion

		#region int Read(byte[],int,int)

		/// <summary>
		/// Prepares a chain of memory blocks, attaches it to the stream and reads the stream
		/// using <see cref="MemoryBlockStream.Read(byte[],int,int)"/>.
		/// </summary>
		[Theory]
		[InlineData(false, 0)]            // empty stream
		[InlineData(false, 1)]            // stream with 1 byte in a single block
		[InlineData(false, TestDataSize)] // huge stream with multiple blocks, read all in single operation
		[InlineData(true, TestDataSize)]  // huge stream with multiple blocks, read in chunks
		public void Read_Buffer(bool chunkedRead, int initialLength)
		{
			int Operation(MemoryBlockStream stream, byte[] readBuffer, ref int bytesToRead)
			{
				return stream.Read(readBuffer, 0, bytesToRead);
			}

			TestRead(initialLength, chunkedRead, Operation);
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.Read(byte[],int,int)"/> blocks, if the stream is synchronized,
		/// and does not block, if the stream is not synchronized.
		/// </summary>
		[Fact]
		public void Read_Buffer_WaitOnLockIfSynchronized()
		{
			void Operation(MemoryBlockStream stream)
			{
				byte[] buffer = new byte[1];
				stream.Read(buffer, 0, buffer.Length);
			}

			TestWaitOnLockIfSynchronized(Operation);
		}

		#endregion

		#region int Read(Span<byte>)

		/// <summary>
		/// Prepares a chain of memory blocks, attaches it to the stream and reads the stream
		/// using <see cref="MemoryBlockStream.Read(Span{byte})"/>.
		/// </summary>
		[Theory]
		[InlineData(false, 0)]            // empty stream
		[InlineData(false, 1)]            // stream with 1 byte in a single block
		[InlineData(false, TestDataSize)] // huge stream with multiple blocks, read all in single operation
		[InlineData(true, TestDataSize)]  // huge stream with multiple blocks, read in chunks
		public void Read_Span(bool chunkedRead, int initialLength)
		{
			int Operation(MemoryBlockStream stream, byte[] readBuffer, ref int bytesToRead)
			{
				return stream.Read(readBuffer.AsSpan(0, bytesToRead));
			}

			TestRead(initialLength, chunkedRead, Operation);
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.Read(Span{byte})"/> blocks, if the stream is synchronized,
		/// and does not block, if the stream is not synchronized.
		/// </summary>
		[Fact]
		public void Read_Span_WaitOnLockIfSynchronized()
		{
			void Operation(MemoryBlockStream stream)
			{
				byte[] readBuffer = new byte[1];
				stream.Read(readBuffer.AsSpan());
			}

			TestWaitOnLockIfSynchronized(Operation);
		}

		#endregion

		#region int ReadAsync(byte[],int,int,CancellationToken)

		/// <summary>
		/// Prepares a chain of memory blocks, attaches it to the stream and reads the stream
		/// using <see cref="MemoryBlockStream.ReadAsync(byte[],int,int,CancellationToken)"/>.
		/// </summary>
		[Theory]
		[InlineData(false, 0)]            // empty stream
		[InlineData(false, 1)]            // stream with 1 byte in a single block
		[InlineData(false, TestDataSize)] // huge stream with multiple blocks, read all in single operation
		[InlineData(true, TestDataSize)]  // huge stream with multiple blocks, read in chunks
		public async Task ReadAsync_Buffer(bool chunkedRead, int initialLength)
		{
			async Task<int> Operation(MemoryBlockStream stream, byte[] readBuffer, int bytesToRead)
			{
				int readByteCount = await stream.ReadAsync(
						                    readBuffer,
						                    0,
						                    bytesToRead,
						                    CancellationToken.None)
					                    .ConfigureAwait(false);
				return readByteCount;
			}

			await TestReadAsync(initialLength, chunkedRead, Operation).ConfigureAwait(false);
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.ReadAsync(byte[],int,int,CancellationToken)"/> blocks,
		/// if the stream is synchronized, and does not block, if the stream is not synchronized. Furthermore tests
		/// cancellation, if the operation is blocked at the lock as well as cancellation with a pre-signaled token.
		/// </summary>
		[Fact]
		public async Task ReadAsync_Buffer_WaitOnLockIfSynchronizedAndCancellation()
		{
			async Task Operation(MemoryBlockStream stream, CancellationToken cancellationToken)
			{
				byte[] readBuffer = new byte[1];
				await stream.ReadAsync(
						readBuffer,
						0,
						readBuffer.Length,
						cancellationToken)
					.ConfigureAwait(false);
			}

			await TestWaitOnLockIfSynchronizedAndCancellation(Operation).ConfigureAwait(false);
		}

		#endregion

		#region int ReadAsync(Memory{byte},CancellationToken)

		/// <summary>
		/// Prepares a chain of memory blocks, attaches it to the stream and reads the stream
		/// using <see cref="MemoryBlockStream.ReadAsync(Memory{byte},CancellationToken)"/>.
		/// </summary>
		[Theory]
		[InlineData(false, 0)]            // empty stream
		[InlineData(false, 1)]            // stream with 1 byte in a single block
		[InlineData(false, TestDataSize)] // huge stream with multiple blocks, read all in single operation
		[InlineData(true, TestDataSize)]  // huge stream with multiple blocks, read in chunks
		public async Task ReadAsync_Memory(bool chunkedRead, int initialLength)
		{
			async Task<int> Operation(MemoryBlockStream stream, byte[] readBuffer, int bytesToRead)
			{
				int readByteCount = await stream.ReadAsync(
						                    readBuffer.AsMemory(0, bytesToRead),
						                    CancellationToken.None)
					                    .ConfigureAwait(false);
				return readByteCount;
			}

			await TestReadAsync(initialLength, chunkedRead, Operation).ConfigureAwait(false);
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.ReadAsync(byte[],int,int,CancellationToken)"/> blocks,
		/// if the stream is synchronized, and does not block, if the stream is not synchronized. Furthermore tests
		/// cancellation, if the operation is blocked at the lock as well as cancellation with a pre-signaled token.
		/// </summary>
		[Fact]
		public async Task ReadAsync_Memory_WaitOnLockIfSynchronizedAndCancellation()
		{
			async Task Operation(MemoryBlockStream stream, CancellationToken cancellationToken)
			{
				byte[] readBuffer = new byte[1];
				await stream
					.ReadAsync(readBuffer.AsMemory(), cancellationToken)
					.ConfigureAwait(false);
			}

			await TestWaitOnLockIfSynchronizedAndCancellation(Operation).ConfigureAwait(false);
		}

		#endregion

		#region int ReadByte()

		/// <summary>
		/// Prepares a chain of blocks, attaches it to the stream and reads the stream
		/// using <see cref="MemoryBlockStream.ReadByte"/>.
		/// </summary>
		[Theory]
		[InlineData(0)]            // empty stream
		[InlineData(1)]            // stream with 1 byte in a single block
		[InlineData(TestDataSize)] // huge stream with multiple blocks
		public void ReadByte(int initialLength)
		{
			int Operation(MemoryBlockStream stream, byte[] readBuffer, ref int bytesToRead)
			{
				Debug.Assert(bytesToRead >= 1);
				bytesToRead = 1; // overrides the number of bytes to read, so the test does not fail...
				int readByte = stream.ReadByte();
				if (readByte < 0) return 0;
				readBuffer[0] = (byte)readByte;
				return 1;
			}

			// test reading byte-wise
			// (use chunked reading as the operation overrides the number of bytes to read)
			TestRead(initialLength, true, Operation);
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.ReadByte"/> blocks, if the stream is synchronized,
		/// and does not block, if the stream is not synchronized.
		/// </summary>
		[Fact]
		public void ReadByte_WaitOnLockIfSynchronized()
		{
			void Operation(MemoryBlockStream stream)
			{
				stream.ReadByte();
			}

			TestWaitOnLockIfSynchronized(Operation);
		}

		#endregion

		#region void Write(byte[],int,int)

		/// <summary>
		/// Writes a random set of bytes into the stream using <see cref="MemoryBlockStream.Write(byte[],int,int)"/>.
		/// The write is one in a single operation.
		/// </summary>
		[Theory]
		[InlineData(0)]            // write empty buffer
		[InlineData(1)]            // write buffer with 1 byte
		[InlineData(TestDataSize)] // write huge buffer that results in multiple blocks in the stream
		public void Write_Buffer_SingleOperation(int count)
		{
			void Operation(MemoryBlockStream stream, byte[] data)
			{
				stream.Write(data, 0, data.Length);
			}

			TestWrite(count, Operation);
		}

		/// <summary>
		/// Writes a random set of bytes into the stream using <see cref="MemoryBlockStream.Write(byte[],int,int)"/>.
		/// The write is done with multiple smaller write operations.
		/// </summary>
		[Theory]
		[InlineData(TestDataSize, 8 * 1024)] // chunk size is power of 2 => fills up full blocks
		[InlineData(TestDataSize, 999)]      // chunk size is odd => write spans blocks
		public void Write_Buffer_MultipleOperations(int count, int chunkSize)
		{
			void Operation(MemoryBlockStream stream, byte[] data)
			{
				int offset = 0;
				do
				{
					int bytesToWrite = Math.Min(data.Length - offset, chunkSize);
					stream.Write(data, offset, bytesToWrite);
					offset += bytesToWrite;
				} while (offset < data.Length);
			}

			TestWrite(count, Operation);
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.Write(byte[],int,int)"/> blocks, if the stream is synchronized,
		/// and does not block, if the stream is not synchronized.
		/// </summary>
		[Fact]
		public void Write_Buffer_WaitOnLockIfSynchronized()
		{
			void Operation(MemoryBlockStream stream)
			{
				byte[] data = new byte[1];
				stream.Write(data, 0, data.Length);
			}

			TestWaitOnLockIfSynchronized(Operation);
		}

		#endregion

		#region void Write(ReadOnlySpan<byte>)

		/// <summary>
		/// Writes a random set of bytes into the stream using <see cref="MemoryBlockStream.Write(ReadOnlySpan{byte})"/>.
		/// The write is one in a single operation.
		/// </summary>
		[Theory]
		[InlineData(0)]            // write empty buffer
		[InlineData(1)]            // write buffer with 1 byte
		[InlineData(TestDataSize)] // write huge buffer that results in multiple blocks in the stream
		public void Write_ReadOnlySpan_SingleOperation(int count)
		{
			void Operation(MemoryBlockStream stream, byte[] data)
			{
				stream.Write(data.AsSpan(0, data.Length));
			}

			TestWrite(count, Operation);
		}

		/// <summary>
		/// Writes a random set of bytes into the stream using <see cref="MemoryBlockStream.Write(ReadOnlySpan{byte})"/>.
		/// The write is done with multiple smaller write operations.
		/// </summary>
		[Theory]
		[InlineData(TestDataSize, 8 * 1024)] // chunk size is power of 2 => fills up full blocks
		[InlineData(TestDataSize, 999)]      // chunk size is odd => write spans blocks
		public void Write_ReadOnlySpan_MultipleOperations(int count, int chunkSize)
		{
			void Operation(MemoryBlockStream stream, byte[] data)
			{
				// write the buffer in chunks
				int offset = 0;
				do
				{
					int bytesToWrite = Math.Min(data.Length - offset, chunkSize);
					stream.Write(data.AsSpan(offset, bytesToWrite));
					offset += bytesToWrite;
				} while (offset < data.Length);
			}

			TestWrite(count, Operation);
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.Write(ReadOnlySpan{byte})"/> blocks, if the stream is synchronized,
		/// and does not block, if the stream is not synchronized.
		/// </summary>
		[Fact]
		public void Write_ReadOnlySpan_WaitOnLockIfSynchronized()
		{
			void Operation(MemoryBlockStream stream)
			{
				byte[] data = new byte[1];
				stream.Write(data.AsSpan(0, data.Length));
			}

			TestWaitOnLockIfSynchronized(Operation);
		}

		#endregion

		#region long Write(Stream)

		/// <summary>
		/// Writes a random set of bytes into the stream using <see cref="MemoryBlockStream.Write(Stream)"/>.
		/// </summary>
		[Theory]
		[InlineData(0)]            // write empty buffer
		[InlineData(1)]            // write buffer with 1 byte
		[InlineData(TestDataSize)] // write huge buffer that results in multiple blocks in the stream
		public void Write_Stream(int count)
		{
			void Operation(MemoryBlockStream stream, byte[] data)
			{
				long bytesWritten = stream.Write(new MemoryStream(data));
				Assert.Equal(data.Length, bytesWritten);
			}

			TestWrite(count, Operation);
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.Write(Stream)"/> blocks, if the stream is synchronized,
		/// and does not block, if the stream is not synchronized.
		/// </summary>
		[Fact]
		public void Write_Stream_WaitOnLockIfSynchronized()
		{
			void Operation(MemoryBlockStream stream)
			{
				byte[] data = new byte[1];
				long bytesWritten = stream.Write(new MemoryStream(data));
				Assert.Equal(data.Length, bytesWritten);
			}

			TestWaitOnLockIfSynchronized(Operation);
		}

		#endregion

		#region WriteByte()

		/// <summary>
		/// Writes a random set of bytes into the stream using <see cref="MemoryBlockStream.WriteByte"/>.
		/// </summary>
		[Theory]
		[InlineData(0)]            // write empty buffer
		[InlineData(1)]            // write buffer with 1 byte
		[InlineData(TestDataSize)] // write huge buffer that results in multiple blocks in the stream
		public void WriteByte(int count)
		{
			void Operation(MemoryBlockStream stream, byte[] data)
			{
				foreach (byte x in data) stream.WriteByte(x);
			}

			TestWrite(count, Operation);
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.WriteByte"/> blocks, if the stream is synchronized,
		/// and does not block, if the stream is not synchronized.
		/// </summary>
		[Fact]
		public void WriteByte_WaitOnLockIfSynchronized()
		{
			void Operation(MemoryBlockStream stream)
			{
				stream.WriteByte(0);
			}

			TestWaitOnLockIfSynchronized(Operation);
		}

		#endregion

		#region Task WriteAsync(byte[],int,int,CancellationToken)

		/// <summary>
		/// Writes a random set of bytes into the stream using <see cref="MemoryBlockStream.WriteAsync(byte[],int,int,CancellationToken)"/>.
		/// The write is one in a single operation.
		/// </summary>
		[Theory]
		[InlineData(0)]            // write empty buffer
		[InlineData(1)]            // write buffer with 1 byte
		[InlineData(TestDataSize)] // write huge buffer that results in multiple blocks in the stream
		public async Task WriteAsync_Buffer_SingleOperation(int count)
		{
			async Task Operation(MemoryBlockStream stream, byte[] data)
			{
				await stream.WriteAsync(data, 0, data.Length, CancellationToken.None).ConfigureAwait(false);
			}

			await TestWriteAsync(count, Operation).ConfigureAwait(false);
		}

		/// <summary>
		/// Writes a random set of bytes into the stream using <see cref="MemoryBlockStream.WriteAsync(byte[],int,int,CancellationToken)"/>.
		/// The write is done with multiple smaller write operations.
		/// </summary>
		[Theory]
		[InlineData(TestDataSize, 8 * 1024)] // chunk size is power of 2 => fills up full blocks
		[InlineData(TestDataSize, 999)]      // chunk size is odd => write spans blocks
		public async Task WriteAsync_Buffer_MultipleOperations(int count, int chunkSize)
		{
			async Task Operation(MemoryBlockStream stream, byte[] data)
			{
				int offset = 0;
				do
				{
					int bytesToWrite = Math.Min(data.Length - offset, chunkSize);

					await stream.WriteAsync(
							data,
							offset,
							bytesToWrite,
							CancellationToken.None)
						.ConfigureAwait(false);

					offset += bytesToWrite;
				} while (offset < data.Length);
			}

			await TestWriteAsync(count, Operation).ConfigureAwait(false);
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.WriteAsync(byte[],int,int,CancellationToken)"/> blocks,
		/// if the stream is synchronized, and does not block, if the stream is not synchronized. Furthermore tests
		/// cancellation, if the operation is blocked at the lock as well as cancellation with a pre-signaled token.
		/// </summary>
		[Fact]
		public async Task WriteAsync_Buffer_WaitOnLockIfSynchronizedAndCancellation()
		{
			async Task Operation(MemoryBlockStream stream, CancellationToken cancellationToken)
			{
				byte[] data = new byte[1];
				await stream
					.WriteAsync(data, 0, data.Length, cancellationToken)
					.ConfigureAwait(false);
			}

			await TestWaitOnLockIfSynchronizedAndCancellation(Operation).ConfigureAwait(false);
		}

		#endregion

		#region Task WriteAsync(ReadOnlyMemory<byte>,CancellationToken)

		/// <summary>
		/// Writes a random set of bytes into the stream using <see cref="MemoryBlockStream.WriteAsync(ReadOnlyMemory{byte},CancellationToken)"/>.
		/// The write is one in a single operation.
		/// </summary>
		[Theory]
		[InlineData(0)]            // write empty buffer
		[InlineData(1)]            // write buffer with 1 byte
		[InlineData(TestDataSize)] // write huge buffer that results in multiple blocks in the stream
		public async Task WriteAsync_ReadOnlyMemory_SingleOperation(int count)
		{
			async Task Operation(MemoryBlockStream stream, byte[] data)
			{
				await stream.WriteAsync(
						data.AsMemory(0, data.Length),
						CancellationToken.None)
					.ConfigureAwait(false);
			}

			await TestWriteAsync(count, Operation).ConfigureAwait(false);
		}

		/// <summary>
		/// Writes a random set of bytes into the stream using <see cref="MemoryBlockStream.WriteAsync(ReadOnlyMemory{byte},CancellationToken)"/>.
		/// The write is done with multiple smaller write operations.
		/// </summary>
		[Theory]
		[InlineData(TestDataSize, 8 * 1024)] // chunk size is power of 2 => fills up full blocks
		[InlineData(TestDataSize, 999)]      // chunk size is odd => write spans blocks
		public async Task WriteAsync_ReadOnlyMemory_MultipleOperations(int count, int chunkSize)
		{
			async Task Operation(MemoryBlockStream stream, byte[] data)
			{
				int offset = 0;
				do
				{
					int bytesToWrite = Math.Min(data.Length - offset, chunkSize);

					// write the buffer
					await stream.WriteAsync(
							data.AsMemory(offset, bytesToWrite),
							CancellationToken.None)
						.ConfigureAwait(false);

					offset += bytesToWrite;
				} while (offset < data.Length);
			}

			await TestWriteAsync(count, Operation).ConfigureAwait(false);
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.WriteAsync(ReadOnlyMemory{byte},CancellationToken)"/> blocks,
		/// if the stream is synchronized, and does not block, if the stream is not synchronized. Furthermore tests
		/// cancellation, if the operation is blocked at the lock as well as cancellation with a pre-signaled token.
		/// </summary>
		[Fact]
		public async Task WriteAsync_ReadOnlyMemory_WaitOnLockIfSynchronizedAndCancellation()
		{
			async Task Operation(MemoryBlockStream stream, CancellationToken cancellationToken)
			{
				byte[] data = new byte[1];
				await stream.WriteAsync(
						data.AsMemory(0, data.Length),
						cancellationToken)
					.ConfigureAwait(false);
			}

			await TestWaitOnLockIfSynchronizedAndCancellation(Operation).ConfigureAwait(false);
		}

		#endregion

		#region Task WriteAsync(Stream,CancellationToken)

		/// <summary>
		/// Writes a random set of bytes into the stream using <see cref="MemoryBlockStream.WriteAsync(Stream,CancellationToken)"/>.
		/// </summary>
		[Theory]
		[InlineData(0)]            // write empty buffer
		[InlineData(1)]            // write buffer with 1 byte
		[InlineData(TestDataSize)] // write huge buffer that results in multiple blocks in the stream
		public async Task WriteAsync_Stream(int count)
		{
			async Task Operation(MemoryBlockStream stream, byte[] data)
			{
				await stream.WriteAsync(
						new MemoryStream(data),
						CancellationToken.None)
					.ConfigureAwait(false);
			}

			await TestWriteAsync(count, Operation).ConfigureAwait(false);
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.WriteAsync(Stream,CancellationToken)"/> blocks,
		/// if the stream is synchronized, and does not block, if the stream is not synchronized. Furthermore tests
		/// cancellation, if the operation is blocked at the lock as well as cancellation with a pre-signaled token.
		/// </summary>
		[Fact]
		public async Task WriteAsync_Stream_WaitOnLockIfSynchronizedAndCancellation()
		{
			async Task Operation(MemoryBlockStream stream, CancellationToken cancellationToken)
			{
				byte[] data = new byte[1];
				await stream.WriteAsync(
						new MemoryStream(data),
						cancellationToken)
					.ConfigureAwait(false);
			}

			await TestWaitOnLockIfSynchronizedAndCancellation(Operation).ConfigureAwait(false);
		}

		#endregion

		#region void CopyTo()

		/// <summary>
		/// Copies a random set of bytes into the stream and copies the stream to another stream
		/// using <see cref="MemoryBlockStream.CopyTo(System.IO.Stream,int)"/>.
		/// </summary>
		[Theory]
		[InlineData(0)]            // write empty buffer
		[InlineData(1)]            // write buffer with 1 byte
		[InlineData(TestDataSize)] // write huge buffer that results in multiple blocks in the stream
		public void CopyTo(int count)
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data
				using (var chain = GetRandomTestDataChain(count, StreamMemoryBlockSize, out var list))
				{
					byte[] data = list.ToArray();

					// attach chain of blocks to the stream
					stream.AttachBuffer(chain);

					// copy the stream to another stream
					var otherStream = new MemoryStream();
					stream.CopyTo(otherStream, 8 * 1024);

					// the read stream should be at its end now
					Assert.Equal(data.Length, stream.Position);
					Assert.Equal(data.Length, stream.Length);

					// the other stream should contain the written data now
					Assert.Equal(data.Length, otherStream.Position);
					Assert.Equal(data.Length, otherStream.Length);
					otherStream.Position = 0;
					Assert.Equal(data, otherStream.ToArray());
				}
			}
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.CopyTo(System.IO.Stream,int)"/> blocks, if the stream is synchronized,
		/// and does not block, if the stream is not synchronized.
		/// </summary>
		[Fact]
		public void CopyTo_WaitOnLockIfSynchronized()
		{
			void Operation(MemoryBlockStream stream)
			{
				var otherStream = new MemoryStream();
				stream.CopyTo(otherStream, 8 * 1024);
			}

			TestWaitOnLockIfSynchronized(Operation);
		}

		#endregion

		#region Task CopyToAsync()

		/// <summary>
		/// Copies a random set of bytes into the stream and copies the stream to another stream
		/// using <see cref="MemoryBlockStream.CopyToAsync(Stream,int,CancellationToken)"/>.
		/// </summary>
		[Theory]
		[InlineData(0)]            // write empty buffer
		[InlineData(1)]            // write buffer with 1 byte
		[InlineData(TestDataSize)] // write huge buffer that results in multiple blocks in the stream
		public async Task CopyToAsync(int count)
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data
				using (var chain = GetRandomTestDataChain(count, StreamMemoryBlockSize, out var list))
				{
					byte[] data = list.ToArray();

					// attach chain of blocks to the stream
					await stream.AttachBufferAsync(chain).ConfigureAwait(false);

					// copy the stream to another stream
					var otherStream = new MemoryStream();
					await stream.CopyToAsync(
							otherStream,
							8 * 1024,
							CancellationToken.None)
						.ConfigureAwait(false);

					// the read stream should be at its end now
					Assert.Equal(data.Length, stream.Position);
					Assert.Equal(data.Length, stream.Length);

					// the other stream should contain the written data now
					Assert.Equal(data.Length, otherStream.Position);
					Assert.Equal(data.Length, otherStream.Length);
					otherStream.Position = 0;
					Assert.Equal(data, otherStream.ToArray());
				}
			}
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.CopyToAsync(Stream,int,CancellationToken)"/> blocks,
		/// if the stream is synchronized, and does not block, if the stream is not synchronized. Furthermore tests
		/// cancellation, if the operation is blocked at the lock as well as cancellation with a pre-signaled token.
		/// </summary>
		[Fact]
		public async Task CopyToAsync_WaitOnLockIfSynchronizedAndCancellation()
		{
			async Task Operation(MemoryBlockStream stream, CancellationToken cancellationToken)
			{
				var otherStream = new MemoryStream();
				await stream.CopyToAsync(
						otherStream,
						8 * 1024,
						cancellationToken)
					.ConfigureAwait(false);
			}

			await TestWaitOnLockIfSynchronizedAndCancellation(Operation).ConfigureAwait(false);
		}

		#endregion

		#region void AttachBuffer()

		/// <summary>
		/// Attaches a prepared chain of memory blocks to the stream using <see cref="MemoryBlockStream.AttachBuffer"/>.
		/// </summary>
		[Fact]
		public void AttachBuffer()
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data and attach the buffer to the stream
				var chain = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out var data);
				stream.AttachBuffer(chain);

				// the stream's properties should reflect the new buffer
				Assert.Equal(0, stream.Position);
				Assert.Equal(data.Count, stream.Length);
			}
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.AttachBuffer"/> blocks, if the stream is synchronized,
		/// and does not block, if the stream is not synchronized.
		/// </summary>
		[Fact]
		public void AttachBuffer_WaitOnLockIfSynchronized()
		{
			void Operation(MemoryBlockStream stream)
			{
				var chain = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out _);
				stream.AttachBuffer(chain);
			}

			TestWaitOnLockIfSynchronized(Operation);
		}

		#endregion

		#region Task AttachBufferAsync()

		/// <summary>
		/// Attaches a prepared chain of memory blocks to the stream using <see cref="MemoryBlockStream.AttachBufferAsync"/>.
		/// </summary>
		[Fact]
		public async Task AttachBufferAsync()
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data and attach the buffer to the stream
				var chain = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out var data);
				await stream.AttachBufferAsync(chain, CancellationToken.None).ConfigureAwait(false);

				// the stream's properties should reflect the new buffer
				Assert.Equal(0, stream.Position);
				Assert.Equal(data.Count, stream.Length);
			}
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.AttachBufferAsync"/> blocks,
		/// if the stream is synchronized, and does not block, if the stream is not synchronized. Furthermore tests
		/// cancellation, if the operation is blocked at the lock as well as cancellation with a pre-signaled token.
		/// </summary>
		[Fact]
		public async Task AttachBufferAsync_WaitOnLockIfSynchronizedAndCancellation()
		{
			async Task Operation(MemoryBlockStream stream, CancellationToken cancellationToken)
			{
				var chain = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out _);
				try
				{
					await stream.AttachBufferAsync(chain, cancellationToken).ConfigureAwait(false);
				}
				catch
				{
					Assert.Null(chain.Previous); // the chain should not have been linked to the chain of blocks backing the stream
					chain.ReleaseChain();
					throw;
				}
			}

			await TestWaitOnLockIfSynchronizedAndCancellation(Operation).ConfigureAwait(false);
		}

		#endregion

		#region ChainableMemoryBlock DetachBuffer()

		/// <summary>
		/// Detaches the chain of memory blocks from the stream using <see cref="MemoryBlockStream.DetachBuffer"/>.
		/// </summary>
		[Fact]
		public void DetachBuffer()
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data and pass ownership to the stream
				var chain = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out var data);
				stream.AttachBuffer(chain);

				// the stream's properties should reflect the new buffer
				Assert.Equal(0, stream.Position);
				Assert.Equal(data.Count, stream.Length);

				// detach the buffer, should be the same as attached
				using (var firstBlock = stream.DetachBuffer())
				{
					Assert.Same(chain, firstBlock);

					// the stream should be empty now
					Assert.Equal(0, stream.Position);
					Assert.Equal(0, stream.Length);

					// check whether the detached buffer contains the same data as the attached buffer
					// (ensures that the stream did not modify it during the procedure)
					Assert.Equal(data, firstBlock.GetChainData());
				}
			}
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.DetachBuffer"/> blocks, if the stream is synchronized,
		/// and does not block, if the stream is not synchronized.
		/// </summary>
		[Fact]
		public void DetachBuffer_WaitOnLockIfSynchronized()
		{
			void Operation(MemoryBlockStream stream)
			{
				// stream is empty, but that's irrelevant for the locking behavior
				using (stream.DetachBuffer())
				{
				}
			}

			TestWaitOnLockIfSynchronized(Operation);
		}

		#endregion

		#region ChainableMemoryBlock DetachBufferAsync(CancellationToken)

		/// <summary>
		/// Detaches the chain of memory blocks from the stream using <see cref="MemoryBlockStream.DetachBufferAsync"/>.
		/// </summary>
		[Fact]
		public async Task DetachBufferAsync()
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data and pass ownership to the stream
				var chain = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out var data);
				await stream.AttachBufferAsync(chain).ConfigureAwait(false);

				// the stream's properties should reflect the new buffer
				Assert.Equal(0, stream.Position);
				Assert.Equal(data.Count, stream.Length);

				// detach the buffer, should be the same as attached
				using (var firstBlock = await stream.DetachBufferAsync(CancellationToken.None).ConfigureAwait(false))
				{
					Assert.Same(chain, firstBlock);

					// the stream should be empty now
					Assert.Equal(0, stream.Position);
					Assert.Equal(0, stream.Length);

					// check whether the detached buffer contains the same data as the attached buffer
					// (ensures that the stream did not modify it during the procedure)
					Assert.Equal(data, firstBlock.GetChainData());
				}
			}
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.DetachBufferAsync"/> blocks,
		/// if the stream is synchronized, and does not block, if the stream is not synchronized. Furthermore tests
		/// cancellation, if the operation is blocked at the lock as well as cancellation with a pre-signaled token.
		/// </summary>
		[Fact]
		public async Task DetachBufferAsync_WaitOnLockIfSynchronized()
		{
			async Task Operation(MemoryBlockStream stream, CancellationToken cancellationToken)
			{
				// stream is empty, but that's irrelevant for the locking behavior
				using (await stream.DetachBufferAsync(cancellationToken).ConfigureAwait(false))
				{
				}
			}

			await TestWaitOnLockIfSynchronizedAndCancellation(Operation).ConfigureAwait(false);
		}

		#endregion

		#region void AppendBuffer(ChainableMemoryBlock)

		/// <summary>
		/// Appends a memory block to the stream using <see cref="MemoryBlockStream.AppendBuffer"/>.
		/// The initial stream is empty.
		/// </summary>
		[Fact]
		public void AppendBuffer_EmptyStream()
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data
				var chain = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out var data);

				// append the second buffer
				stream.AppendBuffer(chain);

				// the stream should contain data from both buffers
				Assert.Equal(0, stream.Position);
				Assert.Equal(data.Count, stream.Length);
				using (var detachedBuffer = stream.DetachBuffer())
				{
					Assert.Equal(data, detachedBuffer.GetChainData());
				}
			}
		}

		/// <summary>
		/// Appends a memory block to the stream using <see cref="MemoryBlockStream.AppendBuffer"/>.
		/// The initial stream contains some data.
		/// </summary>
		[Fact]
		public void AppendBuffer_NonEmptyStream()
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data
				var chain1 = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out var data1);
				var chain2 = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out var data2);

				// attach the first buffer to the stream
				stream.AttachBuffer(chain1);

				// append the second buffer
				stream.AppendBuffer(chain2);

				// the stream should contain data from both buffers
				Assert.Equal(0, stream.Position);
				Assert.Equal(data1.Count + data2.Count, stream.Length);
				using (var detachedBuffer = stream.DetachBuffer())
				{
					Assert.Equal(data1.Concat(data2), detachedBuffer.GetChainData());
				}
			}
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.AppendBuffer"/> blocks, if the stream is synchronized,
		/// and does not block, if the stream is not synchronized.
		/// </summary>
		[Fact]
		public void AppendBuffer_WaitOnLockIfSynchronized()
		{
			void Operation(MemoryBlockStream stream)
			{
				// stream is empty, but that's irrelevant for the locking behavior
				var chain = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out _);
				stream.AppendBuffer(chain);
			}

			TestWaitOnLockIfSynchronized(Operation);
		}

		#endregion

		#region Task AppendBufferAsync(ChainableMemoryBlock,CancellationToken)

		/// <summary>
		/// Appends a memory block to the stream using <see cref="MemoryBlockStream.AppendBufferAsync"/>.
		/// The initial stream is empty.
		/// </summary>
		[Fact]
		public async Task AppendBufferAsync_EmptyStream()
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data
				var chain = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out var data);

				// append the second buffer
				await stream.AppendBufferAsync(chain, CancellationToken.None);

				// the stream should contain data from both buffers
				Assert.Equal(0, stream.Position);
				Assert.Equal(data.Count, stream.Length);
				using (var detachedBuffer = await stream.DetachBufferAsync().ConfigureAwait(false))
				{
					Assert.Equal(data, detachedBuffer.GetChainData());
				}
			}
		}

		/// <summary>
		/// Appends a memory block to the stream using <see cref="MemoryBlockStream.AppendBufferAsync"/>.
		/// The initial stream contains some data.
		/// </summary>
		[Fact]
		public async Task AppendBufferAsync_NonEmptyStream()
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data
				var chain1 = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out var data1);
				var chain2 = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out var data2);

				// attach the first buffer to the stream
				await stream.AttachBufferAsync(chain1, CancellationToken.None).ConfigureAwait(false);

				// append the second buffer
				await stream.AppendBufferAsync(chain2, CancellationToken.None).ConfigureAwait(false);

				// the stream should contain data from both buffers
				Assert.Equal(0, stream.Position);
				Assert.Equal(data1.Count + data2.Count, stream.Length);
				using (var detachedBuffer = await stream.DetachBufferAsync().ConfigureAwait(false))
				{
					Assert.Equal(data1.Concat(data2), detachedBuffer.GetChainData());
				}
			}
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.AppendBufferAsync"/> blocks,
		/// if the stream is synchronized, and does not block, if the stream is not synchronized. Furthermore tests
		/// cancellation, if the operation is blocked at the lock as well as cancellation with a pre-signaled token.
		/// </summary>
		[Fact]
		public async Task AppendBufferAsync_WaitOnLockIfSynchronized()
		{
			async Task Operation(MemoryBlockStream stream, CancellationToken cancellationToken)
			{
				var chain = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out _);
				try
				{
					await stream.AppendBufferAsync(chain, cancellationToken).ConfigureAwait(false);
				}
				catch
				{
					Assert.Null(chain.Previous); // the chain should not have been linked to the chain of blocks backing the stream
					chain.ReleaseChain();
					throw;
				}
			}

			await TestWaitOnLockIfSynchronizedAndCancellation(Operation).ConfigureAwait(false);
		}

		#endregion

		#region [[ Read Test Frames ]]

		private delegate int ReadOperation(MemoryBlockStream stream, byte[] readBuffer, ref int bytesToRead);

		/// <summary>
		/// Common test frame for synchronous read operations.
		/// </summary>
		/// <param name="initialLength">Initial length of the stream.</param>
		/// <param name="randomChunks">
		/// <c>true</c> to read in chunks of random size;
		/// <c>false</c> to read the entire stream at once.
		/// </param>
		/// <param name="operation">Operation that performs the read operation.</param>
		private void TestRead(int initialLength, bool randomChunks, ReadOperation operation)
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data and attach it to the stream
				var chain = GetRandomTestDataChain(initialLength, StreamMemoryBlockSize, out var expectedData);
				stream.AttachBuffer(chain);

				var readData = new List<byte>(expectedData.Count);
				if (randomChunks)
				{
					// read data in chunks of random size
					var random = new Random(0);
					byte[] readBuffer = new byte[8 * 1024];
					int remaining = expectedData.Count;
					while (true)
					{
						int bytesToRead = random.Next(1, readBuffer.Length);
						int bytesRead = operation(stream, readBuffer, ref bytesToRead);
						int expectedByteRead = Math.Min(bytesToRead, remaining);
						Assert.Equal(expectedByteRead, bytesRead);
						if (bytesRead == 0) break;
						readData.AddRange(readBuffer.Take(bytesRead));
						remaining -= bytesRead;
					}
				}
				else
				{
					// read entire stream at once
					byte[] readBuffer = new byte[expectedData.Count + 1];
					int bytesToRead = readBuffer.Length;
					int bytesRead = operation(stream, readBuffer, ref bytesToRead); // operation should not override bytesToRead
					readData.AddRange(readBuffer.Take(bytesRead));
				}

				// the stream has been read to the end
				// it should be empty now and read data should equal the expected test data
				Assert.Equal(expectedData.Count, stream.Position);
				Assert.Equal(expectedData.Count, stream.Length);
				Assert.Equal(expectedData, readData);

				// the stream should have returned its buffers to the pool, if release-after-read is enabled
				if (initialLength > 0)
				{
					if (stream.ReleaseReadBlocks) EnsureBuffersHaveBeenReturned(1);
					else EnsureBuffersHaveNotBeenReturned();
				}
				else
				{
					EnsureBuffersHaveBeenReturned(0);
				}
			}
		}

		/// <summary>
		/// Common test frame for asynchronous read operations.
		/// </summary>
		/// <param name="initialLength">Initial length of the stream.</param>
		/// <param name="randomChunks">
		/// <c>true</c> to read in chunks of random size;
		/// <c>false</c> to read the entire stream at once.
		/// </param>
		/// <param name="operation">Operation that performs the read operation.</param>
		private async Task TestReadAsync(int initialLength, bool randomChunks, Func<MemoryBlockStream, byte[], int, Task<int>> operation)
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data and attach it to the stream
				var chain = GetRandomTestDataChain(initialLength, StreamMemoryBlockSize, out var expectedData);
				await stream.AttachBufferAsync(chain).ConfigureAwait(false);

				var readData = new List<byte>(expectedData.Count);
				if (randomChunks)
				{
					// read data in chunks of random size
					var random = new Random(0);
					byte[] readBuffer = new byte[8 * 1024];
					while (true)
					{
						int bytesToRead = random.Next(1, readBuffer.Length);
						int bytesRead = await operation(stream, readBuffer, bytesToRead).ConfigureAwait(false);
						if (bytesRead == 0) break;
						readData.AddRange(readBuffer.Take(bytesRead));
					}
				}
				else
				{
					// read entire stream at once
					byte[] readBuffer = new byte[expectedData.Count + 1];
					int bytesToRead = readBuffer.Length;
					int bytesRead = await operation(stream, readBuffer, bytesToRead).ConfigureAwait(false);
					readData.AddRange(readBuffer.Take(bytesRead));
				}

				// the stream has been read to the end
				// it should be empty now and read data should equal the expected test data
				Assert.Equal(expectedData.Count, stream.Position);
				Assert.Equal(expectedData.Count, stream.Length);
				Assert.Equal(expectedData, readData);

				// the stream should have returned its buffers to the pool, if release-after-read is enabled
				if (initialLength > 0)
				{
					if (stream.ReleaseReadBlocks) EnsureBuffersHaveBeenReturned(1);
					else EnsureBuffersHaveNotBeenReturned();
				}
				else
				{
					EnsureBuffersHaveBeenReturned(0);
				}
			}
		}

		#endregion

		#region [[ Write Test Frames ]]

		/// <summary>
		/// Common test frame for synchronous write operations.
		/// </summary>
		/// <param name="count">Number of bytes to write to the stream.</param>
		/// <param name="operation">Operation that performs the write operation.</param>
		private void TestWrite(int count, Action<MemoryBlockStream, byte[]> operation)
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data
				using (GetRandomTestDataChain(count, StreamMemoryBlockSize, out var list))
				{
					byte[] data = list.ToArray();

					// write data to the stream
					operation(stream, data);

					// the stream should contain the written data now
					Assert.Equal(data.Length, stream.Position);
					Assert.Equal(data.Length, stream.Length);
					using (var detachedBuffer = stream.DetachBuffer())
					{
						if (count > 0) Assert.Equal(data, detachedBuffer.GetChainData());
						else Assert.Null(detachedBuffer);
					}
				}
			}
		}

		/// <summary>
		/// Common test frame for asynchronous write operations.
		/// </summary>
		/// <param name="count">Number of bytes to write to the stream.</param>
		/// <param name="operation">Operation that performs the write operation.</param>
		private async Task TestWriteAsync(int count, Func<MemoryBlockStream, byte[], Task> operation)
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data
				using (GetRandomTestDataChain(count, StreamMemoryBlockSize, out var list))
				{
					byte[] data = list.ToArray();

					// write the buffer
					await operation(stream, data).ConfigureAwait(false);

					// the stream should contain the written data now
					Assert.Equal(data.Length, stream.Position);
					Assert.Equal(data.Length, stream.Length);
					using (var detachedBuffer = await stream.DetachBufferAsync().ConfigureAwait(false))
					{
						if (count > 0) Assert.Equal(data, detachedBuffer.GetChainData());
						else Assert.Null(detachedBuffer);
					}
				}
			}
		}

		#endregion

		#region [[ Lock/Cancellation Test Frames ]]

		/// <summary>
		/// Common test for checking the locking behavior of synchronous methods.
		/// </summary>
		public void TestWaitOnLockIfSynchronized(Action<MemoryBlockStream> operation)
		{
			using (var stream = CreateStreamToTest())
			{
				Assert.Equal(StreamIsSynchronized, stream.IsSynchronized);

				// acquire the stream lock
				stream.EnterLock();

				// schedule releasing the lock
				var timeToWait = TimeSpan.FromSeconds(3);
				var delayUnlockTask = Task.Delay(timeToWait).ContinueWith((task, obj) => ((MemoryBlockStream)obj).ExitLock(), stream);

				// start stop watch
				var watch = new Stopwatch();
				watch.Start();

				// invoke stream operation (should block)
				operation(stream);

				// stop stopwatch and check whether the read method has really blocked
				watch.Stop();
				if (StreamIsSynchronized)
				{
					Assert.True(watch.Elapsed >= timeToWait - TimeSpan.FromMilliseconds(100)); // -100ms to handle time jitter
				}
				else
				{
					// stream is not synchronized => reading should complete instantly
					Assert.True(watch.Elapsed < TimeSpan.FromMilliseconds(1000)); // -1000ms to handle undeterministic delays on the CI server
				}

				// wait for the stream to get unlocked
				// (should already be done, if everything runs fine...)
				delayUnlockTask.Wait();
			}
		}

		/// <summary>
		/// Common test for checking the locking/cancellation behavior of asynchronous methods.
		/// </summary>
		public async Task TestWaitOnLockIfSynchronizedAndCancellation(Func<MemoryBlockStream, CancellationToken, Task> operation)
		{
			var timeToWait = TimeSpan.FromSeconds(3);

			using (var stream = CreateStreamToTest())
			{
				Assert.Equal(StreamIsSynchronized, stream.IsSynchronized);

				#region Test cancellation with pre-signaled cancellation token (synchronized/unsynchronized streams)

				{
					var ct = new CancellationToken(true);
					await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation(stream, ct)).ConfigureAwait(false);
				}

				#endregion

				#region Test cancellation with delay-signaled cancellation token (synchronized streams only)

				{
					if (StreamIsSynchronized)
					{
						stream.EnterLock();
						var delayUnlockTask = Task.Delay(timeToWait).ContinueWith((task, obj) => ((MemoryBlockStream)obj).ExitLock(), stream);
						var cts = new CancellationTokenSource(500);
						await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation(stream, cts.Token)).ConfigureAwait(false);
						await delayUnlockTask.ConfigureAwait(false);
					}
				}

				#endregion

				#region Test locking

				{
					// start stop watch
					var watch = new Stopwatch();
					watch.Start();

					// acquire the stream lock
					stream.EnterLock();

					// invoke stream operation (should block)
					var delayUnlockTask = Task.Delay(timeToWait).ContinueWith((task, obj) => ((MemoryBlockStream)obj).ExitLock(), stream);
					await operation(stream, CancellationToken.None).ConfigureAwait(false);

					// stop stopwatch and check whether the read method has really blocked
					watch.Stop();
					if (StreamIsSynchronized)
					{
						Assert.True(watch.Elapsed >= timeToWait - TimeSpan.FromMilliseconds(100)); // -100ms to handle time jitter
					}
					else
					{
						// stream is not synchronized => reading should complete instantly
						Assert.True(watch.Elapsed < TimeSpan.FromMilliseconds(1000)); // -1000ms to handle undeterministic delays on the CI server
					}

					// wait for the unlock task to complete
					await delayUnlockTask.ConfigureAwait(false);
				}

				#endregion
			}
		}

		#endregion

		#region [[ Generating Test Data ]]

		/// <summary>
		/// Gets some random test data packed into a chain of memory blocks that can be attached to a memory block stream.
		/// </summary>
		/// <param name="count">Number of random bytes to get.</param>
		/// <param name="blockSize">Minimum size of memory blocks the returned chain of memory blocks should have.</param>
		/// <param name="data">The generated test data.</param>
		/// <returns>First block in the chain of created memory blocks</returns>
		protected ChainableMemoryBlock GetRandomTestDataChain(int count, int blockSize, out List<byte> data)
		{
			if (count == 0)
			{
				data = new List<byte>();
				return null;
			}

			ChainableMemoryBlock firstBlock = null;
			ChainableMemoryBlock previousBlock = null;
			var random = new Random(0);
			data = new List<byte>(count);
			int remaining = count;
			while (remaining > 0)
			{
				// allocate or rent a buffer 
				// (buffer may be larger than requested, if rented from the pool)
				var block = new ChainableMemoryBlock(blockSize, BufferPool, false);
				random.NextBytes(block.Buffer);
				block.Length = Math.Min(remaining, block.Capacity);
				if (previousBlock != null) previousBlock.Next = block;
				data.AddRange(block.Buffer.Take(block.Length));
				remaining -= block.Length;
				if (firstBlock == null) firstBlock = block;
				previousBlock = block;
			}

			return firstBlock;
		}

		#endregion

		#region [[ Rented Buffer Checks ]]

		/// <summary>
		/// Checks whether all buffers have been returned to the array pool, if applicable.
		/// </summary>
		protected void EnsureBuffersHaveBeenReturned(int expectedBufferCount)
		{
			if (BufferPool != null)
			{
				Assert.Equal(expectedBufferCount, BufferPool.RentedBufferCount);
			}
		}

		/// <summary>
		/// Checks whether not all buffers have been returned to the array pool, if applicable.
		/// </summary>
		protected void EnsureBuffersHaveNotBeenReturned()
		{
			if (BufferPool != null)
			{
				Assert.True(BufferPool.RentedBufferCount > 0);
			}
		}

		#endregion
	}

}
