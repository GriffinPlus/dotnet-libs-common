///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Configuration
{

	/// <summary>
	/// Controls the behavior when it comes to saving a cascaded configuration.
	/// </summary>
	[Flags]
	public enum CascadedConfigurationSaveFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0,

		/// <summary>
		/// Save inherited settings, if a configuration item does not have an own value.
		/// If this flag is omitted only configuration items that have a value are saved.
		/// </summary>
		SaveInheritedSettings = 0x1
	}

}
