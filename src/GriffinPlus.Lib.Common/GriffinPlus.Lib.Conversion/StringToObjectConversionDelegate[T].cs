///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Conversion
{

	/// <summary>
	/// Delegate for functions converting a string to an object of a specific type.
	/// </summary>
	/// <typeparam name="T">Type of the object the function converts the string to.</typeparam>
	/// <param name="s">String to parse.</param>
	/// <param name="provider">Format provider to use.</param>
	/// <returns>The created object built from the specified string.</returns>
	public delegate T StringToObjectConversionDelegate<out T>(string s, IFormatProvider provider = null);

}
