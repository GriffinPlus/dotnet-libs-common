///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
//
// This file incorporates work covered by the following copyright and permission notice:
//
//     MIT License
//
//     Copyright (c) 2018 Stephen Cleary
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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading
{

	/// <summary>
	/// An awaitable wrapper around a task whose result is disposable.
	/// The wrapper is not disposable, so this prevents usage errors like "using (MyAsync())" when the appropriate usage
	/// should be "using (await MyAsync())".
	/// </summary>
	/// <typeparam name="T">The type of the result of the underlying task.</typeparam>
	public readonly struct AwaitableDisposable<T> where T : IDisposable
	{
		/// <summary>
		/// The underlying task.
		/// </summary>
		private readonly Task<T> mTask;

		/// <summary>
		/// Initializes a new awaitable wrapper around the specified task.
		/// </summary>
		/// <param name="task">The underlying task to wrap. This may not be <c>null</c>.</param>
		public AwaitableDisposable(Task<T> task)
		{
			mTask = task ?? throw new ArgumentNullException(nameof(task));
		}

		/// <summary>
		/// Returns the underlying task.
		/// </summary>
		public Task<T> AsTask()
		{
			return mTask;
		}

		/// <summary>
		/// Implicit conversion to the underlying task.
		/// </summary>
		/// <param name="source">The awaitable wrapper.</param>
		public static implicit operator Task<T>(AwaitableDisposable<T> source)
		{
			return source.AsTask();
		}

		/// <summary>
		/// Infrastructure. Returns the task awaiter for the underlying task.
		/// </summary>
		public TaskAwaiter<T> GetAwaiter()
		{
			return mTask.GetAwaiter();
		}

		/// <summary>
		/// Infrastructure. Returns a configured task awaiter for the underlying task.
		/// </summary>
		/// <param name="continueOnCapturedContext">Whether to attempt to marshal the continuation back to the captured context.</param>
		public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
		{
			return mTask.ConfigureAwait(continueOnCapturedContext);
		}
	}

}
