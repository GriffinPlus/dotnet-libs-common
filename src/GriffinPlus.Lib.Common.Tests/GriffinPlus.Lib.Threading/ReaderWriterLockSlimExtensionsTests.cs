///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/GriffinPlus/dotnet-libs-common)
//
// Copyright 2019 Sascha Falk <sascha@falk-online.eu>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for
// the specific language governing permissions and limitations under the License.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GriffinPlus.Lib.Threading
{
	/// <summary>
	/// Unit tests targeting the <see cref="ReaderWriterLockSlimExtensions"/> class.
	/// </summary>
	public class ReaderWriterLockSlimExtensionsTests
	{
		private const int TIMEOUT = 100; // ms

		#region LockReadOnly()

		[Fact]
		public void LockReadOnly_WithoutTimeout()
		{
			ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

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
			ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			using (rwlock.LockReadOnly(TIMEOUT))
			{
				Assert.True(rwlock.IsReadLockHeld);
			}

			Assert.False(rwlock.IsReadLockHeld);
			Assert.False(rwlock.IsUpgradeableReadLockHeld);
			Assert.False(rwlock.IsWriteLockHeld);
		}

		[Fact]
		public async Task LockReadOnly_WithTimeout_Timeout()
		{
			ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			// let some other thread acquire the lock
			await Task.Run(() => rwlock.EnterWriteLock());

			// current thread should timeout now
			Assert.Throws<TimeoutException>(() => { rwlock.LockReadOnly(TIMEOUT); });
		}

		#endregion

		#region LockUpgradeableRead()

		[Fact]
		public void LockUpgradeableRead_WithoutTimeout()
		{
			ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

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
			ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

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
			ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			using (rwlock.LockUpgradeableRead(TIMEOUT))
			{
				Assert.True(rwlock.IsUpgradeableReadLockHeld);
			}

			Assert.False(rwlock.IsReadLockHeld);
			Assert.False(rwlock.IsUpgradeableReadLockHeld);
			Assert.False(rwlock.IsWriteLockHeld);
		}

		[Fact]
		public async Task LockUpgradeableRead_WithTimeout_Timeout()
		{
			ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			// let some other thread acquire the lock
			await Task.Run(() => rwlock.EnterWriteLock());

			// current thread should timeout now
			Assert.Throws<TimeoutException>(() => { rwlock.LockUpgradeableRead(TIMEOUT); });
		}

		#endregion

		#region LockReadWrite()

		[Fact]
		public void LockReadWrite_WithoutTimeout()
		{
			ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

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
			ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			using (rwlock.LockUpgradeableRead(TIMEOUT))
			{
				Assert.True(rwlock.IsUpgradeableReadLockHeld);
			}

			Assert.False(rwlock.IsReadLockHeld);
			Assert.False(rwlock.IsUpgradeableReadLockHeld);
			Assert.False(rwlock.IsWriteLockHeld);
		}

		[Fact]
		public async Task LockReadWrite_WithTimeout_Timeout()
		{
			ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			// let some other thread acquire the lock
			await Task.Run(() => rwlock.EnterWriteLock());

			// current thread should timeout now
			Assert.Throws<TimeoutException>(() => { rwlock.LockReadWrite(TIMEOUT); });
		}

		#endregion
	}
}
