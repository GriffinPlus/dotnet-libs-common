///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

// ReSharper disable ConvertClosureToMethodGroup
// ReSharper disable InconsistentlySynchronizedField

namespace GriffinPlus.Lib.Conversion
{

	/// <summary>
	/// Converters the configuration subsystem uses to format and parse setting values.
	/// </summary>
	public static class Converters
	{
		/// <summary>
		/// A converter for translating a <see cref="System.Boolean"/> to a string and vice versa.
		/// </summary>
		public static readonly Converter<bool> Boolean = new Converter<bool>((s, provider) => bool.Parse(s));

		/// <summary>
		/// A converter for translating a <see cref="System.SByte"/> to a string and vice versa.
		/// </summary>
		public static readonly Converter<sbyte> SByte = new Converter<sbyte>((s, provider) => sbyte.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.Byte"/> to a string and vice versa.
		/// </summary>
		public static readonly Converter<byte> Byte = new Converter<byte>((s, provider) => byte.Parse(s, provider));

		/// <summary>
		/// A converter for translating an array of <see cref="System.Byte"/> to a BASE64 encoded string and vice versa.
		/// </summary>
		public static readonly Converter<byte[]> ByteArray = new Converter<byte[]>(
			(s,   provider) => Convert.FromBase64String(s),
			(obj, provider) => Convert.ToBase64String(obj));

		/// <summary>
		/// A converter for translating a <see cref="System.Int16"/> to a string and vice versa.
		/// </summary>
		public static readonly Converter<short> Int16 = new Converter<short>((s, provider) => short.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.UInt16"/> to a string and vice versa.
		/// </summary>
		public static readonly Converter<ushort> UInt16 = new Converter<ushort>((s, provider) => ushort.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.Int32"/> to a string and vice versa.
		/// </summary>
		public static readonly Converter<int> Int32 = new Converter<int>((s, provider) => int.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.UInt32"/> to a string and vice versa.
		/// </summary>
		public static readonly Converter<uint> UInt32 = new Converter<uint>((s, provider) => uint.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.Int64"/> to a string and vice versa.
		/// </summary>
		public static readonly Converter<long> Int64 = new Converter<long>((s, provider) => long.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.UInt64"/> to a string and vice versa.
		/// </summary>
		public static readonly Converter<ulong> UInt64 = new Converter<ulong>((s, provider) => ulong.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.Decimal"/> to a string and vice versa.
		/// </summary>
		public static readonly Converter<decimal> Decimal = new Converter<decimal>((s, provider) => decimal.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.Single"/> to a string and vice versa.
		/// </summary>
		public static readonly Converter<float> Single = new Converter<float>((s, provider) => float.Parse(s, provider));

		/// <summary>
		/// A converter for translating a <see cref="System.Double"/> to a string and vice versa.
		/// </summary>
		public static readonly Converter<double> Double = new Converter<double>((s, provider) => double.Parse(s, provider));

		/// <summary>
		/// The string identity conversion (the string remains the same).
		/// </summary>
		public static readonly Converter<string> String = new Converter<string>((s, provider) => s, (obj, provider) => obj);

		/// <summary>
		/// A converter for translating a <see cref="System.Guid"/> to a string and vice versa.
		/// </summary>
		public static readonly Converter<Guid> Guid = new Converter<Guid>(
			(s,   provider) => System.Guid.Parse(s),
			(obj, provider) => obj.ToString("D"));

		/// <summary>
		/// A converter for translating a <see cref="System.DateTime"/> to a string and vice versa.
		/// </summary>
		public static readonly Converter<DateTime> DateTime = new Converter<DateTime>(
			(s,   provider) => System.DateTime.Parse(s, provider),
			(obj, provider) => obj.ToString("o", provider));

		/// <summary>
		/// A converter for translating a <see cref="System.TimeSpan"/> to a string and vice versa.
		/// </summary>
		public static readonly Converter<TimeSpan> TimeSpan = new Converter<TimeSpan>(
			(s,   provider) => System.TimeSpan.Parse(s, provider),
			(obj, provider) => obj.ToString("c", provider));

		/// <summary>
		/// A converter for translating a <see cref="System.Net.IPAddress"/> to a string and vice versa.
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public static readonly Converter<IPAddress> IPAddress = new Converter<IPAddress>((s, provider) => System.Net.IPAddress.Parse(s));

		/// <summary>
		/// Gets all converters that are provided by the <see cref="Converters"/> class out-of-the-box.
		/// </summary>
		public static readonly IConverter[] Predefined =
		{
			Boolean,
			SByte,
			Byte,
			ByteArray,
			Int16,
			UInt16,
			Int32,
			UInt32,
			Int64,
			UInt64,
			Decimal,
			Single,
			Double,
			String,
			Guid,
			DateTime,
			TimeSpan,
			IPAddress
		};

		private static readonly object                       sSync = new object();
		private static volatile Dictionary<Type, IConverter> sConverters;

		/// <summary>
		/// Initializes the <see cref="Converters"/> class.
		/// </summary>
		static Converters()
		{
			// add predefined converters to the list of converters
			sConverters = new Dictionary<Type, IConverter>();
			foreach (IConverter converter in Predefined)
			{
				sConverters.Add(converter.Type, converter);
			}
		}

		/// <summary>
		/// Registers a converter for global use, i.e. it can be queried using the <see cref="GetGlobalConverter(Type)"/> method.
		/// </summary>
		/// <param name="converter">Converter to register.</param>
		/// <exception cref="ArgumentNullException"><paramref name="converter"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="converter"/> returns <c>null</c> via it's <see cref="IConverter.Type"/> property.</exception>
		/// <exception cref="InvalidOperationException">A converter for the same type is already registered.</exception>
		public static void RegisterGlobalConverter(IConverter converter)
		{
			if (converter == null) throw new ArgumentNullException(nameof(converter));
			if (converter.Type == null) throw new ArgumentException("The specified converter's Type property returns <null>.", nameof(converter));

			lock (sSync)
			{
				// check whether a converter for the same type has already been registered
				if (sConverters.Any(x => x.Key == converter.Type))
					throw new InvalidOperationException($"A converter for the type ({converter.Type.FullName}) is already registered.");

				// replace the old converter dictionary
				// with a copy of the current global converter dictionary plus the new converter
				sConverters = new Dictionary<Type, IConverter>(sConverters) { { converter.Type, converter } };
			}
		}

		/// <summary>
		/// Gets a converter for the specified type.
		/// </summary>
		/// <param name="type">Type of the value to get a converter for.</param>
		/// <returns>
		/// A converter for the specified type;
		/// <c>null</c>, if there is no converter for the specified type.
		/// </returns>
		public static IConverter GetGlobalConverter(Type type)
		{
			if (!sConverters.TryGetValue(type, out var converter))
			{
				// converter is not known, yet
				if (type.IsEnum)
				{
					// type is an enum, register a new converter for it
					// (enums are supported out of the box)
					lock (sSync)
					{
						if (!sConverters.TryGetValue(type, out converter))
						{
							Type converterType = typeof(Converter_Enum<>).MakeGenericType(type);
							converter = (IConverter)Activator.CreateInstance(converterType);
							RegisterGlobalConverter(converter);
						}
					}
				}
			}

			return converter;
		}

		/// <summary>
		/// Gets converters that are predefined or have been registered using the <see cref="RegisterGlobalConverter"/> method.
		/// </summary>
		public static IEnumerable<IConverter> GlobalConverters => sConverters.Values;
	}

}
