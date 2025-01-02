///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;

using GriffinPlus.Lib.Conversion;
using GriffinPlus.Lib.Threading;

using BindingFlags = System.Reflection.BindingFlags;

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// A persistence strategy that enables a <see cref="CascadedConfiguration"/> to persist its data in an XML file.
/// </summary>
public partial class XmlFilePersistenceStrategy : CascadedConfigurationPersistenceStrategy
{
	private static readonly ReaderWriterLockSlim        sSupportedComplexTypesLock = new(LockRecursionPolicy.SupportsRecursion);
	private static readonly Dictionary<Type, CacheItem> sSupportedComplexTypes     = new();
	private readonly        string                      mConfigurationFilePath;
	private                 XmlDocument                 mXmlDocument;

	/// <summary>
	/// Initializes a new instance of the <see cref="XmlFilePersistenceStrategy"/> class.
	/// </summary>
	/// <param name="path">Path of the configuration file.</param>
	public XmlFilePersistenceStrategy(string path)
	{
		mConfigurationFilePath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(path));
	}

	/// <inheritdoc/>
	public override bool IsValidConfigurationName(string name)
	{
		return true;
	}

	/// <inheritdoc/>
	public override bool IsValidItemName(string name)
	{
		return true;
	}

	/// <inheritdoc/>
	public override bool SupportsComments => true;

	/// <summary>
	/// Checks whether the persistence strategy supports the specified type.<br/>
	/// The persistence strategy supports the following types:<br/>
	/// - all types supported by <see cref="Converters.GlobalConverters"/>
	/// - all types supported by converters that have been registered using <see cref="CascadedConfigurationPersistenceStrategy.RegisterValueConverter"/>
	/// <br/>
	/// - all enumeration types<br/>
	/// - all classes/structs with a default constructor and public fields/properties of a type listed above<br/>
	/// - one-dimensional arrays of one of the types listed above
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <returns>
	/// <see langword="true"/> if the persistence strategy supports the specified type;<br/>
	/// otherwise <see langword="false"/>.
	/// </returns>
	public override bool SupportsType(Type type)
	{
		// check fixed types
		if (type.IsArray && type.GetArrayRank() == 1 && SupportsType(type.GetElementType())) return true;
		if (type.IsEnum) return true;

		// check whether there is a converter for the specified type
		// that can convert from the type to string and vice versa
		if ((GetValueConverter(type) ?? Converters.GetGlobalConverter(type)) != null)
			return true;

		// check whether the specified type is a class/struct with a default constructor and public fields and/or properties
		if (type.IsClass || type.IsValueType)
		{
			// query cached type analysis results
			using (new ReaderWriterLockSlimAutoLock(sSupportedComplexTypesLock, ReaderWriterLockSlimAcquireKind.Read))
			{
				if (sSupportedComplexTypes.TryGetValue(type, out CacheItem cacheItem))
					return cacheItem.IsSupported;
			}

			// type has not been analyzed, yet
			// => analyze it and cache the result...

			using (new ReaderWriterLockSlimAutoLock(sSupportedComplexTypesLock, ReaderWriterLockSlimAcquireKind.ReadWrite))
			{
				var complexTypeInfoByType = new Dictionary<Type, CacheItem>();
				if (AnalyzeComplexType(type, complexTypeInfoByType))
				{
					// type is supported
					Debug.Assert(complexTypeInfoByType.ContainsKey(type));
					sSupportedComplexTypes.Add(type, complexTypeInfoByType[type]);
					return true;
				}

				// type is not supported
				sSupportedComplexTypes.Add(type, new CacheItem(type, isSupported: false, constructor: null, fields: null, properties: null));
				return false;
			}
		}

		// the type is not supported by the persistence strategy
		return false;
	}

	private bool AnalyzeComplexType(Type type, Dictionary<Type, CacheItem> complexTypeInfoByType)
	{
		// check fixed types
		if (type.IsArray && type.GetArrayRank() == 1 && AnalyzeComplexType(type.GetElementType(), complexTypeInfoByType)) return true;
		if (type.IsEnum) return true;

		// check whether there is a converter for the specified type
		// that can convert from the type to string and vice versa
		if ((GetValueConverter(type) ?? Converters.GetGlobalConverter(type)) != null)
			return true;

		// check whether the specified type is a class/struct with a default constructor and public fields and/or properties
		if (type.IsClass || type.IsValueType)
		{
			// abort if the specified type is under analysis at the moment (cacheItem == null)
			// or if it has been analyzed to be supported...
			if (complexTypeInfoByType.TryGetValue(type, out CacheItem cacheItem))
			{
				return cacheItem == null || cacheItem.IsSupported;
			}

			// type has not been analyzed, yet
			// => analyze it and cache the result...

			// try to get the default constructor
			// - classes may have an explicit or compiler-generated parameterless constructor
			// - structs may have an explicit parameterless constructor only
			ConstructorInfo constructor = type.GetConstructor(
				bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				Type.DefaultBinder,
				Type.EmptyTypes,
				modifiers: null);

			// abort if the specified type does not have a parameterless constructor
			// (structs are always instantiable using a parameterless constructor)
			if (constructor == null && !type.IsValueType)
			{
				complexTypeInfoByType.Add(type, new CacheItem(type, isSupported: false, constructor: null, fields: null, properties: null));
				return false;
			}

			// pretend that the type is supported to break recursive lookups of field/property types
			// and adjust the cache item afterward
			complexTypeInfoByType.Add(type, null);

			// get public fields and public properties with both a get accessor and a set accessor and supported types
			// (the get/set accessor may be non-public)
			FieldInfo[] fields = type
				.GetFields(bindingAttr: BindingFlags.Instance | BindingFlags.Public)
				.Where(field => AnalyzeComplexType(field.FieldType, complexTypeInfoByType))
				.ToArray();
			PropertyInfo[] properties = type
				.GetProperties(bindingAttr: BindingFlags.Instance | BindingFlags.Public)
				.Where(property => property.CanRead && property.CanWrite && AnalyzeComplexType(property.PropertyType, complexTypeInfoByType))
				.ToArray();

			// remove cache item added to pretend the type is supported
			complexTypeInfoByType.Remove(type);

			// abort if there are neither fields nor properties that can be serialized/deserialized
			if (fields.Length == 0 && properties.Length == 0)
			{
				complexTypeInfoByType.Add(type, new CacheItem(type, isSupported: false, constructor: null, fields: null, properties: null));
				return false;
			}

			// the class/struct is serializable using the persistence strategy
			// (at least the fields/properties that are declared public, other fields/properties keep their default value)
			complexTypeInfoByType.Add(type, new CacheItem(type, isSupported: true, constructor: constructor, fields, properties));
			return true;
		}

		// the type is not supported by the persistence strategy
		return false;
	}

	/// <inheritdoc/>
	public override void Load(CascadedConfiguration configuration)
	{
		if (configuration != configuration.RootConfiguration)
			throw new InvalidOperationException("The configuration is a child configuration (try to load the root configuration instead).");

		lock (configuration.Sync)
		{
			// load existing configuration file
			mXmlDocument = new XmlDocument();
			try
			{
				if (File.Exists(mConfigurationFilePath))
					mXmlDocument.Load(mConfigurationFilePath);
			}
			catch (Exception ex)
			{
				throw new ConfigurationException(
					$"Loading configuration file ({mConfigurationFilePath}) failed.",
					ex);
			}

			// read configuration from the xml document into the configuration (should not throw any exception)
			if (mXmlDocument.SelectSingleNode("//ConfigurationFile") is XmlElement root)
			{
				LoadInternal(configuration, root);
			}
		}
	}

	/// <summary>
	/// Reads the specified XML element (a 'Configuration' element) and updates the corresponding items in the specified
	/// configuration.
	/// </summary>
	/// <param name="configuration">Configuration to update.</param>
	/// <param name="parent">Parent element in the XML tree.</param>
	private void LoadInternal(CascadedConfiguration configuration, XmlNode parent)
	{
		// read 'Configuration' element
		if (parent.SelectSingleNode($"Configuration[@name='{configuration.Name}']") is XmlElement configurationElement)
		{
			foreach (ICascadedConfigurationItem item in configuration.Items)
			{
				if (configurationElement.SelectSingleNode($"Item[@name='{item.Name}']") is XmlElement itemElement)
				{
					item.Value = GetValueFromXmlElement(itemElement, item.Name, item.Type);
					item.Comment = GetCommentFromXmlElement(itemElement);
				}
				else
				{
					item.ResetValue();
				}
			}

			// load child configurations
			foreach (CascadedConfiguration child in configuration.Children)
			{
				LoadInternal(child, configurationElement);
			}
		}
		else
		{
			configuration.ResetItems(true);
		}
	}

	/// <inheritdoc/>
	public override void LoadItem(ICascadedConfigurationItem item)
	{
		CascadedConfigurationBase configuration = item.Configuration.RootConfiguration;
		lock (configuration.Sync)
		{
			if (mXmlDocument?.SelectSingleNode("//ConfigurationFile") is not XmlElement root) return;
			if (root.SelectSingleNode($"Configuration[@name='{configuration.Name}']") is XmlElement rootConfigurationElement)
			{
				LoadItemInternal(item, item.Path.TrimStart('/'), rootConfigurationElement);
			}
		}
	}

	/// <summary>
	/// Recursion helper for the <see cref="LoadItem(ICascadedConfigurationItem)"/> method.
	/// </summary>
	/// <param name="item">Item to load.</param>
	/// <param name="remainingPath">Remaining path to the requested item.</param>
	/// <param name="parent">XML node of the parent configuration.</param>
	private void LoadItemInternal(ICascadedConfigurationItem item, string remainingPath, XmlNode parent)
	{
		// split the path into segments
		string[] pathTokens = CascadedConfigurationPathHelper.SplitPath(
			this,
			remainingPath,
			isItemPath: true,
			checkValidity: true);

		int index = 0;
		while (true)
		{
			if (index + 1 < pathTokens.Length)
			{
				string itemName = CascadedConfigurationPathHelper.UnescapeName(pathTokens[index]);
				if (parent.SelectSingleNode($"Configuration[@name='{itemName}']") is XmlElement configurationElement)
				{
					parent = configurationElement;
					index++;
					continue;
				}
			}
			else
			{
				if (parent.SelectSingleNode($"Item[@name='{item.Name}']") is not XmlElement itemElement) return;
				item.Value = GetValueFromXmlElement(itemElement, item.Name, item.Type);
				item.Comment = GetCommentFromXmlElement(itemElement);
			}

			break;
		}
	}

	/// <summary>
	/// Reads the comment preceding the specified element.
	/// </summary>
	/// <param name="element">XML element containing the inner text to read.</param>
	/// <returns>
	/// The inner text of the comment node preceding the item node;<br/>
	/// <see langword="null"/> there is no comment.
	/// </returns>
	private static string GetCommentFromXmlElement(XmlElement element)
	{
		// read comment, if any
		if (element.ParentNode != null)
		{
			for (int i = 0; i < element.ParentNode.ChildNodes.Count; i++)
			{
				if (element.ParentNode.ChildNodes[i] != element)
					continue;

				if (i <= 0) continue;
				if (element.ParentNode.ChildNodes[i - 1] is XmlComment commentNode)
					return commentNode.InnerText;
			}
		}

		return null;
	}

	/// <summary>
	/// Reads the inner text from the specified XML element and parses it to an instance of the specified type.
	/// </summary>
	/// <param name="element">XML element containing the inner text to read.</param>
	/// <param name="itemPath">Configuration path of the configuration item that corresponds to the value to read.</param>
	/// <param name="type">Type of the configuration item (influences how the inner text of the XML element is parsed).</param>
	/// <returns>An instance of the specified type.</returns>
	/// <exception cref="ConfigurationException">Parsing the inner text of the specified element failed.</exception>
	private object GetValueFromXmlElement(XmlNode element, string itemPath, Type type)
	{
		if (type.IsArray && type.GetArrayRank() == 1)
		{
			// an item value is stored using nested 'Item' elements
			Type elementType = type.GetElementType();
			XmlNodeList nodeList = element.SelectNodes("Item");
			Debug.Assert(elementType != null, nameof(elementType) + " != null");
			Debug.Assert(nodeList != null, nameof(nodeList) + " != null");
			var array = Array.CreateInstance(elementType, nodeList.Count);
			int i = 0;
			foreach (XmlElement itemElement in nodeList)
			{
				object obj = GetValueFromXmlElement(itemElement, itemPath, elementType);
				array.SetValue(obj, i++);
			}

			return array;
		}

		// try to convert the string to the corresponding value using a converter
		IConverter converter = GetValueConverter(type) ?? Converters.GetGlobalConverter(type);
		if (converter != null)
		{
			try
			{
				return converter.ConvertStringToObject(element.InnerText, CultureInfo.InvariantCulture);
			}
			catch (Exception)
			{
				throw new ConfigurationException(
					"Parsing configuration item failed (item: {0}, item type: {1}).",
					itemPath,
					type.FullName);
			}
		}

		// try to read a complex type
		using (new ReaderWriterLockSlimAutoLock(sSupportedComplexTypesLock, ReaderWriterLockSlimAcquireKind.Read))
		{
			if (sSupportedComplexTypes.TryGetValue(type, out CacheItem cacheItem))
			{
				if (!cacheItem.IsSupported)
				{
					throw new ConfigurationException(
						"The configuration contains an item with a complex type the persistence strategy cannot make persistent (item: {0}, item type: {1}).",
						itemPath,
						type.FullName);
				}

				// read the complex object
				object complexObject = type.IsValueType
					                       ? Activator.CreateInstance(type)
					                       : cacheItem.ParameterlessConstructor.Invoke(null);
				for (int i = 0; i < element.ChildNodes.Count; i++)
				{
					if (element.ChildNodes[i] is XmlElement { Name: "Field" } childElement)
					{
						string fieldName = childElement.GetAttribute("name");

						if (!string.IsNullOrEmpty(fieldName))
						{
							bool found = false;
							foreach (FieldInfo field in cacheItem.Fields)
							{
								if (field.Name != fieldName)
									continue;

								object value = GetValueFromXmlElement(childElement, itemPath, field.FieldType);
								field.SetValue(complexObject, value);
								found = true;
								break;
							}

							if (found)
								continue;

							foreach (PropertyInfo property in cacheItem.Properties)
							{
								if (property.Name != fieldName)
									continue;

								object value = GetValueFromXmlElement(childElement, itemPath, property.PropertyType);
								property.SetValue(complexObject, value);
								// found = true;
								break;
							}
						}
					}
				}

				return complexObject;
			}
		}

		throw new ConfigurationException(
			"The configuration contains an item the persistence strategy cannot make persistent (item: {0}, item type: {1}).",
			itemPath,
			type.FullName);
	}

	/// <inheritdoc/>
	public override bool PeekItem(
		string     path,
		Type       itemType,
		out object value,
		out string comment)
	{
		// after loading the xml document the document is immutable
		// => no locking necessary...

		value = default;
		comment = default;

		// abort if the root element is not 'ConfigurationFile'
		if (mXmlDocument?.SelectSingleNode("//ConfigurationFile") is not XmlElement root)
			return false;

		// split the path into segments for further processing
		string[] pathSegments = CascadedConfigurationPathHelper.SplitPath(
			strategy: this,
			path: path,
			isItemPath: true,
			checkValidity: true);

		// start with the root configuration element and walk down the tree in xml
		if (root.SelectSingleNode("Configuration") is XmlElement rootConfigurationElement)
		{
			return PeekItemInternal(
				rootConfigurationElement,
				pathSegments,
				startIndex: 0,
				itemType,
				out value,
				out comment);
		}

		return false;
	}

	private bool PeekItemInternal(
		XmlElement parent,
		string[]   pathSegments,
		int        startIndex,
		Type       itemType,
		out object value,
		out string comment)
	{
		Debug.Assert(startIndex < pathSegments.Length);

		value = default;
		comment = default;

		int index = startIndex;
		while (true)
		{
			if (index + 1 < pathSegments.Length)
			{
				string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[index]);
				if (parent.SelectSingleNode($"Configuration[@name='{itemName}']") is XmlElement configurationElement)
				{
					index++;
					parent = configurationElement;
					continue;
				}
			}
			else
			{
				string itemName = CascadedConfigurationPathHelper.UnescapeName(pathSegments[index]);
				if (parent.SelectSingleNode($"Item[@name='{itemName}']") is not XmlElement itemElement) return false;
				value = GetValueFromXmlElement(itemElement, itemName, itemType);
				comment = GetCommentFromXmlElement(itemElement);
				return true;
			}

			break;
		}

		return false;
	}

	/// <inheritdoc/>
	public override void Save(CascadedConfiguration configuration, CascadedConfigurationSaveFlags flags)
	{
		if (mConfigurationFilePath == null)
			throw new ConfigurationException("The configuration is a child configuration (try to save the root configuration instead).");

		// load existing configuration file
		var doc = new XmlDocument();
		try
		{
			if (File.Exists(mConfigurationFilePath))
				doc.Load(mConfigurationFilePath);
		}
		catch (Exception ex)
		{
			throw new ConfigurationException(
				$"Loading existing configuration file ({mConfigurationFilePath}) before saving failed.",
				ex);
		}

		// create root node, if necessary
		if (doc.SelectSingleNode("//ConfigurationFile") is not XmlElement root)
		{
			root = doc.CreateElement("ConfigurationFile");
			doc.AppendChild(root);
		}

		// modify the xml document to reflect the settings in the configuration
		lock (configuration.Sync)
		{
			SaveInternal(configuration, root, flags);
		}

		// save the xml document
		string directoryPath = Path.GetDirectoryName(mConfigurationFilePath);
		if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
		string tempFile = mConfigurationFilePath + ".tmp";
		doc.Save(tempFile);
		try { File.Delete(mConfigurationFilePath); }
		catch
		{
			/* swallow */
		}

		File.Move(tempFile, mConfigurationFilePath);
	}

	/// <summary>
	/// Writes the specified configuration into the specified XML element
	/// </summary>
	/// <param name="configuration">Configuration to write.</param>
	/// <param name="parent">Parent element in the XML tree.</param>
	/// <param name="flags">Flags controlling the saving behavior.</param>
	private void SaveInternal(CascadedConfiguration configuration, XmlNode parent, CascadedConfigurationSaveFlags flags)
	{
		// create 'Configuration' element
		if (parent.SelectSingleNode($"Configuration[@name='{configuration.Name}']") is not XmlElement configurationElement)
		{
			Debug.Assert(parent.OwnerDocument != null, "parent.OwnerDocument != null");
			configurationElement = parent.OwnerDocument.CreateElement("Configuration");
			XmlAttribute configurationNameAttribute = parent.OwnerDocument.CreateAttribute("name");
			configurationNameAttribute.InnerText = configuration.Name;
			configurationElement.Attributes.Append(configurationNameAttribute);
			parent.AppendChild(configurationElement);
		}

		// add configuration items
		foreach (ICascadedConfigurationItem item in configuration.Items)
		{
			if (item.HasValue || flags.HasFlag(CascadedConfigurationSaveFlags.SaveInheritedSettings))
			{
				// clean up old 'Item' element or add a new one
				// (this ensures that the 'Item' element is edited in-place, if possible)
				if (configurationElement.SelectSingleNode($"Item[@name='{item.Name}']") is XmlElement itemElement)
				{
					itemElement.RemoveAll(); // removes children + attributes
				}
				else
				{
					itemElement = parent.OwnerDocument!.CreateElement(name: "Item");
					configurationElement.AppendChild(itemElement);
				}

				// attach the 'name' attribute to the 'Item' element
				XmlAttribute nameAttribute = parent.OwnerDocument!.CreateAttribute(name: "name");
				nameAttribute.InnerText = item.Name;
				itemElement.Attributes.Append(nameAttribute);

				// put the value of the item into the 'Item' element
				PopulateItemValue(itemElement, configuration, item.Name, item.Type, item.Value);

				// remove all comment nodes before the node
				for (int i = 0; i < configurationElement.ChildNodes.Count; i++)
				{
					XmlNode node = configurationElement.ChildNodes[i];
					if (node != itemElement) continue;

					for (int j = i; j > 0; j--)
					{
						node = configurationElement.ChildNodes[j - 1];
						Debug.Assert(node != null, nameof(node) + " != null");
						if (node.NodeType != XmlNodeType.Comment) break;
						configurationElement.RemoveChild(node);
					}

					break;
				}

				// add comment nodes
				if (item.Comment == null) continue;
				string[] commentLines = item.Comment.Split('\n');
				foreach (string commentLine in commentLines)
				{
					string line = commentLine.Trim();
					if (line.Length <= 0) continue;
					Debug.Assert(configurationElement.OwnerDocument != null, "configurationElement.OwnerDocument != null");
					XmlComment commentNode = configurationElement.OwnerDocument.CreateComment(line);
					configurationElement.InsertBefore(commentNode, itemElement);
				}
			}
			else
			{
				RemoveItem(configurationElement, item.Name);
			}
		}

		// save child configurations
		foreach (CascadedConfiguration child in configuration.Children)
		{
			SaveInternal(child, configurationElement, flags);
		}
	}

	/// <summary>
	/// Populates the specified 'Item' element with the specified value.
	/// </summary>
	/// <param name="parent">The parent XML element of the 'Item' element to populate.</param>
	/// <param name="configuration">Configuration the configuration item is in.</param>
	/// <param name="itemName">Name of the 'Item' element to add/set.</param>
	/// <param name="type">Type of the value to set.</param>
	/// <param name="value">Value of the 'Item' element to add/set (<see langword="null"/> to remove the item).</param>
	private void PopulateItemValue(
		XmlElement            parent,
		CascadedConfiguration configuration,
		string                itemName,
		Type                  type,
		object                value)
	{
		// handle one-dimensional item arrays separately using nested 'Item' elements
		if (type.IsArray && type.GetArrayRank() == 1)
		{
			// add new xml elements, one for each array element
			var array = (Array)value;
			Type elementType = type.GetElementType();
			for (int i = 0; i < array.Length; i++)
			{
				XmlElement itemElement = parent.OwnerDocument!.CreateElement(name: "Item");
				parent.AppendChild(itemElement);
				PopulateItemValue(itemElement, configuration, itemName, elementType, array.GetValue(i));
			}

			return;
		}

		// try to get a converter that has been registered with the configuration
		IConverter converter = GetValueConverter(type) ?? Converters.GetGlobalConverter(type);
		if (converter != null)
		{
			// found converter, write configuration item into the xml document
			string s = converter.ConvertObjectToString(value, CultureInfo.InvariantCulture);
			if (value != null) parent.InnerText = s;
			return;
		}

		// try to serialize the object using its public fields and properties
		using (new ReaderWriterLockSlimAutoLock(sSupportedComplexTypesLock, ReaderWriterLockSlimAcquireKind.Read))
		{
			bool isCached = sSupportedComplexTypes.TryGetValue(type, out CacheItem cacheItem);
			Debug.Assert(isCached, message: "The analysis should have been done already when adding an item.");
			if (cacheItem.IsSupported)
			{
				// abort if there is nothing to write
				if (value == null)
					return;

				// write fields
				foreach (FieldInfo field in cacheItem.Fields)
				{
					// add 'Field' element with a 'name' attribute to the parent element
					XmlElement fieldElement = parent.OwnerDocument!.CreateElement(name: "Field");
					XmlAttribute nameAttribute = parent.OwnerDocument!.CreateAttribute(name: "name");
					nameAttribute.InnerText = field.Name;
					fieldElement.Attributes.Append(nameAttribute);
					parent.AppendChild(fieldElement);
					PopulateItemValue(fieldElement, configuration, itemName, field.FieldType, field.GetValue(value));
				}

				// write properties
				foreach (PropertyInfo property in cacheItem.Properties)
				{
					// add 'Field' element with a 'name' attribute to the 'Item' element
					XmlElement fieldElement = parent.OwnerDocument!.CreateElement(name: "Field");
					XmlAttribute nameAttribute = parent.OwnerDocument!.CreateAttribute(name: "name");
					nameAttribute.InnerText = property.Name;
					fieldElement.Attributes.Append(nameAttribute);
					parent.AppendChild(fieldElement);
					PopulateItemValue(fieldElement, configuration, itemName, property.PropertyType, property.GetValue(value));
				}

				return;
			}
		}

		throw new ConfigurationException(
			format: "The configuration contains an item the persistence strategy cannot make persistent (item: {0}, item type: {1}).",
			CascadedConfigurationPathHelper.CombinePath(configuration.Path, CascadedConfigurationPathHelper.EscapeName(itemName)),
			type.FullName);
	}

	/// <summary>
	/// Removes the XML element ('Item') with the specified name in the 'name' attribute.
	/// </summary>
	/// <param name="parent">The parent XML element of the 'Item' element to remove.</param>
	/// <param name="itemName">Name of the 'Item' element to remove.</param>
	private static void RemoveItem(XmlNode parent, string itemName)
	{
		if (parent.SelectSingleNode($"Item[@name='{itemName}']") is not XmlElement itemElement)
			return;

		// remove all comment nodes before the item element
		for (int i = 0; i < parent.ChildNodes.Count; i++)
		{
			XmlNode node = parent.ChildNodes[i];
			if (node != itemElement) continue;
			for (int j = i; j > 0; j--)
			{
				node = parent.ChildNodes[j - 1];
				Debug.Assert(node != null, nameof(node) + " != null");
				if (node.NodeType != XmlNodeType.Comment) break;
				parent.RemoveChild(node);
			}

			break;
		}

		// remove the item element itself
		parent.RemoveChild(itemElement);
	}
}
