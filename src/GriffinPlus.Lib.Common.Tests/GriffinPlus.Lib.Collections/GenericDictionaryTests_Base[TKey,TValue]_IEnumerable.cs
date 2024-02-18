///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace GriffinPlus.Lib.Collections
{

	public abstract partial class GenericDictionaryTests_Base<TKey, TValue>
	{
		#region GetEnumerator() - incl. all enumerator functionality

		/// <summary>
		/// Tests enumerating key/value pairs using <see cref="IEnumerable.GetEnumerator"/>.
		/// </summary>
		/// <param name="count">Number of elements to populate the dictionary with before running the test.</param>
		[Theory]
		[MemberData(nameof(TestDataSetSizes))]
		public void IEnumerable_GetEnumerator(int count)
		{
			// get test data and create a new dictionary with it
			IDictionary<TKey, TValue> data = GetTestData(count);
			var dict = GetDictionary(data) as IDictionary<TKey, TValue>;

			// get an enumerator
			IEnumerator enumerator = ((IEnumerable)dict).GetEnumerator();

			// the enumerator should point to the position before the first valid element,
			// but the 'Current' property should not throw an exception
			object _ = enumerator.Current;

			// enumerate the key/value pairs in the dictionary
			var enumerated = new List<KeyValuePair<TKey, TValue>>();
			while (enumerator.MoveNext())
			{
				Assert.IsType<KeyValuePair<TKey, TValue>>(enumerator.Current);
				var current = (KeyValuePair<TKey, TValue>)enumerator.Current;
				Assert.IsAssignableFrom<TKey>(current.Key);
				Assert.IsAssignableFrom<TValue>(current.Value);
				enumerated.Add(current);
			}

			// compare collection elements with the expected values
			Assert.Equal(
				data.OrderBy(x => x.Key, KeyComparer),
				enumerated.OrderBy(x => x.Key, KeyComparer),
				KeyValuePairEqualityComparer);

			// the enumerator should point to the position after the last valid element now,
			// but the 'Current' property should not throw an exception
			// ReSharper disable once AssignmentInsteadOfDiscard
			// ReSharper disable once RedundantAssignment
			_ = enumerator.Current;

			// reset the enumerator and try again
			enumerator.Reset();
			enumerated = [];
			while (enumerator.MoveNext())
			{
				Assert.IsType<KeyValuePair<TKey, TValue>>(enumerator.Current);
				var current = (KeyValuePair<TKey, TValue>)enumerator.Current;
				Assert.IsAssignableFrom<TKey>(current.Key);
				Assert.IsAssignableFrom<TValue>(current.Value);
				enumerated.Add(current);
			}

			// compare collection elements with the expected values
			Assert.Equal(
				data.OrderBy(x => x.Key, KeyComparer),
				enumerated.OrderBy(x => x.Key, KeyComparer),
				KeyValuePairEqualityComparer);

			// modify the collection, the enumerator should recognize this
			dict[KeyNotInTestData] = ValueNotInTestData;
			Assert.Throws<InvalidOperationException>(() => enumerator.Reset());
			Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());

			// dispose enumerator
			(enumerator as IDisposable)!.Dispose();
		}

		#endregion
	}

}
