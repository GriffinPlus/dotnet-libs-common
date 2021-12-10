///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

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
		/// <exception cref="ArgumentNullException"><paramref name="obj"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="obj"/> is not of the type handled by the converter.</exception>
		public string ConvertObjectToString(object obj, IFormatProvider provider = null)
		{
			if (obj == null) throw new ArgumentNullException(nameof(obj));
			if (obj.GetType() != Type)
				throw new ArgumentException($"Expecting an object of type {Type.FullName}, got {obj.GetType().FullName}.", nameof(obj));

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
		/// <exception cref="ArgumentNullException"><paramref name="s"/> is <c>null</c>.</exception>
		/// <exception cref="FormatException">Parsing <paramref name="s"/> failed.</exception>
		public object ConvertStringToObject(string s, IFormatProvider provider = null)
		{
			if (s == null) throw new ArgumentNullException(nameof(s));
			return Enum.Parse(Type, s);
		}
	}

}
