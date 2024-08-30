///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

namespace GriffinPlus.Lib;

/// <summary>
/// Event arguments used by the <see cref="RuntimeMetadata.AssemblyScanned"/> event
/// to notify clients that an assembly has been scanned and its metadata is available
/// via <see cref="RuntimeMetadata"/> properties.
/// </summary>
public class AssemblyScannedEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AssemblyScannedEventArgs"/> class.
	/// </summary>
	/// <param name="assembly">The scanned assembly.</param>
	public AssemblyScannedEventArgs(Assembly assembly)
	{
		Assembly = assembly;
	}

	/// <summary>
	/// Gets the assembly that has been scanned by the <see cref="RuntimeMetadata"/> class.
	/// </summary>
	public Assembly Assembly { get; }
}
