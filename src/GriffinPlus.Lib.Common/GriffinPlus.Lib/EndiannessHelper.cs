///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Runtime.CompilerServices;

namespace GriffinPlus.Lib;

/// <summary>
/// Helper methods for handling endianness conversions.
/// </summary>
public static class EndiannessHelper
{
	/// <summary>
	/// Swaps bytes to convert little-endian to big-endian and vice versa.
	/// </summary>
	/// <param name="value">Value to convert.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SwapBytes(ref short value)
	{
		value = (short)((value >> 8) | (value << 8)); // swap adjacent 8-bit blocks
	}

	/// <summary>
	/// Swaps bytes to convert little-endian to big-endian and vice versa.
	/// </summary>
	/// <param name="value">Value to convert.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SwapBytes(ref ushort value)
	{
		value = (ushort)((value >> 8) | (value << 8)); // swap adjacent 8-bit blocks
	}

	/// <summary>
	/// Swaps bytes to convert little-endian to big-endian and vice versa.
	/// </summary>
	/// <param name="value">Value to convert.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SwapBytes(ref char value)
	{
		value = (char)((value >> 8) | (value << 8)); // swap adjacent 8-bit blocks
	}

	/// <summary>
	/// Swaps bytes to convert little-endian to big-endian and vice versa.
	/// </summary>
	/// <param name="value">Value to convert.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SwapBytes(ref int value)
	{
		SwapBytes(ref Unsafe.As<int, uint>(ref value));
	}

	/// <summary>
	/// Swaps bytes to convert little-endian to big-endian and vice versa.
	/// </summary>
	/// <param name="value">Value to convert.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SwapBytes(ref uint value)
	{
		value = (value >> 16) | (value << 16);                             // swap adjacent 16-bit blocks
		value = ((value & 0xFF00FF00) >> 8) | ((value & 0x00FF00FF) << 8); // swap adjacent 8-bit blocks
	}

	/// <summary>
	/// Swaps bytes to convert little-endian to big-endian and vice versa.
	/// </summary>
	/// <param name="value">Value to convert.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SwapBytes(ref long value)
	{
		SwapBytes(ref Unsafe.As<long, ulong>(ref value));
	}

	/// <summary>
	/// Swaps bytes to convert little-endian to big-endian and vice versa.
	/// </summary>
	/// <param name="value">Value to convert.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SwapBytes(ref ulong value)
	{
		value = (value >> 32) | (value << 32);                                               // swap adjacent 32-bit blocks
		value = ((value & 0xFFFF0000FFFF0000) >> 16) | ((value & 0x0000FFFF0000FFFF) << 16); // swap adjacent 16-bit blocks
		value = ((value & 0xFF00FF00FF00FF00) >> 8) | ((value & 0x00FF00FF00FF00FF) << 8);   // swap adjacent 8-bit blocks
	}

	/// <summary>
	/// Swaps bytes to convert little-endian to big-endian and vice versa.
	/// </summary>
	/// <param name="value">Value to convert.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SwapBytes(ref float value)
	{
		ref uint x = ref Unsafe.As<float, uint>(ref value);
		SwapBytes(ref x);
	}

	/// <summary>
	/// Swaps bytes to convert little-endian to big-endian and vice versa.
	/// </summary>
	/// <param name="value">Value to convert.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SwapBytes(ref double value)
	{
		ref ulong x = ref Unsafe.As<double, ulong>(ref value);
		SwapBytes(ref x);
	}
}
