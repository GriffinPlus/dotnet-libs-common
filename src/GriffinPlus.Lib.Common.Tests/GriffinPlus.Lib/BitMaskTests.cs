///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/GriffinPlus/dotnet-libs-common)
//
// Copyright 2018-2019 Sascha Falk <sascha@falk-online.eu>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for
// the specific language governing permissions and limitations under the License.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using Xunit;

namespace GriffinPlus.Lib
{
	/// <summary>
	/// Unit tests targetting the <see cref="BitMask"/> class.
	/// </summary>
	public class BitMaskTests
	{
		[Fact]
		public void CheckZeros()
		{
			BitMask mask = BitMask.Zeros;
			Assert.Equal(0, mask.Size);
			Assert.False(mask.PaddingValue);
			Assert.Empty(mask.AsArray());
		}

		[Fact]
		public void CheckOnes()
		{
			BitMask mask = BitMask.Ones;
			Assert.Equal(0, mask.Size);
			Assert.True(mask.PaddingValue);
			Assert.Empty(mask.AsArray());
		}

		[Theory]
		// zero-length bitmask (bit value is only defined by padding value)
		[InlineData(0, false, false)]
		[InlineData(0, false, true)]
		[InlineData(0, true,  false)]
		[InlineData(0, true,  true)]
		// small bit mask spanning a single uint32 value only, rounded up to 32 bit
		[InlineData(1, false, false)]
		[InlineData(1, false, true)]
		[InlineData(1, true,  false)]
		[InlineData(1, true,  true)]
		// small bit mask spanning a single uint32 value only, rounded up to 32 bit
		[InlineData(31, false, false)]
		[InlineData(31, false, true)]
		[InlineData(31, true, false)]
		[InlineData(31, true, true)]
		// exact size (single uint32 internally), no rounding
		[InlineData(32, false, false)]
		[InlineData(32, false, true)]
		[InlineData(32, true, false)]
		[InlineData(32, true, true)]
		// large bit mask spanning multiple uint32 values, rounded up to 128 bit
		[InlineData(97, false, false)]
		[InlineData(97, false, true)]
		[InlineData(97, true, false)]
		[InlineData(97, true, true)]
		// large bit mask spanning multiple uint32 values, rounded up to 128 bit
		[InlineData(127, false, false)]
		[InlineData(127, false, true)]
		[InlineData(127, true, false)]
		[InlineData(127, true, true)]
		// large bit mask spanning multiple uint32 values, no rounding
		[InlineData(128, false, false)]
		[InlineData(128, false, true)]
		[InlineData(128, true, false)]
		[InlineData(128, true, true)]
		public void Create(int size, bool initialBitValue, bool paddingValue)
		{
			BitMask mask = new BitMask(size, initialBitValue, paddingValue);

			// check the actual size of the mask in bits
			int effectiveSize = ((size + 31) / 32) * 32;
			Assert.Equal(effectiveSize, mask.Size);

			// check padding value
			Assert.Equal(paddingValue, mask.PaddingValue);

			// check underlying buffer
			uint[] maskArray = mask.AsArray();
			uint[] expectedMaskArray = new uint[effectiveSize / 32];
			for (int i = 0; i < expectedMaskArray.Length; i++) {
				expectedMaskArray[i] = initialBitValue ? ~0u : 0u;
			}
			Assert.Equal(expectedMaskArray, maskArray);
		}

		[Theory]
		// small bit mask
		[InlineData(32, 0)]
		[InlineData(32, 15)]
		[InlineData(32, 31)]
		// large bit mask
		[InlineData(128, 0)]
		[InlineData(128, 15)]
		[InlineData(128, 31)]
		public void SetBit(int size, int bit)
		{
			BitMask mask = new BitMask(size, false, false);

			// check the actual size of the mask in bits
			int effectiveSize = ((size + 31) / 32) * 32;
			Assert.Equal(effectiveSize, mask.Size);

			// clear bit
			mask.SetBit(bit);

			// check underlying buffer
			uint[] maskArray = mask.AsArray();
			int setBitArrayIndex = bit / 32;
			int setBitIndex = bit % 32;
			uint[] expectedMaskArray = new uint[effectiveSize / 32];
			for (int i = 0; i < expectedMaskArray.Length; i++) {
				if (i == setBitArrayIndex) {
					expectedMaskArray[i] = 0u | (1u << setBitIndex);
				} else {
					expectedMaskArray[i] = 0u;
				}
			}
			Assert.Equal(expectedMaskArray, maskArray);
		}

		[Theory]
		// small bit mask
		[InlineData(32, 0)]
		[InlineData(32, 15)]
		[InlineData(32, 31)]
		// large bit mask
		[InlineData(128, 0)]
		[InlineData(128, 15)]
		[InlineData(128, 31)]
		public void ClearBit(int size, int bit)
		{
			BitMask mask = new BitMask(size, true, false);

			// check the actual size of the mask in bits
			int effectiveSize = ((size + 31) / 32) * 32;
			Assert.Equal(effectiveSize, mask.Size);

			// clear bit
			mask.ClearBit(bit);

			// check underlying buffer
			uint[] maskArray = mask.AsArray();
			int clearedBitArrayIndex = bit / 32;
			int clearedBitIndex = bit % 32;
			uint[] expectedMaskArray = new uint[effectiveSize / 32];
			for (int i = 0; i < expectedMaskArray.Length; i++)
			{
				if (i == clearedBitArrayIndex)
				{
					expectedMaskArray[i] = ~0u & ~(1u << clearedBitIndex);
				}
				else
				{
					expectedMaskArray[i] = ~0u;
				}
			}
			Assert.Equal(expectedMaskArray, maskArray);
		}

		[Theory]
		// small bit mask
		[InlineData(32,    0,   0, new uint[] { 0x00000000u })]
		[InlineData(32,    0,   1, new uint[] { 0x00000001u })]
		[InlineData(32,    0,   2, new uint[] { 0x00000003u })]
		[InlineData(32,   30,   0, new uint[] { 0x00000000u })]
		[InlineData(32,   30,   1, new uint[] { 0x40000000u })]
		[InlineData(32,   30,   2, new uint[] { 0xC0000000u })]
		[InlineData(32,    1,  30, new uint[] { 0x7FFFFFFEu })] // all bits except the first and the last one
		// large bit mask
		[InlineData(128,   0,   0, new uint[] { 0x00000000u, 0x00000000u, 0x00000000u, 0x00000000u })]
		[InlineData(128,   0,   1, new uint[] { 0x00000001u, 0x00000000u, 0x00000000u, 0x00000000u })]
		[InlineData(128,   0,   2, new uint[] { 0x00000003u, 0x00000000u, 0x00000000u, 0x00000000u })]
		[InlineData(128,  30,   0, new uint[] { 0x00000000u, 0x00000000u, 0x00000000u, 0x00000000u })]
		[InlineData(128,  30,   1, new uint[] { 0x40000000u, 0x00000000u, 0x00000000u, 0x00000000u })]
		[InlineData(128,  30,   2, new uint[] { 0xC0000000u, 0x00000000u, 0x00000000u, 0x00000000u })]
		[InlineData(128,  30,   3, new uint[] { 0xC0000000u, 0x00000001u, 0x00000000u, 0x00000000u })] // spans sections
		[InlineData(128,  30,   4, new uint[] { 0xC0000000u, 0x00000003u, 0x00000000u, 0x00000000u })] // spans sections
		[InlineData(128,  95,   0, new uint[] { 0x00000000u, 0x00000000u, 0x00000000u, 0x00000000u })] 
		[InlineData(128,  95,   1, new uint[] { 0x00000000u, 0x00000000u, 0x80000000u, 0x00000000u })]
		[InlineData(128,  95,   2, new uint[] { 0x00000000u, 0x00000000u, 0x80000000u, 0x00000001u })] // spans sections
		[InlineData(128, 126,   0, new uint[] { 0x00000000u, 0x00000000u, 0x00000000u, 0x00000000u })]
		[InlineData(128, 126,   1, new uint[] { 0x00000000u, 0x00000000u, 0x00000000u, 0x40000000u })]
		[InlineData(128, 126,   2, new uint[] { 0x00000000u, 0x00000000u, 0x00000000u, 0xC0000000u })]
		[InlineData(128,   1, 126, new uint[] { 0xFFFFFFFEu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0x7FFFFFFFu })] // all bits except the first and the last one
		public void SetBits(int size, int fromBit, int count, uint[] expectedMaskArray)
		{
			BitMask mask = new BitMask(size, false, false);

			// check the actual size of the mask in bits
			int effectiveSize = ((size + 31) / 32) * 32;
			Assert.Equal(effectiveSize, mask.Size);

			// clear bit
			mask.SetBits(fromBit, count);

			// check underlying buffer
			uint[] maskArray = mask.AsArray();
			Assert.Equal(expectedMaskArray, maskArray);
		}

		[Theory]
		// small bit mask
		[InlineData(32,    0,   0, new uint[] { 0xFFFFFFFFu })]
		[InlineData(32,    0,   1, new uint[] { 0xFFFFFFFEu })]
		[InlineData(32,    0,   2, new uint[] { 0xFFFFFFFCu })]
		[InlineData(32,   30,   0, new uint[] { 0xFFFFFFFFu })]
		[InlineData(32,   30,   1, new uint[] { 0xBFFFFFFFu })]
		[InlineData(32,   30,   2, new uint[] { 0x3FFFFFFFu })]
		[InlineData(32,    1,  30, new uint[] { 0x80000001u })] // all bits except the first and the last one
		// large bit mask
		[InlineData(128,   0,   0, new uint[] { 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu })]
		[InlineData(128,   0,   1, new uint[] { 0xFFFFFFFEu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu })]
		[InlineData(128,   0,   2, new uint[] { 0xFFFFFFFCu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu })]
		[InlineData(128,  30,   0, new uint[] { 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu })]
		[InlineData(128,  30,   1, new uint[] { 0xBFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu })]
		[InlineData(128,  30,   2, new uint[] { 0x3FFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu })]
		[InlineData(128,  30,   3, new uint[] { 0x3FFFFFFFu, 0xFFFFFFFEu, 0xFFFFFFFFu, 0xFFFFFFFFu })] // spans sections
		[InlineData(128,  30,   4, new uint[] { 0x3FFFFFFFu, 0xFFFFFFFCu, 0xFFFFFFFFu, 0xFFFFFFFFu })] // spans sections
		[InlineData(128,  95,   0, new uint[] { 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu })] 
		[InlineData(128,  95,   1, new uint[] { 0xFFFFFFFFu, 0xFFFFFFFFu, 0x7FFFFFFFu, 0xFFFFFFFFu })]
		[InlineData(128,  95,   2, new uint[] { 0xFFFFFFFFu, 0xFFFFFFFFu, 0x7FFFFFFFu, 0xFFFFFFFEu })] // spans sections
		[InlineData(128, 126,   0, new uint[] { 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu })]
		[InlineData(128, 126,   1, new uint[] { 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xBFFFFFFFu })]
		[InlineData(128, 126,   2, new uint[] { 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0x3FFFFFFFu })]
		[InlineData(128,   1, 126, new uint[] { 0x00000001u, 0x00000000u, 0x00000000u, 0x80000000u })] // all bits except the first and the last one
		public void ClearBits(int size, int fromBit, int count, uint[] expectedMaskArray)
		{
			BitMask mask = new BitMask(size, true, false);

			// check the actual size of the mask in bits
			int effectiveSize = ((size + 31) / 32) * 32;
			Assert.Equal(effectiveSize, mask.Size);

			// clear bit
			mask.ClearBits(fromBit, count);

			// check underlying buffer
			uint[] maskArray = mask.AsArray();
			Assert.Equal(expectedMaskArray, maskArray);
		}

		[Theory]
		// small bit mask
		[InlineData(32,  0)]
		[InlineData(32,  1)]
		[InlineData(32, 30)]
		[InlineData(32, 31)]
		// large bit mask
		[InlineData(128,   0)]
		[InlineData(128,   1)]
		[InlineData(128, 126)]
		[InlineData(128, 127)]
		public void IsBitSet(int size, int bit)
		{
			BitMask mask = new BitMask(size, false, true);
			Assert.False(mask.IsBitSet(bit));
			mask.SetBit(bit);
			Assert.True(mask.IsBitSet(bit));
		}

		[Theory]
		// small bit mask
		[InlineData(32, 0)]
		[InlineData(32, 1)]
		[InlineData(32, 30)]
		[InlineData(32, 31)]
		// large bit mask
		[InlineData(128, 0)]
		[InlineData(128, 1)]
		[InlineData(128, 126)]
		[InlineData(128, 127)]
		public void IsBitCleared(int size, int bit)
		{
			BitMask mask = new BitMask(size, true, false);
			Assert.False(mask.IsBitCleared(bit));
			mask.ClearBit(bit);
			Assert.True(mask.IsBitCleared(bit));
		}

	}
}
