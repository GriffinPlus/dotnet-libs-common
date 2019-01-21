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
	/// Extension methods for <see cref="System.Guid"/>.
	/// </summary>
	public static class GuidExtensions
	{
		/// <summary>
		/// Converts the <see cref="Guid"/> to a byte array containing a RFC 4122 compliant UUID.
		/// </summary>
		/// <param name="guid">The <see cref="Guid"/> itself.</param>
		/// <returns>An array containing a RFC 4122 compliant UUID.</returns>
		public static byte[] ToUuidByteArray(this Guid guid)
		{
			byte[] buffer = guid.ToByteArray();
			buffer.Swap4(0);
			buffer.Swap2(4);
			buffer.Swap2(6);
			return buffer;
		}
	}
}
