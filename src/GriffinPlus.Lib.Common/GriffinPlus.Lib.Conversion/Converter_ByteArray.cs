///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace GriffinPlus.Lib.Conversion
{
	/// <summary>
	/// A converter that can translate an array of <see cref="System.Byte"/> to a string and vice versa.
	/// A byte array is encoded using BASE64 encoding.
	/// </summary>
	public class Converter_ByteArray : Converter_Base<Byte[]>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Converter_ByteArray"/> class.
		/// </summary>
		public Converter_ByteArray()
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
			Debug.Assert(obj.GetType() == typeof(byte[]));
			return Convert.ToBase64String((byte[])obj);
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
			return Convert.FromBase64String(s);
		}

	}
}
