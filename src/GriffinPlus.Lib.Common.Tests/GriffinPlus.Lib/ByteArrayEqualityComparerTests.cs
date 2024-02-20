///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Xunit;

namespace GriffinPlus.Lib;

/// <summary>
/// Unit tests targeting the <see cref="ByteArrayEqualityComparer"/> class.
/// </summary>
public class ByteArrayEqualityComparerTests
{
	#region Creation

	[Fact]
	public void Create()
	{
		_ = new ByteArrayEqualityComparer();
	}

	#endregion

	#region Static: Instance

	[Fact]
	public void Instance()
	{
		var instance1 = ByteArrayEqualityComparer.Instance;
		var instance2 = ByteArrayEqualityComparer.Instance;
		Assert.NotNull(instance1);
		Assert.Same(instance1, instance2);
	}

	#endregion

	#region Static: AreEqual()

	public static IEnumerable<object[]> AreEqual_TestData
	{
		get
		{
			// test passing null references
			byte[] data = Array.Empty<byte>();
			yield return [null, null, true];
			yield return [null, data, false];
			yield return [data, null, false];

			// equality
			// (test up to 16 bytes to check splitting up the data in 8/4/2/1 bytes
			for (int i = 1; i <= 16; i++)
			{
				byte[] data1 = GetConsecutiveByteSequence(i);
				byte[] data2 = GetConsecutiveByteSequence(i);
				yield return [data1, data2, true];
			}

			// no equality
			// different lengths, but overlapping sequence equals
			for (int i = 1; i <= 16; i++)
			{
				byte[] data1 = GetConsecutiveByteSequence(i);
				byte[] data2 = GetConsecutiveByteSequence(16 - i);
				if (data1.Length != data2.Length) yield return [data1, data2, false];
			}

			// no equality
			// same length, but data is different
			for (int i = 1; i <= 16; i++)
			{
				byte[] data1 = GetConsecutiveByteSequence(i);
				byte[] data2 = GetConsecutiveByteSequence(i);
				data2[0] = 0xFF;
				yield return [data1, data2, false];
			}
		}
	}

	[Theory]
	[MemberData(nameof(AreEqual_TestData))]
	public void AreEqual_Span_Aligned(byte[] array1, byte[] array2, bool expected)
	{
		bool areEqual = ByteArrayEqualityComparer.AreEqual(array1.AsSpan(), array2.AsSpan());
		Assert.Equal(expected, areEqual);
	}

	[Fact]
	public void AreEqual_Span_Unaligned()
	{
		const int length = 32;
		byte[] data1 = new byte[length];
		byte[] data2 = new byte[length];
		byte[] data3 = new byte[length];

		for (int i = 0; i < 8; i++)
		{
			// span1 is aligned to an 8 byte boundary
			// span2 and span3 are shifted byte by byte (same shift and content, but different objects)
			for (int j = 0; j < length; j++) data1[j] = (byte)j;
			for (int j = i; j < length; j++) data2[j] = (byte)(j - i);
			for (int j = i; j < length; j++) data3[j] = (byte)(j - i);
			var span1 = new ReadOnlySpan<byte>(data1, 0, 16);
			var span2 = new ReadOnlySpan<byte>(data2, i, 16);
			var span3 = new ReadOnlySpan<byte>(data2, i, 16);

			// test span1 (aligned) compared with span2 (unaligned)
			bool areEqual1 = ByteArrayEqualityComparer.AreEqual(span1, span2);
			Assert.True(areEqual1);

			// test span2 (unaligned) compared with span1 (aligned)
			bool areEqual2 = ByteArrayEqualityComparer.AreEqual(span2, span1);
			Assert.True(areEqual2);

			// test span2 compared with span3 (both unaligned)
			bool areEqual3 = ByteArrayEqualityComparer.AreEqual(span2, span3);
			Assert.True(areEqual3);
		}
	}

	#endregion

	#region Static: GetHashCode()

	public static IEnumerable<object[]> GetHashCode_TestData
	{
		get
		{
			// equality
			yield return [GetConsecutiveByteSequence(0), -2128831035];
			yield return [GetConsecutiveByteSequence(1), 84696351];
			yield return [GetConsecutiveByteSequence(2), 276207162];
			yield return [GetConsecutiveByteSequence(3), 581859880];
			yield return [GetConsecutiveByteSequence(4), -1012248143];
			yield return [GetConsecutiveByteSequence(5), -1172398097];
			yield return [GetConsecutiveByteSequence(6), -399131298];
			yield return [GetConsecutiveByteSequence(7), -459730552];
			yield return [GetConsecutiveByteSequence(8), 1811325981];
		}
	}

	[Theory]
	[MemberData(nameof(GetHashCode_TestData))]
	public void GetHashCode_Span(byte[] array, int expected)
	{
		int hashCode = ByteArrayEqualityComparer.GetHashCode(array.AsSpan());
		Assert.Equal(expected, hashCode);
	}

	[Fact]
	public void GetHashCode_Span_ArgumentNull()
	{
		byte[] array = null;
		// ReSharper disable once ExpressionIsAlwaysNull
		var exception = Assert.Throws<ArgumentNullException>(() => ByteArrayEqualityComparer.GetHashCode(array.AsSpan()));
		Assert.Equal("data", exception.ParamName);
	}

	#endregion

	#region IEqualityComparer<byte[]>.Equals()

	[Theory]
	[MemberData(nameof(AreEqual_TestData))]
	public void IEqualityComparer_Equals(byte[] array1, byte[] array2, bool expected)
	{
		IEqualityComparer<byte[]> comparer = new ByteArrayEqualityComparer();
		bool areEqual = comparer.Equals(array1, array2);
		Assert.Equal(expected, areEqual);
	}

	#endregion

	#region IEqualityComparer<byte[]>.GetHashCode()

	[Theory]
	[MemberData(nameof(GetHashCode_TestData))]
	public void IEqualityComparer_GetHashCode(byte[] array, int expected)
	{
		IEqualityComparer<byte[]> comparer = new ByteArrayEqualityComparer();
		int hashCode = comparer.GetHashCode(array);
		Assert.Equal(expected, hashCode);
	}

	[Fact]
	public void IEqualityComparer_GetHashCode_ArgumentNull()
	{
		byte[] array = null;
		IEqualityComparer<byte[]> comparer = new ByteArrayEqualityComparer();
		// ReSharper disable once AssignNullToNotNullAttribute
		var exception = Assert.Throws<ArgumentNullException>(() => comparer.GetHashCode(array));
		Assert.Equal("obj", exception.ParamName);
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Gets a consecutive sequence of bytes with the specified length.
	/// </summary>
	/// <param name="length">Length of the sequence to create.</param>
	/// <returns>The requested sequence of bytes.</returns>
	private static byte[] GetConsecutiveByteSequence(int length)
	{
		byte[] data = new byte[length];
		for (int i = 0; i < length; i++) data[i] = (byte)i;
		return data;
	}

	#endregion
}
