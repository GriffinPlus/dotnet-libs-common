///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;
using System.Collections.Generic;
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
		protected const int TestDataSize = 1 * 1024 * 1024;

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryBlockStreamTestsBase"/> class.
		/// </summary>
		/// <param name="usePool"></param>
		protected MemoryBlockStreamTestsBase(bool usePool)
		{
			if (usePool) BufferPool = new ArrayPoolMock();
		}

		/// <summary>
		/// Test Teardown.
		/// </summary>
		public void Dispose()
		{
			// ensure that the stream has returned rented buffers to the pool
			EnsureBuffersHaveBeenReturned();
		}

		#region Test Specific Overrides

		/// <summary>
		/// Creates the <see cref="MemoryBlockStream"/> to test.
		/// </summary>
		/// <returns></returns>
		protected abstract MemoryBlockStream CreateStreamToTest();

		/// <summary>
		/// Gets a value indicating whether the stream can seek.
		/// </summary>
		protected abstract bool StreamCanSeek { get; }

		/// <summary>
		/// Gets the expected size of a memory block in the stream.
		/// </summary>
		protected abstract int StreamMemoryBlockSize { get; }

		/// <summary>
		/// Gets the array pool used by the stream, if the stream uses pooled buffers.
		/// </summary>
		protected ArrayPoolMock BufferPool { get; }

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

		#region Flush()

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

		#region FlushAsync()

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

		#region Read()

		/// <summary>
		/// Prepares a chain of memory blocks, attaches it to the stream and reads the stream
		/// using <see cref="MemoryBlockStream.Read(byte[],int,int)"/>.
		/// </summary>
		[Theory]
		[InlineData(0)]            // empty stream
		[InlineData(1)]            // stream with 1 byte in a single block
		[InlineData(TestDataSize)] // huge stream with multiple blocks
		public void Read_Buffer(int initialLength)
		{
			int Operation(MemoryBlockStream stream, byte[] readBuffer, ref int bytesToRead)
			{
				return stream.Read(readBuffer, 0, bytesToRead);
			}

			Read_Common(initialLength, Operation);
		}

		/// <summary>
		/// Prepares a chain of memory blocks, attaches it to the stream and reads the stream
		/// using <see cref="MemoryBlockStream.Read(Span{byte})"/>.
		/// </summary>
		[Theory]
		[InlineData(0)]            // empty stream
		[InlineData(1)]            // stream with 1 byte in a single block
		[InlineData(TestDataSize)] // huge stream with multiple blocks
		public void Read_Span(int initialLength)
		{
			int Operation(MemoryBlockStream stream, byte[] readBuffer, ref int bytesToRead)
			{
				return stream.Read(readBuffer.AsSpan(0, bytesToRead));
			}

			Read_Common(initialLength, Operation);
		}

		private delegate int ReadOperation(MemoryBlockStream Stream, byte[] readBuffer, ref int bytesToRead);

		/// <summary>
		/// Common test frame for synchronous read operations.
		/// </summary>
		/// <param name="initialLength">Initial length of the stream.</param>
		/// <param name="operation">Operation that performs the read operation.</param>
		private void Read_Common(int initialLength, ReadOperation operation)
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data and attach it to the stream
				var chain = GetRandomTestDataChain(initialLength, StreamMemoryBlockSize, out var expectedData);
				stream.AttachBuffer(chain);

				// read data in chunks of random size
				var random = new Random(0);
				byte[] readBuffer = new byte[8 * 1024];
				var readData = new List<byte>(expectedData.Count);
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

				// the stream has been read to the end
				// it should be empty now and read data should equal the expected test data
				Assert.Equal(expectedData.Count, stream.Position);
				Assert.Equal(expectedData.Count, stream.Length);
				Assert.Equal(expectedData, readData);

				// the stream should have returned its buffers to the pool, if release-after-read is enabled
				if (stream.ReleaseReadBlocks) EnsureBuffersHaveBeenReturned();
				else if (initialLength > 0) EnsureBuffersHaveNotBeenReturned();
			}
		}

		#endregion

		#region ReadAsync()

		/// <summary>
		/// Prepares a chain of memory blocks, attaches it to the stream and reads the stream
		/// using <see cref="MemoryBlockStream.ReadAsync(byte[],int,int,CancellationToken)"/>.
		/// </summary>
		[Theory]
		[InlineData(0)]            // empty stream
		[InlineData(1)]            // stream with 1 byte in a single block
		[InlineData(TestDataSize)] // huge stream with multiple blocks
		public async Task ReadAsync_Buffer(int initialLength)
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

			await ReadAsync_Common(initialLength, Operation).ConfigureAwait(false);
		}

		/// <summary>
		/// Prepares a chain of memory blocks, attaches it to the stream and reads the stream
		/// using <see cref="MemoryBlockStream.ReadAsync(Memory{byte},CancellationToken)"/>.
		/// </summary>
		[Theory]
		[InlineData(0)]            // empty stream
		[InlineData(1)]            // stream with 1 byte in a single block
		[InlineData(TestDataSize)] // huge stream with multiple blocks
		public async Task ReadAsync_Memory(int initialLength)
		{
			async Task<int> Operation(MemoryBlockStream stream, byte[] readBuffer, int bytesToRead)
			{
				int readByteCount = await stream.ReadAsync(
						                    readBuffer.AsMemory(0, bytesToRead),
						                    CancellationToken.None)
					                    .ConfigureAwait(false);
				return readByteCount;
			}

			await ReadAsync_Common(initialLength, Operation).ConfigureAwait(false);
		}

		/// <summary>
		/// Common test frame for asynchronous read operations.
		/// </summary>
		/// <param name="initialLength">Initial length of the stream.</param>
		/// <param name="operation">Operation that performs the read operation.</param>
		private async Task ReadAsync_Common(int initialLength, Func<MemoryBlockStream, byte[], int, Task<int>> operation)
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data and attach it to the stream
				var chain = GetRandomTestDataChain(initialLength, StreamMemoryBlockSize, out var expectedData);
				stream.AttachBuffer(chain);

				// read data in chunks of random size
				var random = new Random(0);
				byte[] readBuffer = new byte[8 * 1024];
				var readData = new List<byte>(expectedData.Count);
				while (true)
				{
					int bytesToRead = random.Next(1, readBuffer.Length);
					int bytesRead = await operation(stream, readBuffer, bytesToRead).ConfigureAwait(false);
					if (bytesRead == 0) break;
					readData.AddRange(readBuffer.Take(bytesRead));
				}

				// the stream has been read to the end
				// it should be empty now and read data should equal the expected test data
				Assert.Equal(expectedData.Count, stream.Position);
				Assert.Equal(expectedData.Count, stream.Length);
				Assert.Equal(expectedData, readData);

				// the stream should have returned its buffers to the pool, if release-after-read is enabled
				if (stream.ReleaseReadBlocks) EnsureBuffersHaveBeenReturned();
				else if (initialLength > 0) EnsureBuffersHaveNotBeenReturned();
			}
		}

		#endregion

		#region ReadByte()

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
				bytesToRead = 1; // overrides the number of bytes to read, so the test does not fail...
				int readByte = stream.ReadByte();
				if (readByte < 0) return 0;
				readBuffer[0] = (byte)readByte;
				return 1;
			}

			Read_Common(initialLength, Operation);
		}

		#endregion

		#region Write()

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

			Write_Common(count, Operation);
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

			Write_Common(count, Operation);
		}

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

			Write_Common(count, Operation);
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

			Write_Common(count, Operation);
		}

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
				stream.Write(new MemoryStream(data));
			}

			Write_Common(count, Operation);
		}

		/// <summary>
		/// Common test frame for synchronous write operations.
		/// </summary>
		/// <param name="count">Number of bytes to write to the stream.</param>
		/// <param name="operation">Operation that performs the write operation.</param>
		private void Write_Common(int count, Action<MemoryBlockStream, byte[]> operation)
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

		#endregion

		#region WriteAsync()

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

			await WriteAsync_Common(count, Operation).ConfigureAwait(false);
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

			await WriteAsync_Common(count, Operation).ConfigureAwait(false);
		}

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

			await WriteAsync_Common(count, Operation).ConfigureAwait(false);
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

			await WriteAsync_Common(count, Operation).ConfigureAwait(false);
		}

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

			await WriteAsync_Common(count, Operation).ConfigureAwait(false);
		}

		/// <summary>
		/// Common test frame for asynchronous write operations.
		/// </summary>
		/// <param name="count">Number of bytes to write to the stream.</param>
		/// <param name="operation">Operation that performs the write operation.</param>
		private async Task WriteAsync_Common(int count, Func<MemoryBlockStream, byte[], Task> operation)
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
					using (var detachedBuffer = stream.DetachBuffer())
					{
						if (count > 0) Assert.Equal(data, detachedBuffer.GetChainData());
						else Assert.Null(detachedBuffer);
					}
				}
			}
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

			Write_Common(count, Operation);
		}

		#endregion

		#region CopyTo()

		/// <summary>
		/// Copies a random set of bytes into the stream and copies the stream to another stream
		/// using <see cref="MemoryBlockStream.CopyTo(Stream)"/>.
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

		#endregion

		#region CopyToAsync()

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
					stream.AttachBuffer(chain);

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

		#endregion

		#region AttachBuffer()

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

		#endregion

		#region DetachBuffer()

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

		#endregion

		#region AppendBuffer()

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

		#endregion

		#region Generating Test Data

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
				random.NextBytes(block.InternalBuffer);
				block.Length = Math.Min(remaining, block.Capacity);
				if (previousBlock != null) previousBlock.InternalNext = block;
				data.AddRange(block.InternalBuffer.Take(block.Length));
				remaining -= block.Length;
				if (firstBlock == null) firstBlock = block;
				previousBlock = block;
			}

			return firstBlock;
		}

		#endregion

		#region Rented Buffer Checks

		/// <summary>
		/// Checks whether all buffers have been returned to the array pool, if applicable.
		/// </summary>
		protected void EnsureBuffersHaveBeenReturned()
		{
			if (BufferPool != null)
			{
				Assert.Equal(0, BufferPool.RentedBufferCount);
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

		#region Array Pool Mock

		protected class ArrayPoolMock : ArrayPool<byte>
		{
			private readonly ArrayPool<byte> mPool = Create();
			private          int             mRentedBufferCount;

			public int RentedBufferCount => Volatile.Read(ref mRentedBufferCount);

			public override byte[] Rent(int minimumLength)
			{
				Interlocked.Increment(ref mRentedBufferCount);
				return mPool.Rent(minimumLength);
			}

			public override void Return(byte[] array, bool clearArray = false)
			{
				Interlocked.Decrement(ref mRentedBufferCount);
				mPool.Return(array, clearArray);
			}
		}

		#endregion
	}

}
