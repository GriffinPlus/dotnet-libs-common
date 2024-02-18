///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

// ReSharper disable InconsistentNaming

namespace GriffinPlus.Lib.Cryptography
{

	/// <summary>
	/// An implementation of the <see cref="SecurePasswordHasher"/> according to the
	/// cryptographic hash function SHA-256 (16 bytes salt + 32 bytes hash).
	/// </summary>
	sealed class SecurePasswordHasher_SHA256 : SecurePasswordHasher
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
		/// Initializes a new instance of the <see cref="SecurePasswordHasher_SHA256"/> class.
		/// </summary>
		public SecurePasswordHasher_SHA256() { }

		/// <inheritdoc/>
		public override string AlgorithmName => "SHA256";

		/// <inheritdoc/>
		public override string Hash(string password, int iterations)
		{
			using var sha256 = System.Security.Cryptography.SHA256.Create();
			return Hash(
				sha256,
				AlgorithmName,
				SaltSize,
				HashSize,
				password,
				iterations);
		}

		/// <inheritdoc/>
		public override string Hash(ReadOnlySpan<char> password, int iterations)
		{
			using var sha256 = System.Security.Cryptography.SHA256.Create();
			return Hash(
				sha256,
				AlgorithmName,
				SaltSize,
				HashSize,
				password,
				iterations);
		}

		/// <inheritdoc/>
		public override bool Verify(string password, string passwordHash)
		{
			using var sha256 = System.Security.Cryptography.SHA256.Create();
			return Verify(
				sha256,
				AlgorithmName,
				SaltSize,
				HashSize,
				password,
				passwordHash);
		}

		/// <inheritdoc/>
		public override bool Verify(ReadOnlySpan<char> password, ReadOnlySpan<char> passwordHash)
		{
			using var sha256 = System.Security.Cryptography.SHA256.Create();
			return Verify(
				sha256,
				AlgorithmName.AsSpan(),
				SaltSize,
				HashSize,
				password,
				passwordHash);
		}
	}

}
