///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Xunit;

// ReSharper disable StringLiteralTypo

namespace GriffinPlus.Lib.Cryptography
{

	/// <summary>
	/// Tests targeting the <see cref="SecurePasswordHasher"/> class.
	/// </summary>
	public abstract class SecurePasswordHasherTests_Base
	{
		/// <summary>
		/// Gets the <see cref="SecurePasswordHasher"/> instance to test.
		/// </summary>
		/// <returns></returns>
		protected abstract SecurePasswordHasher GetHasher();

		/// <summary>
		/// Size of the salt (in bytes).
		/// </summary>
		protected abstract int SaltSize { get; }

		/// <summary>
		/// Size of the hash (in bytes).
		/// </summary>
		protected abstract int HashSize { get; }

		/// <summary>
		/// Gets a regular expression matching a password hash emitted by the tested hasher.
		/// </summary>
		protected abstract Regex PasswordHashRegex { get; }

		#region Common Test Data

		/// <summary>
		/// The number of iterations if no iteration count is specified.
		/// </summary>
		public const int DefaultIterationCount = 10000;

		/// <summary>
		/// Regular expression matching all kinds of password hashes.
		/// The string must start with '$&lt;algorithm&gt;$&lt;iterations&gt;$' only.
		/// </summary>
		private static readonly Regex sCommonHashRegex = new Regex(@"^\$(?<algorithm>[^\$]*)\$(?<iterations>[^\$]*)\$(?<hash>[^\$]*)", RegexOptions.Compiled);

		/// <summary>
		/// Test data for testing hashing with a specific number of iterations and verifying the generated password
		/// hash afterwards.
		/// </summary>
		public static IEnumerable<object[]> HashWithDefaultIterationCountAndVerify_TestData
		{
			get { yield return new object[] { "My Password" }; }
		}

		/// <summary>
		/// Test data for testing hashing with a specific number of iterations and verifying the generated password
		/// hash afterwards.
		/// </summary>
		public static IEnumerable<object[]> HashWithSpecificIterationCountAndVerify_TestData
		{
			get
			{
				yield return new object[] { "My Password", 10000 };
				yield return new object[] { "My Password", 20000 };
			}
		}

		#endregion

		#region string SecurePasswordHasher.Hash(string password, int iterations)

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Hash(string,int)"/> method.
		/// The method should throw an <see cref="ArgumentNullException"/> if the password to hash is <c>null</c>.
		/// </summary>
		[Fact]
		public void Hash_WithString_PasswordIsNull()
		{
			SecurePasswordHasher hasher = GetHasher();
			var exception = Assert.Throws<ArgumentNullException>(
				() =>
				{
					const string password = null;
					return hasher.Hash(password, DefaultIterationCount);
				});
			Assert.Equal("password", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Hash(string,int)"/> method.
		/// The method should throw an <see cref="ArgumentException"/> if the password to hash is <c>null</c>.
		/// </summary>
		[Fact]
		public void Hash_WithString_IterationsIsNegative()
		{
			SecurePasswordHasher hasher = GetHasher();
			var exception = Assert.Throws<ArgumentException>(
				() =>
				{
					const string password = "My Password";
					return hasher.Hash(password, -1);
				});
			Assert.Equal("iterations", exception.ParamName);
			Assert.StartsWith("Iterations must be greater than zero.", exception.Message);
		}

		#endregion

		#region string SecurePasswordHasher.Hash(ReadOnlySpan<char> password, int iterations)

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Hash(ReadOnlySpan{char},int)"/> method.
		/// The method should throw an <see cref="ArgumentNullException"/> if the password to hash is <c>null</c>.
		/// </summary>
		[Fact]
		public void Hash_WithSpan_PasswordIsNull()
		{
			SecurePasswordHasher hasher = GetHasher();
			var exception = Assert.Throws<ArgumentNullException>(
				() =>
				{
					ReadOnlySpan<char> password = null;
					return hasher.Hash(password, DefaultIterationCount);
				});
			Assert.Equal("password", exception.ParamName);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Hash(ReadOnlySpan{char},int)"/> method.
		/// The method should throw an <see cref="ArgumentException"/> if the password to hash is <c>null</c>.
		/// </summary>
		[Fact]
		public void Hash_WithSpan_IterationsIsNegative()
		{
			SecurePasswordHasher hasher = GetHasher();
			var exception = Assert.Throws<ArgumentException>(
				() =>
				{
					ReadOnlySpan<char> password = "My Password".AsSpan();
					return hasher.Hash(password, -1);
				});
			Assert.Equal("iterations", exception.ParamName);
			Assert.StartsWith("Iterations must be greater than zero.", exception.Message);
		}

		#endregion

		#region bool SecurePasswordHasher.Verify(string password, string passwordHash)

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Verify(string,string)"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash contains too less fields.
		/// </summary>
		public virtual void Verify_WithString_TooLessFields(string passwordHash)
		{
			SecurePasswordHasher hasher = GetHasher();
			var exception = Assert.Throws<FormatException>(() => hasher.Verify("irrelevant", passwordHash));
			Assert.Equal("The password hash does not have the expected format ($<algorithm>$<iterations>$<salted-hash>).", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Verify(string,string)"/> method.
		/// The method should throw a <see cref="NotSupportedException"/> if the password hash contains an unhandled algorithm identifier.
		/// </summary>
		public virtual void Verify_WithString_AlgorithmNotSupported(string passwordHash)
		{
			// get algorithm field and iterations field from password hash
			Match match = sCommonHashRegex.Match(passwordHash);
			Assert.True(match.Success);
			string algorithm = match.Groups["algorithm"].Value;

			// test the Verify() method
			SecurePasswordHasher hasher = GetHasher();
			var exception = Assert.Throws<NotSupportedException>(() => hasher.Verify("irrelevant", passwordHash));
			Assert.Equal($"The algorithm ({algorithm}) is not supported.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Verify(string,string)"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash contains an invalid iteration count.
		/// </summary>
		public virtual void Verify_WithString_InvalidIterationCount(string passwordHash)
		{
			// get algorithm field and iterations field from password hash
			Match match = sCommonHashRegex.Match(passwordHash);
			Assert.True(match.Success);
			string iterations = match.Groups["iterations"].Value;

			// test the Verify() method
			SecurePasswordHasher hasher = GetHasher();
			var exception = Assert.Throws<FormatException>(() => hasher.Verify("irrelevant", passwordHash));
			Assert.Equal($"The number of iterations ({iterations}) is not a properly formatted integer value.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Verify(string,string)"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash is too short or too long.
		/// </summary>
		public virtual void Verify_WithString_HashSizeTooShortOrToLong(string passwordHash)
		{
			SecurePasswordHasher hasher = GetHasher();
			var exception = Assert.Throws<FormatException>(() => hasher.Verify("irrelevant", passwordHash));
			Assert.Equal($"The salted hash field in the password hash must be {SaltSize + HashSize} bytes long.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Verify(string,string)"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash contains an improper BASE64 formatted string.
		/// </summary>
		public virtual void Verify_WithString_ImproperEncodedSaltedHash(string passwordHash)
		{
			SecurePasswordHasher hasher = GetHasher();
			var exception = Assert.Throws<FormatException>(() => hasher.Verify("irrelevant", passwordHash));
			Assert.Equal("The salted hash is not encoded properly.", exception.Message);
		}

		#endregion

		#region bool SecurePasswordHasher.Verify(ReadOnlySpan<char> password, ReadOnlySpan<char> passwordHash)

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Verify(ReadOnlySpan{char},ReadOnlySpan{char})"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash contains too less fields.
		/// </summary>
		public virtual void Verify_WithSpan_TooLessFields(string passwordHash)
		{
			SecurePasswordHasher hasher = GetHasher();
			var exception = Assert.Throws<FormatException>(() => hasher.Verify("irrelevant".AsSpan(), passwordHash.AsSpan()));
			Assert.Equal("The password hash does not have the expected format ($<algorithm>$<iterations>$<salted-hash>).", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Verify(ReadOnlySpan{char},ReadOnlySpan{char})"/> method.
		/// The method should throw a <see cref="NotSupportedException"/> if the password hash contains an unhandled algorithm identifier.
		/// </summary>
		public virtual void Verify_WithSpan_AlgorithmNotSupported(string passwordHash)
		{
			// get algorithm field and iterations field from password hash
			Match match = sCommonHashRegex.Match(passwordHash);
			Assert.True(match.Success);
			string algorithm = match.Groups["algorithm"].Value;

			// test the Verify() method
			SecurePasswordHasher hasher = GetHasher();
			var exception = Assert.Throws<NotSupportedException>(() => hasher.Verify("irrelevant".AsSpan(), passwordHash.AsSpan()));
			Assert.Equal($"The algorithm ({algorithm}) is not supported.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Verify(ReadOnlySpan{char},ReadOnlySpan{char})"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash contains an invalid iteration count.
		/// </summary>
		public virtual void Verify_WithSpan_InvalidIterationCount(string passwordHash)
		{
			// get algorithm field and iterations field from password hash
			Match match = sCommonHashRegex.Match(passwordHash);
			Assert.True(match.Success);
			string iterations = match.Groups["iterations"].Value;

			// test the Verify() method
			SecurePasswordHasher hasher = GetHasher();
			var exception = Assert.Throws<FormatException>(() => hasher.Verify("irrelevant".AsSpan(), passwordHash.AsSpan()));
			Assert.Equal($"The number of iterations ({iterations}) is not a properly formatted integer value.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Verify(ReadOnlySpan{char},ReadOnlySpan{char})"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash is too short or too long.
		/// </summary>
		public virtual void Verify_WithSpan_HashSizeTooShortOrToLong(string passwordHash)
		{
			SecurePasswordHasher hasher = GetHasher();
			var exception = Assert.Throws<FormatException>(() => hasher.Verify("irrelevant".AsSpan(), passwordHash.AsSpan()));
			Assert.Equal($"The salted hash field in the password hash must be {SaltSize + HashSize} bytes long.", exception.Message);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher_SHA1.Verify(ReadOnlySpan{char},ReadOnlySpan{char})"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash contains an improper BASE64 formatted string.
		/// </summary>
		public virtual void Verify_WithSpan_ImproperEncodedSaltedHash(string passwordHash)
		{
			SecurePasswordHasher hasher = GetHasher();
			var exception = Assert.Throws<FormatException>(() => hasher.Verify("irrelevant".AsSpan(), passwordHash.AsSpan()));
			Assert.Equal("The salted hash is not encoded properly.", exception.Message);
		}

		#endregion

		#region Hashing and Verifying (with String)

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Hash(string)"/> and the <see cref="SecurePasswordHasher.Verify(string,string)"/> method.
		/// The hasher should use the default number of iterations when hashing.
		/// </summary>
		/// <param name="password">Password to hash and verify.</param>
		[Theory]
		[MemberData(nameof(HashWithDefaultIterationCountAndVerify_TestData))]
		public void HashWithDefaultIterationCountAndVerify_WithString(string password)
		{
			SecurePasswordHasher hasher = GetHasher();
			TestHashingAndVerifying_WithString(hasher, PasswordHashRegex, -1, password);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Hash(string,int)"/> and the <see cref="SecurePasswordHasher.Verify(string,string)"/> method.
		/// The hasher should use the specified number of iterations when hashing.
		/// </summary>
		/// <param name="password">Password to hash and verify.</param>
		/// <param name="iterations">Number of iterations to apply when hashing.</param>
		[Theory]
		[MemberData(nameof(HashWithSpecificIterationCountAndVerify_TestData))]
		public void HashWithSpecificIterationCountAndVerify_WithString(string password, int iterations)
		{
			SecurePasswordHasher hasher = GetHasher();
			TestHashingAndVerifying_WithString(hasher, PasswordHashRegex, iterations, password);
		}

		#endregion

		#region Hashing and Verifying (with Span)

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Hash(string)"/> and the <see cref="SecurePasswordHasher.Verify(string,string)"/> method.
		/// The hasher should use the default number of iterations when hashing.
		/// </summary>
		/// <param name="password">Password to hash and verify.</param>
		[Theory]
		[MemberData(nameof(HashWithDefaultIterationCountAndVerify_TestData))]
		public void HashWithDefaultIterationCountAndVerify_WithSpan(string password)
		{
			SecurePasswordHasher hasher = GetHasher();
			TestHashingAndVerifying_WithSpan(hasher, PasswordHashRegex, -1, password.AsSpan());
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher.Hash(string,int)"/> and the <see cref="SecurePasswordHasher.Verify(string,string)"/> method.
		/// The hasher should use the specified number of iterations when hashing.
		/// </summary>
		/// <param name="password">Password to hash and verify.</param>
		/// <param name="iterations">Number of iterations to apply when hashing.</param>
		[Theory]
		[MemberData(nameof(HashWithSpecificIterationCountAndVerify_TestData))]
		public void HashWithSpecificIterationCountAndVerify_WithSpan(string password, int iterations)
		{
			SecurePasswordHasher hasher = GetHasher();
			TestHashingAndVerifying_WithSpan(hasher, PasswordHashRegex, iterations, password.AsSpan());
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Checks whether the specified hasher can hash and verify the specified password using the specified number of iterations.
		/// </summary>
		/// <param name="hasher">The <see cref="SecurePasswordHasher"/> to use.</param>
		/// <param name="regex">Regular expression that matches the password hash.</param>
		/// <param name="iterations">Number of iterations to apply when hashing.</param>
		/// <param name="password">Password to hash.</param>
		private static void TestHashingAndVerifying_WithString(
			SecurePasswordHasher hasher,
			Regex                regex,
			int                  iterations,
			string               password)
		{
			// hash the password using the specified hasher
			string passwordHash;
			if (iterations < 0)
			{
				passwordHash = hasher.Hash(password);
				iterations = DefaultIterationCount;
			}
			else
			{
				passwordHash = hasher.Hash(password, iterations);
			}

			// check whether the password hash looks as expected
			Match match = regex.Match(passwordHash);
			Assert.True(match.Success);
			int parsedIterations = int.Parse(match.Groups["iterations"].Value);
			string parsedBase64Hash = match.Groups["hash"].Value;
			Assert.Equal(iterations, parsedIterations);

			// verify the password
			bool verified = hasher.Verify(password, passwordHash);
			Assert.True(verified);

			// use a different password and check whether verification fails
			verified = hasher.Verify("WRONG!", passwordHash);
			Assert.False(verified);
		}

		/// <summary>
		/// Checks whether the specified hasher can hash and verify the specified password using the specified number of iterations.
		/// </summary>
		/// <param name="hasher">The <see cref="SecurePasswordHasher"/> to use.</param>
		/// <param name="regex">Regular expression that matches the password hash.</param>
		/// <param name="iterations">Number of iterations to apply when hashing.</param>
		/// <param name="password">Password to hash.</param>
		private static void TestHashingAndVerifying_WithSpan(
			SecurePasswordHasher hasher,
			Regex                regex,
			int                  iterations,
			ReadOnlySpan<char>   password)
		{
			// hash the password using the specified hasher
			string passwordHash;
			if (iterations < 0)
			{
				passwordHash = hasher.Hash(password);
				iterations = DefaultIterationCount;
			}
			else
			{
				passwordHash = hasher.Hash(password, iterations);
			}

			// check whether the password hash looks as expected
			Match match = regex.Match(passwordHash);
			Assert.True(match.Success);
			int parsedIterations = int.Parse(match.Groups["iterations"].Value);
			string parsedBase64Hash = match.Groups["hash"].Value;
			Assert.Equal(iterations, parsedIterations);

			// verify the password
			bool verified = hasher.Verify(password, passwordHash.AsSpan());
			Assert.True(verified);

			// use a different password and check whether verification fails
			verified = hasher.Verify("WRONG!", passwordHash);
			Assert.False(verified);
		}

		#endregion
	}

}
