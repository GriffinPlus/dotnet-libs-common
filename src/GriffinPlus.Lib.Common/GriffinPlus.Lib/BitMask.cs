///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace GriffinPlus.Lib
{
	/// <summary>
	/// A bit field of variable size that supports comparisons (equality, inequality), unary logical operations (NOT, XOR)
	/// and binary logical operations (AND, OR).
	/// </summary>
	public class BitMask
	{
		private static readonly uint[] sEmptyBitField = new uint[0];
		private uint[] mBitField;

		/// <summary>
		/// Initializes a new instance of the <see cref="BitMask"/> class.
		/// </summary>
		/// <param name="size">Size of the bit field (is rounded up to the next multiple of 32).</param>
		/// <param name="set">Initial value of the bits in the bit mask.</param>
		/// <param name="paddingValue">
		/// true to consider bits outside the mask as 'set';
		/// false to consider them as 'cleared'.
		/// </param>
		public BitMask(int size, bool set, bool paddingValue)
		{
			if (size < 0) throw new ArgumentException("The size of the bit field must be positive.", nameof(size));
			int count = (size + 31) / 32;
			mBitField = count > 0 ? new uint[count] : sEmptyBitField;
			PaddingValue = paddingValue;

			if (set)
			{
				for (int i = 0; i < mBitField.Length; i++)
				{
					mBitField[i] = ~0u;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BitMask"/> class (for internal use only).
		/// </summary>
		private BitMask()
		{

		}

		/// <summary>
		/// Gets a bit mask of zero length with '0' padding.
		/// </summary>
		public static BitMask Zeros { get; } = new BitMask(0, false, false);

		/// <summary>
		/// Gets a bit mask of zero length with '1' padding.
		/// </summary>
		public static BitMask Ones { get; } = new BitMask(0, true, true);

		/// <summary>
		/// Gets the size of the bit mask (in bits).
		/// </summary>
		public int Size => mBitField.Length * 32;

		/// <summary>
		/// Gets the padding value that is used, when accessing a bit outside the defined mask.
		/// </summary>
		public bool PaddingValue { get; private set; }

		/// <summary>
		/// Inverts the specified bit mask.
		/// </summary>
		/// <param name="mask">Bit mask to invert.</param>
		/// <returns>The resulting bit mask.</returns>
		public static BitMask operator ~(BitMask mask)
		{
			BitMask result = new BitMask();
			int count = mask.mBitField.Length;
			result.mBitField = count > 0 ? new uint[count] : sEmptyBitField;
			result.PaddingValue = !mask.PaddingValue;
			for (int i = 0; i < count; i++) result.mBitField[i] = ~mask.mBitField[i];
			return result;
		}

		/// <summary>
		/// Checks whether the specified bit masks are equal.
		/// </summary>
		/// <param name="mask1">First bit mask.</param>
		/// <param name="mask2">Second bit mask.</param>
		/// <returns>true, if the bit masks are equal; otherwise false.</returns>
		public static bool operator ==(BitMask mask1, BitMask mask2)
		{
			if (mask1 == null) throw new ArgumentNullException(nameof(mask1));
			if (mask2 == null) throw new ArgumentNullException(nameof(mask2));

			int count1 = mask1.mBitField.Length;
			int count2 = mask2.mBitField.Length;

			if (count1 < count2)
			{
				for (int i = 0; i < count1; i++)
				{
					if (mask1.mBitField[i] != mask2.mBitField[i])
					{
						return false;
					}
				}

				uint padding = mask1.PaddingValue ? uint.MaxValue : 0;
				for (int i = count1; i < count2; i++)
				{
					if (mask2.mBitField[i] != padding)
					{
						return false;
					}
				}
			}
			else
			{
				for (int i = 0; i < count2; i++)
				{
					if (mask1.mBitField[i] != mask2.mBitField[i])
					{
						return false;
					}
				}

				uint padding = mask2.PaddingValue ? uint.MaxValue : 0;
				for (int i = count2; i < count1; i++)
				{
					if (mask1.mBitField[i] != padding)
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Checks whether the specified bit masks are different.
		/// </summary>
		/// <param name="mask1">First bit mask.</param>
		/// <param name="mask2">Second bit mask.</param>
		/// <returns>true, if the bit masks are equal; otherwise false.</returns>
		public static bool operator !=(BitMask mask1, BitMask mask2)
		{
			return !(mask1 == mask2);
		}

		/// <summary>
		/// Calculates the logical OR of the specified bit masks.
		/// </summary>
		/// <param name="mask1">First bit mask.</param>
		/// <param name="mask2">Second bit mask.</param>
		/// <returns>
		/// The resulting bit mask (has the same padding value as <paramref name="mask1"/>).
		/// </returns>
		public static BitMask operator |(BitMask mask1, BitMask mask2)
		{
			BitMask result = new BitMask();
			int count1 = mask1.mBitField.Length;
			int count2 = mask2.mBitField.Length;
			int count = count1 > count2 ? count1 : count2;
			result.mBitField = count > 0 ? new uint[count] : sEmptyBitField;
			result.PaddingValue = mask1.PaddingValue;

			uint padding1 = mask1.PaddingValue ? uint.MaxValue : 0;
			for (int i = 0; i < count1; i++) result.mBitField[i] = mask1.mBitField[i];
			for (int i = count1; i < count; i++) result.mBitField[i] = padding1;

			uint padding2 = mask2.PaddingValue ? uint.MaxValue : 0;
			for (int i = 0; i < count2; i++) result.mBitField[i] |= mask2.mBitField[i];
			for (int i = count2; i < count; i++) result.mBitField[i] |= padding2;

			return result;
		}

		/// <summary>
		/// Calculates the logical AND of the specified bit masks.
		/// </summary>
		/// <param name="mask1">First bit mask.</param>
		/// <param name="mask2">Second bit mask.</param>
		/// <returns>
		/// The resulting bit mask (has the same padding value as <paramref name="mask1"/>).
		/// </returns>
		public static BitMask operator &(BitMask mask1, BitMask mask2)
		{
			BitMask result = new BitMask();
			int count1 = mask1.mBitField.Length;
			int count2 = mask2.mBitField.Length;
			int count = count1 > count2 ? count1 : count2;
			result.mBitField = count > 0 ? new uint[count] : sEmptyBitField;
			result.PaddingValue = mask1.PaddingValue;

			uint padding1 = mask1.PaddingValue ? uint.MaxValue : 0;
			for (int i = 0; i < count1; i++) result.mBitField[i] = mask1.mBitField[i];
			for (int i = count1; i < count; i++) result.mBitField[i] = padding1;

			uint padding2 = mask2.PaddingValue ? uint.MaxValue : 0;
			for (int i = 0; i < count2; i++) result.mBitField[i] &= mask2.mBitField[i];
			for (int i = count2; i < count; i++) result.mBitField[i] &= padding2;

			return result;
		}

		/// <summary>
		/// Calculates the logical XOR of the specified bit masks.
		/// </summary>
		/// <param name="mask1">First bit mask.</param>
		/// <param name="mask2">Second bit mask.</param>
		/// <returns>
		/// The resulting bit mask (has the same padding value as <paramref name="mask1"/>).
		/// </returns>
		public static BitMask operator ^(BitMask mask1, BitMask mask2)
		{
			BitMask result = new BitMask();
			int count1 = mask1.mBitField.Length;
			int count2 = mask2.mBitField.Length;
			int count = count1 > count2 ? count1 : count2;
			result.mBitField = count > 0 ? new uint[count] : sEmptyBitField;
			result.PaddingValue = mask1.PaddingValue;

			uint padding1 = mask1.PaddingValue ? uint.MaxValue : 0;
			for (int i = 0; i < count1; i++) result.mBitField[i] = mask1.mBitField[i];
			for (int i = count1; i < count; i++) result.mBitField[i] = padding1;

			uint padding2 = mask2.PaddingValue ? uint.MaxValue : 0;
			for (int i = 0; i < count2; i++) result.mBitField[i] ^= mask2.mBitField[i];
			for (int i = count2; i < count; i++) result.mBitField[i] ^= padding2;

			return result;
		}

		/// <summary>
		/// Checks whether the specified bit is set.
		/// </summary>
		/// <param name="bit">Bit to check.</param>
		/// <returns>true, if the specified bit is set; otherwise false.</returns>
		public bool IsBitSet(int bit)
		{
			if (bit < 0) throw new ArgumentException("The bit index must be positive.", nameof(bit));
			int arrayIndex = bit / 32;
			int bitIndex = bit - arrayIndex * 32;
			return arrayIndex < mBitField.Length ? (mBitField[arrayIndex] & (1u << bitIndex)) != 0 : PaddingValue;
		}

		/// <summary>
		/// Checks whether the specified bit is cleared.
		/// </summary>
		/// <param name="bit">Bit to check.</param>
		/// <returns>true, if the specified bit is cleared; otherwise false.</returns>
		public bool IsBitCleared(int bit)
		{
			if (bit < 0) throw new ArgumentException("The bit index must be positive.", nameof(bit));
			int arrayIndex = bit / 32;
			int bitIndex = bit - arrayIndex * 32;
			return arrayIndex < mBitField.Length ? (mBitField[arrayIndex] & (1u << bitIndex)) == 0 : !PaddingValue;
		}

		/// <summary>
		/// Sets the specified bit.
		/// </summary>
		/// <param name="bit">Bit to set.</param>
		/// <exception cref="ArgumentOutOfRangeException">The specified bit is out of bounds.</exception>
		public void SetBit(int bit)
		{
			if (bit < 0) throw new ArgumentOutOfRangeException(nameof(bit), "The bit index must be positive.");
			int arrayIndex = bit / 32;
			int bitIndex = bit - arrayIndex * 32;
			if (arrayIndex >= mBitField.Length) throw new ArgumentOutOfRangeException(nameof(bit), "The specified bit is out of bounds.");
			mBitField[arrayIndex] |= 1u << bitIndex;
		}

		/// <summary>
		/// Clears the specified bit.
		/// </summary>
		/// <param name="bit">Bit to clear.</param>
		/// <exception cref="ArgumentOutOfRangeException">The specified bit is out of bounds.</exception>
		public void ClearBit(int bit)
		{
			if (bit < 0) throw new ArgumentOutOfRangeException(nameof(bit), "The bit index must be positive.");
			int arrayIndex = bit / 32;
			int bitIndex = bit - arrayIndex * 32;
			if (arrayIndex >= mBitField.Length) throw new ArgumentOutOfRangeException(nameof(bit), "The specified bit is out of bounds.");
			mBitField[arrayIndex] &= ~(1u << bitIndex);
		}

		/// <summary>
		/// Sets a number of bits starting at the specified bit.
		/// </summary>
		/// <param name="bit">Bit to start at.</param>
		/// <param name="count">Number of bits to set.</param>
		public void SetBits(int bit, int count)
		{
			if (bit < 0) throw new ArgumentOutOfRangeException(nameof(bit), "The bit index must be positive.");
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "The number of bits must be positive.");
			if (count == 0) return;
			int startArrayIndex = bit / 32;
			int startBitIndex = bit - startArrayIndex * 32;
			int endArrayIndex = (bit + count) / 32;
			int endBitIndex = bit + count - endArrayIndex * 32;
			if (startArrayIndex >= mBitField.Length) throw new ArgumentOutOfRangeException(nameof(bit), "The start bit is out of bounds.");
			if (endBitIndex == 0) endArrayIndex--;
			if (endArrayIndex >= mBitField.Length) throw new ArgumentOutOfRangeException(nameof(count), "The end bit is out of bounds.");

			uint mask = ~0u << startBitIndex;
			for (int i = startArrayIndex; i <= endArrayIndex; i++)
			{
				if (i == endArrayIndex)
				{
					mask &= ~(0u) >> (32 - endBitIndex);
				}

				mBitField[i] |= mask;
				mask = ~0u;
			}
		}

		/// <summary>
		/// Clears a number of bits starting at the specified bit.
		/// </summary>
		/// <param name="bit">Bit to start at.</param>
		/// <param name="count">Number of bits to clear.</param>
		public void ClearBits(int bit, int count)
		{
			if (bit < 0) throw new ArgumentOutOfRangeException(nameof(bit), "The bit index must be positive.");
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "The number of bits must be positive.");
			if (count == 0) return;
			int startArrayIndex = bit / 32;
			int startBitIndex = bit - startArrayIndex * 32;
			int endArrayIndex = (bit + count) / 32;
			int endBitIndex = bit + count - endArrayIndex * 32;
			if (startArrayIndex >= mBitField.Length) throw new ArgumentOutOfRangeException(nameof(bit), "The start bit is out of bounds.");
			if (endBitIndex == 0) endArrayIndex--;
			if (endArrayIndex >= mBitField.Length) throw new ArgumentOutOfRangeException(nameof(count), "The end bit is out of bounds.");

			uint mask = ~0u << startBitIndex;
			for (int i = startArrayIndex; i <= endArrayIndex; i++)
			{
				if (i == endArrayIndex)
				{
					mask &= ~(0u) >> (32 - endBitIndex);
				}

				mBitField[i] &= ~mask;
				mask = ~0u;
			}
		}

		/// <summary>
		/// Gets the mask as an array of <see cref="System.UInt32"/>.
		/// The value at index 0 contains the bits 0-31, the value at index 1 the bits 32-63 and so on.
		/// </summary>
		/// <returns>
		/// The bit mask as an array of <see cref="System.UInt32"/>.
		/// </returns>
		public uint[] AsArray()
		{
			uint[] copy = new uint[mBitField.Length];
			Array.Copy(mBitField, copy, mBitField.Length);
			return copy;
		}

		/// <summary>
		/// Gets the hash code of the bit mask.
		/// </summary>
		/// <returns>Hash code.</returns>
		/// <remarks>
		/// The hash code does not depend on the length of the bit mask. It considers set bits only.
		/// </remarks>
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 0;
				for (int i = 0; i < mBitField.Length; i++)
				{
					hash ^= (int)mBitField[i];
				}
				return hash;
			}
		}

		/// <summary>
		/// Checks whether the current bit mask equals the specified one (same size and same bits set).
		/// </summary>
		/// <param name="obj">Object to compare with.</param>
		/// <returns>true, if the specified bit mask equals the current one; otherwise false.</returns>
		public override bool Equals(object obj)
		{
			BitMask other = obj as BitMask;
			if (other == null) return false;
			return this == other; // use overloaded equality operator
		}

	}
}