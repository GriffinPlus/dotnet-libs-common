///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
//
// This file incorporates work covered by the following copyright and permission notice:
//
//   Licensed to the .NET Foundation under one or more agreements.
//   The .NET Foundation licenses this file to you under the MIT license.
//   See the LICENSE file in the project root for more information.
//
//   Project: https://github.com/dotnet/wpf
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;
using System.Text;

namespace GriffinPlus.Lib.Imaging
{

	/// <summary>
	/// Describes a color in terms of alpha, red, green, and blue channels.
	/// </summary>
	public partial struct Color : IFormattable, IEquatable<Color>
	{
		private ColorFloat mScRgbColor;
		private ColorByte  mSRgbColor;
		private bool       mIsFromScRgb;

		/// <summary>
		/// Initializes a new <see cref="Color"/> instance from an <see cref="uint"/> value representing the ARGB values.
		/// </summary>
		/// <param name="argb">ARGB values (from MSB to LSB: A, R, G, B).</param>
		/// <returns>A <see cref="Color"/> instance with the specified values.</returns>
		internal static Color FromUInt32(uint argb)
		{
			Color color = new Color
			{
				mSRgbColor =
				{
					A = (byte)((argb & 0xFF000000U) >> 24),
					R = (byte)((argb & 0x00FF0000U) >> 16),
					G = (byte)((argb & 0x0000FF00U) >> 8),
					B = (byte)(argb & byte.MaxValue)
				}
			};
			color.mScRgbColor.A = (float)color.mSRgbColor.A / byte.MaxValue;
			color.mScRgbColor.R = SRgbToScRgb(color.mSRgbColor.R);
			color.mScRgbColor.G = SRgbToScRgb(color.mSRgbColor.G);
			color.mScRgbColor.B = SRgbToScRgb(color.mSRgbColor.B);
			color.mIsFromScRgb = false;
			return color;
		}

		/// <summary>
		/// Creates a new <see cref="Color"/> instance by using the specified scRGB alpha channel and color channel values.
		/// </summary>
		/// <param name="a">The scRGB alpha channel, <see cref="ScA"/>, of the new color.</param>
		/// <param name="r">The scRGB red channel, <see cref="ScR"/>, of the new color.</param>
		/// <param name="g">The scRGB green channel, <see cref="ScG"/>, of the new color.</param>
		/// <param name="b">The scRGB blue channel, <see cref="ScB"/>, of the new color.</param>
		/// <returns>A <see cref="Color"/> instance with the specified values.</returns>
		public static Color FromScRgb(
			float a,
			float r,
			float g,
			float b)
		{
			Color color = new Color();
			color.mScRgbColor.R = r;
			color.mScRgbColor.G = g;
			color.mScRgbColor.B = b;
			color.mScRgbColor.A = a;
			if (a < 0.0f) a = 0.0f;
			else if (a > 1.0f) a = 1.0f;
			color.mSRgbColor.A = (byte)((double)a * byte.MaxValue + 0.5);
			color.mSRgbColor.R = ScRgbTosRgb(color.mScRgbColor.R);
			color.mSRgbColor.G = ScRgbTosRgb(color.mScRgbColor.G);
			color.mSRgbColor.B = ScRgbTosRgb(color.mScRgbColor.B);
			color.mIsFromScRgb = true;
			return color;
		}

		/// <summary>
		/// Creates a new <see cref="Color"/> instance by using the specified sRGB alpha channel and color channel values.
		/// </summary>
		/// <param name="a">The alpha channel, <see cref="A"/>, of the new color.</param>
		/// <param name="r">The red channel, <see cref="R"/>, of the new color.</param>
		/// <param name="g">The green channel, <see cref="G"/>, of the new color.</param>
		/// <param name="b">The blue channel, <see cref="B"/>, of the new color.</param>
		/// <returns>A <see cref="Color"/> instance with the specified values.</returns>
		public static Color FromArgb(
			byte a,
			byte r,
			byte g,
			byte b)
		{
			Color color = new Color
			{
				mScRgbColor =
				{
					A = a / 255.0f,
					R = SRgbToScRgb(r),
					G = SRgbToScRgb(g),
					B = SRgbToScRgb(b)
				},
				mSRgbColor = { A = a }
			};
			color.mSRgbColor.R = ScRgbTosRgb(color.mScRgbColor.R);
			color.mSRgbColor.G = ScRgbTosRgb(color.mScRgbColor.G);
			color.mSRgbColor.B = ScRgbTosRgb(color.mScRgbColor.B);
			color.mIsFromScRgb = false;
			return color;
		}

		/// <summary>
		/// Creates a new <see cref="Color"/> instance by using the specified color channel values.
		/// </summary>
		/// <param name="r">The sRGB red channel, <see cref="R"/>, of the new color.</param>
		/// <param name="g">The sRGB green channel, <see cref="G"/>, of the new color.</param>
		/// <param name="b">The sRGB blue channel, <see cref="B"/>, of the new color.</param>
		/// <returns>A <see cref="Color"/> instance with the specified values and an alpha channel value of 255.</returns>
		public static Color FromRgb(byte r, byte g, byte b) => FromArgb(byte.MaxValue, r, g, b);

		/// <summary>
		/// Gets a hash code for this <see cref="Color"/> instance.
		/// </summary>
		/// <returns>A hash code for this <see cref="Color"/> instance.</returns>
		public override int GetHashCode() => mScRgbColor.GetHashCode();

		/// <summary>
		/// Creates a string representation of the color using the sRGB channels.
		/// </summary>
		/// <returns>
		/// The string representation of the color.
		/// The default implementation represents the <see cref="byte"/> values in hex form, prefixes with the # character,
		/// and starts with the alpha channel. For example, the <see cref="ToString()"/> value for <see cref="Colors.AliceBlue"/>
		/// is #FFF0F8FF.
		/// </returns>
		public override string ToString() => ConvertToString(mIsFromScRgb ? "R" : null, null);

		/// <summary>
		/// Creates a string representation of the color by using the sRGB channels and the specified format provider.
		/// </summary>
		/// <param name="provider">Culture-specific formatting information.</param>
		/// <returns>The string representation of the color.</returns>
		public string ToString(IFormatProvider provider) => ConvertToString(mIsFromScRgb ? "R" : null, provider);

		/// <summary>
		/// Formats the value of the current instance using the specified format.
		/// </summary>
		/// <param name="format">
		/// The format to use.<br/>
		/// -or-<br/>
		/// <c>null</c> to use the default format defined for the type of the <see cref="IFormattable"/> implementation.
		/// </param>
		/// <param name="provider">
		/// The provider to use to format the value.<br/>
		/// -or-<br/>
		/// <c>null</c> to obtain the numeric format information from the current locale setting of the operating system.
		/// </param>
		/// <returns>
		/// The value of the current instance in the specified format.
		/// </returns>
		string IFormattable.ToString(string format, IFormatProvider provider) => ConvertToString(format, provider);

		/// <summary>
		/// Formats the color using the specified <see cref="IFormatProvider"/> and a format specifier to format the channel values.
		/// </summary>
		/// <param name="format">Format specifier to use when formatting the channel values.</param>
		/// <param name="provider">Format provider to use.</param>
		/// <returns>The formatted color.</returns>
		internal string ConvertToString(string format, IFormatProvider provider)
		{
			StringBuilder stringBuilder = new StringBuilder();

			if (format == null)
			{
				stringBuilder.AppendFormat(
					provider,
					"#{0:X2}",
					new object[] { mSRgbColor.A });
				stringBuilder.AppendFormat(
					provider,
					"{0:X2}",
					new object[] { mSRgbColor.R });
				stringBuilder.AppendFormat(
					provider,
					"{0:X2}",
					new object[] { mSRgbColor.G });
				stringBuilder.AppendFormat(
					provider,
					"{0:X2}",
					new object[] { mSRgbColor.B });
			}
			else
			{
				char numericListSeparator = GetNumericListSeparator(provider);
				stringBuilder.AppendFormat(
					provider,
					// ReSharper disable FormatStringProblem
					"sc#{1:" + format + "}{0} {2:" + format + "}{0} {3:" + format + "}{0} {4:" + format + "}",
					// ReSharper restore FormatStringProblem
					numericListSeparator,
					mScRgbColor.A,
					mScRgbColor.R,
					mScRgbColor.G,
					mScRgbColor.B);
			}

			return stringBuilder.ToString();
		}

		/// <summary>
		/// Sets the scRGB channels of the color to within the gamut of 0 to 1, if they are outside that range.
		/// </summary>
		public void Clamp()
		{
			mScRgbColor.A = mScRgbColor.A < 0.0f ? 0.0f : mScRgbColor.A > 1.0f ? 1.0f : mScRgbColor.A;
			mScRgbColor.R = mScRgbColor.R < 0.0f ? 0.0f : mScRgbColor.R > 1.0f ? 1.0f : mScRgbColor.R;
			mScRgbColor.G = mScRgbColor.G < 0.0f ? 0.0f : mScRgbColor.G > 1.0f ? 1.0f : mScRgbColor.G;
			mScRgbColor.B = mScRgbColor.B < 0.0f ? 0.0f : mScRgbColor.B > 1.0f ? 1.0f : mScRgbColor.B;

			mSRgbColor.A = (byte)((double)mScRgbColor.A * byte.MaxValue);
			mSRgbColor.R = ScRgbTosRgb(mScRgbColor.R);
			mSRgbColor.G = ScRgbTosRgb(mScRgbColor.G);
			mSRgbColor.B = ScRgbTosRgb(mScRgbColor.B);
		}

		/// <summary>
		/// Tests whether two <see cref="Color"/> instances are equal.
		/// </summary>
		/// <param name="color1">The first <see cref="Color"/> instance to compare.</param>
		/// <param name="color2">The second <see cref="Color"/> instance to compare.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="color1"/> and <paramref name="color2"/> are equal;
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool Equals(Color color1, Color color2) => color1 == color2;

		/// <summary>
		/// Tests whether the specified <see cref="Color"/> instance is equal to the current instance.
		/// </summary>
		/// <param name="color">The <see cref="Color"/> instance to compare to the current <see cref="Color"/> instance.</param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="Color"/> instance is equal to the current <see cref="Color"/> instance;
		/// otherwise <c>false</c>.
		/// </returns>
		public bool Equals(Color color) => this == color;

		/// <summary>
		/// Tests whether the specified object is a <see cref="Color"/> instance and equal to the current instance.
		/// </summary>
		/// <param name="obj">The object to compare to this <see cref="Color"/> instance.</param>
		/// <returns>
		/// <c>true</c> if the specified object is a <see cref="Color"/> instance and equal to the current <see cref="Color"/> instance;
		/// otherwise <c>false</c>.
		/// </returns>
		public override bool Equals(object obj) => obj is Color color && this == color;

		/// <summary>
		/// Tests whether two <see cref="Color"/> instances are equal.
		/// </summary>
		/// <param name="left">The first <see cref="Color"/> instance to compare.</param>
		/// <param name="right">The second <see cref="Color"/> instance to compare.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="left"/> and <paramref name="right"/> are equal;
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool operator ==(Color left, Color right)
		{
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return
				left.mScRgbColor.R == right.mScRgbColor.R &&
				left.mScRgbColor.G == right.mScRgbColor.G &&
				left.mScRgbColor.B == right.mScRgbColor.B &&
				left.mScRgbColor.A == right.mScRgbColor.A;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Tests whether two <see cref="Color"/> instances are not equal.
		/// </summary>
		/// <param name="left">The first <see cref="Color"/> instance to compare.</param>
		/// <param name="right">The second <see cref="Color"/> instance to compare.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="left"/> and <paramref name="right"/> are not equal;
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool operator !=(Color left, Color right) => !(left == right);

		/// <summary>
		/// Gets or sets the sRGB alpha channel value of the color.
		/// </summary>
		/// <returns>
		/// The sRGB alpha channel value of the color, a value between 0 and 255.
		/// </returns>
		public byte A
		{
			get => mSRgbColor.A;
			set
			{
				mScRgbColor.A = value / 255.0f;
				mSRgbColor.A = value;
			}
		}

		/// <summary>
		/// Gets or sets the sRGB red channel value of the color.
		/// </summary>
		/// <returns>
		/// The sRGB red channel value of the <see cref="Color"/> instance, a value between 0 and 255.
		/// </returns>
		public byte R
		{
			get => mSRgbColor.R;
			set
			{
				mScRgbColor.R = SRgbToScRgb(value);
				mSRgbColor.R = value;
			}
		}

		/// <summary>
		/// Gets or sets the sRGB green channel value of the color.
		/// </summary>
		/// <returns>
		/// The sRGB green channel value of the <see cref="Color"/> instance, a value between 0 and 255.
		/// </returns>
		public byte G
		{
			get => mSRgbColor.G;
			set
			{
				mScRgbColor.G = SRgbToScRgb(value);
				mSRgbColor.G = value;
			}
		}

		/// <summary>
		/// Gets or sets the sRGB blue channel value of the color.
		/// </summary>
		/// <returns>
		/// The sRGB blue channel value of the <see cref="Color"/> instance, a value between 0 and 255.
		/// </returns>
		public byte B
		{
			get => mSRgbColor.B;
			set
			{
				mScRgbColor.B = SRgbToScRgb(value);
				mSRgbColor.B = value;
			}
		}

		/// <summary>
		/// Gets or sets the scRGB alpha channel value of the color.
		/// </summary>
		/// <returns>
		/// The scRGB alpha channel value of the <see cref="Color"/> instance, a value between 0 and 1.
		/// </returns>
		public float ScA
		{
			get => mScRgbColor.A;
			set
			{
				mScRgbColor.A = value;
				if (value < 0.0)
					mSRgbColor.A = 0;
				else if (value > 1.0)
					mSRgbColor.A = 255;
				else
					mSRgbColor.A = (byte)(value * 255.0);
			}
		}

		/// <summary>
		/// Gets or sets the scRGB red channel value of the color.
		/// </summary>
		/// <returns>
		/// The scRGB red channel value of the <see cref="Color"/> instance, a value between 0 and 1.
		/// </returns>
		public float ScR
		{
			get => mScRgbColor.R;
			set
			{
				mScRgbColor.R = value;
				mSRgbColor.R = ScRgbTosRgb(value);
			}
		}

		/// <summary>
		/// Gets or sets the scRGB green channel value of the color.
		/// </summary>
		/// <returns>
		/// The scRGB green channel value of the <see cref="Color"/> instance, a value between 0 and 1.
		/// </returns>
		public float ScG
		{
			get => mScRgbColor.G;
			set
			{
				mScRgbColor.G = value;
				mSRgbColor.G = ScRgbTosRgb(value);
			}
		}

		/// <summary>
		/// Gets or sets the scRGB blue channel value of the color.
		/// </summary>
		/// <returns>
		/// The scRGB blue channel value of the <see cref="Color"/> instance, a value between 0 and 1.
		/// </returns>
		public float ScB
		{
			get => mScRgbColor.B;
			set
			{
				mScRgbColor.B = value;
				mSRgbColor.B = ScRgbTosRgb(value);
			}
		}

		/// <summary>
		/// Converts a channel value from sRGB to scRGB.
		/// </summary>
		/// <param name="value">The sRGB channel value to convert to scRGB.</param>
		/// <returns>The corresponding scRGB channel value.</returns>
		private static float SRgbToScRgb(byte value)
		{
			float num = value / 255.0f;

			if (num <= 0.04045f)
				return num / 12.92f;

			return num < 1.0f
				       ? (float)Math.Pow((num + 0.055) / 1.055, 2.4)
				       : 1.0f;
		}

		/// <summary>
		/// Converts a channel value from scRGB to sRGB.
		/// </summary>
		/// <param name="value">The scRGB channel value to convert to sRGB.</param>
		/// <returns>The corresponding sRGB channel value.</returns>
		private static byte ScRgbTosRgb(float value)
		{
			if (value <= 0.0f)
				return 0;

			if (value <= 0.0031308f)
				return (byte)(255.0 * value * 12.9200000762939 + 0.5);

			return value < 1.0f
				       ? (byte)(255.0 * (1.05499994754791 * Math.Pow(value, 5.0 / 12.0) - 0.0549999997019768) + 0.5)
				       : (byte)255;
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

			// Get the NumberFormatInfo out of the provider, if possible
			// If the IFormatProvider doesn't not contain a NumberFormatInfo, then
			// this method returns the current culture's NumberFormatInfo.
			NumberFormatInfo numberFormat = NumberFormatInfo.GetInstance(provider);

			// use ';' if the decimal separator is the same as the list separator
			if (numberFormat.NumberDecimalSeparator.Length > 0 && numericSeparator == numberFormat.NumberDecimalSeparator[0])
				numericSeparator = ';';

			return numericSeparator;
		}
	}

}
