///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

// ReSharper disable AssignNullToNotNullAttribute

namespace GriffinPlus.Lib.Collections
{

	public abstract partial class ByteSequenceKeyedDictionaryTests_Base<TValue>
	{
		#region IReadOnlyDictionary<TKey,TValue>.Keys

		/// <summary>
		/// Tests accessing the key collection via <see cref="IReadOnlyDictionary{TKey,TValue}.Keys"/>.
		/// The key collection should present all keys in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IReadOnlyDictionaryT_Keys(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as IReadOnlyDictionary<IReadOnlyList<byte>, TValue>;

			// enumerate the keys in the dictionary
			var enumerated = dict.Keys.ToList();

			// compare collection elements with the expected values
			Assert.Equal(
				data.Select(x => x.Key).OrderBy(x => x, ReadOnlyListComparer<byte>.Instance).ToArray(),
				enumerated.OrderBy(x => x, ReadOnlyListComparer<byte>.Instance).ToArray(),
				ReadOnlyListEqualityComparer<byte>.Instance);
		}

		#endregion

		#region IReadOnlyDictionary<TKey,TValue>.Values

		/// <summary>
		/// Tests accessing the value collection via <see cref="IReadOnlyDictionary{TKey,TValue}.Values"/>.
		/// The value collection should present all values in the dictionary.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IReadOnlyDictionaryT_Values(int count)
		{
			// get test data and create a new dictionary with it
			var data = GetTestData(count);
			var dict = new ByteSequenceKeyedDictionary<TValue>(data) as IReadOnlyDictionary<IReadOnlyList<byte>, TValue>;

			// enumerate the values in the dictionary
			var enumerated = dict.Values.ToList();

			// compare collection elements with the expected values
			Assert.Equal(
				data.Select(x => x.Value).OrderBy(x => x, Comparer<TValue>.Default).ToArray(),
				enumerated.OrderBy(x => x, Comparer<TValue>.Default).ToArray(),
				EqualityComparer<TValue>.Default);
		}

		#endregion
	}

}
