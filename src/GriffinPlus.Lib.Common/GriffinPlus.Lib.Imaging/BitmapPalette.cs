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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using GriffinPlus.Lib.Collections;

namespace GriffinPlus.Lib.Imaging
{

	/// <summary>
	/// Defines the available color palette for a supported image type.
	/// </summary>
	public sealed class BitmapPalette : IEquatable<BitmapPalette>
	{
		private readonly PartialList<Color> mColors;

		/// <summary>
		/// Create a palette from the specified list of colors.
		/// </summary>
		public BitmapPalette(IList<Color> colors)
		{
			if (colors == null) throw new ArgumentNullException(nameof(colors));
			int count = colors.Count;
			if (count < 1) throw new ArgumentException("The palette must contain at least one color.", nameof(colors));
			if (count > 256) throw new ArgumentException("The palette must not contain more than 256 colors.", nameof(colors));
			mColors = new PartialList<Color>(colors.ToArray());
		}

		/// <summary>
		/// Create a palette from the specified list of colors (for internal use only).
		/// </summary>
		internal BitmapPalette(params Color[] colors)
		{
			Debug.Assert(colors != null, "The palette must not be null.");
			Debug.Assert(colors.Length >= 1, "The palette must contain at least one color.");
			Debug.Assert(colors.Length <= 256, "The palette must not contain more than 256 colors.");
			mColors = new PartialList<Color>(colors);
		}

		/// <summary>
		/// The contents of the palette.
		/// </summary>
		public IList<Color> Colors => mColors;

		#region Comparisons

		/// <summary>
		/// Compares two <see cref="BitmapPalette"/> instances for equality.
		/// </summary>
		/// <param name="left">The first <see cref="BitmapPalette"/> to compare.</param>
		/// <param name="right">The second <see cref="BitmapPalette"/> to compare.</param>
		/// <returns>
		/// <c>true</c> if the two <see cref="BitmapPalette"/> instances are equal;
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool operator ==(BitmapPalette left, BitmapPalette right) => Equals(left, right);

		/// <summary>
		/// Compares two <see cref="BitmapPalette"/> instances for inequality.
		/// </summary>
		/// <param name="left">The first <see cref="BitmapPalette"/> to compare.</param>
		/// <param name="right">The second <see cref="BitmapPalette"/> to compare.</param>
		/// <returns>
		/// <c>true</c> if the two <see cref="BitmapPalette"/> instances are not equal;
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool operator !=(BitmapPalette left, BitmapPalette right) => !Equals(left, right);

		/// <summary>
		/// Determines whether the specified <see cref="BitmapPalette"/> instances are equal.
		/// </summary>
		/// <param name="palette1">The first <see cref="BitmapPalette"/> instance to compare.</param>
		/// <param name="palette2">The second <see cref="BitmapPalette"/> instance to compare.</param>
		/// <returns>
		/// <c>true</c> if the two <see cref="BitmapPalette"/> instances are equal;
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool Equals(BitmapPalette palette1, BitmapPalette palette2)
		{
			if (ReferenceEquals(palette1, null) && ReferenceEquals(palette2, null)) return true;
			if (ReferenceEquals(palette1, null) || ReferenceEquals(palette2, null)) return false;
			return palette1.mColors.SequenceEqual(palette2.mColors);
		}

		/// <summary>
		/// Determines whether the bitmap palette equals the given <see cref="BitmapPalette"/>.
		/// </summary>
		/// <param name="other">The bitmap palette to compare.</param>
		/// <returns>
		/// <c>true</c> if the two <see cref="BitmapPalette"/> instances are equal;
		/// otherwise <c>false</c>.
		/// </returns>
		public bool Equals(BitmapPalette other) => Equals(this, other);

		/// <summary>
		/// Determines whether the specified object is equal to the current instance.
		/// </summary>
		/// <param name="obj">The Object to compare with the current instance.</param>
		/// <returns>
		/// <c>true</c> if the <see cref="BitmapPalette"/> is equal to <paramref name="obj"/>;
		/// otherwise <c>false</c>.
		/// </returns>
		public override bool Equals(object obj) => obj is BitmapPalette palette && Equals(this, palette);

		/// <summary>
		/// Gets the hash code of the <see cref="BitmapPalette"/>.
		/// </summary>
		/// <returns>The bitmap palette's hash code.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				foreach (Color color in mColors)
				{
					hashCode = (hashCode * 397) ^ color.GetHashCode();
				}

				return hashCode;
			}
		}

		#endregion
	}

}
