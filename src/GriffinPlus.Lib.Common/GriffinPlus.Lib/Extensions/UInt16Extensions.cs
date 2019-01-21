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

namespace GriffinPlus.Lib
{
	/// <summary>
	/// Some extension methods for the <see cref="System.UInt16"/> struct.
	/// </summary>
	public static class UInt16Extensions
	{
		/// <summary>
		/// Checks whether the difference between the current value and the specified value is within the specified tolerance.
		/// </summary>
		/// <param name="self">The current value.</param>
		/// <param name="other">The value to compare with.</param>
		/// <param name="tolerance">Tolerable difference between the current and the specified value (must be positive).</param>
		/// <returns>true, if the difference between the current value and the specified value is within the specified tolerance.</returns>
		public static bool Equals(this ushort self, ushort other, ushort tolerance)
		{
			int difference = self > other ? self - other : other - self;
			return difference <= tolerance;
		}
	}
}
