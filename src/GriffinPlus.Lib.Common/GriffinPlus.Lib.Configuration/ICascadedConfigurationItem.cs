///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// Untyped interface to configuration items.
/// </summary>
public interface ICascadedConfigurationItem : INotifyPropertyChanged
{
	/// <summary>
	/// Gets the name of the configuration item.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the path of the configuration item in the configuration hierarchy.
	/// </summary>
	string Path { get; }

	/// <summary>
	/// Gets the type of the value in the configuration item.
	/// </summary>
	Type Type { get; }

	/// <summary>
	/// Gets a value indicating whether the configuration item contains a valid value.
	/// </summary>
	bool HasValue { get; }

	/// <summary>
	/// Gets or sets the value of the configuration item.
	/// </summary>
	object Value { get; set; }

	/// <summary>
	/// Gets a value indicating whether the configuration supports comments.
	/// </summary>
	bool SupportsComments { get; }

	/// <summary>
	/// Gets a value indicating whether the configuration item contains a comment.
	/// </summary>
	bool HasComment { get; }

	/// <summary>
	/// Gets or sets the comment describing the configuration item.
	/// </summary>
	string Comment { get; set; }

	/// <summary>
	/// Gets the configuration the current item is in.
	/// </summary>
	CascadedConfiguration Configuration { get; }

	/// <summary>
	/// Resets the value of the configuration item, so an inherited configuration value is returned
	/// by the <see cref="Value"/> property.
	/// </summary>
	void ResetValue();
}
