///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib
{

	/// <summary>
	/// Extension methods for arrays of <see cref="System.Byte"/>.
	/// </summary>
	public static class ByteArrayExtensions
	{
		/// <summary>
		/// Converts the specified array of bytes to a string using hexadecimal encoding.
		/// </summary>
		/// <param name="bytes">The byte array to convert.</param>
		/// <param name="upperCase">
		/// true to emit uppercase letters (A-F);
		/// false to emit lowercase letters (a-f).
		/// </param>
		/// <returns>The specified byte array as a hexadecimal encoded string.</returns>
		public static string ToHexString(this byte[] bytes, bool upperCase = true)
		{
			char[] c = new char[bytes.Length * 2];
			char offset = upperCase ? 'A' : 'a';

			for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx)
			{
				byte b = (byte)(bytes[bx] >> 4);
				c[cx] = (char)(b > 9 ? b - 10 + offset : b + '0');

				b = (byte)(bytes[bx] & 0x0F);
				c[++cx] = (char)(b > 9 ? b - 10 + offset : b + '0');
			}

			return new string(c);
		}

		/// <summary>
		/// Checks whether two byte arrays are equal.
		/// </summary>
		/// <param name="a1">Array to compare.</param>
		/// <param name="a2">Array to compare with.</param>
		/// <returns>true, if both arrays are equal; otherwise false.</returns>
		public static bool Equals(this byte[] a1, byte[] a2)
		{
			if (a1 == a2) return true;
			if (a1 != null && a2 == null || a1 == null) return false;
			if (a1.Length != a2.Length) return false;

			// ReSharper disable once LoopCanBeConvertedToQuery
			for (int i = 0; i < a1.Length; i++)
			{
				if (a1[i] != a2[i]) return false;
			}

			return true;
		}

		/// <summary>
		/// Creates a <see cref="System.Guid"/> from the specified byte array containing a RFC 4122 compliant UUID.
		/// </summary>
		/// <param name="uuid">Buffer containing the UUID (must be at least 16 bytes starting at the specified index).</param>
		/// <param name="index">Index in the byte array to start at.</param>
		/// <returns>The converted <see cref="System.Guid"/>.</returns>
		public static Guid ToRfc4122Guid(this byte[] uuid, int index = 0)
		{
			byte[] buffer = new byte[16];
			Array.Copy(uuid, index, buffer, 0, 16);
			Swap4(buffer, 0);
			Swap2(buffer, 4);
			Swap2(buffer, 6);
			return new Guid(buffer);
		}

		/// <summary>
		/// Swaps two bytes in the specified byte array.
		/// </summary>
		/// <param name="buffer">Buffer containing the bytes to swap.</param>
		/// <param name="index">Index in the byte array to start at.</param>
		public static void Swap2(this byte[] buffer, int index)
		{
			byte swap = buffer[index];
			buffer[index] = buffer[index + 1];
			buffer[index + 1] = swap;
		}

		/// <summary>
		/// Swaps four bytes in the specified byte array.
		/// </summary>
		/// <param name="buffer">Buffer containing the bytes to swap.</param>
		/// <param name="index">Index in the byte array to start at.</param>
		public static void Swap4(this byte[] buffer, int index)
		{
			byte swap1 = buffer[index];
			byte swap2 = buffer[index + 1];
			buffer[index] = buffer[index + 3];
			buffer[index + 1] = buffer[index + 2];
			buffer[index + 2] = swap2;
			buffer[index + 3] = swap1;
		}
	}

}
