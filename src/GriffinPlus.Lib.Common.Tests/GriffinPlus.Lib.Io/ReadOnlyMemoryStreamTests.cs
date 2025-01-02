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

// ReSharper disable UseAwaitUsing

namespace GriffinPlus.Lib.Io;

/// <summary>
/// Unit tests targeting the <see cref="ReadOnlyStream"/> class.
/// </summary>
public class ReadOnlyStreamTests
{
	private const int TestBufferSize = 10;

	private sealed class TestStreams
	{
		public byte[]           Data;
		public MockMemoryStream BackingStream;
		public ReadOnlyStream   StreamToTest;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ReadOnlyStreamTests"/> class.
	/// </summary>
	public ReadOnlyStreamTests() { }

	/// <summary>
	/// Creates instances of all <see cref="ReadOnlyStream"/> configurations to test.
	/// </summary>
	/// <returns>The streams to test.</returns>
	private static IEnumerable<TestStreams> GetStreamsToTest(bool filled)
	{
		if (!filled)
		{
			return
				from canRead in new[] { false, true }
				from canWrite in new[] { false, true }
				from canSeek in new[] { false, true }
				from canTimeout in new[] { false, true }
				from readTimeout in new[] { Timeout.Infinite, 1 }
				from writeTimeout in new[] { Timeout.Infinite, 1 }
				select new MockMemoryStream(
					canRead,
					canWrite,
					canSeek,
					canTimeout,
					readTimeout,
					writeTimeout) into mockStream
				let stream = new ReadOnlyStream(mockStream)
				select new TestStreams { Data = [], BackingStream = mockStream, StreamToTest = stream };
		}

		var random = new Random(0);
		byte[] data = new byte[TestBufferSize];
		random.NextBytes(data);

		return
			from canRead in new[] { false, true }
			from canWrite in new[] { false, true }
			from canSeek in new[] { false, true }
			from canTimeout in new[] { false, true }
			from readTimeout in new[] { Timeout.Infinite, 1 }
			from writeTimeout in new[] { Timeout.Infinite, 1 }
			select new MockMemoryStream(
				canRead,
				canWrite,
				canSeek,
				canTimeout,
				readTimeout,
				writeTimeout,
				data) into mockStream
			let stream = new ReadOnlyStream(mockStream)
			select new TestStreams { Data = data, BackingStream = mockStream, StreamToTest = stream };
	}

	#region Construction and Checking Properties

	/// <summary>
	/// Creates a set of <see cref="ReadOnlyStream"/> instances backed by <see cref="MockMemoryStream"/> instances
	/// mocking up streams with different capabilities and the initial state of their properties:<br/>
	/// - <see cref="ReadOnlyStream.CanRead"/><br/>
	/// - <see cref="ReadOnlyStream.CanSeek"/><br/>
	/// - <see cref="ReadOnlyStream.CanTimeout"/><br/>
	/// - <see cref="ReadOnlyStream.CanWrite"/><br/>
	/// - <see cref="ReadOnlyStream.Length"/><br/>
	/// - <see cref="ReadOnlyStream.Position"/><br/>
	/// - <see cref="ReadOnlyStream.ReadTimeout"/><br/>
	/// - <see cref="ReadOnlyStream.WriteTimeout"/>
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void CreateAndCheckProperties(bool isNotEmpty)
	{
		foreach (TestStreams streams in GetStreamsToTest(isNotEmpty))
		{
			// check capabilities of the stream
			// (the ReadOnlyStream should have the same capabilities as the MemoryStream except for the write capability)
			Assert.Equal(streams.BackingStream.CanRead, streams.StreamToTest.CanRead);
			Assert.Equal(streams.BackingStream.CanSeek, streams.StreamToTest.CanSeek);
			Assert.Equal(streams.BackingStream.CanTimeout, streams.StreamToTest.CanTimeout);
			Assert.False(streams.StreamToTest.CanWrite);

			// check position of the stream
			if (streams.BackingStream.CanSeek)
			{
				Assert.Equal(streams.BackingStream.Position, streams.StreamToTest.Position);
			}
			else
			{
				var exception = Assert.Throws<NotSupportedException>(() => streams.StreamToTest.Position);
				Assert.Equal("The stream does not support seeking.", exception.Message);
			}

			// check length of the stream
			if (streams.BackingStream.CanSeek)
			{
				Assert.Equal(streams.BackingStream.Length, streams.StreamToTest.Length);
			}
			else
			{
				var exception = Assert.Throws<NotSupportedException>(() => streams.StreamToTest.Length);
				Assert.Equal("The stream does not support seeking.", exception.Message);
			}

			// check read timeout of the stream
			if (streams.BackingStream.CanRead && streams.BackingStream.CanTimeout)
			{
				Assert.Equal(streams.BackingStream.ReadTimeout, streams.StreamToTest.ReadTimeout);
			}
			else
			{
				var exception = Assert.Throws<InvalidOperationException>(() => streams.StreamToTest.ReadTimeout);
				Assert.Equal("The stream does not support both timeouts and reading.", exception.Message);
			}

			// check write timeout of the stream
			// (stream will never be able to write...)
			{
				var exception = Assert.Throws<InvalidOperationException>(() => streams.StreamToTest.WriteTimeout);
				Assert.Equal("The stream does not support writing.", exception.Message);
			}
		}
	}

	#endregion

	#region void Dispose()

	[Fact]
	public void Dispose_()
	{
		// create the stream to test
		var mockStream = new MockMemoryStream(true, true, true, true, 0, 0);
		var streamToTest = new ReadOnlyStream(mockStream);

		// dispose the stream
		Assert.False(mockStream.IsDisposed);
		streamToTest.Dispose();
		Assert.True(mockStream.IsDisposed);
	}

	#endregion

	#region Task DisposeAsync()

#if !NET461 && !NET48 && !NETCOREAPP2_2

	[Fact]
	public async Task DisposeAsync_()
	{
		// create the stream to test
		var mockStream = new MockMemoryStream(true, true, true, true, 0, 0);
		var streamToTest = new ReadOnlyStream(mockStream);

		// dispose the stream
		Assert.False(mockStream.IsDisposed);
		await streamToTest.DisposeAsync();
		Assert.True(mockStream.IsDisposed);
	}

#endif

	#endregion

	#region long Position

	/// <summary>
	/// Tests getting the <see cref="ReadOnlyStream.Position"/> property.
	/// </summary>
	[Fact]
	public void Position_Get()
	{
		foreach (TestStreams streams in GetStreamsToTest(true))
		{
			if (streams.BackingStream.CanSeek)
			{
				Assert.Equal(streams.BackingStream.Position, streams.StreamToTest.Position);
			}
			else
			{
				var exception = Assert.Throws<NotSupportedException>(() => streams.StreamToTest.Position);
				Assert.Equal("The stream does not support seeking.", exception.Message);
			}
		}
	}

	/// <summary>
	/// Tests setting the <see cref="ReadOnlyStream.Position"/> property.
	/// </summary>
	[Fact]
	public void Position_Set()
	{
		foreach (TestStreams streams in GetStreamsToTest(true))
		{
			long newPosition = streams.Data.Length;

			if (streams.BackingStream.CanSeek)
			{
				streams.StreamToTest.Position = newPosition;
				Assert.Equal(newPosition, streams.BackingStream.Position);
			}
			else
			{
				var exception = Assert.Throws<NotSupportedException>(() => streams.StreamToTest.Position = newPosition);
				Assert.Equal("The stream does not support seeking.", exception.Message);
			}
		}
	}

	#endregion

	#region int ReadTimeout

	/// <summary>
	/// Tests getting the <see cref="ReadOnlyStream.ReadTimeout"/> property.
	/// </summary>
	[Fact]
	public void ReadTimeout_Get()
	{
		foreach (TestStreams streams in GetStreamsToTest(false))
		{
			// get the property
			// (should reflect the behavior of the MockMemoryStream)
			if (streams.BackingStream.CanRead && streams.BackingStream.CanTimeout)
			{
				Assert.Equal(streams.BackingStream.ReadTimeout, streams.StreamToTest.ReadTimeout);
			}
			else
			{
				var exception = Assert.Throws<InvalidOperationException>(() => streams.StreamToTest.ReadTimeout);
				Assert.Equal("The stream does not support both timeouts and reading.", exception.Message);
			}
		}
	}

	/// <summary>
	/// Tests setting the <see cref="ReadOnlyStream.ReadTimeout"/> property.
	/// </summary>
	[Fact]
	public void ReadTimeout_Set()
	{
		const int newTimeout = 100;

		foreach (TestStreams streams in GetStreamsToTest(false))
		{
			// set the property
			// (should reflect the behavior of the MockMemoryStream)
			if (streams.BackingStream.CanRead && streams.BackingStream.CanTimeout)
			{
				streams.StreamToTest.ReadTimeout = newTimeout;
				Assert.Equal(newTimeout, streams.BackingStream.ReadTimeout);
			}
			else
			{
				var exception = Assert.Throws<InvalidOperationException>(() => streams.StreamToTest.ReadTimeout = newTimeout);
				Assert.Equal("The stream does not support both timeouts and reading.", exception.Message);
			}
		}
	}

	#endregion

	#region int WriteTimeout

	/// <summary>
	/// Tests getting the <see cref="ReadOnlyStream.WriteTimeout"/> property.
	/// </summary>
	[Fact]
	public void WriteTimeout_Get()
	{
		foreach (TestStreams streams in GetStreamsToTest(false))
		{
			// get the property
			var exception = Assert.Throws<InvalidOperationException>(() => streams.StreamToTest.WriteTimeout);
			Assert.Equal("The stream does not support writing.", exception.Message);
		}
	}

	/// <summary>
	/// Tests setting the <see cref="ReadOnlyStream.WriteTimeout"/> property.
	/// </summary>
	[Fact]
	public void WriteTimeout_Set()
	{
		foreach (TestStreams streams in GetStreamsToTest(false))
		{
			// get the property
			var exception = Assert.Throws<InvalidOperationException>(() => streams.StreamToTest.WriteTimeout = 0);
			Assert.Equal("The stream does not support writing.", exception.Message);
		}
	}

	#endregion

	#region IAsyncResult BeginRead(byte[],int,int,AsyncCallback,object) / int EndRead(IAsyncResult)

	/// <summary>
	/// Tests reading using <see cref="ReadOnlyStream.BeginRead(byte[],int,int,AsyncCallback,object)"/> and
	/// <see cref="ReadOnlyStream.EndRead(IAsyncResult)"/>.
	/// </summary>
	[Fact]
	public void BeginRead_and_EndRead()
	{
		foreach (TestStreams streams in GetStreamsToTest(true))
		{
			byte[] buffer = new byte[streams.Data.Length];
			if (streams.BackingStream.CanRead)
			{
				// read stream completely
				IAsyncResult asyncResult1 = streams.StreamToTest.BeginRead(buffer, 0, buffer.Length, CallbackExpectingData, streams);
				Assert.NotNull(asyncResult1);

				// the stream should be at the end now
				IAsyncResult asyncResult2 = streams.StreamToTest.BeginRead(buffer, 0, buffer.Length, CallbackExpectingEndOfStream, streams);
				Assert.NotNull(asyncResult2);
			}
			else
			{
				var exception = Assert.Throws<NotSupportedException>(
					() => streams.StreamToTest.BeginRead(
						buffer,
						0,
						buffer.Length,
						CallbackExpectingData,
						streams));
				Assert.Equal("The stream does not support reading.", exception.Message);
			}
		}
		return;

		static void CallbackExpectingData(IAsyncResult ar)
		{
			Assert.NotNull(ar);
			Assert.True(ar.IsCompleted);
			var streams = ar.AsyncState as TestStreams;
			Assert.NotNull(streams);
			int bytesRead = streams.StreamToTest.EndRead(ar);
			Assert.Equal(streams.Data.Length, bytesRead);
		}

		static void CallbackExpectingEndOfStream(IAsyncResult ar)
		{
			Assert.NotNull(ar);
			Assert.True(ar.IsCompleted);
			var streams = ar.AsyncState as TestStreams;
			Assert.NotNull(streams);
			int bytesRead = streams.StreamToTest.EndRead(ar);
			Assert.Equal(0, bytesRead);
		}
	}

	#endregion

	#region IAsyncResult BeginWrite(byte[],int,int,AsyncCallback,object) / void EndWrite(IAsyncResult)

	/// <summary>
	/// Tests the <see cref="ReadOnlyStream.BeginWrite(byte[],int,int,AsyncCallback,object)"/> method.
	/// </summary>
	[Fact]
	public void BeginWrite()
	{
		byte[] buffer = new byte[TestBufferSize];

		foreach (TestStreams streams in GetStreamsToTest(false))
		{
			// try to write to the stream (should fail)
			var exception = Assert.Throws<NotSupportedException>(() => streams.StreamToTest.BeginWrite(buffer, 0, buffer.Length, null, null));
			Assert.Equal("The stream does not support writing.", exception.Message);
		}
	}

	/// <summary>
	/// Tests the <see cref="ReadOnlyStream.EndWrite(IAsyncResult)"/> method.
	/// </summary>
	[Fact]
	public void EndWrite()
	{
		foreach (TestStreams streams in GetStreamsToTest(false))
		{
			// ReSharper disable once AssignNullToNotNullAttribute
			var exception = Assert.Throws<NotSupportedException>(() => streams.StreamToTest.EndWrite(null));
			Assert.Equal("The stream does not support writing.", exception.Message);
		}
	}

	#endregion

	#region void CopyTo(Stream,int)

#if !NETCOREAPP2_2 && !NET461 && !NET48

	/// <summary>
	/// Tests copying the stream to another stream using <see cref="ReadOnlyStream.CopyTo(Stream,int)"/>.
	/// </summary>
	[Fact]
	public void CopyTo()
	{
		foreach (TestStreams streams in GetStreamsToTest(true))
		{
			var stream = new MemoryStream();
			if (streams.BackingStream.CanRead)
			{
				streams.StreamToTest.CopyTo(stream, 1024);
				Assert.Equal(streams.Data, stream.ToArray());
			}
			else
			{
				var exception = Assert.Throws<NotSupportedException>(() => streams.StreamToTest.CopyTo(stream, 1024));
				Assert.Equal("The stream does not support reading.", exception.Message);
			}
		}
	}

#endif

	#endregion

	#region Task CopyToAsync(Stream,int,CancellationToken)

	/// <summary>
	/// Tests copying the stream to another stream using <see cref="ReadOnlyStream.CopyToAsync(Stream,int,CancellationToken)"/>.
	/// </summary>
	[Fact]
	public async Task CopyToAsync()
	{
		foreach (TestStreams streams in GetStreamsToTest(true))
		{
			var stream = new MemoryStream();
			if (streams.BackingStream.CanRead)
			{
				await streams.StreamToTest.CopyToAsync(stream, 1024, CancellationToken.None);
				Assert.Equal(streams.Data, stream.ToArray());
			}
			else
			{
				var exception = await Assert.ThrowsAsync<NotSupportedException>(() => streams.StreamToTest.CopyToAsync(stream, 1024, CancellationToken.None));
				Assert.Equal("The stream does not support reading.", exception.Message);
			}
		}
	}

	#endregion

	#region void Flush()

	/// <summary>
	/// Tests flushing the stream using <see cref="ReadOnlyStream.Flush"/>.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Flush(bool isNotEmpty)
	{
		foreach (TestStreams streams in GetStreamsToTest(isNotEmpty))
		{
			Assert.Equal(0, streams.BackingStream.FlushCounter);
			streams.StreamToTest.Flush();
			Assert.Equal(1, streams.BackingStream.FlushCounter);
		}
	}

	#endregion

	#region Task FlushAsync()

	/// <summary>
	/// Tests flushing the stream using <see cref="ReadOnlyStream.FlushAsync(CancellationToken)"/>.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task FlushAsync(bool isNotEmpty)
	{
		foreach (TestStreams streams in GetStreamsToTest(isNotEmpty))
		{
			Assert.Equal(0, streams.BackingStream.FlushCounter);
			await streams.StreamToTest.FlushAsync(CancellationToken.None);
			Assert.Equal(1, streams.BackingStream.FlushCounter);
		}
	}

	#endregion

	#region int Read(byte[],int,int)

	/// <summary>
	/// Tests reading using <see cref="ReadOnlyStream.Read(byte[],int,int)"/>.
	/// </summary>
	[Fact]
	public void Read_Buffer()
	{
		foreach (TestStreams streams in GetStreamsToTest(true))
		{
			byte[] buffer = new byte[streams.Data.Length];
			if (streams.BackingStream.CanRead)
			{
				// read data
				int bytesRead = streams.StreamToTest.Read(buffer, 0, buffer.Length);
				Assert.Equal(streams.Data.Length, bytesRead);
				Assert.Equal(streams.Data, buffer.Take(bytesRead));

				// the stream should be at the end now
				bytesRead = streams.StreamToTest.Read(buffer, 0, buffer.Length);
				Assert.Equal(0, bytesRead);
			}
			else
			{
				var exception = Assert.Throws<NotSupportedException>(() => streams.StreamToTest.Read(buffer, 0, buffer.Length));
				Assert.Equal("The stream does not support reading.", exception.Message);
			}
		}
	}

	#endregion

	#region int Read(Span<byte>)

#if !NET461 && !NET48
	/// <summary>
	/// Tests reading using <see cref="ReadOnlyStream.Read(Span{byte})"/>.
	/// </summary>
	[Fact]
	public void Read_Span()
	{
		foreach (TestStreams streams in GetStreamsToTest(true))
		{
			byte[] buffer = new byte[streams.Data.Length];
			if (streams.BackingStream.CanRead)
			{
				// read data
				int bytesRead = streams.StreamToTest.Read(buffer.AsSpan(0, buffer.Length));
				Assert.Equal(streams.Data.Length, bytesRead);
				Assert.Equal(streams.Data, buffer.Take(bytesRead));

				// the stream should be at the end now
				bytesRead = streams.StreamToTest.Read(buffer.AsSpan(0, buffer.Length));
				Assert.Equal(0, bytesRead);
			}
			else
			{
				var exception = Assert.Throws<NotSupportedException>(() => streams.StreamToTest.Read(buffer.AsSpan(0, buffer.Length)));
				Assert.Equal("The stream does not support reading.", exception.Message);
			}
		}
	}
#endif

	#endregion

	#region int ReadAsync(byte[],int,int,CancellationToken)

	/// <summary>
	/// Tests reading using <see cref="ReadOnlyStream.ReadAsync(byte[],int,int,CancellationToken)"/>.
	/// </summary>
	[Fact]
	public async Task ReadAsync_Buffer()
	{
		foreach (TestStreams streams in GetStreamsToTest(true))
		{
			byte[] buffer = new byte[streams.Data.Length];
			if (streams.BackingStream.CanRead)
			{
				// read data
				int bytesRead = await streams.StreamToTest.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None);
				Assert.Equal(streams.Data.Length, bytesRead);
				Assert.Equal(streams.Data, buffer.Take(bytesRead));

				// the stream should be at the end now
				bytesRead = await streams.StreamToTest.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None);
				Assert.Equal(0, bytesRead);
			}
			else
			{
				var exception = await Assert.ThrowsAsync<NotSupportedException>(
					                // ReSharper disable once MustUseReturnValue
					                async () => await streams.StreamToTest.ReadAsync(
						                            buffer,
						                            0,
						                            buffer.Length,
						                            CancellationToken.None));
				Assert.Equal("The stream does not support reading.", exception.Message);
			}
		}
	}

	#endregion

	#region int ReadAsync(Memory<byte>,CancellationToken)

#if !NET461 && !NET48
	/// <summary>
	/// Tests reading using <see cref="ReadOnlyStream.ReadAsync(Memory{byte},CancellationToken)"/>.
	/// </summary>
	[Fact]
	public async Task ReadAsync_Memory()
	{
		foreach (TestStreams streams in GetStreamsToTest(true))
		{
			byte[] buffer = new byte[streams.Data.Length];
			if (streams.BackingStream.CanRead)
			{
				// read data
				int bytesRead = await streams.StreamToTest.ReadAsync(buffer.AsMemory(0, buffer.Length), CancellationToken.None);
				Assert.Equal(streams.Data.Length, bytesRead);
				Assert.Equal(streams.Data, buffer.Take(bytesRead));

				// the stream should be at the end now
				bytesRead = await streams.StreamToTest.ReadAsync(buffer.AsMemory(0, buffer.Length), CancellationToken.None);
				Assert.Equal(0, bytesRead);
			}
			else
			{
				var exception = await Assert.ThrowsAsync<NotSupportedException>(
					                // ReSharper disable once MustUseReturnValue
					                async () => await streams.StreamToTest.ReadAsync(
						                            buffer.AsMemory(0, buffer.Length),
						                            CancellationToken.None));
				Assert.Equal("The stream does not support reading.", exception.Message);
			}
		}
	}
#endif

	#endregion

	#region int ReadByte()

	/// <summary>
	/// Tests reading using <see cref="ReadOnlyStream.ReadByte"/>.
	/// </summary>
	[Fact]
	public void ReadByte()
	{
		foreach (TestStreams streams in GetStreamsToTest(true))
		{
			if (streams.BackingStream.CanRead)
			{
				// read data
				int readByte;
				foreach (byte expectedByte in streams.Data)
				{
					readByte = streams.StreamToTest.ReadByte();
					Assert.Equal(expectedByte, (byte)readByte);
				}

				// the stream should be at the end now
				readByte = streams.StreamToTest.ReadByte();
				Assert.Equal(-1, readByte);
			}
			else
			{
				var exception = Assert.Throws<NotSupportedException>(() => streams.StreamToTest.ReadByte());
				Assert.Equal("The stream does not support reading.", exception.Message);
			}
		}
	}

	#endregion

	#region long Seek(long,SeekOrigin)

	/// <summary>
	/// Tests the <see cref="ReadOnlyStream.Seek(long,SeekOrigin)"/> method.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Seek(bool isNotEmpty)
	{
		foreach (TestStreams streams in GetStreamsToTest(isNotEmpty))
		{
			if (streams.BackingStream.CanSeek)
			{
				long position = streams.StreamToTest.Seek(streams.Data.Length, SeekOrigin.Begin);
				Assert.Equal(streams.Data.Length, position);
				Assert.Equal(streams.Data.Length, streams.StreamToTest.Position);
			}
			else
			{
				var exception = Assert.Throws<NotSupportedException>(() => streams.StreamToTest.Seek(streams.Data.Length, SeekOrigin.Begin));
				Assert.Equal("The stream does not support seeking.", exception.Message);
			}
		}
	}

	#endregion

	#region void SetLength(long)

	/// <summary>
	/// Tests the <see cref="ReadOnlyStream.SetLength(long)"/> method.
	/// </summary>
	[Fact]
	public void SetLength()
	{
		foreach (TestStreams streams in GetStreamsToTest(false))
		{
			var exception = Assert.Throws<NotSupportedException>(() => streams.StreamToTest.SetLength(10));
			Assert.Equal("The stream does not support writing.", exception.Message);
		}
	}

	#endregion

	#region void Write(byte[],int,int)

	/// <summary>
	/// Tests writing using <see cref="ReadOnlyStream.Write(byte[],int,int)"/>.
	/// </summary>
	[Fact]
	public void Write_Buffer()
	{
		byte[] data = new byte[TestBufferSize];
		foreach (TestStreams streams in GetStreamsToTest(false))
		{
			var exception = Assert.Throws<NotSupportedException>(() => streams.StreamToTest.Write(data, 0, data.Length));
			Assert.Equal("The stream does not support writing.", exception.Message);
		}
	}

	#endregion

	#region void Write(ReadOnlySpan<byte>)

#if !NET461 && !NET48

	/// <summary>
	/// Tests writing using <see cref="ReadOnlyStream.Write(ReadOnlySpan{byte})"/>.
	/// </summary>
	[Fact]
	public void Write_ReadOnlySpan()
	{
		byte[] data = new byte[TestBufferSize];
		foreach (TestStreams streams in GetStreamsToTest(false))
		{
			var exception = Assert.Throws<NotSupportedException>(() => streams.StreamToTest.Write(data.AsSpan()));
			Assert.Equal("The stream does not support writing.", exception.Message);
		}
	}

#endif

	#endregion

	#region void WriteByte(byte)

	/// <summary>
	/// Tests writing using <see cref="ReadOnlyStream.WriteByte(byte)"/>.
	/// </summary>
	[Fact]
	public void WriteByte()
	{
		foreach (TestStreams streams in GetStreamsToTest(false))
		{
			var exception = Assert.Throws<NotSupportedException>(() => streams.StreamToTest.WriteByte(0));
			Assert.Equal("The stream does not support writing.", exception.Message);
		}
	}

	#endregion

	#region Task WriteAsync(byte[],int,int,CancellationToken)

	/// <summary>
	/// Tests writing using <see cref="ReadOnlyStream.WriteAsync(byte[],int,int,CancellationToken)"/>.
	/// </summary>
	[Fact]
	public async Task WriteAsync_Buffer()
	{
		byte[] data = new byte[TestBufferSize];
		foreach (TestStreams streams in GetStreamsToTest(false))
		{
			var exception = await Assert.ThrowsAsync<NotSupportedException>(() => streams.StreamToTest.WriteAsync(data, 0, data.Length));
			Assert.Equal("The stream does not support writing.", exception.Message);
		}
	}

	#endregion

	#region Task WriteAsync(ReadOnlyMemory<byte>,CancellationToken)

#if !NET461 && !NET48

	/// <summary>
	/// Tests writing using <see cref="ReadOnlyStream.WriteAsync(ReadOnlyMemory{byte},CancellationToken)"/>.
	/// </summary>
	[Fact]
	public async Task WriteAsync_ReadOnlyMemory()
	{
		byte[] data = new byte[TestBufferSize];
		foreach (TestStreams streams in GetStreamsToTest(false))
		{
			var exception = await Assert.ThrowsAsync<NotSupportedException>(async () => await streams.StreamToTest.WriteAsync(data.AsMemory(0, data.Length), CancellationToken.None));
			Assert.Equal("The stream does not support writing.", exception.Message);
		}
	}

#endif

	#endregion
}
