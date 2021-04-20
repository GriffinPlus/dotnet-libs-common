///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Text
{

	/// <summary>
	/// Some stuff that can become in handy when working with Unicode strings.
	/// </summary>
	public static class Unicode
	{
		/// <summary>
		/// A string containing characters that are usually used to represent line breaks.
		/// The string contains the following characters:
		/// line feed (U+000A), form feed (U+000C), carriage return (U+000D), next line (U+0085), line separator (U+2028), paragraph separator (U+2029).
		/// </summary>
		public static readonly string NewLineCharacters =
			"\u000A" + // line feed
			"\u000C" + // form feed
			"\u000D" + // carriage return
			"\u0085" + // next line
			"\u2028" + // line separator
			"\u2029";  // paragraph separator
	}

}
