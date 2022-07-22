﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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
		/// The short name of the algorithm (used as identifier in the hash).
		/// </summary>
		private const string AlgorithmNameDefinition = "SHA384";

		/// <summary>
		/// Regular expression that matches a hash as expected by the hasher.
		/// </summary>
		private static readonly Regex sHashRegex = new Regex(
			$@"^\${Regex.Escape(AlgorithmNameDefinition)}\$(?<iterations>\d+)\$(?<hash>[a-zA-Z0-9+/=]+)",
			RegexOptions.Compiled);

		/// <summary>
		/// Initializes a new instance of the <see cref="SecurePasswordHasher_SHA384"/> class.
		/// </summary>
		public SecurePasswordHasher_SHA384() { }

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
		/// otherwise <c>false</c>
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
			if (password == null) throw new ArgumentNullException(nameof(password));
			if (iterations < 1) throw new ArgumentException("Iterations must be greater than zero.", nameof(iterations));

			// create salt
#if NET6_0
			byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
#elif NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NETCOREAPP3_1 || NET5_0
			byte[] salt;
			using (var random = RandomNumberGenerator.Create())
				random.GetBytes(salt = new byte[SaltSize]);
#else
#error Unhandled target framework.
#endif

			// create hash
			byte[] hash;
			using (var sha384 = System.Security.Cryptography.SHA384.Create())
			{
				sha384.TransformBlock(salt, 0, salt.Length, salt, 0);
				var encodedPassword = Encoding.UTF8.GetBytes(password);
				for (int i = 0; i < iterations; i++) sha384.TransformBlock(encodedPassword, 0, encodedPassword.Length, encodedPassword, 0);
				sha384.TransformFinalBlock(encodedPassword, 0, 0);
				hash = sha384.Hash;
			}

			// combine salt and hash
			var hashBytes = new byte[SaltSize + HashSize];
			Array.Copy(salt, 0, hashBytes, 0, SaltSize);
			Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

			// convert to base64
			var base64Hash = Convert.ToBase64String(hashBytes);

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
			var match = sHashRegex.Match(hashedPassword);
			if (!match.Success) throw new NotSupportedException("The hash type is not supported.");
			var iterations = int.Parse(match.Groups["iterations"].Value);
			var base64Hash = match.Groups["hash"].Value;

			// get hash bytes
			var hashBytes = Convert.FromBase64String(base64Hash);
			if (hashBytes.Length != SaltSize + HashSize)
				throw new FormatException($"The specified hash is not a valid salted SHA384 hash. It must be {SaltSize + HashSize} bytes long.");

			// get salt
			var salt = new byte[SaltSize];
			Array.Copy(hashBytes, 0, salt, 0, SaltSize);

			// create hash
			byte[] hash;
			using (var sha384 = System.Security.Cryptography.SHA384.Create())
			{
				sha384.TransformBlock(salt, 0, salt.Length, salt, 0);
				var encodedPassword = Encoding.UTF8.GetBytes(password);
				for (int i = 0; i < iterations; i++) sha384.TransformBlock(encodedPassword, 0, encodedPassword.Length, encodedPassword, 0);
				sha384.TransformFinalBlock(encodedPassword, 0, 0);
				hash = sha384.Hash;
			}

			// get result
			for (var i = 0; i < HashSize; i++)
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
