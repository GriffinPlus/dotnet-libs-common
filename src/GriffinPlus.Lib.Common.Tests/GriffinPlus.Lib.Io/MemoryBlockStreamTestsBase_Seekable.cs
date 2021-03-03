///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace GriffinPlus.Lib.Io
{

	/// <summary>
	/// Base class for unit tests targeting the <see cref="MemoryBlockStream"/> class.
	/// The stream instance is expected to be seekable.
	/// </summary>
	public abstract class MemoryBlockStreamTestsBase_Seekable : MemoryBlockStreamTestsBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryBlockStreamTestsBase_Seekable"/> class.
		/// </summary>
		/// <param name="synchronized"><c>true</c> if the stream is synchronized; otherwise <c>false</c>.</param>
		/// <param name="usePool"><c>true</c> if the stream uses buffer pooling; otherwise <c>false</c>.</param>
		protected MemoryBlockStreamTestsBase_Seekable(bool synchronized, bool usePool) : base(synchronized, usePool)
		{
		}

		/// <summary>
		/// Gets a value indicating whether the stream can seek.
		/// </summary>
		protected override bool StreamCanSeek => true;

		#region SetLength()

		/// <summary>
		/// Tests setting the length of the stream using <see cref="MemoryBlockStream.SetLength"/>.
		/// </summary>
		[Theory]
		[InlineData(0, 0, 0)]                                      // empty stream, remains as is
		[InlineData(0, 1, 0)]                                      // empty stream, enlarge to 1 byte (zero), position remains unmodified
		[InlineData(1, 1, 0)]                                      // stream with 1 byte, stream length remains as is, position remains unmodified
		[InlineData(1, 1, 1)]                                      // stream with 1 byte, stream length remains as is, position remains unmodified
		[InlineData(1, 0, 0)]                                      // stream with 1 byte, stream is cleared, position remains unmodified
		[InlineData(1, 0, 1)]                                      // stream with 1 byte, stream is cleared, position is adjusted
		[InlineData(TestDataSize, 0, 0)]                           // huge stream with multiple blocks, stream is cleared, position remains unmodified
		[InlineData(TestDataSize, 0, 1)]                           // huge stream with multiple blocks, stream is cleared, position is adjusted
		[InlineData(TestDataSize, 0, TestDataSize)]                // huge stream with multiple blocks, stream is cleared, position is adjusted
		[InlineData(TestDataSize, 1, 0)]                           // huge stream with multiple blocks, stream is shrunk to 1 byte, position remains unmodified
		[InlineData(TestDataSize, 1, 1)]                           // huge stream with multiple blocks, stream is shrunk to 1 byte, position remains unmodified
		[InlineData(TestDataSize, 1, 2)]                           // huge stream with multiple blocks, stream is shrunk to 1 byte, position is adjusted
		[InlineData(TestDataSize, 1, TestDataSize)]                // huge stream with multiple blocks, stream is shrunk to 1 byte, position is adjusted
		[InlineData(TestDataSize, TestDataSize + 1, 0)]            // huge stream with multiple blocks, stream is enlarged by 1 byte (zero), position remains unmodified
		[InlineData(TestDataSize, TestDataSize + 1, 1)]            // huge stream with multiple blocks, stream is enlarged by 1 byte (zero), position remains unmodified
		[InlineData(TestDataSize, TestDataSize + 1, TestDataSize)] // huge stream with multiple blocks, stream is enlarged by 1 byte (zero), position remains unmodified
		public void SetLength(int oldLength, int newLength, int initialPosition)
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// populate the stream with some initial data, if necessary
				var expectedData = new List<byte>();
				if (oldLength > 0)
				{
					// generate some test data and attach it to the stream
					var chain = GetRandomTestDataChain(oldLength, StreamMemoryBlockSize, out expectedData);
					stream.AttachBuffer(chain);
				}

				// seek stream to the initial position
				stream.Position = initialPosition;
				Assert.Equal(initialPosition, stream.Position);

				// set new length
				stream.SetLength(newLength);

				// the stream should reflect the new length now
				Assert.Equal(newLength, stream.Length);

				// the position should have changed only, if it would have been outside the stream bounds now
				long expectedPosition = Math.Min(initialPosition, newLength);
				Assert.Equal(expectedPosition, stream.Position);

				// adjust expected data
				// (shrink buffer, if the stream gets shorter, add zero bytes, if the stream gets longer)
				if (newLength < oldLength) expectedData.RemoveRange(newLength, expectedData.Count - newLength);
				else expectedData.AddRange(Enumerable.Repeat((byte)0, newLength - oldLength));

				// detach the underlying buffer and check its state
				using (var firstBlock = stream.DetachBuffer())
				{
					if (newLength > 0)
					{
						byte[] buffer = firstBlock.GetChainData();
						Assert.Equal(expectedData, buffer);
					}
					else
					{
						Assert.Null(firstBlock);
					}
				}
			}
		}

		#endregion

		#region Seek()

		/// <summary>
		/// Test data for seeking tests.
		/// </summary>
		public static IEnumerable<object[]> Seek_TestData
		{
			get
			{
				var origins = new[] { SeekOrigin.Begin, SeekOrigin.Current, SeekOrigin.End };

				// empty string, don't change position
				foreach (var origin in origins) yield return new object[] { 0, 0, origin, 0, 0 };

				// stream with a single block (1 byte and 10 bytes) and a huge stream with multiple blocks
				// (check border positions)
				foreach (int length in new[] { 1, 10, TestDataSize })
				{
					foreach (int initialPosition in new[] { 0, 1, length / 2, length - 1, length })
					{
						// seek from the beginning and the end of the stream
						foreach (int offset in new[] { 0, 1, length - 1, length })
						{
							yield return new object[] { length, initialPosition, SeekOrigin.Begin, offset, offset };
							yield return new object[] { length, initialPosition, SeekOrigin.End, -offset, length - offset };
						}

						// seek forward from the current position
						foreach (int offset in new[] { 0, 1, length - initialPosition - 1, length - initialPosition })
						{
							if (initialPosition + offset <= length)
								yield return new object[] { length, initialPosition, SeekOrigin.Current, offset, initialPosition + offset };
						}

						// seek backwards from the current position
						foreach (int offset in new[] { 0, 1, initialPosition - 1, initialPosition })
						{
							if (initialPosition - offset >= 0)
								yield return new object[] { length, initialPosition, SeekOrigin.Current, -offset, initialPosition - offset };
						}
					}
				}
			}
		}

		/// <summary>
		/// Prepares a stream and changes the current stream position using <see cref="MemoryBlockStream.Seek"/>.
		/// </summary>
		[Theory]
		[MemberData(nameof(Seek_TestData))]
		public void Seek(
			int        initialLength,
			int        initialPosition,
			SeekOrigin seekOrigin,
			int        seekOffset,
			int        expectedPosition)
		{
			// create a new stream
			using (var stream = CreateStreamToTest())
			{
				// generate some test data and attach it to the stream
				var chain = GetRandomTestDataChain(initialLength, StreamMemoryBlockSize, out var expectedData);
				stream.AttachBuffer(chain);

				// change position (already involves seeking)
				stream.Position = initialPosition;
				Assert.Equal(initialPosition, stream.Position);

				// seek to the specified position
				stream.Seek(seekOffset, seekOrigin);

				// the position of the stream should now be as expected
				Assert.Equal(expectedPosition, stream.Position);

				// the length should not have been changed
				Assert.Equal(expectedData.Count, stream.Length);
			}
		}

		#endregion

		#region InjectBufferAtCurrentPosition()

		/// <summary>
		/// Test data for the <see cref="InjectBufferAtCurrentPosition"/> test method.
		/// </summary>
		public static IEnumerable<object[]> InjectBufferAtCurrentPosition_TestData
		{
			get
			{
				const int streamBlockSize = 1000;

				foreach (int blocksToInsertSize in new[] { streamBlockSize / 4, streamBlockSize / 3, streamBlockSize / 2, streamBlockSize, 3 * streamBlockSize / 2, 2 * streamBlockSize })
				foreach (int blocksToInsertCount in new[] { 1, 2, 3 })
				foreach (int initialLength in new[] { 0, 1, 500, 999, 1000, 1001, 1500, 1999, 2000, 2001, 2500, 2999, 3000, 3001, 3500, 3999, 4000 })
				foreach (bool overwrite in new[] { false, true })
				foreach (bool advancePosition in new[] { false, true })
				{
					// inject at the beginning of the stream
					yield return new object[]
					{
						streamBlockSize,
						initialLength,
						0,
						overwrite,
						advancePosition,
						blocksToInsertCount,
						blocksToInsertSize
					};


					// inject in the middle of the stream
					if (initialLength > 2)
					{
						yield return new object[]
						{
							streamBlockSize,
							initialLength,
							initialLength / 2,
							overwrite,
							advancePosition,
							blocksToInsertCount,
							blocksToInsertSize
						};
					}

					// inject at the end of the stream
					// (may be in the middle of the last block, so the injected block fits into the remaining space of the block)
					if (initialLength > 1)
					{
						yield return new object[]
						{
							streamBlockSize,
							initialLength,
							initialLength - 1,
							overwrite,
							advancePosition,
							blocksToInsertCount,
							blocksToInsertSize
						};
					}
				}
			}
		}

		/// <summary>
		/// Injects a chain of memory blocks at the current position of the stream using <see cref="MemoryBlockStream.InjectBufferAtCurrentPosition"/>.
		/// The initial stream is empty.
		/// </summary>
		[Theory]
		[MemberData(nameof(InjectBufferAtCurrentPosition_TestData))]
		public void InjectBufferAtCurrentPosition(
			int  streamBlockSize,
			int  initialLength,
			int  position,
			bool overwrite,
			bool advancePosition,
			int  blockToInsertCount,
			int  blockToInsertSize)
		{
			using (var stream = CreateStreamToTest(streamBlockSize))
			{
				// generate some test data and attach it to the stream
				var chain = GetRandomTestDataChain(initialLength, streamBlockSize, out var initialStreamData);
				stream.AttachBuffer(chain);

				// set position of the stream to inject the blocks into
				stream.Position = position;

				// inject a chain of blocks into the existing chain of blocks backing the stream
				var chainToInsert = GetRandomTestDataChain(blockToInsertCount * blockToInsertSize, blockToInsertSize, out var dataToInsert);
				stream.InjectBufferAtCurrentPosition(
					chainToInsert,
					overwrite,
					advancePosition);

				// check whether the stream length reflects the change
				long expectedLength = overwrite
					                      ? Math.Max(initialStreamData.Count, position + dataToInsert.Count)
					                      : initialStreamData.Count + dataToInsert.Count;
				Assert.Equal(expectedLength, stream.Length);

				// check whether the stream position has changed as expected
				long expectedPosition = advancePosition
					                        ? position + dataToInsert.Count
					                        : position;
				Assert.Equal(expectedPosition, stream.Position);

				// check whether the data in the stream has changed as expected
				var expectedData = new List<byte>();
				expectedData.AddRange(initialStreamData.Take(position));
				expectedData.AddRange(dataToInsert);
				expectedData.AddRange(initialStreamData.Skip(position));
				if (overwrite) expectedData.RemoveRange(position + dataToInsert.Count, Math.Min(dataToInsert.Count, expectedData.Count - position - dataToInsert.Count));
				using (var detachedBuffer = stream.DetachBuffer())
				{
					byte[] data = detachedBuffer.GetChainData();
					Assert.Equal(expectedData.Count, data.Length);
					Assert.Equal(expectedData, data);
				}
			}
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.InjectBufferAtCurrentPosition"/> blocks, if the stream is synchronized,
		/// and does not block, if the stream is not synchronized.
		/// </summary>
		[Fact]
		public void InjectBufferAtCurrentPosition_WaitOnLockIfSynchronized()
		{
			void Operation(MemoryBlockStream stream)
			{
				// stream is empty, but that's irrelevant for the locking behavior
				var chain = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out var data);
				stream.InjectBufferAtCurrentPosition(chain, false, false);
			}

			TestWaitOnLockIfSynchronized(Operation);
		}

		#endregion

		#region InjectBufferAtCurrentPositionAsync()

		/// <summary>
		/// Injects a chain of memory blocks at the current position of the stream using <see cref="MemoryBlockStream.InjectBufferAtCurrentPositionAsync"/>.
		/// The initial stream is empty.
		/// </summary>
		[Theory]
		[MemberData(nameof(InjectBufferAtCurrentPosition_TestData))]
		public async Task InjectBufferAtCurrentPositionAsync(
			int  streamBlockSize,
			int  initialLength,
			int  position,
			bool overwrite,
			bool advancePosition,
			int  blockToInsertCount,
			int  blockToInsertSize)
		{
			using (var stream = CreateStreamToTest(streamBlockSize))
			{
				// generate some test data and attach it to the stream
				var chain = GetRandomTestDataChain(initialLength, streamBlockSize, out var initialStreamData);
				await stream.AttachBufferAsync(chain).ConfigureAwait(false);

				// set position of the stream to inject the blocks into
				stream.Position = position;

				// inject a chain of blocks into the existing chain of blocks backing the stream
				var chainToInsert = GetRandomTestDataChain(blockToInsertCount * blockToInsertSize, blockToInsertSize, out var dataToInsert);
				await stream.InjectBufferAtCurrentPositionAsync(
						chainToInsert,
						overwrite,
						advancePosition,
						CancellationToken.None)
					.ConfigureAwait(false);

				// check whether the stream length reflects the change
				long expectedLength = overwrite
					                      ? Math.Max(initialStreamData.Count, position + dataToInsert.Count)
					                      : initialStreamData.Count + dataToInsert.Count;
				Assert.Equal(expectedLength, stream.Length);

				// check whether the stream position has changed as expected
				long expectedPosition = advancePosition
					                        ? position + dataToInsert.Count
					                        : position;
				Assert.Equal(expectedPosition, stream.Position);

				// check whether the data in the stream has changed as expected
				var expectedData = new List<byte>();
				expectedData.AddRange(initialStreamData.Take(position));
				expectedData.AddRange(dataToInsert);
				expectedData.AddRange(initialStreamData.Skip(position));
				if (overwrite) expectedData.RemoveRange(position + dataToInsert.Count, Math.Min(dataToInsert.Count, expectedData.Count - position - dataToInsert.Count));
				using (var detachedBuffer = await stream.DetachBufferAsync(CancellationToken.None).ConfigureAwait(false))
				{
					byte[] data = detachedBuffer.GetChainData();
					Assert.Equal(expectedData.Count, data.Length);
					Assert.Equal(expectedData, data);
				}
			}
		}

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.InjectBufferAtCurrentPositionAsync"/> blocks,
		/// if the stream is synchronized, and does not block, if the stream is not synchronized. Furthermore tests
		/// cancellation, if the operation is blocked at the lock as well as cancellation with a pre-signaled token.
		/// </summary>
		[Fact]
		public async Task InjectBufferAtCurrentPositionAsync_WaitOnLockIfSynchronized()
		{
			async Task Operation(MemoryBlockStream stream, CancellationToken cancellationToken)
			{
				var chain = GetRandomTestDataChain(TestDataSize, StreamMemoryBlockSize, out var data);
				try
				{
					await stream
						.InjectBufferAtCurrentPositionAsync(chain, false, false, cancellationToken)
						.ConfigureAwait(false);
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
	}

}
