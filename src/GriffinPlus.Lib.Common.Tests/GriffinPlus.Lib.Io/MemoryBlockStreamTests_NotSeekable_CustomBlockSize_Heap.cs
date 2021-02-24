﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Buffers;

namespace GriffinPlus.Lib.Io
{

	/// <summary>
	/// Unit tests targeting the <see cref="MemoryBlockStream"/> class.
	/// The tests create a stream using <see cref="MemoryBlockStream(int,ArrayPool{byte},bool)"/> constructor.
	/// The stream is not seekable and uses a custom block size and allocates buffers on the heap.
	/// </summary>
	public class MemoryBlockStreamTests_NotSeekable_CustomBlockSize_Heap : MemoryBlockStreamTestsBase_NotSeekable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryBlockStreamTests_NotSeekable_CustomBlockSize_Heap"/> class.
		/// </summary>
		public MemoryBlockStreamTests_NotSeekable_CustomBlockSize_Heap() : base(false)
		{
		}

		/// <summary>
		/// Gets the expected size of a memory block in the stream.
		/// </summary>
		protected override int StreamMemoryBlockSize => 8 * 1024;

		/// <summary>
		/// Creates the <see cref="MemoryBlockStream"/> to test.
		/// </summary>
		/// <returns>The created stream.</returns>
		protected override MemoryBlockStream CreateStreamToTest()
		{
			return new MemoryBlockStream(StreamMemoryBlockSize, null, true);
		}
	}

}