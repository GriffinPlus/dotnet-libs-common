///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using Xunit;

namespace GriffinPlus.Lib.Io
{

	/// <summary>
	/// Base class for unit tests targeting the <see cref="MemoryBlockStream"/> class.
	/// The stream instance is expected to be not seekable.
	/// </summary>
	public abstract class MemoryBlockStreamTestsBase_NotSeekable : MemoryBlockStreamTestsBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryBlockStreamTestsBase_NotSeekable"/> class.
		/// </summary>
		/// <param name="usePool"><c>true</c> if the stream uses buffer pooling; otherwise <c>false</c>.</param>
		protected MemoryBlockStreamTestsBase_NotSeekable(bool usePool) : base(usePool)
		{
		}

		/// <summary>
		/// Gets a value indicating whether the stream can seek.
		/// </summary>
		protected override bool StreamCanSeek => false;

		#region SetLength()

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.SetLength"/> throws <see cref="NotSupportedException"/> as the stream does not support seeking.
		/// </summary>
		[Fact]
		public void SetLength()
		{
			var stream = CreateStreamToTest();
			Assert.Throws<NotSupportedException>(() => stream.SetLength(0));
		}

		#endregion

		#region Seek()

		/// <summary>
		/// Checks whether <see cref="MemoryBlockStream.Seek"/> throws <see cref="NotSupportedException"/> as the stream does not support seeking.
		/// </summary>
		[Fact]
		public void Seek()
		{
			var stream = CreateStreamToTest();
			Assert.Throws<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
		}

		#endregion
	}

}
