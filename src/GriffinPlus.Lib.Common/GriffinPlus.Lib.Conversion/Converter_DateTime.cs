///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/GriffinPlus/dotnet-libs-common)
//
// Copyright 2018-2019 Sascha Falk <sascha@falk-online.eu>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for
// the specific language governing permissions and limitations under the License.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Conversion
{
	/// <summary>
	/// A converter that can translate a <see cref="System.DateTime"/> to a string and vice versa.
	/// A datetime is encoded according to ISO 8601.
	/// </summary>
	public class Converter_DateTime : Converter_Base<DateTime>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Converter_DateTime"/> class.
		/// </summary>
		public Converter_DateTime()
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
			DateTime dt = (DateTime)obj;
			if (provider != null) return dt.ToString("o", provider);
			else                  return dt.ToString("o");
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
			if (provider != null) return DateTime.Parse(s, provider);
			else                  return DateTime.Parse(s);
		}
	}
}
