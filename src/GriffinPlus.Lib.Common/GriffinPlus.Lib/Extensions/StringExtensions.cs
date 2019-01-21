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
			if (hex.Length % 2 == 1) {
				if (throwIfInvalid) throw new FormatException("The string cannot have an odd number of digits.");
				return null;
			}

			byte[] arr = new byte[hex.Length >> 1];

			int count = hex.Length >> 1;
			for (int i = 0; i < count; ++i)
			{
				int high = GetHexVal(hex[i << 1]);
				int low = GetHexVal(hex[(i << 1) + 1]);
				if (high < 0 || low < 0) {
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
		private static int GetHexVal(char hex)
		{
			int val = (int)hex;
			if (hex >= '0' && hex <= '9' || hex >= 'a' && hex <= 'f' || hex >= 'A' && hex <= 'F') {
				return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
			} else {
				return -1;
			}
		}
	}
}
