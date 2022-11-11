///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Xunit;

namespace GriffinPlus.Lib.Imaging
{

	/// <summary>
	/// Unit tests targeting the <see cref="PixelFormat"/> class.
	/// </summary>
	public class PixelFormatTests
	{
		#region Test Data

		/// <summary>
		/// Pixel format test data.
		/// </summary>
		public static IEnumerable<object[]> TestData_AllPixelFormats
		{
			get
			{
				PixelFormat[] formats =
				{
					PixelFormats.Default,
					PixelFormats.Indexed1,
					PixelFormats.Indexed2,
					PixelFormats.Indexed4,
					PixelFormats.Indexed8,
					PixelFormats.BlackWhite,
					PixelFormats.Gray2,
					PixelFormats.Gray4,
					PixelFormats.Gray8,
					PixelFormats.Bgr555,
					PixelFormats.Bgr565,
					PixelFormats.Gray16,
					PixelFormats.Bgr24,
					PixelFormats.Rgb24,
					PixelFormats.Bgr32,
					PixelFormats.Bgra32,
					PixelFormats.Pbgra32,
					PixelFormats.Gray32Float,
					PixelFormats.Bgr101010,
					PixelFormats.Rgb48,
					PixelFormats.Rgba64,
					PixelFormats.Prgba64,
					PixelFormats.Rgba128Float,
					PixelFormats.Prgba128Float,
					PixelFormats.Rgb128Float,
					PixelFormats.Cmyk32
				};

				foreach (PixelFormat format in formats)
				{
					yield return new object[] { format };
				}
			}
		}

		/// <summary>
		/// Pixel format test data.
		/// </summary>
		public static IEnumerable<object[]> TestData_Equals
		{
			get
			{
				PixelFormat[] formats =
				{
					//PixelFormats.Default,
					//PixelFormats.Indexed1,
					//PixelFormats.Indexed2,
					//PixelFormats.Indexed4,
					//PixelFormats.Indexed8,
					//PixelFormats.BlackWhite,
					//PixelFormats.Gray2,
					//PixelFormats.Gray4,
					//PixelFormats.Gray8,
					//PixelFormats.Bgr555,
					//PixelFormats.Bgr565,
					//PixelFormats.Gray16,
					//PixelFormats.Bgr24,
					//PixelFormats.Rgb24,
					//PixelFormats.Bgr32,
					//PixelFormats.Bgra32,
					//PixelFormats.Pbgra32,
					//PixelFormats.Gray32Float,
					//PixelFormats.Bgr101010,
					//PixelFormats.Rgb48,
					//PixelFormats.Rgba64,
					//PixelFormats.Prgba64,
					//PixelFormats.Rgba128Float,
					//PixelFormats.Prgba128Float,
					PixelFormats.Rgb128Float,
					PixelFormats.Cmyk32
				};

				foreach (PixelFormat format1 in formats)
				foreach (PixelFormat format2 in formats)
				{
					yield return new object[]
					{
						format1,
						format2,
						format1.FormatEnum == format2.FormatEnum
					};
				}
			}
		}

		#endregion

		#region bool Equals(PixelFormat format1, PixelFormat format2)

		/// <summary>
		/// Tests the <see cref="PixelFormat.Equals(PixelFormat,PixelFormat)"/> method.
		/// </summary>
		/// <param name="format1">The first pixel format to test.</param>
		/// <param name="format2">The second pixel format to test.</param>
		/// <param name="areEqual">
		/// <c>true</c> if <paramref name="format1"/> and <paramref name="format2"/> are expected to be equal;
		/// otherwise <c>false</c>.
		/// </param>
		[Theory]
		[MemberData(nameof(TestData_Equals))]
		public void Equals_Static(PixelFormat format1, PixelFormat format2, bool areEqual)
		{
			Assert.Equal(areEqual, PixelFormat.Equals(format1, format2));
		}

		#endregion

		#region bool Equals(object obj)

		/// <summary>
		/// Tests the <see cref="PixelFormat.Equals(object)"/> method.
		/// </summary>
		/// <param name="format1">The first pixel format to test.</param>
		/// <param name="format2">The second pixel format to test.</param>
		/// <param name="areEqual">
		/// <c>true</c> if <paramref name="format1"/> and <paramref name="format2"/> are expected to be equal;
		/// otherwise <c>false</c>.
		/// </param>
		[Theory]
		[MemberData(nameof(TestData_Equals))]
		public void Equals_Object(PixelFormat format1, PixelFormat format2, bool areEqual)
		{
			Assert.Equal(areEqual, format1.Equals((object)format2));
		}

		#endregion

		#region bool Equals(PixelFormat pixelFormat)

		/// <summary>
		/// Tests the <see cref="PixelFormat.Equals(PixelFormat)"/> method.
		/// </summary>
		/// <param name="format1">The first pixel format to test.</param>
		/// <param name="format2">The second pixel format to test.</param>
		/// <param name="areEqual">
		/// <c>true</c> if <paramref name="format1"/> and <paramref name="format2"/> are expected to be equal;
		/// otherwise <c>false</c>.
		/// </param>
		[Theory]
		[MemberData(nameof(TestData_Equals))]
		public void Equals_PixelFormat(PixelFormat format1, PixelFormat format2, bool areEqual)
		{
			Assert.Equal(areEqual, format1.Equals(format2));
		}

		#endregion

		#region bool operator== (PixelFormat left, PixelFormat right)

		/// <summary>
		/// Tests the <see cref="PixelFormat.operator==(PixelFormat,PixelFormat)"/> method.
		/// </summary>
		/// <param name="format1">The first pixel format to test.</param>
		/// <param name="format2">The second pixel format to test.</param>
		/// <param name="areEqual">
		/// <c>true</c> if <paramref name="format1"/> and <paramref name="format2"/> are expected to be equal;
		/// otherwise <c>false</c>.
		/// </param>
		[Theory]
		[MemberData(nameof(TestData_Equals))]
		public void EqualityOperator(PixelFormat format1, PixelFormat format2, bool areEqual)
		{
			Assert.Equal(areEqual, format1 == format2);
		}

		#endregion

		#region bool operator!= (PixelFormat left, PixelFormat right)

		/// <summary>
		/// Tests the <see cref="PixelFormat.operator!=(PixelFormat,PixelFormat)"/> method.
		/// </summary>
		/// <param name="format1">The first pixel format to test.</param>
		/// <param name="format2">The second pixel format to test.</param>
		/// <param name="areEqual">
		/// <c>true</c> if <paramref name="format1"/> and <paramref name="format2"/> are expected to be equal;
		/// otherwise <c>false</c>.
		/// </param>
		[Theory]
		[MemberData(nameof(TestData_Equals))]
		public void InequalityOperator(PixelFormat format1, PixelFormat format2, bool areEqual)
		{
			Assert.Equal(!areEqual, format1 != format2);
		}

		#endregion

		#region int GetHashCode()

		/// <summary>
		/// Tests the <see cref="PixelFormat.GetHashCode"/> method.
		/// </summary>
		/// <param name="format1">The first pixel format to test.</param>
		/// <param name="format2">The second pixel format to test.</param>
		/// <param name="areEqual">
		/// <c>true</c> if <paramref name="format1"/> and <paramref name="format2"/> are expected to be equal;
		/// otherwise <c>false</c>.
		/// </param>
		[Theory]
		[MemberData(nameof(TestData_Equals))]
		public void GetHashCode_(PixelFormat format1, PixelFormat format2, bool areEqual)
		{
			// different pixel formats should return a different hash code
			// (theoretically there can be collisions, but these should be rare, especially in the small test data set)
			Assert.Equal(areEqual, format1.GetHashCode() == format2.GetHashCode());
		}

		#endregion

		#region string ToString()

		/// <summary>
		/// Tests the <see cref="PixelFormat.ToString"/> method.
		/// </summary>
		/// <param name="format">Pixel format to test.</param>
		[Theory]
		[MemberData(nameof(TestData_AllPixelFormats))]
		public void ToString_(PixelFormat format)
		{
			string expected = format.FormatEnum.ToString();
			Assert.Equal(expected, format.ToString());
		}

		#endregion
	}

}
