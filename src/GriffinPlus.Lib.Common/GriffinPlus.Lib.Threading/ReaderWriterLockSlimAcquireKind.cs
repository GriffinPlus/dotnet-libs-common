///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.Threading;

/// <summary>
/// Acquire type determining how to acquire a <see cref="System.Threading.ReaderWriterLockSlim"/>.
/// </summary>
public enum ReaderWriterLockSlimAcquireKind
{
	/// <summary>
	/// Acquire the lock for reading only.
	/// </summary>
	Read,

	/// <summary>
	/// Acquire the lock for reading with an option to upgrade to writing while holding the lock.
	/// </summary>
	UpgradeableRead,

	/// <summary>
	/// Acquire the lock for reading and writing.
	/// </summary>
	ReadWrite
}
