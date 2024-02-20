/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
//// The source code is licensed under the MIT license.
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GriffinPlus.Lib.Imaging;

/// <summary>
/// A bitmap backed by <see cref="NativeBuffer"/> or some other external native buffer.
/// </summary>
public unsafe class NativeBitmap : IDisposable
{
	private readonly int           mPixelWidth;
	private readonly int           mPixelHeight;
	private readonly nint          mBufferStride;
	private readonly PixelFormat   mPixelFormat;
	private readonly double        mDpiX;
	private readonly double        mDpiY;
	private readonly BitmapPalette mPalette;
	private readonly NativeBuffer  mBuffer;
	private readonly nint          mBufferStart;
	private readonly nint          mBufferSize;
	private readonly bool          mOwnsBuffer = true;
	private          bool          mDisposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="NativeBitmap"/> class copying the contents of another bitmap.
	/// Adjusts the alignment and stride if it does not match expectations. The buffer alignment and stride are
	/// adjusted, so that rows in the bitmap start at a 32/64/128 bit boundary depending on the pixel format of
	/// the <paramref name="source"/> bitmap.
	/// </summary>
	/// <param name="source">Bitmap to copy into the created instance.</param>
	public NativeBitmap(NativeBitmap source)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		mPixelWidth = source.PixelWidth;
		mPixelHeight = source.PixelHeight;
		mDpiX = source.DpiX;
		mDpiY = source.DpiY;
		mPixelFormat = source.Format;
		mPalette = source.Palette;
		mBufferStride = GetBufferStride(mPixelWidth, mPixelFormat);
		mBufferSize = mBufferStride * mPixelHeight;
		mBuffer = NativeBuffer.CreatePageAligned(mBufferSize);
		mBufferStart = mBuffer.UnsafeAddress;
		source.CopyPixels(mBuffer.UnsafeAddress, mBufferSize, mBufferStride, 0, 0, mPixelWidth, mPixelHeight);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NativeBitmap"/> class copying the contents of another bitmap
	/// changing the horizontal and vertical resolution. Adjusts the alignment and stride if it does not match
	/// expectations. The buffer alignment and stride are adjusted, so that rows in the bitmap start at a 32/64/128
	/// bit boundary depending on the pixel format of the <paramref name="source"/> bitmap.
	/// </summary>
	/// <param name="source">Bitmap to copy into the created instance.</param>
	/// <param name="dpiX">Horizontal resolution to use.</param>
	/// <param name="dpiY">Vertical resolution to use.</param>
	/// <exception cref="ArgumentNullException">The <paramref name="source"/> is <c>null</c>.</exception>
	public NativeBitmap(
		NativeBitmap source,
		double       dpiX,
		double       dpiY)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		mPixelWidth = source.PixelWidth;
		mPixelHeight = source.PixelHeight;
		mDpiX = dpiX;
		mDpiY = dpiY;
		mPixelFormat = source.Format;
		mPalette = source.Palette;
		mBufferStride = GetBufferStride(mPixelWidth, mPixelFormat);
		mBufferSize = mBufferStride * mPixelHeight;
		mBuffer = NativeBuffer.CreatePageAligned(mBufferSize);
		mBufferStart = mBuffer.UnsafeAddress;
		source.CopyPixels(mBuffer.UnsafeAddress, mBufferSize, mBufferStride, 0, 0, mPixelWidth, mPixelHeight);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NativeBitmap"/> class. The alignment and stride of the buffer
	/// is chosen automatically, so that rows in the bitmap start at a 32/64/128 bit boundary depending on the
	/// specified <paramref name="format"/>.
	/// </summary>
	/// <param name="width">Width of the image (in pixels).</param>
	/// <param name="height">Height of the image (in pixels).</param>
	/// <param name="dpiX">Horizontal resolution of the image (in dpi).</param>
	/// <param name="dpiY">Vertical resolution of the image (in dpi).</param>
	/// <param name="format">Pixel format of the image.</param>
	/// <param name="palette">Color palette, if the pixel format requires one.</param>
	/// <exception cref="ArgumentOutOfRangeException">The <paramref name="width"/> or <paramref name="height"/> is less than 1.</exception>
	public NativeBitmap(
		int           width,
		int           height,
		double        dpiX,
		double        dpiY,
		PixelFormat   format,
		BitmapPalette palette)
	{
		if (width < 1) throw new ArgumentOutOfRangeException(nameof(width), "The width must be at least 1 pixel.");
		if (height < 1) throw new ArgumentOutOfRangeException(nameof(height), "The height must be at least 1 pixel.");

		mPixelWidth = width;
		mPixelHeight = height;
		mDpiX = dpiX;
		mDpiY = dpiY;
		mPalette = palette;
		mPixelFormat = format;
		mBufferStride = GetBufferStride(mPixelWidth, mPixelFormat);
		mBufferSize = mBufferStride * mPixelHeight;
		mBuffer = NativeBuffer.CreatePageAligned(mBufferSize);
		mBufferStart = mBuffer.UnsafeAddress;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NativeBitmap"/> class from the specified image data.
	/// Adjusts the alignment and stride if it does not match expectations. The buffer alignment and stride are
	/// adjusted, so that rows in the bitmap start at a 32/64/128 bit boundary depending on the specified <paramref name="format"/>.
	/// </summary>
	/// <param name="source">Buffer containing the image in memory (the data is copied into the bitmap).</param>
	/// <param name="width">Width of the image (in pixels).</param>
	/// <param name="height">Height of the image (in pixels).</param>
	/// <param name="stride">Size of a row in the image (in bytes).</param>
	/// <param name="dpiX">Horizontal resolution of the image (in dpi).</param>
	/// <param name="dpiY">Vertical resolution of the image (in dpi).</param>
	/// <param name="format">Pixel format of the image.</param>
	/// <param name="palette">Color palette, if the pixel format requires one.</param>
	/// <exception cref="ArgumentOutOfRangeException">The <paramref name="width"/> or <paramref name="height"/> is less than 1.</exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="source"/> does not correspond to the specified size and pixel format of the
	/// image.
	/// </exception>
	public NativeBitmap(
		byte[]        source,
		int           width,
		int           height,
		long          stride,
		double        dpiX,
		double        dpiY,
		PixelFormat   format,
		BitmapPalette palette)
	{
		if (width < 1) throw new ArgumentOutOfRangeException(nameof(width), "The width must be at least 1 pixel.");
		if (height < 1) throw new ArgumentOutOfRangeException(nameof(height), "The height must be at least 1 pixel.");

		mPixelWidth = width;
		mPixelHeight = height;
		mDpiX = dpiX;
		mDpiY = dpiY;
		mPalette = palette;
		mPixelFormat = format;
		mBufferStride = GetBufferStride(mPixelWidth, mPixelFormat);
		mBufferSize = mBufferStride * mPixelHeight;
		long validSourceBufferSize = checked(((long)height - 1) * stride + ((long)width * format.BitsPerPixel + 7) / 8);
		if (validSourceBufferSize > source.Length) throw new ArgumentException("The size of the source buffer does not correspond to the specified size and pixel format of the image.", nameof(source));
		validSourceBufferSize = Math.Min(validSourceBufferSize, Math.Min(mBufferSize, source.Length));
		mBuffer = NativeBuffer.CreatePageAligned(mBufferSize);
		mBufferStart = mBuffer.UnsafeAddress;

		// copy buffer
		fixed (byte* pSourceArray = &source[0])
		{
			if (mBufferStride == stride)
			{
				Buffer.MemoryCopy(pSourceArray, (void*)mBufferStart, validSourceBufferSize, validSourceBufferSize);
			}
			else
			{
				byte* pSource = pSourceArray;
				byte* pSourceEnd = pSource + height * stride;
				byte* pDestination = (byte*)mBufferStart;
				long bytesToCopyPerRow = checked(((long)width * format.BitsPerPixel + 7) / 8);
				while (pSource != pSourceEnd)
				{
					Buffer.MemoryCopy(pSource, pDestination, bytesToCopyPerRow, bytesToCopyPerRow);
					pSource += stride;
					pDestination += mBufferStride;
				}
			}
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NativeBitmap"/> class wrapping the specified <see cref="NativeBuffer"/> instance.
	/// </summary>
	/// <param name="buffer">Buffer to use as the back buffer for the image.</param>
	/// <param name="width">Width of the image (in pixels).</param>
	/// <param name="height">Height of the image (in pixels).</param>
	/// <param name="stride">Size of a row in the image (in bytes).</param>
	/// <param name="dpiX">Horizontal resolution of the image (in dpi).</param>
	/// <param name="dpiY">Vertical resolution of the image (in dpi).</param>
	/// <param name="format">Pixel format of the image.</param>
	/// <param name="palette">Color palette, if the pixel format requires one.</param>
	/// <param name="ownsBuffer">
	/// <c>true</c> if the <see cref="NativeBitmap"/> should dispose <paramref name="buffer"/> when it is disposed itself;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The <paramref name="width"/> or <paramref name="height"/> is less than 1.<br/>
	/// -or-<br/>
	/// The <paramref name="stride"/> is too small for an image with the specified <paramref name="width"/> and <paramref name="format"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// The specified image width, height, pixel format and stride requires a larger buffer than specified.
	/// </exception>
	public NativeBitmap(
		NativeBuffer  buffer,
		int           width,
		int           height,
		nint          stride,
		double        dpiX,
		double        dpiY,
		PixelFormat   format,
		BitmapPalette palette,
		bool          ownsBuffer = true)
	{
		if (width < 1) throw new ArgumentOutOfRangeException(nameof(width), "The width must be at least 1 pixel.");
		if (height < 1) throw new ArgumentOutOfRangeException(nameof(height), "The height must be at least 1 pixel.");

		// check whether the specified stride is long enough for an image with the specified width and pixel format
		long minimumStride = ((long)width * format.BitsPerPixel + 7) / 8;
		if (stride < minimumStride)
		{
			throw new ArgumentOutOfRangeException(
				nameof(stride),
				$"The stride ({stride}) is too small for an image with the specified width ({width}) and pixel format ({format}).");
		}

		// ensure that the specified buffer size is large enough to contain an image of the specified width/height,
		// pixel format and stride
		long minimumRequiredBufferSize = checked((height - 1) * stride + ((long)width * format.BitsPerPixel + 7) / 8);
		if (buffer.Size < minimumRequiredBufferSize)
		{
			throw new ArgumentException(
				"The specified image width, height, pixel format and stride requires a larger buffer than specified.",
				nameof(buffer));
		}

		mPixelWidth = width;
		mPixelHeight = height;
		mDpiX = dpiX;
		mDpiY = dpiY;
		mPalette = palette;
		mPixelFormat = format;
		mBufferStride = stride;
		mBufferSize = buffer.Size;
		mBuffer = buffer;
		mBufferStart = mBuffer.UnsafeAddress;
		mOwnsBuffer = ownsBuffer;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NativeBitmap"/> class using the specified image buffer as backing buffer.
	/// The image is _NOT_ copied, so it must be kept valid as long as the <see cref="NativeBitmap"/> instance is used!
	/// </summary>
	/// <param name="pImageData">Pointer to the buffer to use as backing buffer.</param>
	/// <param name="bufferSize">Size of the buffer (in bytes).</param>
	/// <param name="width">Width of the image (in pixels).</param>
	/// <param name="height">Height of the image (in pixels).</param>
	/// <param name="stride">Size of a row in the image (in bytes).</param>
	/// <param name="dpiX">Horizontal resolution of the image (in dpi).</param>
	/// <param name="dpiY">Vertical resolution of the image (in dpi).</param>
	/// <param name="format">Pixel format of the image.</param>
	/// <param name="palette">Color palette, if the pixel format requires one.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The <paramref name="width"/> or <paramref name="height"/> is less than 1.<br/>
	/// -or-<br/>
	/// The <paramref name="stride"/> is too small for an image with the specified <paramref name="width"/> and <paramref name="format"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="width"/>, <paramref name="height"/>, <paramref name="format"/> and <paramref name="stride"/>
	/// requires a larger buffer than <paramref name="bufferSize"/> specifies.
	/// </exception>
	public NativeBitmap(
		nint          pImageData,
		nint          bufferSize,
		int           width,
		int           height,
		nint          stride,
		double        dpiX,
		double        dpiY,
		PixelFormat   format,
		BitmapPalette palette)
	{
		if (width < 1) throw new ArgumentOutOfRangeException(nameof(width), "The width must be at least 1 pixel.");
		if (height < 1) throw new ArgumentOutOfRangeException(nameof(height), "The height must be at least 1 pixel.");

		// check whether the specified stride is long enough for an image with the specified width and pixel format
		long minimumStride = ((long)width * format.BitsPerPixel + 7) / 8;
		if (stride < minimumStride)
		{
			throw new ArgumentOutOfRangeException(
				nameof(stride),
				$"The stride ({stride}) is too small for an image with the specified width ({width}) and pixel format ({format}).");
		}

		// ensure that the specified buffer size is large enough to contain an image of the specified width/height,
		// pixel format and stride
		long minimumRequiredBufferSize = checked((height - 1) * stride + ((long)width * format.BitsPerPixel + 7) / 8);
		if (minimumRequiredBufferSize > bufferSize)
			throw new ArgumentException("The specified image width, height, pixel format and stride requires a larger buffer than specified.", nameof(bufferSize));

		mPixelWidth = width;
		mPixelHeight = height;
		mDpiX = dpiX;
		mDpiY = dpiY;
		mPalette = palette;
		mPixelFormat = format;
		mBufferStart = pImageData;
		mBufferStride = stride;
		mBufferSize = bufferSize;
	}

	/// <summary>
	/// Disposes the current instance releasing unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		if (mDisposed) return;
		if (mOwnsBuffer) mBuffer?.Dispose();
		mDisposed = true;
	}

	/// <summary>
	/// Gets the width of the image (in pixels).
	/// </summary>
	/// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
	public int PixelWidth
	{
		get
		{
			if (mDisposed) throw new ObjectDisposedException(nameof(NativeBitmap));
			return mPixelWidth;
		}
	}

	/// <summary>
	/// Gets the height of the image (in pixels).
	/// </summary>
	/// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
	public int PixelHeight
	{
		get
		{
			if (mDisposed) throw new ObjectDisposedException(nameof(NativeBitmap));
			return mPixelHeight;
		}
	}

	/// <summary>
	/// Gets the horizontal dots per inch (dpi) of the image.
	/// </summary>
	/// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
	public double DpiX
	{
		get
		{
			if (mDisposed) throw new ObjectDisposedException(nameof(NativeBitmap));
			return mDpiX;
		}
	}

	/// <summary>
	/// Gets the vertical dots per inch (dpi) of the image.
	/// </summary>
	/// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
	public double DpiY
	{
		get
		{
			if (mDisposed) throw new ObjectDisposedException(nameof(NativeBitmap));
			return mDpiY;
		}
	}

	/// <summary>
	/// Gets the native pixel format of the bitmap data.
	/// </summary>
	/// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
	public PixelFormat Format
	{
		get
		{
			if (mDisposed) throw new ObjectDisposedException(nameof(NativeBitmap));
			return mPixelFormat;
		}
	}

	/// <summary>
	/// Gets the color palette of the bitmap, if one is specified.
	/// </summary>
	/// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
	public BitmapPalette Palette
	{
		get
		{
			if (mDisposed) throw new ObjectDisposedException(nameof(NativeBitmap));
			return mPalette;
		}
	}

	/// <summary>
	/// Gets the address of the first pixel in the bitmap.
	/// The <see cref="NativeBitmap"/> must be kept alive until the underlying buffer is not used anymore,
	/// otherwise access violations can occur due to the buffer getting collected prematurely. This can be
	/// done by putting a <see cref="GC.KeepAlive"/> at the end of the code using the native buffer.
	/// It is much safer to use <see cref="GetAccessor"/> to obtain an accessor object that keeps the buffer
	/// alive until it is disposed.
	/// </summary>
	/// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
	public nint UnsafeBufferStart
	{
		get
		{
			if (mDisposed) throw new ObjectDisposedException(nameof(NativeBitmap));
			return mBufferStart;
		}
	}

	/// <summary>
	/// Gets the stride of the underlying image buffer.
	/// </summary>
	/// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
	public nint BufferStride
	{
		get
		{
			if (mDisposed) throw new ObjectDisposedException(nameof(NativeBitmap));
			return mBufferStride;
		}
	}

	/// <summary>
	/// Gets the total size of the underlying image buffer (in bytes).
	/// </summary>
	/// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
	public nint BufferSize
	{
		get
		{
			if (mDisposed) throw new ObjectDisposedException(nameof(NativeBitmap));
			return mBufferSize;
		}
	}

	/// <summary>
	/// Gets an accessor that can be used to safely access the underlying bitmap buffer.
	/// The accessor should be used in conjunction with a 'using' statement to work as expected.
	/// </summary>
	/// <returns>A <see cref="NativeBufferAccessor"/> providing access to the underlying bitmap buffer.</returns>
	/// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
	public NativeBitmapAccessor GetAccessor()
	{
		if (mDisposed) throw new ObjectDisposedException(nameof(NativeBitmap));
		return new NativeBitmapAccessor(this);
	}

	/// <summary>
	/// Copies the bitmap pixel data within the specified rectangle into an array of pixels with the specified stride, starting at the specified offset.
	/// If the specified source rectangle is empty, the entire bitmap is copied.
	/// </summary>
	/// <param name="destination">
	/// The array to copy pixels into (supports: sbyte[], byte[], short[], ushort[], int[], uint[], long[], ulong[], float[] and
	/// double[]).
	/// </param>
	/// <param name="destinationOffset">The offset in the buffer where copying begins (in array elements).</param>
	/// <param name="destinationStride">The stride to use in the destination buffer (in bytes).</param>
	/// <param name="sourceRectX">The x-coordinate of the upper left corner of the rectangular region to copy (in pixels).</param>
	/// <param name="sourceRectY">The y-coordinate of the upper left corner of the rectangular region to copy (in pixels).</param>
	/// <param name="sourceRectWidth">The width of the rectangular region to copy (in pixels, pass <c>0</c> to use <see cref="PixelWidth"/>).</param>
	/// <param name="sourceRectHeight">The height of the rectangular region to copy (in pixels, pass <c>0</c> to use <see cref="PixelHeight"/>).</param>
	/// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
	/// <exception cref="ArgumentNullException">The <paramref name="destination"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The <paramref name="destinationStride"/> is negative or too less for a row of the specified length.<br/>
	/// -or-<br/>
	/// The <paramref name="destinationOffset"/> is negative.<br/>
	/// -or-<br/>
	/// One of the arguments specifying the rectangular region to copy (<paramref name="sourceRectX"/>, <paramref name="sourceRectY"/>,
	/// <paramref name="sourceRectWidth"/> or <paramref name="sourceRectHeight"/>) is out of bounds.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// The <paramref name="destination"/> is a multidimensional array, a single dimensional array of an unsupported type or
	/// just too small for the specified region.
	/// </exception>
	public void CopyPixels(
		Array destination,
		int   destinationOffset,
		long  destinationStride,
		int   sourceRectX,
		int   sourceRectY,
		int   sourceRectWidth,
		int   sourceRectHeight)
	{
		if (mDisposed) throw new ObjectDisposedException(nameof(NativeBitmap));
		if (destination == null) throw new ArgumentNullException(nameof(destination));
		if (destination.Rank != 1) throw new ArgumentException("The buffer must be a single dimensional array.", nameof(destination));
		if (destinationOffset < 0) throw new ArgumentOutOfRangeException(nameof(destinationOffset), "The offset must be positive.");
		if (destinationOffset >= destination.Length) throw new ArgumentOutOfRangeException(nameof(destinationOffset), "The offset exceeds the destination buffer.");
		if (destinationStride < 0) throw new ArgumentOutOfRangeException(nameof(destinationStride), "The stride must be positive.");
		if (sourceRectX < 0) throw new ArgumentOutOfRangeException(nameof(sourceRectX), "The x-coordinate of the upper left corner of the region to copy must be positive.");
		if (sourceRectX >= mPixelWidth) throw new ArgumentOutOfRangeException(nameof(sourceRectX), "The x-coordinate of the upper left corner of the region exceeds the bounds of the bitmap.");
		if (sourceRectY < 0) throw new ArgumentOutOfRangeException(nameof(sourceRectY), "The y-coordinate of the upper left corner of the region to copy must be positive.");
		if (sourceRectY >= mPixelHeight) throw new ArgumentOutOfRangeException(nameof(sourceRectY), "The y-coordinate of the upper left corner of the region exceeds the bounds of the bitmap.");
		if (sourceRectWidth < 0) throw new ArgumentOutOfRangeException(nameof(sourceRectWidth), "The width of the rectangular region to copy must be positive.");
		if (sourceRectHeight < 0) throw new ArgumentOutOfRangeException(nameof(sourceRectHeight), "The height of the rectangular region to copy must be positive.");

		// handle empty region and take the entire bitmap instead
		if (sourceRectWidth <= 0) sourceRectWidth = mPixelWidth;
		if (sourceRectHeight <= 0) sourceRectHeight = mPixelHeight;
		if (checked(sourceRectX + sourceRectWidth) > mPixelWidth) throw new ArgumentOutOfRangeException(nameof(sourceRectWidth), "The width of the rectangular region to copy is too great, so the region exceeds the bounds of the bitmap.");
		if (checked(sourceRectY + sourceRectHeight) > mPixelHeight) throw new ArgumentOutOfRangeException(nameof(sourceRectHeight), "The height of the rectangular region to copy is too great, so the region exceeds the bounds of the bitmap.");

		// ensure that the specified stride is great enough to put in a row of the requested length
		long minimumRequiredStride = checked((sourceRectWidth * mPixelFormat.BitsPerPixel + 7) / 8);
		if (destinationStride < minimumRequiredStride)
			throw new ArgumentOutOfRangeException(nameof(destinationStride), "The stride is too less for an entire row.");

		// determine the minimum buffer size that is large enough to receive the requested region
		long minimumRequiredBufferSize = checked(destinationStride * (sourceRectHeight - 1) + minimumRequiredStride);

		switch (destination)
		{
			// handle sbyte[] as well as the runtime does not differentiate between sbyte[] and byte[]
			case byte[] bytes:
			{
				// determine the usable size of the buffer (in array elements)
				long destinationBufferSize = checked(sizeof(byte) * (destination.Length - destinationOffset));

				// ensure that the buffer is large enough to receive the requested region
				if (destinationBufferSize < minimumRequiredBufferSize)
					throw new ArgumentException("The buffer is too small for the requested rectangular region to copy.", nameof(destination));

				fixed (void* pDestinationBuffer = &bytes[destinationOffset])
				{
					CopyPixels(
						(nint)pDestinationBuffer,
						destinationBufferSize,
						destinationStride,
						sourceRectX,
						sourceRectY,
						sourceRectWidth,
						sourceRectHeight);
				}

				return;
			}

			// handle ushort[] as well as the runtime does not differentiate between short[] and ushort[]
			case short[] shorts:
			{
				// determine the usable size of the buffer (in array elements)
				long destinationBufferSize = checked(sizeof(short) * (destination.Length - destinationOffset));

				// ensure that the buffer is large enough to receive the requested region
				if (destinationBufferSize < minimumRequiredBufferSize)
					throw new ArgumentException("The buffer is too small for the requested rectangular region to copy.", nameof(destination));

				fixed (void* pDestinationBuffer = &shorts[destinationOffset])
				{
					CopyPixels(
						(nint)pDestinationBuffer,
						destinationBufferSize,
						destinationStride,
						sourceRectX,
						sourceRectY,
						sourceRectWidth,
						sourceRectHeight);
				}

				return;
			}

			// handle uint[] as well as the runtime does not differentiate between int[] and uint[]
			case int[] ints:
			{
				// determine the usable size of the buffer (in array elements)
				long destinationBufferSize = checked(sizeof(int) * (destination.Length - destinationOffset));

				// ensure that the buffer is large enough to receive the requested region
				if (destinationBufferSize < minimumRequiredBufferSize)
					throw new ArgumentException("The buffer is too small for the requested rectangular region to copy.", nameof(destination));

				fixed (void* pDestinationBuffer = &ints[destinationOffset])
				{
					CopyPixels(
						(nint)pDestinationBuffer,
						destinationBufferSize,
						destinationStride,
						sourceRectX,
						sourceRectY,
						sourceRectWidth,
						sourceRectHeight);
				}

				return;
			}

			// handle ulong[] as well as the runtime does not differentiate between long[] and ulong[]
			case long[] longs:
			{
				// determine the usable size of the buffer (in array elements)
				long destinationBufferSize = checked(sizeof(long) * (destination.Length - destinationOffset));

				// ensure that the buffer is large enough to receive the requested region
				if (destinationBufferSize < minimumRequiredBufferSize)
					throw new ArgumentException("The buffer is too small for the requested rectangular region to copy.", nameof(destination));

				fixed (void* pDestinationBuffer = &longs[destinationOffset])
				{
					CopyPixels(
						(nint)pDestinationBuffer,
						destinationBufferSize,
						destinationStride,
						sourceRectX,
						sourceRectY,
						sourceRectWidth,
						sourceRectHeight);
				}

				return;
			}

			case float[] floats:
			{
				// determine the usable size of the buffer (in array elements)
				long destinationBufferSize = checked(sizeof(float) * (destination.Length - destinationOffset));

				// ensure that the buffer is large enough to receive the requested region
				if (destinationBufferSize < minimumRequiredBufferSize)
					throw new ArgumentException("The buffer is too small for the requested rectangular region to copy.", nameof(destination));

				fixed (void* pDestinationBuffer = &floats[destinationOffset])
				{
					CopyPixels(
						(nint)pDestinationBuffer,
						destinationBufferSize,
						destinationStride,
						sourceRectX,
						sourceRectY,
						sourceRectWidth,
						sourceRectHeight);
				}

				return;
			}

			case double[] doubles:
			{
				// determine the usable size of the buffer (in array elements)
				long destinationBufferSize = checked(sizeof(double) * (destination.Length - destinationOffset));

				// ensure that the buffer is large enough to receive the requested region
				if (destinationBufferSize < minimumRequiredBufferSize)
					throw new ArgumentException("The buffer is too small for the requested rectangular region to copy.", nameof(destination));

				fixed (void* pDestinationBuffer = &doubles[destinationOffset])
				{
					CopyPixels(
						(nint)pDestinationBuffer,
						destinationBufferSize,
						destinationStride,
						sourceRectX,
						sourceRectY,
						sourceRectWidth,
						sourceRectHeight);
				}

				return;
			}

			default:
				throw new ArgumentException(
					"The buffer is of an unsupported type. You may use sbyte[], byte[], short[], ushort[], int[], uint[], float[] and double[].",
					nameof(destination));
		}
	}

	/// <summary>
	/// Copies the bitmap pixel data within the specified rectangle into a buffer with the specified stride.
	/// If the specified source rectangle is empty, the entire bitmap is copied.
	/// </summary>
	/// <param name="destination">A pointer to the buffer to copy pixels into.</param>
	/// <param name="destinationSize">Size of the buffer to copy pixels into (in bytes).</param>
	/// <param name="destinationStride">The stride of the buffer to copy pixels into (in bytes).</param>
	/// <param name="sourceRectX">The x-coordinate of the upper left corner of the rectangle to copy (in pixels).</param>
	/// <param name="sourceRectY">The y-coordinate of the upper left corner of the rectangle to copy (in pixels).</param>
	/// <param name="sourceRectWidth">The width of the rectangle to copy (in pixels, pass <c>0</c> to use <see cref="PixelWidth"/>).</param>
	/// <param name="sourceRectHeight">The height of the rectangle to copy (in pixels, pass <c>0</c> to use <see cref="PixelHeight"/>).</param>
	/// <exception cref="ObjectDisposedException">The instance has been disposed.</exception>
	/// <exception cref="ArgumentNullException">The <paramref name="destination"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The <paramref name="destinationSize"/> is too small for the specified region.<br/>
	/// -or-<br/>
	/// The <paramref name="destinationStride"/> is negative or too less for a row of the specified length.<br/>
	/// -or-<br/>
	/// One of the arguments specifying the rectangular region to copy (<paramref name="sourceRectX"/>, <paramref name="sourceRectY"/>,
	/// <paramref name="sourceRectWidth"/> or <paramref name="sourceRectHeight"/>) is out of bounds.
	/// </exception>
	public void CopyPixels(
		nint destination,
		long destinationSize,
		long destinationStride,
		long sourceRectX,
		long sourceRectY,
		long sourceRectWidth,
		long sourceRectHeight)
	{
		if (mDisposed) throw new ObjectDisposedException(nameof(NativeBitmap));
		if (destination == 0) throw new ArgumentNullException(nameof(destination));
		if (destinationStride < 0) throw new ArgumentOutOfRangeException(nameof(destinationStride), "The stride must be positive.");
		if (sourceRectX < 0) throw new ArgumentOutOfRangeException(nameof(sourceRectX), "The x-coordinate of the upper left corner of the region to copy must be positive.");
		if (sourceRectX >= mPixelWidth) throw new ArgumentOutOfRangeException(nameof(sourceRectX), "The x-coordinate of the upper left corner of the region exceeds the bounds of the bitmap.");
		if (sourceRectY < 0) throw new ArgumentOutOfRangeException(nameof(sourceRectY), "The y-coordinate of the upper left corner of the region to copy must be positive.");
		if (sourceRectY >= mPixelHeight) throw new ArgumentOutOfRangeException(nameof(sourceRectY), "The y-coordinate of the upper left corner of the region exceeds the bounds of the bitmap.");
		if (sourceRectWidth < 0) throw new ArgumentOutOfRangeException(nameof(sourceRectWidth), "The width of the rectangular region to copy must be positive.");
		if (sourceRectHeight < 0) throw new ArgumentOutOfRangeException(nameof(sourceRectHeight), "The height of the rectangular region to copy must be positive.");

		// handle empty region and take the entire bitmap instead
		if (sourceRectWidth <= 0) sourceRectWidth = mPixelWidth;
		if (sourceRectHeight <= 0) sourceRectHeight = mPixelHeight;
		if (checked(sourceRectX + sourceRectWidth) > mPixelWidth) throw new ArgumentOutOfRangeException(nameof(sourceRectWidth), "The width of the rectangular region to copy is too great, so the region exceeds the bounds of the bitmap.");
		if (checked(sourceRectY + sourceRectHeight) > mPixelHeight) throw new ArgumentOutOfRangeException(nameof(sourceRectHeight), "The height of the rectangular region to copy is too great, so the region exceeds the bounds of the bitmap.");

		// ensure that the specified stride is great enough to put in a row of the requested length
		long minimumRequiredStride = checked((sourceRectWidth * mPixelFormat.BitsPerPixel + 7) / 8);
		if (destinationStride < minimumRequiredStride)
			throw new ArgumentOutOfRangeException(nameof(destinationStride), "The stride is too less for an entire row.");

		// ensure that the buffer is large enough to receive the requested region
		long minimumRequiredBufferSize = checked(destinationStride * (sourceRectHeight - 1) + minimumRequiredStride);
		if (destinationSize < minimumRequiredBufferSize)
			throw new ArgumentOutOfRangeException(nameof(destinationSize), "The buffer is too small for the requested rectangular region to copy.");

		if (sourceRectX == 0 && sourceRectY == 0 && sourceRectWidth == mPixelWidth && sourceRectHeight == mPixelHeight)
		{
			// copy the entire bitmap
			if (mBufferStride == destinationStride)
			{
				// the bitmap has the same stride as the destination buffer
				// => can copy the entire buffer at once...
				long bytesToCopy = checked(((long)mPixelHeight - 1) * mBufferStride + ((long)mPixelWidth * mPixelFormat.BitsPerPixel + 7) / 8);
				Buffer.MemoryCopy((void*)mBufferStart, (void*)destination, bytesToCopy, bytesToCopy);
			}
			else
			{
				// the bitmap and the destination buffer have different strides
				// => copy row by row...
				byte* pSource = (byte*)mBufferStart;
				byte* pSourceEnd = pSource + mPixelHeight * mBufferStride;
				byte* pDestination = (byte*)destination;
				byte* pDestinationEnd = pDestination + destinationSize;
				long bytesToCopyPerRow = checked(((long)mPixelWidth * mPixelFormat.BitsPerPixel + 7) / 8);
				while (pSource != pSourceEnd)
				{
					Buffer.MemoryCopy(pSource, pDestination, bytesToCopyPerRow, bytesToCopyPerRow);
					long bytesToClear = Math.Min(destinationStride, pDestinationEnd - pDestination) - bytesToCopyPerRow;
					if (bytesToClear > 0) Unsafe.InitBlockUnaligned(pDestination + bytesToCopyPerRow, 0, (uint)bytesToClear);
					pSource += mBufferStride;
					pDestination += destinationStride;
				}
			}
		}
		else
		{
			// copy only a region of the bitmap
			if (sourceRectX * mPixelFormat.BitsPerPixel % 8 == 0)
			{
				// region to copy starts at a byte boundary
				// => no shifting necessary
				byte* pSource = checked((byte*)mBufferStart + sourceRectY * mBufferStride + sourceRectX * mPixelFormat.BitsPerPixel / 8);
				byte* pSourceEnd = checked(pSource + sourceRectHeight * mBufferStride);
				byte* pDestination = (byte*)destination;
				byte* pDestinationEnd = pDestination + destinationSize;
				long bytesToCopyPerRow = checked((sourceRectWidth * mPixelFormat.BitsPerPixel + 7) / 8);
				while (pSource != pSourceEnd)
				{
					Buffer.MemoryCopy(pSource, pDestination, bytesToCopyPerRow, bytesToCopyPerRow);
					long bytesToClear = Math.Min(destinationStride, pDestinationEnd - pDestination) - bytesToCopyPerRow;
					if (bytesToClear > 0) Unsafe.InitBlockUnaligned(pDestination + bytesToCopyPerRow, 0, (uint)bytesToClear);
					pSource += mBufferStride;
					pDestination += destinationStride;
				}
			}
			else
			{
				// region to copy does not start at a byte boundary
				// => pixels need to be shifted appropriately
				int shift = checked((int)(sourceRectX * mPixelFormat.BitsPerPixel % 8));
				int pixelsInFirstSourceByte = (8 - shift) / mPixelFormat.BitsPerPixel;
				byte* pSourceRowStart = checked((byte*)mBufferStart + sourceRectY * mBufferStride + sourceRectX * mPixelFormat.BitsPerPixel / 8);
				byte* pSourceEnd = checked(pSourceRowStart + sourceRectHeight * mBufferStride);
				byte* pDestinationRowStart = (byte*)destination;
				byte* pDestinationEnd = pDestinationRowStart + destinationSize;
				long sourceBytesPerRow = checked(1 + (Math.Max(sourceRectWidth - pixelsInFirstSourceByte, 0) * mPixelFormat.BitsPerPixel + 7) / 8);
				long destinationBytesPerRow = checked((sourceRectWidth * mPixelFormat.BitsPerPixel + 7) / 8);
				while (pSourceRowStart != pSourceEnd)
				{
					byte* pSource = pSourceRowStart;
					byte* pSourceRowEnd = pSourceRowStart + sourceBytesPerRow;
					byte* pDestination = pDestinationRowStart;
					byte* pDestinationRowEnd = pDestinationRowStart + destinationBytesPerRow;
					int accu = *pSource++ << shift;
					while (pSource != pSourceRowEnd)
					{
						int next = *pSource++;
						accu |= next >> (8 - shift);
						*pDestination++ = (byte)accu;
						accu = next << shift;
					}

					if (pDestination != pDestinationRowEnd)
						*pDestination++ = (byte)accu;

					// pDestination should now be at the byte just after the last byte containing pixel data
					Debug.Assert(pDestination == pDestinationRowStart + destinationBytesPerRow);

					// clear padding in destination buffer
					long bytesToClear = Math.Min(destinationStride, pDestinationEnd - pDestination) - destinationBytesPerRow;
					if (bytesToClear > 0) Unsafe.InitBlockUnaligned(pDestination, 0, (uint)bytesToClear);

					// proceed with the next row
					pSourceRowStart += mBufferStride;
					pDestinationRowStart += destinationStride;
				}
			}
		}
	}

	/// <summary>
	/// Determines the stride of the buffer for the specified image width and the specified pixel format.
	/// </summary>
	/// <param name="width">Width of the image (in pixels).</param>
	/// <param name="format">Pixel format.</param>
	/// <returns>Stride of the image buffer.</returns>
	private static nint GetBufferStride(int width, PixelFormat format)
	{
		checked
		{
			return format.BitsPerPixel switch
			{
				<= 32 => (nint)((long)width * format.BitsPerPixel + 31) / 32 * 4,
				<= 64 => (nint)((long)width * format.BitsPerPixel + 63) / 64 * 8,
				var _ => (nint)((long)width * format.BitsPerPixel + 127) / 128 * 16
			};
		}
	}
}
