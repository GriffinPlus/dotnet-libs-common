///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

#pragma warning disable CS0618 // type or member is obsolete

namespace GriffinPlus.Lib.Cryptography;

/// <summary>
/// Tests targeting the <see cref="SecurePasswordHasher"/> class.
/// </summary>
public class SecurePasswordHasherTests
{
	#region Test Data

	/// <summary>
	/// Test data for the <see cref="VerifyPassword_WithString"/> test method and the <see cref="VerifyPassword_WithSpan"/> test method.
	/// </summary>
	public static IEnumerable<object[]> TestData_VerifyPassword
	{
		get
		{
			yield return ["My Password", "$SHA1$10000$KNBeaGa3wuHptKXLyw2HOPSSTlnGM1D78fAVW+bZnpV8dNCy"];
			yield return ["My Password", "$SHA256$10000$fvnD9a3wElFc72ZVkqGd1tioUQ0eO+gyOvNDF0ZU1Bu26kevy+eCbqRTlMGBVcUG"];
			yield return ["My Password", "$SHA384$10000$BfIJylwtOHwPAyv/vzbzbe47OVOBeYhKkiIdyrtyj1CRpjfhucoqej3ZOtcHqhlbrLPbEvQiuqZgKfJiK7cweg=="];
			yield return ["My Password", "$SHA512$10000$KV8EtTGZFgVBkuNlWNpdooB2aH3hxPrAykH5dz6RB7/j19hP7iZaZQib0x22a3F8HaCrxDDqQvye7yj19vnkrxBQe2Lm6H83rKMCkiJQGBs="];
			yield return ["My Password", "$PBKDF2$10000$ADPaOpUuLQeR1ggvnraz11KDUY60FdzzNKdVp8/iGFxqlPpP"];
			yield return ["My Password", "$PBKDF2-SHA1$10000$ADPaOpUuLQeR1ggvnraz11KDUY60FdzzNKdVp8/iGFxqlPpP"];
#if NET48 || NETCOREAPP3_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0
			yield return ["My Password", "$PBKDF2-SHA256$10000$pamnf76rg587ZF5AlAI9CG1pnsHg7kEI9eYTUlGSg/AAR9HDqQxNiweIiJCYmqNn"];
			yield return ["My Password", "$PBKDF2-SHA512$10000$vCr1ArVabwFt6YNgl2LNMRfaGPrFb+yicQOuTEHnCjQvU84Xb/pvM3WLj1aexLNxeet+DsXyDDhLNyV/Te9JpddDop1be6J1Zrn3pyajizQ="];
#elif NET461 || NETCOREAPP2_2
			// .NET Framework 4.6.1 uses the library built with explicit support for .NET Framework 4.6.1, so there is no support for PBKDF2 with SHA-256 and SHA-512.
			// .NET Core 2.2  uses the library built for .NET Standard 2.0 which does not support PBKDF2 with SHA-256 and SHA-512.
#else
#error Unhandled target framework.
#endif
		}
	}

	/// <summary>
	/// Test data for the <see cref="VerifyPassword_WithString_WrongPasswordHashFormat"/> test method and
	/// the <see cref="VerifyPassword_WithSpan_WrongPasswordHashFormat"/> test method.
	/// </summary>
	public static IEnumerable<object[]> TestData_VerifyPassword_WrongHashFormat
	{
		get { yield return ["XXX"]; }
	}

	/// <summary>
	/// Test data for the <see cref="VerifyPassword_WithString_AlgorithmNotSupported"/> test method and
	/// the <see cref="VerifyPassword_WithSpan_AlgorithmNotSupported"/> test method.
	/// </summary>
	public static IEnumerable<object[]> TestData_VerifyPassword_AlgorithmNotSupported
	{
		get { yield return ["$UNKNOWN$10000$XXXXXXXXXXXXXXXXXXXX"]; }
	}

	/// <summary>
	/// Regular expression matching all kinds of password hashes conforming to the pattern: '$&lt;algorithm&gt;$&lt;iterations&gt;$'
	/// </summary>
	private static readonly Regex sCommonHashRegex = new(@"^\$(?<algorithm>[^\$]*)\$(?<iterations>[^\$]*)\$(?<hash>[^\$]*)", RegexOptions.Compiled);

	#endregion

	#region SecurePasswordHasher.SHA1 (Singleton Instance)

	/// <summary>
	/// Checks whether <see cref="SecurePasswordHasher.SHA1"/> property returns the expected hasher instance.
	/// </summary>
	[Fact]
	public void SHA1()
	{
		SecurePasswordHasher hasher = SecurePasswordHasher.SHA1;
		Assert.IsType<SecurePasswordHasher_SHA1>(hasher);
		Assert.Equal("SHA1", hasher.AlgorithmName);
	}

	#endregion

	#region SecurePasswordHasher.SHA256 (Singleton Instance)

	/// <summary>
	/// Checks whether <see cref="SecurePasswordHasher.SHA256"/> returns the expected hasher instance.
	/// </summary>
	[Fact]
	public void SHA256()
	{
		SecurePasswordHasher hasher = SecurePasswordHasher.SHA256;
		Assert.IsType<SecurePasswordHasher_SHA256>(hasher);
		Assert.Equal("SHA256", hasher.AlgorithmName);
	}

	#endregion

	#region SecurePasswordHasher.SHA384 (Singleton Instance)

	/// <summary>
	/// Checks whether <see cref="SecurePasswordHasher.SHA384"/> returns the expected hasher instance.
	/// </summary>
	[Fact]
	public void SHA384()
	{
		SecurePasswordHasher hasher = SecurePasswordHasher.SHA384;
		Assert.IsType<SecurePasswordHasher_SHA384>(hasher);
		Assert.Equal("SHA384", hasher.AlgorithmName);
	}

	#endregion

	#region SecurePasswordHasher.SHA512 (Singleton Instance)

	/// <summary>
	/// Checks whether <see cref="SecurePasswordHasher.SHA512"/> returns the expected hasher instance.
	/// </summary>
	[Fact]
	public void SHA512()
	{
		SecurePasswordHasher hasher = SecurePasswordHasher.SHA512;
		Assert.IsType<SecurePasswordHasher_SHA512>(hasher);
		Assert.Equal("SHA512", hasher.AlgorithmName);
	}

	#endregion

	#region SecurePasswordHasher.PBKDF2_SHA1 (Singleton Instance)

	/// <summary>
	/// Checks whether <see cref="SecurePasswordHasher.PBKDF2_SHA1"/> returns the expected hasher instance.
	/// </summary>
	[Fact]
	public void PBKDF2_SHA1()
	{
		SecurePasswordHasher hasher = SecurePasswordHasher.PBKDF2_SHA1;
		Assert.IsType<SecurePasswordHasher_PBKDF2_SHA1>(hasher);
		Assert.Equal("PBKDF2-SHA1", hasher.AlgorithmName);
	}

	#endregion

#if NET48 || NETCOREAPP3_1 || NET5_0 || NET6_0 || NET7_0 || NET8_0

	#region SecurePasswordHasher.PBKDF2_SHA256 (Singleton Instance)

	/// <summary>
	/// Checks whether <see cref="SecurePasswordHasher.PBKDF2_SHA256"/> returns the expected hasher instance.
	/// </summary>
	[Fact]
	public void PBKDF2_SHA256()
	{
		SecurePasswordHasher hasher = SecurePasswordHasher.PBKDF2_SHA256;
		Assert.IsType<SecurePasswordHasher_PBKDF2_SHA256>(hasher);
		Assert.Equal("PBKDF2-SHA256", hasher.AlgorithmName);
	}

	#endregion

	#region SecurePasswordHasher.PBKDF2_SHA512 (Singleton Instance)

	/// <summary>
	/// Checks whether <see cref="SecurePasswordHasher.PBKDF2_SHA512"/> returns the expected hasher instance.
	/// </summary>
	[Fact]
	public void PBKDF2_SHA512()
	{
		SecurePasswordHasher hasher = SecurePasswordHasher.PBKDF2_SHA512;
		Assert.IsType<SecurePasswordHasher_PBKDF2_SHA512>(hasher);
		Assert.Equal("PBKDF2-SHA512", hasher.AlgorithmName);
	}

	#endregion

#elif NET461 || NETCOREAPP2_2
	// .NET Framework 4.6.1 uses the library built with explicit support for .NET Framework 4.6.1, so there is no support for PBKDF2 with SHA-256 and SHA-512.
	// .NET Core 2.2  uses the library built for .NET Standard 2.0 which does not support PBKDF2 with SHA-256 and SHA-512.
#else
#error Unhandled target framework.
#endif

	#region bool SecurePasswordHasher.VerifyPassword(string password, string passwordHash)

	/// <summary>
	/// Tests the <see cref="SecurePasswordHasher.VerifyPassword(string,string)"/> method with a set of passwords and password hashes
	/// using supported hashing algorithms.
	/// </summary>
	/// <param name="password">The clear-text password to verify.</param>
	/// <param name="passwordHash">The password hash as generated by the hasher before.</param>
	[Theory]
	[MemberData(nameof(TestData_VerifyPassword))]
	public void VerifyPassword_WithString(string password, string passwordHash)
	{
		// check whether the password is verified successfully against the password hash
		bool verified = SecurePasswordHasher.VerifyPassword(password, passwordHash);
		Assert.True(verified);

		// change the password and try again
		// => the password should not be verified successfully now...
		password += "X";
		verified = SecurePasswordHasher.VerifyPassword(password, passwordHash);
		Assert.False(verified);
	}

	/// <summary>
	/// Tests the <see cref="SecurePasswordHasher.VerifyPassword(string,string)"/> method.
	/// The method should throw an <see cref="ArgumentNullException"/> if the specified password is <c>null</c>.
	/// </summary>
	[Fact]
	public void VerifyPassword_WithString_PasswordIsNull()
	{
		const string password = null;
		const string passwordHash = "XXX";
		var exception = Assert.Throws<ArgumentNullException>(() => SecurePasswordHasher.VerifyPassword(password, passwordHash));
		Assert.Equal("password", exception.ParamName);
	}

	/// <summary>
	/// Tests the <see cref="SecurePasswordHasher.VerifyPassword(string,string)"/> method.
	/// The method should throw an <see cref="ArgumentNullException"/> if the specified password is <c>null</c>.
	/// </summary>
	[Fact]
	public void VerifyPassword_WithString_PasswordHashIsNull()
	{
		const string password = "XXX";
		const string passwordHash = null;
		var exception = Assert.Throws<ArgumentNullException>(() => SecurePasswordHasher.VerifyPassword(password, passwordHash));
		Assert.Equal("passwordHash", exception.ParamName);
	}

	/// <summary>
	/// Tests the <see cref="SecurePasswordHasher.VerifyPassword(string,string)"/> method.
	/// The method should throw a <see cref="FormatException"/> if the password hash does not conform to the common format.
	/// </summary>
	[Theory]
	[MemberData(nameof(TestData_VerifyPassword_WrongHashFormat))]
	public void VerifyPassword_WithString_WrongPasswordHashFormat(string passwordHash)
	{
		var exception = Assert.Throws<FormatException>(() => SecurePasswordHasher.VerifyPassword("Password", passwordHash));
		Assert.Equal($"The specified password hash ({passwordHash}) seems to be improperly formatted.", exception.Message);
	}

	/// <summary>
	/// Tests the <see cref="SecurePasswordHasher.VerifyPassword(string,string)"/> method.
	/// The method should throw a <see cref="NotSupportedException"/> if password hash conforms to the common format,
	/// but an unknown algorithm name is specified.
	/// </summary>
	[Theory]
	[MemberData(nameof(TestData_VerifyPassword_AlgorithmNotSupported))]
	public void VerifyPassword_WithString_AlgorithmNotSupported(string passwordHash)
	{
		// get algorithm field and iterations field from password hash
		Match match = sCommonHashRegex.Match(passwordHash);
		Assert.True(match.Success);
		string algorithm = match.Groups["algorithm"].Value;

		// try to verify the password hash
		var exception = Assert.Throws<NotSupportedException>(() => SecurePasswordHasher.VerifyPassword("Password", passwordHash));
		Assert.Equal($"The hash algorithm ({algorithm}) is not supported.", exception.Message);
	}

	#endregion

	#region bool SecurePasswordHasher.VerifyPassword(ReadOnlySpan<char> password, ReadOnlySpan<char> passwordHash)

	/// <summary>
	/// Tests the <see cref="SecurePasswordHasher.VerifyPassword(ReadOnlySpan{char},ReadOnlySpan{char})"/> method with a set of passwords and password hashes
	/// using supported hashing algorithms.
	/// </summary>
	/// <param name="password">The clear-text password to verify.</param>
	/// <param name="passwordHash">The password hash as generated by the hasher before.</param>
	[Theory]
	[MemberData(nameof(TestData_VerifyPassword))]
	public void VerifyPassword_WithSpan(string password, string passwordHash)
	{
		// check whether the password is verified successfully against the password hash
		bool verified = SecurePasswordHasher.VerifyPassword(password.AsSpan(), passwordHash.AsSpan());
		Assert.True(verified);

		// change the password and try again
		// => the password should not be verified successfully now...
		password += "X";
		verified = SecurePasswordHasher.VerifyPassword(password.AsSpan(), passwordHash.AsSpan());
		Assert.False(verified);
	}

	/// <summary>
	/// Tests the <see cref="SecurePasswordHasher.VerifyPassword(ReadOnlySpan{char},ReadOnlySpan{char})"/> method.
	/// The method should throw an <see cref="ArgumentNullException"/> if the specified password is <c>null</c>.
	/// </summary>
	[Fact]
	public void VerifyPassword_WithSpan_PasswordIsNull()
	{
		const string password = null;
		const string passwordHash = "XXX";
		var exception = Assert.Throws<ArgumentNullException>(() => SecurePasswordHasher.VerifyPassword(password.AsSpan(), passwordHash.AsSpan()));
		Assert.Equal("password", exception.ParamName);
	}

	/// <summary>
	/// Tests the <see cref="SecurePasswordHasher.VerifyPassword(ReadOnlySpan{char},ReadOnlySpan{char})"/> method.
	/// The method should throw an <see cref="ArgumentNullException"/> if the specified password is <c>null</c>.
	/// </summary>
	[Fact]
	public void VerifyPassword_WithSpan_PasswordHashIsNull()
	{
		const string password = "XXX";
		const string passwordHash = null;
		var exception = Assert.Throws<ArgumentNullException>(() => SecurePasswordHasher.VerifyPassword(password.AsSpan(), passwordHash.AsSpan()));
		Assert.Equal("passwordHash", exception.ParamName);
	}

	/// <summary>
	/// Tests the <see cref="SecurePasswordHasher.VerifyPassword(ReadOnlySpan{char},ReadOnlySpan{char})"/> method.
	/// The method should throw a <see cref="FormatException"/> if the password hash does not conform to the common format.
	/// </summary>
	[Theory]
	[MemberData(nameof(TestData_VerifyPassword_WrongHashFormat))]
	public void VerifyPassword_WithSpan_WrongPasswordHashFormat(string passwordHash)
	{
		var exception = Assert.Throws<FormatException>(() => SecurePasswordHasher.VerifyPassword("Password".AsSpan(), passwordHash.AsSpan()));
		Assert.Equal($"The specified password hash ({passwordHash}) seems to be improperly formatted.", exception.Message);
	}

	/// <summary>
	/// Tests the <see cref="SecurePasswordHasher.VerifyPassword(ReadOnlySpan{char},ReadOnlySpan{char})"/> method.
	/// The method should throw a <see cref="NotSupportedException"/> if password hash conforms to the common format,
	/// but an unknown algorithm name is specified.
	/// </summary>
	[Theory]
	[MemberData(nameof(TestData_VerifyPassword_AlgorithmNotSupported))]
	public void VerifyPassword_WithSpan_AlgorithmNotSupported(string passwordHash)
	{
		// get algorithm field and iterations field from password hash
		Match match = sCommonHashRegex.Match(passwordHash);
		Assert.True(match.Success);
		string algorithm = match.Groups["algorithm"].Value;

		// try to verify the password hash
		var exception = Assert.Throws<NotSupportedException>(() => SecurePasswordHasher.VerifyPassword("Password".AsSpan(), passwordHash.AsSpan()));
		Assert.Equal($"The hash algorithm ({algorithm}) is not supported.", exception.Message);
	}

	#endregion
}
