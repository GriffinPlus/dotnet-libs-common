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
		/// A password hasher using the password-based key derivation function 2 (PBKDF2) described in RFC2898 (recommended!).
		/// </summary>
		public static readonly SecurePasswordHasher RFC2898_PBKDF2 = new SecurePasswordHasher_RFC2898_PBKDF2();

		/// <summary>
		/// A password hasher using the SHA-1 hash function (16 bytes salt + 20 bytes hash).
		/// </summary>
		public static readonly SecurePasswordHasher SHA1 = new SecurePasswordHasher_SHA1();

		/// <summary>
		/// A password hasher using the SHA-256 hash function (16 bytes salt + 32 bytes hash).
		/// </summary>
		public static readonly SecurePasswordHasher SHA256 = new SecurePasswordHasher_SHA256();

		/// <summary>
		/// A password hasher using the SHA-384 hash function (16 bytes salt + 48 bytes hash).
		/// </summary>
		public static readonly SecurePasswordHasher SHA384 = new SecurePasswordHasher_SHA384();

		/// <summary>
		/// A password hasher using the SHA-512 hash function (16 bytes salt + 64 bytes hash).
		/// </summary>
		public static readonly SecurePasswordHasher SHA512 = new SecurePasswordHasher_SHA512();

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
				{ RFC2898_PBKDF2.AlgorithmName.ToLower(), RFC2898_PBKDF2 },
				{ SHA1.AlgorithmName.ToLower(), SHA1 },
				{ SHA256.AlgorithmName.ToLower(), SHA256 },
				{ SHA384.AlgorithmName.ToLower(), SHA384 },
				{ SHA512.AlgorithmName.ToLower(), SHA512 }
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
