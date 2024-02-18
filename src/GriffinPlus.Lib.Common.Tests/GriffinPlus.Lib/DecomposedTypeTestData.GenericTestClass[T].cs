///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// ReSharper disable UnusedTypeParameter

namespace GriffinPlus.Lib
{

	partial class DecomposedTypeTestData
	{
		public class GenericTestClass<T>
		{
			public class NestedTestClass;

			public class NestedGenericTestClass<T2>;
		}
	}

}
