///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace GriffinPlus.Lib;

/// <summary>
/// Extension methods for <see cref="System.Exception"/>.
/// </summary>
public static class ExtensionExtensions
{
	/// <summary>
	/// Collects all messages from inner exceptions and aggregate exceptions.
	/// </summary>
	/// <param name="self">The exception.</param>
	/// <returns>All exception messages.</returns>
	public static string[] GetAllMessages(this Exception self)
	{
		var messages = new List<string>();

		if (self is AggregateException aggregateException)
		{
			// the message of an AggregateException is almost always useless
			// => just take the inner exceptions...
			foreach (Exception iae in aggregateException.Flatten().InnerExceptions)
			{
				messages.AddRange(iae.GetAllMessages());
			}
		}
		else
		{
			// take message, if set in the exception
			if (!string.IsNullOrWhiteSpace(self.Message))
				messages.Add(self.Message);

			// collect messages of inner exceptions
			if (self.InnerException != null)
				messages.AddRange(self.InnerException.GetAllMessages());
		}

		return [.. messages];
	}
}
