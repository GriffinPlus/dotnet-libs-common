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

namespace GriffinPlus.Lib.Imaging;

public partial struct Color
{
	private struct ColorFloat
	{
		public float A;
		public float R;
		public float G;
		public float B;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = A.GetHashCode();
				hashCode = (hashCode * 397) ^ R.GetHashCode();
				hashCode = (hashCode * 397) ^ G.GetHashCode();
				hashCode = (hashCode * 397) ^ B.GetHashCode();
				return hashCode;
			}
		}
	}
}
