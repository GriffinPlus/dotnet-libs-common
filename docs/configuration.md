# Griffin+ Configuration

## Overview

The configuration subsystem is part of the [Griffin+ Common Library](https://github.com/GriffinPlus/dotnet-libs-common) for .NET.

The main feature are hierarchical configurations that are cascadable, i.e. multiple configurations can be stacked on another. This allows to create a composite configuration that automatically merges settings from different sources into a single configuration. Configurations at a higher level override settings derived from configurations at a lower level. The configuration subsystem comes with built-in support for persisting settings to XML. Custom persistence strategies can be added as well. Persistence strategies define how values are persisted. The built-in strategies support all types the [conversion subsystem](../README.md#namespace-griffinpluslibconversion) provides a converter for. Arrays of these types are supported as well. Additional converters can be added to the strategies if needed.

The configuration subsystem is designed with usability in mind. The setup is quite easy and done with only a few lines of code. As settings in configurations are organized hierarchically, access to settings can be conveniently done using filesystem like paths. Traversing along nodes in the configuration tree is possible, but not necessary.

## Using

To illustrate the use of the configuration subsystem in a more complex scenario, let's assume that we want to have machine specific settings in a XML file and user specific settings in another XML file. Furthermore default user specific settings should be defined at machine level. Default user specific settings apply to all users unless a setting is overridden at user level.

The requirements translate into the following configuration stack:
- Configuration without persistence that contains default settings
- Configuration for machine specific settings stored in a XML file
- Configuration for user specific settings stored in a XML file

The following code will create the configurations, wire them up and add a machine specific setting and a user specific setting. Both settings are set in the default configuration. The machine specific configuration and the user specific configuration inherit their setting only as these configurations have a configuration item for their setting, but no own value. Furthermore the user specific setting is overridden at machine level. Saving the configurations at the end persists inherited settings in the machine configuration and the user configuration. After that a user can see the settings and modify the defaults which is less error prone than telling a user to insert a certain value with a certain name at a specific place in a configuration. Please note, that the user has a fully initialized configuration after saving inherited settings, i.e. all previously inherited user specific settings (at machine level) are overridden at user level. Changes to default user settings after that have no effect. If you do not want this, please save using `CascadedConfigurationSaveFlags.None` instead of `CascadedConfigurationSaveFlags.SaveInheritedSettings`. This will only persist settings that are really set in the user level configuration.

```csharp
var defaultConfiguration = new CascadedConfiguration("Root Configuration", null);
var machineConfiguration = new CascadedConfiguration(defaultConfiguration, new XmlFilePersistenceStrategy(@"%ALLUSERSPROFILE%\My Company\My App"));
var userConfiguration    = new CascadedConfiguration(machineConfiguration, new XmlFilePersistenceStrategy(@"%LOCALAPPDATA%\My Company\My App"));

// add machine specific setting
string path = "/Settings/in/the/deep/Machine Name";
defaultConfiguration.SetValue<string>(path, "Fancy Machine");
machineConfiguration.SetItem<string>(path); // no value, inherits value from default configuration

// add user specific setting
path = "/Settings/somewhere/else/User Name";
defaultConfiguration.SetValue<string>(path, "Default User");
machineConfiguration.SetValue<string>(path, "Machine User");
userConfiguration.SetItem<string>(path); // no value, inherits value from machine configuration

// save inherited settings to machine/user configurations
machineConfiguration.Save(CascadedConfigurationSaveFlags.SaveInheritedSettings);
userConfiguration.Save(CascadedConfigurationSaveFlags.SaveInheritedSettings);
```
