///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
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
