///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Conversion
{

	/// <summary>
	/// Base class for converters implementing common parts of a converter.
	/// </summary>
	/// <typeparam name="T">The type of the value the converter works with.</typeparam>
	public abstract class ConverterBase<T> : IConverter
	{
		/// <summary>
		/// Gets the type of the value the current converter is working with.
		/// </summary>
		public Type Type => typeof(T);

		/// <summary>
		/// Gets the function that converts an object of the corresponding type to its string representation.
		/// </summary>
		public ObjectToStringConversionDelegate<T> ObjectToStringConversion => ConvertObjectToString;

		/// <summary>
		/// Gets the function that parses the string representation of an object of the corresponding type
		/// to the actual object.
		/// </summary>
		public StringToObjectConversionDelegate<T> StringToObjectConversion => ConvertStringToObject;

		/// <summary>
		/// Converts an object to its string representation.
		/// </summary>
		/// <param name="obj">Object to convert.</param>
		/// <param name="provider">
		/// A format provider that controls how the conversion is done
		/// (<c>null</c> to use the current thread's culture to determine the format).
		/// </param>
		/// <returns>The string representation of the object.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="obj"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="obj"/> is not of the type handled by the converter.</exception>
		public abstract string ConvertObjectToString(T obj, IFormatProvider provider = null);

		/// <summary>
		/// Parses the specified string creating the corresponding object.
		/// </summary>
		/// <param name="s">String to parse.</param>
		/// <param name="provider">
		/// A format provider that controls how the conversion is done
		/// (<c>null</c> to use the current thread's culture to determine the format).
		/// </param>
		/// <returns>The created object.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="s"/> is <c>null</c>.</exception>
		/// <exception cref="FormatException">Parsing <paramref name="s"/> failed.</exception>
		/// <exception cref="OverflowException">Parsing <paramref name="s"/> succeeded, but the result does not fit into target type.</exception>
		public abstract T ConvertStringToObject(string s, IFormatProvider provider = null);

		#region IConverter (Explicit Implementation)

		/// <summary>
		/// Gets the strongly typed conversion delegate converting from the object to a string
		/// (always an instance of <see cref="ObjectToStringConversionDelegate{T}"/> where <c>T</c> is the same as <see cref="Type"/>).
		/// </summary>
		Delegate IConverter.ObjectToStringConversion => ObjectToStringConversion;

		/// <summary>
		/// Gets the strongly typed conversion delegate converting from a string to the object
		/// (always an instance of <see cref="StringToObjectConversionDelegate{T}"/> where <c>T</c> is the same as <see cref="Type"/>).
		/// </summary>
		Delegate IConverter.StringToObjectConversion => StringToObjectConversion;

		/// <summary>
		/// Converts an object to its string representation.
		/// </summary>
		/// <param name="obj">Object to convert.</param>
		/// <param name="provider">
		/// A format provider that controls how the conversion is done
		/// (<c>null</c> to use the current thread's culture to determine the format).
		/// </param>
		/// <returns>The string representation of the object.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="obj"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="obj"/> is not of the type handled by the converter.</exception>
		string IConverter.ConvertObjectToString(object obj, IFormatProvider provider)
		{
			if (obj == null) throw new ArgumentNullException(nameof(obj));
			if (obj.GetType() != typeof(T))
				throw new ArgumentException($"Expecting an object of type '{typeof(T).FullName}', got '{obj.GetType().FullName}'.", nameof(obj));

			return ConvertObjectToString((T)obj, provider);
		}

		/// <summary>
		/// Parses the specified string creating the corresponding object.
		/// </summary>
		/// <param name="s">String to parse.</param>
		/// <param name="provider">
		/// A format provider that controls how the conversion is done
		/// (<c>null</c> to use the current thread's culture to determine the format).
		/// </param>
		/// <returns>The created object.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="s"/> is <c>null</c>.</exception>
		/// <exception cref="FormatException">Parsing <paramref name="s"/> failed.</exception>
		/// <exception cref="OverflowException">Parsing <paramref name="s"/> succeeded, but the result does not fit into target type.</exception>
		object IConverter.ConvertStringToObject(string s, IFormatProvider provider)
		{
			if (s == null) throw new ArgumentNullException(nameof(s));
			return ConvertStringToObject(s, provider);
		}

		#endregion
	}

}
