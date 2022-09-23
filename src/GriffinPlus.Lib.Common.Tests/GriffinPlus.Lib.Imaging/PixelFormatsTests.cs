///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using Xunit;

namespace GriffinPlus.Lib.Imaging
{

	/// <summary>
	/// Unit tests targeting the <see cref="PixelFormats"/> class.
	/// </summary>
	public class PixelFormatsTests
	{
		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Default"/> property.
		/// </summary>
		[Fact]
		public void Default() => CheckPixelFormat(PixelFormats.Default, 0, false, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Indexed1"/> property.
		/// </summary>
		[Fact]
		public void Indexed1() => CheckPixelFormat(PixelFormats.Indexed1, 1, false, true);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Indexed2"/> property.
		/// </summary>
		[Fact]
		public void Indexed2() => CheckPixelFormat(PixelFormats.Indexed2, 2, false, true);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Indexed4"/> property.
		/// </summary>
		[Fact]
		public void Indexed4() => CheckPixelFormat(PixelFormats.Indexed4, 4, false, true);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Indexed8"/> property.
		/// </summary>
		[Fact]
		public void Indexed8() => CheckPixelFormat(PixelFormats.Indexed8, 8, false, true);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.BlackWhite"/> property.
		/// </summary>
		[Fact]
		public void BlackWhite() => CheckPixelFormat(PixelFormats.BlackWhite, 1, false, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Gray2"/> property.
		/// </summary>
		[Fact]
		public void Gray2() => CheckPixelFormat(PixelFormats.Gray2, 2, false, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Gray4"/> property.
		/// </summary>
		[Fact]
		public void Gray4() => CheckPixelFormat(PixelFormats.Gray4, 4, false, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Gray8"/> property.
		/// </summary>
		[Fact]
		public void Gray8() => CheckPixelFormat(PixelFormats.Gray8, 8, false, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Bgr555"/> property.
		/// </summary>
		[Fact]
		public void Bgr555() => CheckPixelFormat(PixelFormats.Bgr555, 16, false, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Bgr565"/> property.
		/// </summary>
		[Fact]
		public void Bgr565() => CheckPixelFormat(PixelFormats.Bgr565, 16, false, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Rgb128Float"/> property.
		/// </summary>
		[Fact]
		public void Rgb128Float() => CheckPixelFormat(PixelFormats.Rgb128Float, 128, false, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Bgr24"/> property.
		/// </summary>
		[Fact]
		public void Bgr24() => CheckPixelFormat(PixelFormats.Bgr24, 24, false, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Rgb24"/> property.
		/// </summary>
		[Fact]
		public void Rgb24() => CheckPixelFormat(PixelFormats.Rgb24, 24, false, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Bgr101010"/> property.
		/// </summary>
		[Fact]
		public void Bgr101010() => CheckPixelFormat(PixelFormats.Bgr101010, 32, false, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Bgr32"/> property.
		/// </summary>
		[Fact]
		public void Bgr32() => CheckPixelFormat(PixelFormats.Bgr32, 32, false, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Bgra32"/> property.
		/// </summary>
		[Fact]
		public void Bgra32() => CheckPixelFormat(PixelFormats.Bgra32, 32, true, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Pbgra32"/> property.
		/// </summary>
		[Fact]
		public void Pbgra32() => CheckPixelFormat(PixelFormats.Pbgra32, 32, true, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Rgb48"/> property.
		/// </summary>
		[Fact]
		public void Rgb48() => CheckPixelFormat(PixelFormats.Rgb48, 48, false, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Rgba64"/> property.
		/// </summary>
		[Fact]
		public void Rgb64() => CheckPixelFormat(PixelFormats.Rgba64, 64, true, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Prgba64"/> property.
		/// </summary>
		[Fact]
		public void Prgba64() => CheckPixelFormat(PixelFormats.Prgba64, 64, true, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Gray16"/> property.
		/// </summary>
		[Fact]
		public void Gray16() => CheckPixelFormat(PixelFormats.Gray16, 16, false, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Gray32Float"/> property.
		/// </summary>
		[Fact]
		public void Gray32Float() => CheckPixelFormat(PixelFormats.Gray32Float, 32, false, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Rgba128Float"/> property.
		/// </summary>
		[Fact]
		public void Rgba128Float() => CheckPixelFormat(PixelFormats.Rgba128Float, 128, true, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Prgba128Float"/> property.
		/// </summary>
		[Fact]
		public void Prgba128Float() => CheckPixelFormat(PixelFormats.Prgba128Float, 128, true, false);

		/// <summary>
		/// Tests getting the <see cref="PixelFormats.Cmyk32"/> property.
		/// </summary>
		[Fact]
		public void Cmyk32() => CheckPixelFormat(PixelFormats.Cmyk32, 32, false, false);

		/// <summary>
		/// Checks whether the specified <see cref="PixelFormat"/> instance reflects the pixel format appropriately.
		/// </summary>
		/// <param name="pixelFormat"><see cref="PixelFormat"/> instance to check.</param>
		/// <param name="bitsPerPixel">Number of bits per pixel.</param>
		/// <param name="hasAlpha"><c>true</c> if the pixel format supports alpha; otherwise <c>false</c>.</param>
		/// <param name="palettized"><c>true</c> if the pixel format is palettized; otherwise <c>false</c>.</param>
		private static void CheckPixelFormat(
			PixelFormat pixelFormat,
			int         bitsPerPixel,
			bool        hasAlpha,
			bool        palettized)
		{
			Assert.Equal(pixelFormat.BitsPerPixel, bitsPerPixel);
			Assert.Equal(pixelFormat.HasAlpha, hasAlpha);
			Assert.Equal(pixelFormat.Palettized, palettized);
		}
	}

}
