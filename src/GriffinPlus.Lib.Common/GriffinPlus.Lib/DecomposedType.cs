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
	/// Some information about a type and the types that compose it (immutable).
	/// </summary>
	[Immutable]
	public sealed class DecomposedType : IEquatable<DecomposedType>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DecomposedType"/> class.
		/// </summary>
		/// <param name="composedType">The composed type.</param>
		/// <param name="type">The type that is part of the decomposition.</param>
		/// <param name="genericTypeArguments">
		/// The list of generic type arguments, if <paramref name="type"/> is a generic type definition.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="composedType"/>, <paramref name="type"/> or <paramref name="genericTypeArguments"/> is <c>null</c>.
		/// </exception>
		internal DecomposedType(
			Type                        composedType,
			Type                        type,
			IEnumerable<DecomposedType> genericTypeArguments)
		{
			ComposedType = composedType ?? throw new ArgumentNullException(nameof(composedType));
			Type = type ?? throw new ArgumentNullException(nameof(type));
			GenericTypeArguments = (genericTypeArguments ?? throw new ArgumentNullException(nameof(genericTypeArguments))).ToList().AsReadOnly();
		}

		/// <summary>
		/// Gets an empty array of <see cref="DecomposedType"/>.
		/// </summary>
		public static DecomposedType[] EmptyTypes { get; } = Array.Empty<DecomposedType>();

		/// <summary>
		/// Gets the composed type, i.e. the type that is the result of recursively
		/// composing <see cref="Type"/> and <see cref="GenericTypeArguments"/>.
		/// </summary>
		public Type ComposedType { get; }

		/// <summary>
		/// Gets the type that is part of the decomposition
		/// (can be a generic type definition or a non-generic type).
		/// </summary>
		public Type Type { get; }

		/// <summary>
		/// Gets the list of generic type arguments, if <see cref="Type"/> is a generic type definition.
		/// The list can contain other generic type definitions and non-generic types.
		/// </summary>
		public IReadOnlyList<DecomposedType> GenericTypeArguments { get; }

		/// <summary>
		/// Checks whether the current <see cref="DecomposedType"/> equals the specified one.
		/// </summary>
		/// <param name="other">The <see cref="DecomposedType"/> to compare with.</param>
		/// <returns>
		/// <c>true</c> if the current <see cref="DecomposedType"/> equals the specified one;
		/// otherwise <c>false</c>.
		/// </returns>
		public bool Equals(DecomposedType other)
		{
			if (ReferenceEquals(null, other))
				return false;

			if (ReferenceEquals(this, other))
				return true;

			return ComposedType == other.ComposedType &&
			       Type == other.Type &&
			       GenericTypeArguments.SequenceEqual(other.GenericTypeArguments);
		}

		/// <summary>
		/// Checks whether the current <see cref="DecomposedType"/> equals the specified object.
		/// </summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns>
		/// <c>true</c> if the current <see cref="DecomposedType"/> equals the specified object;
		/// otherwise <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || (obj is DecomposedType other && Equals(other));
		}

		/// <summary>
		/// Gets the hash code of the current <see cref="DecomposedType"/>.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = ComposedType.GetHashCode();
				hashCode = (hashCode * 397) ^ Type.GetHashCode();
				foreach (var decomposedType in GenericTypeArguments)
				{
					hashCode = (hashCode * 397) ^ decomposedType.GetHashCode();
				}

				return hashCode;
			}
		}
	}

}
