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

using GriffinPlus.Lib;
using System.Text.RegularExpressions;
using Xunit;

namespace UnitTests
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
		[InlineData("?d",      true  )]
		[InlineData("?d?",     true  )]
		[InlineData("?d??",    true  )]
		[InlineData("?d???",   true  )]
		[InlineData("??d",     true  )]
		[InlineData("??d?",    true  )]
		[InlineData("??d??",   true  )]
		[InlineData("??d???",  true  )]
		[InlineData("???d",    true  )]
		[InlineData("???d?",   true  )]
		[InlineData("???d??",  true  )]
		[InlineData("a*",      true  )]
		[InlineData("*d*",     true  )]
		[InlineData("*g",      true  )]
		[InlineData("a??????", true  )]
		[InlineData("???d???", true  )]
		[InlineData("??????g", true  )]
		public void IsWildcardExpression(string pattern, bool isWildcard)
		{
			bool result = pattern.IsWildcardExpression();
			Assert.Equal(isWildcard, result);
		}

		[Theory]
		[InlineData("abcdefg",                         "a",              false )]
		[InlineData("abcdefg",                         "d",              false )]
		[InlineData("abcdefg",                         "g",              false )]
		[InlineData("abcdefg",                         "?d",             false )]
		[InlineData("abcdefg",                         "?d?",            false )]
		[InlineData("abcdefg",                         "?d??",           false )]
		[InlineData("abcdefg",                         "?d???",          false )]
		[InlineData("abcdefg",                         "??d",            false )]
		[InlineData("abcdefg",                         "??d?",           false )]
		[InlineData("abcdefg",                         "??d??",          false )]
		[InlineData("abcdefg",                         "??d???",         false )]
		[InlineData("abcdefg",                         "???d",           false )]
		[InlineData("abcdefg",                         "???d?",          false )]
		[InlineData("abcdefg",                         "???d??",         false )]
		[InlineData("abcdefg",                         "a*",             true  )]
		[InlineData("abcdefg",                         "*d*",            true  )]
		[InlineData("abcdefg",                         "*g",             true  )]
		[InlineData("abcdefg",                         "a??????",        true  )]
		[InlineData("abcdefg",                         "???d???",        true  )]
		[InlineData("abcdefg",                         "??????g",        true  )]
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
