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
		public Type Type
		{
			get { return typeof(T); }
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
		public virtual string ConvertObjectToString(object obj, IFormatProvider provider = null)
		{
			Debug.Assert(obj.GetType() == typeof(T));

			if (provider != null) {
				return string.Format(provider, "{0}", obj);
			} else {
				return string.Format("{0}", obj);
			}
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
