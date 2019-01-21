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
using System.Collections.Generic;

namespace GriffinPlus.Lib
{
	/// <summary>
	/// Extension methods for enumeration types.
	/// </summary>
	public static class EnumExtensions
	{
		/// <summary>
		/// Converts a flagged enumeration value to an array of enumeration values.
		/// </summary>
		/// <param name="self">The current value.</param>
		/// <returns>The separated enumeration values.</returns>
		public static Enum[] ToSeparateFlags(this Enum self)
		{
			List<Enum> result = new List<Enum>();
			foreach(Enum flag in Enum.GetValues(self.GetType())) {
				if (self.HasFlag(flag)) result.Add(flag);
			}
			return result.ToArray();
		}

	}
}
