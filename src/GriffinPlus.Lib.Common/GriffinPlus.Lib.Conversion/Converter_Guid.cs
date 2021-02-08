///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace GriffinPlus.Lib.Conversion
{

	/// <summary>
	/// A converter that can translate a <see cref="System.Guid"/> to a string and vice versa.
	/// </summary>
	public class Converter_Guid : Converter_Base<Guid>
	{
		private readonly string mFormat;

		/// <summary>
		/// Initializes a new instance of the <see cref="Converter_Guid"/> class.
		/// </summary>
		public Converter_Guid()
		{
			mFormat = "D";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Converter_Guid"/> class.
		/// </summary>
		/// <param name="format">
		/// Desired format of the GUID as a string
		/// ("N", "D", "B", "P", or "X"; Please see documentation of <see cref="System.Guid.ToString(string)"/>).
		/// </param>
		public Converter_Guid(string format)
		{
#if DEBUG
			// test the format to avoid failing later on...
			try
			{
				// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
				Guid.NewGuid().ToString(format);
			}
			catch (Exception ex)
			{
				Debug.Fail("The specified GUID format is invalid.", ex.ToString());
			}
#endif

			mFormat = format;
		}

		/// <summary>
		/// Converts a GUID to its string representation.
		/// </summary>
		/// <param name="obj">Object to convert.</param>
		/// <param name="provider">
		/// A format provider that controls how the conversion is done
		/// (null to use the current thread's culture to determine the format).
		/// </param>
		/// <returns>The string representation of the object.</returns>
		public override string ConvertObjectToString(object obj, IFormatProvider provider = null)
		{
			Debug.Assert(obj is Guid);

			var guid = (Guid)obj;
			return guid.ToString(mFormat);
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
			return Guid.Parse(s);
		}
	}

}
