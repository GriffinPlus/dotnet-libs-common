///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib
{

	/// <summary>
	/// Extension methods for <see cref="System.String"/>.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Converts the current string (which should contain a hexadecimal string) to an array of bytes.
		/// </summary>
		/// <param name="hex">Hexadecimal string (number of characters must be a multiple of two).</param>
		/// <param name="throwIfInvalid">
		/// true to throw an exception, if the specified string is not a valid hex-string;
		/// false to return null, if the specified string is not a valid hex-string.
		/// </param>
		/// <returns>Byte array containing the converted hex string.</returns>
		/// <exception cref="FormatException">The string is not formatted hexadecimal.</exception>
		public static byte[] HexToByteArray(this string hex, bool throwIfInvalid = true)
		{
			if (hex.Length % 2 == 1)
			{
				if (throwIfInvalid) throw new FormatException("The string cannot have an odd number of digits.");
				return null;
			}

			byte[] arr = new byte[hex.Length >> 1];

			int count = hex.Length >> 1;
			for (int i = 0; i < count; ++i)
			{
				int high = GetHexValue(hex[i << 1]);
				int low = GetHexValue(hex[(i << 1) + 1]);
				if (high < 0 || low < 0)
				{
					if (throwIfInvalid) throw new FormatException("The specified string contains non-hexadecimal digits.");
					return null;
				}

				arr[i] = (byte)((high << 4) + low);
			}

			return arr;
		}

		/// <summary>
		/// Converts a single hexadecimal character to its integer representation.
		/// </summary>
		/// <param name="hex">Hexadecimal character (0-9,A-F) to convert.</param>
		/// <returns>
		/// The integer representation of the specified hexadecimal character;
		/// -1, if the character is not a valid hex character.
		/// </returns>
		private static int GetHexValue(char hex)
		{
			int val = hex;
			if (hex is >= '0' and <= '9' || hex is >= 'a' and <= 'f' || hex is >= 'A' and <= 'F')
			{
				return val - (val < 58 ? 48 : val < 97 ? 55 : 87);
			}

			return -1;
		}
	}

}
