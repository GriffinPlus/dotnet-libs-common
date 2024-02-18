///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace GriffinPlus.Lib
{

	/// <summary>
	/// Extension methods for <see cref="System.Type"/>.
	/// </summary>
	public static class TypeExtensions
	{
		private static readonly Regex sExtractGenericArgumentTypeRegex = new("^([^`]+)`\\d+$", RegexOptions.Compiled);

		/// <summary>
		/// Gets all public properties of the specified type.
		/// </summary>
		/// <param name="type">Type to get all public properties from.</param>
		/// <returns>All properties of the specified type.</returns>
		public static PropertyInfo[] GetPublicProperties(this Type type)
		{
			if (type.IsInterface)
			{
				var propertyInfos = new List<PropertyInfo>();

				var considered = new HashSet<Type>();
				var queue = new Queue<Type>();
				considered.Add(type);
				queue.Enqueue(type);
				while (queue.Count > 0)
				{
					Type subType = queue.Dequeue();
					foreach (Type subInterface in subType.GetInterfaces())
					{
						if (!considered.Add(subInterface)) continue;
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
				var methodInfos = new List<MethodInfo>();

				var considered = new HashSet<Type>();
				var queue = new Queue<Type>();
				considered.Add(type);
				queue.Enqueue(type);
				while (queue.Count > 0)
				{
					Type subType = queue.Dequeue();
					foreach (Type subInterface in subType.GetInterfaces())
					{
						if (!considered.Add(subInterface)) continue;
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
		/// Determines whether the specified type is immutable.
		/// </summary>
		/// <param name="typeToCheck">Type to check.</param>
		/// <returns>
		/// <c>true</c>, if the specified type is immutable;
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool IsImmutable(this Type typeToCheck)
		{
			return Immutability.IsImmutable(typeToCheck);
		}

		/// <summary>
		/// Determines whether the specified type is a subclass of the specified generic class.
		/// </summary>
		/// <param name="typeToCheck">Type to check.</param>
		/// <param name="genericType">Generic class to check for.</param>
		/// <returns>
		/// <c>true</c>, if the specified type derives from the specified generic type;
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool IsSubclassOfRawGeneric(this Type typeToCheck, Type genericType)
		{
			while (typeToCheck != null && typeToCheck != typeof(object))
			{
				Type currentType = typeToCheck.IsGenericType ? typeToCheck.GetGenericTypeDefinition() : typeToCheck;
				if (genericType == currentType) return true;
				typeToCheck = typeToCheck.BaseType;
			}

			return false;
		}

		/// <summary>
		/// Decomposes the type.
		/// The result contains generic type definitions and non-generic types only.
		/// </summary>
		/// <returns>Information about the decomposed type.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="this"/> is <c>null</c>.</exception>
		public static DecomposedType Decompose(this Type @this)
		{
			if (@this == null) throw new ArgumentNullException(nameof(@this));
			return TypeDecomposer.DecomposeType(@this);
		}

		/// <summary>
		/// Formats the type as C# programmers know it.
		/// The type is formatted with namespace and type name, but without its declaring assembly.
		/// Constructed generic types and generic type definitions are supported.
		/// </summary>
		/// <returns>The formatted type.</returns>
		public static string ToCSharpFormattedString(this Type @this)
		{
			void AppendName(StringBuilder sb, Type t)
			{
				TypeInfo typeInfo = t.GetTypeInfo();
				if (typeInfo.IsGenericParameter)
				{
					sb.Append(typeInfo.Name);
				}
				else if (typeInfo.IsGenericType)
				{
					sb.Append(typeInfo.Namespace);
					sb.Append('.');
					Match match = sExtractGenericArgumentTypeRegex.Match(typeInfo.Name);
					sb.Append(match.Groups[1].Value);
					sb.Append('<');
					if (typeInfo.IsConstructedGenericType)
					{
						for (int i = 0; i < typeInfo.GenericTypeArguments.Length; i++)
						{
							if (i > 0) sb.Append(',');
							AppendName(sb, typeInfo.GenericTypeArguments[i]);
						}
					}
					else
					{
						for (int i = 0; i < typeInfo.GenericTypeParameters.Length; i++)
						{
							if (i > 0) sb.Append(',');
							AppendName(sb, typeInfo.GenericTypeParameters[i]);
						}
					}

					sb.Append('>');
				}
				else
				{
					sb.Append(typeInfo.Namespace);
					sb.Append('.');
					sb.Append(typeInfo.Name);
				}
			}

			var builder = new StringBuilder();
			AppendName(builder, @this);
			return builder.ToString();
		}
	}

}
