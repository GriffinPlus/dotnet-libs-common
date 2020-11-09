///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib
{
	/// <summary>
	/// Extension methods for <see cref="System.Double"/>.
	/// </summary>
	public static class DoubleExtensions
	{
		/// <summary>
		/// Checks whether the difference between the current value and the specified value is within the specified tolerance.
		/// </summary>
		/// <param name="self">The current value.</param>
		/// <param name="other">The value to compare with.</param>
		/// <param name="tolerance">Tolerable difference between the current and the specified value (must be positive).</param>
		/// <returns>
		/// true, if the difference between the current value and the specified value is within the specified tolerance;
		/// otherwise false.
		/// </returns>
		public static bool Equals(this double self, double other, double tolerance)
		{
			if (tolerance < 0) throw new ArgumentException("The specified tolerance must be positive.", nameof(tolerance));
			double difference = self > other ? self - other : other - self;
			return difference <= tolerance;
		}
	}
}
