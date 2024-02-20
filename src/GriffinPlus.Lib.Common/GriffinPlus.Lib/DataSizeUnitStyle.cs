///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib;

/// <summary>
/// The unit style to use when formatting a <see cref="DataSize"/>.
/// </summary>
public enum DataSizeUnitStyle
{
	/// <summary>
	/// Short unit style.<br/>
	/// Base 2: B, KiB, MiB, GiB, TiB, PiB, EiB, ZiB, YiB<br/>
	/// Base 10: B, KB, MB, GB, TB, PB, EB, ZB, YB
	/// </summary>
	Short,

	/// <summary>
	/// Long unit style.<br/>
	/// Base 2: Byte, Kibibyte, Mebibyte, Gibibyte, Tebibyte, Pebibyte, Exbibyte, Zebibyte, Yobibyte<br/>
	/// Base 10: Byte, Kilobyte, Megabyte, Gigabyte, Terabyte, Petabyte, Exabyte, Zettabyte, Yottabyte
	/// </summary>
	Long
}
