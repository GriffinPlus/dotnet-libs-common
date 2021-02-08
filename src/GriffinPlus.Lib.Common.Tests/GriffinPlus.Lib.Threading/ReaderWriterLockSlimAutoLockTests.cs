///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

using Xunit;

namespace GriffinPlus.Lib.Threading
{

	/// <summary>
	/// Unit tests targeting the <see cref="ReaderWriterLockSlimAutoLock"/> struct.
	/// </summary>
	public class ReaderWriterLockSlimAutoLockTests
	{
		[Theory]
		[InlineData(ReaderWriterLockSlimAcquireKind.Read)]
		[InlineData(ReaderWriterLockSlimAcquireKind.UpgradeableRead)]
		[InlineData(ReaderWriterLockSlimAcquireKind.ReadWrite)]
		public void Create(ReaderWriterLockSlimAcquireKind kind)
		{
			var rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
			var autolock = new ReaderWriterLockSlimAutoLock(rwlock, kind);

			Assert.Same(rwlock, autolock.Lock);
			Assert.Equal(kind, autolock.AcquireKind);
			Assert.Equal(kind == ReaderWriterLockSlimAcquireKind.Read, rwlock.IsReadLockHeld);
			Assert.Equal(kind == ReaderWriterLockSlimAcquireKind.UpgradeableRead, rwlock.IsUpgradeableReadLockHeld);
			Assert.Equal(kind == ReaderWriterLockSlimAcquireKind.ReadWrite, rwlock.IsWriteLockHeld);
		}

		[Theory]
		[InlineData(ReaderWriterLockSlimAcquireKind.Read)]
		[InlineData(ReaderWriterLockSlimAcquireKind.UpgradeableRead)]
		[InlineData(ReaderWriterLockSlimAcquireKind.ReadWrite)]
		public void Dispose(ReaderWriterLockSlimAcquireKind kind)
		{
			var rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
			var autolock = new ReaderWriterLockSlimAutoLock(rwlock, kind);
			autolock.Dispose();

			Assert.Same(rwlock, autolock.Lock);
			Assert.Equal(kind, autolock.AcquireKind);
			Assert.False(rwlock.IsReadLockHeld);
			Assert.False(rwlock.IsUpgradeableReadLockHeld);
			Assert.False(rwlock.IsWriteLockHeld);
		}
	}

}
