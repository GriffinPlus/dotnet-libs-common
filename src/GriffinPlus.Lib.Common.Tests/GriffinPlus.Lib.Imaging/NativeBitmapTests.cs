///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Xunit;

namespace GriffinPlus.Lib.Imaging
{

	/// <summary>
	/// Unit tests targeting the <see cref="NativeBitmap"/> class.
	/// </summary>
	public unsafe class NativeBitmapTests
	{
		#region Test Data

		/// <summary>
		/// Test data for tests targeting <see cref="NativeBitmap"/> constructors.
		/// </summary>
		public static IEnumerable<object[]> TestData_Create
		{
			get
			{
				// pixel formats to test with
				// (contains only formats that require different handling)
				PixelFormat[] formats =
				{
					PixelFormats.Indexed1,    // palettized, 1 bpp
					PixelFormats.Indexed2,    // palettized, 2 bpp
					PixelFormats.Indexed4,    // palettized, 4 bpp
					PixelFormats.Indexed8,    // palettized, 8 bpp
					PixelFormats.BlackWhite,  // not palettized, 1 bpp
					PixelFormats.Gray2,       // not palettized, 2 bpp
					PixelFormats.Gray4,       // not palettized, 4 bpp
					PixelFormats.Gray8,       // not palettized, 8 bpp
					PixelFormats.Gray16,      // not palettized, 16 bpp
					PixelFormats.Bgr24,       // not palettized, 24 bpp
					PixelFormats.Bgra32,      // not palettized, 32 bpp
					PixelFormats.Rgb48,       // not palettized, 48 bpp
					PixelFormats.Rgba64,      // not palettized, 64 bpp
					PixelFormats.Rgba128Float // not palettized, 128 bpp
				};

				// use the web palette, if needed as it is compatible with all indexed formats
				BitmapPalette palette = BitmapPalettes.WebPalette;

				foreach (PixelFormat format in formats)
				{
					// note:
					// the width is critical in conjunction with pixel formats with < 8 bits per pixel as shifting kicks in
					// the height is not that problematic, so testing with only one height is sufficient
					const int height = 100;
					const double dpiX = 100.0;
					const double dpiY = 200.0;

					int widthCount = format.BitsPerPixel % 8 != 0 ? 8 / format.BitsPerPixel % 8 : 1;
					for (int i = 0; i < widthCount; i++)
					{
						int width = 100 + i;

						CalculateBufferAlignmentAndStride(format, width, out _, out _);

						// use the same stride for the bitmap to copy as for the copy
						yield return new object[]
						{
							width,                             // width (in pixels)
							height,                            // height (in pixels)
							dpiX,                              // dpiX
							dpiY,                              // dpiY
							format,                            // pixel format
							format.Palettized ? palette : null // bitmap palette, if pixel format needs one
						};

						// increment the stride of bitmap to copy by one byte
						// => destroys the alignment of rows, but this should work as well
						yield return new object[]
						{
							width,                             // width (in pixels)
							height,                            // height (in pixels)
							dpiX,                              // dpiX
							dpiY,                              // dpiY
							format,                            // pixel format
							format.Palettized ? palette : null // bitmap palette, if pixel format needs one
						};
					}
				}
			}
		}

		/// <summary>
		/// Test data for tests targeting <see cref="NativeBitmap"/> copy-constructors.
		/// </summary>
		public static IEnumerable<object[]> TestData_CreateCopy
		{
			get
			{
				// pixel formats to test with
				// (contains only formats that require different handling)
				PixelFormat[] formats =
				{
					PixelFormats.Indexed1,    // palettized, 1 bpp
					PixelFormats.Indexed2,    // palettized, 2 bpp
					PixelFormats.Indexed4,    // palettized, 4 bpp
					PixelFormats.Indexed8,    // palettized, 8 bpp
					PixelFormats.BlackWhite,  // not palettized, 1 bpp
					PixelFormats.Gray2,       // not palettized, 2 bpp
					PixelFormats.Gray4,       // not palettized, 4 bpp
					PixelFormats.Gray8,       // not palettized, 8 bpp
					PixelFormats.Gray16,      // not palettized, 16 bpp
					PixelFormats.Bgr24,       // not palettized, 24 bpp
					PixelFormats.Bgra32,      // not palettized, 32 bpp
					PixelFormats.Rgb48,       // not palettized, 48 bpp
					PixelFormats.Rgba64,      // not palettized, 64 bpp
					PixelFormats.Rgba128Float // not palettized, 128 bpp
				};

				// use the web palette, if needed as it is compatible with all indexed formats
				BitmapPalette palette = BitmapPalettes.WebPalette;

				foreach (PixelFormat format in formats)
				{
					// note:
					// the width is critical in conjunction with pixel formats with < 8 bits per pixel as shifting kicks in
					// the height is not that problematic, so testing with only one height is sufficient
					const int height = 100;
					const double dpiX = 100.0;
					const double dpiY = 200.0;

					int widthCount = format.BitsPerPixel % 8 != 0 ? 8 / format.BitsPerPixel % 8 : 1;
					for (int i = 0; i < widthCount; i++)
					{
						int width = 100 + i;

						CalculateBufferAlignmentAndStride(format, width, out _, out long stride);

						// use the same stride for the bitmap to copy as for the copy
						yield return new object[]
						{
							width,                             // width (in pixels)
							height,                            // height (in pixels)
							stride,                            // stride (in bytes)
							dpiX,                              // dpiX
							dpiY,                              // dpiY
							format,                            // pixel format
							format.Palettized ? palette : null // bitmap palette, if pixel format needs one
						};

						// increment the stride of bitmap to copy by one byte
						// => destroys the alignment of rows, but this should work as well
						yield return new object[]
						{
							width,                             // width (in pixels)
							height,                            // height (in pixels)
							stride + 1,                        // stride (in bytes)
							dpiX,                              // dpiX
							dpiY,                              // dpiY
							format,                            // pixel format
							format.Palettized ? palette : null // bitmap palette, if pixel format needs one
						};
					}
				}
			}
		}

		/// <summary>
		/// Test data for tests targeting <see cref="NativeBitmap.CopyPixels(Array,int,long,int,int,int,int)"/>.
		/// </summary>
		public static IEnumerable<object[]> TestData_CopyPixelsToBufferWithSelection
		{
			get
			{
				// test non-palettized pixel formats only
				// (the bitmap palette does not have any effect on copying)
				PixelFormat[] formats =
				{
					//PixelFormats.Indexed1,    // palettized, 1 bpp
					//PixelFormats.Indexed2,    // palettized, 2 bpp
					//PixelFormats.Indexed4,    // palettized, 4 bpp
					//PixelFormats.Indexed8,    // palettized, 8 bpp
					PixelFormats.BlackWhite,  // not palettized, 1 bpp
					PixelFormats.Gray2,       // not palettized, 2 bpp
					PixelFormats.Gray4,       // not palettized, 4 bpp
					PixelFormats.Gray8,       // not palettized, 8 bpp
					PixelFormats.Gray16,      // not palettized, 16 bpp
					PixelFormats.Bgr24,       // not palettized, 24 bpp
					PixelFormats.Bgra32,      // not palettized, 32 bpp
					PixelFormats.Rgb48,       // not palettized, 48 bpp
					PixelFormats.Rgba64,      // not palettized, 64 bpp
					PixelFormats.Rgba128Float // not palettized, 128 bpp
				};

				Type[] bufferTypes =
				{
					typeof(sbyte),
					typeof(byte),
					typeof(short),
					typeof(ushort),
					typeof(int),
					typeof(uint),
					typeof(long),
					typeof(ulong),
					typeof(float),
					typeof(double)
				};

				BitmapPalette palette = BitmapPalettes.WebPalette;

				foreach (PixelFormat format in formats)
				foreach (Type bufferType in bufferTypes)
				{
					// note:
					// the width is critical in conjunction with pixel formats with < 8 bits per pixel as shifting kicks in
					// the height is not that problematic, so testing with only one height is sufficient
					const int height = 100;
					const double dpiX = 100.0;
					const double dpiY = 200.0;

					int widthCount = format.BitsPerPixel % 8 != 0 ? 8 / format.BitsPerPixel % 8 : 1;
					for (int i = 0; i < widthCount; i++)
					{
						int width = 100 + i;

						// entire bitmap
						// (selection width/height is zero => default to bitmap width/height)
						yield return new object[]
						{
							width,                              // width (in pixels)
							height,                             // height (in pixels)
							dpiX,                               // dpiX
							dpiY,                               // dpiY
							format,                             // pixel format
							format.Palettized ? palette : null, // bitmap palette, if pixel format needs one
							bufferType,                         // type of the destination buffer
							0,                                  // x-coordinate of the selection (in pixels)
							0,                                  // y-coordinate of the selection (in pixels)
							0,                                  // width of the selection (in pixels)
							0                                   // height of the selection (in pixels)
						};

						// real selections
						int selectionCount = format.BitsPerPixel % 8 != 0 ? 8 / format.BitsPerPixel % 8 : 1;
						for (int selectionX = 0; selectionX < selectionCount; selectionX++)
						for (int selectionY = 0; selectionY < selectionCount; selectionY++)
						{
							yield return new object[]
							{
								width,                              // width (in pixels)
								height,                             // height (in pixels)
								dpiX,                               // dpiX
								dpiY,                               // dpiY
								format,                             // pixel format
								format.Palettized ? palette : null, // bitmap palette, if pixel format needs one
								bufferType,                         // type of the destination buffer
								selectionX,                         // x-coordinate of the selection (in pixels)
								selectionY,                         // y-coordinate of the selection (in pixels)
								width - 2 * selectionX,             // width of the selection (in pixels)
								height - 2 * selectionY             // height of the selection (in pixels)
							};
						}
					}
				}
			}
		}

		/// <summary>
		/// Test data for tests targeting <see cref="NativeBitmap.CopyPixels(IntPtr,long,long,long,long,long,long)"/>.
		/// </summary>
		public static IEnumerable<object[]> TestData_CopyPixelsToPointer
		{
			get
			{
				// test non-palettized pixel formats only
				// (the bitmap palette does not have any effect on copying)
				PixelFormat[] formats =
				{
					//PixelFormats.Indexed1,    // palettized, 1 bpp
					//PixelFormats.Indexed2,    // palettized, 2 bpp
					//PixelFormats.Indexed4,    // palettized, 4 bpp
					//PixelFormats.Indexed8,    // palettized, 8 bpp
					PixelFormats.BlackWhite,  // not palettized, 1 bpp
					PixelFormats.Gray2,       // not palettized, 2 bpp
					PixelFormats.Gray4,       // not palettized, 4 bpp
					PixelFormats.Gray8,       // not palettized, 8 bpp
					PixelFormats.Gray16,      // not palettized, 16 bpp
					PixelFormats.Bgr24,       // not palettized, 24 bpp
					PixelFormats.Bgra32,      // not palettized, 32 bpp
					PixelFormats.Rgb48,       // not palettized, 48 bpp
					PixelFormats.Rgba64,      // not palettized, 64 bpp
					PixelFormats.Rgba128Float // not palettized, 128 bpp
				};

				BitmapPalette palette = BitmapPalettes.WebPalette;

				foreach (PixelFormat format in formats)
				{
					// note:
					// the width is critical in conjunction with pixel formats with < 8 bits per pixel as shifting kicks in
					// the height is not that problematic, so testing with only one height is sufficient
					const int height = 100;
					const double dpiX = 100.0;
					const double dpiY = 200.0;

					int widthCount = format.BitsPerPixel % 8 != 0 ? 8 / format.BitsPerPixel % 8 : 1;
					for (int i = 0; i < widthCount; i++)
					{
						int width = 100 + i;

						// entire bitmap
						// (selection width/height is zero => default to bitmap width/height)
						yield return new object[]
						{
							width,                              // width (in pixels)
							height,                             // height (in pixels)
							dpiX,                               // dpiX
							dpiY,                               // dpiY
							format,                             // pixel format
							format.Palettized ? palette : null, // bitmap palette, if pixel format needs one
							0,                                  // x-coordinate of the selection (in pixels)
							0,                                  // y-coordinate of the selection (in pixels)
							0,                                  // width of the selection (in pixels)
							0                                   // height of the selection (in pixels)
						};

						// real selections
						int selectionCount = format.BitsPerPixel % 8 != 0 ? 8 / format.BitsPerPixel % 8 : 1;
						for (int selectionX = 0; selectionX < selectionCount; selectionX++)
						for (int selectionY = 0; selectionY < selectionCount; selectionY++)
						{
							yield return new object[]
							{
								width,                              // width (in pixels)
								height,                             // height (in pixels)
								dpiX,                               // dpiX
								dpiY,                               // dpiY
								format,                             // pixel format
								format.Palettized ? palette : null, // bitmap palette, if pixel format needs one
								selectionX,                         // x-coordinate of the selection (in pixels)
								selectionY,                         // y-coordinate of the selection (in pixels)
								width - 2 * selectionX,             // width of the selection (in pixels)
								height - 2 * selectionY             // height of the selection (in pixels)
							};
						}
					}
				}
			}
		}

		#endregion

		#region NativeBitmap(NativeBitmap source)

		/// <summary>
		/// Tests the <see cref="NativeBitmap(NativeBitmap)"/> constructor,
		/// checks all properties for consistency and disposes the instance.
		/// </summary>
		/// <param name="width">Width of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="height">Height of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="originalStride">Stride of the bitmap to copy (in bytes).</param>
		/// <param name="dpiX">Horizontal resolution of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="dpiY">Vertical resolution of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="format">Pixel format of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="palette">Bitmap palette of the <see cref="NativeBitmap"/> instance to create.</param>
		[Theory]
		[MemberData(nameof(TestData_CreateCopy))]
		public void NativeBitmap_CopyFromExistingNativeBitmap(
			int           width,
			int           height,
			long          originalStride,
			double        dpiX,
			double        dpiY,
			PixelFormat   format,
			BitmapPalette palette)
		{
			// generate a buffer with random data
			// (the buffer contains only the required amount of bytes and initializes non-significant bytes
			// between the last pixel of a row and the end of the row as well, the last row may be shorter
			// than the last row of the actual buffer)
			long originalBufferSize = (height - 1) * originalStride + (width * format.BitsPerPixel + 7) / 8;
			byte[] originalBuffer = new byte[originalBufferSize];
			new Random(0).NextBytes(originalBuffer);

			// create a bitmap with the specified parameters serving as the bitmap to copy
			fixed (byte* pOriginalBuffer = &originalBuffer[0])
			{
				using (var original = new NativeBitmap( // <- _not_ the constructor to test
					       (IntPtr)pOriginalBuffer,
					       originalBuffer.Length,
					       width,
					       height,
					       originalStride,
					       dpiX,
					       dpiY,
					       format,
					       palette))
				{
					// ensure that the properties reflect the correct state
					Assert.Equal(width, original.PixelWidth);
					Assert.Equal(height, original.PixelHeight);
					Assert.Equal(dpiX, original.DpiX);
					Assert.Equal(dpiY, original.DpiY);
					Assert.Equal(format, original.Format);
					Assert.Same(palette, original.Palette);
					Assert.Equal(originalStride, original.BufferStride);
					Assert.Equal(originalBufferSize, original.BufferSize);
					Assert.Equal((IntPtr)pOriginalBuffer, original.BufferStart);

					// create a copy of the bitmap
					using (var copy = new NativeBitmap(original)) // <- constructor to test
					{
						// calculate the expected alignment and stride of the backing buffer
						CalculateBufferAlignmentAndStride(format, width, out int alignment, out long stride);

						// calculate the expected buffer size
						// (for simplicity the NativeBitmap always creates buffers that have a last row of the specified stride)
						long expectedBufferSize = height * stride;

						// check properties of the copy
						Assert.Equal(width, copy.PixelWidth);
						Assert.Equal(height, copy.PixelHeight);
						Assert.Equal(dpiX, copy.DpiX);
						Assert.Equal(dpiY, copy.DpiY);
						Assert.Equal(format, copy.Format);
						Assert.Same(palette, copy.Palette);
						Assert.Equal(stride, copy.BufferStride);
						Assert.Equal(expectedBufferSize, copy.BufferSize);
						Assert.NotEqual(IntPtr.Zero, copy.BufferStart);
						Assert.Equal(0, copy.BufferStart.ToInt64() & (~0u >> (32 - alignment)));

						// ensure the copied bitmap contains the reference data as well
						// (compare byte-wise, but skip non-significant bytes)
						byte* pOriginalRowStart = pOriginalBuffer;
						byte* pCopyRowStart = (byte*)copy.BufferStart;
						for (int y = 0; y < height; y++)
						{
							var pOriginal = pOriginalRowStart;
							var pOriginalEnd = pOriginalRowStart + (width * format.BitsPerPixel + 7) / 8;
							var pCopy = pCopyRowStart;
							while (pOriginal != pOriginalEnd) Assert.Equal(*pOriginal++, *pCopy++);
							pOriginalRowStart += originalStride;
							pCopyRowStart += copy.BufferStride;
						}
					}
				}
			}
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap(NativeBitmap)"/> constructor.
		/// The constructor should throw an exception if the passed bitmap is <c>null</c>.
		/// </summary>
		[Fact]
		public void NativeBitmap_CopyFromExistingNativeBitmap_SourceIsNull()
		{
			var exception = Assert.Throws<ArgumentNullException>(() => new NativeBitmap(null));
			Assert.Equal("source", exception.ParamName);
		}

		#endregion

		#region NativeBitmap(NativeBitmap source, double dpiX, double dpiY)

		/// <summary>
		/// Tests the <see cref="NativeBitmap(NativeBitmap, double, double)"/> constructor,
		/// checks all properties for consistency and disposes the instance.
		/// </summary>
		/// <param name="width">Width of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="height">Height of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="originalStride">Stride of the bitmap to copy (in bytes).</param>
		/// <param name="dpiX">Horizontal resolution of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="dpiY">Vertical resolution of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="format">Pixel format of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="palette">Bitmap palette of the <see cref="NativeBitmap"/> instance to create.</param>
		[Theory]
		[MemberData(nameof(TestData_CreateCopy))]
		public void NativeBitmap_CopyFromExistingNativeBitmap_WithChangedDpi(
			int           width,
			int           height,
			long          originalStride,
			double        dpiX,
			double        dpiY,
			PixelFormat   format,
			BitmapPalette palette)
		{
			// generate a buffer with random data
			// (the buffer contains only the required amount of bytes and initializes non-significant bytes
			// between the last pixel of a row and the end of the row as well, the last row may be shorter
			// than the last row of the actual buffer)
			long originalBufferSize = (height - 1) * originalStride + (width * format.BitsPerPixel + 7) / 8;
			byte[] originalBuffer = new byte[originalBufferSize];
			new Random(0).NextBytes(originalBuffer);

			// create a bitmap with the specified parameters serving as the bitmap to copy
			fixed (byte* pOriginalBuffer = &originalBuffer[0])
			{
				using (var original = new NativeBitmap( // <- _not_ the constructor to test
					       (IntPtr)pOriginalBuffer,
					       originalBuffer.Length,
					       width,
					       height,
					       originalStride,
					       dpiX,
					       dpiY,
					       format,
					       palette))
				{
					// ensure that the properties reflect the correct state
					Assert.Equal(width, original.PixelWidth);
					Assert.Equal(height, original.PixelHeight);
					Assert.Equal(dpiX, original.DpiX);
					Assert.Equal(dpiY, original.DpiY);
					Assert.Equal(format, original.Format);
					Assert.Same(palette, original.Palette);
					Assert.Equal(originalStride, original.BufferStride);
					Assert.Equal(originalBufferSize, original.BufferSize);
					Assert.Equal((IntPtr)pOriginalBuffer, original.BufferStart);

					// create a copy of the bitmap
					// (the resulting bitmap should be a simple copy with changed resolution)
					double newDpiX = dpiX + 10.0;
					double newDpiY = dpiY + 10.0;
					using (var copy = new NativeBitmap(original, newDpiX, newDpiY)) // <- constructor to test
					{
						// calculate the expected alignment and stride of the backing buffer
						CalculateBufferAlignmentAndStride(format, width, out int alignment, out long stride);

						// calculate the expected buffer size
						// (for simplicity the NativeBitmap always creates buffers that have a last row of the specified stride)
						long expectedBufferSize = height * stride;

						// check properties of the copy
						Assert.Equal(width, copy.PixelWidth);
						Assert.Equal(height, copy.PixelHeight);
						Assert.Equal(newDpiX, copy.DpiX);
						Assert.Equal(newDpiY, copy.DpiY);
						Assert.Equal(format, copy.Format);
						Assert.Same(palette, copy.Palette);
						Assert.Equal(stride, copy.BufferStride);
						Assert.Equal(expectedBufferSize, copy.BufferSize);
						Assert.NotEqual(IntPtr.Zero, copy.BufferStart);
						Assert.Equal(0, copy.BufferStart.ToInt64() & (~0u >> (32 - alignment)));

						// ensure the copied bitmap contains the reference data as well
						// (compare byte-wise, but skip non-significant bytes)
						byte* pOriginalRowStart = pOriginalBuffer;
						byte* pCopyRowStart = (byte*)copy.BufferStart;
						for (int y = 0; y < height; y++)
						{
							var pOriginal = pOriginalRowStart;
							var pOriginalEnd = pOriginalRowStart + (width * format.BitsPerPixel + 7) / 8;
							var pCopy = pCopyRowStart;
							while (pOriginal != pOriginalEnd) Assert.Equal(*pOriginal++, *pCopy++);
							pOriginalRowStart += originalStride;
							pCopyRowStart += copy.BufferStride;
						}
					}
				}
			}
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap(NativeBitmap, double, double)"/> constructor.
		/// The constructor should throw an exception if the passed bitmap is <c>null</c>.
		/// </summary>
		[Fact]
		public void NativeBitmap_CopyFromExistingNativeBitmap_WithChangedDpi_SourceIsNull()
		{
			var exception = Assert.Throws<ArgumentNullException>(() => new NativeBitmap(null, 100.0, 100.0));
			Assert.Equal("source", exception.ParamName);
		}

		#endregion

		#region NativeBitmap(int width, int height, double dpiX, double dpiY, PixelFormat format, BitmapPalette palette)

		/// <summary>
		/// Tests the <see cref="NativeBitmap(int, int, double, double, PixelFormat, BitmapPalette)"/> constructor,
		/// checks all properties for consistency and disposes the instance.
		/// </summary>
		/// <param name="width">Width of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="height">Height of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="dpiX">Horizontal resolution of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="dpiY">Vertical resolution of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="format">Pixel format of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="palette">Bitmap palette of the <see cref="NativeBitmap"/> instance to create.</param>
		[Theory]
		[MemberData(nameof(TestData_Create))]
		public void NativeBitmap_NewBitmap(
			int           width,
			int           height,
			double        dpiX,
			double        dpiY,
			PixelFormat   format,
			BitmapPalette palette)
		{
			// calculate the expected alignment and stride of the backing buffer
			CalculateBufferAlignmentAndStride(format, width, out int alignment, out long stride);

			// calculate the expected buffer size
			// (for simplicity the NativeBitmap always creates buffers that have a last row of the specified stride)
			long expectedBufferSize = height * stride;

			using (var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette))
			{
				Assert.Equal(width, bitmap.PixelWidth);
				Assert.Equal(height, bitmap.PixelHeight);
				Assert.Equal(dpiX, bitmap.DpiX);
				Assert.Equal(dpiY, bitmap.DpiY);
				Assert.Equal(format, bitmap.Format);
				Assert.Same(palette, bitmap.Palette);
				Assert.Equal(stride, bitmap.BufferStride);
				Assert.Equal(expectedBufferSize, bitmap.BufferSize);

				// the buffer should be aligned as expected
				byte* pBufferStart = (byte*)bitmap.BufferStart;
				Assert.True(pBufferStart != null);
				Assert.Equal(0, (long)pBufferStart & (~0u >> (32 - alignment)));

				// the buffer should have been cleared
				byte* pBuffer = pBufferStart;
				byte* pBufferEnd = pBufferStart + (height - 1) * stride + (width * format.BitsPerPixel + 7) / 8;
				while (pBuffer != pBufferEnd) Assert.Equal(0, *pBuffer++);
			}
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap(int, int, double, double, PixelFormat, BitmapPalette)"/> constructor.
		/// The constructor should throw an exception if the width is too small.
		/// </summary>
		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public void NativeBitmap_NewBitmap_WidthTooSmall(int width)
		{
			var height = 100;
			double dpi = 100.0;
			var format = PixelFormats.Gray8;
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new NativeBitmap(width, height, dpi, dpi, format, null));
			Assert.Equal("width", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap(int, int, double, double, PixelFormat, BitmapPalette)"/> constructor.
		/// The constructor should throw an exception if the height is too small.
		/// </summary>
		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public void NativeBitmap_NewBitmap_HeightTooSmall(int height)
		{
			var width = 100;
			double dpi = 100.0;
			var format = PixelFormats.Gray8;
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new NativeBitmap(width, height, dpi, dpi, format, null));
			Assert.Equal("height", exception.ParamName);
		}

		#endregion

		#region NativeBitmap(byte[] source, int width, int height, long stride, double dpiX, double dpiY, PixelFormat format, BitmapPalette palette)

		/// <summary>
		/// Tests the <see cref="NativeBitmap(byte[], int, int, long, double, double, PixelFormat, BitmapPalette)"/> constructor,
		/// checks all properties for consistency and disposes the instance.
		/// </summary>
		/// <param name="width">Width of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="height">Height of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="dpiX">Horizontal resolution of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="dpiY">Vertical resolution of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="format">Pixel format of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="palette">Bitmap palette of the <see cref="NativeBitmap"/> instance to create.</param>
		[Theory]
		[MemberData(nameof(TestData_Create))]
		public void NativeBitmap_FromByteArray(
			int           width,
			int           height,
			double        dpiX,
			double        dpiY,
			PixelFormat   format,
			BitmapPalette palette)
		{
			// calculate the expected alignment and stride of the backing buffer of a bitmap with the specified width
			// (alignment and stride should be the same as calculated by the NativeBitmap)
			CalculateBufferAlignmentAndStride(format, width, out int alignment1, out long bufferStride1);

			// calculate the expected stride of the backing buffer of a bitmap with the specified width + 1
			// (the actual bitmap will only be 'width' pixels wide, the last pixel is skipped)
			CalculateBufferAlignmentAndStride(format, width + 1, out _, out long bufferStride2);

			// generate buffers with random data backing the bitmaps
			long bufferSize1 = height * bufferStride1;
			long bufferSize2 = height * bufferStride2;
			byte[] buffer1 = new byte[bufferSize1];
			byte[] buffer2 = new byte[bufferSize2];
			new Random(0).NextBytes(buffer1);
			new Random(0).NextBytes(buffer2);

			// create two bitmap with the specified parameters copying the bitmap stored in the buffer
			// bitmap 1: same stride as the source buffer (the entire buffer is copied at once)
			// bitmap 2: slightly greater stride as the source buffer (copying is done row by row)
			using (var bitmap1 = new NativeBitmap(buffer1, width, height, bufferStride1, dpiX, dpiY, format, palette))
			using (var bitmap2 = new NativeBitmap(buffer2, width, height, bufferStride2, dpiX, dpiY, format, palette))
			{
				void Test(NativeBitmap bitmap, byte[] buffer, long stride)
				{
					// ensure that the properties of the bitmap reflect the correct state
					Assert.Equal(width, bitmap.PixelWidth);
					Assert.Equal(height, bitmap.PixelHeight);
					Assert.Equal(dpiX, bitmap.DpiX);
					Assert.Equal(dpiY, bitmap.DpiY);
					Assert.Equal(format, bitmap.Format);
					Assert.Same(palette, bitmap.Palette);
					Assert.Equal(bufferStride1, bitmap.BufferStride); // buffer1 should have the same layout as the created bitmap
					Assert.Equal(bufferSize1, bitmap.BufferSize);     // buffer1 should have the same layout as the created bitmap

					// the buffer should be aligned as expected
					// (buffer1 should have the same layout as the created bitmap)
					byte* pBufferStart = (byte*)bitmap.BufferStart;
					Assert.True(pBufferStart != null);
					Assert.Equal(0, (long)pBufferStart & (~0u >> (32 - alignment1)));

					// ensure that the bitmap contains the reference data as well
					// (compare byte-wise, but skip non-significant bytes)
					fixed (byte* pOriginalBuffer = &buffer[0])
					{
						byte* pOriginalRowStart = pOriginalBuffer;
						byte* pCopyRowStart = (byte*)bitmap.BufferStart;
						for (int y = 0; y < height; y++)
						{
							var pOriginal = pOriginalRowStart;
							var pOriginalEnd = pOriginalRowStart + (width * format.BitsPerPixel + 7) / 8;
							var pCopy = pCopyRowStart;
							while (pOriginal != pOriginalEnd) Assert.Equal(*pOriginal++, *pCopy++);
							pOriginalRowStart += stride;
							pCopyRowStart += bitmap.BufferStride;
						}
					}
				}

				// check whether the bitmaps have been created as expected
				Test(bitmap1, buffer1, bufferStride1);
				Test(bitmap2, buffer2, bufferStride2);
			}
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap(byte[], int, int, long, double, double, PixelFormat, BitmapPalette)"/> constructor.
		/// The constructor should throw an exception if the width is too small.
		/// </summary>
		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public void NativeBitmap_FromByteArray_WidthTooSmall(int width)
		{
			var height = 100;
			double dpi = 100.0;
			var format = PixelFormats.Gray8;
			byte[] buffer = new byte[1];
			int stride = 1;
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new NativeBitmap(buffer, width, height, stride, dpi, dpi, format, null));
			Assert.Equal("width", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap(byte[], int, int, long, double, double, PixelFormat, BitmapPalette)"/> constructor.
		/// The constructor should throw an exception if the height is too small.
		/// </summary>
		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public void NativeBitmap_FromByteArray_HeightTooSmall(int height)
		{
			var width = 100;
			double dpi = 100.0;
			var format = PixelFormats.Gray8;
			byte[] buffer = new byte[1];
			int stride = 1;
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new NativeBitmap(buffer, width, height, stride, dpi, dpi, format, null));
			Assert.Equal("height", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap(byte[], int, int, long, double, double, PixelFormat, BitmapPalette)"/> constructor.
		/// The constructor should throw an exception if the source array is too small to contain an image of the specified dimensions.
		/// </summary>
		[Fact]
		public void NativeBitmap_FromByteArray_SourceTooSmall()
		{
			var width = 100;
			var height = 100;
			double dpi = 100.0;
			var format = PixelFormats.Gray8;
			CalculateBufferAlignmentAndStride(format, width, out _, out long stride);
			var bufferSize = (height - 1) * stride + (width * format.BitsPerPixel + 7) / 8;
			byte[] buffer = new byte[bufferSize - 1]; // make the buffer 1 byte smaller than required to trigger the exception
			var exception = Assert.Throws<ArgumentException>(() => new NativeBitmap(buffer, width, height, stride, dpi, dpi, format, null));
			Assert.Equal("source", exception.ParamName);
		}

		#endregion

		#region NativeBitmap(IntPtr pImageData, long bufferSize, int width, int height, long stride, double dpiX, double dpiY, PixelFormat format, BitmapPalette palette)

		/// <summary>
		/// Tests the <see cref="NativeBitmap(IntPtr, long, int, int, long, double, double, PixelFormat, BitmapPalette)"/> constructor,
		/// getting all properties and disposing the instance.
		/// </summary>
		/// <param name="width">Width of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="height">Height of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="dpiX">Horizontal resolution of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="dpiY">Vertical resolution of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="format">Pixel format of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="palette">Bitmap palette of the <see cref="NativeBitmap"/> instance to create.</param>
		[Theory]
		[MemberData(nameof(TestData_Create))]
		public void NativeBitmap_FromUnmanagedBuffer(
			int           width,
			int           height,
			double        dpiX,
			double        dpiY,
			PixelFormat   format,
			BitmapPalette palette)
		{
			// calculate the expected alignment and stride of the backing buffer
			CalculateBufferAlignmentAndStride(format, width, out _, out long stride);

			// generate a buffer with random data backing the bitmap
			long bufferSize = height * stride;
			byte[] buffer = new byte[bufferSize];
			new Random(0).NextBytes(buffer);

			// create a bitmap with the specified parameters on top of the buffer
			fixed (byte* pBuffer = &buffer[0])
			{
				using (var bitmap = new NativeBitmap(
					       (IntPtr)pBuffer,
					       buffer.Length,
					       width,
					       height,
					       stride,
					       dpiX,
					       dpiY,
					       format,
					       palette))
				{
					// ensure that the properties reflect the correct state
					Assert.Equal(width, bitmap.PixelWidth);
					Assert.Equal(height, bitmap.PixelHeight);
					Assert.Equal(dpiX, bitmap.DpiX);
					Assert.Equal(dpiY, bitmap.DpiY);
					Assert.Equal(format, bitmap.Format);
					Assert.Same(palette, bitmap.Palette);
					Assert.Equal(stride, bitmap.BufferStride);
					Assert.Equal(bufferSize, bitmap.BufferSize);
					Assert.Equal((IntPtr)pBuffer, bitmap.BufferStart);
				}
			}
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap(IntPtr, long, int, int, long, double, double, PixelFormat, BitmapPalette)"/> constructor.
		/// The constructor should throw an exception if the width is too small.
		/// </summary>
		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public void NativeBitmap_FromUnmanagedBuffer_WidthTooSmall(int width)
		{
			var height = 100;
			double dpi = 100.0;
			var format = PixelFormats.Gray8;
			IntPtr buffer = (IntPtr)0x1; // invalid buffer, but not used anyway
			long bufferSize = 1;
			int stride = 1;
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new NativeBitmap(buffer, bufferSize, width, height, stride, dpi, dpi, format, null));
			Assert.Equal("width", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap(IntPtr, long, int, int, long, double, double, PixelFormat, BitmapPalette)"/> constructor.
		/// The constructor should throw an exception if the height is too small.
		/// </summary>
		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public void NativeBitmap_FromUnmanagedBuffer_HeightTooSmall(int height)
		{
			var width = 100;
			double dpi = 100.0;
			var format = PixelFormats.Gray8;
			IntPtr buffer = (IntPtr)0x1; // invalid buffer, but not used anyway
			long bufferSize = 1;
			int stride = 1;
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new NativeBitmap(buffer, bufferSize, width, height, stride, dpi, dpi, format, null));
			Assert.Equal("height", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap(IntPtr, long, int, int, long, double, double, PixelFormat, BitmapPalette)"/> constructor.
		/// The constructor should throw an exception if the buffer size is too small for the specified bitmap dimensions.
		/// </summary>
		[Fact]
		public void NativeBitmap_FromUnmanagedBuffer_BufferTooSmall()
		{
			var width = 100;
			var height = 100;
			double dpi = 100.0;
			var format = PixelFormats.Gray2; // 2 bits per pixel
			IntPtr buffer = (IntPtr)0x1;     // invalid buffer, but not used anyway
			CalculateBufferAlignmentAndStride(format, width, out _, out long stride);
			long minimumBufferSize = (height - 1) * stride + (width * format.BitsPerPixel + 7) / 8;
			var exception = Assert.Throws<ArgumentException>(() => new NativeBitmap(buffer, minimumBufferSize - 1, width, height, stride, dpi, dpi, format, null));
			Assert.Equal("bufferSize", exception.ParamName);
		}

		#endregion

		#region int PixelWidth { get; }

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.PixelWidth"/> property.
		/// </summary>
		[Fact]
		public void PixelWidth_Get()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			using (var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette))
			{
				Assert.Equal(width, bitmap.PixelWidth);
			}
		}

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.PixelWidth"/> property.
		/// </summary>
		[Fact]
		public void PixelWidth_Get_ObjectDisposed()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette);
			bitmap.Dispose();
			Assert.Throws<ObjectDisposedException>(() => bitmap.PixelWidth);
		}

		#endregion

		#region int PixelHeight { get; }

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.PixelHeight"/> property.
		/// </summary>
		[Fact]
		public void PixelHeight_Get()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			using (var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette))
			{
				Assert.Equal(height, bitmap.PixelHeight);
			}
		}

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.PixelHeight"/> property.
		/// </summary>
		[Fact]
		public void PixelHeight_Get_ObjectDisposed()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette);
			bitmap.Dispose();
			Assert.Throws<ObjectDisposedException>(() => bitmap.PixelHeight);
		}

		#endregion

		#region double DpiX { get; }

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.DpiX"/> property.
		/// </summary>
		[Fact]
		public void DpiX_Get()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			using (var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette))
			{
				Assert.Equal(dpiX, bitmap.DpiX);
			}
		}

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.DpiX"/> property.
		/// </summary>
		[Fact]
		public void DpiX_Get_ObjectDisposed()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette);
			bitmap.Dispose();
			Assert.Throws<ObjectDisposedException>(() => bitmap.DpiX);
		}

		#endregion

		#region double DpiY { get; }

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.DpiY"/> property.
		/// </summary>
		[Fact]
		public void DpiY_Get()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			using (var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette))
			{
				Assert.Equal(dpiY, bitmap.DpiY);
			}
		}

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.DpiY"/> property.
		/// </summary>
		[Fact]
		public void DpiY_Get_ObjectDisposed()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette);
			bitmap.Dispose();
			Assert.Throws<ObjectDisposedException>(() => bitmap.DpiY);
		}

		#endregion

		#region PixelFormat Format { get; }

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.Format"/> property.
		/// </summary>
		[Fact]
		public void Format_Get()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			using (var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette))
			{
				Assert.Equal(format, bitmap.Format);
			}
		}

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.Format"/> property.
		/// </summary>
		[Fact]
		public void Format_Get_ObjectDisposed()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette);
			bitmap.Dispose();
			Assert.Throws<ObjectDisposedException>(() => bitmap.Format);
		}

		#endregion

		#region BitmapPalette Palette { get; }

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.Palette"/> property.
		/// </summary>
		[Fact]
		public void Palette_Get()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			using (var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette))
			{
				Assert.Same(palette, bitmap.Palette);
			}
		}

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.Palette"/> property.
		/// </summary>
		[Fact]
		public void Palette_Get_ObjectDisposed()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette);
			bitmap.Dispose();
			Assert.Throws<ObjectDisposedException>(() => bitmap.Palette);
		}

		#endregion

		#region IntPtr BufferStart { get; }

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.BufferStart"/> property.
		/// </summary>
		[Fact]
		public void BufferStart_Get()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			using (var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette))
			{
				Assert.NotEqual(IntPtr.Zero, bitmap.BufferStart);
			}
		}

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.BufferStart"/> property.
		/// </summary>
		[Fact]
		public void BufferStart_Get_ObjectDisposed()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette);
			bitmap.Dispose();
			Assert.Throws<ObjectDisposedException>(() => bitmap.BufferStart);
		}

		#endregion

		#region long BufferStride { get; }

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.BufferStride"/> property.
		/// </summary>
		[Fact]
		public void BufferStride_Get()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			using (var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette))
			{
				CalculateBufferAlignmentAndStride(format, width, out _, out long stride);
				Assert.Equal(stride, bitmap.BufferStride);
			}
		}

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.BufferStride"/> property.
		/// </summary>
		[Fact]
		public void BufferStride_Get_ObjectDisposed()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette);
			bitmap.Dispose();
			Assert.Throws<ObjectDisposedException>(() => bitmap.BufferStride);
		}

		#endregion

		#region long BufferSize { get; }

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.BufferSize"/> property.
		/// </summary>
		[Fact]
		public void BufferSize_Get()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			using (var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette))
			{
				// for simplicity the last row should also be of the same length as other rows
				CalculateBufferAlignmentAndStride(format, width, out _, out long stride);
				long bufferSize = height * stride;
				Assert.Equal(bufferSize, bitmap.BufferSize);
			}
		}

		/// <summary>
		/// Tests getting the <see cref="NativeBitmap.BufferSize"/> property.
		/// </summary>
		[Fact]
		public void BufferSize_Get_ObjectDisposed()
		{
			const int width = 100;
			const int height = 200;
			const double dpiX = 300.0;
			const double dpiY = 400.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;
			var bitmap = new NativeBitmap(width, height, dpiX, dpiY, format, palette);
			bitmap.Dispose();
			Assert.Throws<ObjectDisposedException>(() => bitmap.BufferSize);
		}

		#endregion

		#region void CopyPixels(Array destination, int destinationOffset, long destinationStride, int sourceRectX, int sourceRectY, int sourceRectWidth, int sourceRectHeight)

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// </summary>
		/// <param name="width">Width of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="height">Height of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="dpiX">Horizontal resolution of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="dpiY">Vertical resolution of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="format">Pixel format of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="palette">Bitmap palette of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="bufferType">Type of elements in the destination array.</param>
		/// <param name="selectionX">X-coordinate of the upper left corner of the rectangular region to select.</param>
		/// <param name="selectionY">Y-coordinate of the upper left corner of the rectangular region to select.</param>
		/// <param name="selectionWidth">Width of the rectangular region to select.</param>
		/// <param name="selectionHeight">Height of the rectangular region to select.</param>
		[Theory]
		[MemberData(nameof(TestData_CopyPixelsToBufferWithSelection))]
		public void CopyPixels_Array(
			int           width,
			int           height,
			double        dpiX,
			double        dpiY,
			PixelFormat   format,
			BitmapPalette palette,
			Type          bufferType,
			int           selectionX,
			int           selectionY,
			int           selectionWidth,
			int           selectionHeight)
		{
			// calculate the expected alignment and stride of the backing buffer
			CalculateBufferAlignmentAndStride(format, width, out _, out long stride);

			// generate a buffer with random data backing the bitmap
			long bitmapBufferSize = height * stride;
			byte[] buffer = new byte[bitmapBufferSize];
			new Random(0).NextBytes(buffer);

			// create a bitmap with the specified parameters on top of the buffer
			fixed (byte* pBuffer = &buffer[0])
			{
				using (var bitmap = new NativeBitmap(
					       (IntPtr)pBuffer,
					       buffer.Length,
					       width,
					       height,
					       stride,
					       dpiX,
					       dpiY,
					       format,
					       palette))
				{
					// ensure that the properties reflect the correct state
					Assert.Equal(width, bitmap.PixelWidth);
					Assert.Equal(height, bitmap.PixelHeight);
					Assert.Equal(dpiX, bitmap.DpiX);
					Assert.Equal(dpiY, bitmap.DpiY);
					Assert.Equal(format, bitmap.Format);
					Assert.Same(palette, bitmap.Palette);
					Assert.Equal(stride, bitmap.BufferStride);
					Assert.Equal(bitmapBufferSize, bitmap.BufferSize);
					Assert.Equal((IntPtr)pBuffer, bitmap.BufferStart);

					long destinationBufferElementSize = Marshal.SizeOf(bufferType);

					#region Actual Test

					void CreateDestinationArrayAndInvokeCopyPixels(int destinationBufferOffset, long destinationBufferStride)
					{
						// create a new destination array of the requested type
						long destinationBufferSize = (bitmap.PixelHeight - 1) * destinationBufferStride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
						destinationBufferSize = (destinationBufferSize + destinationBufferElementSize - 1) & ~(destinationBufferElementSize - 1);
						long destinationBufferLength = destinationBufferSize / destinationBufferElementSize + destinationBufferOffset;
						Array destinationBuffer = Array.CreateInstance(bufferType, destinationBufferLength);

						// let CopyPixel() copy the bitmap into the destination array
						bitmap.CopyPixels(
							destinationBuffer,
							destinationBufferOffset,
							destinationBufferStride,
							selectionX,
							selectionY,
							selectionWidth,
							selectionHeight);

						// ensure the data in the array contains the bitmap data as well
						// (compare byte-wise, but skip non-significant bytes)
						var handle = GCHandle.Alloc(destinationBuffer, GCHandleType.Pinned);
						try
						{
							byte* pDestinationBuffer = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(destinationBuffer, destinationBufferOffset);

							fixed (byte* pBitmapBuffer = &buffer[0])
							{
								if (selectionX * format.BitsPerPixel % 8 == 0)
								{
									// selection started at a byte boundary
									// => no shifting necessary
									byte* pBitmapSelectionRowStart = pBitmapBuffer + selectionY * stride + selectionX * format.BitsPerPixel / 8;
									byte* pBitmapSelectionEnd = pBitmapSelectionRowStart + selectionHeight * stride;
									byte* pDestinationRowStart = pDestinationBuffer;
									long bytesToComparePerRow = ((long)selectionWidth * format.BitsPerPixel + 7) / 8;
									while (pBitmapSelectionRowStart != pBitmapSelectionEnd)
									{
										byte* pBitmapSelection = pBitmapSelectionRowStart;
										byte* pDestination = pDestinationRowStart;
										byte* pDestinationRowEnd = pDestination + bytesToComparePerRow;
										while (pDestination != pDestinationRowEnd) Assert.Equal(*pBitmapSelection++, *pDestination++);
										pBitmapSelectionRowStart += stride;
										pDestinationRowStart += destinationBufferStride;
									}
								}
								else
								{
									// selection did not start at a byte boundary
									// => pixels need to be shifted appropriately
									int shift = (int)((long)selectionX * format.BitsPerPixel % 8);
									int pixelsInFirstBitmapSelectionByte = (8 - shift) / format.BitsPerPixel;
									byte* pBitmapSelectionRowStart = pBitmapBuffer + selectionY * stride + (long)selectionX * format.BitsPerPixel / 8;
									byte* pBitmapSelectionEnd = pBitmapSelectionRowStart + selectionHeight * stride;
									byte* pDestinationRowStart = pDestinationBuffer;
									byte* pDestinationEnd = pDestinationBuffer + destinationBufferSize;
									long bitmapSelectionBytesPerRow = 1 + ((long)Math.Max(selectionWidth - pixelsInFirstBitmapSelectionByte, 0) * format.BitsPerPixel + 7) / 8;
									long destinationBytesPerRow = ((long)selectionWidth * format.BitsPerPixel + 7) / 8;
									while (pBitmapSelectionRowStart != pBitmapSelectionEnd)
									{
										byte* pBitmapSelection = pBitmapSelectionRowStart;
										byte* pBitmapSelectionRowEnd = pBitmapSelectionRowStart + bitmapSelectionBytesPerRow;
										byte* pDestination = pDestinationRowStart;
										byte* pDestinationRowEnd = pDestinationRowStart + destinationBytesPerRow;
										int accu = *pBitmapSelection++ << shift;
										while (pBitmapSelection != pBitmapSelectionRowEnd)
										{
											int next = *pBitmapSelection++;
											accu |= next >> (8 - shift);
											Assert.Equal(accu & 0xFF, *pDestination++);
											accu = next << shift;
										}

										if (pDestination != pDestinationRowEnd)
											Assert.Equal(accu & 0xFF, *pDestination++);

										// test padding in destination buffer
										long bytesToTest = Math.Min(destinationBufferStride, pDestinationEnd - pDestination) - destinationBytesPerRow;
										for (int i = 0; i < bytesToTest; i++) Assert.Equal(0, *pDestination++);

										// proceed with the next row
										pBitmapSelectionRowStart += stride;
										pDestinationRowStart += destinationBufferStride;
									}
								}
							}
						}
						finally
						{
							handle.Free();
						}
					}

					#endregion

					// round up the stride to a multiple of the size of an array element to ensure a row contains a full number of array elements
					long destinationStride = (stride + destinationBufferElementSize - 1) & ~(destinationBufferElementSize - 1);

					// test copying the bitmap into a buffer with the same stride as the original bitmap
					CreateDestinationArrayAndInvokeCopyPixels(0, destinationStride);
					CreateDestinationArrayAndInvokeCopyPixels(1, destinationStride);

					// test copying the bitmap into a buffer that has a greater stride
					// (simply adding one destination buffer element should be enough to enlarge the stride and keep the alignment intact)
					CreateDestinationArrayAndInvokeCopyPixels(0, destinationStride + destinationBufferElementSize);
					CreateDestinationArrayAndInvokeCopyPixels(1, destinationStride + destinationBufferElementSize);
				}
			}
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the bitmap has been disposed.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_BitmapDisposed()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					Array destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ObjectDisposedException>(
						() =>
						{
							bitmap.Dispose();
							bitmap.CopyPixels(
								destinationBuffer,
								0,
								stride,
								0,
								0,
								0,
								0);
						});

					Assert.Equal(nameof(NativeBitmap), exception.ObjectName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the specified buffer is <c>null</c>.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_DestinationIsNull()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					var exception = Assert.Throws<ArgumentNullException>(
						() =>
						{
							bitmap.CopyPixels(
								null,
								0,
								1,
								0,
								0,
								0,
								0);
						});

					Assert.Equal("destination", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the specified destination buffer is too small.
		/// </summary>
		[Theory]
		[InlineData(typeof(sbyte))]
		[InlineData(typeof(byte))]
		[InlineData(typeof(short))]
		[InlineData(typeof(ushort))]
		[InlineData(typeof(int))]
		[InlineData(typeof(uint))]
		[InlineData(typeof(long))]
		[InlineData(typeof(ulong))]
		[InlineData(typeof(float))]
		[InlineData(typeof(double))]
		public void CopyPixels_Array_DestinationSizeIsTooSmall(Type bufferElementType)
		{
			// the selected region should be 2 pixels smaller in all directions to check
			// whether coordinates are considered correctly
			const int selectionMargin = 2;

			// the selected region should be copied into the destination array starting at index 1
			// to check whether the offset is considered correctly
			const int destinationOffset = 1;

			// determine the size of an element in the destination array
			int bufferElementSize = Marshal.SizeOf(bufferElementType);

			CopyPixels_RunTestWithError(
				bitmap =>
				{
					// calculate the stride of the destination buffer and the its overall length needed to receive all pixels
					int selectionWidth = bitmap.PixelWidth - 2 * selectionMargin;
					int selectionHeight = bitmap.PixelHeight - 2 * selectionMargin;
					CalculateBufferAlignmentAndStride(bitmap.Format, selectionWidth, out _, out long destinationStride);
					destinationStride = (destinationStride + bufferElementSize - 1) & ~(bufferElementSize - 1);
					long destinationBufferSize = (selectionHeight - 1) * destinationStride + (selectionWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					destinationBufferSize = (destinationBufferSize + bufferElementSize - 1) & ~(bufferElementSize - 1);
					long destinationBufferLength = destinationBufferSize / bufferElementSize + destinationOffset;

					// buffer is exactly large enough to receive the selected region
					// => operation should succeed
					{
						Array destinationBuffer = Array.CreateInstance(bufferElementType, destinationBufferLength);
						bitmap.CopyPixels(
							destinationBuffer,
							destinationOffset,
							destinationStride,
							selectionMargin,
							selectionMargin,
							selectionWidth,
							selectionHeight);
					}

					// buffer one element too small to receive the selected region
					// => exception expected
					{
						Array destinationBuffer = Array.CreateInstance(bufferElementType, destinationBufferLength - 1);
						var exception = Assert.Throws<ArgumentException>(
							() =>
							{
								bitmap.CopyPixels(
									destinationBuffer,
									destinationOffset,
									destinationStride,
									selectionMargin,
									selectionMargin,
									selectionWidth,
									selectionHeight);
							});

						Assert.Equal("destination", exception.ParamName);
					}
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the specified buffer is a single-dimensional array of an unsupported type.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_DestinationIsSingleDimensionalArrayOfUnsupportedType()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					// single-dimensional array of an unsupported type (decimal[] is not supported)
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationStride = (stride + sizeof(decimal) - 1) & ~(sizeof(decimal) - 1);                                                     // destination buffer stride taking full decimals per row into account
					long destinationBufferSize = (bitmap.PixelHeight - 1) * destinationStride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8; // destination buffer size in bytes
					destinationBufferSize = (destinationBufferSize + sizeof(decimal) - 1) & ~(sizeof(decimal) - 1);                                       // destination buffer size rounded to the next multiple of sizeof(decimal)
					long destinationBufferLength = destinationBufferSize / sizeof(decimal);                                                               // number of decimals in the destination buffer
					decimal[] destinationBuffer = new decimal[destinationBufferLength];
					var exception = Assert.Throws<ArgumentException>(
						() =>
						{
							bitmap.CopyPixels(
								destinationBuffer,
								0,
								destinationStride,
								0,
								0,
								0,
								0);
						});

					Assert.Equal("destination", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the specified buffer is a multi-dimensional array.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_DestinationIsMultiDimensionalArray()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					// multi-dimensional array of a supported type (unsupported type is handled the same way)
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					Array destinationBuffer = new byte[1, destinationBufferSize];
					var exception = Assert.Throws<ArgumentException>(
						() =>
						{
							bitmap.CopyPixels(
								destinationBuffer,
								0,
								stride,
								0,
								0,
								0,
								0);
						});

					Assert.Equal("destination", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the specified buffer offset is negative.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_DestinationOffsetIsNegative()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					Array destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							bitmap.CopyPixels(
								destinationBuffer,
								-1,
								stride,
								0,
								0,
								0,
								0);
						});
					Assert.Equal("destinationOffset", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the specified buffer offset is greater than the length of the buffer.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_DestinationOffsetIsGreaterThanDestinationSize()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					Array destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							bitmap.CopyPixels(
								destinationBuffer,
								destinationBuffer.Length,
								stride,
								0,
								0,
								0,
								0);
						});
					Assert.Equal("destinationOffset", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the specified destination stride is negative.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_DestinationStrideIsNegative()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							bitmap.CopyPixels(
								destinationBuffer,
								0,
								-1,
								0,
								0,
								0,
								0);
						});

					Assert.Equal("destinationStride", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the specified destination stride is too small to store a row of the specified selection width.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_DestinationStrideIsTooSmall()
		{
			// the selected region should be 2 pixels smaller in all directions to check
			// whether coordinates are considered correctly
			const int selectionMargin = 2;

			// the selected region should be copied into the destination array starting at index 1
			// to check whether the offset is considered correctly
			const int destinationOffset = 1;

			CopyPixels_RunTestWithError(
				bitmap =>
				{
					// calculate the stride of the destination buffer and the its overall length needed to receive all pixels
					int selectionWidth = bitmap.PixelWidth - 2 * selectionMargin;
					int selectionHeight = bitmap.PixelHeight - 2 * selectionMargin;
					CalculateBufferAlignmentAndStride(bitmap.Format, selectionWidth, out _, out long destinationStride);
					long destinationBufferSize = (selectionHeight - 1) * destinationStride + (selectionWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					destinationBufferSize += destinationOffset;
					byte[] destinationBuffer = new byte[destinationBufferSize];

					// stride is exactly large enough to store a row of the selected region
					// => should success
					{
						bitmap.CopyPixels(
							destinationBuffer,
							destinationOffset,
							destinationStride,
							selectionMargin,
							selectionMargin,
							selectionWidth,
							selectionHeight);
					}

					// stride is one byte too small to store a row of the selected region
					// => exception expected
					{
						var exception = Assert.Throws<ArgumentOutOfRangeException>(
							() =>
							{
								bitmap.CopyPixels(
									destinationBuffer,
									destinationOffset,
									destinationStride - 1,
									selectionMargin,
									selectionMargin,
									selectionWidth,
									selectionHeight);
							});

						Assert.Equal("destinationStride", exception.ParamName);
					}
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the x-coordinate of the specified source rectangle is negative.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_SourceRectXIsNegative()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							bitmap.CopyPixels(
								destinationBuffer,
								0,
								stride,
								-1,
								0,
								0,
								0);
						});

					Assert.Equal("sourceRectX", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the x-coordinate of the specified source rectangle is greater than the bitmap width.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_SourceRectXIsGreaterThanBitmapWidth()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							bitmap.CopyPixels(
								destinationBuffer,
								0,
								stride,
								bitmap.PixelWidth,
								0,
								0,
								0);
						});

					Assert.Equal("sourceRectX", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the y-coordinate of the specified source rectangle is negative.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_SourceRectYIsNegative()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							bitmap.CopyPixels(
								destinationBuffer,
								0,
								stride,
								0,
								-1,
								0,
								0);
						});

					Assert.Equal("sourceRectY", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the y-coordinate of the specified source rectangle is greater than the bitmap height.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_SourceRectYIsGreaterThanBitmapHeight()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							bitmap.CopyPixels(
								destinationBuffer,
								0,
								stride,
								0,
								bitmap.PixelHeight,
								0,
								0);
						});

					Assert.Equal("sourceRectY", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the width of the specified source rectangle is negative.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_SourceRectWidthIsNegative()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							bitmap.CopyPixels(
								destinationBuffer,
								0,
								stride,
								0,
								0,
								-1,
								0);
						});

					Assert.Equal("sourceRectWidth", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the height of the specified source rectangle is negative.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_SourceRectHeightIsNegative()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							bitmap.CopyPixels(
								destinationBuffer,
								0,
								stride,
								0,
								0,
								0,
								-1);
						});

					Assert.Equal("sourceRectHeight", exception.ParamName);
				});
		}


		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the specified source rectangle exceeds the bounds of the bitmap in x-direction.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_SourceRectExceedsBoundsOfBitmapX()
		{
			// the selected region should be 2 pixels smaller in all directions to check
			// whether coordinates are considered correctly
			const int selectionMargin = 2;

			// the selected region should be copied into the destination array starting at index 1
			// to check whether the offset is considered correctly
			const int destinationOffset = 1;

			CopyPixels_RunTestWithError(
				bitmap =>
				{
					// selection region is still in valid bounds of the bitmap
					// => should succeed
					{
						// calculate the stride of the destination buffer and the its overall length needed to receive all pixels
						int selectionWidth = bitmap.PixelWidth - selectionMargin;
						int selectionHeight = bitmap.PixelHeight - selectionMargin;
						CalculateBufferAlignmentAndStride(bitmap.Format, selectionWidth, out _, out long destinationStride);
						long destinationBufferSize = (selectionHeight - 1) * destinationStride + (selectionWidth * bitmap.Format.BitsPerPixel + 7) / 8;
						destinationBufferSize += destinationOffset;
						byte[] destinationBuffer = new byte[destinationBufferSize];

						// try to copy the pixels
						bitmap.CopyPixels(
							destinationBuffer,
							destinationOffset,
							destinationStride,
							selectionMargin,
							selectionMargin,
							bitmap.PixelWidth - selectionMargin,
							bitmap.PixelHeight - selectionMargin);
					}

					// selection region exceeds the width of the bitmap
					// => exception expected
					{
						// calculate the stride of the destination buffer and the its overall length needed to receive all pixels
						int selectionWidth = bitmap.PixelWidth - selectionMargin + 1;
						int selectionHeight = bitmap.PixelHeight - selectionMargin;
						CalculateBufferAlignmentAndStride(bitmap.Format, selectionWidth, out _, out long destinationStride);
						long destinationBufferSize = (selectionHeight - 1) * destinationStride + (selectionWidth * bitmap.Format.BitsPerPixel + 7) / 8;
						destinationBufferSize += destinationOffset;
						byte[] destinationBuffer = new byte[destinationBufferSize];

						// try to copy the pixels
						var exception = Assert.Throws<ArgumentOutOfRangeException>(
							() =>
							{
								bitmap.CopyPixels(
									destinationBuffer,
									destinationOffset,
									destinationStride,
									selectionMargin,
									selectionMargin,
									selectionWidth,
									selectionHeight);
							});

						Assert.Equal("sourceRectWidth", exception.ParamName);
					}
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(Array, int, long, int, int, int, int)"/> method.
		/// The method should throw an exception if the specified source rectangle exceeds the bounds of the bitmap in y-direction.
		/// </summary>
		[Fact]
		public void CopyPixels_Array_SourceRectExceedsBoundsOfBitmapY()
		{
			// the selected region should be 2 pixels smaller in all directions to check
			// whether coordinates are considered correctly
			const int selectionMargin = 2;

			// the selected region should be copied into the destination array starting at index 1
			// to check whether the offset is considered correctly
			const int destinationOffset = 1;

			CopyPixels_RunTestWithError(
				bitmap =>
				{
					// selection region is still in valid bounds of the bitmap
					// => should succeed
					{
						// calculate the stride of the destination buffer and the its overall length needed to receive all pixels
						int selectionWidth = bitmap.PixelWidth - selectionMargin;
						int selectionHeight = bitmap.PixelHeight - selectionMargin;
						CalculateBufferAlignmentAndStride(bitmap.Format, selectionWidth, out _, out long destinationStride);
						long destinationBufferSize = (selectionHeight - 1) * destinationStride + (selectionWidth * bitmap.Format.BitsPerPixel + 7) / 8;
						destinationBufferSize += destinationOffset;
						byte[] destinationBuffer = new byte[destinationBufferSize];

						// try to copy the pixels
						bitmap.CopyPixels(
							destinationBuffer,
							destinationOffset,
							destinationStride,
							selectionMargin,
							selectionMargin,
							selectionWidth,
							selectionHeight);
					}

					// selection region exceeds the height of the bitmap
					// => exception expected
					{
						// calculate the stride of the destination buffer and the its overall length needed to receive all pixels
						int selectionWidth = bitmap.PixelWidth - selectionMargin;
						int selectionHeight = bitmap.PixelHeight - selectionMargin + 1;
						CalculateBufferAlignmentAndStride(bitmap.Format, selectionWidth, out _, out long destinationStride);
						long destinationBufferSize = (selectionHeight - 1) * destinationStride + (selectionWidth * bitmap.Format.BitsPerPixel + 7) / 8;
						destinationBufferSize += destinationOffset;
						byte[] destinationBuffer = new byte[destinationBufferSize];

						// try to copy the pixels
						var exception = Assert.Throws<ArgumentOutOfRangeException>(
							() =>
							{
								bitmap.CopyPixels(
									destinationBuffer,
									destinationOffset,
									destinationStride,
									selectionMargin,
									selectionMargin,
									selectionWidth,
									selectionHeight);
							});

						Assert.Equal("sourceRectHeight", exception.ParamName);
					}
				});
		}

		#endregion

		#region void CopyPixels(IntPtr destination, long destinationSize, long destinationStride, long sourceRectX, long sourceRectY, long sourceRectWidth, long sourceRectHeight)

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(IntPtr, long, long, long, long, long, long)"/> method.
		/// </summary>
		/// <param name="width">Width of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="height">Height of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="dpiX">Horizontal resolution of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="dpiY">Vertical resolution of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="format">Pixel format of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="palette">Bitmap palette of the <see cref="NativeBitmap"/> instance to create.</param>
		/// <param name="selectionX">X-coordinate of the upper left corner of the rectangular region to select.</param>
		/// <param name="selectionY">Y-coordinate of the upper left corner of the rectangular region to select.</param>
		/// <param name="selectionWidth">Width of the rectangular region to select.</param>
		/// <param name="selectionHeight">Height of the rectangular region to select.</param>
		[Theory]
		[MemberData(nameof(TestData_CopyPixelsToPointer))]
		public void CopyPixels_Pointer(
			int           width,
			int           height,
			double        dpiX,
			double        dpiY,
			PixelFormat   format,
			BitmapPalette palette,
			int           selectionX,
			int           selectionY,
			int           selectionWidth,
			int           selectionHeight)
		{
			// calculate the expected alignment and stride of the backing buffer
			CalculateBufferAlignmentAndStride(format, width, out _, out long stride);

			// generate a buffer with random data backing the bitmap
			long bitmapBufferSize = height * stride;
			byte[] buffer = new byte[bitmapBufferSize];
			new Random(0).NextBytes(buffer);

			// create a bitmap with the specified parameters on top of the buffer
			fixed (byte* pBuffer = &buffer[0])
			{
				using (var bitmap = new NativeBitmap(
					       (IntPtr)pBuffer,
					       buffer.Length,
					       width,
					       height,
					       stride,
					       dpiX,
					       dpiY,
					       format,
					       palette))
				{
					// ensure that the properties reflect the correct state
					Assert.Equal(width, bitmap.PixelWidth);
					Assert.Equal(height, bitmap.PixelHeight);
					Assert.Equal(dpiX, bitmap.DpiX);
					Assert.Equal(dpiY, bitmap.DpiY);
					Assert.Equal(format, bitmap.Format);
					Assert.Same(palette, bitmap.Palette);
					Assert.Equal(stride, bitmap.BufferStride);
					Assert.Equal(bitmapBufferSize, bitmap.BufferSize);
					Assert.Equal((IntPtr)pBuffer, bitmap.BufferStart);

					#region Actual Test

					void CreateDestinationArrayAndInvokeCopyPixels(int destinationBufferOffset, long destinationStride)
					{
						// create a new buffer
						long destinationBufferSize = (height - 1) * destinationStride + (width * format.BitsPerPixel + 7) / 8 + destinationBufferOffset;
						byte[] destinationBuffer = new byte[destinationBufferSize];

						// let CopyPixel() copy the bitmap into the destination buffer and
						// ensure the data in the array contains the bitmap data as well
						// (compare byte-wise, but skip non-significant bytes)
						var handle = GCHandle.Alloc(destinationBuffer, GCHandleType.Pinned);
						try
						{
							byte* pDestinationBufferAtOffset = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(destinationBuffer, destinationBufferOffset);

							// copy pixels into the destination buffer
							bitmap.CopyPixels(
								(IntPtr)pDestinationBufferAtOffset,
								destinationBufferSize - destinationBufferOffset,
								destinationStride,
								selectionX,
								selectionY,
								selectionWidth,
								selectionHeight);

							fixed (byte* pBitmapBuffer = &buffer[0])
							{
								if (selectionX * format.BitsPerPixel % 8 == 0)
								{
									// selection started at a byte boundary
									// => no shifting necessary
									byte* pBitmapSelectionRowStart = pBitmapBuffer + selectionY * stride + selectionX * format.BitsPerPixel / 8;
									byte* pBitmapSelectionEnd = pBitmapSelectionRowStart + selectionHeight * stride;
									byte* pDestinationRowStart = pDestinationBufferAtOffset;
									long bytesToComparePerRow = (selectionWidth * format.BitsPerPixel + 7) / 8;
									while (pBitmapSelectionRowStart != pBitmapSelectionEnd)
									{
										byte* pBitmapSelection = pBitmapSelectionRowStart;
										byte* pDestination = pDestinationRowStart;
										byte* pDestinationRowEnd = pDestination + bytesToComparePerRow;
										while (pDestination != pDestinationRowEnd) Assert.Equal(*pBitmapSelection++, *pDestination++);
										pBitmapSelectionRowStart += stride;
										pDestinationRowStart += destinationStride;
									}
								}
								else
								{
									// selection did not start at a byte boundary
									// => pixels need to be shifted appropriately
									int shift = (int)((long)selectionX * format.BitsPerPixel % 8);
									int pixelsInFirstBitmapSelectionByte = (8 - shift) / format.BitsPerPixel;
									byte* pBitmapSelectionRowStart = pBitmapBuffer + selectionY * stride + (long)selectionX * format.BitsPerPixel / 8;
									byte* pBitmapSelectionEnd = pBitmapSelectionRowStart + selectionHeight * stride;
									byte* pDestinationRowStart = pDestinationBufferAtOffset;
									byte* pDestinationEnd = pDestinationBufferAtOffset + destinationBufferSize;
									long bitmapSelectionBytesPerRow = 1 + ((long)Math.Max(selectionWidth - pixelsInFirstBitmapSelectionByte, 0) * format.BitsPerPixel + 7) / 8;
									long destinationBytesPerRow = ((long)selectionWidth * format.BitsPerPixel + 7) / 8;
									while (pBitmapSelectionRowStart != pBitmapSelectionEnd)
									{
										byte* pBitmapSelection = pBitmapSelectionRowStart;
										byte* pBitmapSelectionRowEnd = pBitmapSelectionRowStart + bitmapSelectionBytesPerRow;
										byte* pDestination = pDestinationRowStart;
										byte* pDestinationRowEnd = pDestinationRowStart + destinationBytesPerRow;
										int accu = *pBitmapSelection++ << shift;
										while (pBitmapSelection != pBitmapSelectionRowEnd)
										{
											int next = *pBitmapSelection++;
											accu |= next >> (8 - shift);
											Assert.Equal(accu & 0xFF, *pDestination++);
											accu = next << shift;
										}

										if (pDestination != pDestinationRowEnd)
											Assert.Equal(accu & 0xFF, *pDestination++);

										// test padding in destination buffer
										long bytesToTest = Math.Min(destinationStride, pDestinationEnd - pDestination) - destinationBytesPerRow;
										for (int i = 0; i < bytesToTest; i++) Assert.Equal(0, *pDestination++);

										// proceed with the next row
										pBitmapSelectionRowStart += stride;
										pDestinationRowStart += destinationStride;
									}
								}
							}
						}
						finally
						{
							handle.Free();
						}
					}

					#endregion

					// test copying the bitmap into a buffer with the same stride as the original bitmap
					// (starting at index 1 can break the alignment, depending on the pixel format, but it should work as well)
					CreateDestinationArrayAndInvokeCopyPixels(0, stride);
					CreateDestinationArrayAndInvokeCopyPixels(1, stride);

					// test copying the bitmap into a buffer that has a greater stride
					// (starting at index 1 and incrementing the stride by 1 can break the alignment, depending on the pixel format,
					// but it should work as well)
					CreateDestinationArrayAndInvokeCopyPixels(0, stride + 1);
					CreateDestinationArrayAndInvokeCopyPixels(1, stride + 1);
				}
			}
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(IntPtr, long, long, long, long, long, long)"/> method.
		/// The method should throw an exception if the bitmap has been disposed.
		/// </summary>
		[Fact]
		public void CopyPixels_Pointer_BitmapDisposed()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ObjectDisposedException>(
						() =>
						{
							bitmap.Dispose();
							fixed (byte* pBuffer = &destinationBuffer[0])
							{
								bitmap.CopyPixels(
									(IntPtr)pBuffer,
									destinationBufferSize,
									stride,
									0,
									0,
									0,
									0);
							}
						});

					Assert.Equal(nameof(NativeBitmap), exception.ObjectName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(IntPtr, long, long, long, long, long, long)"/> method.
		/// The method should throw an exception if the specified buffer is <c>null</c>.
		/// </summary>
		[Fact]
		public void CopyPixels_Pointer_DestinationIsNull()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					var exception = Assert.Throws<ArgumentNullException>(
						() =>
						{
							bitmap.CopyPixels(
								IntPtr.Zero,
								1,
								1,
								0,
								0,
								0,
								0);
						});

					Assert.Equal("destination", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(IntPtr, long, long, long, long, long, long)"/> method.
		/// The method should throw an exception if the specified destination buffer size is too small.
		/// </summary>
		[Fact]
		public void CopyPixels_Pointer_DestinationSizeIsTooSmall()
		{
			// the selected region should be 2 pixel smaller in all directions to check
			// whether coordinates are considered correctly
			const int selectionMargin = 2;

			CopyPixels_RunTestWithError(
				bitmap =>
				{
					int selectionWidth = bitmap.PixelWidth - 2 * selectionMargin;
					int selectionHeight = bitmap.PixelHeight - 2 * selectionMargin;
					CalculateBufferAlignmentAndStride(bitmap.Format, selectionWidth, out _, out long destinationStride);
					long destinationBufferSize = (selectionHeight - 1) * destinationStride + (selectionWidth * bitmap.Format.BitsPerPixel + 7) / 8;

					// buffer is exactly large enough to receive the selected region
					// => operation should succeed
					{
						byte[] destinationBuffer = new byte[destinationBufferSize];
						fixed (byte* pBuffer = &destinationBuffer[0])
						{
							bitmap.CopyPixels(
								(IntPtr)pBuffer,
								destinationBuffer.Length,
								destinationStride,
								selectionMargin,
								selectionMargin,
								selectionWidth,
								selectionHeight);
						}
					}

					// buffer one element too small to receive the selected region
					// => exception expected
					{
						byte[] destinationBuffer = new byte[destinationBufferSize - 1];
						var exception = Assert.Throws<ArgumentOutOfRangeException>(
							() =>
							{
								fixed (byte* pBuffer = &destinationBuffer[0])
								{
									bitmap.CopyPixels(
										(IntPtr)pBuffer,
										destinationBuffer.Length,
										destinationStride,
										selectionMargin,
										selectionMargin,
										selectionWidth,
										selectionHeight);
								}
							});

						Assert.Equal("destinationSize", exception.ParamName);
					}
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(IntPtr, long, long, long, long, long, long)"/> method.
		/// The method should throw an exception if the specified destination stride is negative.
		/// </summary>
		[Fact]
		public void CopyPixels_Pointer_DestinationStrideIsNegative()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							fixed (byte* pBuffer = &destinationBuffer[0])
							{
								bitmap.CopyPixels(
									(IntPtr)pBuffer,
									destinationBufferSize,
									-1,
									0,
									0,
									0,
									0);
							}
						});

					Assert.Equal("destinationStride", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(IntPtr, long, long, long, long, long, long)"/> method.
		/// The method should throw an exception if the specified destination stride is too small to store a row of the specified selection width.
		/// </summary>
		[Fact]
		public void CopyPixels_Pointer_DestinationStrideIsTooSmall()
		{
			// the selected region should be 2 pixels smaller in all directions to check
			// whether coordinates are considered correctly
			const int selectionMargin = 2;

			CopyPixels_RunTestWithError(
				bitmap =>
				{
					// calculate the stride of the destination buffer and the its overall length needed to receive all pixels
					int selectionWidth = bitmap.PixelWidth - 2 * selectionMargin;
					int selectionHeight = bitmap.PixelHeight - 2 * selectionMargin;
					CalculateBufferAlignmentAndStride(bitmap.Format, selectionWidth, out _, out long destinationStride);
					long destinationBufferSize = (selectionHeight - 1) * destinationStride + (selectionWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];

					// stride is exactly large enough to store a row of the selected region
					// => should success
					{
						fixed (byte* pBuffer = &destinationBuffer[0])
						{
							bitmap.CopyPixels(
								(IntPtr)pBuffer,
								destinationBufferSize,
								destinationStride,
								selectionMargin,
								selectionMargin,
								selectionWidth,
								selectionHeight);
						}
					}

					// stride is one byte too small to store a row of the selected region
					// => exception expected
					{
						var exception = Assert.Throws<ArgumentOutOfRangeException>(
							() =>
							{
								fixed (byte* pBuffer = &destinationBuffer[0])
								{
									bitmap.CopyPixels(
										(IntPtr)pBuffer,
										destinationBufferSize,
										destinationStride - 1,
										selectionMargin,
										selectionMargin,
										selectionWidth,
										selectionHeight);
								}
							});

						Assert.Equal("destinationStride", exception.ParamName);
					}
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(IntPtr, long, long, long, long, long, long)"/> method.
		/// The method should throw an exception if the x-coordinate of the specified source rectangle is negative.
		/// </summary>
		[Fact]
		public void CopyPixels_Pointer_SourceRectXIsNegative()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							fixed (byte* pBuffer = &destinationBuffer[0])
							{
								bitmap.CopyPixels(
									(IntPtr)pBuffer,
									destinationBufferSize,
									stride,
									-1,
									0,
									0,
									0);
							}
						});

					Assert.Equal("sourceRectX", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(IntPtr, long, long, long, long, long, long)"/> method.
		/// The method should throw an exception if the x-coordinate of the specified source rectangle is greater than the bitmap width.
		/// </summary>
		[Fact]
		public void CopyPixels_Pointer_SourceRectXIsGreaterThanBitmapWidth()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							fixed (byte* pBuffer = &destinationBuffer[0])
							{
								bitmap.CopyPixels(
									(IntPtr)pBuffer,
									destinationBufferSize,
									stride,
									bitmap.PixelWidth,
									0,
									0,
									0);
							}
						});

					Assert.Equal("sourceRectX", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(IntPtr, long, long, long, long, long, long)"/> method.
		/// The method should throw an exception if the y-coordinate of the specified source rectangle is negative.
		/// </summary>
		[Fact]
		public void CopyPixels_Pointer_SourceRectYIsNegative()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							fixed (byte* pBuffer = &destinationBuffer[0])
							{
								bitmap.CopyPixels(
									(IntPtr)pBuffer,
									destinationBufferSize,
									stride,
									0,
									-1,
									0,
									0);
							}
						});

					Assert.Equal("sourceRectY", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(IntPtr, long, long, long, long, long, long)"/> method.
		/// The method should throw an exception if the y-coordinate of the specified source rectangle is greater than the bitmap height.
		/// </summary>
		[Fact]
		public void CopyPixels_Pointer_SourceRectYIsGreaterThanBitmapHeight()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							fixed (byte* pBuffer = &destinationBuffer[0])
							{
								bitmap.CopyPixels(
									(IntPtr)pBuffer,
									destinationBufferSize,
									stride,
									0,
									bitmap.PixelHeight,
									0,
									0);
							}
						});

					Assert.Equal("sourceRectY", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(IntPtr, long, long, long, long, long, long)"/> method.
		/// The method should throw an exception if the width of the specified source rectangle is negative.
		/// </summary>
		[Fact]
		public void CopyPixels_Pointer_SourceRectWidthIsNegative()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							fixed (byte* pBuffer = &destinationBuffer[0])
							{
								bitmap.CopyPixels(
									(IntPtr)pBuffer,
									destinationBufferSize,
									stride,
									0,
									0,
									-1,
									0);
							}
						});

					Assert.Equal("sourceRectWidth", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(IntPtr, long, long, long, long, long, long)"/> method.
		/// The method should throw an exception if the height of the specified source rectangle is negative.
		/// </summary>
		[Fact]
		public void CopyPixels_Pointer_SourceRectHeightIsNegative()
		{
			CopyPixels_RunTestWithError(
				bitmap =>
				{
					CalculateBufferAlignmentAndStride(bitmap.Format, bitmap.PixelWidth, out _, out long stride);
					long destinationBufferSize = (bitmap.PixelHeight - 1) * stride + (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
					byte[] destinationBuffer = new byte[destinationBufferSize];
					var exception = Assert.Throws<ArgumentOutOfRangeException>(
						() =>
						{
							fixed (byte* pBuffer = &destinationBuffer[0])
							{
								bitmap.CopyPixels(
									(IntPtr)pBuffer,
									destinationBufferSize,
									stride,
									0,
									0,
									0,
									-1);
							}
						});

					Assert.Equal("sourceRectHeight", exception.ParamName);
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(IntPtr, long, long, long, long, long, long)"/> method.
		/// The method should throw an exception if the specified source rectangle exceeds the bounds of the bitmap in x-direction.
		/// </summary>
		[Fact]
		public void CopyPixels_Pointer_SourceRectExceedsBoundsOfBitmapX()
		{
			// the selected region should be 2 pixels smaller in all directions to check
			// whether coordinates are considered correctly
			const int selectionMargin = 2;

			// the selected region should be copied into the destination array starting at index 1
			// to check whether the offset is considered correctly
			const int destinationOffset = 1;

			CopyPixels_RunTestWithError(
				bitmap =>
				{
					// selection region is still in valid bounds of the bitmap
					// => should succeed
					{
						// calculate the stride of the destination buffer and the its overall length needed to receive all pixels
						int selectionWidth = bitmap.PixelWidth - selectionMargin;
						int selectionHeight = bitmap.PixelHeight - selectionMargin;
						CalculateBufferAlignmentAndStride(bitmap.Format, selectionWidth, out _, out long destinationStride);
						long destinationBufferSize = (selectionHeight - 1) * destinationStride + (selectionWidth * bitmap.Format.BitsPerPixel + 7) / 8;
						destinationBufferSize += destinationOffset;
						byte[] destinationBuffer = new byte[destinationBufferSize];

						// try to copy the pixels
						fixed (byte* pBuffer = &destinationBuffer[0])
						{
							bitmap.CopyPixels(
								(IntPtr)pBuffer,
								destinationBufferSize,
								destinationStride,
								selectionMargin,
								selectionMargin,
								selectionWidth,
								selectionHeight);
						}
					}

					// selection region exceeds the width of the bitmap
					// => exception expected
					{
						// calculate the stride of the destination buffer and the its overall length needed to receive all pixels
						int selectionWidth = bitmap.PixelWidth - selectionMargin + 1;
						int selectionHeight = bitmap.PixelHeight - selectionMargin;
						CalculateBufferAlignmentAndStride(bitmap.Format, selectionWidth, out _, out long destinationStride);
						long destinationBufferSize = (selectionHeight - 1) * destinationStride + (selectionWidth * bitmap.Format.BitsPerPixel + 7) / 8;
						destinationBufferSize += destinationOffset;
						byte[] destinationBuffer = new byte[destinationBufferSize];

						// try to copy the pixels
						var exception = Assert.Throws<ArgumentOutOfRangeException>(
							() =>
							{
								fixed (byte* pBuffer = &destinationBuffer[0])
								{
									bitmap.CopyPixels(
										(IntPtr)pBuffer,
										destinationBufferSize,
										destinationStride,
										selectionMargin,
										selectionMargin,
										selectionWidth,
										selectionHeight);
								}
							});

						Assert.Equal("sourceRectWidth", exception.ParamName);
					}
				});
		}

		/// <summary>
		/// Tests the <see cref="NativeBitmap.CopyPixels(IntPtr, long, long, long, long, long, long)"/> method.
		/// The method should throw an exception if the specified source rectangle exceeds the bounds of the bitmap in y-direction.
		/// </summary>
		[Fact]
		public void CopyPixels_Pointer_SourceRectExceedsBoundsOfBitmapY()
		{
			// the selected region should be 2 pixels smaller in all directions to check
			// whether coordinates are considered correctly
			const int selectionMargin = 2;

			// the selected region should be copied into the destination array starting at index 1
			// to check whether the offset is considered correctly
			const int destinationOffset = 1;

			CopyPixels_RunTestWithError(
				bitmap =>
				{
					// selection region is still in valid bounds of the bitmap
					// => should succeed
					{
						// calculate the stride of the destination buffer and the its overall length needed to receive all pixels
						int selectionWidth = bitmap.PixelWidth - selectionMargin;
						int selectionHeight = bitmap.PixelHeight - selectionMargin;
						CalculateBufferAlignmentAndStride(bitmap.Format, selectionWidth, out _, out long destinationStride);
						long destinationBufferSize = (selectionHeight - 1) * destinationStride + (selectionWidth * bitmap.Format.BitsPerPixel + 7) / 8;
						destinationBufferSize += destinationOffset;
						byte[] destinationBuffer = new byte[destinationBufferSize];

						// try to copy the pixels
						fixed (byte* pBuffer = &destinationBuffer[0])
						{
							bitmap.CopyPixels(
								(IntPtr)pBuffer,
								destinationBufferSize,
								destinationStride,
								selectionMargin,
								selectionMargin,
								selectionWidth,
								selectionHeight);
						}
					}

					// selection region exceeds the width of the bitmap
					// => exception expected
					{
						// calculate the stride of the destination buffer and the its overall length needed to receive all pixels
						int selectionWidth = bitmap.PixelWidth - selectionMargin;
						int selectionHeight = bitmap.PixelHeight - selectionMargin + 1;
						CalculateBufferAlignmentAndStride(bitmap.Format, selectionWidth, out _, out long destinationStride);
						long destinationBufferSize = (selectionHeight - 1) * destinationStride + (selectionWidth * bitmap.Format.BitsPerPixel + 7) / 8;
						destinationBufferSize += destinationOffset;
						byte[] destinationBuffer = new byte[destinationBufferSize];

						// try to copy the pixels
						var exception = Assert.Throws<ArgumentOutOfRangeException>(
							() =>
							{
								fixed (byte* pBuffer = &destinationBuffer[0])
								{
									bitmap.CopyPixels(
										(IntPtr)pBuffer,
										destinationBufferSize,
										destinationStride,
										selectionMargin,
										selectionMargin,
										selectionWidth,
										selectionHeight);
								}
							});

						Assert.Equal("sourceRectHeight", exception.ParamName);
					}
				});
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Calculates the buffer alignment and stride taking the pixel format and the width of the bitmap into account.
		/// </summary>
		/// <param name="format">Pixel format of the bitmap.</param>
		/// <param name="width">The width of the bitmap (in pixels).</param>
		/// <param name="alignment">Receives the alignment of rows in the bitmap (4, 8, 16).</param>
		/// <param name="stride">Receives the buffer stride (in bytes).</param>
		private static void CalculateBufferAlignmentAndStride(
			PixelFormat format,
			int         width,
			out int     alignment,
			out long    stride)
		{
			alignment = 0;
			stride = 0;
			if (format.BitsPerPixel <= 32)
			{
				alignment = 4;
				stride = (width * format.BitsPerPixel + 31) / 32 * 4;
			}
			else if (format.BitsPerPixel <= 64)
			{
				alignment = 8;
				stride = (width * format.BitsPerPixel + 63) / 64 * 8;
			}
			else
			{
				alignment = 16;
				stride = (width * format.BitsPerPixel + 127) / 128 * 16;
			}
		}

		/// <summary>
		/// Sets up a bitmap to test the CopyPixels() methods with.
		/// This method should only be used to test throwing exceptions as it only covers only a single
		/// pixel format and bitmap palette.
		/// </summary>
		/// <param name="action">The actual test code working on the <see cref="NativeBitmap"/> instance.</param>
		private static void CopyPixels_RunTestWithError(Action<NativeBitmap> action)
		{
			const int width = 100;
			const int height = 100;
			const double dpiX = 100.0;
			const double dpiY = 200.0;
			PixelFormat format = PixelFormats.Indexed8;
			BitmapPalette palette = BitmapPalettes.WebPalette;

			// calculate the expected alignment and stride of the backing buffer
			CalculateBufferAlignmentAndStride(format, width, out _, out long stride);

			// generate a buffer with random data backing the bitmap
			long bitmapBufferSize = height * stride;
			byte[] buffer = new byte[bitmapBufferSize];
			new Random(0).NextBytes(buffer);

			// create a bitmap with the specified parameters on top of the buffer
			fixed (byte* pBuffer = &buffer[0])
			{
				using (var bitmap = new NativeBitmap((IntPtr)pBuffer, buffer.Length, width, height, stride, dpiX, dpiY, format, palette))
				{
					action(bitmap);
				}
			}
		}

		#endregion
	}

}
