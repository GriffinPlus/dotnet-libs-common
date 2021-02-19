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
					int bytesRead = stream.Read(readBuffer, 0, bytesToRead);
					if (bytesRead == 0) break;
					readData.AddRange(readBuffer.Take(bytesRead));
				}

				// the stream has been read to the end
				// it should be empty now and read data should equal the expected test data
				Assert.Equal(expectedData.Count, stream.Position);
				Assert.Equal(expectedData.Count, stream.Length);
				Assert.Equal(expectedData, readData);
			}
		}

		/// <summary>
		/// Prepares a chain of blocks, attaches it to the stream and reads the stream
		/// using <see cref="MemoryBlockStream.Read(IntPtr,int)"/>.
		/// </summary>
		[Theory]
		[InlineData(0)]            // empty stream
		[InlineData(1)]            // stream with 1 byte in a single block
		[InlineData(TestDataSize)] // huge stream with multiple blocks
		public unsafe void Read_Pointer(int initialLength)
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
					fixed (byte* pBuffer = readBuffer)
					{
						int bytesToRead = random.Next(1, readBuffer.Length);
						int bytesRead = stream.Read(new IntPtr(pBuffer), bytesToRead);
						if (bytesRead == 0) break;
						readData.AddRange(readBuffer.Take(bytesRead));
					}
				}

				// the stream has been read to the end
				// it should be empty now and read data should equal the expected test data
				Assert.Equal(expectedData.Count, stream.Position);
				Assert.Equal(expectedData.Count, stream.Length);
				Assert.Equal(expectedData, readData.ToArray());
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
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data and attach it to the stream
				var chain = GetRandomTestDataChain(initialLength, StreamMemoryBlockSize, out var expectedData);
				stream.AttachBuffer(chain);

				// read data byte-wise
				var random = new Random(0);
				var readData = new List<byte>(expectedData.Count);
				while (true)
				{
					int readByte = stream.ReadByte();
					if (readByte < 0) break;
					readData.Add((byte)readByte);
				}

				// the stream has been read to the end
				// it should be empty now and read data should equal the expected test data
				Assert.Equal(expectedData.Count, stream.Position);
				Assert.Equal(expectedData.Count, stream.Length);
				Assert.Equal(expectedData, readData);
			}
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
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data
				using (GetRandomTestDataChain(count, StreamMemoryBlockSize, out var list))
				{
					byte[] data = list.ToArray();

					// write the buffer
					stream.Write(data, 0, data.Length);

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
		/// Writes a random set of bytes into the stream using <see cref="MemoryBlockStream.Write(byte[],int,int)"/>.
		/// The write is done with multiple smaller write operations.
		/// </summary>
		[Theory]
		[InlineData(TestDataSize, 8 * 1024)] // chunk size is power of 2 => fills up full blocks
		[InlineData(TestDataSize, 999)]      // chunk size is odd => write spans blocks
		public void Write_Buffer_MultipleOperations(int count, int chunkSize)
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data
				using (GetRandomTestDataChain(count, StreamMemoryBlockSize, out var list))
				{
					byte[] data = list.ToArray();

					// write the buffer in chunks
					int offset = 0;
					do
					{
						int bytesToWrite = Math.Min(data.Length - offset, chunkSize);
						stream.Write(data, offset, bytesToWrite);
						offset += bytesToWrite;
					} while (offset < data.Length);

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
		/// Writes a random set of bytes into the stream using <see cref="MemoryBlockStream.Write(IntPtr,int)"/>.
		/// The write is one in a single operation.
		/// </summary>
		[Theory]
		[InlineData(0)]            // write empty buffer
		[InlineData(1)]            // write buffer with 1 byte
		[InlineData(TestDataSize)] // write huge buffer that results in multiple blocks in the stream
		public unsafe void Write_Pointer_SingleOperation(int count)
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data
				using (GetRandomTestDataChain(count, StreamMemoryBlockSize, out var list))
				{
					byte[] data = list.ToArray();

					// write the buffer
					fixed (byte* pBuffer = data)
					{
						stream.Write(new IntPtr(pBuffer), data.Length);
					}

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
		/// Writes a random set of bytes into the stream using <see cref="MemoryBlockStream.Write(IntPtr,int)"/>.
		/// The write is done with multiple smaller write operations.
		/// </summary>
		[Theory]
		[InlineData(TestDataSize, 8 * 1024)] // chunk size is power of 2 => fills up full blocks
		[InlineData(TestDataSize, 999)]      // chunk size is odd => write spans blocks
		public unsafe void Write_Pointer_MultipleOperations(int count, int chunkSize)
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data
				using (GetRandomTestDataChain(count, StreamMemoryBlockSize, out var list))
				{
					byte[] data = list.ToArray();

					// write the buffer in chunks
					fixed (byte* pBuffer = data)
					{
						int offset = 0;
						do
						{
							int bytesToWrite = Math.Min(data.Length - offset, chunkSize);
							stream.Write(new IntPtr(pBuffer) + offset, bytesToWrite);
							offset += bytesToWrite;
						} while (offset < data.Length);
					}

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
		/// Writes a random set of bytes into the stream using <see cref="MemoryBlockStream.Write(Stream)"/>.
		/// </summary>
		[Theory]
		[InlineData(0)]            // write empty buffer
		[InlineData(1)]            // write buffer with 1 byte
		[InlineData(TestDataSize)] // write huge buffer that results in multiple blocks in the stream
		public void Write_Stream(int count)
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data
				using (GetRandomTestDataChain(count, StreamMemoryBlockSize, out var list))
				{
					byte[] data = list.ToArray();

					// write the buffer
					var otherStream = new MemoryStream(data);
					stream.Write(otherStream);

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
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data
				using (GetRandomTestDataChain(count, StreamMemoryBlockSize, out var list))
				{
					byte[] data = list.ToArray();

					// write the buffer
					foreach (byte x in data) stream.WriteByte(x);

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
