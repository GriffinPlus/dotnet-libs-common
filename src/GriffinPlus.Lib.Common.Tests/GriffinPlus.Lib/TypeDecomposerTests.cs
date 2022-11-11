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
	/// Unit tests targeting the <see cref="TypeDecomposer"/> class.
	/// </summary>
	public class TypeDecomposerTests
	{
		/// <summary>
		/// Tests the <see cref="TypeDecomposer.DecomposeType"/> method.
		/// </summary>
		/// <param name="type">Type to decompose.</param>
		/// <param name="expected">The expected type decomposition.</param>
		[Theory]
		[MemberData(nameof(DecomposedTypeTestData.TestData), MemberType = typeof(DecomposedTypeTestData))]
		public void DecomposeType(Type type, DecomposedType expected)
		{
			DecomposedType decomposition = TypeDecomposer.DecomposeType(type);
			Assert.Equal(expected, decomposition);
		}

		/// <summary>
		/// Checks whether <see cref="TypeDecomposer.DecomposeType"/> returns the same type decomposition for the same
		/// type when querying twice.
		/// </summary>
		[Fact]
		public void DecomposeType_SameDecompositionForSameType()
		{
			DecomposedType decomposition1 = TypeDecomposer.DecomposeType(typeof(DecomposedTypeTestData.TestClass));
			DecomposedType decomposition2 = TypeDecomposer.DecomposeType(typeof(DecomposedTypeTestData.TestClass));
			Assert.Same(decomposition1, decomposition2);
		}

		/// <summary>
		/// Checks whether <see cref="TypeDecomposer.DecomposeType"/> raises an exception if the specified type is <c>null</c>.
		/// </summary>
		[Fact]
		public void DecomposeType_TypeIsNull()
		{
			var exception = Assert.Throws<ArgumentNullException>(() => TypeDecomposer.DecomposeType(null));
			Assert.Equal("type", exception.ParamName);
		}
	}

}

#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
#pragma warning restore IDE0060   // Remove unused parameter
