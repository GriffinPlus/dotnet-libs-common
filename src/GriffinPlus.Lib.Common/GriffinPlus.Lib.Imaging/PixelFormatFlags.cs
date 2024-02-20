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

// ReSharper disable InconsistentNaming

namespace GriffinPlus.Lib.Imaging;

[Flags]
enum PixelFormatFlags
{
	BitsPerPixelMask      = 0x000000FF,
	BitsPerPixelUndefined = 0x00000000,
	BitsPerPixel1         = 0x00000001,
	BitsPerPixel2         = 0x00000002,
	BitsPerPixel4         = 0x00000004,
	BitsPerPixel8         = 0x00000008,
	BitsPerPixel16        = 0x00000010,
	BitsPerPixel24        = 0x00000018,
	BitsPerPixel32        = 0x00000020,
	BitsPerPixel48        = 0x00000030,
	BitsPerPixel64        = 0x00000040,
	BitsPerPixel96        = 0x00000060,
	BitsPerPixel128       = 0x00000080,
	IsGray                = 0x00000100,
	IsCMYK                = 0x00000200,
	IsSRGB                = 0x00000400,
	IsScRGB               = 0x00000800,
	Premultiplied         = 0x00001000,
	ChannelOrderMask      = 0x0001E000,
	ChannelOrderRGB       = 0x00002000,
	ChannelOrderBGR       = 0x00004000,
	ChannelOrderARGB      = 0x00008000,
	ChannelOrderABGR      = 0x00010000,
	Palettized            = 0x00020000,
	NChannelAlpha         = 0x00040000,
	IsNChannel            = 0x00080000
}
