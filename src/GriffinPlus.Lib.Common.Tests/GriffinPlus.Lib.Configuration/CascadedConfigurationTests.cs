///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;

using Xunit;

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// Base class with tests for the <see cref="CascadedConfiguration"/> class using different persistence strategies.
/// </summary>
public abstract class CascadedConfigurationTests
{
	#region Types for Testing Purposes

	/// <summary>
	/// An enumeration type for testing purposes.
	/// </summary>
	protected enum TestEnum
	{
		A,
		B,
		C
	}

	#endregion

	#region Test Customization

	/// <summary>
	/// Gets the persistence strategy to test with.
	/// </summary>
	/// <returns>The persistence strategy to test with.</returns>
	protected virtual ICascadedConfigurationPersistenceStrategy GetStrategy()
	{
		return null;
	}

	#endregion

	#region Common Test Data

	/// <summary>
	/// Test data with various item types supported by the conversion subsystem out of the box (no item value).
	/// </summary>
	public static IEnumerable<object[]> ItemTestDataWithoutValue
	{
		get
		{
			string[][] itemPathsList =
			[
				["/Value"],                                              // single value in the root configuration
				["/Child/Value"],                                        // single value in a child configuration
				["/Value1", "/Value2"],                                  // multiple values in the root configuration
				["/Child/Value1", "/Child/Value2"],                      // multiple values in a child configuration
				["/Value1", "/Value2", "/Child/Value1", "/Child/Value2"] // multiple values in the root configuration and a child configuration
			];

			foreach (string[] itemPaths in itemPathsList)
			{
				// System.SByte
				yield return [itemPaths, typeof(sbyte)];
				yield return [itemPaths, typeof(sbyte[])];

				// System.Byte
				yield return [itemPaths, typeof(byte)];
				yield return [itemPaths, typeof(byte[])];

				// System.Int16
				yield return [itemPaths, typeof(short)];
				yield return [itemPaths, typeof(short[])];

				// System.UInt16
				yield return [itemPaths, typeof(ushort)];
				yield return [itemPaths, typeof(ushort[])];

				// System.Int32
				yield return [itemPaths, typeof(int)];
				yield return [itemPaths, typeof(int[])];

				// System.UInt32
				yield return [itemPaths, typeof(uint)];
				yield return [itemPaths, typeof(uint[])];

				// System.Int64
				yield return [itemPaths, typeof(long)];
				yield return [itemPaths, typeof(long[])];

				// System.UInt64
				yield return [itemPaths, typeof(ulong)];
				yield return [itemPaths, typeof(ulong[])];

				// System.Single
				yield return [itemPaths, typeof(float)];
				yield return [itemPaths, typeof(float[])];

				// System.Double
				yield return [itemPaths, typeof(double)];
				yield return [itemPaths, typeof(double[])];

				// System.Decimal
				yield return [itemPaths, typeof(decimal)];
				yield return [itemPaths, typeof(decimal[])];

				// System.String
				yield return [itemPaths, typeof(string)];
				yield return [itemPaths, typeof(string[])];

				// Enums
				yield return [itemPaths, typeof(TestEnum)];
				yield return [itemPaths, typeof(TestEnum[])];

				// System.Guid
				yield return [itemPaths, typeof(Guid)];
				yield return [itemPaths, typeof(Guid[])];

				// System.DateTime
				yield return [itemPaths, typeof(DateTime)];
				yield return [itemPaths, typeof(DateTime[])];

				// System.TimeSpan
				yield return [itemPaths, typeof(TimeSpan)];
				yield return [itemPaths, typeof(TimeSpan[])];

				// System.Net.IPAddress
				yield return [itemPaths, typeof(IPAddress)];
				yield return [itemPaths, typeof(IPAddress[])];
			}
		}
	}

	/// <summary>
	/// Test data with various item types supported by the conversion subsystem out of the box (with item values).
	/// </summary>
	public static IEnumerable<object[]> ItemTestDataWithValue
	{
		get
		{
			string[][] itemPathsList =
			[
				["/Value"],                                              // single value in the root configuration
				["/Child/Value"],                                        // single value in a child configuration
				["/Value1", "/Value2"],                                  // multiple values in the root configuration
				["/Child/Value1", "/Child/Value2"],                      // multiple values in a child configuration
				["/Value1", "/Value2", "/Child/Value1", "/Child/Value2"] // multiple values in the root configuration and a child configuration
			];

			foreach (string[] itemPaths in itemPathsList)
			{
				// System.SByte
				yield return [itemPaths, typeof(sbyte), sbyte.MinValue];
				yield return [itemPaths, typeof(sbyte), (sbyte)0];
				yield return [itemPaths, typeof(sbyte), sbyte.MaxValue];
				yield return [itemPaths, typeof(sbyte[]), new sbyte[] { sbyte.MinValue, 0, sbyte.MaxValue }];

				// System.Byte
				yield return [itemPaths, typeof(byte), byte.MinValue];
				yield return [itemPaths, typeof(byte), byte.MaxValue];
				yield return [itemPaths, typeof(byte[]), new byte[] { byte.MinValue, byte.MinValue + 1, 0x02, byte.MaxValue - 1, byte.MaxValue }];

				// System.Int16
				yield return [itemPaths, typeof(short), short.MinValue];
				yield return [itemPaths, typeof(short), (short)0];
				yield return [itemPaths, typeof(short), short.MaxValue];
				yield return [itemPaths, typeof(short[]), new short[] { short.MinValue, 0, short.MaxValue }];

				// System.UInt16
				yield return [itemPaths, typeof(ushort), ushort.MinValue];
				yield return [itemPaths, typeof(ushort), ushort.MaxValue];
				yield return [itemPaths, typeof(ushort[]), new ushort[] { ushort.MinValue, ushort.MinValue + 1, ushort.MaxValue - 1, ushort.MaxValue }];

				// System.Int32
				yield return [itemPaths, typeof(int), int.MinValue];
				yield return [itemPaths, typeof(int), 0];
				yield return [itemPaths, typeof(int), int.MaxValue];
				yield return [itemPaths, typeof(int[]), new[] { int.MinValue, 0, int.MaxValue }];

				// System.UInt32
				yield return [itemPaths, typeof(uint), uint.MinValue];
				yield return [itemPaths, typeof(uint), uint.MaxValue];
				yield return [itemPaths, typeof(uint[]), new[] { uint.MinValue, uint.MinValue + 1, uint.MaxValue - 1, uint.MaxValue }];

				// System.Int64
				yield return [itemPaths, typeof(long), long.MinValue];
				yield return [itemPaths, typeof(long), (long)0];
				yield return [itemPaths, typeof(long), long.MaxValue];
				yield return [itemPaths, typeof(long[]), new[] { long.MinValue, 0, long.MaxValue }];

				// System.UInt64
				yield return [itemPaths, typeof(ulong), ulong.MinValue];
				yield return [itemPaths, typeof(ulong), ulong.MaxValue];
				yield return [itemPaths, typeof(ulong[]), new[] { ulong.MinValue, ulong.MinValue + 1, ulong.MaxValue - 1, ulong.MaxValue }];

				// System.Single
				yield return [itemPaths, typeof(float), float.MinValue];
				yield return [itemPaths, typeof(float), 0.0f];
				yield return [itemPaths, typeof(float), float.MaxValue];
				yield return [itemPaths, typeof(float), float.NegativeInfinity];
				yield return [itemPaths, typeof(float), float.PositiveInfinity];
				yield return
				[
					itemPaths,
					typeof(float[]),
					new[]
					{
						float.NegativeInfinity,
						float.MinValue,
						float.MinValue + 1,
						0.0f,
						float.MaxValue - 1,
						float.MaxValue,
						float.PositiveInfinity
					}
				];

				// System.Double
				yield return [itemPaths, typeof(double), double.MinValue];
				yield return [itemPaths, typeof(double), 0.0];
				yield return [itemPaths, typeof(double), double.MaxValue];
				yield return [itemPaths, typeof(double), double.NegativeInfinity];
				yield return [itemPaths, typeof(double), double.PositiveInfinity];
				yield return
				[
					itemPaths,
					typeof(double[]),
					new[]
					{
						double.NegativeInfinity,
						double.MinValue,
						double.MinValue + 1,
						0.0,
						double.MaxValue - 1,
						double.MaxValue,
						double.PositiveInfinity
					}
				];

				// System.Decimal
				yield return [itemPaths, typeof(decimal), decimal.MinValue];
				yield return [itemPaths, typeof(decimal), decimal.Zero];
				yield return [itemPaths, typeof(decimal), decimal.MaxValue];
				yield return
				[
					itemPaths,
					typeof(decimal[]),
					new[]
					{
						decimal.MinValue,
						decimal.MinValue + 1,
						decimal.Zero,
						decimal.MaxValue - 1,
						decimal.MaxValue
					}
				];

				// System.String
				yield return [itemPaths, typeof(string), "The quick brown fox jumps over the lazy dog"];
				yield return
				[
					itemPaths,
					typeof(string[]),
					new[]
					{
						"The",
						"quick",
						"brown",
						"fox",
						"jumps",
						"over",
						"the",
						"lazy",
						"dog"
					}
				];

				// Enums
				yield return [itemPaths, typeof(TestEnum), TestEnum.A]; // 0
				yield return [itemPaths, typeof(TestEnum), TestEnum.B]; // 1
				yield return [itemPaths, typeof(TestEnum), TestEnum.C]; // 2

				// System.Guid
				yield return [itemPaths, typeof(Guid), Guid.Parse("{52F3FBBB-F755-468B-904E-D1B1EDD81368}")];
				yield return
				[
					itemPaths,
					typeof(Guid[]),
					new[]
					{
						Guid.Parse("{52F3FBBB-F755-468B-904E-D1B1EDD81368}"),
						Guid.Parse("{0359E56C-81B0-4874-BF04-1B362A652465}"),
						Guid.Parse("{196BB94E-1BE3-4295-94C4-B1ED2D17DAE9}")
					}
				];

				// System.DateTime
				var einsteinsBirthday = new DateTime(1879, 3, 14);
				yield return [itemPaths, typeof(DateTime), DateTime.MinValue];
				yield return [itemPaths, typeof(DateTime), einsteinsBirthday];
				yield return [itemPaths, typeof(DateTime), DateTime.MaxValue];
				yield return
				[
					itemPaths,
					typeof(DateTime[]),
					new[]
					{
						DateTime.MinValue,
						einsteinsBirthday,
						DateTime.MaxValue
					}
				];

				// System.TimeSpan
				yield return [itemPaths, typeof(TimeSpan), TimeSpan.MinValue];
				yield return [itemPaths, typeof(TimeSpan), TimeSpan.Zero];
				yield return [itemPaths, typeof(TimeSpan), TimeSpan.MaxValue];
				yield return
				[
					itemPaths,
					typeof(TimeSpan[]),
					new[]
					{
						TimeSpan.MinValue,
						TimeSpan.Zero,
						TimeSpan.MaxValue
					}
				];

				// System.Net.IPAddress
				yield return [itemPaths, typeof(IPAddress), IPAddress.Parse("0.0.0.0")];              // IPv4 Any
				yield return [itemPaths, typeof(IPAddress), IPAddress.Parse("255.255.255.255")];      // IPv4 Broadcast
				yield return [itemPaths, typeof(IPAddress), IPAddress.Parse("127.0.0.1")];            // IPv4 Loopback
				yield return [itemPaths, typeof(IPAddress), IPAddress.Parse("192.168.10.20")];        // IPv4 Address (Private Network Range)
				yield return [itemPaths, typeof(IPAddress), IPAddress.Parse("::")];                   // IPv6 Any
				yield return [itemPaths, typeof(IPAddress), IPAddress.Parse("::0")];                  // IPv6 None
				yield return [itemPaths, typeof(IPAddress), IPAddress.Parse("::1")];                  // IPv6 Loopback
				yield return [itemPaths, typeof(IPAddress), IPAddress.Parse("fd01:dead:beef::affe")]; // IPv6 Address (ULA range)
				yield return
				[
					itemPaths,
					typeof(IPAddress[]),
					new[]
					{
						IPAddress.Parse("0.0.0.0"),             // IPv4 Any
						IPAddress.Parse("255.255.255.255"),     // IPv4 Broadcast
						IPAddress.Parse("127.0.0.1"),           // IPv4 Loopback
						IPAddress.Parse("192.168.10.20"),       // IPv4 Address (Private Network Range)
						IPAddress.Parse("::"),                  // IPv6 Any
						IPAddress.Parse("::0"),                 // IPv6 None
						IPAddress.Parse("::1"),                 // IPv6 Loopback
						IPAddress.Parse("fd01:dead:beef::affe") // IPv6 Address (ULA range)
					}
				];
			}
		}
	}

	#endregion

	#region Construction

	/// <summary>
	/// Tests creating a base configuration,
	/// i.e. a configuration that does not inherit from another configuration.
	/// </summary>
	[Fact]
	public void Create_FlatConfiguration_WithoutInheritance()
	{
		// create a new root configuration that does not inherit from another configuration
		const string name = "My Configuration";
		ICascadedConfigurationPersistenceStrategy strategy = GetStrategy();
		var configuration = new CascadedConfiguration(name, strategy);

		// the configuration should return the specified name as expected
		Assert.Equal(name, configuration.Name);

		// the configuration should return the persistence strategy as specified
		Assert.Same(strategy, configuration.PersistenceStrategy);

		// a base configuration should not inherit from some other configuration
		Assert.Null(configuration.InheritedConfiguration);

		// a root-level configuration should return itself as the root configuration
		Assert.Same(configuration, configuration.RootConfiguration);

		// a top-level configuration should not have any parent configurations
		Assert.Null(configuration.Parent);
		Assert.Equal("/", configuration.Path);

		// the configuration should not contain any child configurations or items
		Assert.Empty(configuration.Children);
		Assert.Empty(configuration.Items);

		// the configuration should not be marked as 'modified' at start
		Assert.False(configuration.IsModified);

		// the configuration should provide an object to use when locking the configuration
		Assert.NotNull(configuration.Sync);
	}


	/// <summary>
	/// Tests creating a configuration inheriting from a base configuration,
	/// i.e. a configuration that does not inherit from another configuration.
	/// </summary>
	[Fact]
	public void Create_FlatConfiguration_WithInheritance()
	{
		// create a new root configuration that does not inherit from another configuration
		const string baseConfigurationName = "My Configuration";
		ICascadedConfigurationPersistenceStrategy baseConfigurationStrategy = GetStrategy();
		var baseConfiguration = new CascadedConfiguration(baseConfigurationName, baseConfigurationStrategy);

		// create a new root configuration that inherits from the base configuration
		ICascadedConfigurationPersistenceStrategy inheritingConfigurationStrategy = GetStrategy();
		var inheritingConfiguration = new CascadedConfiguration(baseConfiguration, inheritingConfigurationStrategy);

		//
		// check whether the base configuration has the expected state
		//

		// the configuration should return the specified name as expected
		Assert.Equal(baseConfigurationName, baseConfiguration.Name);

		// the configuration should return the persistence strategy as specified
		Assert.Same(baseConfigurationStrategy, baseConfiguration.PersistenceStrategy);

		// a root-level configuration should not inherit from some other configuration
		Assert.Null(baseConfiguration.InheritedConfiguration);

		// a root-level configuration should return itself as the root configuration
		Assert.Same(baseConfiguration, baseConfiguration.RootConfiguration);

		// a top-level configuration should not have any parent configurations
		Assert.Null(baseConfiguration.Parent);
		Assert.Equal("/", baseConfiguration.Path);

		// the configuration should not contain any child configurations or items
		Assert.Empty(baseConfiguration.Children);
		Assert.Empty(baseConfiguration.Items);

		// the configuration should not be marked as 'modified' at start
		Assert.False(baseConfiguration.IsModified);

		// the configuration should provide an object to use when locking the configuration
		Assert.NotNull(baseConfiguration.Sync);

		//
		// check whether the inheriting configuration has the expected state
		//

		// the configuration should return the specified name as expected
		Assert.Equal(baseConfigurationName, inheritingConfiguration.Name);

		// the configuration should return the persistence strategy as specified
		Assert.Same(inheritingConfigurationStrategy, inheritingConfiguration.PersistenceStrategy);

		// a root-level configuration should not inherit from some other configuration
		Assert.Same(baseConfiguration, inheritingConfiguration.InheritedConfiguration);

		// a root-level configuration should return itself as the root configuration
		Assert.Same(inheritingConfiguration, inheritingConfiguration.RootConfiguration);

		// a top-level configuration should not have any parent configurations
		Assert.Null(inheritingConfiguration.Parent);
		Assert.Equal("/", inheritingConfiguration.Path);

		// the configuration should not contain any child configurations or items
		Assert.Empty(inheritingConfiguration.Children);
		Assert.Empty(inheritingConfiguration.Items);

		// the configuration should not be marked as 'modified' at start
		Assert.False(inheritingConfiguration.IsModified);

		// the configuration should provide an object to use when locking the configuration
		Assert.NotNull(inheritingConfiguration.Sync);
	}

	#endregion

	#region GetChildConfiguration()

	/// <summary>
	/// Tests creating a child configuration in a base configuration,
	/// i.e. a configuration that does not inherit from another configuration
	/// using <see cref="CascadedConfiguration.GetChildConfiguration"/>.
	/// </summary>
	[Theory]
	[InlineData("/Child1")]
	[InlineData("/Child1/Child2")]
	public void GetChildConfiguration_NestedConfiguration_WithoutInheritance(string path)
	{
		string[] pathTokens = path.Split(sSeparator, StringSplitOptions.RemoveEmptyEntries);

		// create a new root configuration that does not inherit from another configuration
		const string configurationName = "My Configuration";
		ICascadedConfigurationPersistenceStrategy strategy = GetStrategy();
		var rootConfiguration = new CascadedConfiguration(configurationName, strategy);

		// the child configuration should not exist, yet
		CascadedConfiguration childConfiguration = rootConfiguration.GetChildConfiguration(path, false);
		Assert.Null(childConfiguration);

		// create the child configuration
		string expectedChildConfigurationName = pathTokens.Last();
		childConfiguration = rootConfiguration.GetChildConfiguration(path, true);
		Assert.NotNull(childConfiguration);

		// determine the child configuration directly below the root configuration
		// (may differ from the effective child configuration, if an intermediate configuration is created on the way)
		CascadedConfiguration childOfConfiguration = childConfiguration;
		for (int i = 1; i < pathTokens.Length; i++) childOfConfiguration = childOfConfiguration.Parent;

		// determine the child configuration directly above the created configuration
		CascadedConfiguration parentOfChildConfiguration = pathTokens.Length == 1
			                                                   ? rootConfiguration
			                                                   : rootConfiguration.GetChildConfiguration(path[..path.LastIndexOf('/')], false);
		Assert.NotNull(parentOfChildConfiguration);

		//
		// check whether the root configuration has the expected state
		//

		// the configuration should return the specified name as expected
		Assert.Equal(configurationName, rootConfiguration.Name);

		// the configuration should return the persistence strategy as specified
		Assert.Same(strategy, rootConfiguration.PersistenceStrategy);

		// a base configuration should not inherit from some other configuration
		Assert.Null(rootConfiguration.InheritedConfiguration);

		// a root-level configuration should return itself as the root configuration
		Assert.Same(rootConfiguration, rootConfiguration.RootConfiguration);

		// a top-level configuration should not have any parent configurations
		Assert.Null(rootConfiguration.Parent);
		Assert.Equal("/", rootConfiguration.Path);

		// the configuration should contain the created configuration only
		// (or a configuration on the way to the child configuration).
		Assert.Single(rootConfiguration.Children);
		Assert.Same(childOfConfiguration, rootConfiguration.Children.First());

		// the configuration should not contain any items
		Assert.Empty(rootConfiguration.Items);

		// the configuration should be marked as 'modified' as a child configuration has been added
		Assert.True(rootConfiguration.IsModified);

		// the configuration should provide an object to use when locking the configuration
		Assert.NotNull(rootConfiguration.Sync);

		//
		// check whether the child configuration has the expected state
		//

		// the configuration should return the specified name as expected
		Assert.Equal(expectedChildConfigurationName, childConfiguration.Name);

		// the configuration should return the persistence strategy as specified
		Assert.Same(strategy, childConfiguration.PersistenceStrategy);

		// a base configuration should not inherit from some other configuration
		Assert.Null(childConfiguration.InheritedConfiguration);

		// a child configuration should return the root-level configuration as expected
		Assert.Same(rootConfiguration, childConfiguration.RootConfiguration);

		// a child configuration has a parent
		Assert.Same(parentOfChildConfiguration, childConfiguration.Parent);

		// the configuration should not contain any child configurations or items
		Assert.Empty(childConfiguration.Children);
		Assert.Empty(childConfiguration.Items);

		// the configuration should not be marked as 'modified' at start
		Assert.False(childConfiguration.IsModified);

		// the configuration should provide an object to use when locking the configuration
		Assert.Same(rootConfiguration.Sync, childConfiguration.Sync);
	}


	/// <summary>
	/// Tests creating a child configuration on a configuration with inheritance.
	/// using <see cref="CascadedConfiguration.GetChildConfiguration"/>.
	/// </summary>
	[Theory]
	[InlineData("/Child1")]                              // one child configuration
	[InlineData("/Child1/Child2")]                       // one nested child configuration
	[InlineData("/Child11", "/Child12")]                 // two child configurations
	[InlineData("/Child11/Child21", "/Child11/Child22")] // two nested child configurations, first level is the same
	[InlineData("/Child11/Child21", "/Child21/Child22")] // two nested child configurations, first level is different
	public void GetChildConfiguration_NestedConfiguration_WithInheritance(params string[] paths)
	{
		// create a new root configuration that does not inherit from another configuration
		const string configurationName = "My Configuration";
		ICascadedConfigurationPersistenceStrategy baseConfigurationStrategy = GetStrategy();
		var baseRootConfiguration = new CascadedConfiguration(configurationName, baseConfigurationStrategy);

		// create a new root configuration that inherits from the base configuration
		ICascadedConfigurationPersistenceStrategy inheritingConfigurationStrategy = GetStrategy();
		var inheritingRootConfiguration = new CascadedConfiguration(baseRootConfiguration, inheritingConfigurationStrategy);

		// add child configurations
		var baseRootChildConfigurations = new List<CascadedConfiguration>();
		var inheritingRootChildConfigurations = new List<CascadedConfiguration>();
		foreach (string path in paths)
		{
			string[] pathTokens = path.Split(sSeparator, StringSplitOptions.RemoveEmptyEntries);

			// neither the base configuration nor the inheriting configuration should know the child configuration, yet
			CascadedConfiguration baseChildConfiguration = baseRootConfiguration.GetChildConfiguration(path, false);
			Assert.Null(baseChildConfiguration);
			CascadedConfiguration inheritingChildConfiguration = inheritingRootConfiguration.GetChildConfiguration(path, false);
			Assert.Null(inheritingChildConfiguration);

			// create the child configuration in the inheriting configuration
			inheritingChildConfiguration = inheritingRootConfiguration.GetChildConfiguration(path, true);
			Assert.NotNull(inheritingChildConfiguration);

			// creating the child configuration in the inheriting configuration should have created the child
			// configuration in the inherited configuration as well
			baseChildConfiguration = baseRootConfiguration.GetChildConfiguration(path, false);
			Assert.NotNull(baseChildConfiguration);

			// store the child configurations of the root configuration
			string rootChildConfigurationPath = pathTokens[0];
			CascadedConfiguration baseRootChildConfiguration = baseRootConfiguration.GetChildConfiguration(rootChildConfigurationPath, false);
			CascadedConfiguration inheritingRootChildConfiguration = inheritingRootConfiguration.GetChildConfiguration(rootChildConfigurationPath, false);
			if (!baseRootChildConfigurations.Contains(baseRootChildConfiguration)) baseRootChildConfigurations.Add(baseRootChildConfiguration);
			if (!inheritingRootChildConfigurations.Contains(inheritingRootChildConfiguration)) inheritingRootChildConfigurations.Add(inheritingRootChildConfiguration);
		}

		// check whether the child configuration have been added correctly
		foreach (string path in paths)
		{
			string[] pathTokens = path.Split(sSeparator, StringSplitOptions.RemoveEmptyEntries);
			string expectedChildConfigurationName = pathTokens.Last();

			// get the configurations created in the previous step
			CascadedConfiguration baseChildConfiguration = baseRootConfiguration.GetChildConfiguration(path, false);
			CascadedConfiguration inheritingChildConfiguration = inheritingRootConfiguration.GetChildConfiguration(path, false);
			Assert.NotNull(baseChildConfiguration);
			Assert.NotNull(inheritingChildConfiguration);

			// determine the child configuration directly below the root configuration
			// (may differ from the effective child configuration, if an intermediate configuration is created on the way)
			CascadedConfiguration childOfBaseRootConfiguration = baseChildConfiguration;
			CascadedConfiguration childOfInheritingRootConfiguration = inheritingChildConfiguration;
			for (int i = 1; i < pathTokens.Length; i++) childOfBaseRootConfiguration = childOfBaseRootConfiguration.Parent;
			for (int i = 1; i < pathTokens.Length; i++) childOfInheritingRootConfiguration = childOfInheritingRootConfiguration.Parent;

			// determine the child configuration directly above the created configuration
			string parentOfChildConfigurationPath = path[..path.LastIndexOf('/')];
			CascadedConfiguration parentOfBaseChildConfiguration = pathTokens.Length == 1 ? baseRootConfiguration : baseRootConfiguration.GetChildConfiguration(parentOfChildConfigurationPath, false);
			CascadedConfiguration parentOfInheritingChildConfiguration = pathTokens.Length == 1 ? inheritingRootConfiguration : inheritingRootConfiguration.GetChildConfiguration(parentOfChildConfigurationPath, false);
			Assert.NotNull(parentOfBaseChildConfiguration);
			Assert.NotNull(parentOfInheritingChildConfiguration);

			// all configurations should share the same synchronization object
			object syncObject = baseRootConfiguration.Sync;
			Assert.NotNull(syncObject);

			//
			// check whether the base root configuration has the expected state
			//

			// the configuration should return the specified name as expected
			Assert.Equal(configurationName, baseRootConfiguration.Name);

			// the configuration should return the persistence strategy as specified
			Assert.Same(baseConfigurationStrategy, baseRootConfiguration.PersistenceStrategy);

			// a base configuration should not inherit from some other configuration
			Assert.Null(baseRootConfiguration.InheritedConfiguration);

			// a root-level configuration should return itself as the root configuration
			Assert.Same(baseRootConfiguration, baseRootConfiguration.RootConfiguration);

			// a top-level configuration should not have any parent configurations
			Assert.Null(baseRootConfiguration.Parent);
			Assert.Equal("/", baseRootConfiguration.Path);

			// the configuration should contain the created configurations only
			// (or a configuration on the way to the child configuration).
			Assert.Equal(baseRootChildConfigurations, baseRootConfiguration.Children);

			// the configuration should not contain any items
			Assert.Empty(baseRootConfiguration.Items);

			// the configuration should be marked as 'modified' as a child configuration has been added
			Assert.True(baseRootConfiguration.IsModified);

			// the configuration should provide an object to use when locking the configuration
			// (it should be the same throughout the entire configuration cascade)
			Assert.Same(syncObject, baseRootConfiguration.Sync);

			//
			// check whether the inheriting root configuration has the expected state
			//

			// the configuration should return the specified name as expected
			Assert.Equal(configurationName, inheritingRootConfiguration.Name);

			// the configuration should return the persistence strategy as specified
			Assert.Same(inheritingConfigurationStrategy, inheritingRootConfiguration.PersistenceStrategy);

			// the configuration should inherit from the base configuration
			Assert.Same(baseRootConfiguration, inheritingRootConfiguration.InheritedConfiguration);

			// a root-level configuration should return itself as the root configuration
			Assert.Same(inheritingRootConfiguration, inheritingRootConfiguration.RootConfiguration);

			// a top-level configuration should not have any parent configurations
			Assert.Null(inheritingRootConfiguration.Parent);
			Assert.Equal("/", inheritingRootConfiguration.Path);

			// the configuration should contain the created configurations only
			// (or a configuration on the way to the child configuration).
			Assert.Equal(inheritingRootChildConfigurations, inheritingRootConfiguration.Children);

			// the configuration should not contain any items
			Assert.Empty(inheritingRootConfiguration.Items);

			// the configuration should be marked as 'modified' as a child configuration has been added
			Assert.True(inheritingRootConfiguration.IsModified);

			// the configuration should provide an object to use when locking the configuration
			// (it should be the same throughout the entire configuration cascade)
			Assert.Same(syncObject, inheritingRootConfiguration.Sync);

			//
			// check whether the child configuration in the base configuration has the expected state
			//

			// the configuration should return the specified name as expected
			Assert.Equal(expectedChildConfigurationName, baseChildConfiguration.Name);

			// the configuration should return the persistence strategy as specified
			Assert.Same(baseConfigurationStrategy, baseChildConfiguration.PersistenceStrategy);

			// a base configuration should not inherit from some other configuration
			Assert.Null(baseChildConfiguration.InheritedConfiguration);

			// a child configuration should return the root-level configuration as expected
			Assert.Same(baseRootConfiguration, baseChildConfiguration.RootConfiguration);

			// a child configuration has a parent
			Assert.Same(parentOfBaseChildConfiguration, baseChildConfiguration.Parent);

			// the configuration should not contain any child configurations or items
			Assert.Empty(baseChildConfiguration.Children);
			Assert.Empty(baseChildConfiguration.Items);

			// the configuration should not be marked as 'modified' at start
			Assert.False(baseChildConfiguration.IsModified);

			// the configuration should provide an object to use when locking the configuration
			// (it should be the same throughout the entire configuration cascade)
			Assert.Same(syncObject, baseChildConfiguration.Sync);

			//
			// check whether the child configuration in the inheriting configuration has the expected state
			//

			// the configuration should return the specified name as expected
			Assert.Equal(expectedChildConfigurationName, inheritingChildConfiguration.Name);

			// the configuration should return the persistence strategy as specified
			Assert.Same(inheritingConfigurationStrategy, inheritingChildConfiguration.PersistenceStrategy);

			// the configuration should inherit from the base configuration
			Assert.Same(baseChildConfiguration, inheritingChildConfiguration.InheritedConfiguration);

			// a child configuration should return the root-level configuration as expected
			Assert.Same(inheritingRootConfiguration, inheritingChildConfiguration.RootConfiguration);

			// a child configuration has a parent
			Assert.Same(parentOfInheritingChildConfiguration, inheritingChildConfiguration.Parent);

			// the configuration should not contain any child configurations or items
			Assert.Empty(inheritingChildConfiguration.Children);
			Assert.Empty(inheritingChildConfiguration.Items);

			// the configuration should not be marked as 'modified' at start
			Assert.False(inheritingChildConfiguration.IsModified);

			// the configuration should provide an object to use when locking the configuration
			// (it should be the same throughout the entire configuration cascade)
			Assert.Same(syncObject, inheritingChildConfiguration.Sync);
		}
	}


	/// <summary>
	/// Tests whether <see cref="CascadedConfiguration.GetChildConfiguration"/> throws an exception,
	/// if the specified path is <c>null</c>.
	/// </summary>
	/// <param name="create">
	/// <c>true</c> to create a configuration, if it does not exist;
	/// otherwise <c>false</c>.
	/// </param>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void GetChildConfiguration_PathNull(bool create)
	{
		// create a new root configuration that does not inherit from another configuration
		ICascadedConfigurationPersistenceStrategy strategy = GetStrategy();
		var configuration = new CascadedConfiguration("My Configuration", strategy);

		// check whether GetChildConfiguration() throws an exception if the specified path is null
		Assert.Throws<ArgumentNullException>(
			() =>
			{
				configuration.GetChildConfiguration(null, create);
			});
	}

	#endregion

	#region Clear()

	/// <summary>
	/// Test data for the test methods targeting <see cref="CascadedConfiguration.Clear"/>.
	/// </summary>
	public static IEnumerable<object[]> ClearTestData
	{
		get
		{
			string[][] childConfigurationPathsList =
			[
				Array.Empty<string>(),                                                       // no child configurations at all
				["/Child"],                                                                  // one child configuration
				["/Child1", "/Child2"],                                                      // two child configurations
				["/Child1/Child21", "/Child1/Child22", "/Child2/Child21", "/Child2/Child22"] // mix of configurations
			];

			string[][] itemPathsList =
			[
				Array.Empty<string>(),                                                                         // no items at all
				["/Value"],                                                                                    // one item
				["/Value1", "/Value2"],                                                                        // two items
				["/Child/Value1", "/Child/Value2"],                                                            // item nested in child configuration
				["/Value1", "/Child1/Value1", "/Child1/Value2", "/Value2", "/Child2/Value1", "/Child2/Value2"] // mix of items
			];

			foreach (int inheritanceLevels in new[] { 1, 2 })
			foreach (string[] childConfigurationPaths in childConfigurationPathsList)
			foreach (string[] itemPaths in itemPathsList)
			{
				yield return
				[
					inheritanceLevels,
					childConfigurationPaths,
					itemPaths
				];
			}
		}
	}


	/// <summary>
	/// Tests clearing a configuration with various combinations of child configurations,
	/// items and levels of inheritance using <see cref="CascadedConfiguration.Clear"/>.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="childConfigurationPaths">Paths of child configurations to add.</param>
	/// <param name="itemPaths">Paths of configurations items to add.</param>
	[Theory]
	[MemberData(nameof(ClearTestData))]
	public void Clear_NonEmpty(
		int      inheritanceLevels,
		string[] childConfigurationPaths,
		string[] itemPaths)
	{
		//
		// create a new configuration with the specified number of inheritance levels
		//

		const string configurationName = "My Configuration";
		CascadedConfiguration configuration = null;
		CascadedConfiguration inheritedConfiguration = null;
		ICascadedConfigurationPersistenceStrategy strategy = null;
		for (int inheritanceLevel = 0; inheritanceLevel < inheritanceLevels; inheritanceLevel++)
		{
			inheritedConfiguration = configuration;
			strategy = GetStrategy();
			configuration = configuration != null
				                ? new CascadedConfiguration(configuration, strategy)
				                : new CascadedConfiguration(configurationName, strategy);
		}

		Debug.Assert(configuration != null, nameof(configuration) + " != null");

		// create child configurations in the configuration
		foreach (string path in childConfigurationPaths)
		{
			configuration.GetChildConfiguration(path, true);
		}

		// create items without a value in the configuration
		foreach (string path in itemPaths)
		{
			configuration.SetItem<int>(path);
		}

		//
		// clear the configuration
		//

		configuration.Clear();

		//
		// check the state of the cleared configuration
		//

		// the configuration should return the specified name as expected
		Assert.Equal(configurationName, configuration.Name);

		// the configuration should return the persistence strategy as specified
		Assert.Same(strategy, configuration.PersistenceStrategy);

		// a base configuration should not inherit from some other configuration, inheriting configurations should do this
		if (inheritanceLevels > 1) Assert.Same(inheritedConfiguration, configuration.InheritedConfiguration);
		else Assert.Null(configuration.InheritedConfiguration);

		// a root-level configuration should return itself as the root configuration
		Assert.Same(configuration, configuration.RootConfiguration);

		// a top-level configuration should not have any parent configurations
		Assert.Null(configuration.Parent);
		Assert.Equal("/", configuration.Path);

		// the configuration should not contain any child configurations or items
		Assert.Empty(configuration.Children);
		Assert.Empty(configuration.Items);

		// the configuration should be marked as 'modified' if child configurations and/or items have been added and removed
		Assert.Equal(childConfigurationPaths.Length > 0 || itemPaths.Length > 0, configuration.IsModified);

		// the configuration should provide an object to use when locking the configuration
		// (it should be the same throughout the entire configuration cascade)
		Assert.NotNull(configuration.Sync);
	}

	#endregion

	#region GetAllItems()

	/// <summary>
	/// Test data for the test methods targeting <see cref="CascadedConfiguration.GetAllItems"/>.
	/// </summary>
	public static IEnumerable<object[]> GetAllItemsTestData
	{
		get
		{
			string[][][] itemPathsList =
			[
				// no items at all
				[
					Array.Empty<string>(), // configuration content
					Array.Empty<string>(), // top-level items
					Array.Empty<string>()  // all items
				],

				// one item
				[
					["/Value"], // configuration content
					["/Value"], // top-level items
					["/Value"]  // all items
				],

				// two items
				[
					["/Value1", "/Value2"], // configuration content
					["/Value1", "/Value2"], // top-level items
					["/Value1", "/Value2"]  // all items
				],

				// item nested in child configuration
				[
					["/Child/Value1", "/Child/Value2"], // configuration content
					Array.Empty<string>(),              // top-level items
					["/Child/Value1", "/Child/Value2"]  // all items
				],

				// mix of items
				[
					["/Value1", "/Child1/Value1", "/Child1/Value2", "/Value2", "/Child2/Value1", "/Child2/Value2"], // configuration content
					["/Value1", "/Value2"],                                                                         // top-level items
					["/Value1", "/Child1/Value1", "/Child1/Value2", "/Value2", "/Child2/Value1", "/Child2/Value2"]  // all items
				]
			];

			foreach (int inheritanceLevels in new[] { 1, 2 })
			foreach (bool recursively in new[] { false, true })
			foreach (string[][] itemPaths in itemPathsList)
			{
				yield return
				[
					inheritanceLevels,
					recursively,
					itemPaths[0],
					itemPaths[recursively ? 2 : 1]
				];
			}
		}
	}


	/// <summary>
	/// Tests getting all items with various combinations of child configurations,
	/// items and levels of inheritance using <see cref="CascadedConfiguration.GetAllItems"/>.
	/// </summary>
	[Theory]
	[MemberData(nameof(GetAllItemsTestData))]
	public void GetAllItems(
		int      inheritanceLevels,
		bool     recursively,
		string[] setupItemPaths,
		string[] expectedItemPaths)
	{
		// create a new configuration with the specified number of inheritance levels
		const string configurationName = "My Configuration";
		CascadedConfiguration configuration = null;
		for (int inheritanceLevel = 0; inheritanceLevel < inheritanceLevels; inheritanceLevel++)
		{
			ICascadedConfigurationPersistenceStrategy strategy = GetStrategy();
			configuration = configuration != null
				                ? new CascadedConfiguration(configuration, strategy)
				                : new CascadedConfiguration(configurationName, strategy);
		}

		Debug.Assert(configuration != null, nameof(configuration) + " != null");

		// create items without a value in the configuration
		foreach (string path in setupItemPaths)
		{
			configuration.SetItem<int>(path);
		}

		// get all items in the configuration
		ICascadedConfigurationItem[] items = configuration.GetAllItems(recursively);
		IEnumerable<string> actualItemPaths = items.Select(x => x.Path);
		Assert.Equal(expectedItemPaths.OrderBy(x => x), actualItemPaths.OrderBy(x => x));
	}

	#endregion

	#region SetItem(), SetItem<T>()

	#region Test Data

	public static IEnumerable<object[]> SetItemTestData =>
		from inheritanceLevel in new[] { 1, 2 }
		from data in ItemTestDataWithoutValue
		select (object[])
		[
			inheritanceLevel,
			data[0],
			data[1]
		];

	#endregion

	#region Common Test Code

	/// <summary>
	/// Common test code:
	/// Checks whether the specified item has the expected state after running <see cref="CascadedConfiguration.SetItem"/>
	/// or <see cref="CascadedConfiguration.SetItem{T}"/>.
	/// </summary>
	/// <param name="rootConfiguration">The root configuration the item belongs to.</param>
	/// <param name="item">The configuration item to check.</param>
	/// <param name="expectedItemValueType">The expected item value type.</param>
	private static void SetItem_CheckCreatedItemCommon(
		CascadedConfiguration      rootConfiguration,
		ICascadedConfigurationItem item,
		Type                       expectedItemValueType)
	{
		// get the configuration the item is in (without using the item's Configuration property)
		string configurationPath = item.Path[..item.Path.LastIndexOf('/')];
		CascadedConfiguration itemConfiguration = configurationPath.Length > 0 ? rootConfiguration.GetChildConfiguration(configurationPath, false) : rootConfiguration;

		// shorten access to the persistence strategy
		ICascadedConfigurationPersistenceStrategy strategy = rootConfiguration.PersistenceStrategy;

		// check the state of the item
		string itemName = item.Path[(item.Path.LastIndexOf('/') + 1)..];
		Assert.Equal(itemName, item.Name);
		Assert.Equal(expectedItemValueType, item.Type);
		Assert.Same(itemConfiguration, item.Configuration);
		Assert.False(item.HasComment);
		Assert.Null(item.Comment);
		Assert.Equal(strategy == null || strategy.SupportsComments, item.SupportsComments);
		Assert.False(item.HasValue);
		Assert.Throws<ConfigurationException>(() => item.Value);
	}


	/// <summary>
	/// Common test code:
	/// Checks whether all items have been created appropriately after running <see cref="CascadedConfiguration.SetItem"/>
	/// or <see cref="CascadedConfiguration.SetItem{T}"/>.
	/// </summary>
	/// <param name="rootConfiguration">The root configuration the item belongs to.</param>
	/// <param name="itemPaths">Paths of items that should have been created.</param>
	/// <param name="expectedItemValueType">The expected item value type.</param>
	private static void SetItem_CheckCreatedItemsCommon(
		CascadedConfiguration rootConfiguration,
		IEnumerable<string>   itemPaths,
		Type                  expectedItemValueType)
	{
		// check whether all items have been created appropriately
		ICascadedConfigurationItem[] items = [.. CollectAllItemsInConfiguration(rootConfiguration).OrderBy(x => x.Path)];
		Assert.Equal(itemPaths.OrderBy(x => x), items.Select(x => x.Path));
		Assert.All(
			items,
			item =>
			{
				SetItem_CheckCreatedItemCommon(
					rootConfiguration,
					item,
					expectedItemValueType);
			});
	}

	#endregion

	#region SetItem(string, Type)

	/// <summary>
	/// Tests creating an item without a value using <see cref="CascadedConfiguration.SetItem"/>.
	/// The item does not exist yet, so it should be created.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to create.</param>
	/// <param name="itemValueType">Type of the value the items should store.</param>
	[Theory]
	[MemberData(nameof(SetItemTestData))]
	public void SetItem_ItemDoesNotExistYet(int inheritanceLevels, string[] itemPaths, Type itemValueType)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		// create items using SetItem()
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = rootConfiguration.SetItem(itemPath, itemValueType);
			SetItem_CheckCreatedItemCommon(rootConfiguration, item, itemValueType);
		}

		// check whether all items have been created appropriately
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType);
	}


	/// <summary>
	/// Tests creating an item without a value using <see cref="CascadedConfiguration.SetItem"/>.
	/// The item exists already, so the existing item should be returned.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to create.</param>
	/// <param name="itemValueType">Type of the value the items should store.</param>
	[Theory]
	[MemberData(nameof(SetItemTestData))]
	public void SetItem_ItemExistsAlready(int inheritanceLevels, string[] itemPaths, Type itemValueType)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		// create items regularly using SetItem()
		var newItems = new Queue<ICascadedConfigurationItem>();
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = rootConfiguration.SetItem(itemPath, itemValueType);
			SetItem_CheckCreatedItemCommon(rootConfiguration, item, itemValueType);
			newItems.Enqueue(item);
		}

		// check whether all items have been created appropriately
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType);

		// check whether SetItem<T>() succeeds and returns the same items when calling once again
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = rootConfiguration.SetItem(itemPath, itemValueType);
			Assert.Same(newItems.Dequeue(), item);
		}

		// check whether all items still have the same state
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType);
	}


	/// <summary>
	/// Tests setting an item without a value using <see cref="CascadedConfiguration.SetItem"/>.
	/// The item exists already, but its value type is different.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to create.</param>
	/// <param name="itemValueType">Type of the value the items should store.</param>
	[Theory]
	[MemberData(nameof(SetItemTestData))]
	public void SetItem_ItemWithDifferentValueTypeExists(int inheritanceLevels, string[] itemPaths, Type itemValueType)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		// create items regularly using SetItem()
		Type differentItemValueType = itemValueType == typeof(int) ? typeof(long) : typeof(int);
		var newItems = new Queue<ICascadedConfigurationItem>();
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = rootConfiguration.SetItem(itemPath, differentItemValueType);
			SetItem_CheckCreatedItemCommon(rootConfiguration, item, differentItemValueType);
			newItems.Enqueue(item);
		}

		// check whether all items have been created appropriately
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, differentItemValueType);

		// check whether SetItem() fails, if the value type is different
		foreach (string itemPath in itemPaths)
		{
			Assert.Throws<ConfigurationException>(() => rootConfiguration.SetItem(itemPath, itemValueType));
		}

		// check whether all items still have the same state
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, differentItemValueType);
	}


	/// <summary>
	/// Tests whether <see cref="CascadedConfiguration.SetItem"/> throws an exception,
	/// if the specified path is <c>null</c>.
	/// </summary>
	[Fact]
	public void SetItem_PathNull()
	{
		CascadedConfiguration configuration = CreateConfiguration(1);
		Assert.Throws<ArgumentNullException>(() => configuration.SetItem(null, typeof(int)));
	}


	/// <summary>
	/// Tests whether <see cref="CascadedConfiguration.SetItem"/> throws an exception,
	/// if the specified type is <c>null</c>.
	/// </summary>
	[Fact]
	public void SetItem_TypeNull()
	{
		CascadedConfiguration configuration = CreateConfiguration(1);
		Assert.Throws<ArgumentNullException>(() => configuration.SetItem("/Value", null));
	}

	#endregion

	#region SetItem<T>(string)

	/// <summary>
	/// Tests creating an item without a value using <see cref="CascadedConfiguration.SetItem{T}"/>.
	/// The item does not exist yet, so it should be created.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to create.</param>
	/// <param name="itemValueType">Type of the value the items should store.</param>
	[Theory]
	[MemberData(nameof(SetItemTestData))]
	public void SetItemT_ItemDoesNotExistYet(int inheritanceLevels, string[] itemPaths, Type itemValueType)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		MethodInfo method = rootConfiguration
			.GetType()
			.GetMethods()
			.Single(
				x =>
					x.Name == nameof(CascadedConfiguration.SetItem) &&
					x.IsGenericMethodDefinition &&
					x.GetGenericArguments().Length == 1 &&
					x.GetParameters().Length == 1)
			.MakeGenericMethod(itemValueType);

		// create items using SetItem<T>()
		foreach (string itemPath in itemPaths)
		{
			var item = (ICascadedConfigurationItem)method.Invoke(rootConfiguration, [itemPath]);
			SetItem_CheckCreatedItemCommon(rootConfiguration, item, itemValueType);
		}

		// check whether all items have been created appropriately
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType);
	}


	/// <summary>
	/// Tests creating an item without a value using <see cref="CascadedConfiguration.SetItem{T}"/>.
	/// The item exists already, so the existing item should be returned.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to create.</param>
	/// <param name="itemValueType">Type of the value the items should store.</param>
	[Theory]
	[MemberData(nameof(SetItemTestData))]
	public void SetItemT_ItemExistsAlready(int inheritanceLevels, string[] itemPaths, Type itemValueType)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		MethodInfo method = rootConfiguration
			.GetType()
			.GetMethods()
			.Single(
				x =>
					x.Name == nameof(CascadedConfiguration.SetItem) &&
					x.IsGenericMethodDefinition &&
					x.GetGenericArguments().Length == 1 &&
					x.GetParameters().Length == 1)
			.MakeGenericMethod(itemValueType);

		// create items regularly using SetItem<T>()
		var newItems = new Queue<ICascadedConfigurationItem>();
		foreach (string itemPath in itemPaths)
		{
			var item = (ICascadedConfigurationItem)method.Invoke(rootConfiguration, [itemPath]);
			SetItem_CheckCreatedItemCommon(rootConfiguration, item, itemValueType);
			newItems.Enqueue(item);
		}

		// check whether all items have been created appropriately
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType);

		// check whether SetItem<T>() succeeds and returns the same items when calling once again
		foreach (string itemPath in itemPaths)
		{
			var item = (ICascadedConfigurationItem)method.Invoke(rootConfiguration, [itemPath]);
			Assert.Same(newItems.Dequeue(), item);
		}

		// check whether all items still have the same state
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType);
	}


	/// <summary>
	/// Tests creating an item without a value using <see cref="CascadedConfiguration.SetItem{T}"/>.
	/// The item exists already, but its value type is different.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to create.</param>
	/// <param name="itemValueType">Type of the value the items should store.</param>
	[Theory]
	[MemberData(nameof(SetItemTestData))]
	public void SetItemT_ItemWithDifferentValueTypeExists(int inheritanceLevels, string[] itemPaths, Type itemValueType)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		// create items regularly using SetItem()
		Type differentItemValueType = itemValueType == typeof(int) ? typeof(long) : typeof(int);
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = rootConfiguration.SetItem(itemPath, differentItemValueType);
			SetItem_CheckCreatedItemCommon(rootConfiguration, item, differentItemValueType);
		}

		// check whether all items have been created appropriately
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, differentItemValueType);

		// check whether SetItem<T>() fails, if the value type is different
		MethodInfo method = rootConfiguration
			.GetType()
			.GetMethods()
			.Single(
				x =>
					x.Name == nameof(CascadedConfiguration.SetItem) &&
					x.IsGenericMethodDefinition &&
					x.GetGenericArguments().Length == 1 &&
					x.GetParameters().Length == 1)
			.MakeGenericMethod(itemValueType);
		foreach (string itemPath in itemPaths)
		{
			Assert.IsType<ConfigurationException>(
				Assert.Throws<TargetInvocationException>(
						() => method.Invoke(
							rootConfiguration,
							[itemPath]))
					.InnerException);
		}

		// check whether all items still have the same state
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, differentItemValueType);
	}


	/// <summary>
	/// Tests whether <see cref="CascadedConfiguration.SetItem{T}"/> throws an exception,
	/// if the specified path is <c>null</c>.
	/// </summary>
	[Fact]
	public void SetItemT_PathNull()
	{
		CascadedConfiguration configuration = CreateConfiguration(1);
		Assert.Throws<ArgumentNullException>(() => configuration.SetItem<int>(null));
	}

	#endregion

	#endregion

	#region GetItem(), GetItem<T>()

	#region Test Data

	public static IEnumerable<object[]> GetItemTestData
	{
		get
		{
			return
				from inheritanceLevel in new[] { 1, 2 }
				from data in ItemTestDataWithoutValue
				select (object[])
				[
					inheritanceLevel,
					data[0],
					data[1]
				];
		}
	}

	/// <summary>
	/// Test data for test methods targeting <see cref="CascadedConfiguration.GetItem"/> and <see cref="CascadedConfiguration.GetItem{T}"/>
	/// covering the case that the item to get does not exist.
	/// </summary>
	public static IEnumerable<object[]> GetItem_ItemDoesNotExist_TestData
	{
		get
		{
			string[] itemPathList =
			[
				"/Value",      // single value in the root configuration
				"/Child/Value" // single value in a child configuration
			];

			foreach (int inheritanceLevels in new[] { 1, 2 })
			foreach (string itemPath in itemPathList)
			{
				yield return
				[
					inheritanceLevels,
					itemPath
				];
			}
		}
	}

	#endregion

	#region GetItem(string)

	/// <summary>
	/// Tests getting an item using <see cref="CascadedConfiguration.GetItem"/>.
	/// The item does not exist when the method is called.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPath">Path of the item to get.</param>
	[Theory]
	[MemberData(nameof(GetItem_ItemDoesNotExist_TestData))]
	public void GetItem_ItemDoesNotExist(int inheritanceLevels, string itemPath)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		// try to get item using GetItem()
		ICascadedConfigurationItem item = rootConfiguration.GetItem(itemPath);
		Assert.Null(item);

		// ensure that no configurations/items have been created at all
		IEnumerable<string> paths = CollectElementPaths(rootConfiguration);
		Assert.Empty(paths);
	}


	/// <summary>
	/// Tests getting an item using <see cref="CascadedConfiguration.GetItem"/>.
	/// The item exists when the method is called.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to get.</param>
	/// <param name="itemValueType">Type of the value the item should store.</param>
	[Theory]
	[MemberData(nameof(GetItemTestData))] // use same test data as for SetItem() tests as SetItem() is used to prepare the configuration
	public void GetItem_ItemExists(int inheritanceLevels, string[] itemPaths, Type itemValueType)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		// create items using SetItem() (assumes that SetItem() is working as expected, other tests ensure this)
		foreach (string itemPath in itemPaths)
		{
			rootConfiguration.SetItem(itemPath, itemValueType);
		}

		// check whether all items have been created appropriately
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType);

		// try to get items using GetItem()
		foreach (string itemPath in itemPaths)
		{
			// get the item using its path
			ICascadedConfigurationItem item = rootConfiguration.GetItem(itemPath);

			// the item should have the same state as produced by the SetItem() method
			SetItem_CheckCreatedItemCommon(rootConfiguration, item, itemValueType);
		}

		// check whether all items still have the same state
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType);
	}


	/// <summary>
	/// Tests whether <see cref="CascadedConfiguration.GetItem"/> throws an exception,
	/// if the specified path is <c>null</c>.
	/// </summary>
	[Fact]
	public void GetItem_PathNull()
	{
		CascadedConfiguration configuration = CreateConfiguration(1);
		Assert.Throws<ArgumentNullException>(() => configuration.GetItem(null));
	}

	#endregion

	#region GetItem<T>(string)

	/// <summary>
	/// Tests getting an item using <see cref="CascadedConfiguration.GetItem"/>.
	/// The item does not exist when the method is called.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPath">Path of the item to get.</param>
	[Theory]
	[MemberData(nameof(GetItem_ItemDoesNotExist_TestData))]
	public void GetItemT_ItemDoesNotExist(int inheritanceLevels, string itemPath)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		MethodInfo method = rootConfiguration
			.GetType()
			.GetMethods()
			.Single(
				x =>
					x.Name == nameof(CascadedConfiguration.GetItem) &&
					x.IsGenericMethodDefinition &&
					x.GetGenericArguments().Length == 1 &&
					x.GetParameters().Length == 1)
			.MakeGenericMethod(typeof(string)); // the value type does not matter as the item does not exist...

		// try to get item using GetItem<T>()
		var item = (ICascadedConfigurationItem)method.Invoke(rootConfiguration, [itemPath]);
		Assert.Null(item);

		// ensure that no configurations/items have been created at all
		IEnumerable<string> paths = CollectElementPaths(rootConfiguration);
		Assert.Empty(paths);
	}


	/// <summary>
	/// Tests getting an item using <see cref="CascadedConfiguration.GetItem{T}"/>.
	/// The item exists when the method is called.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to get.</param>
	/// <param name="itemValueType">Type of the value the item should store.</param>
	[Theory]
	[MemberData(nameof(GetItemTestData))] // use same test data as for SetItem() tests as SetItem() is used to prepare the configuration
	public void GetItemT_ItemExists(int inheritanceLevels, string[] itemPaths, Type itemValueType)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		// create items using SetItem() (assumes that SetItem() is working as expected, other tests ensure this)
		foreach (string itemPath in itemPaths)
		{
			rootConfiguration.SetItem(itemPath, itemValueType);
		}

		// check whether all items have been created appropriately
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType);

		// get GetItem<T>() method to invoke
		MethodInfo method = rootConfiguration
			.GetType()
			.GetMethods()
			.Single(
				x =>
					x.Name == nameof(CascadedConfiguration.GetItem) &&
					x.IsGenericMethodDefinition &&
					x.GetGenericArguments().Length == 1 &&
					x.GetParameters().Length == 1)
			.MakeGenericMethod(itemValueType);

		// try to get the items
		foreach (string itemPath in itemPaths)
		{
			// get the item using its path
			var item = (ICascadedConfigurationItem)method.Invoke(rootConfiguration, [itemPath]);

			// the item should have the same state as produced by the SetItem() method
			SetItem_CheckCreatedItemCommon(rootConfiguration, item, itemValueType);
		}

		// check whether all items still have the same state
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType);
	}


	/// <summary>
	/// Tests getting an item using <see cref="CascadedConfiguration.GetItem{T}"/>.
	/// The item exists already, but its value type is different.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to get.</param>
	/// <param name="itemValueType">Type of the value the items should store.</param>
	[Theory]
	[MemberData(nameof(GetItemTestData))]
	public void GetItemT_ItemWithDifferentValueTypeExists(int inheritanceLevels, string[] itemPaths, Type itemValueType)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		// create items regularly using SetItem()
		Type differentItemValueType = itemValueType == typeof(int) ? typeof(long) : typeof(int);
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = rootConfiguration.SetItem(itemPath, differentItemValueType);
			SetItem_CheckCreatedItemCommon(rootConfiguration, item, differentItemValueType);
		}

		// check whether all items have been created appropriately
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, differentItemValueType);

		// check whether GetItem<T>() fails, if the value type is different
		MethodInfo method = rootConfiguration
			.GetType()
			.GetMethods()
			.Single(
				x =>
					x.Name == nameof(CascadedConfiguration.GetItem) &&
					x.IsGenericMethodDefinition &&
					x.GetGenericArguments().Length == 1 &&
					x.GetParameters().Length == 1)
			.MakeGenericMethod(itemValueType);
		foreach (string itemPath in itemPaths)
		{
			Assert.IsType<ConfigurationException>(
				Assert.Throws<TargetInvocationException>(
						() => method.Invoke(
							rootConfiguration,
							[itemPath]))
					.InnerException);
		}

		// check whether all items still have the same state
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, differentItemValueType);
	}


	/// <summary>
	/// Tests whether <see cref="CascadedConfiguration.GetItem{T}"/> throws an exception,
	/// if the specified path is <c>null</c>.
	/// </summary>
	[Fact]
	public void GetItemT_PathNull()
	{
		CascadedConfiguration configuration = CreateConfiguration(1);
		Assert.Throws<ArgumentNullException>(() => configuration.GetItem<int>(null));
	}

	#endregion

	#endregion

	#region SetValue(), SetValue<T>()

	#region Test Data

	public static IEnumerable<object[]> SetValueTestData
	{
		get
		{
			return
				from inheritanceLevel in new[] { 1, 2 }
				from data in ItemTestDataWithValue
				select (object[])
				[
					inheritanceLevel,
					data[0],
					data[1],
					data[2]
				];
		}
	}

	#endregion

	#region Common Test Code

	/// <summary>
	/// Common test code:
	/// Checks whether the specified item has the expected state after running <see cref="CascadedConfiguration.SetValue{T}"/>.
	/// </summary>
	/// <param name="rootConfiguration">The root configuration the item belongs to.</param>
	/// <param name="item">The configuration item to check.</param>
	/// <param name="expectedItemValueType">The expected item value type.</param>
	/// <param name="expectedItemValue">The expected item value.</param>
	private static void SetValue_CheckCreatedItemCommon(
		CascadedConfiguration      rootConfiguration,
		ICascadedConfigurationItem item,
		Type                       expectedItemValueType,
		object                     expectedItemValue)
	{
		// get the configuration the item is in (without using the item's Configuration property)
		string configurationPath = item.Path[..item.Path.LastIndexOf('/')];
		CascadedConfiguration itemConfiguration = configurationPath.Length > 0
			                                          ? rootConfiguration.GetChildConfiguration(configurationPath, false)
			                                          : rootConfiguration;

		// shorten access to the persistence strategy
		ICascadedConfigurationPersistenceStrategy strategy = rootConfiguration.PersistenceStrategy;

		// check the state of the item
		string itemName = item.Path[(item.Path.LastIndexOf('/') + 1)..];
		Assert.Equal(itemName, item.Name);
		Assert.Equal(expectedItemValueType, item.Type);
		Assert.Same(itemConfiguration, item.Configuration);
		Assert.False(item.HasComment);
		Assert.Null(item.Comment);
		Assert.Equal(strategy == null || strategy.SupportsComments, item.SupportsComments);
		Assert.True(item.HasValue);
		Assert.Equal(expectedItemValue, item.Value);
	}


	/// <summary>
	/// Common test code:
	/// Checks whether all items have been created/set appropriately after running <see cref="CascadedConfiguration.SetValue{T}"/>.
	/// </summary>
	/// <param name="rootConfiguration">The root configuration the item belongs to.</param>
	/// <param name="itemPaths">Paths of items that should have been created.</param>
	/// <param name="expectedItemValueType">The expected item value type.</param>
	/// <param name="expectedItemValue">The expected item value.</param>
	private static void SetValue_CheckCreatedItemsCommon(
		CascadedConfiguration rootConfiguration,
		IEnumerable<string>   itemPaths,
		Type                  expectedItemValueType,
		object                expectedItemValue)
	{
		// check whether all items have been created appropriately
		ICascadedConfigurationItem[] items = [.. CollectAllItemsInConfiguration(rootConfiguration).OrderBy(x => x.Path)];
		Assert.Equal(itemPaths.OrderBy(x => x), items.Select(x => x.Path));
		Assert.All(
			items,
			item =>
			{
				SetValue_CheckCreatedItemCommon(
					rootConfiguration,
					item,
					expectedItemValueType,
					expectedItemValue);
			});
	}

	#endregion

	#region SetValue(string, Type, object)

	/// <summary>
	/// Tests creating a new item providing an initial value using <see cref="CascadedConfiguration.SetValue(string,Type,object)"/>.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to create/set.</param>
	/// <param name="itemValueType">Type of the value to set.</param>
	/// <param name="itemValue">Value to set.</param>
	[Theory]
	[MemberData(nameof(SetValueTestData))]
	public void SetValue_ItemDoesNotExistYet(
		int      inheritanceLevels,
		string[] itemPaths,
		Type     itemValueType,
		object   itemValue)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		// create items using SetValue()
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = rootConfiguration.SetValue(itemPath, itemValueType, itemValue);
			SetValue_CheckCreatedItemCommon(rootConfiguration, item, itemValueType, itemValue);
		}

		// check whether all items have been created appropriately
		SetValue_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType, itemValue);
	}


	/// <summary>
	/// Tests setting a value using <see cref="CascadedConfiguration.SetValue(string,Type,object)"/>.
	/// The item exists already and the item value is assignable.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to create.</param>
	/// <param name="itemValueType">Type of the item value.</param>
	/// <param name="itemValue">Value to set to the item.</param>
	[Theory]
	[MemberData(nameof(SetValueTestData))]
	public void SetValue_ItemExistsAlready(
		int      inheritanceLevels,
		string[] itemPaths,
		Type     itemValueType,
		object   itemValue)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		// create items regularly using SetItem()
		// (the item is added to the configuration, but it does not have a value, yet)
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = rootConfiguration.SetItem(itemPath, itemValueType);
			SetItem_CheckCreatedItemCommon(rootConfiguration, item, itemValueType);
		}

		// check whether all items have been created appropriately
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType);

		// check whether SetValue() succeeds
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = rootConfiguration.SetValue(itemPath, itemValueType, itemValue);
			SetValue_CheckCreatedItemCommon(rootConfiguration, item, itemValueType, itemValue);
		}

		// check whether the configuration has the expected state after setting the values
		SetValue_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType, itemValue);
	}


	/// <summary>
	/// Tests setting a value using <see cref="CascadedConfiguration.SetValue(string,Type,object)"/>.
	/// The item exists already, but its value type is different.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to create/set.</param>
	/// <param name="itemValueType">Type of the item value.</param>
	/// <param name="itemValue">Value to set to the item.</param>
	[Theory]
	[MemberData(nameof(SetValueTestData))]
	public void SetValue_ItemWithDifferentValueTypeExists(
		int      inheritanceLevels,
		string[] itemPaths,
		Type     itemValueType,
		object   itemValue)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		// create items regularly using SetItem()
		// (the item is added to the configuration, but it does not have a value, yet)
		Type differentItemValueType = itemValueType == typeof(int) ? typeof(long) : typeof(int);
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = rootConfiguration.SetItem(itemPath, differentItemValueType);
			SetItem_CheckCreatedItemCommon(rootConfiguration, item, differentItemValueType);
		}

		// check whether all items have been created appropriately
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, differentItemValueType);

		// check whether SetValue() fails, if the value type is different
		foreach (string itemPath in itemPaths)
		{
			Assert.Throws<ConfigurationException>(() => rootConfiguration.SetValue(itemPath, itemValueType, itemValue));
		}

		// check whether all items still have the same state
		// (item should not have been modified due to the error)
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, differentItemValueType);
	}


	/// <summary>
	/// Tests whether <see cref="CascadedConfiguration.SetValue"/> throws an exception,
	/// if the specified path is <c>null</c>.
	/// </summary>
	[Fact]
	public void SetValue_PathNull()
	{
		CascadedConfiguration configuration = CreateConfiguration(1);
		Assert.Throws<ArgumentNullException>(() => configuration.SetValue(null, typeof(int), 0));
	}


	/// <summary>
	/// Tests whether <see cref="CascadedConfiguration.SetValue"/> throws an exception,
	/// if the specified type is <c>null</c>.
	/// </summary>
	[Fact]
	public void SetValue_TypeNull()
	{
		CascadedConfiguration configuration = CreateConfiguration(1);
		Assert.Throws<ArgumentNullException>(() => configuration.SetValue("/Value", null, 0));
	}

	#endregion

	#region SetValue<T>(string, T)

	/// <summary>
	/// Tests creating a new item providing an initial value using <see cref="CascadedConfiguration.SetValue{T}"/>.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to create/set.</param>
	/// <param name="itemValueType">Type of the value to set.</param>
	/// <param name="itemValue">Value to set.</param>
	[Theory]
	[MemberData(nameof(SetValueTestData))]
	public void SetValueT_ItemDoesNotExistYet(
		int      inheritanceLevels,
		string[] itemPaths,
		Type     itemValueType,
		object   itemValue)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		MethodInfo method = rootConfiguration
			.GetType()
			.GetMethods()
			.Where(x => x.Name == nameof(CascadedConfiguration.SetValue) && x.GetParameters().Length == 2)
			.Single(x => x.IsGenericMethodDefinition && x.GetGenericArguments().Length == 1)
			.MakeGenericMethod(itemValueType);

		// create items using SetValue<T>()
		foreach (string itemPath in itemPaths)
		{
			var item = (ICascadedConfigurationItem)method.Invoke(rootConfiguration, [itemPath, itemValue]);
			SetValue_CheckCreatedItemCommon(rootConfiguration, item, itemValueType, itemValue);
		}

		// check whether all items have been created appropriately
		SetValue_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType, itemValue);
	}


	/// <summary>
	/// Tests setting a value using <see cref="CascadedConfiguration.SetValue{T}"/>.
	/// The item exists already and the item value is assignable.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to create.</param>
	/// <param name="itemValueType">Type of the item value.</param>
	/// <param name="itemValue">Value to set to the item.</param>
	[Theory]
	[MemberData(nameof(SetValueTestData))]
	public void SetValueT_ItemExistsAlready(
		int      inheritanceLevels,
		string[] itemPaths,
		Type     itemValueType,
		object   itemValue)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		// create items regularly using SetItem()
		// (the item is added to the configuration, but it does not have a value, yet)
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = rootConfiguration.SetItem(itemPath, itemValueType);
			SetItem_CheckCreatedItemCommon(rootConfiguration, item, itemValueType);
		}

		// check whether all items have been created appropriately
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType);

		// prepare access to SetValue<T>()
		MethodInfo method = rootConfiguration
			.GetType()
			.GetMethods()
			.Single(
				x =>
					x.Name == nameof(CascadedConfiguration.SetValue) &&
					x.IsGenericMethodDefinition &&
					x.GetGenericArguments().Length == 1 &&
					x.GetParameters().Length == 2)
			.MakeGenericMethod(itemValueType);

		// check whether SetValue<T>() succeeds
		foreach (string itemPath in itemPaths)
		{
			var item = (ICascadedConfigurationItem)method.Invoke(rootConfiguration, [itemPath, itemValue]);
			SetValue_CheckCreatedItemCommon(rootConfiguration, item, itemValueType, itemValue);
		}

		// check whether the configuration has the expected state after setting the values
		SetValue_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType, itemValue);
	}


	/// <summary>
	/// Tests setting a value using <see cref="CascadedConfiguration.SetValue{T}"/>.
	/// The item exists already, but its value type is different.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to create/set.</param>
	/// <param name="itemValueType">Type of the item value.</param>
	/// <param name="itemValue">Value to set to the item.</param>
	[Theory]
	[MemberData(nameof(SetValueTestData))]
	public void SetValueT_ItemWithDifferentValueTypeExists(
		int      inheritanceLevels,
		string[] itemPaths,
		Type     itemValueType,
		object   itemValue)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		// create items regularly using SetItem()
		// (the item is added to the configuration, but it does not have a value, yet)
		Type differentItemValueType = itemValueType == typeof(int) ? typeof(long) : typeof(int);
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = rootConfiguration.SetItem(itemPath, differentItemValueType);
			SetItem_CheckCreatedItemCommon(rootConfiguration, item, differentItemValueType);
		}

		// check whether all items have been created appropriately
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, differentItemValueType);

		// check whether SetValue<T>() fails, if the value type is different
		MethodInfo method = rootConfiguration
			.GetType()
			.GetMethods()
			.Single(
				x =>
					x.Name == nameof(CascadedConfiguration.SetValue) &&
					x.IsGenericMethodDefinition &&
					x.GetGenericArguments().Length == 1 &&
					x.GetParameters().Length == 2)
			.MakeGenericMethod(itemValueType);
		foreach (string itemPath in itemPaths)
		{
			Assert.IsType<ConfigurationException>(
				Assert.Throws<TargetInvocationException>(
						() => method.Invoke(
							rootConfiguration,
							[itemPath, itemValue]))
					.InnerException);
		}

		// check whether all items still have the same state
		// (item should not have been modified due to the error)
		SetItem_CheckCreatedItemsCommon(rootConfiguration, itemPaths, differentItemValueType);
	}


	/// <summary>
	/// Tests whether <see cref="CascadedConfiguration.SetValue{T}"/> throws an exception,
	/// if the specified path is <c>null</c>.
	/// </summary>
	[Fact]
	public void SetValueT_PathNull()
	{
		CascadedConfiguration configuration = CreateConfiguration(1);
		Assert.Throws<ArgumentNullException>(() => configuration.SetValue(null, 0));
	}

	#endregion

	#endregion

	#region GetValue<T>()

	#region Test Data

	/// <summary>
	/// Test data with various item types supported by the conversion subsystem out of the box (with item values).
	/// </summary>
	public static IEnumerable<object[]> GetValueTestData_WithValue
	{
		get
		{
			return
				from inheritanceLevel in new[] { 1, 2 }
				from data in ItemTestDataWithValue
				select (object[])
				[
					inheritanceLevel,
					data[0],
					data[1],
					data[2]
				];
		}
	}

	/// <summary>
	/// Test data with various item types supported by the conversion subsystem out of the box (without item values).
	/// </summary>
	public static IEnumerable<object[]> GetValueTestData_WithoutValue
	{
		get
		{
			return
				from inheritanceLevel in new[] { 1, 2 }
				from data in ItemTestDataWithoutValue
				select (object[])
				[
					inheritanceLevel,
					data[0],
					data[1]
				];
		}
	}

	#endregion

	#region GetValue<T>(string, T)

	/// <summary>
	/// Tests getting a value using <see cref="CascadedConfiguration.GetValue{T}"/>.
	/// The item with the requested value exists in the most derived configuration.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to create/set.</param>
	/// <param name="itemValueType">Type of the value to set.</param>
	/// <param name="itemValue">Value to set.</param>
	[Theory]
	[MemberData(nameof(GetValueTestData_WithValue))]
	public void GetValueT_ItemExistsInMostDerivedConfiguration(
		int      inheritanceLevels,
		string[] itemPaths,
		Type     itemValueType,
		object   itemValue)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration rootConfiguration = CreateConfiguration(inheritanceLevels);

		// create items using SetValue()
		var items = new Queue<ICascadedConfigurationItem>();
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = rootConfiguration.SetValue(itemPath, itemValueType, itemValue);
			SetValue_CheckCreatedItemCommon(rootConfiguration, item, itemValueType, itemValue);
			items.Enqueue(item);
		}

		// check whether all items have been created appropriately
		SetValue_CheckCreatedItemsCommon(rootConfiguration, itemPaths, itemValueType, itemValue);

		// try to get these items using GetValue()
		// (the items should return the same value as the items returned by SetValue() above)
		MethodInfo method = rootConfiguration
			.GetType()
			.GetMethods()
			.Where(x => x.Name == nameof(CascadedConfiguration.GetValue) && x.GetParameters().Length == 2)
			.Single(x => x.IsGenericMethodDefinition && x.GetGenericArguments().Length == 1)
			.MakeGenericMethod(itemValueType);
		foreach (string itemPath in itemPaths)
		{
			// values are in the most derived configuration, so the value of 'inherit' should not matter
			object expectedItemValue = items.Dequeue().Value;
			object actualItemValue1 = method.Invoke(rootConfiguration, [itemPath, false]);
			Assert.Equal(expectedItemValue, actualItemValue1);
			object actualItemValue2 = method.Invoke(rootConfiguration, [itemPath, true]);
			Assert.Equal(expectedItemValue, actualItemValue2);
		}
	}


	/// <summary>
	/// Tests getting a value using <see cref="CascadedConfiguration.GetValue{T}"/>.
	/// The item with the requested value exists in the base configuration.
	/// </summary>
	/// <param name="itemPaths">Paths of items to create/set.</param>
	/// <param name="itemValueType">Type of the value to set.</param>
	/// <param name="itemValue">Value to set.</param>
	[Theory]
	[MemberData(nameof(ItemTestDataWithValue))]
	public void GetValueT_ItemExistsInInheritedConfiguration(
		string[] itemPaths,
		Type     itemValueType,
		object   itemValue)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration derivedConfiguration = CreateConfiguration(2);
		CascadedConfiguration inheritedConfiguration = derivedConfiguration.InheritedConfiguration;

		// create items using SetValue() on the inherited configuration
		var items = new Queue<object>();
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = inheritedConfiguration.SetValue(itemPath, itemValueType, itemValue);
			SetValue_CheckCreatedItemCommon(inheritedConfiguration, item, itemValueType, itemValue);
			items.Enqueue(item.Value);
		}

		// check whether all items have been created appropriately
		SetValue_CheckCreatedItemsCommon(inheritedConfiguration, itemPaths, itemValueType, itemValue);

		// try to get these items using GetValue() on the derived configuration
		// (the items should return the same value as the items returned by SetValue() above)
		MethodInfo method = derivedConfiguration
			.GetType()
			.GetMethods()
			.Where(x => x.Name == nameof(CascadedConfiguration.GetValue) && x.GetParameters().Length == 2)
			.Single(x => x.IsGenericMethodDefinition && x.GetGenericArguments().Length == 1)
			.MakeGenericMethod(itemValueType);
		foreach (string itemPath in itemPaths)
		{
			object expectedItemValue = items.Dequeue();

			// the most derived configuration should not contain the item and therefore no value...
			var ex = Assert.Throws<TargetInvocationException>(() => method.Invoke(derivedConfiguration, [itemPath, false]));
			Assert.IsType<ConfigurationException>(ex.InnerException);

			// ...but the inherited configuration contains an item with the requested value
			object actualItemValue2 = method.Invoke(derivedConfiguration, [itemPath, true]);
			Assert.Equal(expectedItemValue, actualItemValue2);
		}
	}


	/// <summary>
	/// Tests setting a value using <see cref="CascadedConfiguration.GetValue{T}"/>.
	/// The item exists, but its value type is different.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to create/set.</param>
	/// <param name="itemValueType">Type of the item value.</param>
	[Theory]
	[MemberData(nameof(GetValueTestData_WithoutValue))]
	public void GetValueT_ItemWithDifferentValueTypeExistsInMostDerivedConfiguration(
		int      inheritanceLevels,
		string[] itemPaths,
		Type     itemValueType)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration derivedConfiguration = CreateConfiguration(inheritanceLevels);

		// create items regularly using SetValue()
		Type differentItemValueType;
		object differentItemValue;
		if (itemValueType == typeof(int))
		{
			differentItemValueType = typeof(long);
			differentItemValue = 0L;
		}
		else
		{
			differentItemValueType = typeof(int);
			differentItemValue = 0;
		}

		foreach (string itemPath in itemPaths)
		{
			derivedConfiguration.SetValue(itemPath, differentItemValueType, differentItemValue);
		}

		// check whether GetValue<T>() fails, if the value type is different
		MethodInfo method = derivedConfiguration
			.GetType()
			.GetMethods()
			.Single(
				x =>
					x.Name == nameof(CascadedConfiguration.GetValue) &&
					x.IsGenericMethodDefinition &&
					x.GetGenericArguments().Length == 1 &&
					x.GetParameters().Length == 2)
			.MakeGenericMethod(itemValueType);
		foreach (string itemPath in itemPaths)
		{
			const bool inherit = true;
			Assert.IsType<ConfigurationException>(
				Assert.Throws<TargetInvocationException>(
						() => method.Invoke(
							derivedConfiguration,
							[itemPath, inherit]))
					.InnerException);
		}
	}


	/// <summary>
	/// Tests setting a value using <see cref="CascadedConfiguration.GetValue{T}"/>.
	/// The item exists, but its value type is different.
	/// </summary>
	/// <param name="itemPaths">Paths of items to create/set.</param>
	/// <param name="itemValueType">Type of the item value.</param>
	[Theory]
	[MemberData(nameof(ItemTestDataWithoutValue))]
	public void GetValueT_ItemWithDifferentValueTypeExistsInInheritedConfiguration(
		string[] itemPaths,
		Type     itemValueType)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration derivedConfiguration = CreateConfiguration(2);
		CascadedConfiguration inheritedConfiguration = derivedConfiguration.InheritedConfiguration;

		// create items using SetValue() on the inherited configuration
		Type differentItemValueType;
		object differentItemValue;
		if (itemValueType == typeof(int))
		{
			differentItemValueType = typeof(long);
			differentItemValue = 0L;
		}
		else
		{
			differentItemValueType = typeof(int);
			differentItemValue = 0;
		}

		foreach (string itemPath in itemPaths)
		{
			inheritedConfiguration.SetValue(itemPath, differentItemValueType, differentItemValue);
		}

		// check whether GetValue<T>() fails, if the value type is different
		MethodInfo method = derivedConfiguration
			.GetType()
			.GetMethods()
			.Single(
				x =>
					x.Name == nameof(CascadedConfiguration.GetValue) &&
					x.IsGenericMethodDefinition &&
					x.GetGenericArguments().Length == 1 &&
					x.GetParameters().Length == 2)
			.MakeGenericMethod(itemValueType);
		foreach (string itemPath in itemPaths)
		{
			const bool inherit = true;
			Assert.IsType<ConfigurationException>(
				Assert.Throws<TargetInvocationException>(
						() => method.Invoke(
							derivedConfiguration,
							[itemPath, inherit]))
					.InnerException);
		}
	}


	/// <summary>
	/// Tests whether <see cref="CascadedConfiguration.GetValue{T}"/> throws an exception,
	/// if the specified path is <c>null</c>.
	/// </summary>
	[Fact]
	public void GetValueT_PathNull()
	{
		CascadedConfiguration configuration = CreateConfiguration(1);
		Assert.Throws<ArgumentNullException>(() => configuration.GetValue<int>(null, false));
	}

	#endregion

	#endregion

	#region GetComment()

	#region Test Data

	/// <summary>
	/// Test data with various item types supported by the conversion subsystem out of the box (without item values).
	/// </summary>
	public static IEnumerable<object[]> GetCommentTestData
	{
		get
		{
			return
				from inheritanceLevel in new[] { 1, 2 }
				from data in ItemTestDataWithoutValue
				select (object[])
				[
					inheritanceLevel,
					data[0],
					data[1]
				];
		}
	}

	#endregion

	#region GetComment(string)

	/// <summary>
	/// Tests getting a comment using <see cref="CascadedConfiguration.GetComment"/>.
	/// The item with the requested comment exists in the most derived configuration.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <param name="itemPaths">Paths of items to create/set.</param>
	/// <param name="itemValueType">Type of the value to set.</param>
	[Theory]
	[MemberData(nameof(GetCommentTestData))]
	public void GetComment_ItemExistsInMostDerivedConfiguration(
		int      inheritanceLevels,
		string[] itemPaths,
		Type     itemValueType)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration derivedConfiguration = CreateConfiguration(inheritanceLevels);

		// create items using SetItem() and set the comment explicitly
		const string testComment = "This is a comment!";
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = derivedConfiguration.SetItem(itemPath, itemValueType);
			SetItem_CheckCreatedItemCommon(derivedConfiguration, item, itemValueType);
			item.Comment = testComment;
		}

		// try to get the comment using GetComment()
		foreach (string itemPath in itemPaths)
		{
			// values are in the most derived configuration, so the value of 'inherit' should not matter
			string comment1 = derivedConfiguration.GetComment(itemPath, false);
			Assert.Equal(testComment, comment1);
			string comment2 = derivedConfiguration.GetComment(itemPath, true);
			Assert.Equal(testComment, comment2);
		}
	}


	/// <summary>
	/// Tests getting a comment using <see cref="CascadedConfiguration.GetComment"/>.
	/// The item with the requested comment exists in the base configuration.
	/// </summary>
	/// <param name="itemPaths">Paths of items to create/set.</param>
	/// <param name="itemValueType">Type of the value to set.</param>
	[Theory]
	[MemberData(nameof(ItemTestDataWithoutValue))]
	public void GetComment_ItemExistsInInheritedConfiguration(
		string[] itemPaths,
		Type     itemValueType)
	{
		// create a new configuration with the specified number of inheritance levels
		CascadedConfiguration derivedConfiguration = CreateConfiguration(2);
		CascadedConfiguration inheritedConfiguration = derivedConfiguration.InheritedConfiguration;

		// create items using SetItem() and set the comment explicitly
		const string testComment = "This is a comment!";
		foreach (string itemPath in itemPaths)
		{
			ICascadedConfigurationItem item = inheritedConfiguration.SetItem(itemPath, itemValueType);
			SetItem_CheckCreatedItemCommon(inheritedConfiguration, item, itemValueType);
			item.Comment = testComment;
		}

		// try to get the comment using GetComment()
		foreach (string itemPath in itemPaths)
		{
			// the most derived configuration should not contain the item and therefore no comment...
			string comment1 = derivedConfiguration.GetComment(itemPath, false);
			Assert.Null(comment1);

			// ...but the inherited configuration contains an item with the requested comment
			string comment2 = derivedConfiguration.GetComment(itemPath, true);
			Assert.Equal(testComment, comment2);
		}
	}


	/// <summary>
	/// Tests whether <see cref="CascadedConfiguration.GetComment"/> throws an exception,
	/// if the specified path is <c>null</c>.
	/// </summary>
	[Fact]
	public void GetComment_PathNull()
	{
		CascadedConfiguration configuration = CreateConfiguration(1);
		Assert.Throws<ArgumentNullException>(() => configuration.GetComment(null, false));
	}

	#endregion

	#endregion

	#region ResetItems()

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ResetItems(bool recursive)
	{
		// create a new configuration with no inherited configurations
		// (resetting does not affect inherited configurations)
		CascadedConfiguration rootConfiguration = CreateConfiguration(1);

		// populate configuration with some test data
		CascadedConfigurationItem<int> itemC0V1 = rootConfiguration.SetValue("MyValue1", 1);
		CascadedConfigurationItem<int> itemC0V2 = rootConfiguration.SetValue("MyValue2", 2);
		CascadedConfigurationItem<int> itemC1V1 = rootConfiguration.SetValue("Child1/MyValue1", 1);
		CascadedConfigurationItem<int> itemC1V2 = rootConfiguration.SetValue("Child1/MyValue2", 2);
		CascadedConfigurationItem<int> itemC2V1 = rootConfiguration.SetValue("Child2/MyValue1", 1);
		CascadedConfigurationItem<int> itemC2V2 = rootConfiguration.SetValue("Child2/MyValue2", 2);
		var items = new List<ICascadedConfigurationItem> { itemC0V1, itemC0V2, itemC1V1, itemC1V2, itemC2V1, itemC2V2 };

		// all items should have a value now
		Assert.All(items, item => Assert.True(item.HasValue));

		// reset items
		rootConfiguration.ResetItems(recursive);

		// the items on the same configuration level should always be reset
		Assert.False(itemC0V1.HasValue);
		Assert.False(itemC0V2.HasValue);

		// child items should have a value, if not resetting recursively
		bool childItemsShouldHaveValue = !recursive;
		Assert.Equal(childItemsShouldHaveValue, itemC1V1.HasValue);
		Assert.Equal(childItemsShouldHaveValue, itemC1V2.HasValue);
		Assert.Equal(childItemsShouldHaveValue, itemC2V1.HasValue);
		Assert.Equal(childItemsShouldHaveValue, itemC2V2.HasValue);
	}

	#endregion

	#region RemoveItem()

	/// <summary>
	/// Test data for the test method targeting <see cref="CascadedConfiguration.RemoveItem"/>.
	/// </summary>
	public static IEnumerable<object[]> RemoveItemTestData
	{
		get
		{
			// remove the only item
			yield return
			[
				new[] { "/Value" },   // configuration content
				new[] { "/Value" },   // item to remove
				Array.Empty<string>() // configuration content after removing the item
			];

			// remove first item of two items
			yield return
			[
				new[] { "/Value1", "/Value2" }, // configuration content
				new[] { "/Value1" },            // item to remove
				new[] { "/Value2" }             // configuration content after removing the item
			];

			// remove second item of two items
			yield return
			[
				new[] { "/Value1", "/Value2" }, // configuration content
				new[] { "/Value2" },            // item to remove
				new[] { "/Value1" }             // configuration content after removing the item
			];

			// remove first item nested in child configuration
			yield return
			[
				new[] { "/Child/Value1", "/Child/Value2" }, // configuration content
				new[] { "/Child/Value1" },                  // item to remove
				new[] { "/Child/Value2" }                   // configuration content after removing the item
			];

			// remove second item nested in child configuration
			yield return
			[
				new[] { "/Child/Value1", "/Child/Value2" }, // configuration content
				new[] { "/Child/Value2" },                  // item to remove
				new[] { "/Child/Value1" }                   // configuration content after removing the item
			];

			// remove first top-level item of a mix of items
			yield return
			[
				new[] { "/Value1", "/Child1/Value1", "/Child1/Value2", "/Value2", "/Child2/Value1", "/Child2/Value2" }, // configuration content
				new[] { "/Value1" },                                                                                    // item to remove
				new[] { "/Child1/Value1", "/Child1/Value2", "/Value2", "/Child2/Value1", "/Child2/Value2" }             // configuration content after removing the item
			];

			// remove second top-level item of a mix of items
			yield return
			[
				new[] { "/Value1", "/Child1/Value1", "/Child1/Value2", "/Value2", "/Child2/Value1", "/Child2/Value2" }, // configuration content
				new[] { "/Value2" },                                                                                    // item to remove
				new[] { "/Value1", "/Child1/Value1", "/Child1/Value2", "/Child2/Value1", "/Child2/Value2" }             // configuration content after removing the item
			];

			// remove first item in the first child configuration of a mix of items
			yield return
			[
				new[] { "/Value1", "/Child1/Value1", "/Child1/Value2", "/Value2", "/Child2/Value1", "/Child2/Value2" }, // configuration content
				new[] { "/Child1/Value1" },                                                                             // item to remove
				new[] { "/Value1", "/Child1/Value2", "/Value2", "/Child2/Value1", "/Child2/Value2" }                    // configuration content after removing the item
			];

			// remove second item in the first child configuration of a mix of items
			yield return
			[
				new[] { "/Value1", "/Child1/Value1", "/Child1/Value2", "/Value2", "/Child2/Value1", "/Child2/Value2" }, // configuration content
				new[] { "/Child1/Value2" },                                                                             // item to remove
				new[] { "/Value1", "/Child1/Value1", "/Value2", "/Child2/Value1", "/Child2/Value2" }                    // configuration content after removing the item
			];

			// remove first item in the second child configuration of a mix of items
			yield return
			[
				new[] { "/Value1", "/Child1/Value1", "/Child1/Value2", "/Value2", "/Child2/Value1", "/Child2/Value2" }, // configuration content
				new[] { "/Child2/Value1" },                                                                             // item to remove
				new[] { "/Value1", "/Child1/Value1", "/Child1/Value2", "/Value2", "/Child2/Value2" }                    // configuration content after removing the item
			];

			// remove second item in the second child configuration of a mix of items
			yield return
			[
				new[] { "/Value1", "/Child1/Value1", "/Child1/Value2", "/Value2", "/Child2/Value1", "/Child2/Value2" }, // configuration content
				new[] { "/Child2/Value2" },                                                                             // item to remove
				new[] { "/Value1", "/Child1/Value1", "/Child1/Value2", "/Value2", "/Child2/Value1" }                    // configuration content after removing the item
			];

			// try to remove top-level item that does not exist
			yield return
			[
				new[] { "/Value", "/Child/Value" }, // configuration content
				new[] { "/DoesNotExist" },          // item to remove
				new[] { "/Value", "/Child/Value" }  // configuration content after removing the item
			];

			// try to remove item in child configuration that does not exist
			yield return
			[
				new[] { "/Value", "/Child/Value" },       // configuration content
				new[] { "/ChildThatDoesNotExist/Value" }, // item to remove
				new[] { "/Value", "/Child/Value" }        // configuration content after removing the item
			];
		}
	}

	private static readonly char[] sSeparator = ['/'];

	/// <summary>
	/// Tests removing items using <see cref="CascadedConfiguration.RemoveItem"/>.
	/// </summary>
	/// <param name="setupItemPaths">Paths of items to initialize the configuration with before removing items.</param>
	/// <param name="removeItemPaths">Paths of items to remove.</param>
	/// <param name="expectedRemainingPaths">Paths of items that are expected to be in the configuration after removing items.</param>
	[Theory]
	[MemberData(nameof(RemoveItemTestData))]
	public void RemoveItem(
		string[] setupItemPaths,
		string[] removeItemPaths,
		string[] expectedRemainingPaths)
	{
		// create a new configuration with no inherited configurations
		CascadedConfiguration configuration = CreateConfiguration(1);

		// create items without a value in the configuration
		foreach (string path in setupItemPaths)
		{
			configuration.SetItem<int>(path);
		}

		// remove items
		foreach (string path in removeItemPaths)
		{
			bool shouldBeRemoved = setupItemPaths.Contains(path);
			bool removed = configuration.RemoveItem(path);
			Assert.Equal(shouldBeRemoved, removed);
		}

		IEnumerable<ICascadedConfigurationItem> itemsAfterRemoving = CollectAllItemsInConfiguration(configuration);
		IEnumerable<string> itemsAfterRemovingPaths = itemsAfterRemoving.Select(x => x.Path);
		Assert.Equal(expectedRemainingPaths.OrderBy(x => x), itemsAfterRemovingPaths.OrderBy(x => x));
	}


	/// <summary>
	/// Tests whether <see cref="CascadedConfiguration.RemoveItem"/> throws an exception,
	/// if the specified path is <c>null</c>.
	/// </summary>
	[Fact]
	public void RemoveValue_PathNull()
	{
		CascadedConfiguration configuration = CreateConfiguration(1);
		Assert.Throws<ArgumentNullException>(() => configuration.RemoveItem(null));
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Recursively collects all items in the specified configuration.
	/// </summary>
	/// <param name="configuration">Configuration from which to collect the items.</param>
	/// <returns>All items in the specified configuration.</returns>
	private static IEnumerable<ICascadedConfigurationItem> CollectAllItemsInConfiguration(CascadedConfiguration configuration)
	{
		var items = new List<ICascadedConfigurationItem>();
		Collect(configuration, items);
		return items;

		static void Collect(CascadedConfiguration config, ICollection<ICascadedConfigurationItem> list)
		{
			foreach (ICascadedConfigurationItem item in config.Items)
			{
				list.Add(item);
			}

			foreach (CascadedConfiguration child in config.Children)
			{
				Collect(child, list);
			}
		}
	}


	/// <summary>
	/// Recursively collects paths of child configurations of the specified configuration and items within these configurations.
	/// </summary>
	/// <param name="configuration">Configuration from which to start.</param>
	/// <returns>Paths of all child configurations and items in the specified configuration.</returns>
	private static IEnumerable<string> CollectElementPaths(CascadedConfiguration configuration)
	{
		var items = new List<string>();
		Collect(configuration, items);
		return items;

		static void Collect(CascadedConfiguration config, ICollection<string> list)
		{
			foreach (ICascadedConfigurationItem item in config.Items)
			{
				list.Add(item.Path);
			}

			foreach (CascadedConfiguration child in config.Children)
			{
				list.Add(child.Path + "/");
				Collect(child, list);
			}
		}
	}


	/// <summary>
	/// Creates a stacked configuration with the specified number of inherited configurations.
	/// </summary>
	/// <param name="inheritanceLevels">
	/// Number of stacked configurations
	/// (1 = base configuration only, 2 = one configuration inheriting from a base configuration).
	/// </param>
	/// <returns>The most inherited root configuration.</returns>
	private CascadedConfiguration CreateConfiguration(int inheritanceLevels)
	{
		// create a new configuration with the specified number of inheritance levels
		const string configurationName = "My Configuration";
		CascadedConfiguration rootConfiguration = null;
		for (int inheritanceLevel = 0; inheritanceLevel < inheritanceLevels; inheritanceLevel++)
		{
			ICascadedConfigurationPersistenceStrategy strategy = GetStrategy();
			rootConfiguration = rootConfiguration != null
				                    ? new CascadedConfiguration(rootConfiguration, strategy)
				                    : new CascadedConfiguration(configurationName, strategy);
		}

		Debug.Assert(rootConfiguration != null, nameof(rootConfiguration) + " != null");

		return rootConfiguration;
	}

	#endregion
}
