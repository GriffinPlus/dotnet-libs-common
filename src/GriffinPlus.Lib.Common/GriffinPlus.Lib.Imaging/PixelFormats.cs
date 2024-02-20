///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
//
// This file incorporates work covered by the following copyright and permission notice:
//
//   Licensed to the .NET Foundation under one or more agreements.
//   The .NET Foundation licenses this file to you under the MIT license.
//   See the LICENSE file in the project root for more information.
//
//   Project: https://github.com/dotnet/wpf
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Imaging;

/// <summary>
/// Collection of supported pixel formats.
/// </summary>
public static class PixelFormats
{
	/// <summary>
	/// Gets the pixel format that is best suited for the particular operation.
	/// </summary>
	/// <returns>
	/// The <see cref="PixelFormat"/> best suited for the particular operation.
	/// </returns>
	/// <exception cref="T:System.NotSupportedException">
	/// The <see cref="PixelFormat"/> properties are accessed.
	/// </exception>
	public static PixelFormat Default => new(PixelFormatEnum.Default);

	/// <summary>
	/// Gets the pixel format specifying a paletted bitmap with 2 colors.
	/// </summary>
	/// <returns>
	/// The pixel format which specifying a paletted bitmap with 2 colors.
	/// </returns>
	public static PixelFormat Indexed1 => new(PixelFormatEnum.Indexed1);

	/// <summary>
	/// Gets the pixel format specifying a paletted bitmap with 4 colors.
	/// </summary>
	/// <returns>
	/// The pixel format which specifying a paletted bitmap with 4 colors.
	/// </returns>
	public static PixelFormat Indexed2 => new(PixelFormatEnum.Indexed2);

	/// <summary>
	/// Gets the pixel format specifying a paletted bitmap with 16 colors.
	/// </summary>
	/// <returns>
	/// The pixel format which specifying a paletted bitmap with 16 colors.
	/// </returns>
	public static PixelFormat Indexed4 => new(PixelFormatEnum.Indexed4);

	/// <summary>
	/// Gets the pixel format specifying a paletted bitmap with 256 colors.
	/// </summary>
	/// <returns>
	/// The pixel format which specifying a paletted bitmap with 256 colors.
	/// </returns>
	public static PixelFormat Indexed8 => new(PixelFormatEnum.Indexed8);

	/// <summary>
	/// Gets the black and white pixel format which displays one bit of data per pixel as either black or white.
	/// </summary>
	/// <returns>
	/// The pixel format Black-and-White.
	/// </returns>
	public static PixelFormat BlackWhite => new(PixelFormatEnum.BlackWhite);

	/// <summary>
	/// Gets the <see cref="Gray2"/> pixel format which displays a 2 bits-per-pixel grayscale channel,
	/// allowing 4 shades of gray.
	/// </summary>
	/// <returns>
	/// The <see cref="Gray2"/> pixel format.
	/// </returns>
	public static PixelFormat Gray2 => new(PixelFormatEnum.Gray2);

	/// <summary>
	/// Gets the <see cref="Gray4"/> pixel format which displays a 4 bits-per-pixel grayscale channel,
	/// allowing 16 shades of gray.
	/// </summary>
	/// <returns>
	/// The <see cref="Gray4"/> pixel format.
	/// </returns>
	public static PixelFormat Gray4 => new(PixelFormatEnum.Gray4);

	/// <summary>
	/// Gets the <see cref="Gray8"/> pixel format which displays an 8 bits-per-pixel grayscale channel,
	/// allowing 256 shades of gray.
	/// </summary>
	/// <returns>
	/// The <see cref="Gray8"/> pixel format.
	/// </returns>
	public static PixelFormat Gray8 => new(PixelFormatEnum.Gray8);

	/// <summary>
	/// Gets the <see cref="Bgr555"/> pixel format.
	/// <see cref="Bgr555"/> is a sRGB format with 16 bits per pixel (BPP).
	/// Each color channel (blue, green and red) is allocated 5 bits per pixel (BPP).
	/// </summary>
	/// <returns>
	/// The <see cref="Bgr555"/> pixel format.
	/// </returns>
	public static PixelFormat Bgr555 => new(PixelFormatEnum.Bgr555);

	/// <summary>
	/// Gets the <see cref="Bgr565"/> pixel format.
	/// <see cref="Bgr565"/> is a sRGB format with 16 bits per pixel (BPP).
	/// Each color channel (blue, green and red) is allocated 5, 6, and 5 bits per pixel (BPP) respectively.
	/// </summary>
	/// <returns>
	/// The <see cref="Bgr565"/> pixel format.
	/// </returns>
	public static PixelFormat Bgr565 => new(PixelFormatEnum.Bgr565);

	/// <summary>
	/// Gets the <see cref="Rgb128Float"/> pixel format.
	/// <see cref="Rgb128Float"/> is a ScRGB format with 128 bits per pixel (BPP).
	/// Each color channel is allocated 32 BPP.
	/// This format has a gamma of 1.0.
	/// </summary>
	/// <returns>
	/// The <see cref="Rgb128Float"/> pixel format.
	/// </returns>
	public static PixelFormat Rgb128Float => new(PixelFormatEnum.Rgb128Float);

	/// <summary>
	/// Gets the <see cref="Bgr24"/> pixel format.
	/// <see cref="Bgr24"/> is a sRGB format with 24 bits per pixel (BPP).
	/// Each color channel (blue, green and red) is allocated 8 bits per pixel (BPP).
	/// </summary>
	/// <returns>
	/// The <see cref="Bgr24"/> pixel format.
	/// </returns>
	public static PixelFormat Bgr24 => new(PixelFormatEnum.Bgr24);

	/// <summary>
	/// Gets the <see cref="Rgb24"/> pixel format.
	/// <see cref="Rgb24"/> is a sRGB format with 24 bits per pixel (BPP).
	/// Each color channel (red, green and blue) is allocated 8 bits per pixel (BPP).
	/// </summary>
	/// <returns>
	/// The <see cref="Rgb24"/> pixel format.
	/// </returns>
	public static PixelFormat Rgb24 => new(PixelFormatEnum.Rgb24);

	/// <summary>
	/// Gets the <see cref="Bgr101010"/> pixel format.
	/// <see cref="Bgr101010"/> is a sRGB format with 32 bits per pixel (BPP).
	/// Each color channel (blue, green and red) is allocated 10 bits per pixel (BPP).
	/// </summary>
	/// <returns>
	/// The <see cref="Bgr101010"/> pixel format.
	/// </returns>
	public static PixelFormat Bgr101010 => new(PixelFormatEnum.Bgr101010);

	/// <summary>
	/// Gets the <see cref="Bgr32"/> pixel format.
	/// <see cref="Bgr32"/> is a sRGB format with 32 bits per pixel (BPP).
	/// Each color channel (blue, green and red) is allocated 8 bits per pixel (BPP).
	/// </summary>
	/// <returns>
	/// The <see cref="Bgr32"/> pixel format.
	/// </returns>
	public static PixelFormat Bgr32 => new(PixelFormatEnum.Bgr32);

	/// <summary>
	/// Gets the <see cref="Bgra32"/> pixel format.
	/// <see cref="Bgra32"/> is a sRGB format with 32 bits per pixel (BPP).
	/// Each channel (blue, green, red and alpha) is allocated 8 bits per pixel (BPP).
	/// </summary>
	/// <returns>
	/// The <see cref="Bgra32"/> pixel format.
	/// </returns>
	public static PixelFormat Bgra32 => new(PixelFormatEnum.Bgra32);

	/// <summary>
	/// Gets the <see cref="Pbgra32"/> pixel format.
	/// <see cref="Pbgra32"/> is a sRGB format with 32 bits per pixel (BPP).
	/// Each channel (blue, green, red and alpha) is allocated 8 bits per pixel (BPP).
	/// Each color channel is pre-multiplied by the alpha value.
	/// </summary>
	/// <returns>
	/// The <see cref="Pbgra32"/> pixel format.
	/// </returns>
	public static PixelFormat Pbgra32 => new(PixelFormatEnum.Pbgra32);

	/// <summary>
	/// Gets the <see cref="Rgb48"/> pixel format.
	/// <see cref="Rgb48"/> is a sRGB format with 48 bits per pixel (BPP).
	/// Each color channel (red, green and blue) is allocated 16 bits per pixel (BPP).
	/// This format has a gamma of 1.0.
	/// </summary>
	/// <returns>
	/// The <see cref="Rgb48"/> pixel format.
	/// </returns>
	public static PixelFormat Rgb48 => new(PixelFormatEnum.Rgb48);

	/// <summary>
	/// Gets the <see cref="Rgba64"/> pixel format.
	/// <see cref="Rgba64"/> is an sRGB format with 64 bits per pixel (BPP).
	/// Each channel (red, green, blue and alpha) is allocated 16 bits per pixel (BPP).
	/// This format has a gamma of 1.0.
	/// </summary>
	/// <returns>
	/// The <see cref="Rgba64"/> pixel format.
	/// </returns>
	public static PixelFormat Rgba64 => new(PixelFormatEnum.Rgba64);

	/// <summary>
	/// Gets the <see cref="Prgba64"/> pixel format.
	/// <see cref="Prgba64"/> is a sRGB format with 64 bits per pixel (BPP).
	/// Each channel (blue, green, red and alpha) is allocated 32 bits per pixel (BPP).
	/// Each color channel is pre-multiplied by the alpha value.
	/// This format has a gamma of 1.0.
	/// </summary>
	/// <returns>
	/// The <see cref="Prgba64"/> pixel format.
	/// </returns>
	public static PixelFormat Prgba64 => new(PixelFormatEnum.Prgba64);

	/// <summary>
	/// Gets the <see cref="Gray16"/> pixel format which displays a 16 bits-per-pixel grayscale
	/// channel, allowing 65536 shades of gray. This format has a gamma of 1.0.
	/// </summary>
	/// <returns>
	/// The <see cref="Gray16"/> pixel format.
	/// </returns>
	public static PixelFormat Gray16 => new(PixelFormatEnum.Gray16);

	/// <summary>
	/// Gets the <see cref="Gray32Float"/> pixel format.
	/// <see cref="Gray32Float"/> displays a 32 bits per pixel (BPP) grayscale channel,
	/// allowing over 4 billion shades of gray. This format has a gamma of 1.0.
	/// </summary>
	/// <returns>
	/// The <see cref="Gray32Float"/> pixel format.
	/// </returns>
	public static PixelFormat Gray32Float => new(PixelFormatEnum.Gray32Float);

	/// <summary>
	/// Gets the <see cref="Rgba128Float"/> pixel format.
	/// <see cref="Rgba128Float"/> is a ScRGB format with 128 bits per pixel (BPP).
	/// Each color channel is allocated 32 bits per pixel (BPP).
	/// This format has a gamma of 1.0.
	/// </summary>
	/// <returns>
	/// The <see cref="Rgba128Float"/> pixel format.
	/// </returns>
	public static PixelFormat Rgba128Float => new(PixelFormatEnum.Rgba128Float);

	/// <summary>
	/// Gets the <see cref="Prgba128Float"/> pixel format.
	/// <see cref="Prgba128Float"/> is a ScRGB format with 128 bits per pixel (BPP).
	/// Each channel (red, green, blue and alpha) is allocated 32 bits per pixel (BPP).
	/// Each color channel is pre-multiplied by the alpha value.
	/// This format has a gamma of 1.0.
	/// </summary>
	/// <returns>
	/// The <see cref="Prgba128Float"/> pixel format.
	/// </returns>
	public static PixelFormat Prgba128Float => new(PixelFormatEnum.Prgba128Float);

	/// <summary>
	/// Gets the <see cref="Cmyk32"/> pixel format which displays 32 bits per pixel (BPP)
	/// with each color channel (cyan, magenta, yellow and black) allocated 8 bits per pixel (BPP).
	/// </summary>
	/// <returns>
	/// The CMYK32 pixel format.
	/// </returns>
	public static PixelFormat Cmyk32 => new(PixelFormatEnum.Cmyk32);
}
