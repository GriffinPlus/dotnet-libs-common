///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Imaging
{

	/// <summary>
	/// A bitmap accessor used by the <see cref="NativeBitmap"/> class.
	/// Using the accessor in conjunction with a 'using' statement instead of unsafe pointers helps to keep
	/// an instance of the <see cref="NativeBitmap"/> class alive while accessing the underlying buffer
	/// with pointers.
	/// </summary>
	public readonly struct NativeBitmapAccessor : IDisposable
	{
		private readonly NativeBitmap mBitmap;

		/// <summary>
		/// Initializes a new instance of the <see cref="NativeBitmapAccessor"/>.
		/// </summary>
		/// <param name="bitmap">Bitmap the accessor should work on.</param>
		internal NativeBitmapAccessor(NativeBitmap bitmap)
		{
			mBitmap = bitmap;
		}

		/// <summary>
		/// Disposes the accessor.
		/// </summary>
		public void Dispose()
		{
			// no need to clean up anything,
			// just keep the bitmap alive up to this point
			GC.KeepAlive(mBitmap);
		}

		/// <summary>
		/// Gets the width of the image (in pixels).
		/// </summary>
		/// <exception cref="ObjectDisposedException">The bitmap has been disposed.</exception>
		public int PixelWidth => mBitmap.PixelWidth;

		/// <summary>
		/// Gets the height of the image (in pixels).
		/// </summary>
		/// <exception cref="ObjectDisposedException">The bitmap has been disposed.</exception>
		public int PixelHeight => mBitmap.PixelHeight;

		/// <summary>
		/// Gets the horizontal dots per inch (dpi) of the image.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The bitmap has been disposed.</exception>
		public double DpiX => mBitmap.DpiX;

		/// <summary>
		/// Gets the vertical dots per inch (dpi) of the image.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The bitmap has been disposed.</exception>
		public double DpiY => mBitmap.DpiY;

		/// <summary>
		/// Gets the native pixel format of the bitmap data.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The bitmap has been disposed.</exception>
		public PixelFormat Format => mBitmap.Format;

		/// <summary>
		/// Gets the color palette of the bitmap, if one is specified.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The bitmap has been disposed.</exception>
		public BitmapPalette Palette => mBitmap.Palette;

		/// <summary>
		/// Gets the address of the first pixel in the bitmap
		/// (the buffer is kept alive until the accessor is disposed).
		/// </summary>
		/// <exception cref="ObjectDisposedException">The bitmap has been disposed.</exception>
		public IntPtr BufferStart => mBitmap.UnsafeBufferStart;

		/// <summary>
		/// Gets the stride of the underlying image buffer.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The bitmap has been disposed.</exception>
		public long BufferStride => mBitmap.BufferStride;

		/// <summary>
		/// Gets the total size of the underlying image buffer (in bytes).
		/// </summary>
		/// <exception cref="ObjectDisposedException">The bitmap has been disposed.</exception>
		public long BufferSize => mBitmap.BufferSize;
	}

}
