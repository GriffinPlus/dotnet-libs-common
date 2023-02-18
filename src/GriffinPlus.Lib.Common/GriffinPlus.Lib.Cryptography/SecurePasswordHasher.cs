///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace GriffinPlus.Lib.Cryptography
{

	/// <summary>
	/// A class assisting with hashing and verifying passwords.
	/// </summary>
	public abstract class SecurePasswordHasher
	{
		/// <summary>
		/// A password hasher using the password-based key derivation function 2 (PBKDF2) described in
		/// RFC8018 (obsoletes RFC2898) with SHA-1 as key derivation function (16 bytes salt + 20 bytes hash).
		/// This hasher works the same as <see cref="PBKDF2_SHA1"/>, but has a different algorithm name ('PBKDF2'
		/// instead of 'PBKDF2-SHA1'). The hasher is needed for backwards compatibility only, i.e. for verifying
		/// password hashes created with an older version of the <see cref="SecurePasswordHasher"/> class.
		/// </summary>
		[Obsolete(
			"This hasher uses PBKDF2 with SHA-1 as key derivation function, which is considered too weak meanwhile. " +
			"Please use SecurePasswordHasher.PBKDF2_SHA256 or SecurePasswordHasher.PBKDF2_SHA512 instead.")]
		public static SecurePasswordHasher PBKDF2_Legacy { get; } = new SecurePasswordHasher_PBKDF2_Legacy();

		/// <summary>
		/// A password hasher using the password-based key derivation function 2 (PBKDF2) described in
		/// RFC8018 (obsoletes RFC2898) with SHA-1 as key derivation function (16 bytes salt + 20 bytes hash).
		/// </summary>
		[Obsolete(
			"This hasher uses PBKDF2 with SHA-1 as key derivation function, which is considered too weak meanwhile. " +
			"Please use SecurePasswordHasher.PBKDF2_SHA256 or SecurePasswordHasher.PBKDF2_SHA512 instead.")]
		public static SecurePasswordHasher PBKDF2_SHA1 { get; } = new SecurePasswordHasher_PBKDF2_SHA1();

#if NETSTANDARD2_1 || NETCOREAPP3_1 || NET48 || NET5_0 || NET6_0 || NET7_0
		/// <summary>
		/// A password hasher using the password-based key derivation function 2 (PBKDF2) described in
		/// RFC8018 (obsoletes RFC2898) with SHA-256 as key derivation function (16 bytes salt + 32 bytes hash).
		/// </summary>
		public static SecurePasswordHasher PBKDF2_SHA256 { get; } = new SecurePasswordHasher_PBKDF2_SHA256();

		/// <summary>
		/// A password hasher using the password-based key derivation function 2 (PBKDF2) described in
		/// RFC8018 (obsoletes RFC2898) with SHA-512 as key derivation function (16 bytes salt + 64 bytes hash).
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
		/// The default number of iterations to use when no iteration count is specified.
		/// </summary>
		private const int DefaultIterationCount = 10000;

		/// <summary>
		/// Initializes the <see cref="SecurePasswordHasher"/> class.
		/// </summary>
		static SecurePasswordHasher()
		{
			sHashers = new Dictionary<string, SecurePasswordHasher>
			{
#pragma warning disable CS0618
				// PBKDF2 with SHA-1
				{ PBKDF2_Legacy.AlgorithmName.ToLower(), PBKDF2_Legacy },
				{ PBKDF2_SHA1.AlgorithmName.ToLower(), PBKDF2_SHA1 },
#pragma warning restore CS0618

#if NETSTANDARD2_1 || NETCOREAPP3_1 || NET48 || NET5_0 || NET6_0 || NET7_0
				// PBKDF2 with SHA-256 and SHA-512
				{ PBKDF2_SHA256.AlgorithmName.ToLower(), PBKDF2_SHA256 },
				{ PBKDF2_SHA512.AlgorithmName.ToLower(), PBKDF2_SHA512 },
#elif NETSTANDARD2_0
				// The Rfc2898DeriveBytes class supports SHA-1 only on .NET Standard 2.0.
#else
#error Unhandled target framework.
#endif
				// SHA-1, SHA-256, SHA-384 and SHA-512
				{ SHA1.AlgorithmName.ToLower(), SHA1 },
				{ SHA256.AlgorithmName.ToLower(), SHA256 },
				{ SHA384.AlgorithmName.ToLower(), SHA384 },
				{ SHA512.AlgorithmName.ToLower(), SHA512 }
			};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SecurePasswordHasher"/> class.
		/// </summary>
		protected SecurePasswordHasher() { }

		/// <summary>
		/// Gets the short name of the hashing algorithm (becomes part of generated password hashes).
		/// </summary>
		public abstract string AlgorithmName { get; }

		/// <summary>
		/// Hashes the specified password with 10000 iterations.
		/// </summary>
		/// <param name="password">The password to hash.</param>
		/// <returns>The password hash.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
		public virtual string Hash(string password) => Hash(password, DefaultIterationCount);

		/// <summary>
		/// Hashes the specified password with the specified number of iterations.
		/// </summary>
		/// <param name="password">The password to hash.</param>
		/// <returns>The password hash.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
		public virtual string Hash(ReadOnlySpan<char> password) => Hash(password, DefaultIterationCount);

		/// <summary>
		/// Hashes the specified password with the specified number of iterations.
		/// </summary>
		/// <param name="password">The password to hash.</param>
		/// <param name="iterations">Number of iterations.</param>
		/// <returns>The password hash.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="iterations"/> is less than 1.</exception>
		public abstract string Hash(string password, int iterations);

		/// <summary>
		/// Hashes the specified password with the specified number of iterations.
		/// </summary>
		/// <param name="password">The password to hash.</param>
		/// <param name="iterations">Number of iterations.</param>
		/// <returns>The password hash.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="iterations"/> is less than 1.</exception>
		public abstract string Hash(ReadOnlySpan<char> password, int iterations);

		/// <summary>
		/// Verifies the specified password against the specified password hash.
		/// </summary>
		/// <param name="password">The password to check.</param>
		/// <param name="passwordHash">The password hash to check against.</param>
		/// <returns>
		/// <c>true</c> if the password was successfully verified against the password hash;
		/// otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="FormatException">
		/// <paramref name="passwordHash"/> does not have the expected format ($&lt;algorithm&gt;>$&lt;iterations&gt;$&lt;salted-hash&gt;).<br/>
		/// -or-<br/>
		/// The iteration count specified in the <paramref name="passwordHash"/> is not a properly formatted integer value.<br/>
		/// -or-<br/>
		/// The specified salted hash in <paramref name="passwordHash"/> does not have the expected length.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The algorithm specified in the <paramref name="passwordHash"/> does not match the expected algorithm identifier.
		/// </exception>
		public abstract bool Verify(string password, string passwordHash);

		/// <summary>
		/// Verifies the specified password against the specified password hash.
		/// </summary>
		/// <param name="password">The  password to check.</param>
		/// <param name="passwordHash">The password hash to check against.</param>
		/// <returns>
		/// <c>true</c> if the password was successfully verified against the password hash;
		/// otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="FormatException">
		/// <paramref name="passwordHash"/> does not have the expected format ($&lt;algorithm&gt;>$&lt;iterations&gt;$&lt;salted-hash&gt;).<br/>
		/// -or-<br/>
		/// The iteration count specified in the <paramref name="passwordHash"/> is not a properly formatted integer value.<br/>
		/// -or-<br/>
		/// The specified salted hash in <paramref name="passwordHash"/> does not have the expected length.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The algorithm specified in the <paramref name="passwordHash"/> does not match the expected algorithm identifier.
		/// </exception>
		public abstract bool Verify(ReadOnlySpan<char> password, ReadOnlySpan<char> passwordHash);

		/// <summary>
		/// Verifies the specified password against the specified password hash using all available hashers.
		/// </summary>
		/// <param name="password">The password to check.</param>
		/// <param name="passwordHash">The password hash to check against.</param>
		/// <returns>
		/// <c>true</c> if the password was successfully verified against the password hash;
		/// otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="FormatException">The specified password hash seems to be improperly formatted.</exception>
		/// <exception cref="NotSupportedException">The hash algorithm specified in the password hash is not supported.</exception>
		public static bool VerifyPassword(string password, string passwordHash)
		{
			if (password == null) throw new ArgumentNullException(nameof(password));
			if (passwordHash == null) throw new ArgumentNullException(nameof(passwordHash));
			if (!TrySplitField(passwordHash.AsSpan(), out ReadOnlySpan<char> algorithm, out ReadOnlySpan<char> _, out ReadOnlySpan<char> _))
				throw new FormatException($"The specified password hash ({passwordHash}) seems to be improperly formatted.");
			Span<char> lowerCaseAlgorithm = stackalloc char[algorithm.Length];
			algorithm.ToLowerInvariant(lowerCaseAlgorithm);
			if (!sHashers.TryGetValue(lowerCaseAlgorithm.ToString(), out SecurePasswordHasher hasher))
				throw new NotSupportedException($"The hash algorithm ({algorithm.ToString()}) is not supported.");
			return hasher.Verify(password, passwordHash);
		}

		/// <summary>
		/// Verifies the specified password against the specified password hash using all available hashers.
		/// </summary>
		/// <param name="password">The password to check.</param>
		/// <param name="passwordHash">The password hash to check against.</param>
		/// <returns>
		/// <c>true</c> if the password was successfully verified against the password hash;
		/// otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="FormatException">The specified password hash seems to be improperly formatted.</exception>
		/// <exception cref="NotSupportedException">The hash algorithm specified in the password hash is not supported.</exception>
		public static bool VerifyPassword(ReadOnlySpan<char> password, ReadOnlySpan<char> passwordHash)
		{
			if (password == null) throw new ArgumentNullException(nameof(password));
			if (passwordHash == null) throw new ArgumentNullException(nameof(passwordHash));
			if (!TrySplitField(passwordHash, out ReadOnlySpan<char> algorithm, out ReadOnlySpan<char> _, out ReadOnlySpan<char> _))
				throw new FormatException($"The specified password hash ({passwordHash.ToString()}) seems to be improperly formatted.");
			Span<char> lowerCaseAlgorithm = stackalloc char[algorithm.Length];
			algorithm.ToLowerInvariant(lowerCaseAlgorithm);
			if (!sHashers.TryGetValue(lowerCaseAlgorithm.ToString(), out SecurePasswordHasher hasher))
				throw new NotSupportedException($"The hash algorithm ({algorithm.ToString()}) is not supported.");
			return hasher.Verify(password, passwordHash);
		}

		/// <summary>
		/// Hashes the specified password using the specified <see cref="HashAlgorithm"/>.
		/// </summary>
		/// <param name="hashAlgorithm">Hash algorithm to use.</param>
		/// <param name="algorithmIdentifier">Identifier of the hash algorithm as used in the password hash string.</param>
		/// <param name="saltSize">Size of the salt (in bytes).</param>
		/// <param name="hashSize">Size of the hash (in bytes).</param>
		/// <param name="password">Password to hash.</param>
		/// <param name="iterations">Number of iterations.</param>
		/// <returns>The password hash.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="iterations"/> is less than 1.</exception>
		protected static string Hash(
			HashAlgorithm hashAlgorithm,
			string        algorithmIdentifier,
			int           saltSize,
			int           hashSize,
			string        password,
			int           iterations)
		{
			if (password == null) throw new ArgumentNullException(nameof(password));
			if (iterations < 1) throw new ArgumentException("Iterations must be greater than zero.", nameof(iterations));

			// get a temporary buffer for the salt, the hash value and the encoded password
			byte[] buffer = ArrayPool<byte>.Shared.Rent(saltSize + hashSize + Encoding.UTF8.GetMaxByteCount(password.Length));

			// initialize the salt in the first part of the password hash
#if NETSTANDARD2_0 || NET48
			using (var random = RandomNumberGenerator.Create())
			{
				random.GetBytes(buffer, 0, saltSize);
			}
#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET5_0 || NET6_0 || NET7_0
			RandomNumberGenerator.Fill(buffer.AsSpan().Slice(0, saltSize));
#else
#error Unhandled target framework.
#endif

			// hash the password with the salt and the specified iterations using the specified algorithm
			hashAlgorithm.TransformBlock(buffer, 0, saltSize, buffer, 0);
			int encodedPasswordOffset = saltSize + hashSize;
			int encodedPasswordLength = Encoding.UTF8.GetBytes(password, 0, password.Length, buffer, encodedPasswordOffset);
			for (int i = 0; i < iterations; i++) hashAlgorithm.TransformBlock(buffer, encodedPasswordOffset, encodedPasswordLength, buffer, encodedPasswordOffset);
			hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
			byte[] hash = hashAlgorithm.Hash;
			Debug.Assert(hash != null);

			// combine salt and hash value, convert it to base64 and format the password hash
			Array.Copy(hash, 0, buffer, saltSize, hashSize);
			string base64Hash = Convert.ToBase64String(buffer, 0, saltSize + hashSize);
			ArrayPool<byte>.Shared.Return(buffer);
			return $"${algorithmIdentifier}${iterations}${base64Hash}";
		}

		/// <summary>
		/// Hashes the specified password using the specified <see cref="HashAlgorithm"/>.
		/// </summary>
		/// <param name="hashAlgorithm">Hash algorithm to use.</param>
		/// <param name="algorithmIdentifier">Identifier of the hash algorithm as used in the password hash string.</param>
		/// <param name="saltSize">Size of the salt (in bytes).</param>
		/// <param name="hashSize">Size of the hash (in bytes).</param>
		/// <param name="password">Password to verify.</param>
		/// <param name="iterations">Number of iterations.</param>
		/// <returns>The password hash.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="iterations"/> is less than 1.</exception>
		protected static string Hash(
			HashAlgorithm      hashAlgorithm,
			string             algorithmIdentifier,
			int                saltSize,
			int                hashSize,
			ReadOnlySpan<char> password,
			int                iterations)
		{
			if (password == null) throw new ArgumentNullException(nameof(password));
			if (iterations < 1) throw new ArgumentException("Iterations must be greater than zero.", nameof(iterations));

			// get a temporary buffer for the salt, the hash value and the encoded password
			byte[] buffer = ArrayPool<byte>.Shared.Rent(saltSize + hashSize + Encoding.UTF8.GetMaxByteCount(password.Length));

			// initialize the salt in the first part of the password hash
#if NETSTANDARD2_0 || NET48
			using (var random = RandomNumberGenerator.Create())
			{
				random.GetBytes(buffer, 0, saltSize);
			}
#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET5_0 || NET6_0 || NET7_0
			RandomNumberGenerator.Fill(buffer.AsSpan().Slice(0, saltSize));
#else
#error Unhandled target framework.
#endif

			// hash the password with the salt and the specified iterations using the specified algorithm
			hashAlgorithm.TransformBlock(buffer, 0, saltSize, buffer, 0);
			int encodedPasswordOffset = saltSize + hashSize;
#if NETSTANDARD2_0 || NET48
			int encodedPasswordLength = Encoding.UTF8.GetBytes(password.ToString(), 0, password.Length, buffer, encodedPasswordOffset);
#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET5_0 || NET6_0 || NET7_0
			int encodedPasswordLength = Encoding.UTF8.GetBytes(password, buffer.AsSpan().Slice(encodedPasswordOffset));
#else
#error Unhandled target framework.
#endif
			for (int i = 0; i < iterations; i++) hashAlgorithm.TransformBlock(buffer, encodedPasswordOffset, encodedPasswordLength, buffer, encodedPasswordOffset);
			hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
			byte[] hash = hashAlgorithm.Hash;
			Debug.Assert(hash != null);

			// combine salt and hash value, convert it to base64 and format the password hash
			Array.Copy(hash, 0, buffer, saltSize, hashSize);
			string base64Hash = Convert.ToBase64String(buffer, 0, saltSize + hashSize);
			ArrayPool<byte>.Shared.Return(buffer);
			return $"${algorithmIdentifier}${iterations}${base64Hash}";
		}

		/// <summary>
		/// Hashes the specified password using PBKDF2 with the specified hash algorithm as key derivation function.
		/// </summary>
		/// <param name="hashAlgorithmName">Hash algorithm to use as key derivation function.</param>
		/// <param name="algorithmIdentifier">Identifier of the hash algorithm as used in the password hash string.</param>
		/// <param name="saltSize">Size of the salt (in bytes).</param>
		/// <param name="hashSize">Size of the hash (in bytes).</param>
		/// <param name="password">Password to hash.</param>
		/// <param name="iterations">Number of iterations.</param>
		/// <returns>The password hash.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="iterations"/> is less than 1.</exception>
		protected static string Hash(
			HashAlgorithmName hashAlgorithmName,
			string            algorithmIdentifier,
			int               saltSize,
			int               hashSize,
			string            password,
			int               iterations)
		{
			if (password == null) throw new ArgumentNullException(nameof(password));
			if (iterations < 1) throw new ArgumentException("Iterations must be greater than zero.", nameof(iterations));

			// prepare salt for hashing
			byte[] salt = ArrayPool<byte>.Shared.Rent(saltSize);
#if NETSTANDARD2_0 || NET48
			using (var random = RandomNumberGenerator.Create())
			{
				random.GetBytes(salt, 0, saltSize);
			}
#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET5_0 || NET6_0 || NET7_0
			RandomNumberGenerator.Fill(salt.AsSpan());
#else
#error Unhandled target framework.
#endif

			// hash the password with the salt and the specified iterations using the specified algorithm
			byte[] hash;
#if NETSTANDARD2_0
			Debug.Assert(hashAlgorithmName == HashAlgorithmName.SHA1);
			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET48 || NET5_0 || NET6_0 || NET7_0
			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, hashAlgorithmName))
#else
#error Unhandled target framework.
#endif
			{
				hash = pbkdf2.GetBytes(hashSize);
			}

			// combine salt and hash value, convert it to base64 and format the password hash
			byte[] buffer = ArrayPool<byte>.Shared.Rent(saltSize + hashSize);
			Array.Copy(salt, 0, buffer, 0, saltSize);
			Array.Copy(hash, 0, buffer, saltSize, hashSize);
			string base64Hash = Convert.ToBase64String(buffer, 0, saltSize + hashSize);
			ArrayPool<byte>.Shared.Return(buffer);
			ArrayPool<byte>.Shared.Return(salt);
			return $"${algorithmIdentifier}${iterations}${base64Hash}";
		}

		/// <summary>
		/// Hashes the specified password using PBKDF2 with the specified hash algorithm as key derivation function.
		/// </summary>
		/// <param name="hashAlgorithmName">Hash algorithm to use as key derivation function.</param>
		/// <param name="algorithmIdentifier">Identifier of the hash algorithm as used in the password hash string.</param>
		/// <param name="saltSize">Size of the salt (in bytes).</param>
		/// <param name="hashSize">Size of the hash (in bytes).</param>
		/// <param name="password">Password to verify.</param>
		/// <param name="iterations">Number of iterations.</param>
		/// <returns>The password hash.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="iterations"/> is less than 1.</exception>
		protected static string Hash(
			HashAlgorithmName  hashAlgorithmName,
			string             algorithmIdentifier,
			int                saltSize,
			int                hashSize,
			ReadOnlySpan<char> password,
			int                iterations)
		{
			if (password == null) throw new ArgumentNullException(nameof(password));
			if (iterations < 1) throw new ArgumentException("Iterations must be greater than zero.", nameof(iterations));

			// prepare salt for hashing
			byte[] salt = ArrayPool<byte>.Shared.Rent(saltSize);
#if NETSTANDARD2_0 || NET48
			using (var random = RandomNumberGenerator.Create())
			{
				random.GetBytes(salt, 0, saltSize);
			}
#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET5_0 || NET6_0 || NET7_0
			RandomNumberGenerator.Fill(salt.AsSpan());
#else
#error Unhandled target framework.
#endif

			// hash the password with the salt and the specified iterations using the specified algorithm
			byte[] hash;
#if NETSTANDARD2_0
			Debug.Assert(hashAlgorithmName == HashAlgorithmName.SHA1);
			using (var pbkdf2 = new Rfc2898DeriveBytes(password.ToString(), salt, iterations))
#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET48 || NET5_0 || NET6_0 || NET7_0
			using (var pbkdf2 = new Rfc2898DeriveBytes(password.ToString(), salt, iterations, hashAlgorithmName))
#else
#error Unhandled target framework.
#endif
			{
				hash = pbkdf2.GetBytes(hashSize);
			}

			// combine salt and hash value, convert it to base64 and format the password hash
			byte[] buffer = ArrayPool<byte>.Shared.Rent(saltSize + hashSize);
			Array.Copy(salt, 0, buffer, 0, saltSize);
			Array.Copy(hash, 0, buffer, saltSize, hashSize);
			string base64Hash = Convert.ToBase64String(buffer, 0, saltSize + hashSize);
			ArrayPool<byte>.Shared.Return(buffer);
			ArrayPool<byte>.Shared.Return(salt);
			return $"${algorithmIdentifier}${iterations}${base64Hash}";
		}

		/// <summary>
		/// Verifies the specified password using the specified <see cref="HashAlgorithm"/>.
		/// </summary>
		/// <param name="hashAlgorithm">Hash algorithm to use.</param>
		/// <param name="algorithmIdentifier">Identifier of the hash algorithm as used in the password hash string.</param>
		/// <param name="saltSize">Size of the salt (in bytes).</param>
		/// <param name="hashSize">Size of the hash (in bytes).</param>
		/// <param name="password">Password to verify.</param>
		/// <param name="passwordHash">The password hash to check <paramref name="password"/> against.</param>
		/// <returns>
		/// <c>true</c> if the password was verified successfully;<br/>
		/// otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="FormatException">
		/// <paramref name="passwordHash"/> does not have the expected format ($&lt;algorithm&gt;>$&lt;iterations&gt;$&lt;salted-hash&gt;).<br/>
		/// -or-<br/>
		/// The iteration count specified in the <paramref name="passwordHash"/> is not a properly formatted integer value.<br/>
		/// -or-<br/>
		/// The specified salted hash in <paramref name="passwordHash"/> does not have the expected length or is not encoded properly.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The algorithm specified in the <paramref name="passwordHash"/> does not match <paramref name="algorithmIdentifier"/>.
		/// </exception>
		protected static bool Verify(
			HashAlgorithm hashAlgorithm,
			string        algorithmIdentifier,
			int           saltSize,
			int           hashSize,
			string        password,
			string        passwordHash)
		{
			// split the password hash into fields for algorithm, iterations and the salted hash for further processing
			if (!TrySplitField(passwordHash.AsSpan(), out ReadOnlySpan<char> algorithmField, out ReadOnlySpan<char> iterationsField, out ReadOnlySpan<char> saltedHashField))
				throw new FormatException("The password hash does not have the expected format ($<algorithm>$<iterations>$<salted-hash>).");

			// check whether the algorithm is supported
			if (!algorithmField.Equals(algorithmIdentifier.AsSpan(), StringComparison.InvariantCultureIgnoreCase))
				throw new NotSupportedException($"The algorithm ({algorithmField.ToString()}) is not supported.");

#if NETSTANDARD2_0 || NET48
			// parse the number of iterations
			if (!int.TryParse(iterationsField.ToString(), out int iterationCount))
				throw new FormatException($"The number of iterations ({iterationsField.ToString()}) is not a properly formatted integer value.");

			// convert the salted hash string to bytes
			Span<byte> saltedHashBytes;
			try { saltedHashBytes = Convert.FromBase64String(saltedHashField.ToString()); }
			catch { throw new FormatException("The salted hash is not encoded properly."); }

#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET5_0 || NET6_0 || NET7_0
			// parse the number of iterations
			if (!int.TryParse(iterationsField, out int iterationCount))
				throw new FormatException($"The number of iterations ({iterationsField.ToString()}) is not a properly formatted integer value.");

			// convert the salted hash string to bytes
			int maxSaltedHashByteCount = GetMaxBase64Length(saltedHashField.Length);
			Span<byte> saltedHashBytes = stackalloc byte[maxSaltedHashByteCount];
			if (!Convert.TryFromBase64Chars(saltedHashField, saltedHashBytes, out int saltedHashByteCount))
				throw new FormatException("The salted hash is not encoded properly.");
			saltedHashBytes = saltedHashBytes.Slice(0, saltedHashByteCount);
#else
#error Unhandled target framework.
#endif

			// ensure that the salted hash has the expected length
			if (saltedHashBytes.Length != saltSize + hashSize)
				throw new FormatException($"The salted hash field in the password hash must be {saltSize + hashSize} bytes long.");

			// get salt
			byte[] salt = saltedHashBytes.Slice(0, saltSize).ToArray();

			// calculate hash over the salt and the repeatedly hashed UTF-8 encoded password
			hashAlgorithm.TransformBlock(salt, 0, salt.Length, salt, 0);
			byte[] encodedPassword = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(password.Length));
#if NETSTANDARD2_0 || NET48
			int encodedPasswordLength = Encoding.UTF8.GetBytes(password, 0, password.Length, encodedPassword, 0);
#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET5_0 || NET6_0 || NET7_0
			int encodedPasswordLength = Encoding.UTF8.GetBytes(password.AsSpan(), encodedPassword.AsSpan());
#else
#error Unhandled target framework.
#endif
			for (int i = 0; i < iterationCount; i++) hashAlgorithm.TransformBlock(encodedPassword, 0, encodedPasswordLength, encodedPassword, 0);
			hashAlgorithm.TransformFinalBlock(encodedPassword, 0, 0);
			ArrayPool<byte>.Shared.Return(encodedPassword);
			byte[] hash = hashAlgorithm.Hash;

			// get result
			Debug.Assert(hash != null);
			return saltedHashBytes.Slice(saltSize).SequenceEqual(hash.AsSpan());
		}

		/// <summary>
		/// Verifies the specified password using the specified <see cref="HashAlgorithm"/>.
		/// </summary>
		/// <param name="hashAlgorithm">Hash algorithm to use.</param>
		/// <param name="algorithmIdentifier">Identifier of the hash algorithm as used in the password hash string.</param>
		/// <param name="saltSize">Size of the salt (in bytes).</param>
		/// <param name="hashSize">Size of the hash (in bytes).</param>
		/// <param name="password">Password to verify.</param>
		/// <param name="passwordHash">The password hash to check <paramref name="password"/> against.</param>
		/// <returns>
		/// <c>true</c> if the password was verified successfully;<br/>
		/// otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="FormatException">
		/// <paramref name="passwordHash"/> does not have the expected format ($&lt;algorithm&gt;>$&lt;iterations&gt;$&lt;salted-hash&gt;).<br/>
		/// -or-<br/>
		/// The iteration count specified in the <paramref name="passwordHash"/> is not a properly formatted integer value.<br/>
		/// -or-<br/>
		/// The specified salted hash in <paramref name="passwordHash"/> does not have the expected length or is not encoded properly.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The algorithm specified in the <paramref name="passwordHash"/> does not match <paramref name="algorithmIdentifier"/>.
		/// </exception>
		protected static bool Verify(
			HashAlgorithm      hashAlgorithm,
			ReadOnlySpan<char> algorithmIdentifier,
			int                saltSize,
			int                hashSize,
			ReadOnlySpan<char> password,
			ReadOnlySpan<char> passwordHash)
		{
			// split the password hash into fields for algorithm, iterations and the salted hash for further processing
			if (!TrySplitField(passwordHash, out ReadOnlySpan<char> algorithmField, out ReadOnlySpan<char> iterationsField, out ReadOnlySpan<char> saltedHashField))
				throw new FormatException("The password hash does not have the expected format ($<algorithm>$<iterations>$<salted-hash>).");

			// check whether the algorithm is supported
			if (!algorithmField.Equals(algorithmIdentifier, StringComparison.InvariantCultureIgnoreCase))
				throw new NotSupportedException($"The algorithm ({algorithmField.ToString()}) is not supported.");

#if NETSTANDARD2_0 || NET48
			// parse the number of iterations
			if (!int.TryParse(iterationsField.ToString(), out int iterationCount))
				throw new FormatException($"The number of iterations ({iterationsField.ToString()}) is not a properly formatted integer value.");

			// convert the salted hash string to bytes
			Span<byte> saltedHashBytes;
			try { saltedHashBytes = Convert.FromBase64String(saltedHashField.ToString()); }
			catch { throw new FormatException("The salted hash is not encoded properly."); }

#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET5_0 || NET6_0 || NET7_0
			// parse the number of iterations
			if (!int.TryParse(iterationsField, out int iterationCount))
				throw new FormatException($"The number of iterations ({iterationsField.ToString()}) is not a properly formatted integer value.");

			// convert the salted hash string to bytes
			int maxSaltedHashByteCount = GetMaxBase64Length(saltedHashField.Length);
			Span<byte> saltedHashBytes = stackalloc byte[maxSaltedHashByteCount];
			if (!Convert.TryFromBase64Chars(saltedHashField, saltedHashBytes, out int saltedHashByteCount))
				throw new FormatException("The salted hash is not encoded properly.");
			saltedHashBytes = saltedHashBytes.Slice(0, saltedHashByteCount);
#else
#error Unhandled target framework.
#endif

			// ensure that the salted hash has the expected length
			if (saltedHashBytes.Length != saltSize + hashSize)
				throw new FormatException($"The salted hash field in the password hash must be {saltSize + hashSize} bytes long.");

			// get salt
			byte[] salt = saltedHashBytes.Slice(0, saltSize).ToArray();

			// calculate hash over the salt and the repeatedly hashed UTF-8 encoded password
			hashAlgorithm.TransformBlock(salt, 0, salt.Length, salt, 0);
			byte[] encodedPassword = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(password.Length));
#if NETSTANDARD2_0 || NET48
			int encodedPasswordLength = Encoding.UTF8.GetBytes(password.ToString(), 0, password.Length, encodedPassword, 0);
#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET5_0 || NET6_0 || NET7_0
			int encodedPasswordLength = Encoding.UTF8.GetBytes(password, encodedPassword.AsSpan());
#else
#error Unhandled target framework.
#endif
			for (int i = 0; i < iterationCount; i++) hashAlgorithm.TransformBlock(encodedPassword, 0, encodedPasswordLength, encodedPassword, 0);
			hashAlgorithm.TransformFinalBlock(encodedPassword, 0, 0);
			ArrayPool<byte>.Shared.Return(encodedPassword);
			byte[] hash = hashAlgorithm.Hash;

			// get result
			Debug.Assert(hash != null);
			return saltedHashBytes.Slice(saltSize).SequenceEqual(hash.AsSpan());
		}

		/// <summary>
		/// Verifies the specified password using <see cref="Rfc2898DeriveBytes"/> with the specified hash algorithm as key derivation function.
		/// </summary>
		/// <param name="hashAlgorithmName">Name of the hash algorithm to use for the key derivation.</param>
		/// <param name="algorithmIdentifier">Identifier of the hash algorithm as used in the password hash string.</param>
		/// <param name="saltSize">Size of the salt (in bytes).</param>
		/// <param name="hashSize">Size of the hash (in bytes).</param>
		/// <param name="password">Password to verify.</param>
		/// <param name="passwordHash">The password hash to check <paramref name="password"/> against.</param>
		/// <returns>
		/// <c>true</c> if the password was verified successfully;<br/>
		/// otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="FormatException">
		/// <paramref name="passwordHash"/> does not have the expected format ($&lt;algorithm&gt;>$&lt;iterations&gt;$&lt;salted-hash&gt;).<br/>
		/// -or-<br/>
		/// The iteration count specified in the <paramref name="passwordHash"/> is not a properly formatted integer value.<br/>
		/// -or-<br/>
		/// The specified salted hash in <paramref name="passwordHash"/> does not have the expected length or is not encoded properly.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The algorithm specified in the <paramref name="passwordHash"/> does not match <paramref name="algorithmIdentifier"/>.
		/// </exception>
		protected static bool Verify(
			HashAlgorithmName hashAlgorithmName,
			string            algorithmIdentifier,
			int               saltSize,
			int               hashSize,
			string            password,
			string            passwordHash)
		{
			// split the password hash into fields for algorithm, iterations and the salted hash for further processing
			if (!TrySplitField(passwordHash.AsSpan(), out ReadOnlySpan<char> algorithmField, out ReadOnlySpan<char> iterationsField, out ReadOnlySpan<char> saltedHashField))
				throw new FormatException("The password hash does not have the expected format ($<algorithm>$<iterations>$<salted-hash>).");

			// check whether the algorithm is supported
			if (!algorithmField.Equals(algorithmIdentifier.AsSpan(), StringComparison.InvariantCultureIgnoreCase))
				throw new NotSupportedException($"The algorithm ({algorithmField.ToString()}) is not supported.");

#if NETSTANDARD2_0 || NET48
			// parse the number of iterations
			if (!int.TryParse(iterationsField.ToString(), out int iterationCount))
				throw new FormatException($"The number of iterations ({iterationsField.ToString()}) is not a properly formatted integer value.");

			// convert the salted hash string to bytes
			Span<byte> saltedHashBytes;
			try { saltedHashBytes = Convert.FromBase64String(saltedHashField.ToString()); }
			catch { throw new FormatException("The salted hash is not encoded properly."); }

#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET5_0 || NET6_0 || NET7_0
			// parse the number of iterations
			if (!int.TryParse(iterationsField, out int iterationCount))
				throw new FormatException($"The number of iterations ({iterationsField.ToString()}) is not a properly formatted integer value.");

			// convert the salted hash string to bytes
			int maxSaltedHashByteCount = GetMaxBase64Length(saltedHashField.Length);
			Span<byte> saltedHashBytes = stackalloc byte[maxSaltedHashByteCount];
			if (!Convert.TryFromBase64Chars(saltedHashField, saltedHashBytes, out int saltedHashByteCount))
				throw new FormatException("The salted hash is not encoded properly.");
			saltedHashBytes = saltedHashBytes.Slice(0, saltedHashByteCount);
#else
#error Unhandled target framework.
#endif

			// ensure that the salted hash has the expected length
			if (saltedHashBytes.Length != saltSize + hashSize)
				throw new FormatException($"The salted hash field in the password hash must be {saltSize + hashSize} bytes long.");

			// get salt
			byte[] salt = saltedHashBytes.Slice(0, saltSize).ToArray();

			// calculate hash
			byte[] hash;

#if NETSTANDARD2_0
			Debug.Assert(hashAlgorithmName == HashAlgorithmName.SHA1);
			Debug.Assert(iterationCount == DefaultIterationCount);
			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterationCount))
#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET48 || NET5_0 || NET6_0 || NET7_0
			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterationCount, hashAlgorithmName))
#else
#error Unhandled target framework.
#endif
			{
				hash = pbkdf2.GetBytes(hashSize);
			}

			// get result
			return saltedHashBytes.Slice(saltSize).SequenceEqual(hash.AsSpan());
		}

		/// <summary>
		/// Verifies the specified password using <see cref="Rfc2898DeriveBytes"/> with the specified hash algorithm as key derivation function.
		/// </summary>
		/// <param name="hashAlgorithmName">Name of the hash algorithm to use for the key derivation.</param>
		/// <param name="algorithmIdentifier">Identifier of the hash algorithm as used in the password hash string.</param>
		/// <param name="saltSize">Size of the salt (in bytes).</param>
		/// <param name="hashSize">Size of the hash (in bytes).</param>
		/// <param name="password">Password to verify.</param>
		/// <param name="passwordHash">The password hash to check <paramref name="password"/> against.</param>
		/// <returns>
		/// <c>true</c> if the password was verified successfully;<br/>
		/// otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="FormatException">
		/// <paramref name="passwordHash"/> does not have the expected format ($&lt;algorithm&gt;>$&lt;iterations&gt;$&lt;salted-hash&gt;).<br/>
		/// -or-<br/>
		/// The iteration count specified in the <paramref name="passwordHash"/> is not a properly formatted integer value.<br/>
		/// -or-<br/>
		/// The specified salted hash in <paramref name="passwordHash"/> does not have the expected length or is not encoded properly.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The algorithm specified in the <paramref name="passwordHash"/> does not match <paramref name="algorithmIdentifier"/>.
		/// </exception>
		protected static bool Verify(
			HashAlgorithmName  hashAlgorithmName,
			ReadOnlySpan<char> algorithmIdentifier,
			int                saltSize,
			int                hashSize,
			ReadOnlySpan<char> password,
			ReadOnlySpan<char> passwordHash)
		{
			// split the password hash into fields for algorithm, iterations and the salted hash for further processing
			if (!TrySplitField(passwordHash, out ReadOnlySpan<char> algorithmField, out ReadOnlySpan<char> iterationsField, out ReadOnlySpan<char> saltedHashField))
				throw new FormatException("The password hash does not have the expected format ($<algorithm>$<iterations>$<salted-hash>).");

			// check whether the algorithm is supported
			if (!algorithmField.Equals(algorithmIdentifier, StringComparison.InvariantCultureIgnoreCase))
				throw new NotSupportedException($"The algorithm ({algorithmField.ToString()}) is not supported.");

#if NETSTANDARD2_0 || NET48
			// parse the number of iterations
			if (!int.TryParse(iterationsField.ToString(), out int iterationCount))
				throw new FormatException($"The number of iterations ({iterationsField.ToString()}) is not a properly formatted integer value.");

			// convert the salted hash string to bytes
			Span<byte> saltedHashBytes;
			try { saltedHashBytes = Convert.FromBase64String(saltedHashField.ToString()); }
			catch { throw new FormatException("The salted hash is not encoded properly."); }

#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET5_0 || NET6_0 || NET7_0
			// parse the number of iterations
			if (!int.TryParse(iterationsField, out int iterationCount))
				throw new FormatException($"The number of iterations ({iterationsField.ToString()}) is not a properly formatted integer value.");

			// convert the salted hash string to bytes
			int maxSaltedHashByteCount = GetMaxBase64Length(saltedHashField.Length);
			Span<byte> saltedHashBytes = stackalloc byte[maxSaltedHashByteCount];
			if (!Convert.TryFromBase64Chars(saltedHashField, saltedHashBytes, out int saltedHashByteCount))
				throw new FormatException("The salted hash is not encoded properly.");
			saltedHashBytes = saltedHashBytes.Slice(0, saltedHashByteCount);
#else
#error Unhandled target framework.
#endif

			// ensure that the salted hash has the expected length
			if (saltedHashBytes.Length != saltSize + hashSize)
				throw new FormatException($"The salted hash field in the password hash must be {saltSize + hashSize} bytes long.");

			// get salt
			byte[] salt = saltedHashBytes.Slice(0, saltSize).ToArray();

			// calculate hash
			byte[] hash;

#if NETSTANDARD2_0
			Debug.Assert(hashAlgorithmName == HashAlgorithmName.SHA1);
			Debug.Assert(iterationCount == DefaultIterationCount);
			using (var pbkdf2 = new Rfc2898DeriveBytes(password.ToString(), salt, iterationCount))
#elif NETSTANDARD2_1 || NETCOREAPP3_1 || NET48 || NET5_0 || NET6_0 || NET7_0
			using (var pbkdf2 = new Rfc2898DeriveBytes(password.ToString(), salt, iterationCount, hashAlgorithmName))
#else
#error Unhandled target framework.
#endif
			{
				hash = pbkdf2.GetBytes(hashSize);
			}

			// get result
			return saltedHashBytes.Slice(saltSize).SequenceEqual(hash.AsSpan());
		}

		/// <summary>
		/// Gets the maximum number of bytes needed to store the specified number of BASE64 encoded characters.
		/// </summary>
		/// <param name="charCount">Number of characters to calculate the number of BASE64 bytes for.</param>
		/// <returns>
		/// Maximum number of bytes needed to store the specified number of BASE64 encoded characters.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static int GetMaxBase64Length(int charCount)
		{
			return (Encoding.UTF8.GetMaxByteCount(charCount) >> 2) * 3;
		}

		/// <summary>
		/// Tries to split the specified password hash string into an algorithm field, an iterations field and a field
		/// containing the salted hash (only splitting, no formatting checks).
		/// </summary>
		/// <param name="passwordHash">The password hash string to split.</param>
		/// <param name="algorithm">Receives the substring representing the name of the algorithm.</param>
		/// <param name="iterations">Receives the substring representing the iterations field.</param>
		/// <param name="saltedHash">Receives the substring representing the salted hash.</param>
		/// <returns>
		/// <c>true</c> if splitting <paramref name="passwordHash"/> succeeded;<br/>
		/// <c>false</c> if splitting failed.
		/// </returns>
		protected static bool TrySplitField(
			ReadOnlySpan<char>     passwordHash,
			out ReadOnlySpan<char> algorithm,
			out ReadOnlySpan<char> iterations,
			out ReadOnlySpan<char> saltedHash)
		{
			algorithm = null;
			iterations = null;
			saltedHash = null;

			// the password hash should start with '$'
			ReadOnlySpan<char> remaining = passwordHash;
			int index = remaining.IndexOf('$');
			if (index != 0) return false;
			remaining = remaining.Slice(1);

			// ... followed by the algorithm name
			index = remaining.IndexOf('$');
			if (index < 0) return false;
			algorithm = remaining.Slice(0, index);
			remaining = remaining.Slice(index + 1);

			// ... followed by the number of iterations
			index = remaining.IndexOf('$');
			if (index < 0) return false;
			iterations = remaining.Slice(0, index);
			remaining = remaining.Slice(index + 1);

			// ... followed by the salted hash
			saltedHash = remaining.Slice(0);

			return true;
		}
	}

}
