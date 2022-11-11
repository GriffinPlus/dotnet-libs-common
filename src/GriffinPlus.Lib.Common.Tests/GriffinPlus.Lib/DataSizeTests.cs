///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;

using Xunit;

#pragma warning disable IDE0060   // Remove unused parameter
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters

namespace GriffinPlus.Lib
{

	/// <summary>
	/// Unit tests targeting the <see cref="DataSize"/> struct.
	/// </summary>
	public class DataSizeTests
	{
		#region Creation

		#region DataSize FromBytes(long)

		[Theory]
		[InlineData(long.MinValue)]
		[InlineData(long.MaxValue)]
		public void FromBytes(long bytes)
		{
			DataSize size = DataSize.FromBytes(bytes);
			Assert.Equal(bytes, size.TotalBytes);
		}

		#endregion

		#region DataSize FromKilobytes(long)

		[Theory]
		[InlineData(long.MinValue / 1000L)]
		[InlineData(long.MaxValue / 1000L)]
		public void FromKilobytes(long kilobytes)
		{
			DataSize size = DataSize.FromKilobytes(kilobytes);
			Assert.Equal(1000L * kilobytes, size.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue / 1000L - 1)]
		[InlineData(long.MaxValue / 1000L + 1)]
		public void FromKilobytes_OutOfRange(long kilobytes)
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => DataSize.FromKilobytes(kilobytes));
			Assert.Equal("value", exception.ParamName);
		}

		#endregion

		#region DataSize FromMegabytes(long)

		[Theory]
		[InlineData(long.MinValue / 1000000L)]
		[InlineData(long.MaxValue / 1000000L)]
		public void FromMegabytes(long megabytes)
		{
			DataSize size = DataSize.FromMegabytes(megabytes);
			Assert.Equal(1000000L * megabytes, size.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue / 1000000L - 1)]
		[InlineData(long.MaxValue / 1000000L + 1)]
		public void FromMegabytes_OutOfRange(long megabytes)
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => DataSize.FromMegabytes(megabytes));
			Assert.Equal("value", exception.ParamName);
		}

		#endregion

		#region DataSize FromGigabytes(long)

		[Theory]
		[InlineData(long.MinValue / 1000000000L)]
		[InlineData(long.MaxValue / 1000000000L)]
		public void FromGigabytes(long gigabytes)
		{
			DataSize size = DataSize.FromGigabytes(gigabytes);
			Assert.Equal(1000000000L * gigabytes, size.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue / 1000000000L - 1)]
		[InlineData(long.MaxValue / 1000000000L + 1)]
		public void FromGigabytes_OutOfRange(long gigabytes)
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => DataSize.FromGigabytes(gigabytes));
			Assert.Equal("value", exception.ParamName);
		}

		#endregion

		#region DataSize FromTerabytes(long)

		[Theory]
		[InlineData(long.MinValue / 1000000000000L)]
		[InlineData(long.MaxValue / 1000000000000L)]
		public void FromTerabytes(long terabytes)
		{
			DataSize size = DataSize.FromTerabytes(terabytes);
			Assert.Equal(1000000000000L * terabytes, size.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue / 1000000000000L - 1)]
		[InlineData(long.MaxValue / 1000000000000L + 1)]
		public void FromTerabytes_OutOfRange(long terabytes)
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => DataSize.FromTerabytes(terabytes));
			Assert.Equal("value", exception.ParamName);
		}

		#endregion

		#region DataSize FromPetabytes(long)

		[Theory]
		[InlineData(long.MinValue / 1000000000000000L)]
		[InlineData(long.MaxValue / 1000000000000000L)]
		public void FromPetabytes(long petabytes)
		{
			DataSize size = DataSize.FromPetabytes(petabytes);
			Assert.Equal(1000000000000000L * petabytes, size.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue / 1000000000000000L - 1)]
		[InlineData(long.MaxValue / 1000000000000000L + 1)]
		public void FromPetabytes_OutOfRange(long petabytes)
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => DataSize.FromPetabytes(petabytes));
			Assert.Equal("value", exception.ParamName);
		}

		#endregion

		#region DataSize FromExabytes(long)

		[Theory]
		[InlineData(long.MinValue / 1000000000000000000L)]
		[InlineData(long.MaxValue / 1000000000000000000L)]
		public void FromExabytes(long exabytes)
		{
			DataSize size = DataSize.FromExabytes(exabytes);
			Assert.Equal(1000000000000000000L * exabytes, size.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue / 1000000000000000000L - 1)]
		[InlineData(long.MaxValue / 1000000000000000000L + 1)]
		public void FromExabytes_OutOfRange(long exabytes)
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => DataSize.FromExabytes(exabytes));
			Assert.Equal("value", exception.ParamName);
		}

		#endregion

		#region DataSize FromKibibytes(long)

		[Theory]
		[InlineData(long.MinValue / (1L << 10))]
		[InlineData(long.MaxValue / (1L << 10))]
		public void FromKibibytes(long kibibytes)
		{
			DataSize size = DataSize.FromKibibytes(kibibytes);
			Assert.Equal((1L << 10) * kibibytes, size.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue / (1L << 10) - 1)]
		[InlineData(long.MaxValue / (1L << 10) + 1)]
		public void FromKibibytes_OutOfRange(long kibibytes)
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => DataSize.FromKibibytes(kibibytes));
			Assert.Equal("value", exception.ParamName);
		}

		#endregion

		#region DataSize FromMebibytes(long)

		[Theory]
		[InlineData(long.MinValue / (1L << 20))]
		[InlineData(long.MaxValue / (1L << 20))]
		public void FromMebibytes(long mebibytes)
		{
			DataSize size = DataSize.FromMebibytes(mebibytes);
			Assert.Equal((1L << 20) * mebibytes, size.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue / (1L << 20) - 1)]
		[InlineData(long.MaxValue / (1L << 20) + 1)]
		public void FromMebibytes_OutOfRange(long mebibytes)
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => DataSize.FromMebibytes(mebibytes));
			Assert.Equal("value", exception.ParamName);
		}

		#endregion

		#region DataSize FromGibibytes(long)

		[Theory]
		[InlineData(long.MinValue / (1L << 30))]
		[InlineData(long.MaxValue / (1L << 30))]
		public void FromGibibytes(long gibibytes)
		{
			DataSize size = DataSize.FromGibibytes(gibibytes);
			Assert.Equal((1L << 30) * gibibytes, size.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue / (1L << 30) - 1)]
		[InlineData(long.MaxValue / (1L << 30) + 1)]
		public void FromGibibytes_OutOfRange(long gibibytes)
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => DataSize.FromGibibytes(gibibytes));
			Assert.Equal("value", exception.ParamName);
		}

		#endregion

		#region DataSize FromTebibytes(long)

		[Theory]
		[InlineData(long.MinValue / (1L << 40))]
		[InlineData(long.MaxValue / (1L << 40))]
		public void FromTebibytes(long tebibytes)
		{
			DataSize size = DataSize.FromTebibytes(tebibytes);
			Assert.Equal((1L << 40) * tebibytes, size.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue / (1L << 40) - 1)]
		[InlineData(long.MaxValue / (1L << 40) + 1)]
		public void FromTebibytes_OutOfRange(long tebibytes)
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => DataSize.FromTebibytes(tebibytes));
			Assert.Equal("value", exception.ParamName);
		}

		#endregion

		#region DataSize FromPebibytes(long)

		[Theory]
		[InlineData(long.MinValue / (1L << 50))]
		[InlineData(long.MaxValue / (1L << 50))]
		public void FromPebibytes(long pebibytes)
		{
			DataSize size = DataSize.FromPebibytes(pebibytes);
			Assert.Equal((1L << 50) * pebibytes, size.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue / (1L << 50) - 1)]
		[InlineData(long.MaxValue / (1L << 50) + 1)]
		public void FromPebibytes_OutOfRange(long petabytes)
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => DataSize.FromPebibytes(petabytes));
			Assert.Equal("value", exception.ParamName);
		}

		#endregion

		#region DataSize FromExbibytes(long)

		[Theory]
		[InlineData(long.MinValue / (1L << 60))]
		[InlineData(long.MaxValue / (1L << 60))]
		public void FromExbibytes(long exbibytes)
		{
			DataSize size = DataSize.FromExbibytes(exbibytes);
			Assert.Equal((1L << 60) * exbibytes, size.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue / (1L << 60) - 1)]
		[InlineData(long.MaxValue / (1L << 60) + 1)]
		public void FromExbibytes_OutOfRange(long exbibytes)
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => DataSize.FromExbibytes(exbibytes));
			Assert.Equal("value", exception.ParamName);
		}

		#endregion

		#endregion

		#region Calculations

		#region DataSize operator+(DataSize, DataSize)

		[Theory]
		[InlineData(long.MinValue, 1L)]
		[InlineData(long.MaxValue, -1L)]
		public void OperatorAdd(long x, long y)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize sizeY = DataSize.FromBytes(y);
			DataSize result = sizeX + sizeY;
			Assert.Equal(x + y, result.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue, -1L)]
		[InlineData(long.MaxValue, 1L)]
		public void OperatorAdd_ResultIsOutOfRange(long x, long y)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize sizeY = DataSize.FromBytes(y);
			Assert.Throws<ArgumentException>(() => sizeX + sizeY);
		}

		#endregion

		#region DataSize operator-(DataSize, DataSize)

		[Theory]
		[InlineData(long.MinValue, -1L)]
		[InlineData(long.MaxValue, 1L)]
		public void OperatorSubtract(long x, long y)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize sizeY = DataSize.FromBytes(y);
			DataSize result = sizeX - sizeY;
			Assert.Equal(x - y, result.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue, 1L)]
		[InlineData(long.MaxValue, -1L)]
		public void OperatorSubtract_ResultIsOutOfRange(long x, long y)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize sizeY = DataSize.FromBytes(y);
			Assert.Throws<ArgumentException>(() => sizeX - sizeY);
		}

		#endregion

		#region DataSize operator*(DataSize, long)

		[Theory]
		[InlineData(long.MinValue, 1L)]
		[InlineData(-2L, -2L)]
		[InlineData(-2L, 2L)]
		[InlineData(2L, -2L)]
		[InlineData(2L, 2L)]
		[InlineData(long.MaxValue, 1L)]
		public void OperatorMultiply_DataSizeWithFactor(long x, long factor)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize result = sizeX * factor;
			Assert.Equal(x * factor, result.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue / 2 - 1, 2L)]
		[InlineData(long.MaxValue / 2 + 1, 2L)]
		public void OperatorMultiply_DataSizeWithFactor_ResultIsOutOfRange(long x, long factor)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			Assert.Throws<ArgumentException>(() => sizeX * factor);
		}

		#endregion

		#region DataSize operator*(long, DataSize)

		[Theory]
		[InlineData(long.MinValue, 1L)]
		[InlineData(-2L, -2L)]
		[InlineData(-2L, 2L)]
		[InlineData(2L, -2L)]
		[InlineData(2L, 2L)]
		[InlineData(long.MaxValue, 1L)]
		public void OperatorMultiply_FactorWithDataSize(long x, long factor)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize result = factor * sizeX;
			Assert.Equal(x * factor, result.TotalBytes);
		}

		[Theory]
		[InlineData(long.MinValue / 2 - 1, 2L)]
		[InlineData(long.MaxValue / 2 + 1, 2L)]
		public void OperatorMultiply_FactorWithDataSize_ResultIsOutOfRange(long x, long factor)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			Assert.Throws<ArgumentException>(() => factor * sizeX);
		}

		#endregion

		#region DataSize operator/(DataSize, long)

		[Theory]
		[InlineData(long.MinValue, 1L)]
		[InlineData(long.MinValue, 2L)]
		[InlineData(-4L, -2L)]
		[InlineData(-4L, 2L)]
		[InlineData(4L, -2L)]
		[InlineData(4L, 2L)]
		[InlineData(long.MaxValue, 2L)]
		[InlineData(long.MaxValue, 1L)]
		public void OperatorDivide_DataSizeWithFactor(long x, long divisor)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize result = sizeX / divisor;
			Assert.Equal(x / divisor, result.TotalBytes);
		}

		#endregion

		#endregion

		#region Comparisons

		public static IEnumerable<object[]> ComparisonTestData
		{
			get
			{
				yield return new object[] { long.MinValue, long.MinValue, 0 };
				yield return new object[] { long.MinValue, long.MinValue + 1, -1 };
				yield return new object[] { 0L, -1L, 1 };
				yield return new object[] { 0L, 0L, 0 };
				yield return new object[] { 0L, 1L, -1 };
				yield return new object[] { long.MaxValue, long.MaxValue - 1, 1 };
				yield return new object[] { long.MaxValue, long.MaxValue, 0 };
			}
		}

		#region bool operator<(DataSize)

		[Theory]
		[MemberData(nameof(ComparisonTestData))]
		public void OperatorLessThan(long x, long y, int comparisonResult)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize sizeY = DataSize.FromBytes(y);
			bool result = sizeX < sizeY;
			Assert.Equal(comparisonResult < 0, result);
		}

		#endregion

		#region bool operator<=(DataSize)

		[Theory]
		[MemberData(nameof(ComparisonTestData))]
		public void OperatorLessThanOrEqual(long x, long y, int comparisonResult)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize sizeY = DataSize.FromBytes(y);
			bool result = sizeX <= sizeY;
			Assert.Equal(comparisonResult <= 0, result);
		}

		#endregion

		#region bool operator>=(DataSize)

		[Theory]
		[MemberData(nameof(ComparisonTestData))]
		public void OperatorGreaterThanOrEqual(long x, long y, int comparisonResult)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize sizeY = DataSize.FromBytes(y);
			bool result = sizeX >= sizeY;
			Assert.Equal(comparisonResult >= 0, result);
		}

		#endregion

		#region bool operator>(DataSize)

		[Theory]
		[MemberData(nameof(ComparisonTestData))]
		public void OperatorGreaterThan(long x, long y, int comparisonResult)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize sizeY = DataSize.FromBytes(y);
			bool result = sizeX > sizeY;
			Assert.Equal(comparisonResult > 0, result);
		}

		#endregion

		#region bool operator==(DataSize)

		[Theory]
		[MemberData(nameof(ComparisonTestData))]
		public void OperatorEquality(long x, long y, int comparisonResult)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize sizeY = DataSize.FromBytes(y);
			bool result = sizeX == sizeY;
			Assert.Equal(comparisonResult == 0, result);
		}

		#endregion

		#region bool operator!=(DataSize)

		[Theory]
		[MemberData(nameof(ComparisonTestData))]
		public void OperatorInequality(long x, long y, int comparisonResult)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize sizeY = DataSize.FromBytes(y);
			bool result = sizeX != sizeY;
			Assert.Equal(comparisonResult != 0, result);
		}

		#endregion

		#region int CompareTo(DataSize)

		[Theory]
		[MemberData(nameof(ComparisonTestData))]
		public void CompareTo(long x, long y, int comparisonResult)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize sizeY = DataSize.FromBytes(y);
			int result = sizeX.CompareTo(sizeY);
			Assert.Equal(comparisonResult, result);
		}

		#endregion

		#region bool Equals(DataSize)

		[Theory]
		[MemberData(nameof(ComparisonTestData))]
		public void Equals_DataSize(long x, long y, int comparisonResult)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize sizeY = DataSize.FromBytes(y);
			bool result = sizeX.Equals(sizeY);
			Assert.Equal(comparisonResult == 0, result);
		}

		#endregion

		#region bool Equals(object)

		[Theory]
		[MemberData(nameof(ComparisonTestData))]
		public void Equals_Object(long x, long y, int comparisonResult)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			DataSize sizeY = DataSize.FromBytes(y);
			bool result = sizeX.Equals((object)sizeY);
			Assert.Equal(comparisonResult == 0, result);
		}

		#endregion

		#region int GetHashCode()

		[Theory]
		[InlineData(long.MinValue)]
		[InlineData(long.MaxValue)]
		public void GetHashCode_(long x)
		{
			DataSize sizeX = DataSize.FromBytes(x);
			int result = sizeX.GetHashCode();
			Assert.Equal(x.GetHashCode(), result);
		}

		#endregion

		#endregion

		#region Formatting

		public static IEnumerable<object[]> FormattingTestData
		{
			get
			{
				// -----------------------------------------------------------------------------------------------------------------
				// auto-scaling
				// -----------------------------------------------------------------------------------------------------------------

				// ReSharper disable once InconsistentNaming
				long MV(int order) // metric value
				{
					long value = 1;
					for (int i = 0; i < 3 * order; i++) value *= 10;
					return value;
				}

				// ReSharper disable once InconsistentNaming
				long MTP(int order) // metric turning point
				{
					long value = 1;
					for (int i = 0; i < order; i++) value *= 1000;
					return value - value / (10 * 1000) + 5 * (value / (100 * 1000));
				}

				// ReSharper disable once InconsistentNaming
				long BV(int order) // binary value
				{
					return 1L << (10 * order);
				}

				// ReSharper disable once InconsistentNaming
				long BTP(int order) // binary turning point
				{
					long value = 1L << (10 * order);
					return value - value / (10 * 1024) + 5 * (value / (100 * 1024));
				}

				//
				// minimum, auto-scaling, different formats
				// (tests the various combinations of metric/binary units with short/long unit styles)
				//

				// long metric unit style
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"-9.22 Exabytes"
				};

				// short metric unit style
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"-9.22 EB"
				};

				// defaults to short metric unit style
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric",
					"-9.22 EB"
				};

				// defaults to short metric unit style
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"",
					"-9.22 EB"
				};

				// long binary unit style
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"-8.00 Exbibytes"
				};

				// short binary unit style
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"-8.00 EiB"
				};

				// defaults to long binary unit style
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary",
					"-8.00 EiB"
				};

				//
				// sizes in between, auto-scaling, long metric units
				//

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"0 Bytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					1L,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"1 Bytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					999L,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"999 Bytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					1000L,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"1.00 Kilobytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(2) - 1,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"999.9 Kilobytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(2),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"1.00 Megabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MV(2),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"1.00 Megabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(3) - 1,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"999.9 Megabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(3),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"1.00 Gigabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MV(3),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"1.00 Gigabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(4) - 1,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"999.9 Gigabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(4),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"1.00 Terabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MV(4),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"1.00 Terabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(5) - 1,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"999.9 Terabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(5),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"1.00 Petabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MV(5),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"1.00 Petabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(6) - 1,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"999.9 Petabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(6),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"1.00 Exabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MV(6),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"1.00 Exabytes"
				};

				//
				// sizes in between, auto-scaling, short metric units
				//

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"0 B"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					1L,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"1 B"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					999L,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"999 B"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					1000L,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"1.00 KB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(2) - 1,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"999.9 KB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(2),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"1.00 MB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MV(2),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"1.00 MB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(3) - 1,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"999.9 MB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(3),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"1.00 GB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MV(3),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"1.00 GB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(4) - 1,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"999.9 GB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(4),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"1.00 TB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MV(4),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"1.00 TB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(5) - 1,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"999.9 TB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(5),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"1.00 PB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MV(5),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"1.00 PB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(6) - 1,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"999.9 PB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MTP(6),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"1.00 EB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					MV(6),
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"1.00 EB"
				};

				//
				// sizes in between, auto-scaling, long binary units
				//

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"0 Bytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					1L,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1 Bytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BV(1) - 1,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1023 Bytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BV(1),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1.00 Kibibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(2) - 1,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1023.9 Kibibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(2),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1.00 Mebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BV(2),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1.00 Mebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(3) - 1,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1023.9 Mebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(3),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1.00 Gibibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BV(3),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1.00 Gibibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(4) - 1,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1023.9 Gibibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(4),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1.00 Tebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BV(4),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1.00 Tebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(5) - 1,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1023.9 Tebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(5),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1.00 Pebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BV(5),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1.00 Pebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(6) - 1,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1023.9 Pebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(6),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1.00 Exbibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BV(6),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"1.00 Exbibytes"
				};

				//
				// sizes in between, auto-scaling, short binary units
				//

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"0 B"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					1L,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1 B"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BV(1) - 1,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1023 B"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BV(1),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1.00 KiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(2) - 1,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1023.9 KiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(2),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1.00 MiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BV(2),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1.00 MiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(3) - 1,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1023.9 MiB"
				};
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(3),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1.00 GiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BV(3),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1.00 GiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(4) - 1,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1023.9 GiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(4),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1.00 TiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BV(4),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1.00 TiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(5) - 1,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1023.9 TiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(5),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1.00 PiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BV(5),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1.00 PiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(6) - 1,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1023.9 PiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BTP(6),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1.00 EiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					BV(6),
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"1.00 EiB"
				};

				//
				// maximum, auto-scaling, different formats
				// (tests the various combinations of metric/binary units with short/long unit styles)
				//

				// long metric unit style
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Long,
					"metric,long",
					"9.22 Exabytes"
				};

				// short metric unit style
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric,short",
					"9.22 EB"
				};

				// default to short metric unit style
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"metric",
					"9.22 EB"
				};

				// default to short metric unit style
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.AutoBase10,
					DataSizeUnitStyle.Short,
					"",
					"9.22 EB"
				};

				// long binary unit style
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Long,
					"binary,long",
					"8.00 Exbibytes"
				};

				// short binary unit style
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary,short",
					"8.00 EiB"
				};

				// default to short binary unit style
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.AutoBase2,
					DataSizeUnitStyle.Short,
					"binary",
					"8.00 EiB"
				};

				// -----------------------------------------------------------------------------------------------------------------
				// fixed scaling
				// -----------------------------------------------------------------------------------------------------------------

				//
				// minimum, fixed units, different formats
				// (tests the various combinations of metric/binary units with short/long unit styles)
				//

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Byte,
					DataSizeUnitStyle.Long,
					"B,long",
					"-9223372036854775808 Bytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Byte,
					DataSizeUnitStyle.Short,
					"B,short",
					"-9223372036854775808 B"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Byte,
					DataSizeUnitStyle.Short,
					"B",
					"-9223372036854775808 B"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Kilobyte,
					DataSizeUnitStyle.Long,
					"KB,long",
					"-9223372036854775.8 Kilobytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Kilobyte,
					DataSizeUnitStyle.Short,
					"KB,short",
					"-9223372036854775.8 KB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Kilobyte,
					DataSizeUnitStyle.Short,
					"KB",
					"-9223372036854775.8 KB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Megabyte,
					DataSizeUnitStyle.Long,
					"MB,long",
					"-9223372036854.8 Megabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Megabyte,
					DataSizeUnitStyle.Short,
					"MB,short",
					"-9223372036854.8 MB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Megabyte,
					DataSizeUnitStyle.Short,
					"MB",
					"-9223372036854.8 MB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Gigabyte,
					DataSizeUnitStyle.Long,
					"GB,long",
					"-9223372036.9 Gigabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Gigabyte,
					DataSizeUnitStyle.Short,
					"GB,short",
					"-9223372036.9 GB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Gigabyte,
					DataSizeUnitStyle.Short,
					"GB",
					"-9223372036.9 GB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Terabyte,
					DataSizeUnitStyle.Long,
					"TB,long",
					"-9223372.0 Terabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Terabyte,
					DataSizeUnitStyle.Short,
					"TB,short",
					"-9223372.0 TB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Terabyte,
					DataSizeUnitStyle.Short,
					"TB",
					"-9223372.0 TB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Petabyte,
					DataSizeUnitStyle.Long,
					"PB,long",
					"-9223.4 Petabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Petabyte,
					DataSizeUnitStyle.Short,
					"PB,short",
					"-9223.4 PB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Petabyte,
					DataSizeUnitStyle.Short,
					"PB",
					"-9223.4 PB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Exabyte,
					DataSizeUnitStyle.Long,
					"EB,long",
					"-9.22 Exabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Exabyte,
					DataSizeUnitStyle.Short,
					"EB,short",
					"-9.22 EB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Exabyte,
					DataSizeUnitStyle.Short,
					"EB",
					"-9.22 EB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Kibibyte,
					DataSizeUnitStyle.Long,
					"KiB,long",
					"-9007199254740992.0 Kibibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Kibibyte,
					DataSizeUnitStyle.Short,
					"KiB,short",
					"-9007199254740992.0 KiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Kibibyte,
					DataSizeUnitStyle.Short,
					"KiB",
					"-9007199254740992.0 KiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Mebibyte,
					DataSizeUnitStyle.Long,
					"MiB,long",
					"-8796093022208.0 Mebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Mebibyte,
					DataSizeUnitStyle.Short,
					"MiB,short",
					"-8796093022208.0 MiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Mebibyte,
					DataSizeUnitStyle.Short,
					"MiB",
					"-8796093022208.0 MiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Gibibyte,
					DataSizeUnitStyle.Long,
					"GiB,long",
					"-8589934592.0 Gibibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Gibibyte,
					DataSizeUnitStyle.Short,
					"GiB,short",
					"-8589934592.0 GiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Gibibyte,
					DataSizeUnitStyle.Short,
					"GiB",
					"-8589934592.0 GiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Tebibyte,
					DataSizeUnitStyle.Long,
					"TiB,long",
					"-8388608.0 Tebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Tebibyte,
					DataSizeUnitStyle.Short,
					"TiB,short",
					"-8388608.0 TiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Tebibyte,
					DataSizeUnitStyle.Short,
					"TiB",
					"-8388608.0 TiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Pebibyte,
					DataSizeUnitStyle.Long,
					"PiB,long",
					"-8192.0 Pebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Pebibyte,
					DataSizeUnitStyle.Short,
					"PiB,short",
					"-8192.0 PiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Pebibyte,
					DataSizeUnitStyle.Short,
					"PiB",
					"-8192.0 PiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Exbibyte,
					DataSizeUnitStyle.Long,
					"EiB,long",
					"-8.00 Exbibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Exbibyte,
					DataSizeUnitStyle.Short,
					"EiB,short",
					"-8.00 EiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MinValue,
					DataSizeUnit.Exbibyte,
					DataSizeUnitStyle.Short,
					"EiB",
					"-8.00 EiB"
				};

				//
				// zero, fixed units, different formats
				// (tests the various combinations of metric/binary units with short/long unit styles)
				//

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Byte,
					DataSizeUnitStyle.Long,
					"B,long",
					"0 Bytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Byte,
					DataSizeUnitStyle.Short,
					"B,short",
					"0 B"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Byte,
					DataSizeUnitStyle.Short,
					"B",
					"0 B"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Kilobyte,
					DataSizeUnitStyle.Long,
					"KB,long",
					"0.00 Kilobytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Kilobyte,
					DataSizeUnitStyle.Short,
					"KB,short",
					"0.00 KB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Kilobyte,
					DataSizeUnitStyle.Short,
					"KB",
					"0.00 KB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Megabyte,
					DataSizeUnitStyle.Long,
					"MB,long",
					"0.00 Megabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Megabyte,
					DataSizeUnitStyle.Short,
					"MB,short",
					"0.00 MB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Megabyte,
					DataSizeUnitStyle.Short,
					"MB",
					"0.00 MB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Gigabyte,
					DataSizeUnitStyle.Long,
					"GB,long",
					"0.00 Gigabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Gigabyte,
					DataSizeUnitStyle.Short,
					"GB,short",
					"0.00 GB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Gigabyte,
					DataSizeUnitStyle.Short,
					"GB",
					"0.00 GB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Terabyte,
					DataSizeUnitStyle.Long,
					"TB,long",
					"0.00 Terabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Terabyte,
					DataSizeUnitStyle.Short,
					"TB,short",
					"0.00 TB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Terabyte,
					DataSizeUnitStyle.Short,
					"TB",
					"0.00 TB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Petabyte,
					DataSizeUnitStyle.Long,
					"PB,long",
					"0.00 Petabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Petabyte,
					DataSizeUnitStyle.Short,
					"PB,short",
					"0.00 PB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Petabyte,
					DataSizeUnitStyle.Short,
					"PB",
					"0.00 PB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Exabyte,
					DataSizeUnitStyle.Long,
					"EB,long",
					"0.00 Exabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Exabyte,
					DataSizeUnitStyle.Short,
					"EB,short",
					"0.00 EB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Exabyte,
					DataSizeUnitStyle.Short,
					"EB",
					"0.00 EB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Kibibyte,
					DataSizeUnitStyle.Long,
					"KiB,long",
					"0.00 Kibibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Kibibyte,
					DataSizeUnitStyle.Short,
					"KiB,short",
					"0.00 KiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Kibibyte,
					DataSizeUnitStyle.Short,
					"KiB",
					"0.00 KiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Mebibyte,
					DataSizeUnitStyle.Long,
					"MiB,long",
					"0.00 Mebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Mebibyte,
					DataSizeUnitStyle.Short,
					"MiB,short",
					"0.00 MiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Mebibyte,
					DataSizeUnitStyle.Short,
					"MiB",
					"0.00 MiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Gibibyte,
					DataSizeUnitStyle.Long,
					"GiB,long",
					"0.00 Gibibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Gibibyte,
					DataSizeUnitStyle.Short,
					"GiB,short",
					"0.00 GiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Gibibyte,
					DataSizeUnitStyle.Short,
					"GiB",
					"0.00 GiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Tebibyte,
					DataSizeUnitStyle.Long,
					"TiB,long",
					"0.00 Tebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Tebibyte,
					DataSizeUnitStyle.Short,
					"TiB,short",
					"0.00 TiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Tebibyte,
					DataSizeUnitStyle.Short,
					"TiB",
					"0.00 TiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Pebibyte,
					DataSizeUnitStyle.Long,
					"PiB,long",
					"0.00 Pebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Pebibyte,
					DataSizeUnitStyle.Short,
					"PiB,short",
					"0.00 PiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Pebibyte,
					DataSizeUnitStyle.Short,
					"PiB",
					"0.00 PiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Exbibyte,
					DataSizeUnitStyle.Long,
					"EiB,long",
					"0.00 Exbibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Exbibyte,
					DataSizeUnitStyle.Short,
					"EiB,short",
					"0.00 EiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					0L,
					DataSizeUnit.Exbibyte,
					DataSizeUnitStyle.Short,
					"EiB",
					"0.00 EiB"
				};

				//
				// maximum, fixed units, different formats
				// (tests the various combinations of metric/binary units with short/long unit styles)
				//

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Byte,
					DataSizeUnitStyle.Long,
					"B,long",
					"9223372036854775807 Bytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Byte,
					DataSizeUnitStyle.Short,
					"B,short",
					"9223372036854775807 B"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Byte,
					DataSizeUnitStyle.Short,
					"B",
					"9223372036854775807 B"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Kilobyte,
					DataSizeUnitStyle.Long,
					"KB,long",
					"9223372036854775.8 Kilobytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Kilobyte,
					DataSizeUnitStyle.Short,
					"KB,short",
					"9223372036854775.8 KB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Kilobyte,
					DataSizeUnitStyle.Short,
					"KB",
					"9223372036854775.8 KB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Megabyte,
					DataSizeUnitStyle.Long,
					"MB,long",
					"9223372036854.8 Megabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Megabyte,
					DataSizeUnitStyle.Short,
					"MB,short",
					"9223372036854.8 MB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Megabyte,
					DataSizeUnitStyle.Short,
					"MB",
					"9223372036854.8 MB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Gigabyte,
					DataSizeUnitStyle.Long,
					"GB,long",
					"9223372036.9 Gigabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Gigabyte,
					DataSizeUnitStyle.Short,
					"GB,short",
					"9223372036.9 GB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Gigabyte,
					DataSizeUnitStyle.Short,
					"GB",
					"9223372036.9 GB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Terabyte,
					DataSizeUnitStyle.Long,
					"TB,long",
					"9223372.0 Terabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Terabyte,
					DataSizeUnitStyle.Short,
					"TB,short",
					"9223372.0 TB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Terabyte,
					DataSizeUnitStyle.Short,
					"TB",
					"9223372.0 TB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Petabyte,
					DataSizeUnitStyle.Long,
					"PB,long",
					"9223.4 Petabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Petabyte,
					DataSizeUnitStyle.Short,
					"PB,short",
					"9223.4 PB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Petabyte,
					DataSizeUnitStyle.Short,
					"PB",
					"9223.4 PB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Exabyte,
					DataSizeUnitStyle.Long,
					"EB,long",
					"9.22 Exabytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Exabyte,
					DataSizeUnitStyle.Short,
					"EB,short",
					"9.22 EB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Exabyte,
					DataSizeUnitStyle.Short,
					"EB",
					"9.22 EB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Kibibyte,
					DataSizeUnitStyle.Long,
					"KiB,long",
					"9007199254740992.0 Kibibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Kibibyte,
					DataSizeUnitStyle.Short,
					"KiB,short",
					"9007199254740992.0 KiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Kibibyte,
					DataSizeUnitStyle.Short,
					"KiB",
					"9007199254740992.0 KiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Mebibyte,
					DataSizeUnitStyle.Long,
					"MiB,long",
					"8796093022208.0 Mebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Mebibyte,
					DataSizeUnitStyle.Short,
					"MiB,short",
					"8796093022208.0 MiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Mebibyte,
					DataSizeUnitStyle.Short,
					"MiB",
					"8796093022208.0 MiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Gibibyte,
					DataSizeUnitStyle.Long,
					"GiB,long",
					"8589934592.0 Gibibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Gibibyte,
					DataSizeUnitStyle.Short,
					"GiB,short",
					"8589934592.0 GiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Gibibyte,
					DataSizeUnitStyle.Short,
					"GiB",
					"8589934592.0 GiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Tebibyte,
					DataSizeUnitStyle.Long,
					"TiB,long",
					"8388608.0 Tebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Tebibyte,
					DataSizeUnitStyle.Short,
					"TiB,short",
					"8388608.0 TiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Tebibyte,
					DataSizeUnitStyle.Short,
					"TiB",
					"8388608.0 TiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Pebibyte,
					DataSizeUnitStyle.Long,
					"PiB,long",
					"8192.0 Pebibytes"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Pebibyte,
					DataSizeUnitStyle.Short,
					"PiB,short",
					"8192.0 PiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Pebibyte,
					DataSizeUnitStyle.Short,
					"PiB",
					"8192.0 PiB"
				};

				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Exbibyte,
					DataSizeUnitStyle.Long,
					"EiB,long",
					"8.00 Exbibytes"
				};
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Exbibyte,
					DataSizeUnitStyle.Short,
					"EiB,short",
					"8.00 EiB"
				};
				yield return new object[]
				{
					CultureInfo.InvariantCulture,
					long.MaxValue,
					DataSizeUnit.Exbibyte,
					DataSizeUnitStyle.Short,
					"EiB",
					"8.00 EiB"
				};
			}
		}

		#region string ToString()

		public static IEnumerable<object[]> FormattingTestData_AutoBase10_Short_Only
		{
			get
			{
				foreach (object[] data in FormattingTestData)
				{
					var unit = (DataSizeUnit)data[2];
					var style = (DataSizeUnitStyle)data[3];
					if (unit == DataSizeUnit.AutoBase10 && style == DataSizeUnitStyle.Short)
					{
						yield return new[]
						{
							data[0], // IFormatProvider
							data[1], // long, size in bytes
							data[5]  // string, expected result
						};
					}
				}
			}
		}

		[Theory]
		[MemberData(nameof(FormattingTestData_AutoBase10_Short_Only))]
		public void ToString_WithoutFormat(
			CultureInfo cultureInfo,
			long        sizeInBytes,
			string      expected)
		{
			CultureInfo oldCulture = CultureInfo.CurrentCulture;
			try
			{
				CultureInfo.CurrentCulture = cultureInfo;
				DataSize size = DataSize.FromBytes(sizeInBytes);
				string result = size.ToString();
				Assert.Equal(expected, result);
			}
			finally
			{
				CultureInfo.CurrentCulture = oldCulture;
			}
		}

		#endregion

		#region string ToString(string, IFormatProvider)

		[Theory]
		[MemberData(nameof(FormattingTestData))]
		public void ToString_WithFormat(
			CultureInfo       cultureInfo,
			long              sizeInBytes,
			DataSizeUnit      unit,
			DataSizeUnitStyle unitStyle,
			string            format,
			string            expected)
		{
			DataSize size = DataSize.FromBytes(sizeInBytes);
			string result = size.ToString(format, cultureInfo);
			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("X")]   // invalid unit
		[InlineData("B,X")] // invalid unit style
		public void ToString_WithFormat_FormatNotSupported(string format)
		{
			DataSize size = DataSize.FromBytes(0);
			Assert.Throws<FormatException>(() => size.ToString(format, CultureInfo.InvariantCulture));
		}

		#endregion

		#region string Format(DataSizeUnit, DataSizeUnitStyle)

		[Theory]
		[MemberData(nameof(FormattingTestData))]
		public void Format_WithoutFormatProvider(
			CultureInfo       cultureInfo,
			long              sizeInBytes,
			DataSizeUnit      unit,
			DataSizeUnitStyle unitStyle,
			string            format,
			string            expected)
		{
			CultureInfo oldCulture = CultureInfo.CurrentCulture;
			try
			{
				CultureInfo.CurrentCulture = cultureInfo;
				DataSize size = DataSize.FromBytes(sizeInBytes);
				string result = size.Format(unit, unitStyle);
				Assert.Equal(expected, result);
			}
			finally
			{
				CultureInfo.CurrentCulture = oldCulture;
			}
		}

		#endregion

		#region string Format(IFormatProvider, DataSizeUnit, DataSizeUnitStyle)

		[Theory]
		[MemberData(nameof(FormattingTestData))]
		public void Format_WithFormatProvider(
			CultureInfo       cultureInfo,
			long              sizeInBytes,
			DataSizeUnit      unit,
			DataSizeUnitStyle unitStyle,
			string            format,
			string            expected)
		{
			DataSize size = DataSize.FromBytes(sizeInBytes);
			string result = size.Format(cultureInfo, unit, unitStyle);
			Assert.Equal(expected, result);
		}

		#endregion

		#endregion
	}

}
