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

using System;

namespace GriffinPlus.Lib.Imaging;

/// <summary>
/// Format of a pixel in a <see cref="NativeBitmap"/>.
/// </summary>
public readonly struct PixelFormat : IEquatable<PixelFormat>
{
	private readonly PixelFormatFlags mFlags;

	/// <summary>
	/// Creates a new <see cref="PixelFormat"/> instance.
	/// </summary>
	/// <param name="format">Enumeration value of the pixel format to create.</param>
	internal PixelFormat(PixelFormatEnum format)
	{
		FormatEnum = format;
		mFlags = GetPixelFormatFlagsFromEnum(format);
		BitsPerPixel = GetBitsPerPixelFromEnum(format);
	}

	/// <summary>
	/// Gets the pixel format with the specified id as returned by <see cref="Id"/>.
	/// </summary>
	/// <param name="id">ID of the pixel format to get.</param>
	/// <returns>The pixel format with the specified id.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The specified <paramref name="id"/> is not a valid pixel format.
	/// </exception>
	public static PixelFormat FromId(int id)
	{
		if (id is < 0 or > (int)PixelFormatEnum.MaxValue)
			throw new ArgumentOutOfRangeException(nameof(id), id, "The specified id is not a valid pixel format.");

		return new PixelFormat((PixelFormatEnum)id);
	}

	/// <summary>
	/// Gets the pixel format enumeration value.
	/// </summary>
	internal PixelFormatEnum FormatEnum { get; }

	/// <summary>
	/// Gets the number of bits-per-pixel (bpp) for this pixel format.
	/// </summary>
	/// <returns>The number of bits-per-pixel (bpp) for this pixel format.</returns>
	public int BitsPerPixel { get; }

	/// <summary>
	/// Gets a value indicating whether the pixel format contains alpha information.
	/// </summary>
	public bool HasAlpha => (mFlags & PixelFormatFlags.ChannelOrderABGR) != PixelFormatFlags.BitsPerPixelUndefined ||
	                        (mFlags & PixelFormatFlags.ChannelOrderARGB) != PixelFormatFlags.BitsPerPixelUndefined ||
	                        (mFlags & PixelFormatFlags.NChannelAlpha) != 0;

	/// <summary>
	/// Gets a value uniquely identifying the pixel format.
	/// </summary>
	public int Id => (int)FormatEnum;

	/// <summary>
	/// Gets a value indicating whether the pixel format is palettized.
	/// </summary>
	internal bool Palettized => (mFlags & PixelFormatFlags.Palettized) != 0;

	#region Comparisons

	/// <summary>
	/// Compares two <see cref="PixelFormat"/> instances for equality.
	/// </summary>
	/// <param name="left">The first <see cref="PixelFormat"/> instance to compare.</param>
	/// <param name="right">The second <see cref="PixelFormat"/> instance to compare.</param>
	/// <returns>
	/// <c>true</c> if the two <see cref="PixelFormat"/> instance are equal;
	/// otherwise <c>false</c>.
	/// </returns>
	public static bool operator ==(PixelFormat left, PixelFormat right)
	{
		return Equals(left, right);
	}

	/// <summary>
	/// Compares two <see cref="PixelFormat"/> instances for inequality.
	/// </summary>
	/// <param name="left">The first <see cref="PixelFormat"/> instance to compare.</param>
	/// <param name="right">The second <see cref="PixelFormat"/> instance to compare.</param>
	/// <returns>
	/// <c>true</c> if the two <see cref="PixelFormat"/> instances are not equal;
	/// otherwise <c>false</c>.
	/// </returns>
	public static bool operator !=(PixelFormat left, PixelFormat right)
	{
		return !Equals(left, right);
	}

	/// <summary>
	/// Determines whether the specified <see cref="PixelFormat"/> instances are considered equal.
	/// </summary>
	/// <param name="pixelFormat1">The first <see cref="PixelFormat"/> instance to compare for equality.</param>
	/// <param name="pixelFormat2">The second <see cref="PixelFormat"/> instance to compare for equality.</param>
	/// <returns>
	/// <c>true</c> if the two <see cref="PixelFormat"/> instances are equal;
	/// otherwise <c>false</c>.
	/// </returns>
	public static bool Equals(PixelFormat pixelFormat1, PixelFormat pixelFormat2)
	{
		return pixelFormat1.FormatEnum == pixelFormat2.FormatEnum;
	}

	/// <summary>
	/// Determines whether the pixel format equals the given <see cref="PixelFormat"/>.
	/// </summary>
	/// <param name="pixelFormat">The pixel format to compare.</param>
	/// <returns>
	/// <c>true</c> if the two <see cref="PixelFormat"/> instances are equal;
	/// otherwise <c>false</c>.
	/// </returns>
	public bool Equals(PixelFormat pixelFormat)
	{
		return Equals(this, pixelFormat);
	}

	/// <summary>
	/// Determines whether the specified object is equal to the current object.
	/// </summary>
	/// <param name="obj">The Object to compare with the current Object.</param>
	/// <returns>
	/// <c>true</c> if the <see cref="PixelFormat"/> is equal to <paramref name="obj"/>;
	/// otherwise <c>false</c>.
	/// </returns>
	public override bool Equals(object obj)
	{
		return obj is PixelFormat pixelFormat && Equals(this, pixelFormat);
	}

	/// <summary>
	/// Gets the hash code of the <see cref="PixelFormat"/> instance.
	/// </summary>
	/// <returns>The pixel format's hash code.</returns>
	public override int GetHashCode()
	{
		return FormatEnum.GetHashCode();
	}

	#endregion

	#region ToString()

	/// <summary>
	/// Gets the string representation of the pixel format.
	/// </summary>
	/// <returns>The string representation of the pixel format.</returns>
	public override string ToString()
	{
		return FormatEnum.ToString();
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Gets the <see cref="PixelFormatFlags"/> for the specified pixel format.
	/// </summary>
	/// <param name="format">Pixel format for which to get the format flags.</param>
	/// <returns>Format flags of the specified pixel format.</returns>
	private static PixelFormatFlags GetPixelFormatFlagsFromEnum(PixelFormatEnum format)
	{
		switch (format)
		{
			case PixelFormatEnum.Indexed1:
				return PixelFormatFlags.BitsPerPixel1 | PixelFormatFlags.Palettized;

			case PixelFormatEnum.Indexed2:
				return PixelFormatFlags.BitsPerPixel2 | PixelFormatFlags.Palettized;

			case PixelFormatEnum.Indexed4:
				return PixelFormatFlags.BitsPerPixel4 | PixelFormatFlags.Palettized;

			case PixelFormatEnum.Indexed8:
				return PixelFormatFlags.BitsPerPixel8 | PixelFormatFlags.Palettized;

			case PixelFormatEnum.BlackWhite:
				return PixelFormatFlags.BitsPerPixel1 | PixelFormatFlags.IsGray;

			case PixelFormatEnum.Gray2:
				return PixelFormatFlags.BitsPerPixel2 | PixelFormatFlags.IsGray;

			case PixelFormatEnum.Gray4:
				return PixelFormatFlags.BitsPerPixel4 | PixelFormatFlags.IsGray;

			case PixelFormatEnum.Gray8:
				return PixelFormatFlags.BitsPerPixel8 | PixelFormatFlags.IsGray;

			case PixelFormatEnum.Bgr555:
				return PixelFormatFlags.BitsPerPixel16 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderBGR;

			case PixelFormatEnum.Bgr565:
				return PixelFormatFlags.BitsPerPixel16 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderBGR;

			case PixelFormatEnum.Gray16:
				return PixelFormatFlags.BitsPerPixel16 | PixelFormatFlags.IsGray | PixelFormatFlags.IsSRGB;

			case PixelFormatEnum.Bgr24:
				return PixelFormatFlags.BitsPerPixel24 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderBGR;

			case PixelFormatEnum.Rgb24:
				return PixelFormatFlags.BitsPerPixel24 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderRGB;

			case PixelFormatEnum.Bgr32:
				return PixelFormatFlags.BitsPerPixel32 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderBGR;

			case PixelFormatEnum.Bgra32:
				return PixelFormatFlags.BitsPerPixel32 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderABGR;

			case PixelFormatEnum.Pbgra32:
				return PixelFormatFlags.BitsPerPixel32 | PixelFormatFlags.IsSRGB | PixelFormatFlags.Premultiplied | PixelFormatFlags.ChannelOrderABGR;

			case PixelFormatEnum.Gray32Float:
				return PixelFormatFlags.BitsPerPixel32 | PixelFormatFlags.IsGray | PixelFormatFlags.IsScRGB;

			case PixelFormatEnum.Bgr101010:
				return PixelFormatFlags.BitsPerPixel32 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderBGR;

			case PixelFormatEnum.Rgb48:
				return PixelFormatFlags.BitsPerPixel48 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderRGB;

			case PixelFormatEnum.Rgba64:
				return PixelFormatFlags.BitsPerPixel64 | PixelFormatFlags.IsSRGB | PixelFormatFlags.ChannelOrderARGB;

			case PixelFormatEnum.Prgba64:
				return PixelFormatFlags.BitsPerPixel64 | PixelFormatFlags.IsSRGB | PixelFormatFlags.Premultiplied | PixelFormatFlags.ChannelOrderARGB;

			case PixelFormatEnum.Rgba128Float:
				return PixelFormatFlags.BitsPerPixel128 | PixelFormatFlags.IsScRGB | PixelFormatFlags.ChannelOrderARGB;

			case PixelFormatEnum.Prgba128Float:
				return PixelFormatFlags.BitsPerPixel128 | PixelFormatFlags.IsScRGB | PixelFormatFlags.Premultiplied | PixelFormatFlags.ChannelOrderARGB;

			case PixelFormatEnum.Rgb128Float:
				return PixelFormatFlags.BitsPerPixel128 | PixelFormatFlags.IsScRGB | PixelFormatFlags.ChannelOrderRGB;

			case PixelFormatEnum.Cmyk32:
				return PixelFormatFlags.BitsPerPixel32 | PixelFormatFlags.IsCMYK;

			case PixelFormatEnum.Default:
			default:
				return PixelFormatFlags.BitsPerPixelUndefined;
		}
	}

	/// <summary>
	/// Gets the number of bits per pixel (bpp) of the specified pixel format.
	/// </summary>
	/// <param name="format">Pixel format for which to get the number of bits per pixel.</param>
	/// <returns>Number of bits per pixel of the specified pixel format.</returns>
	private static int GetBitsPerPixelFromEnum(PixelFormatEnum format)
	{
		switch (format)
		{
			case PixelFormatEnum.Indexed1:
				return 1;

			case PixelFormatEnum.Indexed2:
				return 2;

			case PixelFormatEnum.Indexed4:
				return 4;

			case PixelFormatEnum.Indexed8:
				return 8;

			case PixelFormatEnum.BlackWhite:
				return 1;

			case PixelFormatEnum.Gray2:
				return 2;

			case PixelFormatEnum.Gray4:
				return 4;

			case PixelFormatEnum.Gray8:
				return 8;

			case PixelFormatEnum.Bgr555:
			case PixelFormatEnum.Bgr565:
				return 16;

			case PixelFormatEnum.Gray16:
				return 16;

			case PixelFormatEnum.Bgr24:
			case PixelFormatEnum.Rgb24:
				return 24;

			case PixelFormatEnum.Bgr32:
			case PixelFormatEnum.Bgra32:
			case PixelFormatEnum.Pbgra32:
				return 32;

			case PixelFormatEnum.Gray32Float:
				return 32;

			case PixelFormatEnum.Bgr101010:
				return 32;

			case PixelFormatEnum.Rgb48:
				return 48;

			case PixelFormatEnum.Rgba64:
			case PixelFormatEnum.Prgba64:
				return 64;

			case PixelFormatEnum.Rgba128Float:
			case PixelFormatEnum.Prgba128Float:
			case PixelFormatEnum.Rgb128Float:
				return 128;

			case PixelFormatEnum.Cmyk32:
				return 32;

			case PixelFormatEnum.Default:
			default:
				return 0;
		}
	}

	#endregion
}
