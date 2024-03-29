﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace GriffinPlus.Lib.Collections;

partial class IdentityKeyedDictionaryTests_Base<TValue>
{
	#region KeyCollection # GetEnumerator() - incl. all enumerator functionality

	/// <summary>
	/// Tests the <see cref="GetEnumerator"/> method of the <see cref="IdentityKeyedDictionary{TKey,TValue}.Keys"/> collection.
	/// </summary>
	/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
	[Theory]
	[MemberData(nameof(TestDataSetSizes))]
	public void KeyCollection_GetEnumerator(int count)
	{
		// get test data and create a new dictionary with it
		IDictionary<string, TValue> data = GetTestData(count);
		var dict = new IdentityKeyedDictionary<string, TValue>(data);
		IdentityKeyedDictionary<string, TValue>.KeyCollection collection = dict.Keys;

		// get an enumerator
		IdentityKeyedDictionary<string, TValue>.KeyCollection.Enumerator enumerator = collection.GetEnumerator();

		// the enumerator should point to the position before the first valid element,
		// but the 'Current' property should not throw an exception
		string _ = enumerator.Current;

		// enumerate the keys in the collection
		var enumerated = new List<string>();
		while (enumerator.MoveNext()) enumerated.Add(enumerator.Current);

		// the order of keys should be the same as returned by the dictionary enumerator
		Assert.Equal(
			dict.Select(x => x.Key),
			enumerated,
			KeyEqualityComparer);

		// the enumerator should point to the position after the last valid element now,
		// but the 'Current' property should not throw an exception
		// ReSharper disable once AssignmentInsteadOfDiscard
		// ReSharper disable once RedundantAssignment
		_ = enumerator.Current;

		// modify the collection, the enumerator should recognize this
		dict[KeyNotInTestData] = ValueNotInTestData;
		Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());

		// dispose the enumerator
		enumerator.Dispose();
	}

	#endregion
}
