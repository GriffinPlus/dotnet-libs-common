///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace GriffinPlus.Lib
{

	/// <summary>
	/// A native buffer with alignment constraints.
	/// </summary>
	public sealed unsafe class NativeBuffer : SafeHandle
	{
		#region P/Invoke (Windows)

		[Flags]
		private enum AllocationType : uint
		{
			COMMIT      = 0x1000,
			RESERVE     = 0x2000,
			RESET       = 0x80000,
			LARGE_PAGES = 0x20000000,
			PHYSICAL    = 0x400000,
			TOP_DOWN    = 0x100000,
			WRITE_WATCH = 0x200000
		}

		[Flags]
		private enum MemoryFreeType : uint
		{
			DECOMMIT = 0x4000,
			RELEASE  = 0x8000
		}

		[Flags]
		private enum MemoryProtection : uint
		{
			EXECUTE                   = 0x10,
			EXECUTE_READ              = 0x20,
			EXECUTE_READWRITE         = 0x40,
			EXECUTE_WRITECOPY         = 0x80,
			NOACCESS                  = 0x01,
			READONLY                  = 0x02,
			READWRITE                 = 0x04,
			WRITECOPY                 = 0x08,
			GUARD_Modifierflag        = 0x100,
			NOCACHE_Modifierflag      = 0x200,
			WRITECOMBINE_Modifierflag = 0x400
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr VirtualAlloc(
			IntPtr           lpAddress,
			UIntPtr          dwSize,
			AllocationType   flAllocationType,
			MemoryProtection flProtect);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool VirtualFree(
			IntPtr         lpAddress,
			UIntPtr        dwSize,
			MemoryFreeType dwFreeType);

		#endregion

		/// <summary>
		/// The size of a memory page on the current system (in bytes).
		/// </summary>
		private static readonly int sPageSize = Environment.SystemPageSize;

		/// <summary>
		/// Callback that handles freeing the native buffer.
		/// </summary>
		private readonly NativeBufferFreeCallback mFreeCallback;

		/// <summary>
		/// Creates a buffer with the specified size and no alignment constraints.
		/// </summary>
		/// <param name="size">Size of the buffer to create.</param>
		/// <returns>A buffer with the specified size.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="size"/> is negative or greater than <see cref="int.MaxValue"/> in a 32-bit process.
		/// </exception>
		/// <exception cref="OutOfMemoryException">
		/// Allocation failed because there is not enough memory available.
		/// </exception>
		public static NativeBuffer Create(long size)
		{
			return new NativeBuffer(size, 1);
		}

		/// <summary>
		/// Creates a buffer with the specified size and alignment.
		/// </summary>
		/// <param name="size">Size of the buffer to create.</param>
		/// <param name="alignment">Alignment of the buffer (must be a power of 2).</param>
		/// <returns>A buffer with the specified size and alignment.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="size"/> is negative or greater than <see cref="int.MaxValue"/> in a 32-bit process.<br/>
		/// -or-<br/>
		/// <paramref name="alignment"/> is negative.
		/// </exception>
		/// <exception cref="ArgumentException"><paramref name="alignment"/> must be a power of 2.</exception>
		/// <exception cref="OutOfMemoryException">
		/// Allocation failed because there is not enough memory available.
		/// </exception>
		public static NativeBuffer CreateAligned(long size, long alignment)
		{
			return new NativeBuffer(size, alignment);
		}

		/// <summary>
		/// Creates a page-aligned buffer with the specified size.
		/// </summary>
		/// <param name="size">Size of the buffer to allocate.</param>
		/// <returns>A page-aligned buffer with the specified size.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="size"/> is negative or greater than <see cref="int.MaxValue"/> in a 32-bit process.
		/// </exception>
		/// <exception cref="OutOfMemoryException">
		/// Allocation failed because there is not enough memory available.
		/// </exception>
		public static NativeBuffer CreatePageAligned(long size)
		{
			return new NativeBuffer(size, sPageSize);
		}

		/// <summary>
		/// Creates a <see cref="NativeBuffer"/> instance wrapping a pre-allocated buffer.
		/// </summary>
		/// <param name="address">Address of the buffer to wrap.</param>
		/// <param name="size">Size of the buffer (in bytes).</param>
		/// <param name="ownsBuffer">
		/// Indicates whether this instance should own the specified native buffer.
		/// If it owns the buffer, it will free it on disposal or finalization.
		/// </param>
		/// <param name="freeCallback">Callback that frees the specified buffer.</param>
		/// <returns>A <see cref="NativeBuffer"/> instance wrapping the specified buffer.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="address"/> or <paramref name="freeCallback"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="size"/> is negative or greater than <see cref="int.MaxValue"/> in a 32-bit process.
		/// </exception>
		/// <exception cref="OutOfMemoryException">
		/// Allocation failed because there is not enough memory available.
		/// </exception>
		public static NativeBuffer FromPointer(
			IntPtr                   address,
			long                     size,
			bool                     ownsBuffer,
			NativeBufferFreeCallback freeCallback)
		{
			return new NativeBuffer(address, size, ownsBuffer, freeCallback);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NativeBuffer"/> class.
		/// </summary>
		/// <param name="size">Size of the buffer (in bytes).</param>
		/// <param name="alignment">
		/// Alignment of the buffer (must be a power of 2 and at maximum the system's page size).
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="size"/> is negative or greater than <see cref="int.MaxValue"/> in a 32-bit process.<br/>
		/// -or-<br/>
		/// <paramref name="alignment"/> is negative.
		/// </exception>
		/// <exception cref="ArgumentException"><paramref name="alignment"/> must be a power of 2.</exception>
		/// <exception cref="OutOfMemoryException">Allocation failed because there is not enough memory available.</exception>
		private NativeBuffer(long size, long alignment) :
			base(IntPtr.Zero, true)
		{
			if (size < 0) throw new ArgumentOutOfRangeException(nameof(size), size, "The size must be positive.");

			if (IntPtr.Size == 4 && size > int.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(size), size, "The size must be int.MaxValue at maximum in a 32-bit process.");

			if ((alignment & (alignment - 1)) != 0 || alignment <= 0)
				throw new ArgumentException("The alignment must be a power of 2.", nameof(alignment));

			if (alignment > sPageSize)
				throw new ArgumentException("The alignment must be at maximum {Environment.SystemPageSize} (page size).", nameof(alignment));

			// adjust size to support zero length buffers, but store the requested size
			Size = size;
			size = size > 0 ? size : 1;

			// the buffer is allocated on our own, so we should take care of freeing it
			OwnsBuffer = true;

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				// windows platform

				// use VirtualAlloc() in the following cases
				// a) buffer must be aligned to a page boundary
				// b) buffer is so large that some waste is negligible (threshold 5% waste)
				long allocSize = (size + sPageSize - 1) & ~(sPageSize - 1);
				if (alignment == sPageSize || (double)(allocSize - size) / size < 0.05)
				{
					// the buffer must be aligned to a page boundary, contains entire pages or the buffer is so large
					// that some waste is negligible
					// => using VirtualAlloc() is the most performant way to get entire memory pages
					ActualSize = allocSize;
					Address = VirtualAlloc(
						IntPtr.Zero,
						new UIntPtr((ulong)ActualSize),
						AllocationType.RESERVE | AllocationType.COMMIT,
						MemoryProtection.READWRITE);
					mFreeCallback = buffer => VirtualFree(buffer.handle, UIntPtr.Zero, MemoryFreeType.RELEASE);
					SetHandle(Address);
					if (IsInvalid) throw new OutOfMemoryException();
					GC.AddMemoryPressure(ActualSize);
					return;
				}
			}

#if NET6_0
			if (alignment > 1)
			{
				// use the platform dependent aligned allocation API, e.g. aligned_alloc or _aligned_malloc
				ActualSize = (size + alignment - 1) & ~(alignment - 1);
				Address = new IntPtr(NativeMemory.AlignedAlloc((nuint)ActualSize, (nuint)alignment));
				mFreeCallback = buffer => NativeMemory.AlignedFree(buffer.handle.ToPointer());
				SetHandle(Address);
				if (IsInvalid) throw new OutOfMemoryException();
				GC.AddMemoryPressure(ActualSize);
				return;
			}
#elif NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0
#else
#error Unhandled target framework.
#endif

			// fall back to using Marshal.AllocHGlobal() and allocate a buffer that is a bit larger
			// than requested and adjust the alignment appropriately as required
			ActualSize = (size + alignment - 1) & ~(alignment - 1);
			var bufferAddress = Marshal.AllocHGlobal(new IntPtr(ActualSize));
			// ReSharper disable UselessBinaryOperation
			Address = new IntPtr(alignment * ((bufferAddress.ToInt64() + alignment - 1) / alignment));
			// ReSharper restore UselessBinaryOperation
			mFreeCallback = buffer => Marshal.FreeHGlobal(buffer.handle);
			SetHandle(bufferAddress);
			if (IsInvalid) throw new OutOfMemoryException();
			GC.AddMemoryPressure(ActualSize);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NativeBuffer"/> class wrapping a pre-allocated buffer.
		/// </summary>
		/// <param name="address">Address of the buffer to wrap.</param>
		/// <param name="size">Size of the buffer (in bytes).</param>
		/// <param name="ownsBuffer">
		/// Indicates whether this instance should own the specified native buffer.
		/// If <c>true</c> it will add the appropriate amount of memory pressure to the garbage collection and free the
		/// buffer on disposal or finalization and remove the memory pressure from the garbage collection.
		/// </param>
		/// <param name="freeCallback">Callback that frees the specified buffer.</param>
		/// <exception cref="ArgumentNullException"><paramref name="address"/> or <paramref name="freeCallback"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="size"/> is negative or greater than <see cref="int.MaxValue"/> in a 32-bit process.
		/// </exception>
		private NativeBuffer(
			IntPtr                   address,
			long                     size,
			bool                     ownsBuffer,
			NativeBufferFreeCallback freeCallback) :
			base(IntPtr.Zero, true)
		{
			if (address == IntPtr.Zero) throw new ArgumentNullException(nameof(address));

			if (size < 0) throw new ArgumentOutOfRangeException(nameof(size), size, "The size must be positive.");

			if (IntPtr.Size == 4 && size > int.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(size), size, "The size must be int.MaxValue at maximum in a 32-bit process.");

			mFreeCallback = freeCallback ?? throw new ArgumentNullException(nameof(freeCallback));
			ActualSize = Size = size;
			OwnsBuffer = ownsBuffer;
			SetHandle(address);
			if (OwnsBuffer && ActualSize > 0) GC.AddMemoryPressure(ActualSize);
		}

		/// <summary>
		/// Gets a value indicating whether the buffer is invalid.
		/// </summary>
		public override bool IsInvalid => IsClosed || handle == IntPtr.Zero;

		/// <summary>
		/// Gets a value indicating whether this instance owns the wrapped native buffer. If <c>true</c> it will free
		/// the buffer on disposal or finalization and reduce the previously added memory pressure from the garbage
		/// collection.
		/// </summary>
		public bool OwnsBuffer { get; private set; }

		/// <summary>
		/// Gets the address of the buffer.
		/// </summary>
		/// <returns>Address of the buffer.</returns>
		public IntPtr Address { get; private set; }

		/// <summary>
		/// Gets the size of the buffer.
		/// </summary>
		public long Size { get; private set; }

		/// <summary>
		/// Gets the actual address of the wrapped native buffer. It may be less than <see cref="Address"/>
		/// due to adjustments that were done to guarantee a requested alignment.
		/// </summary>
		public IntPtr ActualAddress => IsInvalid ? IntPtr.Zero : handle;

		/// <summary>
		/// The actual size of the native buffer. It may be greater than <see cref="Size"/>
		/// due to adjustments that were done to guarantee a requested alignment.
		/// </summary>
		public long ActualSize { get; private set; }

		/// <summary>
		/// Frees the native buffer.
		/// </summary>
		/// <returns>Always <c>true</c>.</returns>
		protected override bool ReleaseHandle()
		{
			mFreeCallback(this);
			SetHandleAsInvalid();
			if (OwnsBuffer && ActualSize > 0) GC.RemoveMemoryPressure(ActualSize);
			OwnsBuffer = false;
			ActualSize = 0;
			Address = IntPtr.Zero;
			Size = 0;
			return true;
		}

		/// <summary>
		/// Returns the buffer as a <see cref="Span{T}"/>.<br/>
		/// The buffer must be kept alive as long as the span is used, because the span does not contain a
		/// reference to the <see cref="NativeBuffer"/> instance. The garbage collector might collect it, if it is not
		/// referenced somewhere else. Nevertheless it is a good notion to dispose the <see cref="NativeBuffer"/> instance
		/// when it is not needed any more. This also ensures that the garbage collector does not collect it by accident.
		/// </summary>
		/// <returns>A <see cref="Span{T}"/> representing the buffer.</returns>
		/// <exception cref="NotSupportedException">The buffer is too big to be represented as a Span&lt;byte&gt;.</exception>
		public Span<byte> AsSpan()
		{
			if (Size > int.MaxValue)
				throw new NotSupportedException("The buffer is too big to be represented as a Span<byte>.");

			return new Span<byte>(Address.ToPointer(), (int)Size);
		}
	}

}
