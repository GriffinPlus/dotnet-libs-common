///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Configuration
{

	/// <summary>
	/// Untyped interface to configuration items (for internal use only).
	/// </summary>
	interface ICascadedConfigurationItemInternal : ICascadedConfigurationItem
	{
		/// <summary>
		/// Sets the configuration the current item is in.
		/// </summary>
		/// <param name="configuration">Configuration to set.</param>
		void SetConfiguration(CascadedConfiguration configuration);
	}

}
