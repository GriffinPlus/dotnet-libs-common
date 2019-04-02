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
			ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
			ReaderWriterLockSlimAutoLock autolock = new ReaderWriterLockSlimAutoLock(rwlock, kind);

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
			ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
			ReaderWriterLockSlimAutoLock autolock = new ReaderWriterLockSlimAutoLock(rwlock, kind);
			autolock.Dispose();

			Assert.Same(rwlock, autolock.Lock);
			Assert.Equal(kind, autolock.AcquireKind);
			Assert.False(rwlock.IsReadLockHeld);
			Assert.False(rwlock.IsUpgradeableReadLockHeld);
			Assert.False(rwlock.IsWriteLockHeld);
		}
	}
}
