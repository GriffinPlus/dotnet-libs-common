///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

// ReSharper disable InconsistentNaming

namespace GriffinPlus.Lib.Cryptography
{

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
		/// The short name of the algorithm (used as identifier in the hash).
		/// </summary>
		internal const string AlgorithmNameDefinition = "PBKDF2-SHA1";

		/// <summary>
		/// The short name of the algorithm (used as identifier in the hash).
		/// For backwards compatibility.
		/// </summary>
		internal const string AlternativeAlgorithmNameDefinition = "PBKDF2";

		/// <summary>
		/// Regular expression that matches a hash as expected by the PBKDF2 hasher.
		/// </summary>
		private static readonly Regex sHashRegex = new Regex(
			$@"^\$(?:{Regex.Escape(AlgorithmNameDefinition)})|(?:{Regex.Escape(AlternativeAlgorithmNameDefinition)})\$(?<iterations>\d+)\$(?<hash>[a-zA-Z0-9+/=]+)",
			RegexOptions.Compiled);

		/// <summary>
		/// Initializes a new instance of the <see cref="SecurePasswordHasher_PBKDF2_SHA1"/> class.
		/// </summary>
		public SecurePasswordHasher_PBKDF2_SHA1() { }

		/// <summary>
		/// Gets the short name of the hashing algorithm (becomes part of the generated hash string).
		/// </summary>
		public override string AlgorithmName => AlgorithmNameDefinition;

		/// <summary>
		/// Checks whether the current implementation can handle the hash.
		/// </summary>
		/// <param name="hash">The hash to check.</param>
		/// <returns>
		/// <c>true</c> if the hash algorithm is supported;
		/// otherwise <c>false</c>.
		/// </returns>
		public override bool CanHandle(string hash)
		{
			return sHashRegex.IsMatch(hash);
		}

		/// <summary>
		/// Creates a hash from a password.
		/// </summary>
		/// <param name="password">The password to hash.</param>
		/// <param name="iterations">Number of iterations.</param>
		/// <returns>The hash.</returns>
		public override string Hash(string password, int iterations)
		{
			// create salt
#if NETSTANDARD2_1 || NETCOREAPP3_1 || NET5_0 || NET6_0 || NET7_0
			byte[] salt = new byte[SaltSize];
			RandomNumberGenerator.Fill(salt);
#elif NETSTANDARD2_0 || NET48
			byte[] salt;
			using (var random = RandomNumberGenerator.Create())
			{
				random.GetBytes(salt = new byte[SaltSize]);
			}
#else
#error Unhandled target framework.
#endif

			// create hash
			byte[] hash;
#if NETSTANDARD2_0
			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET48 || NET5_0 || NET6_0 || NET7_0
			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA1))
#else
#error Unhandled target framework.
#endif
			{
				hash = pbkdf2.GetBytes(HashSize);
			}

			// combine salt and hash
			byte[] hashBytes = new byte[SaltSize + HashSize];
			Array.Copy(salt, 0, hashBytes, 0, SaltSize);
			Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

			// convert to base64
			string base64Hash = Convert.ToBase64String(hashBytes);

			// format hash with extra information
			return $"${AlgorithmNameDefinition}${iterations}${base64Hash}";
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
		/// <exception cref="NotSupportedException">The hash type is not supported.</exception>
		public override bool Verify(string password, string hashedPassword)
		{
			// extract fields from hashed password
			Match match = sHashRegex.Match(hashedPassword);
			if (!match.Success) throw new NotSupportedException("The hash type is not supported.");
			int iterations = int.Parse(match.Groups["iterations"].Value);
			string base64Hash = match.Groups["hash"].Value;

			// get hash bytes
			byte[] hashBytes = Convert.FromBase64String(base64Hash);

			// get salt
			byte[] salt = new byte[SaltSize];
			Array.Copy(hashBytes, 0, salt, 0, SaltSize);

			// create hash with given salt
			byte[] hash;
#if NETSTANDARD2_0
			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET48 || NET5_0 || NET6_0 || NET7_0
			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA1))
#else
#error Unhandled target framework.
#endif
			{
				hash = pbkdf2.GetBytes(HashSize);
			}

			// get result
			for (int i = 0; i < HashSize; i++)
			{
				if (hashBytes[i + SaltSize] != hash[i])
				{
					return false; // verification failed
				}
			}

			// verification succeeded
			return true;
		}
	}

}
