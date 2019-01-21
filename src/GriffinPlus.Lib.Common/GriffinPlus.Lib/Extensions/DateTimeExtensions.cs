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

namespace GriffinPlus.Lib
{
	/// <summary>
	/// Extension methods for <see cref="System.DateTime"/>.
	/// </summary>
	public static class DateTimeExtensions
	{
		/// <summary>
		/// Truncates the specified <see cref="DateTime"/> to the specified fraction.
		/// </summary>
		/// <param name="dateTime">The <see cref="DateTime"/> to truncate.</param>
		/// <param name="precision">
		/// Precision the resulting <see cref="DateTime"/> should have:
		/// - TimeSpan.FromMilliseconds(1) -> truncate to whole milliseconds
		/// - TimeSpan.FromSeconds(1)      -> truncate to whole seconds
		/// - TimeSpan.FromMinutes(1)      -> truncate to whole minutes
		/// </param>
		/// <returns>The truncated <see cref="DateTime"/>.</returns>
		public static DateTime Truncate(this DateTime dateTime, TimeSpan precision)
		{
			if (precision == TimeSpan.Zero) return dateTime;
			return dateTime.AddTicks(-(dateTime.Ticks % precision.Ticks));
		}
	}
}
