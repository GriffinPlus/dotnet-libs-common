﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Xunit;

namespace GriffinPlus.Lib.Io
{

	/// <summary>
	/// Unit tests targeting the <see cref="ChainableMemoryBlock"/> class.
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

		/// <summary>
		/// Chains two memory blocks using <see cref="ChainableMemoryBlock.Previous"/>
		/// and determines whether <see cref="ChainableMemoryBlock.Previous"/> and <see cref="ChainableMemoryBlock.Next"/> are set appropriately.
		/// </summary>
		[Fact]
		public void Previous()
		{
			// create chain of two blocks
			var block1 = new ChainableMemoryBlock(100);
			var block2 = new ChainableMemoryBlock(100);
			block2.Previous = block1;

			// check previous/next of block1
			Assert.Null(block1.Previous);
			Assert.Same(block2, block1.Next);

			// check previous/next of block2
			Assert.Same(block1, block2.Previous);
			Assert.Null(block2.Next);
		}

		/// <summary>
		/// Chains two memory blocks using <see cref="ChainableMemoryBlock.Next"/>
		/// and determines whether <see cref="ChainableMemoryBlock.Previous"/> and <see cref="ChainableMemoryBlock.Next"/> are set appropriately.
		/// </summary>
		[Fact]
		public void Next()
		{
			// create chain of two blocks
			var block1 = new ChainableMemoryBlock(100);
			var block2 = new ChainableMemoryBlock(100);
			block1.Next = block2;

			// check previous/next of block1
			Assert.Null(block1.Previous);
			Assert.Same(block2, block1.Next);

			// check previous/next of block2
			Assert.Same(block1, block2.Previous);
			Assert.Null(block2.Next);
		}

		/// <summary>
		/// Chains multiple blocks and tries to get the first block of the chain using <see cref="ChainableMemoryBlock.GetStartOfChain()"/>.
		/// </summary>
		/// <param name="chainLength">Initial length of the chain.</param>
		/// <param name="startIndex">Index of the block to start at.</param>
		[Theory]
		[InlineData(1, 0)] // single block
		[InlineData(5, 0)] // multiple blocks, start at first block
		[InlineData(5, 1)] // multiple blocks, start left of in the middle
		[InlineData(5, 3)] // multiple blocks, start right of in the middle
		[InlineData(5, 4)] // multiple blocks, start at last block
		public void GetStartOfChain_WithoutLength(int chainLength, int startIndex)
		{
			// create chain of blocks
			var blocks = new ChainableMemoryBlock[chainLength];
			for (int i = 0; i < chainLength; i++)
			{
				blocks[i] = new ChainableMemoryBlock(100)
				{
					Length = 10,
					Previous = i > 0 ? blocks[i - 1] : null
				};
			}

			// get the first block of the chain
			var block = blocks[startIndex].GetStartOfChain();
			Assert.Same(blocks[0], block);
		}

		/// <summary>
		/// Chains multiple blocks and tries to get the first block of the chain using <see cref="ChainableMemoryBlock.GetStartOfChain(out long)"/>.
		/// </summary>
		/// <param name="chainLength">Initial length of the chain.</param>
		/// <param name="startIndex">Index of the block to start at.</param>
		[Theory]
		[InlineData(1, 0)] // single block
		[InlineData(5, 0)] // multiple blocks, start at first block
		[InlineData(5, 1)] // multiple blocks, start left of in the middle
		[InlineData(5, 3)] // multiple blocks, start right of in the middle
		[InlineData(5, 4)] // multiple blocks, start at last block
		public void GetStartOfChain_WithLength(int chainLength, int startIndex)
		{
			// create chain of blocks
			var blocks = new ChainableMemoryBlock[chainLength];
			for (int i = 0; i < chainLength; i++)
			{
				blocks[i] = new ChainableMemoryBlock(100)
				{
					Length = 10,
					Previous = i > 0 ? blocks[i - 1] : null
				};
			}

			// calculate the expected accumulated length of the chain
			long expectedAccumulatedLength = 10 * (startIndex + 1);

			// get the first block of the chain and the accumulated length of all blocks on the way
			var block = blocks[startIndex].GetStartOfChain(out long accumulatedLength);
			Assert.Same(blocks[0], block);
			Assert.Equal(expectedAccumulatedLength, accumulatedLength);
		}

		/// <summary>
		/// Chains multiple blocks and tries to get the last block of the chain using <see cref="ChainableMemoryBlock.GetEndOfChain()"/>.
		/// </summary>
		/// <param name="chainLength">Initial length of the chain.</param>
		/// <param name="startIndex">Index of the block to start at.</param>
		[Theory]
		[InlineData(1, 0)] // single block
		[InlineData(5, 0)] // multiple blocks, start at first block
		[InlineData(5, 1)] // multiple blocks, start left of in the middle
		[InlineData(5, 3)] // multiple blocks, start right of in the middle
		[InlineData(5, 4)] // multiple blocks, start at last block
		public void GetEndOfChain_WithoutLength(int chainLength, int startIndex)
		{
			// create chain of blocks
			var blocks = new ChainableMemoryBlock[chainLength];
			for (int i = 0; i < chainLength; i++)
			{
				blocks[i] = new ChainableMemoryBlock(100)
				{
					Length = 10,
					Previous = i > 0 ? blocks[i - 1] : null
				};
			}

			// get the last block of the chain
			var block = blocks[startIndex].GetEndOfChain();
			Assert.Same(blocks[chainLength - 1], block);
		}

		/// <summary>
		/// Chains multiple blocks and tries to get the last block of the chain using <see cref="ChainableMemoryBlock.GetEndOfChain(out long)"/>.
		/// </summary>
		/// <param name="chainLength">Initial length of the chain.</param>
		/// <param name="startIndex">Index of the block to start at.</param>
		[Theory]
		[InlineData(1, 0)] // single block
		[InlineData(5, 0)] // multiple blocks, start at first block
		[InlineData(5, 1)] // multiple blocks, start left of in the middle
		[InlineData(5, 3)] // multiple blocks, start right of in the middle
		[InlineData(5, 4)] // multiple blocks, start at last block
		public void GetEndOfChain_WithLength(int chainLength, int startIndex)
		{
			// create chain of blocks
			var blocks = new ChainableMemoryBlock[chainLength];
			for (int i = 0; i < chainLength; i++)
			{
				blocks[i] = new ChainableMemoryBlock(100)
				{
					Length = 10,
					Previous = i > 0 ? blocks[i - 1] : null
				};
			}

			// calculate the expected accumulated length of the chain
			long expectedAccumulatedLength = 10 * (chainLength - startIndex);

			// get the first block of the chain and the accumulated length of all blocks on the way
			var block = blocks[startIndex].GetEndOfChain(out long accumulatedLength);
			Assert.Same(blocks[chainLength - 1], block);
			Assert.Equal(expectedAccumulatedLength, accumulatedLength);
		}

		/// <summary>
		/// Chains multiple blocks and tries to get all data stored in the chain using <see cref="ChainableMemoryBlock.GetChainData"/>.
		/// </summary>
		[Theory]
		[InlineData(1, 0)] // single block
		[InlineData(5, 0)] // multiple blocks, start at first block
		[InlineData(5, 1)] // multiple blocks, start left of in the middle
		[InlineData(5, 3)] // multiple blocks, start right of in the middle
		[InlineData(5, 4)] // multiple blocks, start at last block
		public void GetChainData(int chainLength, int startIndex)
		{
			// create chain of blocks of variable size and with random data
			var random = new Random(0);
			var expectedData = new List<byte>();
			var blocks = new ChainableMemoryBlock[chainLength];
			for (int i = 0; i < chainLength; i++)
			{
				int blockLength = 10 * (i + 1);
				blocks[i] = new ChainableMemoryBlock(10 * chainLength)
				{
					Length = blockLength,
					Previous = i > 0 ? blocks[i - 1] : null
				};

				byte[] data = new byte[blockLength];
				random.NextBytes(data);
				Array.Copy(data, blocks[i].Buffer, data.Length);
				if (i >= startIndex) expectedData.AddRange(data);
			}

			// get data in the chain starting at the specified block
			byte[] dataInChain = blocks[startIndex].GetChainData();
			Assert.Equal(expectedData, dataInChain);
		}
	}

}
