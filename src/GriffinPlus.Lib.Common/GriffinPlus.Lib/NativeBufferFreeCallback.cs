///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib
{

	/// <summary>
	/// Callback that is invoked to free the buffer wrapped by a <see cref="NativeBuffer"/> instance.
	/// </summary>
	/// <param name="buffer">The <see cref="NativeBuffer"/> instance wrapping the buffer to free.</param>
	public delegate void NativeBufferFreeCallback(NativeBuffer buffer);

}
