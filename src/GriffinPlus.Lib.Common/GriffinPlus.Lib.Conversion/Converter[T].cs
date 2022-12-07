///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Conversion
{

	/// <summary>
	/// Converter implementation using callbacks (for simple converters).
	/// </summary>
	/// <typeparam name="T">The type of the value the converter works with.</typeparam>
	public sealed class Converter<T> : IConverter
	{
		/// <summary>
		/// Initializes the <see cref="Converter{T}"/> class.
		/// </summary>
		static Converter()
		{
			DefaultObjectToStringConversion = (obj, provider) =>
			{
				if (obj == null) throw new ArgumentNullException(nameof(obj));
				if (obj.GetType() != typeof(T))
					throw new ArgumentException($"Expecting an object of type {typeof(T).FullName}, got {obj.GetType().FullName}.", nameof(obj));

				return provider != null ? string.Format(provider, "{0}", obj) : $"{obj}";
			};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Converter{T}"/> class.
		/// </summary>
		/// <param name="string2Obj">Function that parses a string and creates an object of the corresponding type.</param>
		/// <param name="obj2String">
		/// Function that converts an object of the corresponding type to its string representation
		/// (null to use a primitive conversion using <see cref="string.Format(IFormatProvider, string, object)"/>, which
		/// suits the needs in most cases).
		/// </param>
		public Converter(StringToObjectConversionDelegate<T> string2Obj, ObjectToStringConversionDelegate<T> obj2String = null)
		{
			ObjectToStringConversion = obj2String ?? DefaultObjectToStringConversion;
			StringToObjectConversion = string2Obj;
		}

		/// <summary>
		/// Gets the type of the value the current converter is working with.
		/// </summary>
		public Type Type => typeof(T);

		/// <summary>
		/// Gets the default conversion from an object of the corresponding type to its string representation using
		/// <see cref="string.Format(IFormatProvider, string, object)"/>, which suits the needs in most cases.
		/// </summary>
		public static ObjectToStringConversionDelegate<T> DefaultObjectToStringConversion { get; }

		/// <summary>
		/// Gets the function that converts an object of the corresponding type to its string representation.
		/// </summary>
		public ObjectToStringConversionDelegate<T> ObjectToStringConversion { get; }

		/// <summary>
		/// Gets the strongly typed conversion delegate converting from the object to a string
		/// (always an instance of <see cref="ObjectToStringConversionDelegate{T}"/> where <c>T</c> is the same as <see cref="Type"/>).
		/// </summary>
		Delegate IConverter.ObjectToStringConversion => ObjectToStringConversion;

		/// <summary>
		/// Gets the function that parses the string representation of an object of the corresponding type
		/// to the actual object.
		/// </summary>
		public StringToObjectConversionDelegate<T> StringToObjectConversion { get; }

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
		public string ConvertObjectToString(object obj, IFormatProvider provider = null)
		{
			if (obj == null) throw new ArgumentNullException(nameof(obj));
			if (obj.GetType() != typeof(T))
				throw new ArgumentException($"Expecting an object of type '{typeof(T).FullName}', got '{obj.GetType().FullName}'.", nameof(obj));

			return ObjectToStringConversion((T)obj, provider);
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
		public object ConvertStringToObject(string s, IFormatProvider provider = null)
		{
			if (s == null) throw new ArgumentNullException(nameof(s));
			return StringToObjectConversion(s, provider);
		}
	}

}
