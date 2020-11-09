///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace GriffinPlus.Lib.Threading
{
	/// <summary>
	/// Extension methods for the <see cref="System.Threading.ReaderWriterLockSlim"/> class.
	/// </summary>
	public static class ReaderWriterLockSlimExtensions
	{
		/// <summary>
		/// Locks the reader-writer-lock for reading only
		/// and returns an auto-lock object that can be used to release the lock automatically at the end of a <c>using</c> statement.
		/// </summary>
		/// <param name="this">The lock to acquire.</param>
		/// <returns>The auto-lock object.</returns>
		public static ReaderWriterLockSlimAutoLock LockReadOnly(this ReaderWriterLockSlim @this)
		{
			return new ReaderWriterLockSlimAutoLock(@this, ReaderWriterLockSlimAcquireKind.Read);
		}

		/// <summary>
		/// Locks the reader-writer-lock for reading only
		/// and returns an auto-lock object that can be used to release the lock automatically at the end of a <c>using</c> statement (with timeout).
		/// </summary>
		/// <param name="this">The lock to acquire.</param>
		/// <param name="timeout">Time to wait for the lock (in ms, -1 to wait infinitely).</param>
		/// <returns>The auto-lock object.</returns>
		/// <exception cref="TimeoutException">The lock could not be acquired within the specified time.</exception>
		public static ReaderWriterLockSlimAutoLock LockReadOnly(this ReaderWriterLockSlim @this, int timeout)
		{
			return new ReaderWriterLockSlimAutoLock(@this, ReaderWriterLockSlimAcquireKind.Read, timeout);
		}

		/// <summary>
		/// Locks the reader-writer-lock for reading with option to upgrade to writing
		/// and returns an auto-lock object that can be used to release the lock automatically at the end of a <c>using</c> statement.
		/// </summary>
		/// <param name="this">The lock to acquire.</param>
		/// <returns>The auto-lock object.</returns>
		public static ReaderWriterLockSlimAutoLock LockUpgradeableRead(this ReaderWriterLockSlim @this)
		{
			return new ReaderWriterLockSlimAutoLock(@this, ReaderWriterLockSlimAcquireKind.UpgradeableRead);
		}

		/// <summary>
		/// Locks the reader-writer-lock for reading with option to upgrade to writing
		/// and returns an auto-lock object that can be used to release the lock automatically at the end of a <c>using</c> statement (with timeout).
		/// </summary>
		/// <param name="this">The lock to acquire.</param>
		/// <param name="timeout">Time to wait for the lock (in ms, -1 to wait infinitely).</param>
		/// <returns>The auto-lock object.</returns>
		/// <exception cref="TimeoutException">The lock could not be acquired within the specified time.</exception>
		public static ReaderWriterLockSlimAutoLock LockUpgradeableRead(this ReaderWriterLockSlim @this, int timeout)
		{
			return new ReaderWriterLockSlimAutoLock(@this, ReaderWriterLockSlimAcquireKind.UpgradeableRead, timeout);
		}

		/// <summary>
		/// Locks the reader-writer-lock for reading and writing
		/// and returns an auto-lock object that can be used to release the lock automatically at the end of a <c>using</c> statement.
		/// </summary>
		/// <param name="this">The lock to acquire.</param>
		/// <returns>The auto-lock object.</returns>
		public static ReaderWriterLockSlimAutoLock LockReadWrite(this ReaderWriterLockSlim @this)
		{
			return new ReaderWriterLockSlimAutoLock(@this, ReaderWriterLockSlimAcquireKind.ReadWrite);
		}

		/// <summary>
		/// Locks the reader-writer-lock for reading and writing
		/// and returns an auto-lock object that can be used to release the lock automatically at the end of a <c>using</c> statement (with timeout).
		/// </summary>
		/// <param name="this">The lock to acquire.</param>
		/// <param name="timeout">Time to wait for the lock (in ms, -1 to wait infinitely).</param>
		/// <returns>The auto-lock object.</returns>
		/// <exception cref="TimeoutException">The lock could not be acquired within the specified time.</exception>
		public static ReaderWriterLockSlimAutoLock LockReadWrite(this ReaderWriterLockSlim @this, int timeout)
		{
			return new ReaderWriterLockSlimAutoLock(@this, ReaderWriterLockSlimAcquireKind.ReadWrite, timeout);
		}

		/// <summary>
		/// Locks the reader-writer-lock
		/// and returns an auto-lock object that can be used to release the lock automatically at the end of a <c>using</c> statement.
		/// </summary>
		/// <param name="this">The lock to acquire.</param>
		/// <param name="kind">The kind of lock to acquire.</param>
		/// <returns>The auto-lock object.</returns>
		public static ReaderWriterLockSlimAutoLock Lock(this ReaderWriterLockSlim @this, ReaderWriterLockSlimAcquireKind kind)
		{
			return new ReaderWriterLockSlimAutoLock(@this, kind);
		}

		/// <summary>
		/// Locks the reader-writer-lock
		/// and returns an auto-lock object that can be used to release the lock automatically at the end of a <c>using</c> statement (with timeout).
		/// </summary>
		/// <param name="this">The lock to acquire.</param>
		/// <param name="kind">The kind of lock to acquire.</param>
		/// <param name="timeout">Time to wait for the lock (in ms, -1 to wait infinitely).</param>
		/// <returns>The auto-lock object.</returns>
		/// <exception cref="TimeoutException">The lock could not be acquired within the specified time.</exception>
		public static ReaderWriterLockSlimAutoLock Lock(this ReaderWriterLockSlim @this, ReaderWriterLockSlimAcquireKind kind, int timeout)
		{
			return new ReaderWriterLockSlimAutoLock(@this, kind, timeout);
		}
	}
}
