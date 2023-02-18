///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#if NETCOREAPP3_1 || NET48 || NET5_0 || NET6_0 || NET7_0
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Xunit;

// ReSharper disable StringLiteralTypo

#pragma warning disable CS0618 // Type or member is obsolete

namespace GriffinPlus.Lib.Cryptography
{

	/// <summary>
	/// Tests targeting the <see cref="SecurePasswordHasher_PBKDF2_SHA1"/> class.
	/// </summary>
	public class SecurePasswordHasherTests_PBKDF2_SHA1 : SecurePasswordHasherTests_Base
	{
		/// <summary>
		/// Gets the <see cref="SecurePasswordHasher_PBKDF2_SHA1"/> instance to test.
		/// </summary>
		/// <returns></returns>
		protected override SecurePasswordHasher GetHasher() => SecurePasswordHasher.PBKDF2_SHA1;

		/// <summary>
		/// Size of the salt (in bytes).
		/// </summary>
		protected override int SaltSize => 16;

		/// <summary>
		/// Size of the hash (in bytes).
		/// </summary>
		protected override int HashSize => 20;

		/// <summary>
		/// Gets a regular expression matching a password hash emitted by the tested hasher.
		/// </summary>
		protected override Regex PasswordHashRegex { get; } = new Regex(@"^\$PBKDF2-SHA1\$(?<iterations>\d+)\$(?<hash>[a-zA-Z0-9+/=]{48})$", RegexOptions.Compiled);

		#region Test Data

		/// <summary>
		/// Test data for the <see cref="Verify_WithString_TooLessFields"/> and <see cref="Verify_WithSpan_TooLessFields"/> test methods.
		/// </summary>
		public static IEnumerable<object[]> TestData_Verify_TooLessFields
		{
			get
			{
				yield return new object[] { "" };
				yield return new object[] { "$" };
				yield return new object[] { "$PBKDF2-SHA1$" };
				yield return new object[] { "$PBKDF2-SHA1$10000" };
			}
		}

		/// <summary>
		/// Test data for the <see cref="Verify_WithString_AlgorithmNotSupported"/> and <see cref="Verify_WithSpan_AlgorithmNotSupported"/> test methods.
		/// </summary>
		public static IEnumerable<object[]> TestData_Verify_AlgorithmNotSupported
		{
			get { yield return new object[] { "$XXX$10000$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=" }; }
		}

		/// <summary>
		/// Test data for the <see cref="Verify_WithString_InvalidIterationCount"/> and <see cref="Verify_WithSpan_InvalidIterationCount"/> test methods.
		/// </summary>
		public static IEnumerable<object[]> TestData_Verify_InvalidIterationCount
		{
			get { yield return new object[] { "$PBKDF2-SHA1$x$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=" }; }
		}

		/// <summary>
		/// Test data for the <see cref="Verify_WithString_HashSizeTooShortOrToLong"/> and <see cref="Verify_WithSpan_HashSizeTooShortOrToLong"/> test methods.
		/// </summary>
		public static IEnumerable<object[]> TestData_Verify_HashSizeTooShortOrToLong
		{
			get
			{
				yield return new object[] { "$PBKDF2-SHA1$10000$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==" };
				yield return new object[] { "$PBKDF2-SHA1$10000$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" };
			}
		}

		/// <summary>
		/// Test data for the <see cref="Verify_WithString_ImproperEncodedSaltedHash"/> and <see cref="Verify_WithSpan_ImproperEncodedSaltedHash"/> test methods.
		/// </summary>
		public static IEnumerable<object[]> TestData_Verify_ImproperEncodedSaltedHash
		{
			get { yield return new object[] { "$PBKDF2-SHA1$10000$============================================================================================================" }; }
		}

		#endregion

		#region bool SecurePasswordHasher_PBKDF2_SHA1.Verify(string password, string passwordHash)

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher_PBKDF2_SHA1.Verify(string,string)"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash contains too less fields.
		/// </summary>
		[Theory]
		[MemberData(nameof(TestData_Verify_TooLessFields))]
		public override void Verify_WithString_TooLessFields(string passwordHash)
		{
			base.Verify_WithString_TooLessFields(passwordHash);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher_PBKDF2_SHA1.Verify(string,string)"/> method.
		/// The method should throw a <see cref="NotSupportedException"/> if the password hash contains an unhandled algorithm identifier.
		/// </summary>
		[Theory]
		[MemberData(nameof(TestData_Verify_AlgorithmNotSupported))]
		public override void Verify_WithString_AlgorithmNotSupported(string passwordHash)
		{
			base.Verify_WithString_AlgorithmNotSupported(passwordHash);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher_PBKDF2_SHA1.Verify(string,string)"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash contains an invalid iteration count.
		/// </summary>
		[Theory]
		[MemberData(nameof(TestData_Verify_InvalidIterationCount))]
		public override void Verify_WithString_InvalidIterationCount(string passwordHash)
		{
			base.Verify_WithString_InvalidIterationCount(passwordHash);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher_PBKDF2_SHA1.Verify(string,string)"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash is too short or too long.
		/// </summary>
		[Theory]
		[MemberData(nameof(TestData_Verify_HashSizeTooShortOrToLong))]
		public override void Verify_WithString_HashSizeTooShortOrToLong(string passwordHash)
		{
			base.Verify_WithString_HashSizeTooShortOrToLong(passwordHash);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher_PBKDF2_SHA1.Verify(string,string)"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash contains an improper BASE64 formatted string.
		/// </summary>
		[Theory]
		[MemberData(nameof(TestData_Verify_ImproperEncodedSaltedHash))]
		public override void Verify_WithString_ImproperEncodedSaltedHash(string passwordHash)
		{
			base.Verify_WithString_ImproperEncodedSaltedHash(passwordHash);
		}

		#endregion

		#region bool SecurePasswordHasher_PBKDF2_SHA1.Verify(ReadOnlySpan<char> password, ReadOnlySpan<char> passwordHash)

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher_PBKDF2_SHA1.Verify(ReadOnlySpan{char},ReadOnlySpan{char})"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash contains too less fields.
		/// </summary>
		[Theory]
		[MemberData(nameof(TestData_Verify_TooLessFields))]
		public override void Verify_WithSpan_TooLessFields(string passwordHash)
		{
			base.Verify_WithSpan_TooLessFields(passwordHash);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher_PBKDF2_SHA1.Verify(ReadOnlySpan{char},ReadOnlySpan{char})"/> method.
		/// The method should throw a <see cref="NotSupportedException"/> if the password hash contains an unhandled algorithm identifier.
		/// </summary>
		[Theory]
		[MemberData(nameof(TestData_Verify_AlgorithmNotSupported))]
		public override void Verify_WithSpan_AlgorithmNotSupported(string passwordHash)
		{
			base.Verify_WithSpan_AlgorithmNotSupported(passwordHash);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher_PBKDF2_SHA1.Verify(ReadOnlySpan{char},ReadOnlySpan{char})"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash contains an invalid iteration count.
		/// </summary>
		[Theory]
		[MemberData(nameof(TestData_Verify_InvalidIterationCount))]
		public override void Verify_WithSpan_InvalidIterationCount(string passwordHash)
		{
			base.Verify_WithSpan_InvalidIterationCount(passwordHash);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher_PBKDF2_SHA1.Verify(ReadOnlySpan{char},ReadOnlySpan{char})"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash is too short or too long.
		/// </summary>
		[Theory]
		[MemberData(nameof(TestData_Verify_HashSizeTooShortOrToLong))]
		public override void Verify_WithSpan_HashSizeTooShortOrToLong(string passwordHash)
		{
			base.Verify_WithSpan_HashSizeTooShortOrToLong(passwordHash);
		}

		/// <summary>
		/// Tests the <see cref="SecurePasswordHasher_PBKDF2_SHA1.Verify(ReadOnlySpan{char},ReadOnlySpan{char})"/> method.
		/// The method should throw a <see cref="FormatException"/> if the password hash contains an improper BASE64 formatted string.
		/// </summary>
		[Theory]
		[MemberData(nameof(TestData_Verify_ImproperEncodedSaltedHash))]
		public override void Verify_WithSpan_ImproperEncodedSaltedHash(string passwordHash)
		{
			base.Verify_WithSpan_ImproperEncodedSaltedHash(passwordHash);
		}

		#endregion
	}

}

#pragma warning restore CS0618 // Type or member is obsolete

#elif NET462
// The Rfc2898DeriveBytes class supports SHA-1 only on .NET Standard 2.0.
#else
#error Unhandled target framework.
#endif
