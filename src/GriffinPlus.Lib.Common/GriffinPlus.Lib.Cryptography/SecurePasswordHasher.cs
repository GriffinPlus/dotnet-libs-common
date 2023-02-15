///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable InconsistentNaming

namespace GriffinPlus.Lib.Cryptography
{

	/// <summary>
	/// A class assisting with hashing and verifying passwords.
	/// </summary>
	public abstract class SecurePasswordHasher
	{
		/// <summary>
		/// A password hasher using the password-based key derivation function 2 (PBKDF2) described in
		/// RFC8018 (obsoletes RFC2898) with SHA-1 as key derivation function (deprecated).
		/// </summary>
		[Obsolete(
			"This hasher uses PBKDF2 with SHA-1 as key derivation function, which is considered too short meanwhile. " +
			"Please use SecurePasswordHasher.PBKDF2_SHA256 or SecurePasswordHasher.PBKDF2_SHA512 instead.")]
		public static SecurePasswordHasher PBKDF2_SHA1 { get; } = new SecurePasswordHasher_PBKDF2_SHA1();

#if NETSTANDARD2_1 || NETCOREAPP3_1 || NET48 || NET5_0 || NET6_0 || NET7_0

		/// <summary>
		/// A password hasher using the password-based key derivation function 2 (PBKDF2) described in
		/// RFC8018 (obsoletes RFC2898) with SHA-256 as key derivation function.
		/// </summary>
		public static SecurePasswordHasher PBKDF2_SHA256 { get; } = new SecurePasswordHasher_PBKDF2_SHA256();

		/// <summary>
		/// A password hasher using the password-based key derivation function 2 (PBKDF2) described in
		/// RFC8018 (obsoletes RFC2898) with SHA-512 as key derivation function.
		/// </summary>
		public static SecurePasswordHasher PBKDF2_SHA512 { get; } = new SecurePasswordHasher_PBKDF2_SHA512();

#elif NETSTANDARD2_0
// The Rfc2898DeriveBytes class supports SHA-1 only on .NET Standard 2.0.
#else
#error Unhandled target framework.
#endif

		/// <summary>
		/// A password hasher using the SHA-1 hash function (16 bytes salt + 20 bytes hash).
		/// </summary>
		public static SecurePasswordHasher SHA1 { get; } = new SecurePasswordHasher_SHA1();

		/// <summary>
		/// A password hasher using the SHA-256 hash function (16 bytes salt + 32 bytes hash).
		/// </summary>
		public static SecurePasswordHasher SHA256 { get; } = new SecurePasswordHasher_SHA256();

		/// <summary>
		/// A password hasher using the SHA-384 hash function (16 bytes salt + 48 bytes hash).
		/// </summary>
		public static SecurePasswordHasher SHA384 { get; } = new SecurePasswordHasher_SHA384();

		/// <summary>
		/// A password hasher using the SHA-512 hash function (16 bytes salt + 64 bytes hash).
		/// </summary>
		public static SecurePasswordHasher SHA512 { get; } = new SecurePasswordHasher_SHA512();

		/// <summary>
		/// Registered hashers by algorithm.
		/// </summary>
		private static readonly Dictionary<string, SecurePasswordHasher> sHashers;

		/// <summary>
		/// Regex matching a hash string.
		/// </summary>
		private static readonly Regex sCommonHashRegex = new Regex(@"^\$(?<algorithm>[a-zA-Z0-9_\-+#]+)\$.+", RegexOptions.Compiled);

		/// <summary>
		/// Initializes the <see cref="SecurePasswordHasher"/> class.
		/// </summary>
		static SecurePasswordHasher()
		{
			sHashers = new Dictionary<string, SecurePasswordHasher>
			{
				// PBKDF2 with SHA-1
#pragma warning disable CS0618
				{ SecurePasswordHasher_PBKDF2_SHA1.AlgorithmNameDefinition.ToLower(), PBKDF2_SHA1 },
				{ SecurePasswordHasher_PBKDF2_SHA1.AlternativeAlgorithmNameDefinition.ToLower(), PBKDF2_SHA1 },
#pragma warning restore CS0618

				// PBKDF2 with SHA-256 and SHA-512
#if NETSTANDARD2_1 || NETCOREAPP3_1 || NET48 || NET5_0 || NET6_0 || NET7_0
				{ SecurePasswordHasher_PBKDF2_SHA256.AlgorithmNameDefinition.ToLower(), PBKDF2_SHA256 },
				{ SecurePasswordHasher_PBKDF2_SHA512.AlgorithmNameDefinition.ToLower(), PBKDF2_SHA512 },
#elif NETSTANDARD2_0
// The Rfc2898DeriveBytes class supports SHA-1 only on .NET Standard 2.0.
#else
#error Unhandled target framework.
#endif
				// SHA1, SHA-256, SHA-384 and SHA-512
				{ SecurePasswordHasher_SHA1.AlgorithmNameDefinition.ToLower(), SHA1 },
				{ SecurePasswordHasher_SHA256.AlgorithmNameDefinition.ToLower(), SHA256 },
				{ SecurePasswordHasher_SHA384.AlgorithmNameDefinition.ToLower(), SHA384 },
				{ SecurePasswordHasher_SHA512.AlgorithmNameDefinition.ToLower(), SHA512 }
			};
		}

		/// <summary>
		/// Gets the short name of the hashing algorithm (becomes part of the generated hash string).
		/// </summary>
		public abstract string AlgorithmName { get; }

		/// <summary>
		/// Checks whether the current implementation can handle the hash.
		/// </summary>
		/// <param name="hash">The hash to check.</param>
		/// <returns>
		/// <c>true</c> if the hash algorithm is supported;
		/// otherwise <c>false</c>.
		/// </returns>
		public abstract bool CanHandle(string hash);

		/// <summary>
		/// Creates a hash from a password with the specified number of iterations.
		/// </summary>
		/// <param name="password">The password to hash.</param>
		/// <param name="iterations">Number of iterations.</param>
		/// <returns>The hash.</returns>
		public abstract string Hash(string password, int iterations);

		/// <summary>
		/// Creates a hash from a password with 10000 iterations.
		/// </summary>
		/// <param name="password">The password to hash.</param>
		/// <returns>The hash.</returns>
		public string Hash(string password)
		{
			return Hash(password, 10000);
		}

		/// <summary>
		/// Verifies the specified password against the specified hash.
		/// </summary>
		/// <param name="password">The password to check.</param>
		/// <param name="hashedPassword">The hashed password to check against.</param>
		/// <returns>
		/// <c>true</c> if the password was successfully verified against the hash;
		/// <c>false</c> if the verification failed.
		/// </returns>
		public abstract bool Verify(string password, string hashedPassword);

		/// <summary>
		/// Verifies the specified password against the specified hash using all registered hashers.
		/// </summary>
		/// <param name="password">The password to check.</param>
		/// <param name="hashedPassword">The hashed password to check against.</param>
		/// <returns>
		/// <c>true</c> if the password was successfully verified against the hash;
		/// <c>false</c> if the verification failed.
		/// </returns>
		/// <exception cref="NotSupportedException">The hash type is not supported.</exception>
		public static bool VerifyPassword(string password, string hashedPassword)
		{
			Match match = sCommonHashRegex.Match(hashedPassword);
			if (!match.Success) throw new NotSupportedException("The hash type is not supported.");
			string hashName = match.Groups["algorithm"].Value.ToLower();
			if (!sHashers.TryGetValue(hashName, out SecurePasswordHasher hasher)) throw new NotSupportedException("The hash type is not supported.");
			return hasher.Verify(password, hashedPassword);
		}
	}

}
