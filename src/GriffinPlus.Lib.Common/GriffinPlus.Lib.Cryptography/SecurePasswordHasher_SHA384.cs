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
	/// cryptographic hash function SHA-384 (16 bytes salt + 48 bytes hash).
	/// </summary>
	sealed class SecurePasswordHasher_SHA384 : SecurePasswordHasher
	{
		/// <summary>
		/// Size of the salt (in bytes).
		/// </summary>
		private const int SaltSize = 16;

		/// <summary>
		/// Size of the hash (in bytes).
		/// </summary>
		private const int HashSize = 48;

		/// <summary>
		/// Initializes a new instance of the <see cref="SecurePasswordHasher_SHA384"/> class.
		/// </summary>
		public SecurePasswordHasher_SHA384() { }

		/// <inheritdoc />
		public override string AlgorithmName => "SHA384";

		/// <inheritdoc />
		public override string Hash(string password, int iterations)
		{
			using (var sha384 = System.Security.Cryptography.SHA384.Create())
			{
				return Hash(
					sha384,
					AlgorithmName,
					SaltSize,
					HashSize,
					password,
					iterations);
			}
		}

		/// <inheritdoc />
		public override string Hash(ReadOnlySpan<char> password, int iterations)
		{
			using (var sha384 = System.Security.Cryptography.SHA384.Create())
			{
				return Hash(
					sha384,
					AlgorithmName,
					SaltSize,
					HashSize,
					password,
					iterations);
			}
		}

		/// <inheritdoc />
		public override bool Verify(string password, string passwordHash)
		{
			using (var sha384 = System.Security.Cryptography.SHA384.Create())
			{
				return Verify(
					sha384,
					AlgorithmName,
					SaltSize,
					HashSize,
					password,
					passwordHash);
			}
		}

		/// <inheritdoc />
		public override bool Verify(ReadOnlySpan<char> password, ReadOnlySpan<char> passwordHash)
		{
			using (var sha384 = System.Security.Cryptography.SHA384.Create())
			{
				return Verify(
					sha384,
					AlgorithmName.AsSpan(),
					SaltSize,
					HashSize,
					password,
					passwordHash);
			}
		}
	}

}
