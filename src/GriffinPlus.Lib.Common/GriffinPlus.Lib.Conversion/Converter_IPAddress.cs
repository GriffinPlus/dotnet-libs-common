///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Net;

namespace GriffinPlus.Lib.Conversion
{
	/// <summary>
	/// A converter that can translate a <see cref="System.Net.IPAddress"/> to a string and vice versa.
	/// The ip address is converted to/from IPv4 dotted-quad or IPv6 colon-hexadecimal notation.
	/// </summary>
	public class Converter_IPAddress : Converter_Base<IPAddress>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Converter_IPAddress"/> class.
		/// </summary>
		public Converter_IPAddress()
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
		/// <returns>The string represention of the object.</returns>
		public override string ConvertObjectToString(object obj, IFormatProvider provider = null)
		{
			Debug.Assert(obj.GetType() == typeof(IPAddress));
			return obj.ToString();
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
			return IPAddress.Parse(s);
		}
	}
}
