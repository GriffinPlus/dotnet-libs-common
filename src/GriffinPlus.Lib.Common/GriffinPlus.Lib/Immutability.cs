///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

// ReSharper disable InconsistentlySynchronizedField

namespace GriffinPlus.Lib
{

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
		private static volatile Dictionary<Type, Info> sCache = new Dictionary<Type, Info>(); // immutable, dictionary is exchanged atomically
		private static readonly object                 sSync  = new object();

		private const string Reason_PrimitiveType             = "primitive type, inherently immutable";
		private const string Reason_BuiltinType               = "builtin type known to be immutable";
		private const string Reason_EnumType                  = "enum type, inherently immutable";
		private const string Reason_OverrideByMethod          = "type was declared immutable (by method)";
		private const string Reason_OverrideByAttribute       = "type was declared immutable (by attribute)";
		private const string Reason_AnalysisYieldedMutability = "analysis yielded mutability";

		/// <summary>
		/// Initializes the <see cref="Immutability"/> class.
		/// </summary>
		static Immutability()
		{
			// all primitive type are inherently immutable
			AddImmutableType(typeof(bool), Reason_PrimitiveType);
			AddImmutableType(typeof(sbyte), Reason_PrimitiveType);
			AddImmutableType(typeof(byte), Reason_PrimitiveType);
			AddImmutableType(typeof(short), Reason_PrimitiveType);
			AddImmutableType(typeof(ushort), Reason_PrimitiveType);
			AddImmutableType(typeof(int), Reason_PrimitiveType);
			AddImmutableType(typeof(uint), Reason_PrimitiveType);
			AddImmutableType(typeof(long), Reason_PrimitiveType);
			AddImmutableType(typeof(ulong), Reason_PrimitiveType);
			AddImmutableType(typeof(float), Reason_PrimitiveType);
			AddImmutableType(typeof(double), Reason_PrimitiveType);
			AddImmutableType(typeof(char), Reason_PrimitiveType);
			AddImmutableType(typeof(IntPtr), Reason_PrimitiveType);
			AddImmutableType(typeof(UIntPtr), Reason_PrimitiveType);

			// other built-in types known to be immutable in practice
			AddImmutableType(typeof(Guid), Reason_BuiltinType);
			AddImmutableType(typeof(DateTime), Reason_BuiltinType);
			AddImmutableType(typeof(DateTimeOffset), Reason_BuiltinType);
			AddImmutableType(typeof(string), Reason_BuiltinType);
			AddImmutableType(typeof(TimeSpan), Reason_BuiltinType);
			AddImmutableType(typeof(TimeZoneInfo), Reason_BuiltinType);
			AddImmutableType(typeof(Type), Reason_BuiltinType);
			AddImmutableType(typeof(Uri), Reason_BuiltinType);
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
		public static bool IsImmutable(Type type)
		{
			if (!sCache.TryGetValue(type, out var info))
			{
				info = Analyze(type);
				lock (sSync)
				{
					sCache = new Dictionary<Type, Info>(sCache) { [type] = info };
				}
			}

			return info.IsImmutable;
		}

		/// <summary>
		/// Registers the specified as immutable, overrides immutability analysis results.
		/// You must only add a type, if you're 100% sure that the type is immutable.
		/// </summary>
		/// <typeparam name="T">Type to register as immutable.</typeparam>
		/// <returns>Information about the registered type.</returns>
		public static Info AddImmutableType<T>()
		{
			return AddImmutableType(typeof(T), Reason_OverrideByMethod);
		}

		/// <summary>
		/// Registers the specified as immutable, overrides immutability analysis results.
		/// You must only add a type, if you're 100% sure that the type is immutable.
		/// </summary>
		/// <param name="type">Type to register as immutable.</param>
		/// <returns>Information about the registered type.</returns>
		public static Info AddImmutableType(Type type)
		{
			return AddImmutableType(type, Reason_OverrideByMethod);
		}

		/// <summary>
		/// Registers the specified as immutable, overrides immutability analysis results.
		/// You must only add a type, if you're 100% sure that the type is immutable.
		/// </summary>
		/// <param name="type">Type to register as immutable.</param>
		/// <param name="reason">Reason describing what led to the immutability evaluation.</param>
		/// <returns>Information about the registered type.</returns>
		private static Info AddImmutableType(Type type, string reason)
		{
			lock (sSync)
			{
				var cache = new Dictionary<Type, Info>(sCache);
				var info = new Info(type, true, reason);
				cache[type] = info;
				sCache = cache;
				return info;
			}
		}

		/// <summary>
		/// Analyzes the specified type for immutability.
		/// </summary>
		/// <param name="type">Type to analyze.</param>
		/// <returns>Information about the analyzed type.</returns>
		private static Info Analyze(Type type)
		{
			// all primitive type are inherently immutable
			if (type.IsPrimitive)
				return new Info(type, true, Reason_PrimitiveType);

			// types that are annotated with the 'Immutable' attribute are considered
			// immutable by contract (in hope the implementer has checked this properly)
			var attribute = type.GetCustomAttribute<ImmutableAttribute>();
			if (attribute != null)
				return new Info(type, true, Reason_OverrideByAttribute);

			// enum types are immutable as they are always backed by primitive types
			if (type.IsEnum)
				return new Info(type, true, Reason_EnumType);

			// TODO: Add additional analysis steps using reflection here to examine the type

			// not sure whether the type is immutable
			// => assume that the type is mutable
			return new Info(type, false, Reason_AnalysisYieldedMutability);
		}
	}

}
