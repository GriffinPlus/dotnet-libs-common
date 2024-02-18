///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;

namespace GriffinPlus.Lib
{

	/// <summary>
	/// Size of some piece of data, e.g. a file.
	/// It supports calculations and a very flexible way of formatting the size choosing the optimum unit multiplier.
	/// The user has the choice of binary prefixes as described by IEEE 1541 (https://en.wikipedia.org/wiki/IEEE_1541-2002)
	/// or metric (SI) prefixes (https://en.wikipedia.org/wiki/Metric_prefix). The size is backed by a 64-bit integer value, i.e.
	/// the supported size range is -9,223,372,036,854,775,808 bytes (-9,2 Exabyte) up to 9,223,372,036,854,775,807 bytes (9,2 Exabyte).<br/>
	/// <br/>
	/// The type supports formatting, which can be customized using the following format specifier:<br/>
	/// <br/>
	/// unit[,unit style]<br/>
	/// <br/>
	/// 'unit' may be one of the following:<br/>
	/// - 'B'      : Enforce formatting as byte.<br/>
	/// - 'Metric' : Auto-scale, use metric prefix (default).<br/>
	/// - 'KB'     : Enforce formatting as kilobyte (metric).<br/>
	/// - 'MB'     : Enforce formatting as megabyte (metric).<br/>
	/// - 'GB'     : Enforce formatting as gigabyte (metric).<br/>
	/// - 'TB'     : Enforce formatting as terabyte (metric).<br/>
	/// - 'PB'     : Enforce formatting as petabyte (metric).<br/>
	/// - 'EB'     : Enforce formatting as exabyte (metric).<br/>
	/// - 'Binary' : Auto-scale, use binary prefix.<br/>
	/// - 'KiB'    : Enforce formatting as kibibyte (binary).<br/>
	/// - 'MiB'    : Enforce formatting as mebibyte (binary).<br/>
	/// - 'GiB'    : Enforce formatting as gibibyte (binary).<br/>
	/// - 'TiB'    : Enforce formatting as tebibyte (binary).<br/>
	/// - 'PiB'    : Enforce formatting as pebibyte (binary).<br/>
	/// - 'EiB'    : Enforce formatting as exbibyte (binary).<br/>
	/// <br/>
	/// 'unit style' may be one of the following:<br/>
	/// - 'short' : Use short unit name (default).<br/>
	/// - 'long' : Use long unit name.<br/>
	/// <br/>
	/// The format specifier may be <c>null</c> to default to 'Metric,short'.
	/// </summary>
	public readonly struct DataSize : IComparable<DataSize>, IEquatable<DataSize>, IFormattable
	{
		#region Constants

		private const long Kilo = 1000;
		private const long Mega = 1000 * Kilo;
		private const long Giga = 1000 * Mega;
		private const long Tera = 1000 * Giga;
		private const long Peta = 1000 * Tera;
		private const long Exa  = 1000 * Peta;
		private const long Kibi = 1L << 10;
		private const long Mebi = 1L << 20;
		private const long Gibi = 1L << 30;
		private const long Tebi = 1L << 40;
		private const long Pebi = 1L << 50;
		private const long Exbi = 1L << 60;

		#endregion

		#region Formatting Rules

		/// <summary>
		/// A formatting rule.
		/// </summary>
		private readonly struct Rule
		{
			public readonly DataSizeUnit Unit;        // size unit the rule applies to
			public readonly ulong        ActualLimit; // upper size limit the rule applies to (inclusive)
			public readonly ulong        Divisor;     // divisor to use when scaling the size to the appropriate unit
			public readonly string       LongUnit;    // long name of the unit to use
			public readonly string       ShortUnit;   // short name of the unit to use

			public Rule(
				DataSizeUnit unit,
				ulong        limit,
				ulong        divisor,
				string       longUnit,
				string       shortUnit)
			{
				Unit = unit;
				Divisor = divisor;
				LongUnit = longUnit;
				ShortUnit = shortUnit;

				// formatted sizes near the turning point always have 3 digits and 1 fractional digit
				// => adjust the limit to handle rounding issues properly
				switch (unit)
				{
					case DataSizeUnit.Byte:

					case DataSizeUnit.AutoBase10:
					case DataSizeUnit.Kilobyte:
					case DataSizeUnit.Megabyte:
					case DataSizeUnit.Gigabyte:
					case DataSizeUnit.Terabyte:
					case DataSizeUnit.Petabyte:
					case DataSizeUnit.Exabyte:
						ActualLimit = limit - limit / (10 * 1000) + 5 * (limit / (100 * 1000)) - 1;
						break;

					case DataSizeUnit.AutoBase2:
					case DataSizeUnit.Kibibyte:
					case DataSizeUnit.Mebibyte:
					case DataSizeUnit.Gibibyte:
					case DataSizeUnit.Tebibyte:
					case DataSizeUnit.Pebibyte:
					case DataSizeUnit.Exbibyte:
						ActualLimit = limit - limit / (10 * 1024) + 5 * (limit / (100 * 1024)) - 1;
						break;

					default:
						throw new NotSupportedException($"Unit '{unit}' is not supported.");
				}
			}
		}

		/// <summary>
		/// Rules for formatting using metric units.
		/// </summary>
		private static readonly Rule[] sMetricFormattingRules =
		[
			new Rule(
				DataSizeUnit.Byte,
				1UL * 1000,
				1UL,
				"Byte",
				"B"),
			new Rule(
				DataSizeUnit.Kilobyte,
				1UL * 1000 * 1000,
				1UL * 1000,
				"Kilobyte",
				"KB"),
			new Rule(
				DataSizeUnit.Megabyte,
				1UL * 1000 * 1000 * 1000,
				1UL * 1000 * 1000,
				"Megabyte",
				"MB"),
			new Rule(
				DataSizeUnit.Gigabyte,
				1UL * 1000 * 1000 * 1000 * 1000,
				1UL * 1000 * 1000 * 1000,
				"Gigabyte",
				"GB"),
			new Rule(
				DataSizeUnit.Terabyte,
				1UL * 1000 * 1000 * 1000 * 1000 * 1000,
				1UL * 1000 * 1000 * 1000 * 1000,
				"Terabyte",
				"TB"),
			new Rule(
				DataSizeUnit.Petabyte,
				1UL * 1000 * 1000 * 1000 * 1000 * 1000 * 1000,
				1UL * 1000 * 1000 * 1000 * 1000 * 1000,
				"Petabyte",
				"PB"),
			new Rule(
				DataSizeUnit.Exabyte,
				ulong.MaxValue,
				1UL * 1000 * 1000 * 1000 * 1000 * 1000 * 1000,
				"Exabyte",
				"EB")
		];

		/// <summary>
		/// Rules for formatting using binary units.
		/// </summary>
		private static readonly Rule[] sBinaryFormattingRules =
		[
			new Rule(
				DataSizeUnit.Byte,
				1UL * 1024,
				1UL,
				"Byte",
				"B"),
			new Rule(
				DataSizeUnit.Kibibyte,
				1UL * 1024 * 1024,
				1UL * 1024,
				"Kibibyte",
				"KiB"),
			new Rule(
				DataSizeUnit.Mebibyte,
				1UL * 1024 * 1024 * 1024,
				1UL * 1024 * 1024,
				"Mebibyte",
				"MiB"),
			new Rule(
				DataSizeUnit.Gibibyte,
				1UL * 1024 * 1024 * 1024 * 1024,
				1UL * 1024 * 1024 * 1024,
				"Gibibyte",
				"GiB"),
			new Rule(
				DataSizeUnit.Tebibyte,
				1UL * 1024 * 1024 * 1024 * 1024 * 1024,
				1UL * 1024 * 1024 * 1024 * 1024,
				"Tebibyte",
				"TiB"),
			new Rule(
				DataSizeUnit.Pebibyte,
				1UL * 1024 * 1024 * 1024 * 1024 * 1024 * 1024,
				1UL * 1024 * 1024 * 1024 * 1024 * 1024,
				"Pebibyte",
				"PiB"),
			new Rule(
				DataSizeUnit.Exbibyte,
				1UL * 1024 * 1024 * 1024 * 1024 * 1024 * 1024,
				1UL * 1024 * 1024 * 1024 * 1024 * 1024 * 1024,
				"Exbibyte",
				"EiB")
		];

		#endregion

		#region Creating

		/// <summary>
		/// Initializes a new instance of the <see cref="DataSize"/> struct wrapping the specified number of bytes.
		/// </summary>
		/// <param name="bytes">Number of bytes.</param>
		private DataSize(long bytes)
		{
			TotalBytes = bytes;
		}

		/// <summary>
		/// Creates a new <see cref="DataSize"/> instance from the specified number of bytes.
		/// </summary>
		/// <param name="value">The size in bytes.</param>
		/// <returns>The created <see cref="DataSize"/> instance.</returns>
		public static DataSize FromBytes(long value)
		{
			return new DataSize(value);
		}

		/// <summary>
		/// Creates a new <see cref="DataSize"/> instance from the specified number of kilobytes (10^3 bytes).
		/// </summary>
		/// <param name="value">The size in kilobytes.</param>
		/// <returns>The created <see cref="DataSize"/> instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is too large, the effective size does not fit into a 64-bit signed integer value.
		/// </exception>
		public static DataSize FromKilobytes(long value)
		{
			try
			{
				return new DataSize(checked(Kilo * value));
			}
			catch (OverflowException)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"The specified size ({value} kilobytes) exceeds the supported bounds.");
			}
		}

		/// <summary>
		/// Creates a new <see cref="DataSize"/> instance from the specified number of megabytes (10^6 bytes).
		/// </summary>
		/// <param name="value">The size in megabytes.</param>
		/// <returns>The created <see cref="DataSize"/> instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is too large, the effective size does not fit into a 64-bit signed integer value.
		/// </exception>
		public static DataSize FromMegabytes(long value)
		{
			try
			{
				return new DataSize(checked(Mega * value));
			}
			catch (OverflowException)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"The specified size ({value} megabytes) exceeds the supported bounds.");
			}
		}

		/// <summary>
		/// Creates a new <see cref="DataSize"/> instance from the specified number of gigabytes (10^9 bytes).
		/// </summary>
		/// <param name="value">The size in gigabytes.</param>
		/// <returns>The created <see cref="DataSize"/> instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is too large, the effective size does not fit into a 64-bit signed integer value.
		/// </exception>
		public static DataSize FromGigabytes(long value)
		{
			try
			{
				return new DataSize(checked(Giga * value));
			}
			catch (OverflowException)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"The specified size ({value} gigabytes) exceeds the supported bounds.");
			}
		}

		/// <summary>
		/// Creates a new <see cref="DataSize"/> instance from the specified number of terabytes (10^9 bytes).
		/// </summary>
		/// <param name="value">The size in terabytes.</param>
		/// <returns>The created <see cref="DataSize"/> instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is too large, the effective size does not fit into a 64-bit signed integer value.
		/// </exception>
		public static DataSize FromTerabytes(long value)
		{
			try
			{
				return new DataSize(checked(Tera * value));
			}
			catch (OverflowException)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"The specified size ({value} terabytes) exceeds the supported bounds.");
			}
		}

		/// <summary>
		/// Creates a new <see cref="DataSize"/> instance from the specified number of petabytes (10^12 bytes).
		/// </summary>
		/// <param name="value">The size in petabytes.</param>
		/// <returns>The created <see cref="DataSize"/> instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is too large, the effective size does not fit into a 64-bit signed integer value.
		/// </exception>
		public static DataSize FromPetabytes(long value)
		{
			try
			{
				return new DataSize(checked(Peta * value));
			}
			catch (OverflowException)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"The specified size ({value} petabytes) exceeds the supported bounds.");
			}
		}

		/// <summary>
		/// Creates a new <see cref="DataSize"/> instance from the specified number of exabytes (10^15 bytes).
		/// </summary>
		/// <param name="value">The size in exabytes.</param>
		/// <returns>The created <see cref="DataSize"/> instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is too large, the effective size does not fit into a 64-bit signed integer value.
		/// </exception>
		public static DataSize FromExabytes(long value)
		{
			try
			{
				return new DataSize(checked(Exa * value));
			}
			catch (OverflowException)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"The specified size ({value} exabytes) exceeds the supported bounds.");
			}
		}

		/// <summary>
		/// Creates a new <see cref="DataSize"/> instance from the specified number of kibibytes (2^10 bytes).
		/// </summary>
		/// <param name="value">The size in kibibytes.</param>
		/// <returns>The created <see cref="DataSize"/> instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is too large, the effective size does not fit into a 64-bit signed integer value.
		/// </exception>
		public static DataSize FromKibibytes(long value)
		{
			try
			{
				return new DataSize(checked(Kibi * value));
			}
			catch (OverflowException)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"The specified size ({value} kibibytes) exceeds the supported bounds.");
			}
		}

		/// <summary>
		/// Creates a new <see cref="DataSize"/> instance from the specified number of mebibytes (2^20 bytes).
		/// </summary>
		/// <param name="value">The size in mebibytes.</param>
		/// <returns>The created <see cref="DataSize"/> instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is too large, the effective size does not fit into a 64-bit signed integer value.
		/// </exception>
		public static DataSize FromMebibytes(long value)
		{
			try
			{
				return new DataSize(checked(Mebi * value));
			}
			catch (OverflowException)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"The specified size ({value} mebibytes) exceeds the supported bounds.");
			}
		}

		/// <summary>
		/// Creates a new <see cref="DataSize"/> instance from the specified number of gibibytes (2^30 bytes).
		/// </summary>
		/// <param name="value">The size in gibibytes.</param>
		/// <returns>The created <see cref="DataSize"/> instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is too large, the effective size does not fit into a 64-bit signed integer value.
		/// </exception>
		public static DataSize FromGibibytes(long value)
		{
			try
			{
				return new DataSize(checked(Gibi * value));
			}
			catch (OverflowException)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"The specified size ({value} gibibytes) exceeds the supported bounds.");
			}
		}

		/// <summary>
		/// Creates a new <see cref="DataSize"/> instance from the specified number of tebibytes (2^40 bytes).
		/// </summary>
		/// <param name="value">The size in tebibytes.</param>
		/// <returns>The created <see cref="DataSize"/> instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is too large, the effective size does not fit into a 64-bit signed integer value.
		/// </exception>
		public static DataSize FromTebibytes(long value)
		{
			try
			{
				return new DataSize(checked(Tebi * value));
			}
			catch (OverflowException)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"The specified size ({value} tebibytes) exceeds the supported bounds.");
			}
		}

		/// <summary>
		/// Creates a new <see cref="DataSize"/> instance from the specified number of pebibytes (2^50 bytes).
		/// </summary>
		/// <param name="value">The size in pebibytes.</param>
		/// <returns>The created <see cref="DataSize"/> instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is too large, the effective size does not fit into a 64-bit signed integer value.
		/// </exception>
		public static DataSize FromPebibytes(long value)
		{
			try
			{
				return new DataSize(checked(Pebi * value));
			}
			catch (OverflowException)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"The specified size ({value} pebibytes) exceeds the supported bounds.");
			}
		}

		/// <summary>
		/// Creates a new <see cref="DataSize"/> instance from the specified number of exbibytes (2^60 bytes).
		/// </summary>
		/// <param name="value">The size in exbibytes.</param>
		/// <returns>The created <see cref="DataSize"/> instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is too large, the effective size does not fit into a 64-bit signed integer value.
		/// </exception>
		public static DataSize FromExbibytes(long value)
		{
			try
			{
				return new DataSize(checked(Exbi * value));
			}
			catch (OverflowException)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"The specified size ({value} exbibytes) exceeds the supported bounds.");
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the total number of bytes.
		/// </summary>
		public long TotalBytes { get; }

		#endregion

		#region Calculations

		/// <summary>
		/// Adds two specified <see cref="DataSize"/> instances.
		/// </summary>
		/// <param name="lhs">The first size to add.</param>
		/// <param name="rhs">The second size to add.</param>
		/// <returns>
		/// An object whose value is the sum of the value of <paramref name="lhs"/> and the value of <paramref name="rhs"/>.
		/// </returns>
		public static DataSize operator +(DataSize lhs, DataSize rhs)
		{
			long result;

			try
			{
				result = checked(lhs.TotalBytes + rhs.TotalBytes);
			}
			catch (OverflowException)
			{
				throw new ArgumentException($"The sum of {lhs.TotalBytes} bytes and {rhs.TotalBytes} bytes exceeds the supported bounds.");
			}

			return new DataSize(result);
		}

		/// <summary>
		/// Subtracts the specified <see cref="DataSize"/> from another specified <see cref="DataSize"/>.
		/// </summary>
		/// <param name="lhs">The minuend.</param>
		/// <param name="rhs">The subtrahend.</param>
		/// <returns>
		/// An object whose value is the result of the value of <paramref name="lhs"/> minus the value of <paramref name="rhs"/>.
		/// </returns>
		public static DataSize operator -(DataSize lhs, DataSize rhs)
		{
			long result;

			try
			{
				result = checked(lhs.TotalBytes - rhs.TotalBytes);
			}
			catch (OverflowException)
			{
				throw new ArgumentException($"The difference of {lhs.TotalBytes} bytes and {rhs.TotalBytes} bytes exceeds the supported bounds.");
			}

			return new DataSize(result);
		}

		/// <summary>
		/// Returns a new <see cref="DataSize"/> object whose value is the result of multiplying the specified
		/// <paramref name="size"/> and the specified <paramref name="factor"/>.
		/// </summary>
		/// <param name="size">The size to multiply.</param>
		/// <param name="factor">The factor to multiply the size with.</param>
		/// <returns>
		/// A new object that represents the value of the specified <paramref name="size"/> multiplied by the value of the
		/// specified <paramref name="factor"/>.
		/// </returns>
		public static DataSize operator *(DataSize size, long factor)
		{
			long result;

			try
			{
				result = checked(size.TotalBytes * factor);
			}
			catch (OverflowException)
			{
				throw new ArgumentException($"The product of {size.TotalBytes} bytes and factor {factor} exceeds the supported bounds.");
			}

			return new DataSize(result);
		}

		/// <summary>
		/// Returns a new <see cref="DataSize"/> object whose value is the result of multiplying the specified
		/// <paramref name="factor"/> and the specified <paramref name="size"/>.
		/// </summary>
		/// <param name="factor">The factor.</param>
		/// <param name="size">The size to multiply with the factor.</param>
		/// <returns>
		/// A new object that represents the value of the specified <paramref name="factor"/> multiplied by the value of the
		/// specified <paramref name="size"/>.
		/// </returns>
		public static DataSize operator *(long factor, DataSize size)
		{
			long result;

			try
			{
				result = checked(size.TotalBytes * factor);
			}
			catch (OverflowException)
			{
				throw new ArgumentException($"The product of {size.TotalBytes} bytes and factor {factor} exceeds the supported bounds.");
			}

			return new DataSize(result);
		}

		/// <summary>
		/// Returns a new <see cref="DataSize"/> object which value is the result of division of <paramref name="size"/>
		/// and the specified <paramref name="divisor"/>.
		/// </summary>
		/// <param name="size">The value to be divided.</param>
		/// <param name="divisor">The value to be divided by.</param>
		/// <returns>
		/// A new object that represents the value of <paramref name="size"/> divided by the value of <paramref name="divisor"/>.
		/// </returns>
		public static DataSize operator /(DataSize size, long divisor)
		{
			return new DataSize(size.TotalBytes / divisor);
		}

		#endregion

		#region Comparisons

		/// <summary>
		/// Indicates whether two <see cref="DataSize"/> instances are equal.
		/// </summary>
		/// <param name="lhs">The first time size to compare.</param>
		/// <param name="rhs">The second time size to compare.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="lhs"/> and <paramref name="rhs"/> are equal;
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool operator ==(DataSize lhs, DataSize rhs)
		{
			return lhs.Equals(rhs);
		}

		/// <summary>
		/// Indicates whether two <see cref="DataSize"/> instances are not equal.
		/// </summary>
		/// <param name="lhs">The first time size to compare.</param>
		/// <param name="rhs">The second time size to compare.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="lhs"/> and <paramref name="rhs"/> are not equal;
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool operator !=(DataSize lhs, DataSize rhs)
		{
			return !lhs.Equals(rhs);
		}

		/// <summary>
		/// Indicates whether a specified <see cref="DataSize"/> is less than another specified <see cref="DataSize"/>.
		/// </summary>
		/// <param name="lhs">The first time size to compare.</param>
		/// <param name="rhs">The second time size to compare.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="lhs"/> is smaller than <paramref name="rhs"/>;
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool operator <(DataSize lhs, DataSize rhs)
		{
			return lhs.CompareTo(rhs) < 0;
		}

		/// <summary>
		/// Indicates whether a specified <see cref="DataSize"/> is greater than another specified <see cref="DataSize"/>.
		/// </summary>
		/// <param name="lhs">The first time size to compare.</param>
		/// <param name="rhs">The second time size to compare.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="lhs"/> is greater than <paramref name="rhs"/>;
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool operator >(DataSize lhs, DataSize rhs)
		{
			return lhs.CompareTo(rhs) > 0;
		}

		/// <summary>
		/// Indicates whether a specified <see cref="DataSize"/> is less than or equal to another specified <see cref="DataSize"/>.
		/// </summary>
		/// <param name="lhs">The first time size to compare.</param>
		/// <param name="rhs">The second time size to compare.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="lhs"/> is less than or equal to <paramref name="rhs"/>;
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool operator <=(DataSize lhs, DataSize rhs)
		{
			return lhs.CompareTo(rhs) <= 0;
		}

		/// <summary>
		/// Indicates whether a specified <see cref="DataSize"/> is greater than or equal to another specified <see cref="DataSize"/>.
		/// </summary>
		/// <param name="lhs">The first time size to compare.</param>
		/// <param name="rhs">The second time size to compare.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="lhs"/> is greater than or equal to <paramref name="rhs"/>;
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool operator >=(DataSize lhs, DataSize rhs)
		{
			return lhs.CompareTo(rhs) >= 0;
		}

		/// <summary>
		/// Compares this instance to another <see cref="DataSize"/> object and returns an integer that indicates
		/// whether this instance is smaller than, equal to, or larger than the specified <see cref="DataSize"/> object.
		/// </summary>
		/// <param name="other">The <see cref="DataSize"/> object to compare with.</param>
		/// <returns>
		/// Less than zero: This instance is smaller than <paramref name="other"/>.<br/>
		/// Zero: This instance is the same as <paramref name="other"/>.<br/>
		/// Greater than zero: This instance is larger than <paramref name="other"/>.
		/// </returns>
		public int CompareTo(DataSize other)
		{
			return TotalBytes.CompareTo(other.TotalBytes);
		}

		/// <summary>
		/// Returns a value indicating whether this instance is equal to a specified <see cref="DataSize"/> object.
		/// </summary>
		/// <param name="other">Object to compare with this instance.</param>
		/// <returns>
		/// <c>true</c> if value is a <see cref="DataSize"/> object that represents the same size as the current
		/// <see cref="DataSize"/> structure; otherwise <c>false</c>.
		/// </returns>
		public bool Equals(DataSize other)
		{
			return TotalBytes == other.TotalBytes;
		}

		/// <summary>
		/// Returns a value indicating whether this instance is equal to the specified object.
		/// </summary>
		/// <param name="obj">Object to compare with this instance.</param>
		/// <returns>
		/// <c>true</c> if value is a <see cref="DataSize"/> object that represents the same size as the current
		/// <see cref="DataSize"/> structure; otherwise <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			return obj is DataSize other && Equals(other);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return TotalBytes.GetHashCode();
		}

		#endregion

		#region Formatting

		/// <summary>
		/// Formats the value of the current instance using the specified format.
		/// </summary>
		/// <param name="format">
		/// The format specifier to use: unit[,unit style]<br/>
		/// <br/>
		/// 'unit' may be one of the following:<br/>
		/// - 'B'      : Enforce formatting as byte.<br/>
		/// - 'Metric' : Auto-scale, use metric prefix (default).<br/>
		/// - 'KB'     : Enforce formatting as kilobyte (metric).<br/>
		/// - 'MB'     : Enforce formatting as megabyte (metric).<br/>
		/// - 'GB'     : Enforce formatting as gigabyte (metric).<br/>
		/// - 'TB'     : Enforce formatting as terabyte (metric).<br/>
		/// - 'PB'     : Enforce formatting as petabyte (metric).<br/>
		/// - 'EB'     : Enforce formatting as exabyte (metric).<br/>
		/// - 'Binary' : Auto-scale, use binary prefix.<br/>
		/// - 'KiB'    : Enforce formatting as kibibyte (binary).<br/>
		/// - 'MiB'    : Enforce formatting as mebibyte (binary).<br/>
		/// - 'GiB'    : Enforce formatting as gibibyte (binary).<br/>
		/// - 'TiB'    : Enforce formatting as tebibyte (binary).<br/>
		/// - 'PiB'    : Enforce formatting as pebibyte (binary).<br/>
		/// - 'EiB'    : Enforce formatting as exbibyte (binary).<br/>
		/// <br/>
		/// 'unit style' may be one of the following:<br/>
		/// - 'short' : Use short unit name (default).<br/>
		/// - 'long' : Use long unit name.<br/>
		/// <br/>
		/// The format specifier may be <c>null</c> to default to 'Metric,short'.
		/// </param>
		/// <param name="formatProvider">
		/// The format provider to use to format the value;
		/// <c>null</c> to use <see cref="CultureInfo.CurrentCulture"/>.
		/// </param>
		/// <returns>The formatted data size.</returns>
		/// <exception cref="FormatException">The specified <paramref name="format"/> is not supported.</exception>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			formatProvider ??= CultureInfo.CurrentCulture;

			var unit = DataSizeUnit.AutoBase10;
			var unitStyle = DataSizeUnitStyle.Short;
			if (!string.IsNullOrEmpty(format))
			{
				ReadOnlySpan<char> remaining = format.AsSpan();
				int modifierIndex = remaining.IndexOf(',');
				if (modifierIndex >= 0)
				{
					ReadOnlySpan<char> unitSpecifier = remaining.Slice(0, modifierIndex).Trim();
					remaining = remaining.Slice(modifierIndex + 1);
					unit = ParseDataSizeUnit(format, unitSpecifier);

					// determine whether to use long or short units
					ReadOnlySpan<char> unitStyleSpecifier = remaining.Trim();
					if (unitStyleSpecifier.CompareTo("long".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0)
					{
						unitStyle = DataSizeUnitStyle.Long;
					}
					else if (unitStyleSpecifier.CompareTo("short".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0)
					{
						unitStyle = DataSizeUnitStyle.Short;
					}
					else
					{
						throw new FormatException(
							$"The '{format}' format string is not supported. The unit style '{unitStyleSpecifier.ToString()}' is unknown. " +
							"Please use one of the following modifiers: long, short");
					}
				}
				else
				{
					ReadOnlySpan<char> unitSpecifier = remaining.Trim();
					unit = ParseDataSizeUnit(format, unitSpecifier);
				}
			}

			return Format(formatProvider, unit, unitStyle);
		}

		/// <summary>
		/// Parses the specified unit specifier to the corresponding <see cref="DataSizeUnit"/> value.
		/// </summary>
		/// <param name="format">
		/// Format string that was used to format the size (for generating an exception message only).
		/// </param>
		/// <param name="unitSpecifier">Unit specifier to parse.</param>
		/// <returns>The parsed <see cref="DataSizeUnit"/> value.</returns>
		/// <exception cref="FormatException">The <paramref name="unitSpecifier"/> is not supported.</exception>
		private static DataSizeUnit ParseDataSizeUnit(string format, ReadOnlySpan<char> unitSpecifier)
		{
			if (unitSpecifier.CompareTo("Metric".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0) return DataSizeUnit.AutoBase10;
			if (unitSpecifier.CompareTo("Binary".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0) return DataSizeUnit.AutoBase2;
			if (unitSpecifier.CompareTo("B".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0) return DataSizeUnit.Byte;
			if (unitSpecifier.CompareTo("KB".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0) return DataSizeUnit.Kilobyte;
			if (unitSpecifier.CompareTo("MB".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0) return DataSizeUnit.Megabyte;
			if (unitSpecifier.CompareTo("GB".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0) return DataSizeUnit.Gigabyte;
			if (unitSpecifier.CompareTo("TB".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0) return DataSizeUnit.Terabyte;
			if (unitSpecifier.CompareTo("PB".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0) return DataSizeUnit.Petabyte;
			if (unitSpecifier.CompareTo("EB".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0) return DataSizeUnit.Exabyte;
			if (unitSpecifier.CompareTo("KiB".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0) return DataSizeUnit.Kibibyte;
			if (unitSpecifier.CompareTo("MiB".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0) return DataSizeUnit.Mebibyte;
			if (unitSpecifier.CompareTo("GiB".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0) return DataSizeUnit.Gibibyte;
			if (unitSpecifier.CompareTo("TiB".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0) return DataSizeUnit.Tebibyte;
			if (unitSpecifier.CompareTo("PiB".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0) return DataSizeUnit.Pebibyte;
			if (unitSpecifier.CompareTo("EiB".AsSpan(), StringComparison.OrdinalIgnoreCase) == 0) return DataSizeUnit.Exbibyte;

			throw new FormatException(
				$"The '{format}' format string is not supported. The unit '{unitSpecifier.ToString()}' is unknown. " +
				"Please use one of the following units: Metric, Binary, KB, MB, GB, TB, PB, EB, KiB, MiB, GiB, TiB, PiB, EiB");
		}

		/// <summary>
		/// Formats the value of the current instance using the optimum unit.
		/// </summary>
		/// <returns>The formatted data size.</returns>
		public override string ToString() => ToString(null, null);

		/// <summary>
		/// Formats the current size using the specified base and style.
		/// </summary>
		/// <param name="unit">Unit to use when formatting.</param>
		/// <param name="unitStyle">Style to use when formatting the unit.</param>
		/// <returns>The formatted data size.</returns>
		public string Format(
			DataSizeUnit      unit      = DataSizeUnit.AutoBase10,
			DataSizeUnitStyle unitStyle = DataSizeUnitStyle.Long)
		{
			return Format(CultureInfo.CurrentCulture, TotalBytes, unit, unitStyle);
		}

		/// <summary>
		/// Formats the current size using the specified base and style.
		/// </summary>
		/// <param name="formatProvider">
		/// The format provider to use to format the value;
		/// <c>null</c> to use <see cref="CultureInfo.CurrentCulture"/>.
		/// </param>
		/// <param name="unit">Unit to use when formatting.</param>
		/// <param name="unitStyle">Style to use when formatting the unit.</param>
		/// <returns>The formatted data size.</returns>
		public string Format(
			IFormatProvider   formatProvider,
			DataSizeUnit      unit      = DataSizeUnit.AutoBase10,
			DataSizeUnitStyle unitStyle = DataSizeUnitStyle.Long)
		{
			return Format(formatProvider, TotalBytes, unit, unitStyle);
		}

		/// <summary>
		/// Formats the specified size using the specified base and style.
		/// </summary>
		/// <param name="formatProvider">
		/// The format provider to use to format the value;
		/// <c>null</c> to use <see cref="CultureInfo.CurrentCulture"/>.
		/// </param>
		/// <param name="size">Size to format (in bytes).</param>
		/// <param name="unit">Unit to use when formatting.</param>
		/// <param name="unitStyle">Style to use when formatting the unit.</param>
		/// <returns>The formatted data size.</returns>
		public static string Format(
			IFormatProvider   formatProvider,
			long              size,
			DataSizeUnit      unit      = DataSizeUnit.AutoBase10,
			DataSizeUnitStyle unitStyle = DataSizeUnitStyle.Long)
		{
			formatProvider ??= CultureInfo.CurrentCulture;

			// determine whether the size is negative and strip the sign
			// (it is considered at the end)
			ulong absoluteSize;
			unchecked
			{
				absoluteSize = (ulong)size;
				if (size < 0) absoluteSize = (absoluteSize ^ 0xFFFFFFFFFFFFFFFFUL) + 1;
			}

			Rule[] rules;
			switch (unit)
			{
				case DataSizeUnit.Byte:

				case DataSizeUnit.AutoBase10:
				case DataSizeUnit.Kilobyte:
				case DataSizeUnit.Megabyte:
				case DataSizeUnit.Gigabyte:
				case DataSizeUnit.Terabyte:
				case DataSizeUnit.Petabyte:
				case DataSizeUnit.Exabyte:
					rules = sMetricFormattingRules;
					break;

				case DataSizeUnit.AutoBase2:
				case DataSizeUnit.Kibibyte:
				case DataSizeUnit.Mebibyte:
				case DataSizeUnit.Gibibyte:
				case DataSizeUnit.Tebibyte:
				case DataSizeUnit.Pebibyte:
				case DataSizeUnit.Exbibyte:
					rules = sBinaryFormattingRules;
					break;

				default:
					throw new NotSupportedException($"Unit '{unit}' is not supported.");
			}

			// prepare the sign string
			string sign = size < 0 ? "-" : "";

			// find the formatting rule to use
			Rule rule;
			if (unit == DataSizeUnit.AutoBase2 || unit == DataSizeUnit.AutoBase10)
			{
				int i = 0;
				while (absoluteSize > rules[i].ActualLimit && i < rules.Length - 1) i++;
				rule = rules[i];
			}
			else
			{
				int i = 0;
				while (rules[i].Unit != unit && i < rules.Length - 1) i++;
				rule = rules[i];
			}

			// never show post decimal digits for 'Bytes'
			if (rule.Unit == DataSizeUnit.Byte)
			{
				return unitStyle == DataSizeUnitStyle.Long
					       ? string.Format(formatProvider, "{0}{1} Bytes", sign, absoluteSize)
					       : string.Format(formatProvider, "{0}{1} B", sign, absoluteSize);
			}

			// scale the size using the appropriate divisor
			decimal scaledSize = (decimal)absoluteSize / rule.Divisor;

			// for scaled sizes less than 100 use 2 fractional digits
			// for scaled sizes greater than or equal to 100 use only 1 fractional digit
			if (unitStyle == DataSizeUnitStyle.Long)
			{
				return string.Format(
					formatProvider,
					scaledSize < 100 ? "{0}{1:0.00} {2}s" : "{0}{1:0.0} {2}s",
					sign,
					scaledSize,
					rule.LongUnit);
			}

			return string.Format(
				formatProvider,
				scaledSize < 100 ? "{0}{1:0.00} {2}" : "{0}{1:0.0} {2}",
				sign,
				scaledSize,
				rule.ShortUnit);
		}

		#endregion
	}

}
