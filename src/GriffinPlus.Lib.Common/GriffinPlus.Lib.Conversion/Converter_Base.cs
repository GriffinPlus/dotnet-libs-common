///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace GriffinPlus.Lib.Conversion
{

	/// <summary>
	/// Base class for converters implementing common parts of a converter.
	/// </summary>
	/// <typeparam name="T">The type of the value the converter works with.</typeparam>
	public abstract class Converter_Base<T> : IConverter
	{
		/// <summary>
		/// Gets the type of the value the current converter is working with.
		/// </summary>
		public Type Type => typeof(T);

		/// <summary>
		/// Converts an object to its string representation.
		/// </summary>
		/// <param name="obj">Object to convert.</param>
		/// <param name="provider">
		/// A format provider that controls how the conversion is done
		/// (null to use the current thread's culture to determine the format).
		/// </param>
		/// <returns>The string representation of the object.</returns>
		public virtual string ConvertObjectToString(object obj, IFormatProvider provider = null)
		{
			Debug.Assert(obj.GetType() == typeof(T));
			if (provider != null) return string.Format(provider, "{0}", obj);
			return $"{obj}";
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
		public abstract object ConvertStringToObject(string s, IFormatProvider provider = null);
	}

}
