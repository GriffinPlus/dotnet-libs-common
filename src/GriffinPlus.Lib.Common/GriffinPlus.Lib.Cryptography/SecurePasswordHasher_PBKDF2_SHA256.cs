///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#if NETSTANDARD2_1 || NET48 || NET5_0 || NET6_0 || NET7_0 || NET8_0

using System;
using System.Security.Cryptography;

// ReSharper disable InconsistentNaming

namespace GriffinPlus.Lib.Cryptography
{

	/// <summary>
	/// An implementation of the <see cref="SecurePasswordHasher"/> according to the password-based key
	/// derivation function 2 (PBKDF2) described in RFC8018 (obsoletes RFC2898).
	/// The hash algorithm used to derive the key is SHA-256 (32 bytes hash).
	/// It uses 16 bytes salt when hashing.
	/// </summary>
	sealed class SecurePasswordHasher_PBKDF2_SHA256 : SecurePasswordHasher
	{
		/// <summary>
		/// Size of the salt (in bytes).
		/// </summary>
		private const int SaltSize = 16;

		/// <summary>
		/// Size of the hash (in bytes).
		/// </summary>
		private const int HashSize = 32;

		/// <summary>
		/// Initializes a new instance of the <see cref="SecurePasswordHasher_PBKDF2_SHA256"/> class.
		/// </summary>
		public SecurePasswordHasher_PBKDF2_SHA256() { }

		/// <inheritdoc/>
		public override string AlgorithmName => "PBKDF2-SHA256";

		/// <inheritdoc/>
		public override string Hash(string password, int iterations)
		{
			return Hash(
				HashAlgorithmName.SHA256,
				AlgorithmName,
				SaltSize,
				HashSize,
				password,
				iterations);
		}

		/// <inheritdoc/>
		public override string Hash(ReadOnlySpan<char> password, int iterations)
		{
			return Hash(
				HashAlgorithmName.SHA256,
				AlgorithmName,
				SaltSize,
				HashSize,
				password,
				iterations);
		}

		/// <inheritdoc/>
		public override bool Verify(string password, string passwordHash)
		{
			return Verify(
				HashAlgorithmName.SHA256,
				AlgorithmName,
				SaltSize,
				HashSize,
				password,
				passwordHash);
		}

		/// <inheritdoc/>
		public override bool Verify(ReadOnlySpan<char> password, ReadOnlySpan<char> passwordHash)
		{
			return Verify(
				HashAlgorithmName.SHA256,
				AlgorithmName.AsSpan(),
				SaltSize,
				HashSize,
				password,
				passwordHash);
		}
	}

}
#elif NETSTANDARD2_0 || NET461
// The Rfc2898DeriveBytes class supports SHA-1 only on .NET Standard 2.0 and .NET Framework 4.6.1.
#else
#error Unhandled target framework.
#endif
