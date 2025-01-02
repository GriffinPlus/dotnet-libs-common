///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Collections;

/// <summary>
/// Unit tests targeting the <see cref="ByteSequenceKeyedDictionary{TValue}"/> class (for reference types).
/// </summary>
// ReSharper disable once UnusedMember.Global
public class ByteSequenceKeyedDictionaryTests_ReferenceType : ByteSequenceKeyedDictionaryTests_Base<string>
{
	/// <summary>
	/// Gets an instance of the dictionary to test, populated with the specified data.
	/// </summary>
	/// <param name="data">Data to populate the dictionary with.</param>
	/// <returns>A new instance of the dictionary to test, populated with the specified data.</returns>
	protected override IGenericDictionary<IReadOnlyList<byte>, string> GetDictionary(IDictionary<IReadOnlyList<byte>, string> data = null)
	{
		return data != null
			       ? new ByteSequenceKeyedDictionary<string>(data)
			       : new ByteSequenceKeyedDictionary<string>();
	}

	/// <summary>
	/// Gets a dictionary containing some test data.
	/// </summary>
	/// <param name="count">Number of entries in the dictionary.</param>
	/// <returns>A test data dictionary.</returns>
	protected override IDictionary<IReadOnlyList<byte>, string> GetTestData(int count)
	{
		// generate random test data
		const int minKeyLength = 10;
		const int maxKeyLength = 50;
		var dict = new Dictionary<IReadOnlyList<byte>, string>(ReadOnlyListEqualityComparer<byte>.Instance);
		var random = new Random(0);
		while (dict.Count < count)
		{
			byte[] key = new byte[random.Next(minKeyLength, maxKeyLength)];
			random.NextBytes(key);
			for (int i = 0; i < key.Length; i++) key[i] %= 0xff; // keep 0xff out of the sequence
			dict[key] = key.ToHexString();                       // may overwrite the same item
		}

		return dict;
	}

	/// <summary>
	/// Gets a key that is guaranteed to be not in the generated test data set.
	/// </summary>
	protected override IReadOnlyList<byte> KeyNotInTestData => [0xff]; // 0xff is not in any of the generated keys

	/// <summary>
	/// Gets a value that is guaranteed to be not in the generated test data set.
	/// Must not be the default value of <see cref="System.String"/>.
	/// </summary>
	protected override string ValueNotInTestData => "xxx"; // not a hex byte sequence
}
