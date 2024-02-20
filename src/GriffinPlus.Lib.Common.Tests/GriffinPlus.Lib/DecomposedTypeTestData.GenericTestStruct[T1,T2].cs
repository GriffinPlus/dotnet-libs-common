///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// ReSharper disable UnusedTypeParameter

namespace GriffinPlus.Lib;

/// <summary>
/// Unit tests targeting the <see cref="TypeDecomposer"/> class.
/// </summary>
public partial class DecomposedTypeTestData
{
	public struct GenericTestStruct<T1, T2>
	{
		public struct NestedTestStruct;

		public struct NestedGenericTestStruct<T3, T4>;
	}
}
