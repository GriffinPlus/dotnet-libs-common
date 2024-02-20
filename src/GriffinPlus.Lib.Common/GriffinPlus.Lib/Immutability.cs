///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

// ReSharper disable InconsistentlySynchronizedField

namespace GriffinPlus.Lib;

/// <summary>
/// Utility class that helps to determine whether a type is immutable.
/// A type that is reported to be mutable may in fact be immutable (false-negative),
/// if the analysis is not 100% sure that the type is immutable.
/// A type is considered immutable, if one of the following conditions is met:
/// - the type is a primitive or an enum
/// - the type is annotated with the <see cref="ImmutableAttribute"/>
/// </summary>
public static partial class Immutability
{
	private static volatile Dictionary<Type, Info> sCache = new(); // immutable, dictionary is exchanged atomically
	private static readonly object                 sSync  = new();

	private const string Reason_PrimitiveType             = "primitive type, inherently immutable";
	private const string Reason_BuiltinType               = "builtin type, known to be immutable";
	private const string Reason_EnumType                  = "enum type, inherently immutable";
	private const string Reason_InterfaceType             = "interface type, inherently mutable";
	private const string Reason_OverrideByMethod          = "type was declared immutable (by method)";
	private const string Reason_OverrideByAttribute       = "type was declared immutable (by attribute)";
	private const string Reason_AnalysisYieldedMutability = "analysis yielded mutability";

	/// <summary>
	/// Initializes the <see cref="Immutability"/> class.
	/// </summary>
	static Immutability()
	{
		// primitive types are inherently immutable
		AddImmutableType(typeof(bool), true, Reason_PrimitiveType);
		AddImmutableType(typeof(sbyte), true, Reason_PrimitiveType);
		AddImmutableType(typeof(byte), true, Reason_PrimitiveType);
		AddImmutableType(typeof(short), true, Reason_PrimitiveType);
		AddImmutableType(typeof(ushort), true, Reason_PrimitiveType);
		AddImmutableType(typeof(int), true, Reason_PrimitiveType);
		AddImmutableType(typeof(uint), true, Reason_PrimitiveType);
		AddImmutableType(typeof(long), true, Reason_PrimitiveType);
		AddImmutableType(typeof(ulong), true, Reason_PrimitiveType);
		AddImmutableType(typeof(float), true, Reason_PrimitiveType);
		AddImmutableType(typeof(double), true, Reason_PrimitiveType);
		AddImmutableType(typeof(char), true, Reason_PrimitiveType);
		AddImmutableType(typeof(nint), true, Reason_PrimitiveType);
		AddImmutableType(typeof(nuint), true, Reason_PrimitiveType);

		// other built-in types known to be immutable in practice
		AddImmutableType(typeof(Guid), true, Reason_BuiltinType);
		AddImmutableType(typeof(DateTime), true, Reason_BuiltinType);
		AddImmutableType(typeof(DateTimeOffset), true, Reason_BuiltinType);
		AddImmutableType(typeof(object), true, Reason_BuiltinType);
		AddImmutableType(typeof(string), true, Reason_BuiltinType);
		AddImmutableType(typeof(TimeSpan), true, Reason_BuiltinType);
		AddImmutableType(typeof(TimeZoneInfo), true, Reason_BuiltinType);
		AddImmutableType(typeof(Type), true, Reason_BuiltinType);
		AddImmutableType(typeof(Uri), true, Reason_BuiltinType);
	}

	/// <summary>
	/// Gets information about all types that have been evaluated for immutability.
	/// </summary>
	public static IEnumerable<Info> EvaluatedTypeInfos => sCache.Values;

	/// <summary>
	/// Checks whether the specified type is immutable.
	/// </summary>
	/// <typeparam name="T">Type to check.</typeparam>
	/// <returns>
	/// <c>true</c> if the specified type is immutable;
	/// <c>false</c> if the specified type is not immutable.
	/// </returns>
	public static bool IsImmutable<T>()
	{
		return IsImmutable(typeof(T));
	}

	/// <summary>
	/// Checks whether the specified type is immutable.
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <returns>
	/// <c>true</c> if the specified type is immutable;
	/// <c>false</c> if the specified type is not immutable.
	/// </returns>
	/// <exception cref="ArgumentNullException">The specified <paramref name="type"/> is <c>null</c>.</exception>
	public static bool IsImmutable(Type type)
	{
		if (type == null)
			throw new ArgumentNullException(nameof(type));

		if (sCache.TryGetValue(type, out Info info))
			return info.IsImmutable;

		lock (sSync)
		{
			var cache = new Dictionary<Type, Info>(sCache);
			info = AnalyzeAndAddToCache(cache, type);
			sCache = cache;
		}

		return info.IsImmutable;
	}

	/// <summary>
	/// Checks whether the specified type and derived types (if any) are immutable.
	/// May return <c>false</c> although all derived types are immutable in practice,
	/// but the analysis could not guarantee that this is always the case (false-negative).
	/// </summary>
	/// <typeparam name="T">Type to check.</typeparam>
	/// <returns>
	/// <c>true</c> if the specified type and derived types (if any) are immutable;
	/// otherwise <c>false</c>.
	/// </returns>
	public static bool HasImmutableDerivationsOnly<T>()
	{
		return HasImmutableDerivationsOnly(typeof(T));
	}

	/// <summary>
	/// Checks whether the specified type and derived types (if any) are immutable.
	/// May return <c>false</c> although all derived types are immutable in practice,
	/// but the analysis could not guarantee that this is always the case (false-negative).
	/// </summary>
	/// <param name="type">Type to check.</param>
	/// <returns>
	/// <c>true</c> if the specified type and derived types (if any) are immutable;
	/// otherwise <c>false</c>.
	/// </returns>
	/// <exception cref="ArgumentNullException">The specified <paramref name="type"/> is <c>null</c>.</exception>
	public static bool HasImmutableDerivationsOnly(Type type)
	{
		if (type == null)
			throw new ArgumentNullException(nameof(type));

		if (sCache.TryGetValue(type, out Info info))
			return info.HasImmutableDerivationsOnly;

		lock (sSync)
		{
			var cache = new Dictionary<Type, Info>(sCache);
			info = AnalyzeAndAddToCache(cache, type);
			sCache = cache;
		}

		return info.HasImmutableDerivationsOnly;
	}

	/// <summary>
	/// Declares the specified class/struct immutable, overrides immutability analysis results.
	/// You must only do this, if you are 100% sure that the type is immutable.
	/// </summary>
	/// <typeparam name="T">Type to declare immutable.</typeparam>
	/// <returns>Information about the registered type.</returns>
	public static Info AddImmutableType<T>()
	{
		return AddImmutableType(typeof(T), Reason_OverrideByMethod);
	}

	/// <summary>
	/// Declares the specified class/struct immutable, overrides immutability analysis results.
	/// You must only do this, if you are 100% sure that the type is immutable.
	/// </summary>
	/// <param name="type">Type to declare immutable.</param>
	/// <returns>Information about the registered type.</returns>
	/// <exception cref="ArgumentNullException">The specified <paramref name="type"/> is <c>null</c>.</exception>
	public static Info AddImmutableType(Type type)
	{
		return AddImmutableType(type, Reason_OverrideByMethod);
	}

	/// <summary>
	/// Declares the specified class/struct immutable, overrides immutability analysis results
	/// (analyzes whether the type has derived types that are immutable only).
	/// </summary>
	/// <param name="type">Type to declare immutable.</param>
	/// <param name="reason">Reason describing what led to the immutability evaluation.</param>
	/// <returns>Information about the registered type.</returns>
	/// <exception cref="ArgumentNullException">The specified <paramref name="type"/> is <c>null</c>.</exception>
	private static Info AddImmutableType(Type type, string reason)
	{
		if (type == null) throw new ArgumentNullException(nameof(type));

		if (!type.IsClass && !type.IsValueType)
			throw new ArgumentException($"The specified type '{type.FullName}' is neither a class nor a struct.");

		lock (sSync)
		{
			var cache = new Dictionary<Type, Info>(sCache);
			var info = new Info(type, true, false, reason);
			cache[type] = info;
			info.HasImmutableDerivationsOnly = AreTypeAndDerivedTypesImmutable(cache, type);
			sCache = cache;
			return info;
		}
	}

	/// <summary>
	/// Declares the specified class/struct immutable, overrides immutability analysis results
	/// (sets whether the type has derived types that are immutable only).
	/// </summary>
	/// <param name="type">Type to declare immutable.</param>
	/// <param name="hasImmutableDerivationsOnly">
	/// <c>true</c> if derived types are also immutable; otherwise <c>false</c>.
	/// </param>
	/// <param name="reason">Reason describing what led to the immutability evaluation.</param>
	/// <exception cref="ArgumentNullException">The specified <paramref name="type"/> is <c>null</c>.</exception>
	private static void AddImmutableType(Type type, bool hasImmutableDerivationsOnly, string reason)
	{
		if (type == null) throw new ArgumentNullException(nameof(type));

		if (!type.IsClass && !type.IsValueType)
			throw new ArgumentException($"The specified type '{type.FullName}' is neither a class nor a struct.");

		lock (sSync)
		{
			var cache = new Dictionary<Type, Info>(sCache);
			var info = new Info(type, true, hasImmutableDerivationsOnly, reason);
			cache[type] = info;
			sCache = cache;
		}
	}

	/// <summary>
	/// Analyzes the specified type for immutability.
	/// </summary>
	/// <param name="cache">Currently populated analysis evaluation result cache.</param>
	/// <param name="type">Type to analyze.</param>
	/// <returns>Information about the analyzed type.</returns>
	private static Info AnalyzeAndAddToCache(IDictionary<Type, Info> cache, Type type)
	{
		Info info;

		// all primitive type are inherently immutable
		if (type.IsPrimitive)
		{
			info = new Info(type, true, true, Reason_PrimitiveType);
			cache[type] = info;
			return info;
		}

		// types that are annotated with the 'Immutable' attribute are considered
		// immutable by contract (in hope the implementer has checked this properly)
		var attribute = type.GetCustomAttribute<ImmutableAttribute>();
		if (attribute != null)
		{
			info = new Info(type, true, false, Reason_OverrideByAttribute);
			cache[type] = info;
			info.HasImmutableDerivationsOnly = AreTypeAndDerivedTypesImmutable(cache, type);
			return info;
		}

		// enum types are immutable as they are always backed by primitive types
		if (type.IsEnum)
		{
			info = new Info(type, true, true, Reason_EnumType);
			cache[type] = info;
			return info;
		}

		// interface types cannot be immutable
		if (type.IsInterface)
		{
			info = new Info(type, false, false, Reason_InterfaceType);
			cache[type] = info;
			return info;
		}

		// TODO: Add additional analysis steps using reflection here to examine the type

		// not sure whether the type is immutable
		// => assume that the type is mutable
		info = new Info(type, false, false, Reason_AnalysisYieldedMutability);
		cache[type] = info;
		return info;
	}

	/// <summary>
	/// Checks whether the specified type is immutable and adds the evaluation result to the specified cache.
	/// </summary>
	/// <param name="cache">Currently populated analysis evaluation result cache.</param>
	/// <param name="type">Type to check.</param>
	/// <returns><c>true</c> if the specified type is immutable; otherwise <c>false</c>.</returns>
	private static bool IsImmutable(IDictionary<Type, Info> cache, Type type)
	{
		Debug.Assert(Monitor.IsEntered(sSync));
		if (!cache.TryGetValue(type, out Info info))
		{
			info = AnalyzeAndAddToCache(cache, type);
		}

		return info.IsImmutable;
	}

	/// <summary>
	/// Determines whether the specified type and derived types are immutable.
	/// This is especially important when using the type for a field expecting that the object stored in this field is
	/// immutable. The field can be assigned an object of the specified type or an object of a derived type, so it is
	/// essential to be sure that derived types are immutable as well. The check examines the assembly the specified type
	/// is declared in only, so if there is a chance that there is some type in another assembly deriving from the specified
	/// type, <c>false</c> is returned.
	/// </summary>
	/// <param name="cache">Currently populated analysis evaluation result cache.</param>
	/// <param name="type">Type to check.</param>
	/// <returns>
	/// <c>true</c> if the specified type and derived types are guaranteed to be immutable;
	/// otherwise <c>false</c>.
	/// </returns>
	private static bool AreTypeAndDerivedTypesImmutable(IDictionary<Type, Info> cache, Type type)
	{
		Debug.Assert(Monitor.IsEntered(sSync));

		// abort, if the specified type is not immutable
		if (!IsImmutable(cache, type)) return false;

		// the specified type is immutable
		// => abort, if the type is not a class or a class that has been sealed
		// => no inheritance possible, immutability can be guaranteed
		if (!type.IsClass || type.IsSealed) return true;

		// the type is an unsealed class
		// => perform additional checks

		if (!type.IsVisible)
		{
			// the type is visible inside its assembly only
			// => check derived types in the same assembly for immutability
			return type
				.Assembly
				.GetTypes()
				.Where(x => x.BaseType == type)
				.All(derivedType => AreTypeAndDerivedTypesImmutable(cache, derivedType));
		}

		// the type is visible outside its assembly
		// => if it does not have public, protected or protected internal constructors,
		//    there cannot be derived types defined in some other assembly,
		//    otherwise, immutability cannot be guaranteed as we cannot see all derived classes that may exist
		ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		return !constructors.Any(x => x.IsPublic || x.IsFamily || x.IsFamilyOrAssembly);
	}
}
