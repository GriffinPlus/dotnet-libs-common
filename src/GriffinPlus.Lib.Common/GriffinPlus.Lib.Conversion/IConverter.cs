///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Conversion
{

	/// <summary>
	/// Interface for classes implementing a converter that can convert values to strings and vice versa.
	/// </summary>
	public interface IConverter
	{
		/// <summary>
		/// Gets the type of the value the current converter is working with.
		/// </summary>
		Type Type { get; }

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
		string ConvertObjectToString(object obj, IFormatProvider provider = null);

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
		/// <exception cref="OverflowException">Parsing <paramref name="s"/> succeeded, but the result does not fit into target type.</exception>
		object ConvertStringToObject(string s, IFormatProvider provider = null);
	}

}
