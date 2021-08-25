///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Collections
{

	/// <summary>
	/// Unit tests targeting the <see cref="ByteSequenceKeyedDictionary{TValue}"/> class (for value types).
	/// </summary>
	public class ByteSequenceKeyedDictionaryTests_ValueType : ByteSequenceKeyedDictionaryTests_Base<int>
	{
		/// <summary>
		/// Gets a dictionary containing some test data.
		/// </summary>
		/// <param name="count">Number of entries in the dictionary.</param>
		/// <param name="minKeyLength">Minimum number of bytes forming the key.</param>
		/// <param name="maxKeyLength">Maximum number of bytes forming the key.</param>
		/// <returns>A test data dictionary.</returns>
		protected override IDictionary<IReadOnlyList<byte>, int> GetTestData(int count, int minKeyLength = 0, int maxKeyLength = 50)
		{
			// generate random test data
			var dict = new Dictionary<IReadOnlyList<byte>, int>(ReadOnlyListEqualityComparer<byte>.Instance);
			var random = new Random(0);
			while (dict.Count < count)
			{
				byte[] key = new byte[random.Next(minKeyLength, maxKeyLength)];
				random.NextBytes(key);
				for (int i = 0; i < key.Length; i++) key[i] %= 0xff; // keep 0xff out of the sequence
				dict[key] = random.Next(1, int.MaxValue);            // do not assign 0 (default(int)) to have a value that is surely not in the dictionary
			}

			return dict;
		}

		/// <summary>
		/// Gets a key that is guaranteed to be not in the generated test data set.
		/// </summary>
		protected override IReadOnlyList<byte> KeyNotInTestData => new byte[] { 0xff }; // 0xff is not in any of the generated keys

		/// <summary>
		/// Gets a value that is guaranteed to be not in the generated test data set.
		/// Must not be the default value of <see cref="System.Int32"/>.
		/// </summary>
		protected override int ValueNotInTestData => -1;
	}

}
