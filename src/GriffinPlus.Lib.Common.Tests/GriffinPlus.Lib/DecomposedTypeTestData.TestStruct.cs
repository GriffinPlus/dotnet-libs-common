﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// ReSharper disable UnusedTypeParameter

namespace GriffinPlus.Lib;

partial class DecomposedTypeTestData
{
	public struct TestStruct
	{
		public struct NestedTestStruct;

		public struct NestedGenericTestStruct<T>;
	}
}
