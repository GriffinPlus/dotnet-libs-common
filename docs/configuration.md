# Griffin+ Configuration

## Overview

The configuration subsystem is part of the [Griffin+ Common Library](https://github.com/GriffinPlus/dotnet-libs-common) for .NET.

The main feature are hierarchical configurations that are cascadable, i.e. multiple configurations can be stacked on another. This allows to create a composite configuration that automatically merges settings from different sources into a single configuration. Configurations at a higher level override settings inherit from configurations at a lower level. The configuration subsystem comes with built-in support for persisting settings to XML. Custom persistence strategies can be added as well. Persistence strategies define how values are persisted. The built-in strategies support all types the [conversion subsystem](../README.md#namespace-griffinpluslibconversion) provides a converter for. Arrays of these types are supported as well. Additional converters can be added to the strategies if needed.

The configuration subsystem is designed with usability in mind. The setup is quite easy and done with only a few lines of code. As settings in configurations are organized hierarchically, access to settings can be conveniently done using filesystem-like paths. Traversing along nodes in the configuration tree is possible, but not necessary.

## Using

To illustrate the use of the configuration subsystem in a more complex scenario, let's assume that we want to have a configuration with default settings and a XML configuration file on top of that. For testing purposes another layer with the option to override settings should be added.

The requirements translate into the following configuration stack:
- Configuration without persistence that contains default settings
- Configuration for custom settings stored in a XML file
- Configuration for override settings that can be set at runtime (by code)

The following code will create the configurations, wire them up and add a configuration item with the desired default value to the default configuration. This will create configuration items in the inheriting configuration as well. These items exist, but they do not have any value, yet. All configurations return the same (default) value for the added setting. The default configuration provides its own value, while inheriting configurations provide the inherited value.

When saving the custom configuration you can save settings with own values only (specify flag `CascadedConfigurationSaveFlags.None`) or all inherited settings (specifiy flag `CascadedConfigurationSaveFlags.SaveInheritedSettings`). This is really useful as it allows to write a configuration with default values. After that a user can see the settings and modify the defaults which is less error prone than telling a user to insert a certain value with a certain name at a specific place in a configuration file. Please note, that the user has a fully initialized configuration after saving inherited settings, i.e. all previously inherited settings are overridden at custom level. Changes to default settings have no effect after that.

```csharp
var defaultConfiguration = new DefaultCascadedConfiguration("Root Configuration");
var customConfiguration = defaultConfiguration.AddInheritingConfiguration(new XmlFilePersistenceStrategy(@"%ProgramData%\My Company\My App\Settings.xml"));
var overrideConfiguration = customConfiguration.AddInheritingConfiguration(null);

// add item with a value in the default configuration
// (also adds items without a value to inheriting configurations)
string path = "/Settings/My Setting";
defaultConfiguration.AddItem<string>(path, "My Default Value");

// the item in the default configuration provides its own value when queried
var defaultItem = defaultConfiguration.GetItem<string>(path);
Debug.Assert(defaultItem.HasValue);
Debug.Assert(defaultItem.Value == "My Default Value");

// the item in the custom configuration provides the inherited value when queried
var customItem = customConfiguration.GetItem<string>(path);
Debug.Assert(!customItem.HasValue);
Debug.Assert(customItem.Value == "My Default Value");

// the item in the override configuration provides the inherited value when queried
var overrideItem = overrideConfiguration.GetItem<string>(path);
Debug.Assert(!overrideItem.HasValue);
Debug.Assert(overrideItem.Value == "My Default Value");

// change the setting in the override configuration
overrideConfiguration.SetValue(path, "My Override Value");

// the item in the default configuration provides its own value when queried
Debug.Assert(defaultItem.HasValue);
Debug.Assert(defaultItem.Value == "My Default Value");

// the item in the custom configuration provides the inherited value when queried
Debug.Assert(!customItem.HasValue);
Debug.Assert(customItem.Value == "My Default Value");

// the item in the override configuration provides its own value when queried
Debug.Assert(overrideItem.HasValue);
Debug.Assert(overrideItem.Value == "My Override Value");

// save inherited settings to the custom configuration file
// (this saves all settings inherited from the default configuration,
// but not changes done in the override configuration)
customConfiguration.Save(CascadedConfigurationSaveFlags.SaveInheritedSettings);
```
