///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// Some helper functions for handling paths in a <see cref="CascadedConfiguration"/>.
/// </summary>
public static class CascadedConfigurationPathHelper
{
	/// <summary>
	/// Combines a base path with path segments to create a new path.
	/// </summary>
	/// <param name="basePath">Base path of the new path.</param>
	/// <param name="pathSegments">Path segments to use (must be escaped properly).</param>
	/// <returns>The combined path.</returns>
	public static string CombinePath(string basePath, params string[] pathSegments)
	{
		if (basePath.Length > 1)
			return basePath + '/' + string.Join("/", pathSegments);

		Debug.Assert(basePath == "/");
		return basePath + string.Join("/", pathSegments);
	}

	private static readonly Regex sPathSplitterRegex = new(@"(?:(?<![\\])[/])|(?:(?<![\\])[\\](?![\\]))", RegexOptions.Compiled);

	/// <summary>
	/// Splits up the specified path into a list of path segments using '/' and '\' as delimiters.
	/// </summary>
	/// <param name="strategy">The persistence strategy that is used.</param>
	/// <param name="path">
	/// Path to split (path delimiter are '/' and '\', escape these characters, if a path segment contains one of them,
	/// otherwise the segment will be split up).
	/// </param>
	/// <param name="isItemPath">
	/// <see langword="true"/> if the path is an item path;<br/>
	/// <see langword="false"/> if the specified path is a configuration node path.
	/// </param>
	/// <param name="checkValidity">
	/// <see langword="true"/> to check the validity of path segment names using the specified persistence strategy;<br/>
	/// <see langword="false"/> to skip checking the validity of path segment names.
	/// </param>
	/// <returns>The resulting list of path segments.</returns>
	/// <exception cref="ConfigurationException">
	/// <paramref name="path"/> contains parts that are not supported by the specified persistence strategy.
	/// </exception>
	public static string[] SplitPath(
		ICascadedConfigurationPersistenceStrategy strategy,
		string                                    path,
		bool                                      isItemPath,
		bool                                      checkValidity)
	{
		List<string> segments = (
			                        from segment in sPathSplitterRegex.Split(path)
			                        let s = segment.Trim()
			                        where s.Length > 0
			                        select segment
		                        ).ToList();

		if (checkValidity)
		{
			if (strategy != null)
			{
				for (int i = 0; i < segments.Count; i++)
				{
					string name = segments[i];
					if (i + 1 < segments.Count)
					{
						// intermediate segment (can be a configuration only)
						if (!strategy.IsValidConfigurationName(name))
							throw new ConfigurationException("The specified configuration name ({0}) is not supported by the persistence strategy.", name);
					}
					else
					{
						// last segment (can be a configuration or an item)
						if (isItemPath)
						{
							if (!strategy.IsValidItemName(name))
								throw new ConfigurationException("The specified configuration name ({0}) is not supported by the persistence strategy.", name);
						}
						else
						{
							if (!strategy.IsValidConfigurationName(name))
								throw new ConfigurationException("The specified configuration name ({0}) is not supported by the persistence strategy.", name);
						}
					}
				}
			}
		}

		if (segments.Count == 0)
			throw new ArgumentException("The path is invalid, since it does not contain any location information.");

		return [.. segments];
	}

	/// <summary>
	/// Checks whether the persistence strategy can handle the specified type and throws an exception, if it can not.
	/// </summary>
	/// <param name="strategy">The persistence strategy that is used.</param>
	/// <param name="type">Type to check.</param>
	/// <exception cref="ConfigurationException">The specified type is not supported by the persistence strategy.</exception>
	public static void EnsureThatPersistenceStrategyCanHandleValueType(
		ICascadedConfigurationPersistenceStrategy strategy,
		Type                                      type)
	{
		if (strategy != null && !strategy.SupportsType(type))
		{
			throw new ConfigurationException(
				"The specified type ({0}) is not supported by the persistence strategy.",
				type.FullName);
		}
	}

	/// <summary>
	/// Checks whether the persistence strategy supports assigning the specified value to an item of the specified type.<br/>
	/// Throws an exception, if it can not.
	/// </summary>
	/// <param name="strategy">The persistence strategy that is used.</param>
	/// <param name="itemType">Item type to check.</param>
	/// <param name="value">Value to check.</param>
	/// <exception cref="ConfigurationException">
	/// The specified <paramref name="value"/> is not supported by a configuration item of the specified <paramref name="itemType"/>.
	/// </exception>
	public static void EnsureThatPersistenceStrategyCanAssignValue(
		ICascadedConfigurationPersistenceStrategy strategy,
		Type                                      itemType,
		object                                    value)
	{
		if (strategy != null && !strategy.IsAssignable(itemType, value))
		{
			throw new ConfigurationException(
				"The specified value is not supported for a configuration item of type '{0}'.",
				itemType.FullName);
		}
	}

	/// <summary>
	/// Checks whether the specified string contains a (non-escaped) path separator.
	/// </summary>
	/// <param name="s">String to check.</param>
	/// <returns>
	/// <see langword="true"/> if the specified strings contains a path separator;<br/>
	/// otherwise <see langword="false"/>.
	/// </returns>
	public static bool ContainsPathSeparator(string s)
	{
		return sPathSplitterRegex.IsMatch(s);
	}

	private static readonly Regex sEscapeRegex = new(@"(?<sep>[\\/])", RegexOptions.Compiled);

	/// <summary>
	/// Escapes the specified name for use in the configuration (avoid splitting up path segments unintentionally).
	/// </summary>
	/// <param name="s">String to escape.</param>
	/// <returns>The escaped string.</returns>
	public static string EscapeName(string s)
	{
		return sEscapeRegex.Replace(s, "\\${sep}");
	}

	private static readonly Regex sUnescapeRegex = new(@"[\\](?<sep>[\\/])", RegexOptions.Compiled);

	/// <summary>
	/// Removes path delimiter escaping from the specified string.
	/// </summary>
	/// <param name="s">String to remove path delimiter escaping from.</param>
	/// <returns>The resulting string.</returns>
	public static string UnescapeName(string s)
	{
		return sUnescapeRegex.Replace(s, "${sep}");
	}
}
