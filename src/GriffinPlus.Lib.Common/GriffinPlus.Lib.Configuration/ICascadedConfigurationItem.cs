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
	/// Gets the type of the value in the configuration item
	/// (the type of the actual value may be the type or a type deriving from this type).
	/// </summary>
	Type Type { get; }

	/// <summary>
	/// Gets a value indicating whether the configuration item contains a valid value.
	/// </summary>
	bool HasValue { get; }

	/// <summary>
	/// Gets or sets the value of the configuration item.
	/// </summary>
	/// <exception cref="ConfigurationException">The configuration item does not have a value.</exception>
	/// <remarks>
	/// This property gets the value of the current configuration item, if the current configuration item provides a value for it.<br/>
	/// If it doesn't, inherited configurations in the configuration cascade are queried.<br/>
	/// Setting the property effects the current configuration item only.
	/// </remarks>
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
	/// <remarks>
	/// This property gets the comment of the current configuration item, if the current configuration item provides a comment.<br/>
	/// If it doesn't, inherited configurations in the configuration cascade are queried.<br/>
	/// Setting the property effects the current configuration item only.<br/>
	/// Setting the property to <see langword="null"/> resets the comment making an inherited comment visible.
	/// </remarks>
	string Comment { get; set; }

	/// <summary>
	/// Gets the configuration the current item is in.
	/// </summary>
	CascadedConfigurationBase Configuration { get; }

	/// <summary>
	/// Gets the inherited item, if any.
	/// </summary>
	ICascadedConfigurationItem InheritedItem { get; }

	/// <summary>
	/// Resets the value of the configuration item, so an inherited configuration value is returned
	/// by the <see cref="Value"/> property.
	/// </summary>
	void ResetValue();
}
