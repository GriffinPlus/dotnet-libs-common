///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace GriffinPlus.Lib.Conversion
{

	/// <summary>
	/// A converter that can translate an enumeration value to a string and vice versa.
	/// </summary>
	public class Converter_Enum : IConverter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Converter_Enum"/> class.
		/// </summary>
		/// <param name="type">The enumeration type to convert.</param>
		public Converter_Enum(Type type)
		{
			Type = type;
		}

		/// <summary>
		/// Gets the type of the value the current converter is working with.
		/// </summary>
		public Type Type { get; }

		/// <summary>
		/// Converts an object to its string representation.
		/// </summary>
		/// <param name="obj">Object to convert.</param>
		/// <param name="provider">
		/// A format provider that controls how the conversion is done
		/// (null to use the current thread's culture to determine the format).
		/// </param>
		/// <returns>The string representation of the object.</returns>
		public string ConvertObjectToString(object obj, IFormatProvider provider = null)
		{
			Debug.Assert(obj.GetType() == Type);
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
		public object ConvertStringToObject(string s, IFormatProvider provider = null)
		{
			return Enum.Parse(Type, s);
		}
	}

}
