///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GriffinPlus.Lib.Collections
{

	/// <summary>
	/// Unit tests targeting the <see cref="TypeKeyedDictionary{TValue}"/> class.
	/// </summary>
	public abstract class TypeKeyedDictionaryTests_Base<TValue> : GenericDictionaryTests_Base<Type, TValue>
	{
		/// <summary>
		/// A comparer for <see cref="System.Type"/> taking only the assembly-qualified type name into account.
		/// </summary>
		private class TypeComparer : IComparer<Type>
		{
			/// <summary>
			/// An instance of the <see cref="TypeComparer"/> class.
			/// </summary>
			public static readonly TypeComparer Instance = new TypeComparer();

			/// <summary>
			/// Compares the assembly qualified name of the specified types with each other.
			/// </summary>
			/// <param name="x">First type to compare.</param>
			/// <param name="y">Second type to compare.</param>
			/// <returns>
			/// -1, if the assembly qualified name of <paramref name="x"/> is less than the assembly qualified name of <paramref name="y"/>.<br/>
			/// 1, if the assembly qualified name of <paramref name="x"/> is greater than the assembly qualified name of <paramref name="y"/>.<br/>
			/// 0, if the assembly qualified name of both types is the same.
			/// </returns>
			public int Compare(Type x, Type y)
			{
				if (x == null && y == null) return 0;
				if (x == null) return -1;
				if (y == null) return 1;
				return StringComparer.Ordinal.Compare(x.AssemblyQualifiedName, y.AssemblyQualifiedName);
			}
		}

		/// <summary>
		/// Gets a comparer for comparing keys.
		/// </summary>
		protected override IComparer<Type> KeyComparer => TypeComparer.Instance;

		/// <summary>
		/// Gets an equality comparer for comparing keys.
		/// </summary>
		protected override IEqualityComparer<Type> KeyEqualityComparer => EqualityComparer<Type>.Default;

		/// <summary>
		/// Gets a key that is guaranteed to be not in the generated test data set.
		/// </summary>
		protected override Type KeyNotInTestData => typeof(TypeKeyedDictionary<>);

		/// <summary>
		/// Gets types defined in all assemblies loaded into the current application domain,
		/// except <see cref="KeyNotInTestData"/>.
		/// </summary>
		/// <returns>Types defined in assemblies loaded into the current application domain, except <see cref="KeyNotInTestData"/>.</returns>
		protected Type[] GetTypes()
		{
			return AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => !a.IsDynamic)
				.SelectMany(a =>
				{
					try
					{
						return a.GetTypes();
					}
					catch (ReflectionTypeLoadException ex)
					{
						return ex.Types;
					}
				})
				.Where(x => x != KeyNotInTestData)
				.ToArray();
		}
	}

}
