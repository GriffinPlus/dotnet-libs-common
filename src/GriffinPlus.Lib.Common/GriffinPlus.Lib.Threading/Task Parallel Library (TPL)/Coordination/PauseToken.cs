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
//
// This file incorporates work covered by the following copyright and permission notice:
//
//     MIT License
//
//     Copyright (c) 2019 Stephen Cleary
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in all
//     copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//     SOFTWARE.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading
{
	/// <summary>
	/// The source (controller) of a "pause token", which can be used to cooperatively pause and unpause operations.
	/// </summary>
	public sealed class PauseTokenSource
	{
		/// <summary>
		/// The manual-reset-event that manages the "pause" logic.
		/// When the manual-reset-event is set, the token is not paused; when the manual-reset-event is not set, the token is paused.
		/// </summary>
		private readonly AsyncManualResetEvent mManualResetEvent = new AsyncManualResetEvent(true);

		/// <summary>
		/// Whether or not this source (and its tokens) are in the paused state.
		/// This member is seldom used; code using this member has a high possibility of race conditions.
		/// </summary>
		public bool IsPaused
		{
			get => !mManualResetEvent.IsSet;
			set
			{
				if (value)
					mManualResetEvent.Reset();
				else
					mManualResetEvent.Set();
			}
		}

		/// <summary>
		/// Gets a pause token controlled by this source.
		/// </summary>
		public PauseToken Token => new PauseToken(mManualResetEvent);
	}

	/// <summary>
	/// A type that allows an operation to be cooperatively paused.
	/// </summary>
	public struct PauseToken
	{
		/// <summary>
		/// The manual-reset-event that manages the "pause" logic, or <c>null</c> if this token can never be paused.
		/// When the manual-reset-event is set, the token is not paused; when the manual-reset-event is not set, the token is paused.
		/// </summary>
		private readonly AsyncManualResetEvent mManualResetEvent;

		internal PauseToken(AsyncManualResetEvent mre)
		{
			mManualResetEvent = mre;
		}

		/// <summary>
		/// Whether this token can ever possibly be paused.
		/// </summary>
		public bool CanBePaused => mManualResetEvent != null;

		/// <summary>
		/// Whether or not this token is in the paused state.
		/// </summary>
		public bool IsPaused => mManualResetEvent != null && !mManualResetEvent.IsSet;

		/// <summary>
		/// Asynchronously waits until the pause token is not paused.
		/// </summary>
		/// <param name="token">
		/// The cancellation token to observe.
		/// If the token is already canceled, this method will first check if the pause token is unpaused, and will return without an exception in that case.
		/// </param>
		public Task WaitWhilePausedAsync(CancellationToken token = default(CancellationToken))
		{
			if (mManualResetEvent == null)
				return TaskConstants.Completed;

			return mManualResetEvent.WaitAsync(token);
		}

		/// <summary>
		/// Synchronously waits until the pause token is not paused.
		/// </summary>
		public void WaitWhilePaused()
		{
			mManualResetEvent?.Wait();
		}

		/// <summary>
		/// Synchronously waits until the pause token is not paused, or until this wait is canceled by the cancellation token.
		/// </summary>
		/// <param name="token">
		/// The cancellation token to observe.
		/// If the token is already canceled, this method will first check if the pause token is unpaused, and will return without an exception in that case.
		/// </param>
		public void WaitWhilePaused(CancellationToken token)
		{
			mManualResetEvent?.Wait(token);
		}
	}
}
