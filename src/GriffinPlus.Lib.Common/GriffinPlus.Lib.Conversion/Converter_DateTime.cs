///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Conversion
{

	/// <summary>
	/// A converter that can translate a <see cref="System.DateTime"/> to a string and vice versa.
	/// A datetime is encoded according to ISO 8601.
	/// </summary>
	public class Converter_DateTime : Converter_Base<DateTime>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Converter_DateTime"/> class.
		/// </summary>
		public Converter_DateTime()
		{
		}

		/// <summary>
		/// Converts an object to its string representation.
		/// </summary>
		/// <param name="obj">Object to convert.</param>
		/// <param name="provider">
		/// A format provider that controls how the conversion is done
		/// (null to use the current thread's culture to determine the format).
		/// </param>
		/// <returns>The string representation of the object.</returns>
		public override string ConvertObjectToString(object obj, IFormatProvider provider = null)
		{
			var dt = (DateTime)obj;
			if (provider != null) return dt.ToString("o", provider);
			return dt.ToString("o");
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
			if (provider != null) return DateTime.Parse(s, provider);
			return DateTime.Parse(s);
		}
	}

}
