///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

namespace GriffinPlus.Lib
{

	/// <summary>
	/// Utility class that helps with decomposing types.
	/// Generic types are recursively decomposed to generic type definitions and non-generic types.
	/// </summary>
	public static class TypeDecomposer
	{
		private static volatile Dictionary<Type, DecomposedType> sCache = new Dictionary<Type, DecomposedType>(); // immutable, dictionary is exchanged atomically
		private static readonly object                           sSync  = new object();

		/// <summary>
		/// Decomposes the specified type.
		/// The result contains generic type definitions and non-generic types only.
		/// </summary>
		/// <param name="type">Type to decompose.</param>
		/// <returns>Information about the decomposed type.</returns>
		/// <exception cref="ArgumentNullException">The specified <paramref name="type"/> is <c>null</c>.</exception>
		public static DecomposedType DecomposeType(Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			// check whether the cache contains the decomposition already
			if (sCache.TryGetValue(type, out var decomposedType))
				return decomposedType;

			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				// the type is a generic type
				// => decompose it
				var genericTypeDefinition = type.GetGenericTypeDefinition();
				var genericArguments = type.GetGenericArguments().Select(DecomposeType).ToList();
				decomposedType = new DecomposedType(type, genericTypeDefinition, genericArguments);
			}
			else
			{
				// a non-generic type or a generic type definition
				decomposedType = new DecomposedType(type, type, DecomposedType.EmptyTypes);
			}

			lock (sSync) sCache = new Dictionary<Type, DecomposedType>(sCache) { [type] = decomposedType };
			return decomposedType;
		}
	}

}
