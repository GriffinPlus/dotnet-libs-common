///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

// ReSharper disable ForCanBeConvertedToForeach

namespace GriffinPlus.Lib
{

	/// <summary>
	/// An equality comparer for comparing the contents of a byte array (thread-safe).
	/// The class also provides static methods for comparing using spans.
	/// </summary>
	public class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
	{
		/// <summary>
		/// Gets the <see cref="ByteArrayEqualityComparer"/> instance.
		/// </summary>
		public static readonly ByteArrayEqualityComparer Instance = new();

		/// <summary>
		/// Determines whether the specified spans of byte are equal.
		/// </summary>
		/// <param name="x">The first span to compare.</param>
		/// <param name="y">The second span to compare.</param>
		/// <returns>
		/// <c>true</c>true if the specified spans are equal; otherwise <c>false</c>.
		/// </returns>
		public static unsafe bool AreEqual(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
		{
			if (x == y)
				return true;

			if (x == null || y == null || x.Length != y.Length)
				return false;

			fixed (byte* px_start = x)
			fixed (byte* py_start = y)
			{
				byte* px = px_start;
				byte* py = py_start;
				int len = x.Length;

				if (((ulong)px & 0x7) == 0 && ((ulong)py & 0x7) == 0)
				{
					// both pointers are aligned to an 8 byte boundary
					// => we can compare multiple bytes at once

					// compare 64-bit
					for (int i = 0; i < len / 8; i++, px += 8, py += 8)
					{
						if (*(long*)px != *(long*)py) return false;
					}

					// compare 32-bit
					if ((len & 4) != 0)
					{
						if (*(int*)px != *(int*)py) return false;
						px += 4;
						py += 4;
					}

					// compare 16-bit
					if ((len & 2) != 0)
					{
						if (*(short*)px != *(short*)py) return false;
						px += 2;
						py += 2;
					}

					// compare 8-bit
					if ((len & 1) != 0)
					{
						if (*px != *py) return false;
					}
				}
				else if (((ulong)px & 0x3) == 0 && ((ulong)py & 0x3) == 0)
				{
					// both pointers are aligned to an 4 byte boundary
					// => we can compare multiple bytes at once

					// compare 32-bit
					for (int i = 0; i < len / 4; i++, px += 4, py += 4)
					{
						if (*(int*)px != *(int*)py) return false;
					}

					// compare 16-bit
					if ((len & 2) != 0)
					{
						if (*(short*)px != *(short*)py) return false;
						px += 2;
						py += 2;
					}

					// compare 8-bit
					if ((len & 1) != 0)
					{
						if (*px != *py) return false;
					}
				}
				else if (((ulong)px & 0x1) == 0 && ((ulong)py & 0x1) == 0)
				{
					// both pointers are aligned to an 2 byte boundary
					// => we can compare multiple bytes at once

					// compare 16-bit
					for (int i = 0; i < len / 2; i++, px += 2, py += 2)
					{
						if (*(short*)px != *(short*)py) return false;
					}

					// compare 8-bit
					if ((len & 1) != 0)
					{
						if (*px != *py) return false;
					}
				}
				else
				{
					// fallback

					// compare 8-bit
					byte* pEndX = px + len;
					while (px != pEndX)
					{
						if (*px++ != *py++) return false;
					}
				}

				return true;
			}
		}

		/// <summary>
		/// Gets a hash code for the specified span of bytes.
		/// </summary>
		/// <param name="data">The span of bytes to calculate a hash code for.</param>
		/// <returns>The hash code for the specified span of bytes.</returns>
		/// <exception cref="System.ArgumentNullException"><paramref name="data"/> is null.</exception>
		public static int GetHashCode(ReadOnlySpan<byte> data)
		{
			if (data == null) throw new ArgumentNullException(nameof(data));

			// hash function: FNV-1a (Fowler–Noll–Vo hash function)
			// https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
			unchecked
			{
				const int p = 16777619;
				int hash = (int)2166136261;

				// abort, if the span is null
				if (data == null) return p; // could be any number

				for (int i = 0; i < data.Length; i++)
				{
					hash ^= data[i];
					hash *= p;
				}

				return hash;
			}
		}

		#region Explicit Implementation of IEqualityComparer<byte[]>

		/// <summary>
		/// Determines whether the specified byte arrays are equal.
		/// </summary>
		/// <param name="x">The first byte array to compare.</param>
		/// <param name="y">The second byte array to compare.</param>
		/// <returns>
		/// <c>true</c>true if the specified byte arrays are equal;
		/// otherwise <c>false</c>false.
		/// </returns>
		bool IEqualityComparer<byte[]>.Equals(byte[] x, byte[] y)
		{
			return AreEqual(x, y);
		}

		/// <summary>
		/// Returns a hash code for the specified byte array.
		/// </summary>
		/// <param name="obj">The byte array to calculate a hash code for.</param>
		/// <returns>The hash code for the specified byte array.</returns>
		/// <exception cref="System.ArgumentNullException"><paramref name="obj"/> is null.</exception>
		int IEqualityComparer<byte[]>.GetHashCode(byte[] obj)
		{
			if (obj == null) throw new ArgumentNullException(nameof(obj));
			return GetHashCode(obj);
		}

		#endregion
	}

}
