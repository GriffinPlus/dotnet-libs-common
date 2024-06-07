///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;

using GriffinPlus.Lib.Conversion;

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// A persistence strategy that enables a <see cref="CascadedConfiguration"/> to persist its data in an XML file.
/// </summary>
public class XmlFilePersistenceStrategy : CascadedConfigurationPersistenceStrategy
{
	private readonly string      mConfigurationFilePath;
	private          XmlDocument mXmlDocument;

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

	/// <inheritdoc/>
	public override bool SupportsType(Type type)
	{
		// check fixed types
		if (type.IsArray && type.GetArrayRank() == 1 && SupportsType(type.GetElementType())) return true;
		if (type.IsEnum) return true;

		IConverter converter = GetValueConverter(type) ?? Converters.GetGlobalConverter(type);
		return converter != null;
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
		while (true)
		{
			string[] pathTokens = CascadedConfigurationPathHelper.SplitPath(this, remainingPath, true, true);

			if (pathTokens.Length > 1)
			{
				string itemName = CascadedConfigurationPathHelper.UnescapeName(pathTokens[0]);
				if (parent.SelectSingleNode($"Configuration[@name='{itemName}']") is XmlElement configurationElement)
				{
					remainingPath = string.Join("/", pathTokens, 1, pathTokens.Length - 1);
					parent = configurationElement;
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
			// a hash value is stored using nested 'Item' elements
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

		// try to get a converter that has been registered with the configuration
		IConverter converter = GetValueConverter(type) ?? Converters.GetGlobalConverter(type);
		if (converter == null)
		{
			throw new ConfigurationException(
				"The configuration contains an item the persistence strategy cannot make persistent (item: {0}, item type: {1}).",
				itemPath,
				type.FullName);
		}

		// convert the string to the corresponding value
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

		foreach (ICascadedConfigurationItem item in configuration.Items)
		{
			if (item.HasValue || flags.HasFlag(CascadedConfigurationSaveFlags.SaveInheritedSettings))
			{
				XmlElement itemElement = SetItem(configurationElement, configuration, item.Name, item.Type, item.Value);

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
	/// Adds/sets an XML element ('Item') with the specified name in the 'name' attribute and the specified value
	/// as inner text of the 'Item' element.
	/// </summary>
	/// <param name="parent">The parent XML element of the 'Item' element to add/set.</param>
	/// <param name="configuration">Configuration the configuration item is in.</param>
	/// <param name="itemName">Name of the 'Item' element to add/set.</param>
	/// <param name="type">Type of the value to set.</param>
	/// <param name="value">Value of the 'Item' element to add/set (<see langword="null"/> to remove the item).</param>
	private XmlElement SetItem(
		XmlNode               parent,
		CascadedConfiguration configuration,
		string                itemName,
		Type                  type,
		object                value)
	{
		if (type.IsArray && type.GetArrayRank() == 1)
		{
			// a hash value is stored using nested 'Item' elements
			Type elementType = type.GetElementType();
			var array = (Array)value;
			XmlElement arrayElement = SetItem(parent, itemName, null);

			// remove all old xml elements representing a hash element
			foreach (XmlNode node in arrayElement.SelectNodes("Item")!)
			{
				arrayElement.RemoveChild(node);
			}

			// add new xml elements, one for each hash element
			for (int i = 0; i < array.Length; i++)
			{
				object obj = array.GetValue(i);
				SetItem(arrayElement, configuration, null, elementType, obj);
			}

			return arrayElement;
		}

		// try to get a converter that has been registered with the configuration
		IConverter converter = GetValueConverter(type) ?? Converters.GetGlobalConverter(type);
		if (converter == null)
		{
			throw new ConfigurationException(
				"The configuration contains an item the persistence strategy cannot make persistent (configuration: {0}, item: {1}, item type: {2}).",
				configuration.Name,
				itemName,
				type.FullName);
		}

		// found converter, write configuration item into the xml document
		string s = converter.ConvertObjectToString(value, CultureInfo.InvariantCulture);
		return SetItem(parent, itemName, s);
	}

	/// <summary>
	/// Adds/sets an XML element ('Item') of the configuration item.
	/// </summary>
	/// <param name="configurationElement">XML element representing the configuration.</param>
	/// <param name="name">Name of the 'Item' element to add/set.</param>
	/// <param name="value">String representation of the value to add/set.</param>
	/// <returns>The added/set 'Item' element.</returns>
	private static XmlElement SetItem(XmlNode configurationElement, string name, string value)
	{
		// convert the object to a string and put it into the xml document
		if (configurationElement.SelectSingleNode($"Item[@name='{name}']") is XmlElement itemElement)
		{
			if (value != null) itemElement.InnerText = value;
		}
		else
		{
			itemElement = configurationElement.OwnerDocument!.CreateElement("Item");

			if (name != null)
			{
				XmlAttribute nameAttribute = configurationElement.OwnerDocument.CreateAttribute("name");
				nameAttribute.InnerText = name;
				itemElement.Attributes.Append(nameAttribute);
			}

			if (value != null)
				itemElement.InnerText = value;

			configurationElement.AppendChild(itemElement);
		}

		return itemElement;
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
