///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
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

using System;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading
{
	/// <summary>
	/// Provides extension methods for task factories.
	/// </summary>
	public static class TaskFactoryExtensions
	{
		/// <summary>
		/// Queues work to the task factory and returns a <see cref="Task"/> representing that work.
		/// If the task factory does not specify a task scheduler, the thread pool task scheduler is used.
		/// </summary>
		/// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
		/// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
		/// <returns>The started task.</returns>
		public static Task Run(this TaskFactory @this, Action action)
		{
			if (@this == null) throw new ArgumentNullException(nameof(@this));
			if (action == null) throw new ArgumentNullException(nameof(action));

			return @this.StartNew(
				action,
				@this.CancellationToken,
				@this.CreationOptions | TaskCreationOptions.DenyChildAttach,
				@this.Scheduler ?? TaskScheduler.Default);
		}

		/// <summary>
		/// Queues work to the task factory and returns a <see cref="Task{TResult}"/> representing that work.
		/// If the task factory does not specify a task scheduler, the thread pool task scheduler is used.
		/// </summary>
		/// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
		/// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
		/// <returns>The started task.</returns>
		public static Task<TResult> Run<TResult>(this TaskFactory @this, Func<TResult> action)
		{
			if (@this == null) throw new ArgumentNullException(nameof(@this));
			if (action == null) throw new ArgumentNullException(nameof(action));

			return @this.StartNew(
				action,
				@this.CancellationToken,
				@this.CreationOptions | TaskCreationOptions.DenyChildAttach,
				@this.Scheduler ?? TaskScheduler.Default);
		}

		/// <summary>
		/// Queues work to the task factory and returns a proxy <see cref="Task"/> representing that work.
		/// If the task factory does not specify a task scheduler, the thread pool task scheduler is used.
		/// </summary>
		/// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
		/// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
		/// <returns>The started task.</returns>
		public static Task Run(this TaskFactory @this, Func<Task> action)
		{
			if (@this == null) throw new ArgumentNullException(nameof(@this));
			if (action == null) throw new ArgumentNullException(nameof(action));

			return @this.StartNew(
				action,
				@this.CancellationToken,
				@this.CreationOptions | TaskCreationOptions.DenyChildAttach,
				@this.Scheduler ?? TaskScheduler.Default).Unwrap();
		}

		/// <summary>
		/// Queues work to the task factory and returns a proxy <see cref="Task{TResult}"/> representing that work.
		/// If the task factory does not specify a task scheduler, the thread pool task scheduler is used.
		/// </summary>
		/// <param name="this">The <see cref="TaskFactory"/>. May not be <c>null</c>.</param>
		/// <param name="action">The action delegate to execute. May not be <c>null</c>.</param>
		/// <returns>The started task.</returns>
		public static Task<TResult> Run<TResult>(this TaskFactory @this, Func<Task<TResult>> action)
		{
			if (@this == null) throw new ArgumentNullException(nameof(@this));
			if (action == null) throw new ArgumentNullException(nameof(action));

			return @this.StartNew(
				action,
				@this.CancellationToken,
				@this.CreationOptions | TaskCreationOptions.DenyChildAttach,
				@this.Scheduler ?? TaskScheduler.Default).Unwrap();
		}
	}
}