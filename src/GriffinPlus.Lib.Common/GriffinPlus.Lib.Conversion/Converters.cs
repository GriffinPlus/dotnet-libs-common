///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Conversion
{
	/// <summary>
	/// A couple of converters the library provides.
	/// </summary>
	public class Converters
	{
		#region Predefined Converters

		/// <summary>
		/// Gets all converters that are provided by the <see cref="Converters"/> class out-of-the-box.
		/// </summary>
		public readonly static IConverter[] Predefined;

		/// <summary>
		/// A converter for translating a <see cref="System.Boolean"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_Boolean Boolean;

		/// <summary>
		/// A converter for translating a <see cref="System.SByte"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_SByte SByte;

		/// <summary>
		/// A converter for translating a <see cref="System.Byte"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_Byte Byte;

		/// <summary>
		/// A converter for translating an array of <see cref="System.Byte"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_ByteArray ByteArray;

		/// <summary>
		/// A converter for translating a <see cref="System.Int16"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_Int16 Int16;

		/// <summary>
		/// A converter for translating a <see cref="System.UInt16"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_UInt16 UInt16;

		/// <summary>
		/// A converter for translating a <see cref="System.Int32"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_Int32 Int32;

		/// <summary>
		/// A converter for translating a <see cref="System.UInt32"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_UInt32 UInt32;

		/// <summary>
		/// A converter for translating a <see cref="System.Int64"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_Int64 Int64;

		/// <summary>
		/// A converter for translating a <see cref="System.UInt64"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_UInt64 UInt64;

		/// <summary>
		/// A converter for translating a <see cref="System.Decimal"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_Decimal Decimal;

		/// <summary>
		/// A converter for translating a <see cref="System.Single"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_Single Single;

		/// <summary>
		/// A converter for translating a <see cref="System.Double"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_Double Double;

		/// <summary>
		/// The string identity conversion (the string remains the same).
		/// </summary>
		public readonly static Converter_String String;

		/// <summary>
		/// A converter for translating a <see cref="System.Guid"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_Guid Guid;

		/// <summary>
		/// A converter for translating a <see cref="System.DateTime"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_DateTime DateTime;

		/// <summary>
		/// A converter for translating a <see cref="System.TimeSpan"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_TimeSpan TimeSpan;

		/// <summary>
		/// A converter for translating a <see cref="System.Net.IPAddress"/> to a string and vice versa.
		/// </summary>
		public readonly static Converter_IPAddress IPAddress;

		#endregion

		#region Class Variables

		private static object sSync = new object();
		private static volatile Dictionary<Type, IConverter> sConverters;

		#endregion

		#region Construction

		/// <summary>
		/// Initializes the <see cref="Converters"/> class.
		/// </summary>
		static Converters()
		{
			Predefined = new IConverter[]
			{
				Boolean   = new Converter_Boolean(),
				SByte     = new Converter_SByte(),
				Byte      = new Converter_Byte(),
				ByteArray = new Converter_ByteArray(),
				Int16     = new Converter_Int16(),
				UInt16    = new Converter_UInt16(),
				Int32     = new Converter_Int32(),
				UInt32    = new Converter_UInt32(),
				Int64     = new Converter_Int64(),
				UInt64    = new Converter_UInt64(),
				Decimal   = new Converter_Decimal(),
				Single    = new Converter_Single(),
				Double    = new Converter_Double(),
				String    = new Converter_String(),
				Guid      = new Converter_Guid(),
				DateTime  = new Converter_DateTime(),
				TimeSpan  = new Converter_TimeSpan(),
				IPAddress = new Converter_IPAddress(),
			};

			// add predefined converters to the list of global converters
			sConverters = new Dictionary<Type,IConverter>();
			foreach (IConverter converter in Predefined) {
				sConverters.Add(converter.Type, converter);
			}
		}

		/// <summary>
		/// Not available, since this is a utility class.
		/// </summary>
		private Converters()
		{

		}

		#endregion

		#region Global Converters

		/// <summary>
		/// Registers a converter for global use, i.e. it can be queried using the <see cref="GetGlobalConverter"/> method.
		/// </summary>
		/// <param name="converter">Converter to register</param>
		public static void RegisterGlobalConverter(IConverter converter)
		{
			lock (sSync)
			{
				// copy the current global converter dictionary
				Dictionary<Type, IConverter> copy = new Dictionary<Type,IConverter>();
				foreach (KeyValuePair<Type,IConverter> kvp in sConverters) {
					copy.Add(kvp.Key, kvp.Value);
				}

				// add the new converter to the copy
				copy.Add(converter.Type, converter);

				// replace the old converter dictionary
				sConverters = copy;
			}
		}

		/// <summary>
		/// Gets a global converter for the specified type.
		/// </summary>
		/// <param name="type">Type of the value to get a converter for.</param>
		/// <returns>
		/// A converter for the specified type;
		/// null, if there is no global converter for the specified type.
		/// </returns>
		public static IConverter GetGlobalConverter(Type type)
		{
			if (type.IsEnum) {
				return new Converter_Enum(type);
			}

			IConverter converter = null;
			sConverters.TryGetValue(type, out converter);
			return converter;
		}

		/// <summary>
		/// Gets converters that are predefined or have been registered using the <see cref="RegisterGlobalConverter"/> method.
		/// </summary>
		public static IEnumerable<IConverter> GlobalConverters
		{
			get {
				return sConverters.Values;
			}
		}

		#endregion
	}
}
