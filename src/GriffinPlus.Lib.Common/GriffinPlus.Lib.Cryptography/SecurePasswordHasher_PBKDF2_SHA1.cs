///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Security.Cryptography;

// ReSharper disable InconsistentNaming

namespace GriffinPlus.Lib.Cryptography;

/// <summary>
/// An implementation of the <see cref="SecurePasswordHasher"/> according to the password-based key
/// derivation function 2 (PBKDF2) described in RFC8018 (obsoletes RFC2898, 16 bytes salt + 20 bytes hash).
/// The hash algorithm used to derive the key is SHA-1 which is considered too weak meanwhile.
/// Applications should use stronger hashing functions like SHA-256 or SHA-512 as key derivation functions now.
/// </summary>
sealed class SecurePasswordHasher_PBKDF2_SHA1 : SecurePasswordHasher
{
	/// <summary>
	/// Size of the salt (in bytes).
	/// </summary>
	private const int SaltSize = 16;

	/// <summary>
	/// Size of the hash (in bytes).
	/// </summary>
	private const int HashSize = 20;

	/// <summary>
	/// Initializes a new instance of the <see cref="SecurePasswordHasher_PBKDF2_SHA1"/> class.
	/// </summary>
	public SecurePasswordHasher_PBKDF2_SHA1() { }

	/// <inheritdoc/>
	public override string AlgorithmName => "PBKDF2-SHA1";

	/// <inheritdoc/>
	public override string Hash(string password, int iterations)
	{
		return Hash(
			HashAlgorithmName.SHA1,
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
			HashAlgorithmName.SHA1,
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
			HashAlgorithmName.SHA1,
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
			HashAlgorithmName.SHA1,
			AlgorithmName.AsSpan(),
			SaltSize,
			HashSize,
			password,
			passwordHash);
	}
}
