///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;

using Xunit;

namespace GriffinPlus.Lib.Imaging;

/// <summary>
/// Unit tests targeting the <see cref="Color"/> class.
/// </summary>
public class ColorTests
{
	#region Test Data

	/// <summary>
	/// Color test data (sRGB, with alpha).
	/// </summary>
	public static IEnumerable<object[]> TestData_Argb
	{
		get
		{
			yield return [0xFF000000]; // opaque black
			yield return [0xFF000080]; // opaque half blue
			yield return [0xFF0000FF]; // opaque full blue
			yield return [0xFF008000]; // opaque half green
			yield return [0xFF00FF00]; // opaque full green
			yield return [0xFF800000]; // opaque half red
			yield return [0xFFFF0000]; // opaque full red
			yield return [0x00000000]; // full transparent black
			yield return [0x80000000]; // half transparent black
		}
	}

	/// <summary>
	/// Color test data (sRGB, without alpha).
	/// </summary>
	public static IEnumerable<object[]> TestData_Rgb
	{
		get
		{
			yield return [0xFF000000]; // opaque black
			yield return [0xFF000080]; // opaque half blue
			yield return [0xFF0000FF]; // opaque full blue
			yield return [0xFF008000]; // opaque half green
			yield return [0xFF00FF00]; // opaque full green
			yield return [0xFF800000]; // opaque half red
			yield return [0xFFFF0000]; // opaque full red
		}
	}

	/// <summary>
	/// Color test data (scRGB, with alpha).
	/// </summary>
	public static IEnumerable<object[]> TestData_ScRgb
	{
		get
		{
			yield return [0.0f, 0.0f, 0.0f, 0.0f]; // minimum

			yield return [-0.1f, 0.0f, 0.0f, 0.0f]; // minimum alpha value exceeded, clipped for SRGB calculation
			yield return [1.1f, 0.0f, 0.0f, 0.0f];  // maximum alpha value exceeded, clipped for SRGB calculation

			yield return [0.0f, 0031308f, 0031308f, 0031308f]; // value == 0.0031308f, special case for SRGB calculation
			yield return [0.0f, 1.1f, 1.1f, 1.1f];             // maximum RGB values exceeded, clipped for SRGB calculation

			yield return [1.0f, 1.0f, 1.0f, 1.0f]; // maximum
		}
	}

	/// <summary>
	/// Color test data (scRGB) for tests checking modifications of a <see cref="Color"/> instance.
	/// </summary>
	public static IEnumerable<object[]> TestData_ScRgb_Change
	{
		get
		{
			yield return
			[
				0.0f, 0.0f, 0.0f, 0.0f, // ScARGB (minimum)
				0.5f, 0.5f, 0.5f, 0.5f  // ScARGB (middle)
			];

			yield return
			[
				0.0f, 0.0f, 0.0f, 0.0f, // ScARGB (minimum)
				1.0f, 1.0f, 1.0f, 1.0f  // ScARGB (maximum)
			];

			yield return
			[
				0.0f, 0.0f, 0.0f, 0.0f, // ScARGB (minimum)
				-0.1f, 0.0f, 0.0f, 0.0f // ScARGB (minimum alpha value exceeded, clipped for SRGB calculation)
			];

			yield return
			[
				0.0f, 0.0f, 0.0f, 0.0f, // ScARGB (minimum)
				1.1f, 0.0f, 0.0f, 0.0f  // ScARGB (maximum alpha value exceeded, clipped for SRGB calculation)
			];

			yield return
			[
				0.0f, 0.0f, 0.0f, 0.0f,                  // ScARGB (minimum)
				0.0f, 0.0031308f, 0.0031308f, 0.0031308f // ScARGB (value == 0.0031308f, special case for SRGB calculation)
			];

			yield return
			[
				0.0f, 0.0f, 0.0f, 0.0f, // ScARGB (minimum)
				0.0f, 1.1f, 1.1f, 1.1f  // ScARGB (maximum ScRGB values exceeded, values clipped for SRGB calculation)
			];
		}
	}

	/// <summary>
	/// Color test data (sRGB, without alpha).
	/// </summary>
	public static IEnumerable<object[]> TestData_Equals
	{
		get
		{
			uint[] colors =
			[
				0xFF000000, // opaque black
				0xFF0000FF, // opaque full blue
				0xFF00FF00, // opaque full green
				0xFFFF0000, // opaque full red
				0x00000000, // transparent black
				0x000000FF, // transparent full blue
				0x0000FF00, // transparent full green
				0x00FF0000  // transparent full red
			];

			foreach (uint argb1 in colors)
			foreach (uint argb2 in colors)
			{
				yield return
				[
					Color.FromUInt32(argb1),
					Color.FromUInt32(argb2),
					argb1 == argb2
				];
			}
		}
	}

	#endregion

	#region byte [A, R, G, B] { get; set; }

	/// <summary>
	/// Tests getting and setting the properties <see cref="Color.A"/>, <see cref="Color.R"/>, <see cref="Color.G"/> and <see cref="Color.B"/>.
	/// </summary>
	/// <param name="fromArgb">The initial ARGB value to start with.</param>
	/// <param name="toArgb">ARGB value to set the color to.</param>
	[Theory]
	[InlineData(0x11223344u, 0x55667788u)]
	public void ARGB_GetAndSet(uint fromArgb, uint toArgb)
	{
		byte fromSrgbA = (byte)((fromArgb & 0xFF000000U) >> 24);
		byte fromSRgbR = (byte)((fromArgb & 0x00FF0000U) >> 16);
		byte fromSRgbG = (byte)((fromArgb & 0x0000FF00U) >> 8);
		byte fromSRgbB = (byte)(fromArgb & 0x000000FF);

		float fromScRgbA = fromSrgbA / 255.0f;
		float fromScRgbR = SRgbToScRgb(fromSRgbR);
		float fromScRgbG = SRgbToScRgb(fromSRgbG);
		float fromScRgbB = SRgbToScRgb(fromSRgbB);

		byte toSRgbA = (byte)((toArgb & 0xFF000000U) >> 24);
		byte toSRgbR = (byte)((toArgb & 0x00FF0000U) >> 16);
		byte toSRgbG = (byte)((toArgb & 0x0000FF00U) >> 8);
		byte toSRgbB = (byte)(toArgb & 0x000000FF);

		float toScRgbA = toSRgbA / 255.0f;
		float toScRgbR = SRgbToScRgb(toSRgbR);
		float toScRgbG = SRgbToScRgb(toSRgbG);
		float toScRgbB = SRgbToScRgb(toSRgbB);

		// create the color to start with
		Color color = Color.FromUInt32(fromArgb);

		// check whether the properties A, R, G and B reflect the expected channel values
		Assert.Equal(fromSrgbA, color.A);
		Assert.Equal(fromSRgbR, color.R);
		Assert.Equal(fromSRgbG, color.G);
		Assert.Equal(fromSRgbB, color.B);
		Assert.Equal(fromScRgbA, color.ScA);
		Assert.Equal(fromScRgbR, color.ScR);
		Assert.Equal(fromScRgbG, color.ScG);
		Assert.Equal(fromScRgbB, color.ScB);

		// set property A and check whether the change is reflected as expected
		color.A = toSRgbA;
		Assert.Equal(toSRgbA, color.A);
		Assert.Equal(fromSRgbR, color.R);
		Assert.Equal(fromSRgbG, color.G);
		Assert.Equal(fromSRgbB, color.B);
		Assert.Equal(toScRgbA, color.ScA);
		Assert.Equal(fromScRgbR, color.ScR);
		Assert.Equal(fromScRgbG, color.ScG);
		Assert.Equal(fromScRgbB, color.ScB);

		// set property R and check whether the change is reflected as expected
		color.R = toSRgbR;
		Assert.Equal(toSRgbA, color.A);
		Assert.Equal(toSRgbR, color.R);
		Assert.Equal(fromSRgbG, color.G);
		Assert.Equal(fromSRgbB, color.B);
		Assert.Equal(toScRgbA, color.ScA);
		Assert.Equal(toScRgbR, color.ScR);
		Assert.Equal(fromScRgbG, color.ScG);
		Assert.Equal(fromScRgbB, color.ScB);

		// set property G and check whether the change is reflected as expected
		color.G = toSRgbG;
		Assert.Equal(toSRgbA, color.A);
		Assert.Equal(toSRgbR, color.R);
		Assert.Equal(toSRgbG, color.G);
		Assert.Equal(fromSRgbB, color.B);
		Assert.Equal(toScRgbA, color.ScA);
		Assert.Equal(toScRgbR, color.ScR);
		Assert.Equal(toScRgbG, color.ScG);
		Assert.Equal(fromScRgbB, color.ScB);

		// set property B and check whether the change is reflected as expected
		color.B = toSRgbB;
		Assert.Equal(toSRgbA, color.A);
		Assert.Equal(toSRgbR, color.R);
		Assert.Equal(toSRgbG, color.G);
		Assert.Equal(toSRgbB, color.B);
		Assert.Equal(toScRgbA, color.ScA);
		Assert.Equal(toScRgbR, color.ScR);
		Assert.Equal(toScRgbG, color.ScG);
		Assert.Equal(toScRgbB, color.ScB);
	}

	#endregion

	#region float [ScA, ScR, ScG, ScB] { get; set; }

	/// <summary>
	/// Tests getting and setting the properties <see cref="Color.ScA"/>, <see cref="Color.ScR"/>, <see cref="Color.ScG"/> and <see cref="Color.ScB"/>.
	/// </summary>
	/// <param name="fromScRgbA">Initial value of the scRGB alpha channel.</param>
	/// <param name="fromScRgbR">Initial value of the scRGB red channel.</param>
	/// <param name="fromScRgbG">Initial value of the scRGB green channel.</param>
	/// <param name="fromScRgbB">Initial value of the scRGB blue channel.</param>
	/// <param name="toScRgbA">Value of the scRGB alpha channel to set.</param>
	/// <param name="toScRgbR">Value of the scRGB red channel to set.</param>
	/// <param name="toScRgbG">Value of the scRGB green channel to set.</param>
	/// <param name="toScRgbB">Value of the scRGB blue channel to set.</param>
	[Theory]
	[MemberData(nameof(TestData_ScRgb_Change))]
	public void ScARGB_GetAndSet(
		float fromScRgbA,
		float fromScRgbR,
		float fromScRgbG,
		float fromScRgbB,
		float toScRgbA,
		float toScRgbR,
		float toScRgbG,
		float toScRgbB)
	{
		byte fromSRgbA = ScAlphaTosRgb(fromScRgbA);
		byte fromSRgbR = ScRgbTosRgb(fromScRgbR);
		byte fromSRgbG = ScRgbTosRgb(fromScRgbG);
		byte fromSRgbB = ScRgbTosRgb(fromScRgbB);

		byte toSRgbA = ScAlphaTosRgb(toScRgbA);
		byte toSRgbR = ScRgbTosRgb(toScRgbR);
		byte toSRgbG = ScRgbTosRgb(toScRgbG);
		byte toSRgbB = ScRgbTosRgb(toScRgbB);

		// create the color to start with
		Color color = Color.FromScRgb(fromScRgbA, fromScRgbR, fromScRgbG, fromScRgbB);

		// check whether the properties A, R, G and B reflect the expected channel values
		Assert.Equal(fromSRgbA, color.A);
		Assert.Equal(fromSRgbR, color.R);
		Assert.Equal(fromSRgbG, color.G);
		Assert.Equal(fromSRgbB, color.B);
		Assert.Equal(fromScRgbA, color.ScA);
		Assert.Equal(fromScRgbR, color.ScR);
		Assert.Equal(fromScRgbG, color.ScG);
		Assert.Equal(fromScRgbB, color.ScB);

		// set property A and check whether the change is reflected as expected
		color.ScA = toScRgbA;
		Assert.Equal(toSRgbA, color.A);
		Assert.Equal(fromSRgbR, color.R);
		Assert.Equal(fromSRgbG, color.G);
		Assert.Equal(fromSRgbB, color.B);
		Assert.Equal(toScRgbA, color.ScA);
		Assert.Equal(fromScRgbR, color.ScR);
		Assert.Equal(fromScRgbG, color.ScG);
		Assert.Equal(fromScRgbB, color.ScB);

		// set property R and check whether the change is reflected as expected
		color.ScR = toScRgbR;
		Assert.Equal(toSRgbA, color.A);
		Assert.Equal(toSRgbR, color.R);
		Assert.Equal(fromSRgbG, color.G);
		Assert.Equal(fromSRgbB, color.B);
		Assert.Equal(toScRgbA, color.ScA);
		Assert.Equal(toScRgbR, color.ScR);
		Assert.Equal(fromScRgbG, color.ScG);
		Assert.Equal(fromScRgbB, color.ScB);

		// set property G and check whether the change is reflected as expected
		color.ScG = toScRgbG;
		Assert.Equal(toSRgbA, color.A);
		Assert.Equal(toSRgbR, color.R);
		Assert.Equal(toSRgbG, color.G);
		Assert.Equal(fromSRgbB, color.B);
		Assert.Equal(toScRgbA, color.ScA);
		Assert.Equal(toScRgbR, color.ScR);
		Assert.Equal(toScRgbG, color.ScG);
		Assert.Equal(fromScRgbB, color.ScB);

		// set property B and check whether the change is reflected as expected
		color.ScB = toScRgbB;
		Assert.Equal(toSRgbA, color.A);
		Assert.Equal(toSRgbR, color.R);
		Assert.Equal(toSRgbG, color.G);
		Assert.Equal(toSRgbB, color.B);
		Assert.Equal(toScRgbA, color.ScA);
		Assert.Equal(toScRgbR, color.ScR);
		Assert.Equal(toScRgbG, color.ScG);
		Assert.Equal(toScRgbB, color.ScB);
	}

	#endregion

	#region Color FromArgb(byte a, byte r, byte g, byte b)

	/// <summary>
	/// Tests the <see cref="Color.FromArgb"/> method.
	/// </summary>
	/// <param name="argb">Test sRGB color value (encoding: ARGB (MSB to LSB)).</param>
	[Theory]
	[MemberData(nameof(TestData_Argb))]
	public void FromArgb(uint argb)
	{
		byte argbA = (byte)((argb & 0xFF000000U) >> 24);
		byte argbR = (byte)((argb & 0x00FF0000U) >> 16);
		byte argbG = (byte)((argb & 0x0000FF00U) >> 8);
		byte argbB = (byte)(argb & 0x000000FF);

		Color color = Color.FromArgb(argbA, argbR, argbG, argbB);

		float scRgbA = argbA / 255.0f;
		float scRgbR = SRgbToScRgb(argbR);
		float scRgbG = SRgbToScRgb(argbG);
		float scRgbB = SRgbToScRgb(argbB);

		Assert.Equal(argbA, color.A);
		Assert.Equal(argbR, color.R);
		Assert.Equal(argbG, color.G);
		Assert.Equal(argbB, color.B);

		Assert.Equal(scRgbA, color.ScA);
		Assert.Equal(scRgbR, color.ScR);
		Assert.Equal(scRgbG, color.ScG);
		Assert.Equal(scRgbB, color.ScB);
	}

	#endregion

	#region Color FromRgb(byte r, byte g, byte b)

	/// <summary>
	/// Tests the <see cref="Color.FromRgb"/> method.
	/// </summary>
	/// <param name="argb">
	/// Test sRGB color value (encoding: ARGB (MSB to LSB)).
	/// The alpha channel is always 255 in the test data set.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Rgb))]
	public void FromRgb(uint argb)
	{
		byte argbA = (byte)((argb & 0xFF000000U) >> 24);
		byte argbR = (byte)((argb & 0x00FF0000U) >> 16);
		byte argbG = (byte)((argb & 0x0000FF00U) >> 8);
		byte argbB = (byte)(argb & 0x000000FF);

		Color color = Color.FromRgb(argbR, argbG, argbB);

		float scRgbA = argbA / 255.0f;
		float scRgbR = SRgbToScRgb(argbR);
		float scRgbG = SRgbToScRgb(argbG);
		float scRgbB = SRgbToScRgb(argbB);

		Assert.Equal(argbA, color.A);
		Assert.Equal(argbR, color.R);
		Assert.Equal(argbG, color.G);
		Assert.Equal(argbB, color.B);

		Assert.Equal(scRgbA, color.ScA);
		Assert.Equal(scRgbR, color.ScR);
		Assert.Equal(scRgbG, color.ScG);
		Assert.Equal(scRgbB, color.ScB);
	}

	#endregion

	#region Color FromScRgb(float a, float r, float g, float b)

	/// <summary>
	/// Tests the <see cref="Color.FromScRgb"/> method.
	/// </summary>
	/// <param name="scRgbA">scRGB alpha value.</param>
	/// <param name="scRgbR">scRGB red channel value.</param>
	/// <param name="scRgbG">scRGB green channel value.</param>
	/// <param name="scRgbB">scRGB blue channel value.</param>
	[Theory]
	[MemberData(nameof(TestData_ScRgb))]
	public void FromScRgb(
		float scRgbA,
		float scRgbR,
		float scRgbG,
		float scRgbB)
	{
		byte argbA = ScAlphaTosRgb(scRgbA);
		byte argbR = ScRgbTosRgb(scRgbR);
		byte argbG = ScRgbTosRgb(scRgbG);
		byte argbB = ScRgbTosRgb(scRgbB);

		Color color = Color.FromScRgb(scRgbA, scRgbR, scRgbG, scRgbB);

		Assert.Equal(argbA, color.A);
		Assert.Equal(argbR, color.R);
		Assert.Equal(argbG, color.G);
		Assert.Equal(argbB, color.B);

		Assert.Equal(scRgbA, color.ScA);
		Assert.Equal(scRgbR, color.ScR);
		Assert.Equal(scRgbG, color.ScG);
		Assert.Equal(scRgbB, color.ScB);
	}

	#endregion

	#region void Clamp()

	/// <summary>
	/// Tests the <see cref="Color.Clamp"/> method.
	/// </summary>
	/// <param name="scRgbA">The scRGB value of the alpha channel of the test color.</param>
	/// <param name="scRgbR">The scRGB value of the red channel of the test color.</param>
	/// <param name="scRgbG">The scRGB value of the green channel of the test color.</param>
	/// <param name="scRgbB">The scRGB value of the blue channel of the test color.</param>
	[Theory]
	[InlineData(0.0f, 0.0f, 0.0f, 0.0f)]                      // no clamping
	[InlineData(0.999999f, 0.999999f, 0.999999f, 0.999999f)]  // no clamping
	[InlineData(1.0f, 1.0f, 1.0f, 1.0f)]                      // no clamping
	[InlineData(1.0000001f, 1.0000001, 1.0000001, 1.0000001)] // clamping
	public void Clamp(
		float scRgbA,
		float scRgbR,
		float scRgbG,
		float scRgbB)
	{
		Color color = Color.FromScRgb(scRgbA, scRgbR, scRgbG, scRgbB);

		Assert.Equal(scRgbA, color.ScA);
		Assert.Equal(scRgbR, color.ScR);
		Assert.Equal(scRgbG, color.ScG);
		Assert.Equal(scRgbB, color.ScB);

		color.Clamp();

		Assert.Equal(Math.Min(scRgbA, 1.0f), color.ScA);
		Assert.Equal(Math.Min(scRgbR, 1.0f), color.ScR);
		Assert.Equal(Math.Min(scRgbG, 1.0f), color.ScG);
		Assert.Equal(Math.Min(scRgbB, 1.0f), color.ScB);

		Assert.Equal(ScAlphaTosRgb(color.ScA), color.A);
		Assert.Equal(ScRgbTosRgb(color.ScR), color.R);
		Assert.Equal(ScRgbTosRgb(color.ScG), color.G);
		Assert.Equal(ScRgbTosRgb(color.ScB), color.B);
	}

	#endregion

	#region bool Equals(Color color1, Color color2)

	/// <summary>
	/// Tests the <see cref="Color.Equals(Color,Color)"/> method.
	/// </summary>
	/// <param name="color1">First color to test with.</param>
	/// <param name="color2">Second color to test with.</param>
	/// <param name="areEqual">
	/// <c>true</c> if <paramref name="color1"/> and <paramref name="color2"/> are expected to be equal;
	/// otherwise <c>false</c>.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Equals))]
	public void Equals_Static(Color color1, Color color2, bool areEqual)
	{
		Assert.Equal(areEqual, Color.Equals(color1, color2));
	}

	#endregion

	#region bool Equals(object obj)

	/// <summary>
	/// Tests the <see cref="Color.Equals(object)"/> method.
	/// </summary>
	/// <param name="color1">First color to test with.</param>
	/// <param name="color2">Second color to test with.</param>
	/// <param name="areEqual">
	/// <c>true</c> if <paramref name="color1"/> and <paramref name="color2"/> are expected to be equal;
	/// otherwise <c>false</c>.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Equals))]
	public void Equals_Object(Color color1, Color color2, bool areEqual)
	{
		Assert.Equal(areEqual, color1.Equals((object)color2));
	}

	#endregion

	#region bool Equals(Color color)

	/// <summary>
	/// Tests the <see cref="Color.Equals(Color)"/> method.
	/// </summary>
	/// <param name="color1">First color to test with.</param>
	/// <param name="color2">Second color to test with.</param>
	/// <param name="areEqual">
	/// <c>true</c> if <paramref name="color1"/> and <paramref name="color2"/> are expected to be equal;
	/// otherwise <c>false</c>.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Equals))]
	public void Equals_Color(Color color1, Color color2, bool areEqual)
	{
		Assert.Equal(areEqual, color1.Equals(color2));
	}

	#endregion

	#region bool operator== (Color left, Color right)

	/// <summary>
	/// Tests the <see cref="Color.operator==(Color,Color)"/> method.
	/// </summary>
	/// <param name="color1">First color to test with.</param>
	/// <param name="color2">Second color to test with.</param>
	/// <param name="areEqual">
	/// <c>true</c> if <paramref name="color1"/> and <paramref name="color2"/> are expected to be equal;
	/// otherwise <c>false</c>.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Equals))]
	public void EqualityOperator(Color color1, Color color2, bool areEqual)
	{
		Assert.Equal(areEqual, color1 == color2);
	}

	#endregion

	#region bool operator!= (Color left, Color right)

	/// <summary>
	/// Tests the <see cref="Color.operator!=(Color,Color)"/> method.
	/// </summary>
	/// <param name="color1">First color to test with.</param>
	/// <param name="color2">Second color to test with.</param>
	/// <param name="areEqual">
	/// <c>true</c> if <paramref name="color1"/> and <paramref name="color2"/> are expected to be equal;
	/// otherwise <c>false</c>.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Equals))]
	public void InequalityOperator(Color color1, Color color2, bool areEqual)
	{
		Assert.Equal(!areEqual, color1 != color2);
	}

	#endregion

	#region int GetHashCode()

	/// <summary>
	/// Tests the <see cref="Color.GetHashCode()"/> method.
	/// </summary>
	/// <param name="color1">First color to test with.</param>
	/// <param name="color2">Second color to test with.</param>
	/// <param name="areEqual">
	/// <c>true</c> if <paramref name="color1"/> and <paramref name="color2"/> are expected to be equal;
	/// otherwise <c>false</c>.
	/// </param>
	[Theory]
	[MemberData(nameof(TestData_Equals))]
	public void GetHashCode_(Color color1, Color color2, bool areEqual)
	{
		// different colors should return a different hash code
		// (theoretically there can be collisions, but these should be rare, especially in the small test data set)
		Assert.Equal(areEqual, color1.GetHashCode() == color2.GetHashCode());
	}

	#endregion

	#region string ToString()

	/// <summary>
	/// Tests the <see cref="Color.ToString()"/> method.
	/// </summary>
	/// <param name="argb">The ARGB value to start with.</param>
	[Theory]
	[MemberData(nameof(TestData_Argb))]
	public void ToString_(uint argb)
	{
		byte rgbA = (byte)((argb & 0xFF000000U) >> 24);
		byte rgbR = (byte)((argb & 0x00FF0000U) >> 16);
		byte rgbG = (byte)((argb & 0x0000FF00U) >> 8);
		byte rgbB = (byte)(argb & 0x000000FF);

		Color colorFromArgb = Color.FromArgb(rgbA, rgbR, rgbG, rgbB);
		string expectedArgb = $"#{rgbA:X2}{rgbR:X2}{rgbG:X2}{rgbB:X2}";
		Assert.Equal(expectedArgb, colorFromArgb.ToString());

		float scRgbA = rgbA / 255.0f;
		float scRgbR = SRgbToScRgb(rgbR);
		float scRgbG = SRgbToScRgb(rgbG);
		float scRgbB = SRgbToScRgb(rgbB);

		Color colorFromScRgb = Color.FromScRgb(scRgbA, scRgbR, scRgbG, scRgbB);
		string expectedScRgb = string.Format("sc#{1:R}{0} {2:R}{0} {3:R}{0} {4:R}", GetNumericListSeparator(null), scRgbA, scRgbR, scRgbG, scRgbB);
		Assert.Equal(expectedScRgb, colorFromScRgb.ToString());
	}

	#endregion

	#region string ToString(IFormatProvider provider)

	/// <summary>
	/// Tests the <see cref="Color.ToString(IFormatProvider)"/> method.
	/// </summary>
	/// <param name="argb">The ARGB value to start with.</param>
	[Theory]
	[MemberData(nameof(TestData_Argb))]
	public void ToString_WithFormatProvider(uint argb)
	{
		CultureInfo provider = CultureInfo.InvariantCulture;

		byte rgbA = (byte)((argb & 0xFF000000U) >> 24);
		byte rgbR = (byte)((argb & 0x00FF0000U) >> 16);
		byte rgbG = (byte)((argb & 0x0000FF00U) >> 8);
		byte rgbB = (byte)(argb & 0x000000FF);

		Color colorFromArgb = Color.FromArgb(rgbA, rgbR, rgbG, rgbB);
		string expectedArgb = string.Format(provider, "#{0:X2}{1:X2}{2:X2}{3:X2}", rgbA, rgbR, rgbG, rgbB);
		Assert.Equal(expectedArgb, colorFromArgb.ToString(provider));

		float scRgbA = rgbA / 255.0f;
		float scRgbR = SRgbToScRgb(rgbR);
		float scRgbG = SRgbToScRgb(rgbG);
		float scRgbB = SRgbToScRgb(rgbB);

		Color colorFromScRgb = Color.FromScRgb(scRgbA, scRgbR, scRgbG, scRgbB);
		string expectedScRgb = string.Format(provider, "sc#{1:R}{0} {2:R}{0} {3:R}{0} {4:R}", GetNumericListSeparator(provider), scRgbA, scRgbR, scRgbG, scRgbB);
		Assert.Equal(expectedScRgb, colorFromScRgb.ToString(provider));
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Converts a channel value from sRGB to scRGB.
	/// </summary>
	/// <param name="value">The sRGB channel value to convert to scRGB.</param>
	/// <returns>The corresponding scRGB channel value.</returns>
	private static float SRgbToScRgb(byte value)
	{
		float num = value / 255.0f;

		return num switch
		{
			<= 0.0f     => 0.0f,
			<= 0.04045f => num / 12.92f,
			var _       => num < 1.0f ? (float)Math.Pow((num + 0.055) / 1.055, 2.4) : 1.0f
		};
	}

	/// <summary>
	/// Converts the alpha channel value from scRGB to sRGB.
	/// </summary>
	/// <param name="value">The scRGB alpha channel value to convert to sRGB.</param>
	/// <returns>The corresponding sRGB alpha channel value.</returns>
	private static byte ScAlphaTosRgb(float value)
	{
		if (value < 0.0) return 0;
		if (value > 1.0) return 255;
		return (byte)(value * 255.0);
	}

	/// <summary>
	/// Converts a channel value from scRGB to sRGB.
	/// </summary>
	/// <param name="value">The scRGB channel value to convert to sRGB.</param>
	/// <returns>The corresponding sRGB channel value.</returns>
	private static byte ScRgbTosRgb(float value)
	{
		return value switch
		{
			<= 0.0f       => 0,
			<= 0.0031308f => (byte)(255.0 * value * 12.9200000762939 + 0.5),
			var _         => value < 1.0f ? (byte)(255.0 * (1.05499994754791 * Math.Pow(value, 5.0 / 12.0) - 0.0549999997019768) + 0.5) : (byte)255
		};
	}

	/// <summary>
	/// Gets the numeric list separator for a given <see cref="IFormatProvider"/>.
	/// Separator is a comma [,] if the decimal separator is not a comma, or a semicolon [;] otherwise.
	/// </summary>
	/// <param name="provider">The format provider to use.</param>
	/// <returns>The separator char.</returns>
	private static char GetNumericListSeparator(IFormatProvider provider)
	{
		char numericSeparator = ',';

		// Get the NumberFormatInfo out of the provider, if possible.
		// If the IFormatProvider doesn't contain a NumberFormatInfo,
		// then this method returns the current culture's NumberFormatInfo.
		var numberFormat = NumberFormatInfo.GetInstance(provider);

		// use ';' if the decimal separator is the same as the list separator
		if (numberFormat.NumberDecimalSeparator.Length > 0 && numericSeparator == numberFormat.NumberDecimalSeparator[0])
			numericSeparator = ';';

		return numericSeparator;
	}

	#endregion
}
