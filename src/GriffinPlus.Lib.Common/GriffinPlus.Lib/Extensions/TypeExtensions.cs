///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/GriffinPlus/dotnet-libs-common)
//
// Copyright 2018-2019 Sascha Falk <sascha@falk-online.eu>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for
// the specific language governing permissions and limitations under the License.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GriffinPlus.Lib
{
	/// <summary>
	/// Extension methods for <see cref="System.Type"/>.
	/// </summary>
	public static class TypeExtensions
	{
		/// <summary>
		/// Gets all public properties of the specified type.
		/// </summary>
		/// <param name="type">Type to get all public properties from.</param>
		/// <returns>All properties of the specified type.</returns>
		public static PropertyInfo[] GetPublicProperties(this Type type)
		{
			if (type.IsInterface)
			{
				List<PropertyInfo> propertyInfos = new List<PropertyInfo>();

				HashSet<Type> considered = new HashSet<Type>();
				Queue<Type> queue = new Queue<Type>();
				considered.Add(type);
				queue.Enqueue(type);
				while (queue.Count > 0)
				{
					Type subType = queue.Dequeue();
					foreach (Type subInterface in subType.GetInterfaces())
					{
						if (considered.Contains(subInterface)) continue;
						considered.Add(subInterface);
						queue.Enqueue(subInterface);
					}

					PropertyInfo[] typeProperties = subType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
					IEnumerable<PropertyInfo> newPropertyInfos = typeProperties.Where(x => !propertyInfos.Contains(x));
					propertyInfos.InsertRange(0, newPropertyInfos);
				}

				return propertyInfos.ToArray();
			}

			return type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
		}

		/// <summary>
		/// Gets all public methods of the specified type.
		/// </summary>
		/// <param name="type">Type to get all public methods from.</param>
		/// <returns>All methods of the specified type.</returns>
		public static MethodInfo[] GetPublicMethods(this Type type)
		{
			if (type.IsInterface)
			{
				List<MethodInfo> methodInfos = new List<MethodInfo>();

				HashSet<Type> considered = new HashSet<Type>();
				Queue<Type> queue = new Queue<Type>();
				considered.Add(type);
				queue.Enqueue(type);
				while (queue.Count > 0)
				{
					Type subType = queue.Dequeue();
					foreach (Type subInterface in subType.GetInterfaces())
					{
						if (considered.Contains(subInterface)) continue;
						considered.Add(subInterface);
						queue.Enqueue(subInterface);
					}

					MethodInfo[] typeMethods = subType.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
					IEnumerable<MethodInfo> newMethodInfos = typeMethods.Where(x => !methodInfos.Contains(x));
					methodInfos.InsertRange(0, newMethodInfos);
				}

				return methodInfos.ToArray();
			}

			return type.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
		}

		/// <summary>
		/// Determines whether the specified type is a subclass of the specified generic class.
		/// </summary>
		/// <param name="genericType">Generic class to check for.</param>
		/// <param name="typeToCheck">Type to check.</param>
		/// <returns>true, if the specified type derives from the specified generic type; otherwise false.</returns>
		public static bool IsSubclassOfRawGeneric(this Type typeToCheck, Type genericType)
		{
			while (typeToCheck != null && typeToCheck != typeof(object))
			{
				var currentType = typeToCheck.IsGenericType ? typeToCheck.GetGenericTypeDefinition() : typeToCheck;
				if (genericType == currentType) return true;
				typeToCheck = typeToCheck.BaseType;
			}
			return false;
		}

	}
}
