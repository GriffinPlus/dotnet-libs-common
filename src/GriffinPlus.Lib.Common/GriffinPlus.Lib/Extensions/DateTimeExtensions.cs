///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib
{
	/// <summary>
	/// Extension methods for <see cref="System.DateTime"/>.
	/// </summary>
	public static class DateTimeExtensions
	{
		/// <summary>
		/// Truncates the specified <see cref="DateTime"/> to the specified fraction.
		/// </summary>
		/// <param name="dateTime">The <see cref="DateTime"/> to truncate.</param>
		/// <param name="precision">
		/// Precision the resulting <see cref="DateTime"/> should have:
		/// - TimeSpan.FromMilliseconds(1) -> truncate to whole milliseconds
		/// - TimeSpan.FromSeconds(1)      -> truncate to whole seconds
		/// - TimeSpan.FromMinutes(1)      -> truncate to whole minutes
		/// </param>
		/// <returns>The truncated <see cref="DateTime"/>.</returns>
		public static DateTime Truncate(this DateTime dateTime, TimeSpan precision)
		{
			if (precision == TimeSpan.Zero) return dateTime;
			return dateTime.AddTicks(-(dateTime.Ticks % precision.Ticks));
		}
	}
}
