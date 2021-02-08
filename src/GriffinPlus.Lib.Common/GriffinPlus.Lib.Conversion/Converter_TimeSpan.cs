///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Conversion
{

	/// <summary>
	/// A converter that can translate a <see cref="System.TimeSpan"/> to a string and vice versa.
	/// The common timespan format is used ('c', i.e. [-][d.]hh:mm:ss[.fffffff]).
	/// </summary>
	public class Converter_TimeSpan : Converter_Base<TimeSpan>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Converter_TimeSpan"/> class.
		/// </summary>
		public Converter_TimeSpan()
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
			var span = (TimeSpan)obj;
			if (provider != null) return span.ToString("c", provider);
			return span.ToString("c");
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
			if (provider != null) return TimeSpan.Parse(s, provider);
			return TimeSpan.Parse(s);
		}
	}

}
