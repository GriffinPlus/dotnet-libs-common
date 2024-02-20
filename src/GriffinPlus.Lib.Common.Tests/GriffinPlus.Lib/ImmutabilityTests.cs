///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Xunit;

#pragma warning disable 169 // The field '...' is never used

namespace GriffinPlus.Lib;

/// <summary>
/// Unit tests targeting the <see cref="Immutability"/> class.
/// </summary>
public class ImmutabilityTests
{
	#region Common Test Data

	public class TestClass_Mutable
	{
		private string mField;
	}

	public struct TestStruct_Mutable
	{
		private string mField;
	}

	[Immutable]
	public class TestClass_Public_Unsealed_ImmutableByAttribute
	{
		private string mField;
	}

	[Immutable]
	internal class TestClass_Internal_Unsealed_ImmutableByAttribute_NoDerivations
	{
		private string mField;
	}

	[Immutable]
	internal class TestClass_Internal_Unsealed_ImmutableByAttribute_HasMutableDerivation
	{
		private string mField;
	}

	internal class TestClass_Mutable_DerivedFromImmutable : TestClass_Internal_Unsealed_ImmutableByAttribute_HasMutableDerivation
	{
		private string mField;
	}

	[Immutable]
	public sealed class TestClass_Sealed_ImmutableByAttribute
	{
		private string mField;
	}

	[Immutable]
	public struct TestStruct_ImmutableByAttribute
	{
		private string mField;
	}

	public static IEnumerable<object[]> IsImmutable_TestData
	{
		get
		{
			// primitive types are inherently immutable
			// ----------------------------------------------------------------------------------------------------
			string reason = "primitive type, inherently immutable";
			yield return [typeof(bool), true, true, reason];
			yield return [typeof(sbyte), true, true, reason];
			yield return [typeof(byte), true, true, reason];
			yield return [typeof(short), true, true, reason];
			yield return [typeof(ushort), true, true, reason];
			yield return [typeof(int), true, true, reason];
			yield return [typeof(uint), true, true, reason];
			yield return [typeof(long), true, true, reason];
			yield return [typeof(ulong), true, true, reason];
			yield return [typeof(float), true, true, reason];
			yield return [typeof(double), true, true, reason];
			yield return [typeof(char), true, true, reason];
			yield return [typeof(nint), true, true, reason];
			yield return [typeof(nuint), true, true, reason];

			// built-in types that are known to be immutable in practice
			// ----------------------------------------------------------------------------------------------------
			reason = "builtin type, known to be immutable";
			yield return [typeof(Guid), true, true, reason];
			yield return [typeof(DateTime), true, true, reason];
			yield return [typeof(DateTimeOffset), true, true, reason];
			yield return [typeof(object), true, true, reason];
			yield return [typeof(string), true, true, reason];
			yield return [typeof(TimeSpan), true, true, reason];
			yield return [typeof(TimeZoneInfo), true, true, reason];
			yield return [typeof(Type), true, true, reason];
			yield return [typeof(Uri), true, true, reason];

			// enum types
			// ----------------------------------------------------------------------------------------------------
			reason = "enum type, inherently immutable";
			yield return [typeof(DateTimeKind), true, true, reason];

			// types that have been annotated with the 'Immutable' attribute
			// ----------------------------------------------------------------------------------------------------
			reason = "type was declared immutable (by attribute)";

			// unsealed public class, may have mutable derivations in other assemblies
			yield return [typeof(TestClass_Public_Unsealed_ImmutableByAttribute), true, false, reason];

			// unsealed internal class, current assembly does not contain mutable derivations
			yield return [typeof(TestClass_Internal_Unsealed_ImmutableByAttribute_NoDerivations), true, true, reason];

			// unsealed internal class, current assembly contains mutable derivations
			yield return [typeof(TestClass_Internal_Unsealed_ImmutableByAttribute_HasMutableDerivation), true, false, reason];

			// sealed class, cannot have derivations
			yield return [typeof(TestClass_Sealed_ImmutableByAttribute), true, true, reason];

			// struct, cannot have derivations
			yield return [typeof(TestStruct_ImmutableByAttribute), true, true, reason];

			// interfaces
			// ----------------------------------------------------------------------------------------------------
			reason = "interface type, inherently mutable";
			yield return [typeof(IDisposable), false, false, reason];

			// mutable types
			// ----------------------------------------------------------------------------------------------------
			reason = "analysis yielded mutability";
			yield return [typeof(TestClass_Mutable), false, false, reason];
			yield return [typeof(TestStruct_Mutable), false, false, reason];
			yield return [typeof(TestClass_Mutable_DerivedFromImmutable), false, false, reason];
		}
	}

	#endregion

	#region IsImmutable<T>() and IsImmutable(...)

	/// <summary>
	/// Tests whether <see cref="Immutability.IsImmutable{T}"/> works as expected.
	/// </summary>
	/// <param name="type">Type to check for immutability.</param>
	/// <param name="isImmutable">
	/// <c>true</c>, if the type is expected to be reported as immutable;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	/// <param name="hasImmutableDerivationsOnly">
	/// <c>true</c> if the type is immutable and all derived types (if any) are immutable as well;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	/// <param name="reason">Expected reason associated with the evaluation result.</param>
	[Theory]
	[MemberData(nameof(IsImmutable_TestData))]
	public void IsImmutableT(
		Type   type,
		bool   isImmutable,
		bool   hasImmutableDerivationsOnly,
		string reason)
	{
		// invoke IsImmutable<T>()
		MethodInfo method = typeof(Immutability)
			.GetMethods()
			.First(x => x.Name == nameof(Immutability.IsImmutable) && x.IsGenericMethod)
			.MakeGenericMethod(type);
		object actualIsImmutable = method.Invoke(null, []);
		Assert.Equal(isImmutable, actualIsImmutable);

		// check cached information about the type
		Immutability.Info info = Immutability.EvaluatedTypeInfos.FirstOrDefault(x => x.Type == type);
		Assert.NotNull(info);
		Assert.Equal(isImmutable, info.IsImmutable);
		Assert.Equal(hasImmutableDerivationsOnly, info.HasImmutableDerivationsOnly);
		Assert.Equal(reason, info.Reason);
	}

	/// <summary>
	/// Tests whether <see cref="Immutability.IsImmutable"/> works as expected.
	/// </summary>
	/// <param name="type">Type to check for immutability.</param>
	/// <param name="isImmutable">
	/// <c>true</c>, if the type is expected to be reported as immutable;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	/// <param name="hasImmutableDerivationsOnly">
	/// <c>true</c> if the type is immutable and all derived types (if any) are immutable as well;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	/// <param name="reason">Expected reason associated with the evaluation result.</param>
	[Theory]
	[MemberData(nameof(IsImmutable_TestData))]
	public void IsImmutable(
		Type   type,
		bool   isImmutable,
		bool   hasImmutableDerivationsOnly,
		string reason)
	{
		// invoke IsImmutable(Type)
		bool actualIsImmutable = Immutability.IsImmutable(type);
		Assert.Equal(isImmutable, actualIsImmutable);

		// check cached information about the type
		Immutability.Info info = Immutability.EvaluatedTypeInfos.FirstOrDefault(x => x.Type == type);
		Assert.NotNull(info);
		Assert.Equal(isImmutable, info.IsImmutable);
		Assert.Equal(hasImmutableDerivationsOnly, info.HasImmutableDerivationsOnly);
		Assert.Equal(reason, info.Reason);
	}

	#endregion

	#region IsImmutableFieldType<T>() and IsImmutableFieldType(...)

	/// <summary>
	/// Tests whether <see cref="Immutability.HasImmutableDerivationsOnly{T}"/> works as expected.
	/// </summary>
	/// <param name="type">Type to check for immutability.</param>
	/// <param name="isImmutable">
	/// <c>true</c>, if the type is expected to be reported as immutable;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	/// <param name="hasImmutableDerivationsOnly">
	/// <c>true</c> if the type is immutable and all derived types (if any) are immutable as well;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	/// <param name="reason">Expected reason associated with the evaluation result.</param>
	[Theory]
	[MemberData(nameof(IsImmutable_TestData))]
	public void HasImmutableDerivationsOnlyT(
		Type   type,
		bool   isImmutable,
		bool   hasImmutableDerivationsOnly,
		string reason)
	{
		// invoke IsImmutable<T>()
		MethodInfo method = typeof(Immutability)
			.GetMethods()
			.First(x => x.Name == nameof(Immutability.HasImmutableDerivationsOnly) && x.IsGenericMethod)
			.MakeGenericMethod(type);
		object actualHasImmutableDerivationsOnly = method.Invoke(null, []);
		Assert.Equal(hasImmutableDerivationsOnly, actualHasImmutableDerivationsOnly);

		// check cached information about the type
		Immutability.Info info = Immutability.EvaluatedTypeInfos.FirstOrDefault(x => x.Type == type);
		Assert.NotNull(info);
		Assert.Equal(isImmutable, info.IsImmutable);
		Assert.Equal(hasImmutableDerivationsOnly, info.HasImmutableDerivationsOnly);
		Assert.Equal(reason, info.Reason);
	}

	/// <summary>
	/// Tests whether <see cref="Immutability.IsImmutable"/> works as expected.
	/// </summary>
	/// <param name="type">Type to check for immutability.</param>
	/// <param name="isImmutable">
	/// <c>true</c>, if the type is expected to be reported as immutable;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	/// <param name="hasImmutableDerivationsOnly">
	/// <c>true</c> if the type is immutable and all derived types (if any) are immutable as well;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	/// <param name="reason">Expected reason associated with the evaluation result.</param>
	[Theory]
	[MemberData(nameof(IsImmutable_TestData))]
	public void HasImmutableDerivationsOnly(
		Type   type,
		bool   isImmutable,
		bool   hasImmutableDerivationsOnly,
		string reason)
	{
		// invoke IsImmutable(Type)
		bool actualHasImmutableDerivationsOnly = Immutability.HasImmutableDerivationsOnly(type);
		Assert.Equal(hasImmutableDerivationsOnly, actualHasImmutableDerivationsOnly);

		// check cached information about the type
		Immutability.Info info = Immutability.EvaluatedTypeInfos.FirstOrDefault(x => x.Type == type);
		Assert.NotNull(info);
		Assert.Equal(isImmutable, info.IsImmutable);
		Assert.Equal(hasImmutableDerivationsOnly, info.HasImmutableDerivationsOnly);
		Assert.Equal(reason, info.Reason);
	}

	#endregion

	#region AddImmutableType<T>()

	public class AddImmutableTypeT_TestClass_Mutable
	{
		private string mField;
	}

	public struct AddImmutableTypeT_TestStruct_Mutable
	{
		private string mField;
	}

	public static IEnumerable<object[]> AddImmutableTypeT_TestData
	{
		get
		{
			yield return [typeof(AddImmutableTypeT_TestClass_Mutable)];
			yield return [typeof(AddImmutableTypeT_TestStruct_Mutable)];
		}
	}

	/// <summary>
	/// Tests overriding immutability information using <see cref="Immutability.AddImmutableType{T}"/>.
	/// </summary>
	/// <param name="type">Type to declare immutable.</param>
	[Theory]
	[MemberData(nameof(AddImmutableTypeT_TestData))]
	public void AddImmutableTypeT(Type type)
	{
		const string reason = "type was declared immutable (by method)";

		// at start, no information about the type should be in the cache
		Immutability.Info info = Immutability.EvaluatedTypeInfos.FirstOrDefault(x => x.Type == type);
		Assert.Null(info);

		// invoke AddImmutableType<T>() to register the type as immutable
		MethodInfo method = typeof(Immutability)
			.GetMethods()
			.First(x => x.Name == nameof(Immutability.AddImmutableType) && x.IsGenericMethod)
			.MakeGenericMethod(type);
		method.Invoke(null, []);

		// now some information about the type should be in the cache
		info = Immutability.EvaluatedTypeInfos.FirstOrDefault(x => x.Type == type);
		Assert.NotNull(info);
		Assert.True(info.IsImmutable);
		Assert.Equal(reason, info.Reason);
	}

	#endregion

	#region AddImmutable(...)

	public class AddImmutableType_TestClass_Mutable
	{
		private string mField;
	}

	public struct AddImmutableType_TestStruct_Mutable
	{
		private string mField;
	}

	public static IEnumerable<object[]> AddImmutableType_TestData
	{
		get
		{
			yield return [typeof(AddImmutableType_TestClass_Mutable)];
			yield return [typeof(AddImmutableType_TestStruct_Mutable)];
		}
	}

	/// <summary>
	/// Tests overriding immutability information using <see cref="Immutability.AddImmutableType"/>.
	/// </summary>
	/// <param name="type">Type to declare immutable.</param>
	[Theory]
	[MemberData(nameof(AddImmutableType_TestData))]
	public void AddImmutableType(Type type)
	{
		const string reason = "type was declared immutable (by method)";

		// at start, no information about the type should be in the cache
		Immutability.Info info = Immutability.EvaluatedTypeInfos.FirstOrDefault(x => x.Type == type);
		Assert.Null(info);

		// invoke AddImmutableType(...) to register the type as immutable
		Immutability.AddImmutableType(type);

		// now some information about the type should be in the cache
		info = Immutability.EvaluatedTypeInfos.FirstOrDefault(x => x.Type == type);
		Assert.NotNull(info);
		Assert.True(info.IsImmutable);
		Assert.Equal(reason, info.Reason);
	}

	#endregion
}
