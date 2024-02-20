///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace GriffinPlus.Lib.Imaging;

/// <summary>
/// Unit tests targeting the <see cref="BitmapPalette"/> class.
/// </summary>
public class BitmapPaletteTests
{
	#region Test Data

	/// <summary>
	/// Test data for methods that only need one palette.
	/// </summary>
	public static IEnumerable<object[]> TestData_Palettes
	{
		get
		{
			//yield return [BitmapPalettes.BlackAndWhite];
			//yield return [BitmapPalettes.Halftone8];
			//yield return [BitmapPalettes.Halftone27];
			//yield return [BitmapPalettes.Halftone64];
			//yield return [BitmapPalettes.Halftone125];
			//yield return [BitmapPalettes.Halftone216];
			//yield return [BitmapPalettes.Halftone252];
			//yield return [BitmapPalettes.Halftone256];
			//yield return [BitmapPalettes.Gray4];
			//yield return [BitmapPalettes.Gray16];
			yield return [BitmapPalettes.Gray256];
			yield return [BitmapPalettes.WebPalette];
		}
	}

	/// <summary>
	/// Test data for methods that cannot handle null references.
	/// </summary>
	public static IEnumerable<object[]> TestData_Equals_NonNullOnly
	{
		get
		{
			return
				from palette1 in TestData_Palettes.Select(x => x[0])
				from palette2 in TestData_Palettes.Select(x => x[0])
				select (object[])
				[
					palette1,
					palette2,
					ReferenceEquals(palette1, palette2)
				];
		}
	}

	/// <summary>
	/// Test data for methods checking equality and support <c>null</c> as the
	/// first and the second palette argument.
	/// </summary>
	public static IEnumerable<object[]> TestData_Equals_FirstAndSecondWithNull
	{
		get
		{
			foreach (object[] data in TestData_Equals_NonNullOnly)
			{
				yield return data;
			}

			BitmapPalette palette = BitmapPalettes.Gray256;
			yield return [palette, null, false];
			yield return [null, palette, false];
			yield return [null, null, true];
		}
	}

	/// <summary>
	/// Test data for methods checking equality and support <c>null</c> as the
	/// second palette argument only.
	/// </summary>
	public static IEnumerable<object[]> TestData_Equals_SecondWithNull
	{
		get
		{
			foreach (object[] data in TestData_Equals_NonNullOnly)
			{
				yield return data;
			}

			BitmapPalette palette = BitmapPalettes.Gray256;
			yield return [palette, null, false];
		}
	}

	#endregion

	#region BitmapPalette(IList<Color> colors)

	/// <summary>
	/// Tests creating an instance of the <see cref="BitmapPalette"/> class using the <see cref="BitmapPalette(IList{Color})"/> constructor.
	/// </summary>
	[Fact]
	public void BitmapPalette_WithColorList()
	{
		// for simplicity, take the color list of a predefined palette
		BitmapPalette expected = BitmapPalettes.Gray256;
		IList<Color> colors = expected.Colors.ToList();
		var palette = new BitmapPalette(colors);
		Assert.Equal(colors, palette.Colors);
	}

	/// <summary>
	/// Tests creating an instance of the <see cref="BitmapPalette"/> class using the <see cref="BitmapPalette(IList{Color})"/> constructor.
	/// The constructor should throw an exception if the list is <c>null</c>.
	/// </summary>
	[Fact]
	public void BitmapPalette_WithColorList_ListIsNull()
	{
		var exception = Assert.Throws<ArgumentNullException>(() => new BitmapPalette((IList<Color>)null));
		Assert.Equal("colors", exception.ParamName);
	}

	/// <summary>
	/// Tests creating an instance of the <see cref="BitmapPalette"/> class using the <see cref="BitmapPalette(IList{Color})"/> constructor.
	/// The constructor should throw an exception if the list is empty.
	/// </summary>
	[Fact]
	public void BitmapPalette_WithColorList_ListIsEmpty()
	{
		List<Color> list = Enumerable.Repeat(Color.FromUInt32(0), 0).ToList();
		var exception = Assert.Throws<ArgumentException>(() => new BitmapPalette(list));
		Assert.Equal("colors", exception.ParamName);
	}

	/// <summary>
	/// Tests creating an instance of the <see cref="BitmapPalette"/> class using the <see cref="BitmapPalette(IList{Color})"/> constructor.
	/// The constructor should throw an exception if the list specifies too many colors.
	/// </summary>
	[Fact]
	public void BitmapPalette_WithColorList_ListIsTooLarge()
	{
		List<Color> list = Enumerable.Repeat(Color.FromUInt32(0), 257).ToList();
		var exception = Assert.Throws<ArgumentException>(() => new BitmapPalette(list));
		Assert.Equal("colors", exception.ParamName);
	}

	#endregion

	#region bool Equals(BitmapPalette palette1, BitmapPalette palette2)

	/// <summary>
	/// Tests the <see cref="BitmapPalette.Equals(BitmapPalette,BitmapPalette)"/> method.
	/// </summary>
	/// <param name="palette1">First bitmap palette to test with.</param>
	/// <param name="palette2">Second bitmap palette to test with.</param>
	/// <param name="areEqual">
	/// <c>true</c> if <paramref name="palette1"/> and <paramref name="palette2"/> are expected to be equal;
	/// otherwise <c>false</c>.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Equals_FirstAndSecondWithNull))]
	public void Equals_Static(BitmapPalette palette1, BitmapPalette palette2, bool areEqual)
	{
		Assert.Equal(areEqual, BitmapPalette.Equals(palette1, palette2));
	}

	#endregion

	#region bool Equals(object obj)

	/// <summary>
	/// Tests the <see cref="BitmapPalette.Equals(object)"/> method.
	/// </summary>
	/// <param name="palette1">First bitmap palette to test with.</param>
	/// <param name="palette2">Second bitmap palette to test with.</param>
	/// <param name="areEqual">
	/// <c>true</c> if <paramref name="palette1"/> and <paramref name="palette2"/> are expected to be equal;
	/// otherwise <c>false</c>.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Equals_SecondWithNull))]
	public void Equals_Object(BitmapPalette palette1, BitmapPalette palette2, bool areEqual)
	{
		Assert.Equal(areEqual, palette1.Equals((object)palette2));
	}

	#endregion

	#region bool Equals(BitmapPalette palette)

	/// <summary>
	/// Tests the <see cref="BitmapPalette.Equals(BitmapPalette)"/> method.
	/// </summary>
	/// <param name="palette1">First bitmap palette to test with.</param>
	/// <param name="palette2">Second bitmap palette to test with.</param>
	/// <param name="areEqual">
	/// <c>true</c> if <paramref name="palette1"/> and <paramref name="palette2"/> are expected to be equal;
	/// otherwise <c>false</c>.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Equals_SecondWithNull))]
	public void Equals_BitmapPalette(BitmapPalette palette1, BitmapPalette palette2, bool areEqual)
	{
		Assert.Equal(areEqual, palette1.Equals(palette2));
	}

	#endregion

	#region bool operator== (BitmapPalette left, BitmapPalette right)

	/// <summary>
	/// Tests the <see cref="BitmapPalette.operator==(BitmapPalette,BitmapPalette)"/> method.
	/// </summary>
	/// <param name="palette1">First bitmap palette to test with.</param>
	/// <param name="palette2">Second bitmap palette to test with.</param>
	/// <param name="areEqual">
	/// <c>true</c> if <paramref name="palette1"/> and <paramref name="palette2"/> are expected to be equal;
	/// otherwise <c>false</c>.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Equals_SecondWithNull))]
	public void EqualityOperator(BitmapPalette palette1, BitmapPalette palette2, bool areEqual)
	{
		Assert.Equal(areEqual, palette1 == palette2);
	}

	#endregion

	#region bool operator!= (BitmapPalette left, BitmapPalette right)

	/// <summary>
	/// Tests the <see cref="BitmapPalette.operator!=(BitmapPalette,BitmapPalette)"/> method.
	/// </summary>
	/// <param name="palette1">First bitmap palette to test with.</param>
	/// <param name="palette2">Second bitmap palette to test with.</param>
	/// <param name="areEqual">
	/// <c>true</c> if <paramref name="palette1"/> and <paramref name="palette2"/> are expected to be equal;
	/// otherwise <c>false</c>.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Equals_SecondWithNull))]
	public void InequalityOperator(BitmapPalette palette1, BitmapPalette palette2, bool areEqual)
	{
		Assert.Equal(!areEqual, palette1 != palette2);
	}

	#endregion

	#region int GetHashCode()

	/// <summary>
	/// Tests the <see cref="BitmapPalette.GetHashCode()"/> method.
	/// </summary>
	/// <param name="palette1">First bitmap palette to test with.</param>
	/// <param name="palette2">Second bitmap palette to test with.</param>
	/// <param name="areEqual">
	/// <c>true</c> if <paramref name="palette1"/> and <paramref name="palette2"/> are expected to be equal;
	/// otherwise <c>false</c>.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Equals_NonNullOnly))]
	public void GetHashCode_(BitmapPalette palette1, BitmapPalette palette2, bool areEqual)
	{
		// different palettes should return a different hash code
		// (theoretically there can be collisions, but these should be rare, especially in the small test data set)
		Assert.Equal(areEqual, palette1.GetHashCode() == palette2.GetHashCode());
	}

	#endregion
}
