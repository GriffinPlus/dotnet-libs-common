///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

using Xunit;

#pragma warning disable xUnit1024 // Test methods cannot have overloads

namespace GriffinPlus.Lib;

/// <summary>
/// Unit tests targeting the <see cref="DecomposedType"/> class.
/// </summary>
public class DecomposedTypeTests
{
	/// <summary>
	/// Tests creating an instance of the <see cref="DecomposedType"/> class.
	/// </summary>
	[Fact]
	public void Create()
	{
		// create decomposed type (invalid, it's for checking only)
		var decomposedType = new DecomposedType(typeof(int), typeof(uint), DecomposedType.EmptyTypes);
		Assert.Equal(typeof(int), decomposedType.ComposedType);
		Assert.Equal(typeof(uint), decomposedType.Type);
		Assert.Empty(decomposedType.GenericTypeArguments);
	}

	/// <summary>
	/// Tests creating an instance of the <see cref="DecomposedType"/> class.
	/// The constructor should throw an exception, if the 'composedType' argument is <c>null</c>.
	/// </summary>
	[Fact]
	public void Create_ComposedTypeIsNull()
	{
		var exception = Assert.Throws<ArgumentNullException>(
			() => new DecomposedType(
				null,
				typeof(int),
				DecomposedType.EmptyTypes));

		Assert.Equal("composedType", exception.ParamName);
	}

	/// <summary>
	/// Tests creating an instance of the <see cref="DecomposedType"/> class.
	/// The constructor should throw an exception, if the 'type' argument is <c>null</c>.
	/// </summary>
	[Fact]
	public void Create_TypeIsNull()
	{
		var exception = Assert.Throws<ArgumentNullException>(
			() => new DecomposedType(
				typeof(int),
				null,
				DecomposedType.EmptyTypes));

		Assert.Equal("type", exception.ParamName);
	}

	/// <summary>
	/// Tests creating an instance of the <see cref="DecomposedType"/> class.
	/// The constructor should throw an exception, if the 'genericTypeArguments' argument is <c>null</c>.
	/// </summary>
	[Fact]
	public void Create_GenericTypeArgumentsIsNull()
	{
		var exception = Assert.Throws<ArgumentNullException>(
			() => new DecomposedType(
				typeof(int),
				typeof(int),
				null));

		Assert.Equal("genericTypeArguments", exception.ParamName);
	}

	/// <summary>
	/// Tests <see cref="DecomposedType.GetHashCode"/>, <see cref="DecomposedType.Equals(DecomposedType)"/> and
	/// <see cref="DecomposedType.Equals(object)"/>.
	/// </summary>
	[Fact]
	public void GetHashCode_And_Equals()
	{
		// testing GetHashCode() is difficult as unstable type hashes get into the combined hash code
		// => check whether all fields get into the hash code by changing fields and comparing the result

		// note: the decomposed types are invalid, it's all for checking GetHashCode()...

		// create reference
		var decomposedTypeReference = new DecomposedType(typeof(int), typeof(int), DecomposedType.EmptyTypes);
		int referenceHashCode = decomposedTypeReference.GetHashCode();

		// reference should always equal itself
		Assert.True(decomposedTypeReference.Equals(decomposedTypeReference));
		Assert.True(decomposedTypeReference.Equals((object)decomposedTypeReference));

		// reference should never equal null
		Assert.False(decomposedTypeReference.Equals(null));
		Assert.False(decomposedTypeReference.Equals((object)null));

		// same as reference (should equal and return the same hash code)
		var decomposedTypeEqual = new DecomposedType(typeof(int), typeof(int), DecomposedType.EmptyTypes);
		Assert.True(decomposedTypeReference.Equals(decomposedTypeEqual));
		Assert.True(decomposedTypeReference.Equals((object)decomposedTypeEqual));
		Assert.Equal(referenceHashCode, decomposedTypeEqual.GetHashCode());

		// different composed type
		var decomposedTypeDiff1 = new DecomposedType(typeof(uint), typeof(int), DecomposedType.EmptyTypes);
		Assert.False(decomposedTypeReference.Equals(decomposedTypeDiff1));
		Assert.False(decomposedTypeReference.Equals((object)decomposedTypeDiff1));
		Assert.NotEqual(referenceHashCode, decomposedTypeDiff1.GetHashCode());

		// different decomposed type
		var decomposedTypeDiff2 = new DecomposedType(typeof(int), typeof(uint), DecomposedType.EmptyTypes);
		Assert.False(decomposedTypeReference.Equals(decomposedTypeDiff2));
		Assert.False(decomposedTypeReference.Equals((object)decomposedTypeDiff2));
		Assert.NotEqual(referenceHashCode, decomposedTypeDiff2.GetHashCode());

		// different generic type arguments
		var decomposedTypeDiff3 = new DecomposedType(
			typeof(int),
			typeof(int),
			[
				new DecomposedType(typeof(int), typeof(int), DecomposedType.EmptyTypes)
			]);
		Assert.False(decomposedTypeReference.Equals(decomposedTypeDiff3));
		Assert.False(decomposedTypeReference.Equals((object)decomposedTypeDiff3));
		Assert.NotEqual(referenceHashCode, decomposedTypeDiff3.GetHashCode());
	}
}

#pragma warning restore xUnit1024 // Test methods cannot have overloads
