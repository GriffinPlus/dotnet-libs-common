///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Buffers;

namespace GriffinPlus.Lib.Io
{

	/// <summary>
	/// Unit tests targeting the <see cref="MemoryBlockStream"/> class.
	/// The tests create a stream using <see cref="MemoryBlockStream(ArrayPool{byte})"/> constructor.
	/// The stream is seekable and uses the default block size and rents buffers from an array pool.
	/// </summary>
	public class MemoryBlockStreamTests_Seekable_Pool : MemoryBlockStreamTestsBase_Seekable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryBlockStreamTests_Seekable_Pool"/> class.
		/// </summary>
		public MemoryBlockStreamTests_Seekable_Pool() : base(true)
		{
		}

		/// <summary>
		/// Gets the expected size of a memory block in the stream.
		/// </summary>
		protected override int StreamMemoryBlockSize => 64 * 1024;

		/// <summary>
		/// Creates the <see cref="MemoryBlockStream"/> to test.
		/// </summary>
		/// <returns>The created stream.</returns>
		protected override MemoryBlockStream CreateStreamToTest()
		{
			return new MemoryBlockStream(BufferPool);
		}
	}

}
