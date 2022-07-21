///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib
{

	/// <summary>
	/// The unit to use when formatting a <see cref="DataSize"/>.
	/// </summary>
	public enum DataSizeUnit
	{
		/// <summary>
		/// Unit: Byte.
		/// </summary>
		Byte,

		/// <summary>
		/// Base 2: Automatically chosen unit (units: [kibi, mebi, gibi, tebi, pebi, exbi]byte).
		/// </summary>
		AutoBase2,

		/// <summary>
		/// Base 10 unit: Kibibyte (2^10 bytes).
		/// </summary>
		Kibibyte,

		/// <summary>
		/// Base 10 unit: Mebibyte (2^20 bytes).
		/// </summary>
		Mebibyte,

		/// <summary>
		/// Base 10 unit: Gibibyte (2^30 bytes).
		/// </summary>
		Gibibyte,

		/// <summary>
		/// Base 10 unit: Tebibyte (2^40 bytes).
		/// </summary>
		Tebibyte,

		/// <summary>
		/// Base 10 unit: Pebibyte (2^50 bytes).
		/// </summary>
		Pebibyte,

		/// <summary>
		/// Base 10 unit: Exbibyte (2^60 bytes).
		/// </summary>
		Exbibyte,

		/// <summary>
		/// Automatically chosen unit, base 10 (units: [kilo, mega, giga, tera, peta, exa]byte).
		/// </summary>
		AutoBase10,

		/// <summary>
		/// Base 10 unit: Kilobyte (10^3 bytes).
		/// </summary>
		Kilobyte,

		/// <summary>
		/// Base 10 unit: Megabyte (10^6 bytes).
		/// </summary>
		Megabyte,

		/// <summary>
		/// Base 10 unit: Gigabyte (10^9 bytes).
		/// </summary>
		Gigabyte,

		/// <summary>
		/// Base 10 unit: Terabyte (10^12 bytes).
		/// </summary>
		Terabyte,

		/// <summary>
		/// Base 10 unit: Petabyte (10^15 bytes).
		/// </summary>
		Petabyte,

		/// <summary>
		/// Base 10 unit: Exabyte (10^18 bytes).
		/// </summary>
		Exabyte
	}

}
