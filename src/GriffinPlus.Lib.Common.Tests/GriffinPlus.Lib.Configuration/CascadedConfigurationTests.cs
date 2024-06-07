///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

using Xunit;

using Type = System.Type;

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// Tests targeting all classes of the configuration system together, namely:<br/>
/// - <see cref="DefaultCascadedConfiguration"/><br/>
/// - <see cref="CascadedConfiguration"/><br/>
/// - <see cref="CascadedConfigurationItem{T}"/><br/>
/// - <see cref="XmlFilePersistenceStrategy"/>
/// </summary>
public class CascadedConfigurationTests
{
	private static readonly char[] sSeparator               = ['/'];
	private const           string XmlConfigurationFilePath = "CascadedXmlFileConfigurationTest.xml";

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

	/// <summary>
	/// Some information about an item in a configuration.
	/// </summary>
	/// <param name="path">Absolute path of the item in the configuration.</param>
	/// <param name="type">Value type of the item.</param>
	/// <param name="defaultValue">Value of the item in the base configuration (default value).</param>
	/// <param name="baseItem">Item object in the base configuration.</param>
	public class ItemInfo(
		string                     path,
		Type                       type,
		object                     defaultValue,
		ICascadedConfigurationItem baseItem)
	{
		/// <summary>
		/// Absolute path the item in the configuration.
		/// </summary>
		public string Path = path;

		/// <summary>
		/// Value type of the item.
		/// </summary>
		public Type Type = type;

		/// <summary>
		/// Value of the item in the base configuration (default value).
		/// </summary>
		public object DefaultValue = defaultValue;

		/// <summary>
		/// Associated item objects in a prepared configuration:<br/>
		/// - index = 0: item in the base configuration<br/>
		/// - index &gt; 0: item in a derived configuration (the greater the index, the more derived is the configuration the item is in).
		/// </summary>
		public List<ICascadedConfigurationItem> Items = [baseItem];
	}

	#endregion

	#region Common Test Data

	/// <summary>
	/// Helper property that assists with building test data. It only creates a <see cref="XmlFilePersistenceStrategy"/> instance.
	/// </summary>
	private static XmlFilePersistenceStrategy Persistence => new("Temp.xml");

	/// <summary>
	/// Examples for configuration stacks with different depths and persistence strategies.<br/>
	/// The examples range from a configuration that consists of a base configuration only up to a
	/// configuration with three inheriting configurations with and without persistence strategies on top.
	/// </summary>
	private static readonly ICascadedConfigurationPersistenceStrategy[][] sInheritedConfigurationPersistenceStrategies =
	[                                                 // --- configuration stack ---
		[null],                                       // base | -------------- | -------------- | --------------
		[null, null],                                 // base | no persistence | -------------- | --------------
		[null, null, null],                           // base | no persistence | no persistence | --------------
		[null, null, Persistence],                    // base | no persistence | persistence    | --------------
		[null, null, null, null],                     // base | no persistence | no persistence | no persistence
		[null, null, null, Persistence],              // base | no persistence | no persistence | persistence
		[null, null, Persistence, null],              // base | no persistence | persistence    | no persistence
		[null, null, Persistence, Persistence],       // base | no persistence | persistence    | persistence
		[null, Persistence],                          // base | persistence    | -------------- | --------------
		[null, Persistence, null],                    // base | persistence    | no persistence | --------------
		[null, Persistence, Persistence],             // base | persistence    | persistence    | --------------
		[null, Persistence, null, null],              // base | persistence    | no persistence | no persistence
		[null, Persistence, null, Persistence],       // base | persistence    | no persistence | persistence
		[null, Persistence, Persistence, null],       // base | persistence    | persistence    | no persistence
		[null, Persistence, Persistence, Persistence] // base | persistence    | persistence    | persistence
	];

	private struct ValueTypeAndDefaultValue(Type type, object defaultValue)
	{
		public readonly Type   ValueType    = type;
		public readonly object DefaultValue = defaultValue;
	}

	/// <summary>
	/// Types that are supported by the Griffin+ conversion system.
	/// </summary>
	private static IEnumerable<ValueTypeAndDefaultValue> ItemTypesWithDefaultValues
	{
		get
		{
			// System.SByte
			yield return new ValueTypeAndDefaultValue(typeof(sbyte), (sbyte)1);
			yield return new ValueTypeAndDefaultValue(typeof(sbyte[]), new sbyte[] { 1 });

			// System.Byte
			yield return new ValueTypeAndDefaultValue(typeof(byte), (byte)1);
			yield return new ValueTypeAndDefaultValue(typeof(byte[]), new byte[] { 1 });

			// System.Int16
			yield return new ValueTypeAndDefaultValue(typeof(short), (short)1);
			yield return new ValueTypeAndDefaultValue(typeof(short[]), new short[] { 1 });

			// System.UInt16
			yield return new ValueTypeAndDefaultValue(typeof(ushort), (ushort)1);
			yield return new ValueTypeAndDefaultValue(typeof(ushort[]), new ushort[] { 1 });

			// System.Int32
			yield return new ValueTypeAndDefaultValue(typeof(int), 1);
			yield return new ValueTypeAndDefaultValue(typeof(int[]), new[] { 1 });

			// System.UInt32
			yield return new ValueTypeAndDefaultValue(typeof(uint), 1U);
			yield return new ValueTypeAndDefaultValue(typeof(uint[]), new uint[] { 1 });

			// System.Int64
			yield return new ValueTypeAndDefaultValue(typeof(long), 1L);
			yield return new ValueTypeAndDefaultValue(typeof(long[]), new long[] { 1 });

			// System.UInt64
			yield return new ValueTypeAndDefaultValue(typeof(ulong), 1UL);
			yield return new ValueTypeAndDefaultValue(typeof(ulong[]), new ulong[] { 1 });

			// System.Single
			yield return new ValueTypeAndDefaultValue(typeof(float), 1.0f);
			yield return new ValueTypeAndDefaultValue(typeof(float[]), new float[] { 1 });

			// System.Double
			yield return new ValueTypeAndDefaultValue(typeof(double), 1.0d);
			yield return new ValueTypeAndDefaultValue(typeof(double[]), new double[] { 1 });

			// System.Decimal
			yield return new ValueTypeAndDefaultValue(typeof(decimal), 1.0M);
			yield return new ValueTypeAndDefaultValue(typeof(decimal[]), new decimal[] { 1 });

			// System.String
			yield return new ValueTypeAndDefaultValue(typeof(string), "Test");
			yield return new ValueTypeAndDefaultValue(typeof(string[]), new[] { "Test" });

			// Enums
			yield return new ValueTypeAndDefaultValue(typeof(TestEnum), TestEnum.B);
			yield return new ValueTypeAndDefaultValue(typeof(TestEnum[]), new[] { TestEnum.B });

			// System.Guid
			Guid guid = Guid.Parse("{B905EAE7-4CEE-4BAC-8977-EE17CA908CC8}");
			yield return new ValueTypeAndDefaultValue(typeof(Guid), guid);
			yield return new ValueTypeAndDefaultValue(typeof(Guid[]), new[] { guid });

			// System.DateTime
			DateTime dateTime = DateTime.Parse("2024-01-01T01:02:03.12345");
			yield return new ValueTypeAndDefaultValue(typeof(DateTime), dateTime);
			yield return new ValueTypeAndDefaultValue(typeof(DateTime[]), new[] { dateTime });

			// System.TimeSpan
			TimeSpan timeSpan = TimeSpan.Parse("01:02:03.12345");
			yield return new ValueTypeAndDefaultValue(typeof(TimeSpan), timeSpan);
			yield return new ValueTypeAndDefaultValue(typeof(TimeSpan[]), new[] { timeSpan });

			// System.Net.IPAddress
			IPAddress ipAddress = IPAddress.Parse("1.2.3.4");
			yield return new ValueTypeAndDefaultValue(typeof(IPAddress), ipAddress);
			yield return new ValueTypeAndDefaultValue(typeof(IPAddress[]), new[] { ipAddress });
		}
	}

	/// <summary>
	/// Examples test data for testing item related functionality.<br/>
	/// It provides the path and a default value for various value types supported by the conversion subsystem out of the box.
	/// </summary>
	public static IEnumerable<object[]> ItemTestData
	{
		get
		{
			string[][] itemPathsList =
			[
				["/Item"],                                           // single item in the root configuration
				["/Child/Item"],                                     // single item in a child configuration
				["/Item1", "/Item2"],                                // multiple items in the root configuration
				["/Child/Item1", "/Child/Item2"],                    // multiple items in a child configuration
				["/Item1", "/Item2", "/Child/Item1", "/Child/Item2"] // multiple items in the root configuration and a child configuration
			];

			foreach (string[] itemPaths in itemPathsList)
			foreach (ValueTypeAndDefaultValue itemTypeAndDefaultValue in ItemTypesWithDefaultValues)
			{
				yield return
				[
					itemPaths,
					itemTypeAndDefaultValue.ValueType,
					itemTypeAndDefaultValue.DefaultValue
				];
			}
		}
	}

	// TODO: Add tests targeting the proper serialization of the types below...

	/// <summary>
	/// Test data with various item types supported by the conversion subsystem out of the box (with item values).
	/// </summary>
	public static IEnumerable<object[]> ItemTestDataWithValue
	{
		get
		{
			string[][] itemPathsList =
			[
				["Value"],                                           // single value in the root configuration
				["Child/Value"],                                     // single value in a child configuration
				["Value1", "Value2"],                                // multiple values in the root configuration
				["Child/Value1", "Child/Value2"],                    // multiple values in a child configuration
				["Value1", "Value2", "Child/Value1", "Child/Value2"] // multiple values in the root configuration and a child configuration
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

	#region DefaultCascadedConfiguration only: Constructor

	/// <summary>
	/// Tests creating the root of a new base configuration using the <see cref="DefaultCascadedConfiguration(string)"/> constructor.
	/// </summary>
	[Fact]
	public void CreateBaseConfiguration()
	{
		// create a new base configuration
		const string baseConfigurationName = "My Configuration";
		var baseConfiguration = new DefaultCascadedConfiguration(baseConfigurationName);

		// the configuration should return the specified name as expected
		Assert.Equal(baseConfigurationName, baseConfiguration.Name);

		// the configuration should return the persistence strategy as specified
		Assert.Null(baseConfiguration.PersistenceStrategy);

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
	}

	#endregion

	#region DefaultCascadedConfiguration only: AddInheritingConfiguration(ICascadedConfigurationPersistenceStrategy strategy)

	/// <summary>
	/// Test data for tests targeting the <see cref="CascadedConfigurationBase.AddInheritingConfiguration"/> method.
	/// </summary>
	public static IEnumerable<object[]> AddInheritingConfiguration_TestData
	{
		get
		{
			yield return [null];                                                     // no persistence
			yield return [new XmlFilePersistenceStrategy(XmlConfigurationFilePath)]; // xml persistence strategy
		}
	}

	/// <summary>
	/// Tests adding a new <see cref="CascadedConfiguration"/> inheriting from a <see cref="DefaultCascadedConfiguration"/>
	/// using <see cref="DefaultCascadedConfiguration.AddInheritingConfiguration(ICascadedConfigurationPersistenceStrategy)"/>.
	/// </summary>
	/// <param name="strategy">Persistence strategy the inheriting configuration should use.</param>
	[Theory]
	[MemberData(nameof(AddInheritingConfiguration_TestData))]
	public void AddInheritingConfiguration(ICascadedConfigurationPersistenceStrategy strategy)
	{
		// create a new root configuration that does not inherit from another configuration
		const string baseConfigurationName = "My Configuration";
		var baseConfiguration = new DefaultCascadedConfiguration(baseConfigurationName);

		// create a new root configuration that inherits from the base configuration
		CascadedConfiguration inheritingConfiguration = baseConfiguration.AddInheritingConfiguration(strategy);

		//
		// check whether the base configuration has the expected state
		//

		// the configuration should return the specified name as expected
		Assert.Equal(baseConfigurationName, baseConfiguration.Name);

		// the configuration should return the persistence strategy as specified
		Assert.Null(baseConfiguration.PersistenceStrategy);

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
		Assert.Same(strategy, inheritingConfiguration.PersistenceStrategy);

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

		// the configuration should provide the same synchronization object as the base configuration
		Assert.Same(baseConfiguration.Sync, inheritingConfiguration.Sync);
	}

	#endregion

	#region DefaultCascadedConfiguration only: AddItem<T>(string path, T defaultValue) and AddItemDynamically(string path, Type type, object defaultValue)

	#region Test Data

	/// <summary>
	/// Test data for test methods targeting <see cref="DefaultCascadedConfiguration.AddItem{T}(string,T)"/> and
	/// <see cref="DefaultCascadedConfiguration.AddItemDynamically(string,Type,object)"/>.
	/// </summary>
	public static IEnumerable<object[]> AddItem_TestData
	{
		get
		{
			//////////////////////////////////////////////////////////////////////////////////////////////////////////
			// check whether the configuration behaves as expected when adding items
			// (the type is not significant here, supported types are tested below...)
			//////////////////////////////////////////////////////////////////////////////////////////////////////////

			string[][] itemPathsList =
			[
				["/Item"],                                                                               // one top-level item
				["/Child/Item"],                                                                         // one child-level item
				["/Item1", "/Item2"],                                                                    // two top-level items
				["/Child/Item1", "/Child/Item2"],                                                        // two items in same child configuration
				["/Child1/Item", "/Child2/Item"],                                                        // two items in different child configurations
				["/Item1", "/Child1/Item1", "/Child1/Item2", "/Item2", "/Child2/Item1", "/Child2/Item2"] // mixed
			];

			foreach (ICascadedConfigurationPersistenceStrategy[] strategies in sInheritedConfigurationPersistenceStrategies)
			foreach (string[] itemPaths in itemPathsList)
			{
				Type valueType = typeof(int);
				object defaultValue = 1;
				yield return CreateTestData(strategies, itemPaths, new ValueTypeAndDefaultValue(valueType, defaultValue));
			}

			//////////////////////////////////////////////////////////////////////////////////////////////////////////
			// check whether adding items with specific value types is working as expected
			// (it is sufficient to add a value at the root of the configuration, child configurations behave the same)
			//////////////////////////////////////////////////////////////////////////////////////////////////////////

			itemPathsList =
			[
				["/Item"]
			];

			foreach (ICascadedConfigurationPersistenceStrategy[] strategies in sInheritedConfigurationPersistenceStrategies)
			foreach (string[] itemPaths in itemPathsList)
			foreach (ValueTypeAndDefaultValue testType in ItemTypesWithDefaultValues)
			{
				yield return CreateTestData(strategies, itemPaths, testType);
			}

			yield break;

			static object[] CreateTestData(
				IReadOnlyList<ICascadedConfigurationPersistenceStrategy> strategies,
				IEnumerable<string>                                      itemPaths,
				ValueTypeAndDefaultValue                                 testType)
			{
				// create a new configuration with the specified number of inheritance levels
				List<CascadedConfigurationBase> rootConfigurations = CreateConfigurationsWithPersistenceStrategies(strategies);

				// prepare item infos, but do not add any items to the configuration, yet...
				var itemInfos = new List<ItemInfo>();
				foreach (string path in itemPaths)
				{
					var itemInfo = new ItemInfo(path, testType.ValueType, testType.DefaultValue, null);
					for (int configurationIndex = 1; configurationIndex < rootConfigurations.Count; configurationIndex++)
					{
						itemInfo.Items.Add(null);
					}

					itemInfos.Add(itemInfo);
				}

				// pass prepared configuration to the test
				return
				[
					rootConfigurations.ToArray(),
					itemInfos.ToArray()
				];
			}
		}
	}

	#endregion

	#region AddItem<T>(string path, T defaultValue)

	/// <summary>
	/// Tests adding an item using <see cref="DefaultCascadedConfiguration.AddItem{T}"/>.<br/>
	/// The item does not exist when the method is called.<br/>
	/// The method is expected to succeed.
	/// </summary>
	[Theory]
	[MemberData(nameof(AddItem_TestData))]
	public void AddItemT(
		CascadedConfigurationBase[] rootConfigurations,
		ItemInfo[]                  itemInfos)
	{
		var baseRootConfiguration = (DefaultCascadedConfiguration)rootConfigurations[0];

		// add items to the base configuration
		foreach (ItemInfo itemInfo in itemInfos)
		{
			// get AddItem<T>() method to invoke
			MethodInfo method = typeof(DefaultCascadedConfiguration)
				.GetMethods()
				.Single(
					x =>
						x.Name == nameof(DefaultCascadedConfiguration.AddItem) &&
						x.IsGenericMethodDefinition &&
						x.GetGenericArguments().Length == 1 &&
						x.GetParameters().Length == 2)
				.MakeGenericMethod(itemInfo.Type);

			method.Invoke(baseRootConfiguration, [itemInfo.Path, itemInfo.DefaultValue]);
		}

		// check whether the items were added to the base configuration and inheriting configurations as expected
		AddItem_CheckAssertions(rootConfigurations, itemInfos);
	}

	/// <summary>
	/// Tests adding an item using <see cref="DefaultCascadedConfiguration.AddItem{T}"/>.<br/>
	/// The path of the item to add is <see langword="null"/>.<br/>
	/// The method is expected to throw an <see cref="ArgumentNullException"/>.
	/// </summary>
	[Fact]
	public void AddItemT_PathIsNull()
	{
		var baseRootConfiguration = new DefaultCascadedConfiguration("My Configuration");
		Assert.Throws<ArgumentNullException>(() => baseRootConfiguration.AddItem(null, 0));
	}

	/// <summary>
	/// Tests adding an item using <see cref="DefaultCascadedConfiguration.AddItem{T}"/>.<br/>
	/// An item with the same name exists already when the method is called.<br/>
	/// The method is expected to throw a <see cref="ArgumentException"/>.
	/// </summary>
	[Theory]
	[InlineData("Item")]
	[InlineData("Child/Item")]
	public void AddItemT_ItemAtSamePathExistsAlready(string path)
	{
		const int defaultValue = 0;

		// create a base configuration
		var baseRootConfiguration = new DefaultCascadedConfiguration("My Configuration");

		// add an item at the specified path (should succeed)
		baseRootConfiguration.AddItem(path, defaultValue);

		// try to add another item at the same path (should fail)
		var exception = Assert.Throws<ArgumentException>(() => baseRootConfiguration.AddItem(path, defaultValue));
		Assert.StartsWith($"The configuration already contains an item at the specified path ({path}).", exception.Message);
		Assert.Equal("path", exception.ParamName);
	}

	#endregion

	#region AddItemDynamically(string path, Type type, object defaultValue)

	/// <summary>
	/// Tests adding an item using <see cref="DefaultCascadedConfiguration.AddItemDynamically"/>.<br/>
	/// The item does not exist when the method is called.<br/>
	/// The method is expected to succeed.
	/// </summary>
	[Theory]
	[MemberData(nameof(AddItem_TestData))]
	public void AddItemDynamically(
		CascadedConfigurationBase[] rootConfigurations,
		ItemInfo[]                  itemInfos)
	{
		var baseRootConfiguration = (DefaultCascadedConfiguration)rootConfigurations[0];

		// add items to the base configuration
		foreach (ItemInfo itemInfo in itemInfos)
		{
			baseRootConfiguration.AddItemDynamically(itemInfo.Path, itemInfo.Type, itemInfo.DefaultValue);
		}

		// check whether the items were added to the base configuration and inheriting configurations as expected
		AddItem_CheckAssertions(rootConfigurations, itemInfos);
	}

	/// <summary>
	/// Tests adding an item using <see cref="DefaultCascadedConfiguration.AddItemDynamically"/>.<br/>
	/// The path of the item to add is <see langword="null"/>.<br/>
	/// The method is expected to throw an <see cref="ArgumentNullException"/>.
	/// </summary>
	[Fact]
	public void AddItemDynamically_PathNull()
	{
		var baseRootConfiguration = new DefaultCascadedConfiguration("My Configuration");
		Assert.Throws<ArgumentNullException>(() => baseRootConfiguration.AddItemDynamically(null, typeof(int), 0));
	}

	/// <summary>
	/// Tests adding an item using <see cref="DefaultCascadedConfiguration.AddItemDynamically"/>.<br/>
	/// The type of the item to add is <see langword="null"/>.<br/>
	/// The method is expected to throw an <see cref="ArgumentNullException"/>.
	/// </summary>
	[Fact]
	public void AddItemDynamically_TypeIsNull()
	{
		var baseRootConfiguration = new DefaultCascadedConfiguration("My Configuration");
		Assert.Throws<ArgumentNullException>(() => baseRootConfiguration.AddItemDynamically("Item", null, 0));
	}

	/// <summary>
	/// Tests adding an item using <see cref="DefaultCascadedConfiguration.AddItemDynamically"/>.<br/>
	/// An item with the same name exists already when the method is called.<br/>
	/// The method is expected to throw a <see cref="ArgumentException"/>.
	/// </summary>
	[Theory]
	[InlineData("Item")]
	[InlineData("Child/Item")]
	public void AddItemDynamically_ItemAtSamePathExistsAlready(string path)
	{
		const int defaultValue = 0;

		// create a base configuration
		var baseRootConfiguration = new DefaultCascadedConfiguration("My Configuration");

		// add an item at the specified path (should succeed)
		baseRootConfiguration.AddItemDynamically(path, typeof(int), defaultValue);

		// try to add another item at the same path (should fail)
		var exception = Assert.Throws<ArgumentException>(() => baseRootConfiguration.AddItemDynamically(path, typeof(int), defaultValue));
		Assert.StartsWith($"The configuration already contains an item at the specified path ({path}).", exception.Message);
		Assert.Equal("path", exception.ParamName);
	}

	#endregion

	#region Helper

	/// <summary>
	/// Checks whether the items have been added to the configuration as expected.
	/// </summary>
	/// <param name="rootConfigurations">The configuration stack (index 0 = base configuration, index &gt; 0 = inheriting configurations).</param>
	/// <param name="itemInfos">Some information about the added items (references to items are set during the check).</param>
	private static void AddItem_CheckAssertions(
		CascadedConfigurationBase[] rootConfigurations,
		ItemInfo[]                  itemInfos)
	{
		// check whether all items have been added correctly
		// (warning: item infos do not contain references to added items as they have not been set up as part of the test data set!!!)
		ItemInfo[] expectedItemInfos = itemInfos.OrderBy(itemInfo => itemInfo.Path, StringComparer.InvariantCulture).ToArray();
		string[] expectedItemPaths = expectedItemInfos.Select(itemInfo => itemInfo.Path).ToArray();
		for (int configurationIndex = 0; configurationIndex < rootConfigurations.Length; configurationIndex++)
		{
			// get all items in the configuration sorted by their path (uses enumerators only, items are already sorted by their path)
			ICascadedConfigurationItem[] actualItems = CollectAllItemsInConfiguration(rootConfigurations[configurationIndex]).ToArray();

			// check whether all added items are in the configuration
			string[] actualItemPaths = actualItems.Select(item => item.Path).ToArray();
			Assert.Equal(expectedItemPaths, actualItemPaths);

			for (int i = 0; i < expectedItemInfos.Length; i++)
			{
				ItemInfo expectedItemInfo = expectedItemInfos[i];
				string expectedItemName = expectedItemInfo.Path[(expectedItemInfo.Path.LastIndexOf('/') + 1)..];
				string expectedConfigurationPath = expectedItemInfo.Path[..expectedItemInfo.Path.LastIndexOf('/')];
				CascadedConfigurationBase expectedConfiguration = GetConfiguration(rootConfigurations[configurationIndex], expectedConfigurationPath); // uses enumerators only
				ICascadedConfigurationItem actualItem = actualItems[i];

				// set item reference to the item in test data
				// (needed for checking the InheritedItem property later on)
				Assert.Null(expectedItemInfo.Items[configurationIndex]);
				expectedItemInfo.Items[configurationIndex] = actualItem;

				// check whether the item state is as expected
				Assert.Equal(expectedItemName, actualItem.Name);
				Assert.Equal(expectedItemInfo.Path, actualItem.Path);
				Assert.Equal(expectedItemInfo.Type, actualItem.Type);
				Assert.Same(expectedConfiguration, actualItem.Configuration);
				Assert.True(actualItem.SupportsComments);
				Assert.Null(actualItem.Comment);

				if (configurationIndex == 0)
				{
					// base configuration
					Assert.Null(actualItem.InheritedItem);

					// the value is in the item itself
					Assert.True(actualItem.HasValue);
					Assert.Equal(expectedItemInfo.DefaultValue, actualItem.Value);
				}
				else
				{
					// inheriting configuration
					Assert.Same(expectedItemInfo.Items[configurationIndex - 1], actualItem.InheritedItem);

					// the value is inherited from the item in the base configuration
					Assert.False(actualItem.HasValue);
					Assert.Equal(expectedItemInfo.DefaultValue, actualItem.Value);
				}
			}
		}
	}

	#endregion

	#endregion

	#region Children { get; }

	/// <summary>
	/// Test data for the test methods targeting:<br/>
	/// - <see cref="CascadedConfigurationBase.Children"/><br/>
	/// - <see cref="CascadedConfiguration.Children"/><br/>
	/// - <see cref="DefaultCascadedConfiguration.Children"/>
	/// </summary>
	public static IEnumerable<object[]> Children_TestData
	{
		get
		{
			ICascadedConfigurationPersistenceStrategy[][] strategiesList =
			[                // --- configuration stack ---
				[null],      // base | -------------- |
				[null, null] // base | no persistence |
			];

			string[][][] configurationPathsList =
			[
				// one child configuration
				[
					["/Child1"], // setup
					["/Child1"]  // expected (as the test collects configurations recursively)
				],

				// one nested child configuration
				[
					["/Child1/Child2"],           // setup
					["/Child1", "/Child1/Child2"] // expected (as the test collects configurations recursively)
				],

				// two child configurations
				[
					["/Child11", "/Child12"], // setup
					["/Child11", "/Child12"]  // expected (as the test collects configurations recursively)
				],

				// two nested child configurations, first level is the same
				[
					["/Child11/Child21", "/Child11/Child22"],            // setup
					["/Child11", "/Child11/Child21", "/Child11/Child22"] // expected (as the test collects configurations recursively)
				],

				// two nested child configurations, first level is different
				[
					["/Child11/Child21", "/Child21/Child22"],                        // setup
					["/Child11", "/Child11/Child21", "/Child21", "/Child21/Child22"] // expected (as the test collects configurations recursively)
				]
			];

			foreach (ICascadedConfigurationPersistenceStrategy[] strategies in strategiesList)
			foreach (string[][] pathsToTest in configurationPathsList)
			{
				// create a new configuration with the specified number of inheritance levels and strategies
				List<CascadedConfigurationBase> rootConfigurations = CreateConfigurationsWithPersistenceStrategies(strategies);
				var baseRootConfiguration = (DefaultCascadedConfiguration)rootConfigurations[0];

				// create configurations using DefaultConfiguration.GetChildConfiguration(string path, bool create)
				// note: yes, this method is subject to other tests, but it is the only way to add configurations...
				foreach (string configurationPath in pathsToTest[0])
				{
					baseRootConfiguration.GetChildConfiguration(configurationPath, true);
				}

				yield return
				[
					rootConfigurations,
					pathsToTest[1]
				];
			}
		}
	}

	/// <summary>
	/// Tests enumerating child configurations using:<br/>
	/// - <see cref="DefaultCascadedConfiguration.Children"/><br/>
	/// - <see cref="CascadedConfiguration.Children"/><br/>
	/// - <see cref="CascadedConfigurationBase.Children"/>
	/// </summary>
	[Theory]
	[MemberData(nameof(Children_TestData))]
	public void Children_Get(
		List<CascadedConfigurationBase> rootConfigurations,
		string[]                        expectedConfigurationPaths)
	{
		var baseRootConfiguration = (DefaultCascadedConfiguration)rootConfigurations[0];

		// the enumerated child configurations should be sorted by name in ascending order

		// test DefaultCascadedConfiguration.Children { get; }
		// (base configuration only)
		{
			var actualConfigurations = new List<DefaultCascadedConfiguration>();
			Collect_Default(baseRootConfiguration, actualConfigurations);
			Assert.Equal(expectedConfigurationPaths, actualConfigurations.Select(x => x.Path));
		}

		// test CascadedConfiguration.Children { get; }
		// (inheriting configurations only)
		for (int configurationIndex = 1; configurationIndex < rootConfigurations.Count; configurationIndex++)
		{
			var rootConfiguration = (CascadedConfiguration)rootConfigurations[configurationIndex];
			var actualConfigurations = new List<CascadedConfiguration>();
			Collect_Inherited(rootConfiguration, actualConfigurations);
			Assert.Equal(expectedConfigurationPaths, actualConfigurations.Select(x => x.Path));
		}

		// test CascadedConfigurationBase.Children { get; }
		// (base configuration + inheriting configurations)
		for (int configurationIndex = 0; configurationIndex < rootConfigurations.Count; configurationIndex++)
		{
			CascadedConfigurationBase rootConfiguration = rootConfigurations[configurationIndex];
			var actualConfigurations = new List<CascadedConfigurationBase>();
			Collect_Base(rootConfiguration, actualConfigurations);
			Assert.Equal(expectedConfigurationPaths, actualConfigurations.Select(x => x.Path));
		}

		return;

		static void Collect_Default(DefaultCascadedConfiguration config, ICollection<DefaultCascadedConfiguration> list)
		{
			foreach (DefaultCascadedConfiguration child in config.Children) // <- property to test
			{
				list.Add(child);
				Collect_Default(child, list);
			}
		}

		static void Collect_Inherited(CascadedConfiguration config, ICollection<CascadedConfiguration> list)
		{
			foreach (CascadedConfiguration child in config.Children) // <- property to test
			{
				list.Add(child);
				Collect_Inherited(child, list);
			}
		}

		static void Collect_Base(CascadedConfigurationBase config, ICollection<CascadedConfigurationBase> list)
		{
			foreach (CascadedConfigurationBase child in config.Children) // <- property to test
			{
				list.Add(child);
				Collect_Base(child, list);
			}
		}
	}

	#endregion

	#region GetChildConfiguration(string name) and GetChildConfiguration(string name, bool create)

	/// <summary>
	/// Test data for test methods targeting adding and getting child configurations.
	/// </summary>
	public static IEnumerable<object[]> GetChildConfiguration_TestData
	{
		get
		{
			string[][] pathsList =
			[
				["Child1"],                             // one child configuration
				["Child1/Child2"],                      // one nested child configuration
				["Child11", "Child12"],                 // two child configurations
				["Child11/Child21", "Child11/Child22"], // two nested child configurations, first level is the same
				["Child11/Child21", "Child21/Child22"]  // two nested child configurations, first level is different
			];

			return
				from strategies in sInheritedConfigurationPersistenceStrategies
				from paths in pathsList
				select (object[])
				[
					paths,
					strategies
				];
		}
	}

	/// <summary>
	/// Tests creating and getting a child configurations using the following methods:<br/>
	/// - <see cref="DefaultCascadedConfiguration.GetChildConfiguration(string,bool)"/><br/>
	/// - <see cref="DefaultCascadedConfiguration.GetChildConfiguration(string)"/><br/>
	/// - <see cref="CascadedConfiguration.GetChildConfiguration(string)"/><br/>
	/// - <see cref="CascadedConfigurationBase.GetChildConfiguration(string)"/>
	/// </summary>
	[Theory]
	[MemberData(nameof(GetChildConfiguration_TestData))]
	public void GetChildConfiguration(
		string[]                                    paths,
		ICascadedConfigurationPersistenceStrategy[] configurationPersistenceStrategies)
	{
		// ------------------------------------------------------------------------------------------------------------
		// create a new base configuration and stack inherited configurations with the specified persistence strategies on top
		// ------------------------------------------------------------------------------------------------------------

		const string configurationName = "My Configuration";
		var baseRootConfiguration = new DefaultCascadedConfiguration(configurationName);
		var rootConfigurations = new List<CascadedConfigurationBase> { baseRootConfiguration };
		CascadedConfigurationBase mostSpecificRootConfiguration = baseRootConfiguration;
		foreach (ICascadedConfigurationPersistenceStrategy strategy in configurationPersistenceStrategies.Skip(1)) // skip strategy for base configuration (always null)
		{
			mostSpecificRootConfiguration = mostSpecificRootConfiguration.AddInheritingConfiguration(strategy);
			rootConfigurations.Add(mostSpecificRootConfiguration);
		}

		// ------------------------------------------------------------------------------------------------------------
		// the child configurations should neither exist in the base configuration nor in any inherited configuration, yet
		// ------------------------------------------------------------------------------------------------------------

		foreach (string path in paths)
		{
			// base configuration only
			// use: DefaultCascadedConfiguration.GetChildConfiguration(string path)
			Assert.Null(((DefaultCascadedConfiguration)rootConfigurations[0]).GetChildConfiguration(path));

			// base configuration only
			// use: DefaultCascadedConfiguration.GetChildConfiguration(string path, bool create) with create = false
			Assert.Null(((DefaultCascadedConfiguration)rootConfigurations[0]).GetChildConfiguration(path, false));

			// inherited configurations only
			// use: CascadedConfiguration.GetChildConfiguration(string path)
			Assert.All(
				rootConfigurations
					.Skip(1)
					.Cast<CascadedConfiguration>()
					.Select(rootConfiguration => rootConfiguration.GetChildConfiguration(path)),
				Assert.Null);

			// base and inherited configurations
			// use: CascadedConfigurationBase.GetChildConfiguration(string path)
			Assert.All(
				rootConfigurations
					.Skip(1)
					.Select(rootConfiguration => rootConfiguration.GetChildConfiguration(path)),
				Assert.Null);
		}

		// ------------------------------------------------------------------------------------------------------------
		// add child configurations
		// ------------------------------------------------------------------------------------------------------------

		foreach (string path in paths)
		{
			string[] pathTokens = path.Split(sSeparator, StringSplitOptions.RemoveEmptyEntries);
			string expectedChildConfigurationName = pathTokens.Last();

			// ---------------------------------------------------------------------------------------------
			// the child configuration does not exist, yet...
			// ---------------------------------------------------------------------------------------------

			// base configuration only
			// use: DefaultCascadedConfiguration.GetChildConfiguration(string path)
			Assert.Null(baseRootConfiguration.GetChildConfiguration(path));

			// base configuration only
			// use: DefaultCascadedConfiguration.GetChildConfiguration(string path, bool create) with create = false
			Assert.Null(baseRootConfiguration.GetChildConfiguration(path, false));

			// inherited configurations only
			// use: CascadedConfiguration.GetChildConfiguration(string path)
			Assert.All(
				rootConfigurations
					.Skip(1)
					.Cast<CascadedConfiguration>()
					.Select(rootConfiguration => rootConfiguration.GetChildConfiguration(path)),
				Assert.Null);

			// base and inherited configurations
			// use: CascadedConfigurationBase.GetChildConfiguration(string path)
			Assert.All(
				rootConfigurations
					.Skip(1)
					.Select(rootConfiguration => rootConfiguration.GetChildConfiguration(path)),
				Assert.Null);

			// ------------------------------------------------------------------------------------------------------------
			// create the child configuration in the base configuration
			// ------------------------------------------------------------------------------------------------------------

			// create the child configuration
			// use: DefaultCascadedConfiguration.GetChildConfiguration(string path, bool create) with create = true
			DefaultCascadedConfiguration baseChildConfiguration = baseRootConfiguration.GetChildConfiguration(path, true);
			Assert.NotNull(baseChildConfiguration);
			Assert.Equal(expectedChildConfigurationName, baseChildConfiguration.Name);

			// the child configuration should exist now...
			// => the tested methods should return the same instance

			// use: CascadedConfigurationBase.GetChildConfiguration(string path)
			Assert.Same(baseChildConfiguration, ((CascadedConfigurationBase)baseRootConfiguration).GetChildConfiguration(path));

			// use: DefaultCascadedConfiguration.GetChildConfiguration(string path)
			Assert.Same(baseChildConfiguration, baseRootConfiguration.GetChildConfiguration(path));

			// use: DefaultCascadedConfiguration.GetChildConfiguration(string path, bool create) with create = false
			Assert.Same(baseChildConfiguration, baseRootConfiguration.GetChildConfiguration(path, false));

			// ------------------------------------------------------------------------------------------------------------
			// all inherited configurations should have the child configuration as well
			// ------------------------------------------------------------------------------------------------------------

			foreach (CascadedConfiguration inheritedRootConfiguration in rootConfigurations.Skip(1).Cast<CascadedConfiguration>())
			{
				// use: CascadedConfigurationBase.GetChildConfiguration(string path), implicit create = false
				CascadedConfigurationBase inheritedChildConfigurationBase = ((CascadedConfigurationBase)inheritedRootConfiguration).GetChildConfiguration(path);
				Assert.NotNull(inheritedChildConfigurationBase);

				// use: CascadedConfiguration.GetChildConfiguration(string path), implicit create = false
				CascadedConfiguration inheritedChildConfiguration = inheritedRootConfiguration.GetChildConfiguration(path);
				Assert.NotNull(inheritedChildConfiguration);

				// both returned child configurations should be the same
				Assert.Same(inheritedChildConfigurationBase, inheritedChildConfiguration);
				Assert.Equal(expectedChildConfigurationName, inheritedChildConfiguration.Name);
			}

			// ------------------------------------------------------------------------------------------------------------
			// the base configuration and all inherited configuration should have the same child configurations now...
			// ------------------------------------------------------------------------------------------------------------

			for (int level = 0; level < configurationPersistenceStrategies.Length; level++)
			{
				ICascadedConfigurationPersistenceStrategy strategy = configurationPersistenceStrategies[level];
				CascadedConfigurationBase rootConfiguration = rootConfigurations[level];

				// determine the configuration directly above the added child configuration
				CascadedConfigurationBase parentOfChildConfiguration = pathTokens.Length == 1
					                                                       ? rootConfiguration
					                                                       : rootConfiguration.GetChildConfiguration(path[..path.LastIndexOf('/')]);
				Assert.NotNull(parentOfChildConfiguration);

				// determine the inherited configuration directly above the added child configuration
				CascadedConfigurationBase inheritedChildConfiguration = null;
				if (level > 0) inheritedChildConfiguration = rootConfigurations[level - 1].GetChildConfiguration(path);

				// determine the child configuration
				CascadedConfigurationBase childConfiguration = rootConfiguration.GetChildConfiguration(path);

				// determine the configuration just below the root on the way to the added child configuration
				CascadedConfigurationBase childOfRootConfiguration = pathTokens.Length == 1
					                                                     ? childConfiguration
					                                                     : rootConfiguration.GetChildConfiguration(path[..path.IndexOf('/')]);

				//
				// check whether the root configuration has the expected state
				//

				// the configuration should return the specified name as expected
				Assert.Equal(configurationName, rootConfiguration.Name);

				// the configuration should return the persistence strategy as specified
				Assert.Same(strategy, rootConfiguration.PersistenceStrategy);

				// a base configuration should not inherit from some other configuration,
				// but inherited configurations should do...
				if (level == 0) Assert.Null(rootConfiguration.InheritedConfiguration);
				else Assert.Same(rootConfigurations[level - 1], rootConfiguration.InheritedConfiguration);

				// a root-level configuration should return itself as the root configuration
				Assert.Same(rootConfiguration, rootConfiguration.RootConfiguration);

				// a top-level configuration should not have any parent configurations
				Assert.Null(rootConfiguration.Parent);
				Assert.Equal("/", rootConfiguration.Path);

				// the configuration should contain the created configuration only
				// (or a configuration on the way to the child configuration).
				Assert.NotEmpty(rootConfiguration.Children);
				Assert.Contains(childOfRootConfiguration, rootConfiguration.Children);

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

				// a base configuration should not inherit from some other configuration,
				// but inherited configurations should do...
				if (level == 0) Assert.Null(childConfiguration.InheritedConfiguration);
				else Assert.Same(inheritedChildConfiguration, childConfiguration.InheritedConfiguration);

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
		}
	}

	/// <summary>
	/// Tests whether the following methods throw an <see cref="ArgumentNullException"/> if the specified path is <see langword="null"/>:<br/>
	/// - <see cref="DefaultCascadedConfiguration.GetChildConfiguration(string,bool)"/> (with <c>create</c> = <see langword="false"/>)<br/>
	/// - <see cref="DefaultCascadedConfiguration.GetChildConfiguration(string)"/><br/>
	/// - <see cref="CascadedConfiguration.GetChildConfiguration(string)"/><br/>
	/// - <see cref="CascadedConfigurationBase.GetChildConfiguration(string)"/>
	/// </summary>
	[Fact]
	public void GetChildConfiguration_PathIsNull()
	{
		var baseConfiguration = new DefaultCascadedConfiguration("My Configuration");
		CascadedConfiguration inheritedConfiguration = baseConfiguration.AddInheritingConfiguration(null);

		// ------------------------------------------------------------------------------------------------------------
		// on base configuration (DefaultCascadedConfiguration)
		// ------------------------------------------------------------------------------------------------------------

		// use: DefaultCascadedConfiguration.GetChildConfiguration(string)
		var exception = Assert.Throws<ArgumentNullException>(() => { baseConfiguration.GetChildConfiguration(null); });
		Assert.Equal("path", exception.ParamName);

		// use: DefaultCascadedConfiguration.GetChildConfiguration(string,bool)
		exception = Assert.Throws<ArgumentNullException>(() => { baseConfiguration.GetChildConfiguration(null, false); });
		Assert.Equal("path", exception.ParamName);
		exception = Assert.Throws<ArgumentNullException>(() => { baseConfiguration.GetChildConfiguration(null, true); });
		Assert.Equal("path", exception.ParamName);

		// use: CascadedConfigurationBase.GetChildConfiguration(string)
		exception = Assert.Throws<ArgumentNullException>(() => { ((CascadedConfigurationBase)baseConfiguration).GetChildConfiguration(null); });
		Assert.Equal("path", exception.ParamName);

		// ------------------------------------------------------------------------------------------------------------
		// on inherited configuration (CascadedConfiguration)
		// ------------------------------------------------------------------------------------------------------------

		// tests: CascadedConfiguration.GetChildConfiguration(string)
		exception = Assert.Throws<ArgumentNullException>(() => { inheritedConfiguration.GetChildConfiguration(null); });
		Assert.Equal("path", exception.ParamName);

		// tests: CascadedConfigurationBase.GetChildConfiguration(string)
		exception = Assert.Throws<ArgumentNullException>(() => { ((CascadedConfigurationBase)inheritedConfiguration).GetChildConfiguration(null); });
		Assert.Equal("path", exception.ParamName);
	}

	#endregion

	#region GetAllItems(bool recursively)

	/// <summary>
	/// Test data for the test methods targeting <see cref="CascadedConfigurationBase.GetAllItems"/>.
	/// </summary>
	public static IEnumerable<object[]> GetAllItems_TestData
	{
		get
		{
			string[][][] itemPathsList =
			[
				// no items at all
				[
					[], // setup: configuration content
					[], // expected: top-level items
					[]  // expected: all items
				],

				// one item
				[
					["Item"],  // setup: configuration content
					["/Item"], // expected: top-level items
					["/Item"]  // expected: all items
				],

				// two items
				[
					["Item1", "Item2"],   // setup: configuration content
					["/Item1", "/Item2"], // expected: top-level items
					["/Item1", "/Item2"]  // expected: all items
				],

				// item nested in child configuration
				[
					["Child/Item1", "Child/Item2"],  // setup: configuration content
					[],                              // expected: top-level items
					["/Child/Item1", "/Child/Item2"] // expected: all items
				],

				// mix of items
				// (items are enumerated per configuration first, then nested configurations)
				[
					["Item1", "Child1/Item1", "Child1/Item2", "Item2", "Child2/Item1", "Child2/Item2"],      // setup: configuration content
					["/Item1", "/Item2"],                                                                    // expected: top-level items
					["/Item1", "/Item2", "/Child1/Item1", "/Child1/Item2", "/Child2/Item1", "/Child2/Item2"] // expected: all items
				]
			];

			foreach (ICascadedConfigurationPersistenceStrategy[] strategies in sInheritedConfigurationPersistenceStrategies)
			foreach (bool recursively in new[] { true, false })
			foreach (string[][] itemPaths in itemPathsList)
			{
				// create a new configuration with the specified number of inheritance levels and strategies
				List<CascadedConfigurationBase> rootConfigurations = CreateConfigurationsWithPersistenceStrategies(strategies);
				var baseRootConfiguration = (DefaultCascadedConfiguration)rootConfigurations[0];

				// add items with a default value in the base configuration
				// (creates items without a value in the inherited configuration, if any).
				// the items are inserted honoring the internal order of the collection
				// => the items may appear in an order different from the order in which they were added!
				int value = 0;
				var allItemInfos = new List<ItemInfo>();
				foreach (string relativeItemPath in itemPaths[0])
				{
					CascadedConfigurationItem<int> baseItem = baseRootConfiguration.AddItem(relativeItemPath, ++value);
					Assert.True(baseItem.HasValue);
					Assert.Equal(value, baseItem.Value);
					var itemInfo = new ItemInfo("/" + relativeItemPath, typeof(int), value, baseItem);
					for (int configurationIndex = 1; configurationIndex < rootConfigurations.Count; configurationIndex++)
					{
						ICascadedConfigurationItem inheritingItem = GetItemOfConfiguration(rootConfigurations[configurationIndex], itemInfo.Path);
						itemInfo.Items.Add(inheritingItem);
					}
					allItemInfos.Add(itemInfo);
				}

				// determine items that are expected to be returned
				var expectedItemInfos = new List<ItemInfo>();
				foreach (string itemPath in itemPaths[recursively ? 2 : 1])
				{
					ItemInfo itemInfo = allItemInfos.Find(x => x.Path == itemPath);
					Assert.NotNull(itemInfo);
					expectedItemInfos.Add(itemInfo);
				}

				// pass prepared configuration to the test
				yield return
				[
					rootConfigurations,
					recursively,
					expectedItemInfos
				];
			}
		}
	}

	/// <summary>
	/// Tests getting all items with various combinations of child configurations,
	/// items and levels of inheritance using <see cref="CascadedConfiguration.GetAllItems"/>.
	/// </summary>
	[Theory]
	[MemberData(nameof(GetAllItems_TestData))]
	public void GetAllItems(
		CascadedConfigurationBase[] rootConfigurations,
		bool                        recursively,
		List<ItemInfo>              itemInfos)
	{
		// check whether the base configuration (index 0) and all inheriting configuration (index > 0)
		// return the expected items
		for (int configurationIndex = 0; configurationIndex < rootConfigurations.Length; configurationIndex++)
		{
			// get all items in the most specific configuration
			// (the items are enumerated items-first, then child configurations...)
			List<ICascadedConfigurationItem> actualItems = [..rootConfigurations[configurationIndex].GetAllItems(recursively)]; // <- method to test

			// the number of items should be as expected
			Assert.Equal(itemInfos.Count, actualItems.Count);

			// the enumerated items should reflect the expected items
			for (int i = 0; i < actualItems.Count; i++)
			{
				Assert.Equal(itemInfos[i].Items[configurationIndex].Path, actualItems[i].Path);
				Assert.False(actualItems[i].HasComment);

				if (configurationIndex == 0)
				{
					// base configuration
					Assert.Same(itemInfos[i].Items[configurationIndex], actualItems[i]);
					Assert.Null(actualItems[i].InheritedItem);
					Assert.True(actualItems[i].HasValue);
				}
				else
				{
					// inheriting configuration
					Assert.Same(itemInfos[i].Items[configurationIndex], actualItems[i]);
					Assert.Same(itemInfos[i].Items[configurationIndex - 1], actualItems[i].InheritedItem);
					Assert.False(actualItems[i].HasValue);
				}
			}
		}
	}

	#endregion

	#region GetItem<T>(string path) and GetItem(string path)

	#region Test Data

	/// <summary>
	/// Test data for test methods targeting <see cref="CascadedConfigurationBase.GetItem{T}(string)"/> and
	/// <see cref="CascadedConfigurationBase.GetItem(string)"/> covering the case that the item to get exists.
	/// </summary>
	public static IEnumerable<object[]> GetItem_ItemExists_TestData
	{
		get
		{
			string[][] itemPathsList =
			[
				["/Item"],                                                                               // one top-level item
				["/Child/Item"],                                                                         // one child-level item
				["/Item1", "/Item2"],                                                                    // two top-level items
				["/Child/Item1", "/Child/Item2"],                                                        // two items in same child configuration
				["/Child1/Item", "/Child2/Item"],                                                        // two items in different child configurations
				["/Item1", "/Child1/Item1", "/Child1/Item2", "/Item2", "/Child2/Item1", "/Child2/Item2"] // mixed
			];

			foreach (ICascadedConfigurationPersistenceStrategy[] strategies in sInheritedConfigurationPersistenceStrategies)
			foreach (string[] itemPaths in itemPathsList)
			{
				// create a new configuration with the specified number of inheritance levels
				List<CascadedConfigurationBase> rootConfigurations = CreateConfigurationsWithPersistenceStrategies(strategies);
				var baseRootConfiguration = (DefaultCascadedConfiguration)rootConfigurations[0];

				// add items to the base configuration using AddItem<T>()
				// (items without a value are added to inheriting configurations as well)
				int value = 0;
				var itemInfos = new List<ItemInfo>();
				foreach (string path in itemPaths)
				{
					CascadedConfigurationItem<int> baseItem = baseRootConfiguration.AddItem(path, ++value);
					Assert.Equal(path, baseItem.Path);
					Assert.True(baseItem.HasValue);
					Assert.Equal(value, baseItem.Value);

					var itemInfo = new ItemInfo(path, typeof(int), value, baseItem);
					for (int configurationIndex = 1; configurationIndex < rootConfigurations.Count; configurationIndex++)
					{
						ICascadedConfigurationItem inheritingItem = GetItemOfConfiguration(rootConfigurations[configurationIndex], itemInfo.Path);
						itemInfo.Items.Add(inheritingItem);
					}

					itemInfos.Add(itemInfo);
				}

				// pass prepared configuration to the test
				yield return
				[
					rootConfigurations.ToArray(),
					typeof(int),
					itemInfos.ToArray()
				];
			}
		}
	}

	/// <summary>
	/// Test data for test methods targeting <see cref="CascadedConfigurationBase.GetItem{T}(string)"/> and
	/// <see cref="CascadedConfigurationBase.GetItem(string)"/> covering the case that the item to get does not exist.
	/// </summary>
	public static IEnumerable<object[]> GetItem_ItemDoesNotExist_TestData
	{
		get
		{
			string[] itemPathList =
			[
				"/Item",                   // single item in the root configuration
				"/Child/Item",             // single item in a child configuration (child exists)
				"/Non-Existing Child/Item" // single item in a child configuration (child does not exist)
			];

			foreach (ICascadedConfigurationPersistenceStrategy[] strategies in sInheritedConfigurationPersistenceStrategies)
			foreach (string path in itemPathList)
			{
				// create a new configuration with the specified number of inheritance levels
				List<CascadedConfigurationBase> rootConfigurations = CreateConfigurationsWithPersistenceStrategies(strategies);
				var baseRootConfiguration = (DefaultCascadedConfiguration)rootConfigurations[0];

				// add child configuration 'Child' to test for an item that does not exist in a child configuration
				baseRootConfiguration.GetChildConfiguration("Child", true);

				// pass prepared configuration to the test
				yield return
				[
					rootConfigurations.ToArray(),
					path
				];
			}
		}
	}

	#endregion

	#region GetItem<T>(string path)

	/// <summary>
	/// Tests getting an item using <see cref="CascadedConfigurationBase.GetItem{T}"/>.<br/>
	/// The item exists when the method is called.<br/>
	/// The method is expected to succeed.
	/// </summary>
	[Theory]
	[MemberData(nameof(GetItem_ItemExists_TestData))]
	public void GetItemT_ItemExists(
		CascadedConfigurationBase[] rootConfigurations,
		Type                        itemValueType,
		ItemInfo[]                  itemInfos)
	{
		// get GetItem<T>() method to invoke
		MethodInfo method = typeof(CascadedConfigurationBase)
			.GetMethods()
			.Single(
				x =>
					x.Name == nameof(CascadedConfigurationBase.GetItem) &&
					x.IsGenericMethodDefinition &&
					x.GetGenericArguments().Length == 1 &&
					x.GetParameters().Length == 1)
			.MakeGenericMethod(itemValueType);

		// iterate through the configurations and check whether GetItem<T>() returns the expected items
		for (int configurationIndex = 0; configurationIndex < rootConfigurations.Length; configurationIndex++)
		{
			foreach (ItemInfo itemInfo in itemInfos)
			{
				// get the item using
				// CascadedConfigurationItem<T> GetItem<T>(string path)
				var item = (ICascadedConfigurationItem)method.Invoke(rootConfigurations[configurationIndex], [itemInfo.Path]); // <- method to test
				Assert.NotNull(item);

				// the returned item should be the expected item
				Assert.Same(itemInfo.Items[configurationIndex], item);
			}
		}
	}

	/// <summary>
	/// Tests getting an item using <see cref="CascadedConfigurationBase.GetItem{T}"/>.<br/>
	/// The item does not exist when the method is called.<br/>
	/// The method is expected to throw a <see cref="ConfigurationException"/>.
	/// </summary>
	[Theory]
	[MemberData(nameof(GetItem_ItemDoesNotExist_TestData))]
	public void GetItemT_ItemDoesNotExist(
		CascadedConfigurationBase[] rootConfigurations,
		string                      path)
	{
		// get GetItem<T>() method to invoke
		MethodInfo method = typeof(CascadedConfigurationBase)
			.GetMethods()
			.Single(
				x =>
					x.Name == nameof(CascadedConfigurationBase.GetItem) &&
					x.IsGenericMethodDefinition &&
					x.GetGenericArguments().Length == 1 &&
					x.GetParameters().Length == 1)
			.MakeGenericMethod(typeof(int)); // the value type does not matter as the item does not exist...

		// determine paths of elements in the configurations
		List<List<string>> elementPathsBefore = rootConfigurations
			.Select(rootConfiguration => (List<string>) [..CollectElementPaths(rootConfiguration)])
			.ToList();

		// iterate through the configurations and check whether GetItem<T>() throws an exception
		foreach (CascadedConfigurationBase configuration in rootConfigurations)
		{
			// check whether GetItem<T>() throws an exception
			var exception = Assert.Throws<TargetInvocationException>(() => method.Invoke(configuration, [path]));
			Assert.IsType<ConfigurationException>(exception.InnerException);
			Assert.Equal($"The configuration does not contain an item at the specified path ({path}).", exception.InnerException.Message);
		}

		// determine paths of elements in the configurations
		List<List<string>> elementPathsAfter = rootConfigurations
			.Select(rootConfiguration => (List<string>) [..CollectElementPaths(rootConfiguration)])
			.ToList();

		// no elements should have been added/removed
		Assert.Equal(elementPathsBefore, elementPathsAfter);
	}

	/// <summary>
	/// Tests getting an item using <see cref="CascadedConfigurationBase.GetItem{T}"/>.<br/>
	/// The item exists already, but its value type is different.<br/>
	/// The method is expected to throw a <see cref="ConfigurationException"/>.
	/// </summary>
	[Theory]
	[MemberData(nameof(GetItem_ItemExists_TestData))]
	public void GetItemT_ItemExistsButTypeIsDifferent(
		CascadedConfigurationBase[] rootConfigurations,
		Type                        itemValueType,
		ItemInfo[]                  itemInfos)
	{
		// choose a value type that is _not_ a used item value type
		Type differentItemValueType = itemValueType == typeof(int) ? typeof(long) : typeof(int);

		// get GetItem<T>() method to invoke
		MethodInfo method = typeof(CascadedConfigurationBase)
			.GetMethods()
			.Single(
				x =>
					x.Name == nameof(CascadedConfigurationBase.GetItem) &&
					x.IsGenericMethodDefinition &&
					x.GetGenericArguments().Length == 1 &&
					x.GetParameters().Length == 1)
			.MakeGenericMethod(differentItemValueType);

		// iterate through the configurations and check whether GetItem<T>() returns the expected items
		foreach (CascadedConfigurationBase rootConfiguration in rootConfigurations)
		{
			foreach (ItemInfo itemInfo in itemInfos)
			{
				// get the item using
				// CascadedConfigurationItem<T> GetItem<T>(string path)
				var exception = Assert.Throws<TargetInvocationException>(() => method.Invoke(rootConfiguration, [itemInfo.Path])); // <- method to test
				Assert.IsType<ConfigurationException>(exception.InnerException);
				Assert.Equal(
					$"The configuration contains an item at the specified path ({itemInfo.Path}), but with a different type (configuration item: {itemValueType.FullName}, specified: {differentItemValueType.FullName}).",
					exception.InnerException.Message);
			}
		}
	}

	/// <summary>
	/// Tests getting an item using <see cref="CascadedConfigurationBase.GetItem{T}"/>.<br/>
	/// The path of the item to get is <see langword="null"/>.<br/>
	/// The method is expected to throw an <see cref="ArgumentNullException"/>.
	/// </summary>
	[Fact]
	public void GetItemT_PathNull()
	{
		// create a base configuration and let a configuration inherit from it
		const string configurationName = "My Configuration";
		var baseRootConfiguration = new DefaultCascadedConfiguration(configurationName);
		const ICascadedConfigurationPersistenceStrategy strategy = null;
		CascadedConfiguration inheritedRootConfiguration = baseRootConfiguration.AddInheritingConfiguration(strategy);

		// check whether GetItem<T>() throws the expected exception
		Assert.Throws<ArgumentNullException>(() => baseRootConfiguration.GetItem<int>(null));      // DefaultCascadedConfiguration
		Assert.Throws<ArgumentNullException>(() => inheritedRootConfiguration.GetItem<int>(null)); // CascadedConfiguration
	}

	#endregion

	#region GetItem(string path)

	/// <summary>
	/// Tests getting an item using <see cref="CascadedConfigurationBase.GetItem"/>.<br/>
	/// The item exists when the method is called.<br/>
	/// The method is expected to succeed.
	/// </summary>
	[Theory]
	[MemberData(nameof(GetItem_ItemExists_TestData))]
	public void GetItem_ItemExists(
		CascadedConfigurationBase[] rootConfigurations,
		Type                        _,
		ItemInfo[]                  itemInfos)
	{
		// iterate through the configurations and check whether GetItem() returns the expected items
		for (int configurationIndex = 0; configurationIndex < rootConfigurations.Length; configurationIndex++)
		{
			foreach (ItemInfo itemInfo in itemInfos)
			{
				// get the item using
				// ICascadedConfigurationItem GetItem(string path)
				ICascadedConfigurationItem item = rootConfigurations[configurationIndex].GetItem(itemInfo.Path); // <- method to test
				Assert.NotNull(item);

				// the returned item should be the expected item
				Assert.Same(itemInfo.Items[configurationIndex], item);
			}
		}
	}

	/// <summary>
	/// Tests getting an item using <see cref="CascadedConfigurationBase.GetItem"/>.<br/>
	/// The item does not exist when the method is called.<br/>
	/// The method is expected to throw a <see cref="ConfigurationException"/>.
	/// </summary>
	[Theory]
	[MemberData(nameof(GetItem_ItemDoesNotExist_TestData))]
	public void GetItem_ItemDoesNotExist(
		CascadedConfigurationBase[] rootConfigurations,
		string                      path)
	{
		// determine paths of elements in the configurations
		List<List<string>> elementPathsBefore = rootConfigurations
			.Select(rootConfiguration => (List<string>) [..CollectElementPaths(rootConfiguration)])
			.ToList();

		// iterate through the configurations and check whether GetItem<T>() throws an exception
		foreach (CascadedConfigurationBase configuration in rootConfigurations)
		{
			// check whether GetItem<T>() throws an exception
			var exception = Assert.Throws<ConfigurationException>(() => configuration.GetItem(path));
			Assert.Equal($"The configuration does not contain an item at the specified path ({path}).", exception.Message);
		}

		// determine paths of elements in the configurations
		List<List<string>> elementPathsAfter = rootConfigurations
			.Select(rootConfiguration => (List<string>) [..CollectElementPaths(rootConfiguration)])
			.ToList();

		// no elements should have been added/removed
		Assert.Equal(elementPathsBefore, elementPathsAfter);
	}

	/// <summary>
	/// Tests getting an item using <see cref="CascadedConfigurationBase.GetItem"/>.<br/>
	/// The path of the item to get is <see langword="null"/>.<br/>
	/// The method is expected to throw an <see cref="ArgumentNullException"/>.
	/// </summary>
	[Fact]
	public void GetItem_PathIsNull()
	{
		// create a base configuration and let a configuration inherit from it
		const string configurationName = "My Configuration";
		var baseRootConfiguration = new DefaultCascadedConfiguration(configurationName);
		const ICascadedConfigurationPersistenceStrategy strategy = null;
		CascadedConfiguration inheritedRootConfiguration = baseRootConfiguration.AddInheritingConfiguration(strategy);

		// check whether GetItem() throws the expected exception
		Assert.Throws<ArgumentNullException>(() => baseRootConfiguration.GetItem(null));      // DefaultCascadedConfiguration
		Assert.Throws<ArgumentNullException>(() => inheritedRootConfiguration.GetItem(null)); // CascadedConfiguration
	}

	#endregion

	#endregion

	#region Load() and Save()

	/// <summary>
	/// Test data for test methods targeting <see cref="CascadedConfiguration.Load"/> and <see cref="CascadedConfiguration.Save"/>.
	/// </summary>
	public static IEnumerable<object[]> SaveAndLoad_TestData
	{
		get
		{
			foreach (CascadedConfigurationSaveFlags flags in new[] { CascadedConfigurationSaveFlags.None, CascadedConfigurationSaveFlags.SaveInheritedSettings })
			{
				// one top-level item
				yield return
				[
					flags,
					new[] { "/Item" }
				];

				// one child-level item
				yield return
				[
					flags,
					new[] { "/Child/Item" }
				];

				// two top-level items
				yield return
				[
					flags,
					new[] { "/Item1", "/Item2" }
				];

				// two items in same child configuration
				yield return
				[
					flags,
					new[] { "/Child/Item1", "/Child/Item2" }
				];

				// two items in different child configurations
				yield return
				[
					flags,
					new[] { "/Child1/Item", "/Child2/Item" }
				];

				// mixed
				yield return
				[
					flags,
					new[] { "/Item1", "/Child1/Item1", "/Child1/Item2", "/Item2", "/Child2/Item1", "/Child2/Item2" }
				];
			}
		}
	}

	[Theory]
	[MemberData(nameof(SaveAndLoad_TestData))]
	public void SaveAndLoad_AddItemsBeforeLoad(CascadedConfigurationSaveFlags flags, string[] itemPaths)
	{
		// set up the configuration to test
		File.Delete(XmlConfigurationFilePath); // avoid merging with existing configuration file
		int totalItemsCount = SetupConfiguration(
			itemPaths,
			true,
			out DefaultCascadedConfiguration baseRootConfiguration,
			out CascadedConfiguration inheritingRootConfiguration);

		// save the inheriting configuration
		inheritingRootConfiguration.Save(flags);

		// set up the same configuration once again
		SetupConfiguration(
			itemPaths,
			false,
			out DefaultCascadedConfiguration _,
			out CascadedConfiguration loadedInheritingRootConfiguration);

		// load the inheriting configuration
		loadedInheritingRootConfiguration.Load();

		// check whether the loaded configuration contains the expected items
		ICascadedConfigurationItem[] savedBaseItems = CollectAllItemsInConfiguration(baseRootConfiguration);                    // uses enumerators only, sorted by path in ascending order
		ICascadedConfigurationItem[] savedInheritingItems = CollectAllItemsInConfiguration(inheritingRootConfiguration);        // uses enumerators only, sorted by path in ascending order
		ICascadedConfigurationItem[] loadedInheritingItems = CollectAllItemsInConfiguration(loadedInheritingRootConfiguration); // uses enumerators only, sorted by path in ascending order
		Assert.Equal(totalItemsCount, savedBaseItems.Length);
		Assert.Equal(totalItemsCount, savedInheritingItems.Length);
		Assert.Equal(totalItemsCount, loadedInheritingItems.Length);
		for (int i = 0; i < itemPaths.Length; i++)
		{
			string itemPath = savedBaseItems[i].Path;
			Assert.Equal(itemPath, savedInheritingItems[i].Path);
			Assert.Equal(itemPath, loadedInheritingItems[i].Path);

			if (flags == CascadedConfigurationSaveFlags.SaveInheritedSettings)
			{
				// the loaded configuration should contain items with values and comments that were inherited when saving
				// => all items should have a value and a comment
				Assert.True(loadedInheritingItems[i].HasValue);
				Assert.True(loadedInheritingItems[i].HasComment);
				if (loadedInheritingItems[i].Path.StartsWith("/Overridden/", StringComparison.InvariantCulture))
				{
					// value
					Assert.True(savedInheritingItems[i].HasValue);
					Assert.Equal(savedInheritingItems[i].Value, loadedInheritingItems[i].Value);

					// comment
					Assert.True(savedInheritingItems[i].HasComment);
					Assert.Equal(savedInheritingItems[i].Comment, loadedInheritingItems[i].Comment);
				}
				else
				{
					// value
					Assert.True(savedBaseItems[i].HasValue);
					Assert.Equal(savedBaseItems[i].Value, loadedInheritingItems[i].Value);

					// comment
					Assert.True(savedBaseItems[i].HasComment);
					Assert.Equal(savedBaseItems[i].Comment, loadedInheritingItems[i].Comment);
				}
			}
			else if (flags == CascadedConfigurationSaveFlags.None)
			{
				// the loaded configuration should contain items with values that were inherited when saving
				// => only items at the path starting with 'Overridden' should have a value
				if (loadedInheritingItems[i].Path.StartsWith("/Overridden/", StringComparison.InvariantCulture))
				{
					// value
					Assert.True(loadedInheritingItems[i].HasValue);
					Assert.Equal(savedInheritingItems[i].Value, loadedInheritingItems[i].Value);

					// comment
					Assert.True(loadedInheritingItems[i].HasComment);
					Assert.Equal(savedInheritingItems[i].Comment, loadedInheritingItems[i].Comment);
				}
				else
				{
					// value
					Assert.False(loadedInheritingItems[i].HasValue);
					Assert.True(savedBaseItems[i].HasValue);
					Assert.Equal(savedBaseItems[i].Value, loadedInheritingItems[i].Value);

					// comment
					Assert.False(loadedInheritingItems[i].HasComment);
					Assert.True(savedBaseItems[i].HasComment);
					Assert.Equal(savedBaseItems[i].Comment, loadedInheritingItems[i].Comment);
				}
			}
		}
		return;

		static int SetupConfiguration(
			IEnumerable<string>              itemPaths,
			bool                             setItemOverrides,
			out DefaultCascadedConfiguration baseRootConfiguration,
			out CascadedConfiguration        inheritingRootConfiguration)
		{
			// create a new base configuration and let another configuration with persistence support inherit from it
			const string configurationName = "My Configuration";
			baseRootConfiguration = new DefaultCascadedConfiguration(configurationName);
			inheritingRootConfiguration = baseRootConfiguration.AddInheritingConfiguration(new XmlFilePersistenceStrategy(XmlConfigurationFilePath));

			// add items to the configuration twice:
			// - Overridden/* for items that are overridden in the inheriting configuration
			// - Inherited/* for items that are not overridden in the inheriting configuration
			int value = 0;
			int totalItemsCount = 0;
			foreach (string itemPath in itemPaths)
			{
				CascadedConfigurationItem<int> item1 = baseRootConfiguration.AddItem("Overridden" + itemPath, ++value);
				item1.Comment = $"Comment #{value}";
				totalItemsCount++;

				CascadedConfigurationItem<int> item2 = baseRootConfiguration.AddItem("Inherited" + itemPath, ++value);
				item2.Comment = $"Comment #{value}";
				totalItemsCount++;

				if (setItemOverrides)
				{
					CascadedConfigurationItem<int> item3 = inheritingRootConfiguration.SetValue("Overridden" + itemPath, ++value);
					item3.Comment = $"Comment #{value}";
				}
				else
				{
					++value;
				}
			}

			return totalItemsCount;
		}
	}

	[Theory]
	[MemberData(nameof(SaveAndLoad_TestData))]
	public void SaveAndLoad_AddItemsAfterLoad(CascadedConfigurationSaveFlags flags, string[] itemPaths)
	{
		const string configurationName = "My Configuration";

		// set up the configuration to test
		File.Delete(XmlConfigurationFilePath); // avoid merging with existing configuration file
		var baseRootConfiguration = new DefaultCascadedConfiguration(configurationName);
		CascadedConfiguration inheritingRootConfiguration = baseRootConfiguration.AddInheritingConfiguration(new XmlFilePersistenceStrategy(XmlConfigurationFilePath));
		int value = 0;
		int totalItemsCount = 0;
		foreach (string itemPath in itemPaths)
		{
			CascadedConfigurationItem<int> item1 = baseRootConfiguration.AddItem("Overridden" + itemPath, ++value);
			item1.Comment = $"Comment #{value}";
			totalItemsCount++;

			CascadedConfigurationItem<int> item2 = baseRootConfiguration.AddItem("Inherited" + itemPath, ++value);
			item2.Comment = $"Comment #{value}";
			totalItemsCount++;

			CascadedConfigurationItem<int> item3 = inheritingRootConfiguration.SetValue("Overridden" + itemPath, ++value);
			item3.Comment = $"Comment #{value}";
		}

		// save the inheriting configuration
		inheritingRootConfiguration.Save(flags);

		// set up the same configuration once again, but without items at first, then load the configuration
		// and add items afterward
		var loadedBaseRootConfiguration = new DefaultCascadedConfiguration(configurationName);
		CascadedConfiguration loadedInheritingRootConfiguration = loadedBaseRootConfiguration.AddInheritingConfiguration(new XmlFilePersistenceStrategy(XmlConfigurationFilePath));
		loadedInheritingRootConfiguration.Load();
		value = 0;
		foreach (string itemPath in itemPaths)
		{
			CascadedConfigurationItem<int> item1 = loadedBaseRootConfiguration.AddItem("Overridden" + itemPath, ++value);
			item1.Comment = $"Comment #{value}";

			CascadedConfigurationItem<int> item2 = loadedBaseRootConfiguration.AddItem("Inherited" + itemPath, ++value);
			item2.Comment = $"Comment #{value}";

			// skip item3
			++value;
		}

		// check whether the loaded configuration contains the expected items
		ICascadedConfigurationItem[] savedBaseItems = CollectAllItemsInConfiguration(baseRootConfiguration);                    // uses enumerators only, sorted by path in ascending order
		ICascadedConfigurationItem[] savedInheritingItems = CollectAllItemsInConfiguration(inheritingRootConfiguration);        // uses enumerators only, sorted by path in ascending order
		ICascadedConfigurationItem[] loadedInheritingItems = CollectAllItemsInConfiguration(loadedInheritingRootConfiguration); // uses enumerators only, sorted by path in ascending order
		Assert.Equal(totalItemsCount, savedBaseItems.Length);
		Assert.Equal(totalItemsCount, savedInheritingItems.Length);
		Assert.Equal(totalItemsCount, loadedInheritingItems.Length);
		for (int i = 0; i < itemPaths.Length; i++)
		{
			string itemPath = savedBaseItems[i].Path;
			Assert.Equal(itemPath, savedInheritingItems[i].Path);
			Assert.Equal(itemPath, loadedInheritingItems[i].Path);

			if (flags == CascadedConfigurationSaveFlags.SaveInheritedSettings)
			{
				// the loaded configuration should contain items with values and comments that were inherited when saving
				// => all items should have a value and a comment
				Assert.True(loadedInheritingItems[i].HasValue);
				Assert.True(loadedInheritingItems[i].HasComment);
				if (loadedInheritingItems[i].Path.StartsWith("/Overridden/", StringComparison.InvariantCulture))
				{
					// value
					Assert.True(savedInheritingItems[i].HasValue);
					Assert.Equal(savedInheritingItems[i].Value, loadedInheritingItems[i].Value);

					// comment
					Assert.True(savedInheritingItems[i].HasComment);
					Assert.Equal(savedInheritingItems[i].Comment, loadedInheritingItems[i].Comment);
				}
				else
				{
					// value
					Assert.True(savedBaseItems[i].HasValue);
					Assert.Equal(savedBaseItems[i].Value, loadedInheritingItems[i].Value);

					// comment
					Assert.True(savedBaseItems[i].HasComment);
					Assert.Equal(savedBaseItems[i].Comment, loadedInheritingItems[i].Comment);
				}
			}
			else if (flags == CascadedConfigurationSaveFlags.None)
			{
				// the loaded configuration should contain items with values that were inherited when saving
				// => only items at the path starting with 'Overridden' should have a value
				if (loadedInheritingItems[i].Path.StartsWith("/Overridden/", StringComparison.InvariantCulture))
				{
					// value
					Assert.True(loadedInheritingItems[i].HasValue);
					Assert.Equal(savedInheritingItems[i].Value, loadedInheritingItems[i].Value);

					// comment
					Assert.True(loadedInheritingItems[i].HasComment);
					Assert.Equal(savedInheritingItems[i].Comment, loadedInheritingItems[i].Comment);
				}
				else
				{
					// value
					Assert.False(loadedInheritingItems[i].HasValue);
					Assert.True(savedBaseItems[i].HasValue);
					Assert.Equal(savedBaseItems[i].Value, loadedInheritingItems[i].Value);

					// comment
					Assert.False(loadedInheritingItems[i].HasComment);
					Assert.True(savedBaseItems[i].HasComment);
					Assert.Equal(savedBaseItems[i].Comment, loadedInheritingItems[i].Comment);
				}
			}
		}
	}

	#endregion


	#region ResetItems(bool recursive)

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ResetItems(bool recursive)
	{
		// create a new base configuration with an inheriting configuration on top
		const string configurationName = "My Configuration";
		var baseRootConfiguration = new DefaultCascadedConfiguration(configurationName);
		CascadedConfiguration inheritedRootConfiguration = baseRootConfiguration.AddInheritingConfiguration(persistence: null);

		// populate configuration with some items
		CascadedConfigurationItem<int> itemC0V1_base = baseRootConfiguration.AddItem("Item1", 1);
		CascadedConfigurationItem<int> itemC0V2_base = baseRootConfiguration.AddItem("Item2", 2);
		CascadedConfigurationItem<int> itemC1V1_base = baseRootConfiguration.AddItem("Child1/Item1", 11);
		CascadedConfigurationItem<int> itemC1V2_base = baseRootConfiguration.AddItem("Child1/Item2", 12);
		CascadedConfigurationItem<int> itemC2V1_base = baseRootConfiguration.AddItem("Child2/Item1", 21);
		CascadedConfigurationItem<int> itemC2V2_base = baseRootConfiguration.AddItem("Child2/Item2", 22);
		var items_base = new List<ICascadedConfigurationItem>
		{
			itemC0V1_base,
			itemC0V2_base,
			itemC1V1_base,
			itemC1V2_base,
			itemC2V1_base,
			itemC2V2_base
		};

		// all base items should have a value
		Assert.All(items_base, item => Assert.True(item.HasValue));

		// set value of items in the inherited configuration
		CascadedConfigurationItem<int> itemC0V1_inherited = inheritedRootConfiguration.SetValue("Item1", 101);
		CascadedConfigurationItem<int> itemC0V2_inherited = inheritedRootConfiguration.SetValue("Item2", 102);
		CascadedConfigurationItem<int> itemC1V1_inherited = inheritedRootConfiguration.SetValue("Child1/Item1", 111);
		CascadedConfigurationItem<int> itemC1V2_inherited = inheritedRootConfiguration.SetValue("Child1/Item2", 112);
		CascadedConfigurationItem<int> itemC2V1_inherited = inheritedRootConfiguration.SetValue("Child2/Item1", 121);
		CascadedConfigurationItem<int> itemC2V2_inherited = inheritedRootConfiguration.SetValue("Child2/Item2", 122);
		var items_inherited = new List<ICascadedConfigurationItem>
		{
			itemC0V1_inherited,
			itemC0V2_inherited,
			itemC1V1_inherited,
			itemC1V2_inherited,
			itemC2V1_inherited,
			itemC2V2_inherited
		};

		// all inherited items should have a value
		Assert.All(items_inherited, item => Assert.True(item.HasValue));

		// reset items in the inherited configuration
		inheritedRootConfiguration.ResetItems(recursive);

		// the items on the same configuration level should always be reset
		Assert.False(itemC0V1_inherited.HasValue);
		Assert.False(itemC0V2_inherited.HasValue);

		// child items should have a value, if not resetting recursively
		bool childItemsShouldHaveValue = !recursive;
		Assert.Equal(childItemsShouldHaveValue, itemC1V1_inherited.HasValue);
		Assert.Equal(childItemsShouldHaveValue, itemC1V2_inherited.HasValue);
		Assert.Equal(childItemsShouldHaveValue, itemC2V1_inherited.HasValue);
		Assert.Equal(childItemsShouldHaveValue, itemC2V2_inherited.HasValue);
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Recursively collects all items in the specified configuration (uses enumerators only).
	/// </summary>
	/// <param name="configuration">Configuration from which to collect the items.</param>
	/// <returns>All items in the specified configuration.</returns>
	private static ICascadedConfigurationItem[] CollectAllItemsInConfiguration(CascadedConfigurationBase configuration)
	{
		var items = new List<ICascadedConfigurationItem>();
		Collect(configuration, items);
		return items.OrderBy(x => x.Path, StringComparer.InvariantCulture).ToArray();

		static void Collect(CascadedConfigurationBase config, ICollection<ICascadedConfigurationItem> list)
		{
			foreach (ICascadedConfigurationItem item in config.Items)
			{
				list.Add(item);
			}

			foreach (CascadedConfigurationBase child in config.Children)
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
	private static IEnumerable<string> CollectElementPaths(CascadedConfigurationBase configuration)
	{
		var items = new List<string>();
		Collect(configuration, items);
		return items;

		static void Collect(CascadedConfigurationBase config, ICollection<string> list)
		{
			foreach (ICascadedConfigurationItem item in config.Items)
			{
				list.Add(item.Path);
			}

			foreach (CascadedConfigurationBase child in config.Children)
			{
				list.Add(child.Path + "/");
				Collect(child, list);
			}
		}
	}

	private static DefaultCascadedConfiguration GetConfiguration(DefaultCascadedConfiguration configuration, string path)
	{
		string[] pathSegments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries); // does not support escaping...
		if (pathSegments.Length == 0) return configuration;
		DefaultCascadedConfiguration child = configuration.Children.FirstOrDefault(x => x.Name == pathSegments[0]);
		Assert.NotNull(child);
		return GetConfiguration(child, string.Join("/", pathSegments.Skip(1)));
	}

	private static CascadedConfigurationBase GetConfiguration(CascadedConfigurationBase configuration, string path)
	{
		string[] pathSegments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries); // does not support escaping...
		if (pathSegments.Length == 0) return configuration;
		CascadedConfigurationBase child = configuration.Children.FirstOrDefault(x => x.Name == pathSegments[0]);
		Assert.NotNull(child);
		return GetConfiguration(child, string.Join("/", pathSegments.Skip(1)));
	}

	private static List<CascadedConfigurationBase> GetConfigurations(CascadedConfigurationBase configuration)
	{
		var items = new List<CascadedConfigurationBase>();
		Collect(configuration, items);
		items.Sort(ConfigurationByPathComparer.InvariantCultureComparer);
		return items;

		static void Collect(CascadedConfigurationBase config, ICollection<CascadedConfigurationBase> list)
		{
			foreach (CascadedConfigurationBase child in config.Children)
			{
				list.Add(child);
				Collect(child, list);
			}
		}
	}

	private static ICascadedConfigurationItem GetItemOfConfiguration(CascadedConfigurationBase configuration, string path)
	{
		string[] pathSegments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries); // does not support escaping...

		if (pathSegments.Length > 1)
		{
			CascadedConfigurationBase child = configuration.Children.FirstOrDefault(x => x.Name == pathSegments[0]);
			Assert.NotNull(child);
			return GetItemOfConfiguration(child, string.Join("/", pathSegments.Skip(1)));
		}

		ICascadedConfigurationItem item = configuration.Items.FirstOrDefault(x => x.Name == pathSegments[0]);
		Assert.NotNull(item);
		return item;
	}

	private static List<CascadedConfigurationBase> CreateConfigurationsWithPersistenceStrategies(IReadOnlyList<ICascadedConfigurationPersistenceStrategy> strategies)
	{
		// create a new configuration with the specified number of inheritance levels
		const string configurationName = "My Configuration";
		var rootConfigurations = new List<CascadedConfigurationBase>();
		var baseRootConfiguration = new DefaultCascadedConfiguration(configurationName);
		rootConfigurations.Add(baseRootConfiguration);
		CascadedConfigurationBase mostSpecificRootConfiguration = baseRootConfiguration;
		for (int inheritanceLevel = 1; inheritanceLevel < strategies.Count; inheritanceLevel++)
		{
			mostSpecificRootConfiguration = mostSpecificRootConfiguration.AddInheritingConfiguration(strategies[inheritanceLevel]);
			rootConfigurations.Add(mostSpecificRootConfiguration);
		}
		return rootConfigurations;
	}

	#endregion
}
