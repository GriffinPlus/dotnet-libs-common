///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using System.Text.RegularExpressions;

namespace GriffinPlus.Lib;

/// <summary>
/// Some helper methods around working with regular expressions.
/// </summary>
public static class RegexHelpers
{
	/// <summary>
	/// Checks whether the specified string is a wildcard expression
	/// </summary>
	/// <param name="expression">String to check.</param>
	/// <returns>true, if the specified string is a wildcard expression; otherwise false.</returns>
	public static bool IsWildcardExpression(this string expression)
	{
		return expression.Any(c => c is '?' or '*');
	}

	/// <summary>
	/// Converts the specified wildcard expression to a regular expression.
	/// </summary>
	/// <param name="expression">Wildcard expression to convert.</param>
	/// <param name="regexOptions">Options to apply when creating the Regex.</param>
	/// <returns>A regular expression matching the same text as the wildcard expression.</returns>
	public static Regex FromWildcardExpression(string expression, RegexOptions regexOptions = RegexOptions.Singleline)
	{
		string regex = "^" + Regex.Escape(expression).Replace("\\*", ".*").Replace("\\?", ".") + "$"; // greedy
		return new Regex(regex, regexOptions);
	}
}
