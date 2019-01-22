﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
			try {
				Guid.NewGuid().ToString(format);
			} catch (Exception ex) {
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
		/// <returns>The string represention of the object.</returns>
		public override string ConvertObjectToString(object obj, IFormatProvider provider = null)
		{
			Debug.Assert(obj.GetType() == typeof(Guid));

			Guid guid = (Guid)obj;
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
