///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using Xunit;

namespace GriffinPlus.Lib
{

	/// <summary>
	/// Helper class that assists with making test collections run without parallelization.
	/// This is usually useful if the tests consume a lot of resources.
	/// The excessive use of resources could otherwise disturb other tests running in parallel,
	/// especially if these tests try to test timeout behavior of operations.
	/// </summary>
	[CollectionDefinition("NoParallelization", DisableParallelization = true)]
	public class NoParallelizationCollection { }

}
