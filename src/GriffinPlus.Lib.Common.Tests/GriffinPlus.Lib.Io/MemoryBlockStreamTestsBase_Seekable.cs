///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
		/// <param name="usePool"><c>true</c> if the stream uses buffer pooling; otherwise <c>false</c>.</param>
		protected MemoryBlockStreamTestsBase_Seekable(bool usePool) : base(usePool)
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
	}

}
