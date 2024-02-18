///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
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
		/// Converts the <see cref="Guid"/> to a byte array containing an RFC 4122 compliant UUID.
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
