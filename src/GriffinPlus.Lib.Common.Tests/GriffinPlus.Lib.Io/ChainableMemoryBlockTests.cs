///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

using Xunit;

namespace GriffinPlus.Lib.Io
{

	/// <summary>
	/// Unit tests targetting the <see cref="ChainableMemoryBlock"/> class.
	/// </summary>
	public class ChainableMemoryBlockTests
	{
		/// <summary>
		/// Checks the creation of a memory block.
		/// </summary>
		/// <param name="capacity">Capacity of the memory block to create.</param>
		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		// [InlineData(int.MaxValue)] // will cause problems on build servers with low memory
		public void Create(int capacity)
		{
			var block = new ChainableMemoryBlock(capacity);
			Assert.Equal(capacity, block.Capacity);
			Assert.NotNull(block.Buffer);
			Assert.Equal(capacity, block.Buffer.Length);
			Assert.Equal(0, block.Length);
			Assert.Null(block.Next);
		}

		/// <summary>
		/// Checks setting the effective length of the memory block.
		/// </summary>
		/// <param name="valid">
		/// true, if setting the length is expected to succeed;
		/// false, if setting the length is expected to fail.
		/// </param>
		/// <param name="capacity">Capacity of the memory block to test.</param>
		/// <param name="length">Length to try to set on the memory block.</param>
		[Theory]
		[InlineData(true, 0, 0)]
		[InlineData(true, 100, 0)]
		[InlineData(true, 100, 1)]
		[InlineData(true, 100, 99)]
		[InlineData(true, 100, 100)]
		[InlineData(false, 0, -1)]
		[InlineData(false, 0, 1)]
		[InlineData(false, 100, -1)]
		[InlineData(false, 100, 101)]
		public void Length(bool valid, int capacity, int length)
		{
			var block1 = new ChainableMemoryBlock(capacity);
			if (valid)
			{
				// setting is expected to succeed
				block1.Length = length;
				Assert.Equal(length, block1.Length);
			}
			else
			{
				// setting is expected to fail
				var ex = Assert.Throws<ArgumentOutOfRangeException>(
					() =>
					{
						block1.Length = length;
					});

				Assert.Equal("value", ex.ParamName);
			}
		}

		/// <summary>
		/// Chains two memory blocks and determines the total length.
		/// </summary>
		[Fact]
		public void ChainLength()
		{
			// create the first block
			var block1 = new ChainableMemoryBlock(100);
			block1.Length = 10;
			Assert.Equal(10, block1.ChainLength);

			// create the second block
			var block2 = new ChainableMemoryBlock(200);
			block2.Length = 20;
			Assert.Equal(20, block2.ChainLength);

			// chain both blocks together
			block1.Next = block2;
			Assert.Equal(30, block1.ChainLength);
			Assert.Equal(20, block2.ChainLength);
		}
	}

}
