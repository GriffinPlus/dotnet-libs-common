﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Collections;

/// <summary>
/// Unit tests targeting the <see cref="TypeKeyedDictionary{TValue}"/> class (for value types).
/// </summary>
// ReSharper disable once UnusedMember.Global
public class TypeKeyedDictionaryTests_ValueType : TypeKeyedDictionaryTests_Base<int>
{
	/// <summary>
	/// Gets an instance of the dictionary to test, populated with the specified data.
	/// </summary>
	/// <param name="data">Data to populate the dictionary with.</param>
	/// <returns>A new instance of the dictionary to test, populated with the specified data.</returns>
	protected override IGenericDictionary<Type, int> GetDictionary(IDictionary<Type, int> data = null)
	{
		return data != null
			       ? new TypeKeyedDictionary<int>(data)
			       : [];
	}

	/// <summary>
	/// Gets a dictionary containing some test data.
	/// </summary>
	/// <param name="count">Number of entries in the dictionary.</param>
	/// <returns>A test data dictionary.</returns>
	protected override IDictionary<Type, int> GetTestData(int count)
	{
		// generate random test data
		var dict = new Dictionary<Type, int>(EqualityComparer<Type>.Default);
		Type[] types = GetTypes();
		for (int i = 0; i < count; i++)
		{
			Type key = types[i];
			dict[key] = types[i].GetHashCode();
		}

		return dict;
	}

	/// <summary>
	/// Gets a value that is guaranteed to be not in the generated test data set.
	/// Must not be the default value of <see cref="System.Int32"/>.
	/// </summary>
	protected override int ValueNotInTestData => -1;
}
