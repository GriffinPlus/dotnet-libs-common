///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib;

/// <summary>
/// Some extension methods for the <see cref="System.UInt64"/> struct.
/// </summary>
public static class UInt64Extensions
{
	/// <summary>
	/// Checks whether the difference between the current value and the specified value is within the specified tolerance.
	/// </summary>
	/// <param name="self">The current value.</param>
	/// <param name="other">The value to compare with.</param>
	/// <param name="tolerance">Tolerable difference between the current and the specified value (must be positive).</param>
	/// <returns>true, if the difference between the current value and the specified value is within the specified tolerance.</returns>
	public static bool Equals(this ulong self, ulong other, ulong tolerance)
	{
		ulong difference = self > other ? self - other : other - self;
		return difference <= tolerance;
	}
}
