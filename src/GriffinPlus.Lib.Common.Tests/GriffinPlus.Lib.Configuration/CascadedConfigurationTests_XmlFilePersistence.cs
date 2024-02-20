///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// Tests for the <see cref="CascadedConfiguration"/> class using <see cref="XmlFilePersistenceStrategy"/> to persist data.
/// </summary>
// ReSharper disable once UnusedMember.Global
public class CascadedConfigurationTests_XmlFilePersistence : CascadedConfigurationTests
{
	private const string ConfigurationFilePath = "CascadedXmlFileConfigurationTest.xml";

	/// <summary>
	/// Gets the persistence strategy to test with.
	/// </summary>
	/// <returns>The persistence strategy to test with.</returns>
	protected override ICascadedConfigurationPersistenceStrategy GetStrategy()
	{
		return new XmlFilePersistenceStrategy(ConfigurationFilePath);
	}
}
