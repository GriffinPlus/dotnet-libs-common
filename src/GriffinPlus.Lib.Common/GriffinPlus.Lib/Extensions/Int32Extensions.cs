﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib
{

	/// <summary>
	/// Extension methods for <see cref="System.Int32"/>.
	/// </summary>
	public static class Int32Extensions
	{
		/// <summary>
		/// Checks whether the difference between the current value and the specified value is within the specified tolerance.
		/// </summary>
		/// <param name="self">The current value.</param>
		/// <param name="other">The value to compare with.</param>
		/// <param name="tolerance">Tolerable difference between the current and the specified value (must be positive).</param>
		/// <returns>true, if the difference between the current value and the specified value is within the specified tolerance.</returns>
		public static bool Equals(this int self, int other, int tolerance)
		{
			if (tolerance < 0) throw new ArgumentException("The specified tolerance must be positive.", nameof(tolerance));
			int difference = self > other ? self - other : other - self;
			return difference <= tolerance;
		}
	}

}
