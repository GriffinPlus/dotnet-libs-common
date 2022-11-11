///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Collections
{

	/// <summary>
	/// Unit tests targeting the <see cref="IdentityKeyedDictionary{TKey,TValue}"/> class (for reference types as value).
	/// </summary>
	public class IdentityKeyedDictionaryTests_ReferenceType : IdentityKeyedDictionaryTests_Base<string>
	{
		/// <summary>
		/// Gets an instance of the dictionary to test, populated with the specified data.
		/// </summary>
		/// <param name="data">Data to populate the dictionary with.</param>
		/// <returns>A new instance of the dictionary to test, populated with the specified data.</returns>
		protected override IGenericDictionary<string, string> GetDictionary(IDictionary<string, string> data = null)
		{
			if (data != null) return new IdentityKeyedDictionary<string, string>(data);
			return new IdentityKeyedDictionary<string, string>();
		}

		/// <summary>
		/// Gets a dictionary containing some test data.
		/// </summary>
		/// <param name="count">Number of entries in the dictionary.</param>
		/// <returns>A test data dictionary.</returns>
		protected override IDictionary<string, string> GetTestData(int count)
		{
			var dict = new Dictionary<string, string>(EqualityComparer<string>.Default);
			var random = new Random(0);
			while (dict.Count < count)
			{
				string key = $"{random.Next():X8}";
				string value = $"{random.Next():X8}";
				dict[key] = value;
			}

			return dict;
		}

		/// <summary>
		/// Gets a value that is guaranteed to be not in the generated test data set.
		/// Must not be the default value of <see cref="System.String"/>.
		/// </summary>
		protected override string ValueNotInTestData => "xxx"; // not a valid hex string
	}

}
