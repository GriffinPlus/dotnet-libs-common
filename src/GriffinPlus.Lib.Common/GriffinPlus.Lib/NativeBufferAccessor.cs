///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib
{

	/// <summary>
	/// A buffer accessor used by the <see cref="NativeBuffer"/> class.
	/// Using the accessor in conjunction with a 'using' statement instead of unsafe pointers helps to keep
	/// an instance of the <see cref="NativeBuffer"/> class alive while accessing the underlying buffer
	/// with pointers.
	/// </summary>
	public readonly struct NativeBufferAccessor : IDisposable
	{
		private readonly NativeBuffer mBuffer;

		/// <summary>
		/// Initializes a new instance of the <see cref="NativeBufferAccessor"/>.
		/// </summary>
		/// <param name="buffer">Buffer the accessor should work on.</param>
		internal NativeBufferAccessor(NativeBuffer buffer)
		{
			mBuffer = buffer;
		}

		/// <summary>
		/// Disposes the accessor.
		/// </summary>
		public void Dispose()
		{
			// no need to clean up anything,
			// just keep the buffer alive up to this point
			GC.KeepAlive(mBuffer);
		}

		/// <summary>
		/// Gets the address of the buffer.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
		public IntPtr Address => mBuffer.UnsafeAddress;

		/// <summary>
		/// Gets the size of the buffer.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The buffer has been disposed.</exception>
		public long Size => mBuffer.Size;
	}

}
