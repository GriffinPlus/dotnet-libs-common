﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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

namespace GriffinPlus.Lib.Threading
{
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
		ReadWrite,
	}
}