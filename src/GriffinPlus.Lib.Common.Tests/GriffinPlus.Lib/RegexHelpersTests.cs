///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Text.RegularExpressions;
using Xunit;

namespace GriffinPlus.Lib
{
	/// <summary>
	/// Unit tests targetting the <see cref="RegexHelpers"/> class.
	/// </summary>
	public class RegexHelpersTests
	{
		[Theory]
		[InlineData("a",       false )]
		[InlineData("ab",      false )]
		[InlineData("abc",     false )]
		[InlineData("?e",      true  )]
		[InlineData("?c?",     true  )]
		[InlineData("?c??",    true  )]
		[InlineData("??c",     true  )]
		[InlineData("??c?",    true  )]
		[InlineData("??c??",   true  )]
		[InlineData("a*",      true  )]
		[InlineData("*c*",     true  )]
		[InlineData("*e",      true  )]
		public void IsWildcardExpression(string pattern, bool isWildcard)
		{
			bool result = pattern.IsWildcardExpression();
			Assert.Equal(isWildcard, result);
		}

		[Theory]
		[InlineData("abcde",                           "a",              false )]
		[InlineData("abcde",                           "c",              false )]
		[InlineData("abcde",                           "e",              false )]
		[InlineData("abcde",                           "?c",             false )]
		[InlineData("abcde",                           "?c?",            false )]
		[InlineData("abcde",                           "?c??",           false )]
		[InlineData("abcde",                           "?c???",          false )]
		[InlineData("abcde",                           "??c",            false )]
		[InlineData("abcde",                           "??c?",           false )]
		[InlineData("abcde",                           "a*",             true  )]
		[InlineData("abcde",                           "*d*",            true  )]
		[InlineData("abcde",                           "*e",             true  )]
		[InlineData("abcde",                           "a????",          true  )]
		[InlineData("abcde",                           "??c??",          true  )]
		[InlineData("abcde",                           "????e",          true  )]
		[InlineData("The fox jumps over the lazy dog", "lazy",           false )]
		[InlineData("The fox jumps over the lazy dog", "the lazy dog",   false )]
		[InlineData("The fox jumps over the lazy dog", "the ?azy dog",   false )]
		[InlineData("The fox jumps over the lazy dog", "the *azy dog",   false )]
		[InlineData("The fox jumps over the lazy dog", "*lazy?",         false )]
		[InlineData("The fox jumps over the lazy dog", "*the lazy dog?", false )]
		[InlineData("The fox jumps over the lazy dog", "*the ?azy dog?", false )]
		[InlineData("The fox jumps over the lazy dog", "*the *azy dog?", false )]
		[InlineData("The fox jumps over the lazy dog", "*the lazy dog",  true  )]
		[InlineData("The fox jumps over the lazy dog", "*the ?azy dog",  true  )]
		[InlineData("The fox jumps over the lazy dog", "*the *azy dog",  true  )]
		[InlineData("The fox jumps over the lazy dog", "*lazy*",         true  )]
		[InlineData("The fox jumps over the lazy dog", "*the lazy dog*", true  )]
		[InlineData("The fox jumps over the lazy dog", "*the ?azy dog*", true  )]
		[InlineData("The fox jumps over the lazy dog", "*the *azy dog*", true  )]
		public void RegexFromWildcardString(string text, string pattern, bool isMatch)
		{
			Regex regex = RegexHelpers.FromWildcardExpression(pattern);
			Match match = regex.Match(text);
			Assert.Equal(isMatch, match.Success);
		}

	}
}
