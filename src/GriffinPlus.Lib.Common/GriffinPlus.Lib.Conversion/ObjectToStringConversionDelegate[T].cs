///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Conversion;

/// <summary>
/// Delegate for functions converting an object to its string representation.
/// </summary>
/// <param name="obj">Object to convert to a string.</param>
/// <param name="provider">Format provider to use.</param>
/// <returns>The object in its string representation.</returns>
public delegate string ObjectToStringConversionDelegate<in T>(T obj, IFormatProvider provider = null);
