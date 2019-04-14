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
using System.Threading;

namespace GriffinPlus.Lib.Threading
{
	/// <summary>
	/// Helper class that enables a <see cref="ReaderWriterLockSlim"/> to be used in a <c>using</c> statement that
	/// ensures that the lock is released properly at the end of the <c>using</c> block.
	/// </summary>
	public struct ReaderWriterLockSlimAutoLock : IDisposable
	{
		/// <summary>
		/// The reader-writer-lock.
		/// </summary>
		public ReaderWriterLockSlim Lock { get; }

		/// <summary>
		/// The operation the lock was acquired for (read-only, upgradeable-read or read-write).
		/// </summary>
		public ReaderWriterLockSlimAcquireKind AcquireKind { get; }

		/// <summary>
		/// Indicates whether the lock is acquired.
		/// </summary>
		private bool mIsLockAcquired;

		/// <summary>
		/// Locks the specified reader-writer-lock for the reading, writing or reading-with-write-upgrade-option (without timeout).
		/// </summary>
		/// <param name="lock">The lock to acquire.</param>
		/// <param name="acquireKind">Determines how to acquire the lock.</param>
		public ReaderWriterLockSlimAutoLock(ReaderWriterLockSlim @lock, ReaderWriterLockSlimAcquireKind acquireKind)
		{
			Lock = @lock;
			AcquireKind = acquireKind;
			mIsLockAcquired = true;
			switch (AcquireKind)
			{
				case ReaderWriterLockSlimAcquireKind.Read:
					Lock.EnterReadLock();
					break;
				case ReaderWriterLockSlimAcquireKind.UpgradeableRead:
					Lock.EnterUpgradeableReadLock();
					break;
				case ReaderWriterLockSlimAcquireKind.ReadWrite:
					Lock.EnterWriteLock();
					break;
				default:
					throw new ArgumentException("Invalid acquire type.", nameof (acquireKind));
			}
		}

		/// <summary>
		/// Locks the specified reader-writer-lock for the reading, writing or reading-with-write-upgrade-option (with timeout).
		/// </summary>
		/// <param name="lock">The lock to acquire.</param>
		/// <param name="acquireKind">Determines how to acquire the lock.</param>
		/// <param name="timeout">Time to wait for the lock (in ms, -1 to wait infinitely).</param>
		/// <exception cref="TimeoutException">The lock could not be acquired within the specified time.</exception>
		public ReaderWriterLockSlimAutoLock(ReaderWriterLockSlim @lock, ReaderWriterLockSlimAcquireKind acquireKind, int timeout)
		{
			Lock = @lock;
			AcquireKind = acquireKind;

			switch (AcquireKind)
			{
				case ReaderWriterLockSlimAcquireKind.Read:
					mIsLockAcquired = Lock.TryEnterReadLock(timeout);
					break;
				case ReaderWriterLockSlimAcquireKind.UpgradeableRead:
					mIsLockAcquired = Lock.TryEnterUpgradeableReadLock(timeout);
					break;
				case ReaderWriterLockSlimAcquireKind.ReadWrite:
					mIsLockAcquired = Lock.TryEnterWriteLock(timeout);
					break;
				default:
					throw new ArgumentException("Invalid acquire type.", nameof (acquireKind));
			}

			if (!mIsLockAcquired) throw new TimeoutException("The locked could not be acquired within the specified time.");
		}

		/// <summary>
		/// Releases the lock.
		/// </summary>
		public void Dispose()
		{
			if (mIsLockAcquired)
			{
				switch (AcquireKind)
				{
					case ReaderWriterLockSlimAcquireKind.Read:
						Lock.ExitReadLock();
						break;
					case ReaderWriterLockSlimAcquireKind.UpgradeableRead:
						Lock.ExitUpgradeableReadLock();
						break;
					case ReaderWriterLockSlimAcquireKind.ReadWrite:
						Lock.ExitWriteLock();
						break;
				}

				mIsLockAcquired = false;
			}
		}
	}
}
