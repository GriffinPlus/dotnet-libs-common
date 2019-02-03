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
//     Copyright (c) 2014-2018 Stephen Cleary
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
	/// Provides completed task constants.
	/// </summary>
	public static class TaskConstants
	{
		/// <summary>
		/// A task that has been completed with the value <c>true</c>.
		/// </summary>
		public static Task<bool> BooleanTrue { get; } = Task.FromResult(true);

		/// <summary>
		/// A task that has been completed with the value <c>false</c>.
		/// </summary>
		public static Task<bool> BooleanFalse { get; } = TaskConstants<bool>.Default;

		/// <summary>
		/// A task that has been completed with the value <c>0</c>.
		/// </summary>
		public static Task<int> Int32Zero { get; } = TaskConstants<int>.Default;

		/// <summary>
		/// A task that has been completed with the value <c>-1</c>.
		/// </summary>
		public static Task<int> Int32NegativeOne { get; } = Task.FromResult(-1);

		/// <summary>
		/// A <see cref="Task"/> that has been completed.
		/// </summary>
		public static Task Completed { get; } = Task.CompletedTask;

		/// <summary>
		/// A task that has been canceled.
		/// </summary>
		public static Task Canceled { get; } = TaskConstants<object>.Canceled;
	}

	/// <summary>
	/// Provides completed task constants.
	/// </summary>
	/// <typeparam name="T">The type of the task result.</typeparam>
	public static class TaskConstants<T>
	{
		/// <summary>
		/// A task that has been completed with the default value of <typeparamref name="T"/>.
		/// </summary>
		public static Task<T> Default { get; } = Task.FromResult(default(T));

		/// <summary>
		/// A task that has been canceled.
		/// </summary>
		public static Task<T> Canceled { get; } = Task.FromCanceled<T>(new CancellationToken(true));
	}
}