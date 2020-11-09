///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Conversion
{
	/// <summary>
	/// A converter that can translate a <see cref="System.UInt16"/> to a string and vice versa.
	/// </summary>
	public class Converter_UInt16 : Converter_Base<UInt16>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Converter_UInt16"/> class.
		/// </summary>
		public Converter_UInt16()
		{

		}

		/// <summary>
		/// Parses the specified string creating the corresponding object.
		/// </summary>
		/// <param name="s">String to parse.</param>
		/// <param name="provider">
		/// A format provider that controls how the conversion is done
		/// (null to use the current thread's culture to determine the format).
		/// </param>
		/// <returns>The created object.</returns>
		public override object ConvertStringToObject(string s, IFormatProvider provider = null)
		{
			return UInt16.Parse(s, provider);
		}
	}
}
