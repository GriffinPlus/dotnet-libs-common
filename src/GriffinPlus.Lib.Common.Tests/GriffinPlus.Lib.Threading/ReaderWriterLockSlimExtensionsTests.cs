///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using Xunit;

namespace GriffinPlus.Lib.Threading
{

	/// <summary>
	/// Unit tests targeting the <see cref="ReaderWriterLockSlimExtensions"/> class.
	/// </summary>
	public class ReaderWriterLockSlimExtensionsTests
	{
		private const int Timeout = 1000; // ms

		#region LockReadOnly()

		[Fact]
		public void LockReadOnly_WithoutTimeout()
		{
			var rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			using (rwlock.LockReadOnly())
			{
				Assert.True(rwlock.IsReadLockHeld);
			}

			Assert.False(rwlock.IsReadLockHeld);
			Assert.False(rwlock.IsUpgradeableReadLockHeld);
			Assert.False(rwlock.IsWriteLockHeld);
		}

		[Fact]
		public void LockReadOnly_WithTimeout_Success()
		{
			var rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			using (rwlock.LockReadOnly(Timeout))
			{
				Assert.True(rwlock.IsReadLockHeld);
			}

			Assert.False(rwlock.IsReadLockHeld);
			Assert.False(rwlock.IsUpgradeableReadLockHeld);
			Assert.False(rwlock.IsWriteLockHeld);
		}

		[Fact]
		public void LockReadOnly_WithTimeout_Timeout()
		{
			var rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			using (var acquired = new SemaphoreSlim(0))
			{
				// let some other thread acquire the lock
				ThreadPool.QueueUserWorkItem(
					acquiredSemaphore =>
					{
						rwlock.EnterWriteLock();
						((SemaphoreSlim)acquiredSemaphore).Release();
					},
					acquired);

				// wait for the thread to acquire the lock
				acquired.Wait();

				// current thread should timeout now
				Assert.Throws<TimeoutException>(() => { rwlock.LockReadOnly(Timeout); });
			}
		}

		#endregion

		#region LockUpgradeableRead()

		[Fact]
		public void LockUpgradeableRead_WithoutTimeout()
		{
			var rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			using (rwlock.LockUpgradeableRead())
			{
				Assert.True(rwlock.IsUpgradeableReadLockHeld);
			}

			Assert.False(rwlock.IsReadLockHeld);
			Assert.False(rwlock.IsUpgradeableReadLockHeld);
			Assert.False(rwlock.IsWriteLockHeld);
		}

		[Fact]
		public void LockUpgradeableRead_WithoutTimeout_WithUpgradeToWrite()
		{
			var rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			using (rwlock.LockUpgradeableRead())
			{
				Assert.False(rwlock.IsReadLockHeld);
				Assert.True(rwlock.IsUpgradeableReadLockHeld);
				Assert.False(rwlock.IsWriteLockHeld);

				using (rwlock.LockReadWrite())
				{
					Assert.False(rwlock.IsReadLockHeld);
					Assert.True(rwlock.IsUpgradeableReadLockHeld);
					Assert.True(rwlock.IsWriteLockHeld);
				}
			}

			Assert.False(rwlock.IsReadLockHeld);
			Assert.False(rwlock.IsUpgradeableReadLockHeld);
			Assert.False(rwlock.IsWriteLockHeld);
		}

		[Fact]
		public void LockUpgradeableRead_WithTimeout_Success()
		{
			var rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			using (rwlock.LockUpgradeableRead(Timeout))
			{
				Assert.True(rwlock.IsUpgradeableReadLockHeld);
			}

			Assert.False(rwlock.IsReadLockHeld);
			Assert.False(rwlock.IsUpgradeableReadLockHeld);
			Assert.False(rwlock.IsWriteLockHeld);
		}

		[Fact]
		public void LockUpgradeableRead_WithTimeout_Timeout()
		{
			var rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			using (var acquired = new SemaphoreSlim(0))
			{
				// let some other thread acquire the lock
				ThreadPool.QueueUserWorkItem(
					acquiredSemaphore =>
					{
						rwlock.EnterWriteLock();
						((SemaphoreSlim)acquiredSemaphore).Release();
					},
					acquired);

				// wait for the thread to acquire the lock
				acquired.Wait();

				// current thread should timeout now
				Assert.Throws<TimeoutException>(() => { rwlock.LockUpgradeableRead(Timeout); });
			}
		}

		#endregion

		#region LockReadWrite()

		[Fact]
		public void LockReadWrite_WithoutTimeout()
		{
			var rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			using (rwlock.LockReadWrite())
			{
				Assert.True(rwlock.IsWriteLockHeld);
			}

			Assert.False(rwlock.IsReadLockHeld);
			Assert.False(rwlock.IsUpgradeableReadLockHeld);
			Assert.False(rwlock.IsWriteLockHeld);
		}

		[Fact]
		public void LockReadWrite_WithTimeout_Success()
		{
			var rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			using (rwlock.LockUpgradeableRead(Timeout))
			{
				Assert.True(rwlock.IsUpgradeableReadLockHeld);
			}

			Assert.False(rwlock.IsReadLockHeld);
			Assert.False(rwlock.IsUpgradeableReadLockHeld);
			Assert.False(rwlock.IsWriteLockHeld);
		}

		[Fact]
		public void LockReadWrite_WithTimeout_Timeout()
		{
			var rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			using (var acquired = new SemaphoreSlim(0))
			{
				// let some other thread acquire the lock
				ThreadPool.QueueUserWorkItem(
					acquiredSemaphore =>
					{
						rwlock.EnterWriteLock();
						((SemaphoreSlim)acquiredSemaphore).Release();
					},
					acquired);

				// wait for the thread to acquire the lock
				acquired.Wait();

				// current thread should timeout now
				Assert.Throws<TimeoutException>(() => { rwlock.LockReadWrite(Timeout); });
			}
		}

		#endregion
	}

}
