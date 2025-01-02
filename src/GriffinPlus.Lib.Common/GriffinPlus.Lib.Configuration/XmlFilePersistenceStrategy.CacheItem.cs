///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

namespace GriffinPlus.Lib.Configuration;

partial class XmlFilePersistenceStrategy
{
	/// <summary>
	/// Some information about a complex type and whether it is supported by the persistence strategy.
	/// </summary>
	/// <param name="type">The type the information is about.</param>
	/// <param name="isSupported">Indicates whether the type is supported by the persistence strategy.</param>
	/// <param name="constructor">The parameterless constructor.</param>
	/// <param name="fields">Information about public fields that are subject to serialization.</param>
	/// <param name="properties">Information about properties that are subject to serialization.</param>
	private sealed class CacheItem(
		Type            type,
		bool            isSupported,
		ConstructorInfo constructor,
		FieldInfo[]     fields,
		PropertyInfo[]  properties)
	{
		/// <summary>
		/// The type the information is about.
		/// </summary>
		public readonly Type Type = type;

		/// <summary>
		/// Indicates whether the type is supported by the persistence strategy.
		/// </summary>
		public readonly bool IsSupported = isSupported;

		/// <summary>
		/// The parameterless constructor.
		/// </summary>
		public readonly ConstructorInfo ParameterlessConstructor = constructor;

		/// <summary>
		/// Information about public fields that are subject to serialization.
		/// </summary>
		public readonly FieldInfo[] Fields = fields;

		/// <summary>
		/// Information about properties that are subject to serialization.
		/// </summary>
		public readonly PropertyInfo[] Properties = properties;
	}
}
