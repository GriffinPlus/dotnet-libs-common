﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace GriffinPlus.Lib;

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
	private static extern nint VirtualAlloc(
		nint             lpAddress,
		nint             dwSize,
		AllocationType   flAllocationType,
		MemoryProtection flProtect);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool VirtualFree(
		nint           lpAddress,
		nint           dwSize,
		MemoryFreeType dwFreeType);

	#endregion

	/// <summary>
	/// The size of a memory page on the current system (in bytes).
	/// </summary>
	private static readonly nint sPageSize = Environment.SystemPageSize;

	/// <summary>
	/// Callback that handles freeing the native buffer.
	/// </summary>
	private readonly NativeBufferFreeCallback mFreeCallback;

	/// <summary>
	/// A disposable pre-allocated buffer of some external code.
	/// </summary>
	private readonly IDisposable mDisposableBuffer;

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
	public static NativeBuffer Create(nint size)
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
	public static NativeBuffer CreateAligned(nint size, nint alignment)
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
	public static NativeBuffer CreatePageAligned(nint size)
	{
		return new NativeBuffer(size, sPageSize);
	}

	/// <summary>
	/// Creates a <see cref="NativeBuffer"/> instance wrapping a pre-allocated buffer.<br/>
	/// Optionally, the pre-allocated buffer can be owned by the <see cref="NativeBuffer"/> instance.
	/// In this case the pre-allocated buffer is released on disposal or finalization using the specified callback.
	/// If the buffer is owned it will add the appropriate amount of memory pressure to the garbage collection and free
	/// the buffer on disposal or finalization and remove previously added memory pressure.
	/// </summary>
	/// <param name="address">Address of the buffer to wrap.</param>
	/// <param name="size">Size of the buffer (in bytes).</param>
	/// <param name="ownsBuffer">
	/// Indicates whether this instance should own the specified native buffer.<br/>
	/// If <c>true</c>, it will add the appropriate amount of memory pressure to the garbage collection and free the
	/// buffer on disposal or finalization and remove previously added memory pressure.
	/// </param>
	/// <param name="freeCallback">Callback that frees the specified buffer.</param>
	/// <returns>A <see cref="NativeBuffer"/> instance wrapping the specified buffer.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="address"/> or <paramref name="freeCallback"/> is <c>null</c>.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="size"/> is negative or greater than <see cref="int.MaxValue"/> in a 32-bit process.
	/// </exception>
	public static NativeBuffer FromPointer(
		nint                     address,
		nint                     size,
		bool                     ownsBuffer,
		NativeBufferFreeCallback freeCallback)
	{
		return new NativeBuffer(address, size, ownsBuffer, freeCallback);
	}

	/// <summary>
	/// Creates a <see cref="NativeBuffer"/> instance wrapping a pre-allocated buffer.<br/>
	/// The buffer is released on disposal by disposing the pre-allocated buffer.<br/>
	/// Assuming the pre-allocated buffer already adds memory pressure to the garbage collection accordingly
	/// <see cref="NativeBuffer"/> will NOT apply any additional pressure.
	/// </summary>
	/// <param name="buffer">The buffer to wrap.</param>
	/// <param name="address">Address of the buffer to wrap (must be the buffer passed to <paramref name="buffer"/>).</param>
	/// <param name="size">Size of the buffer (in bytes).</param>
	/// <param name="ownsBuffer">
	/// Indicates whether this instance should own the <paramref name="buffer"/>.<br/>
	/// If <c>true</c> the <see cref="NativeBuffer"/> will dispose <paramref name="buffer"/> on its own disposal.
	/// </param>
	/// <returns>A <see cref="NativeBuffer"/> instance wrapping the specified buffer.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="buffer"/> or <paramref name="address"/> is <c>null</c>.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="size"/> is negative or greater than <see cref="int.MaxValue"/> in a 32-bit process.
	/// </exception>
	public static NativeBuffer FromPreAllocatedBuffer(
		IDisposable buffer,
		nint        address,
		nint        size,
		bool        ownsBuffer)
	{
		return new NativeBuffer(buffer, address, size, ownsBuffer);
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
	private NativeBuffer(nint size, nint alignment) :
		base(IntPtr.Zero, true)
	{
		if (size < 0) throw new ArgumentOutOfRangeException(nameof(size), size, "The size must be positive.");

		if (sizeof(nint) == 4 && size > int.MaxValue)
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
			// a. buffer must be aligned to a page boundary
			// b. buffer is so large that some waste is negligible (threshold 5% waste)
			nint allocSize = (size + sPageSize - 1) & ~(sPageSize - 1);
			if (alignment == sPageSize || (double)(allocSize - size) / size < 0.05)
			{
				// the buffer must be aligned to a page boundary, contains entire pages or the buffer is so large
				// that some waste is negligible
				// => using VirtualAlloc() is the most performant way to get entire memory pages
				ActualSize = allocSize;
				UnsafeAddress = VirtualAlloc(
					0,
					ActualSize,
					AllocationType.RESERVE | AllocationType.COMMIT,
					MemoryProtection.READWRITE);
				mFreeCallback = buffer => VirtualFree(buffer.handle, 0, MemoryFreeType.RELEASE);
				SetHandle(UnsafeAddress);
				if (IsInvalid) throw new OutOfMemoryException();
				GC.AddMemoryPressure(ActualSize);
				return;
			}
		}

#if NET6_0 || NET7_0 || NET8_0
		if (alignment > 1)
		{
			// use the platform dependent aligned allocation API, e.g. aligned_alloc or _aligned_malloc
			ActualSize = size;
			UnsafeAddress = (nint)NativeMemory.AlignedAlloc((nuint)ActualSize, (nuint)alignment);
			mFreeCallback = buffer => NativeMemory.AlignedFree(buffer.handle.ToPointer());
			SetHandle(UnsafeAddress);
			if (IsInvalid) throw new OutOfMemoryException();
			GC.AddMemoryPressure(ActualSize);

			// clear the allocated buffer
			ClearBuffer(UnsafeAddress, ActualSize);

			return;
		}
#elif NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48 || NETCOREAPP3_0 || NET5_0
#else
#error Unhandled target framework.
#endif

		// fall back to using Marshal.AllocHGlobal() and allocate a buffer that is a bit larger
		// than requested and adjust the alignment appropriately as required
		ActualSize = size + alignment - 1;
		nint bufferAddress = Marshal.AllocHGlobal(ActualSize);
		UnsafeAddress = (bufferAddress + alignment - 1) & ~(alignment - 1);
		mFreeCallback = buffer => Marshal.FreeHGlobal(buffer.handle);
		SetHandle(bufferAddress);
		if (IsInvalid) throw new OutOfMemoryException();
		GC.AddMemoryPressure(ActualSize);

		// clear the allocated buffer
		ClearBuffer(bufferAddress, ActualSize);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NativeBuffer"/> class wrapping a pre-allocated buffer.<br/>
	/// Optionally, the pre-allocated buffer can be owned by the <see cref="NativeBuffer"/> instance.
	/// In this case the pre-allocated buffer is released on disposal or finalization using the specified callback.
	/// If the buffer is owned it will add the appropriate amount of memory pressure to the garbage collection and free
	/// the buffer on disposal or finalization and remove previously added memory pressure.
	/// </summary>
	/// <param name="address">Address of the buffer to wrap.</param>
	/// <param name="size">Size of the buffer (in bytes).</param>
	/// <param name="ownsBuffer">
	/// Indicates whether this instance should own the specified native buffer.<br/>
	/// If <c>true</c>, it will add the appropriate amount of memory pressure to the garbage collection and free the
	/// buffer on disposal or finalization and remove previously added memory pressure.
	/// </param>
	/// <param name="freeCallback">Callback that frees the specified buffer.</param>
	/// <exception cref="ArgumentNullException"><paramref name="address"/> or <paramref name="freeCallback"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="size"/> is negative or greater than <see cref="int.MaxValue"/> in a 32-bit process.
	/// </exception>
	private NativeBuffer(
		nint                     address,
		nint                     size,
		bool                     ownsBuffer,
		NativeBufferFreeCallback freeCallback) :
		base(IntPtr.Zero, true)
	{
		if (address == 0) throw new ArgumentNullException(nameof(address));

		if (size < 0) throw new ArgumentOutOfRangeException(nameof(size), size, "The size must be positive.");

		if (sizeof(nint) == 4 && size > int.MaxValue)
			throw new ArgumentOutOfRangeException(nameof(size), size, "The size must be int.MaxValue at maximum in a 32-bit process.");

		mFreeCallback = freeCallback ?? throw new ArgumentNullException(nameof(freeCallback));
		UnsafeAddress = address;
		ActualSize = Size = size;
		OwnsBuffer = ownsBuffer;
		SetHandle(address);
		if (OwnsBuffer && ActualSize > 0) GC.AddMemoryPressure(ActualSize);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NativeBuffer"/> class wrapping a disposable pre-allocated buffer.
	/// The buffer is released on disposal by disposing the pre-allocated buffer.<br/>
	/// Assuming the pre-allocated buffer already adds memory pressure to the garbage collection accordingly
	/// <see cref="NativeBuffer"/> will NOT apply any additional pressure.
	/// </summary>
	/// <param name="buffer">The buffer to wrap.</param>
	/// <param name="address">Address of the buffer to wrap (must be the buffer passed to <paramref name="buffer"/>).</param>
	/// <param name="size">Size of the buffer (in bytes).</param>
	/// <param name="ownsBuffer">
	/// Indicates whether this instance should own the <paramref name="buffer"/>.<br/>
	/// If <c>true</c> the <see cref="NativeBuffer"/> will dispose <paramref name="buffer"/> on its own disposal.
	/// </param>
	/// <returns>A <see cref="NativeBuffer"/> instance wrapping the specified buffer.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="buffer"/> or <paramref name="address"/> is <c>null</c>.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="size"/> is negative or greater than <see cref="int.MaxValue"/> in a 32-bit process.
	/// </exception>
	private NativeBuffer(
		IDisposable buffer,
		nint        address,
		nint        size,
		bool        ownsBuffer) :
		base(IntPtr.Zero, true)
	{
		if (address == 0) throw new ArgumentNullException(nameof(address));

		if (size < 0) throw new ArgumentOutOfRangeException(nameof(size), size, "The size must be positive.");

		if (sizeof(nint) == 4 && size > int.MaxValue)
			throw new ArgumentOutOfRangeException(nameof(size), size, "The size must be int.MaxValue at maximum in a 32-bit process.");

		mDisposableBuffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
		mFreeCallback = nativeBuffer =>
		{
			if (nativeBuffer.OwnsBuffer)
				nativeBuffer.mDisposableBuffer.Dispose();
		};
		UnsafeAddress = address;
		ActualSize = Size = size;
		OwnsBuffer = ownsBuffer;
		SetHandle(address);
	}

	/// <summary>
	/// Gets a value indicating whether the buffer is invalid.
	/// </summary>
	public override bool IsInvalid => IsClosed || handle == IntPtr.Zero;

	/// <summary>
	/// Gets a value indicating whether this instance owns the wrapped native buffer. If <c>true</c>, it will free
	/// the buffer on disposal or finalization and reduce previously added memory pressure from the garbage
	/// collection.
	/// </summary>
	public bool OwnsBuffer { get; private set; }

	/// <summary>
	/// Gets the address of the buffer.
	/// The <see cref="NativeBuffer"/> must be kept alive until the underlying buffer is not used anymore,
	/// otherwise access violations can occur due to the buffer getting collected prematurely. This can be
	/// done by putting a <see cref="GC.KeepAlive"/> at the end of the code using the native buffer.
	/// It is much safer to use <see cref="GetAccessor"/> to obtain an accessor object that keeps the buffer
	/// alive until it is disposed.
	/// </summary>
	/// <returns>Address of the buffer.</returns>
	public nint UnsafeAddress { get; private set; }

	/// <summary>
	/// Gets the size of the buffer.
	/// </summary>
	public nint Size { get; private set; }

	/// <summary>
	/// Gets the actual address of the wrapped native buffer. It may be less than <see cref="UnsafeAddress"/>
	/// due to adjustments that were done to guarantee a requested alignment.
	/// The <see cref="NativeBuffer"/> must be kept alive until the underlying buffer is not used anymore,
	/// otherwise access violations can occur due to the buffer getting collected prematurely. This can be
	/// done by putting a <see cref="GC.KeepAlive"/> at the end of the code using the native buffer.
	/// </summary>
	public nint UnsafeActualAddress => IsInvalid ? 0 : handle;

	/// <summary>
	/// The actual size of the native buffer. It may be greater than <see cref="Size"/>
	/// due to adjustments that were done to guarantee a requested alignment.
	/// </summary>
	public nint ActualSize { get; private set; }

	/// <summary>
	/// Gets an accessor that can be used to safely access the buffer.
	/// The accessor should be used in conjunction with a 'using' statement to work as expected.
	/// </summary>
	/// <returns>A <see cref="NativeBufferAccessor"/> providing access to the buffer.</returns>
	public NativeBufferAccessor GetAccessor()
	{
		return new NativeBufferAccessor(this);
	}

	/// <summary>
	/// Frees the native buffer.
	/// </summary>
	/// <returns>Always <c>true</c>.</returns>
	protected override bool ReleaseHandle()
	{
		mFreeCallback(this);
		SetHandleAsInvalid();
		if (OwnsBuffer && ActualSize > 0 && mDisposableBuffer == null) GC.RemoveMemoryPressure(ActualSize);
		OwnsBuffer = false;
		ActualSize = 0;
		UnsafeAddress = 0;
		Size = 0;
		return true;
	}

	/// <summary>
	/// Returns the buffer as a <see cref="Span{T}"/>.<br/>
	/// The buffer must be kept alive as long as the span is used, because the span does not contain a
	/// reference to the <see cref="NativeBuffer"/> instance. The garbage collector might collect it, if it is not
	/// referenced somewhere else. Nevertheless, it is a good notion to dispose the <see cref="NativeBuffer"/> instance
	/// when it is not needed any more. This also ensures that the garbage collector does not collect it by accident.
	/// </summary>
	/// <returns>A <see cref="Span{T}"/> representing the buffer.</returns>
	/// <exception cref="NotSupportedException">The buffer is too big to be represented as a Span&lt;byte&gt;.</exception>
	public Span<byte> AsSpan()
	{
		if (Size > int.MaxValue)
			throw new NotSupportedException("The buffer is too big to be represented as a Span<byte>.");

		return new Span<byte>((void*)UnsafeAddress, (int)Size);
	}

	/// <summary>
	/// Clears the specified buffer.
	/// </summary>
	/// <param name="buffer">Address of the buffer to clear.</param>
	/// <param name="size">Size of the buffer to clear.</param>
	private static void ClearBuffer(nint buffer, nint size)
	{
		// clear the allocated buffer
		// (split clearing the memory block into chunks as the API supports 32-bit sized blocks only)
		byte* pBufferToClear = (byte*)buffer;
		byte* pBufferToClearEnd = pBufferToClear + size;
		while (pBufferToClear != pBufferToClearEnd)
		{
			// memory should always be pointer-aligned for Unsafe.InitBlock()
			Debug.Assert(((long)pBufferToClear & (sizeof(nint) - 1)) == 0);
			uint bytesToClear = (uint)Math.Min(pBufferToClearEnd - pBufferToClear, uint.MaxValue);
			Unsafe.InitBlock(pBufferToClear, 0, bytesToClear);
			pBufferToClear += bytesToClear;
		}
	}
}
