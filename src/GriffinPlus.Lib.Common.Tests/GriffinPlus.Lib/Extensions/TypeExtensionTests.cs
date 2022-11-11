///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

using Xunit;

#pragma warning disable IDE0060   // Remove unused parameter
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters

namespace GriffinPlus.Lib
{

	/// <summary>
	/// Unit tests targeting the <see cref="TypeExtensions"/> class.
	/// </summary>
	public class TypeExtensionTests
	{
		#region Decompose()

		/// <summary>
		/// Tests the <see cref="TypeExtensions.Decompose"/> method.
		/// </summary>
		/// <param name="type">Type to decompose.</param>
		/// <param name="expected">The expected type decomposition.</param>
		[Theory]
		[MemberData(nameof(DecomposedTypeTestData.TestData), MemberType = typeof(DecomposedTypeTestData))]
		public void Decompose(Type type, DecomposedType expected)
		{
			DecomposedType decomposition = type.Decompose();
			Assert.Equal(expected, decomposition);
		}

		/// <summary>
		/// Checks whether <see cref="TypeExtensions.Decompose"/> returns the same type decomposition for the same
		/// type when querying twice.
		/// </summary>
		[Fact]
		public void Decompose_SameDecompositionForSameType()
		{
			DecomposedType decomposition1 = typeof(DecomposedTypeTestData.TestClass).Decompose();
			DecomposedType decomposition2 = typeof(DecomposedTypeTestData.TestClass).Decompose();
			Assert.Same(decomposition1, decomposition2);
		}

		/// <summary>
		/// Checks whether <see cref="TypeExtensions.Decompose"/> raises an exception if the specified type is <c>null</c>.
		/// </summary>
		[Fact]
		public void Decompose_TypeIsNull()
		{
			var exception = Assert.Throws<ArgumentNullException>(() => ((Type)null).Decompose());
			Assert.Equal("this", exception.ParamName);
		}

		#endregion
	}

}

#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
#pragma warning restore IDE0060   // Remove unused parameter
