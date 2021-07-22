///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

#pragma warning disable 169 // The field '...' is never used

namespace GriffinPlus.Lib
{

	/// <summary>
	/// Unit tests targeting the <see cref="Immutability"/> class.
	/// </summary>
	public class ImmutabilityTests
	{
		#region IsImmutable<T>() and IsImmutable(...)

		public class TestClass_Mutable
		{
			private string mField;
		}

		public struct TestStruct_Mutable
		{
			private string mField;
		}

		[Immutable]
		public class TestClass_ImmutableByAttribute
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
				string reason = "primitive type, inherently immutable";
				yield return new object[] { typeof(bool), true, reason };
				yield return new object[] { typeof(sbyte), true, reason };
				yield return new object[] { typeof(byte), true, reason };
				yield return new object[] { typeof(short), true, reason };
				yield return new object[] { typeof(ushort), true, reason };
				yield return new object[] { typeof(int), true, reason };
				yield return new object[] { typeof(uint), true, reason };
				yield return new object[] { typeof(long), true, reason };
				yield return new object[] { typeof(ulong), true, reason };
				yield return new object[] { typeof(float), true, reason };
				yield return new object[] { typeof(double), true, reason };
				yield return new object[] { typeof(char), true, reason };
				yield return new object[] { typeof(IntPtr), true, reason };
				yield return new object[] { typeof(UIntPtr), true, reason };

				// built-in types that are known to be immutable in practice
				reason = "builtin type known to be immutable";
				yield return new object[] { typeof(Guid), true, reason };
				yield return new object[] { typeof(DateTime), true, reason };
				yield return new object[] { typeof(DateTimeOffset), true, reason };
				yield return new object[] { typeof(string), true, reason };
				yield return new object[] { typeof(TimeSpan), true, reason };
				yield return new object[] { typeof(TimeZoneInfo), true, reason };
				yield return new object[] { typeof(Type), true, reason };
				yield return new object[] { typeof(Uri), true, reason };

				// enum types
				reason = "enum type, inherently immutable";
				yield return new object[] { typeof(DateTimeKind), true, reason };

				// types that have been annotated with the 'Immutable' attribute
				reason = "type was declared immutable (by attribute)";
				yield return new object[] { typeof(TestClass_ImmutableByAttribute), true, reason };
				yield return new object[] { typeof(TestStruct_ImmutableByAttribute), true, reason };

				// mutable types
				reason = "analysis yielded mutability";
				yield return new object[] { typeof(TestClass_Mutable), false, reason };
				yield return new object[] { typeof(TestStruct_Mutable), false, reason };
			}
		}

		/// <summary>
		/// Tests whether <see cref="Immutability.IsImmutable{T}"/> works as expected.
		/// </summary>
		/// <param name="type">Type to check for immutability.</param>
		/// <param name="isImmutable">
		/// <c>true</c>, if the type is expected to reported as immutable;
		/// otherwise <c>false</c>.
		/// </param>
		/// <param name="reason">Expected reason associated with the evaluation result.</param>
		[Theory]
		[MemberData(nameof(IsImmutable_TestData))]
		public void IsImmutableT(Type type, bool isImmutable, string reason)
		{
			// invoke IsImmutable<T>()
			var method = typeof(Immutability)
				.GetMethods()
				.First(x => x.Name == nameof(Immutability.IsImmutable) && x.IsGenericMethod)
				.MakeGenericMethod(type);
			object actualIsImmutable = method.Invoke(null, new object[] { });
			Assert.Equal(isImmutable, actualIsImmutable);

			// check cached information about the type
			var info = Immutability.EvaluatedTypeInfos.FirstOrDefault(x => x.Type == type);
			Assert.NotNull(info);
			Assert.Equal(isImmutable, info.IsImmutable);
			Assert.Equal(reason, info.Reason);
		}

		/// <summary>
		/// Tests whether <see cref="Immutability.IsImmutable"/> works as expected.
		/// </summary>
		/// <param name="type">Type to check for immutability.</param>
		/// <param name="isImmutable">
		/// <c>true</c>, if the type is expected to reported as immutable;
		/// otherwise <c>false</c>.
		/// </param>
		/// <param name="reason">Expected reason associated with the evaluation result.</param>
		[Theory]
		[MemberData(nameof(IsImmutable_TestData))]
		public void IsImmutable(Type type, bool isImmutable, string reason)
		{
			// invoke IsImmutable(Type)
			bool actualIsImmutable = Immutability.IsImmutable(type);
			Assert.Equal(isImmutable, actualIsImmutable);

			// check cached information about the type
			var info = Immutability.EvaluatedTypeInfos.FirstOrDefault(x => x.Type == type);
			Assert.NotNull(info);
			Assert.Equal(isImmutable, info.IsImmutable);
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
				yield return new object[] { typeof(AddImmutableTypeT_TestClass_Mutable) };
				yield return new object[] { typeof(AddImmutableTypeT_TestStruct_Mutable) };
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
			var info = Immutability.EvaluatedTypeInfos.FirstOrDefault(x => x.Type == type);
			Assert.Null(info);

			// invoke AddImmutableType<T>() to register the type as immutable
			var method = typeof(Immutability)
				.GetMethods()
				.First(x => x.Name == nameof(Immutability.AddImmutableType) && x.IsGenericMethod)
				.MakeGenericMethod(type);
			method.Invoke(null, new object[] { });

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
				yield return new object[] { typeof(AddImmutableType_TestClass_Mutable) };
				yield return new object[] { typeof(AddImmutableType_TestStruct_Mutable) };
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
			var info = Immutability.EvaluatedTypeInfos.FirstOrDefault(x => x.Type == type);
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

}
