///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib
{

	partial class DecomposedTypeTestData
	{
		public struct GenericTestStruct<T>
		{
			public struct NestedTestStruct
			{
			}

			public struct NestedGenericTestStruct<T2>
			{
			}
		}
	}

}
