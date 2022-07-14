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
//     Copyright (c) 2016-2018 Stephen Cleary
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

using System;

namespace GriffinPlus.Lib.Disposables
{

	/// <summary>
	/// A disposable that executes a delegate when disposed.
	/// </summary>
	public sealed class AnonymousDisposable : SingleDisposable<Action>
	{
		/// <summary>
		/// Creates a new disposable that executes <paramref name="dispose"/> when disposed.
		/// </summary>
		/// <param name="dispose">
		/// The delegate to execute when disposed.
		/// If this is <c>null</c>, then this instance does nothing when it is disposed.
		/// </param>
		public AnonymousDisposable(Action dispose)
			: base(dispose) { }

		/// <inheritdoc/>
		protected override void Dispose(Action context)
		{
			context?.Invoke();
		}

		/// <summary>
		/// Adds a delegate to be executed when this instance is disposed.
		/// If this instance is already disposed or disposing, then <paramref name="dispose"/> is executed immediately.
		/// </summary>
		/// <param name="dispose">The delegate to add. May be <c>null</c> to indicate no additional action.</param>
		public void Add(Action dispose)
		{
			if (dispose == null)
				return;

			if (!TryUpdateContext(x => x + dispose))
				dispose();
		}

		/// <summary>
		/// Creates a new disposable that executes <paramref name="dispose"/> when disposed.
		/// </summary>
		/// <param name="dispose">The delegate to execute when disposed. May not be <c>null</c>.</param>
		public static AnonymousDisposable Create(Action dispose)
		{
			return new AnonymousDisposable(dispose);
		}
	}

}
