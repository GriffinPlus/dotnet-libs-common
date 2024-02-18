///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Collections
{

	/// <summary>
	/// Unit tests targeting the <see cref="IdentityKeyedDictionary{TKey,TValue}"/> class (for value types as value).
	/// </summary>
	// ReSharper disable once UnusedMember.Global
	public class IdentityKeyedDictionaryTests_ValueType : IdentityKeyedDictionaryTests_Base<int>
	{
		/// <summary>
		/// Gets an instance of the dictionary to test, populated with the specified data.
		/// </summary>
		/// <param name="data">Data to populate the dictionary with.</param>
		/// <returns>A new instance of the dictionary to test, populated with the specified data.</returns>
		protected override IGenericDictionary<string, int> GetDictionary(IDictionary<string, int> data = null)
		{
			if (data != null) return new IdentityKeyedDictionary<string, int>(data);
			return new IdentityKeyedDictionary<string, int>();
		}

		/// <summary>
		/// Gets a dictionary containing some test data.
		/// </summary>
		/// <param name="count">Number of entries in the dictionary.</param>
		/// <returns>A test data dictionary.</returns>
		protected override IDictionary<string, int> GetTestData(int count)
		{
			var dict = new Dictionary<string, int>(EqualityComparer<string>.Default);
			var random = new Random(0);
			while (dict.Count < count)
			{
				string key = $"{random.Next():X8}";
				int value = random.Next();
				dict[key] = value;
			}

			return dict;
		}

		/// <summary>
		/// Gets a value that is guaranteed to be not in the generated test data set.
		/// Must not be the default value of <see cref="System.String"/>.
		/// </summary>
		protected override int ValueNotInTestData => -1;
	}

}
