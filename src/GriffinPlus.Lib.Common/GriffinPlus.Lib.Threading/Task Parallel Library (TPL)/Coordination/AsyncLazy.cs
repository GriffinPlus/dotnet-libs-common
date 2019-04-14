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

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading
{
	/// <summary>
	/// Flags controlling the behavior of <see cref="AsyncLazy{T}"/>.
	/// </summary>
	[Flags]
	public enum AsyncLazyFlags
	{
		/// <summary>
		/// No special flags.
		/// The factory method is executed on a thread pool thread, and does not retry initialization on failures (failures are cached).
		/// </summary>
		None = 0x0,

		/// <summary>
		/// Execute the factory method on the calling thread.
		/// </summary>
		ExecuteOnCallingThread = 0x1,

		/// <summary>
		/// If the factory method fails, then re-run the factory method the next time instead of caching the failed task.
		/// </summary>
		RetryOnFailure = 0x2,
	}

	/// <summary>
	/// Provides support for asynchronous lazy initialization.
	/// This type is fully thread-safe.
	/// </summary>
	/// <typeparam name="T">The type of object that is being asynchronously initialized.</typeparam>
	[DebuggerDisplay("Id = {Id}, State = {GetStateForDebugger}")]
	[DebuggerTypeProxy(typeof(AsyncLazy<>.DebugView))]
	public sealed class AsyncLazy<T>
	{
		/// <summary>
		/// The synchronization object protecting <c>mInstance</c>.
		/// </summary>
		private readonly object mMutex;

		/// <summary>
		/// The factory method to call.
		/// </summary>
		private readonly Func<Task<T>> mFactory;

		/// <summary>
		/// The underlying lazy task.
		/// </summary>
		private Lazy<Task<T>> mInstance;

		/// <summary>
		/// The semi-unique identifier for this instance.
		/// This is 0 if the id has not yet been created.
		/// </summary>
		private int mId;

		[DebuggerNonUserCode]
		internal LazyState GetStateForDebugger
		{
			get
			{
				if (!mInstance.IsValueCreated) return LazyState.NotStarted;
				if (!mInstance.Value.IsCompleted) return LazyState.Executing;
				return LazyState.Completed;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncLazy{T}"/> class.
		/// </summary>
		/// <param name="factory">
		/// The asynchronous delegate that is invoked to produce the value when it is needed.
		/// May not be <c>null</c>.
		/// </param>
		/// <param name="flags">Flags to influence async lazy semantics.</param>
		public AsyncLazy(Func<Task<T>> factory, AsyncLazyFlags flags = AsyncLazyFlags.None)
		{
			mFactory = factory ?? throw new ArgumentNullException(nameof(factory));
			if ((flags & AsyncLazyFlags.RetryOnFailure) == AsyncLazyFlags.RetryOnFailure)
				mFactory = RetryOnFailure(mFactory);
			if ((flags & AsyncLazyFlags.ExecuteOnCallingThread) != AsyncLazyFlags.ExecuteOnCallingThread)
				mFactory = RunOnThreadPool(mFactory);

			mMutex = new object();
			mInstance = new Lazy<Task<T>>(mFactory);
		}

		/// <summary>
		/// Gets a semi-unique identifier for this asynchronous lazy instance.
		/// </summary>
		public int Id => IdManager<AsyncLazy<object>>.GetId(ref mId);

		/// <summary>
		/// Whether the asynchronous factory method has started.
		/// This is initially <c>false</c> and becomes <c>true</c> when this instance is awaited or after <see cref="Start"/> is called.
		/// </summary>
		public bool IsStarted
		{
			get
			{
				lock (mMutex)
					return mInstance.IsValueCreated;
			}
		}

		/// <summary>
		/// Starts the asynchronous factory method, if it has not already started, and returns the resulting task.
		/// </summary>
		public Task<T> Task
		{
			get
			{
				lock (mMutex)
					return mInstance.Value;
			}
		}

		private Func<Task<T>> RetryOnFailure(Func<Task<T>> factory)
		{
			return async () =>
			{
				try
				{
					return await factory().ConfigureAwait(false);
				}
				catch
				{
					lock (mMutex)
					{
						mInstance = new Lazy<Task<T>>(mFactory);
					}
					throw;
				}
			};
		}

		private Func<Task<T>> RunOnThreadPool(Func<Task<T>> factory)
		{
			return () => System.Threading.Tasks.Task.Run(factory);
		}

		/// <summary>
		/// Asynchronous infrastructure support.
		/// This method permits instances of <see cref="AsyncLazy{T}"/> to be awaited.
		/// </summary>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public TaskAwaiter<T> GetAwaiter()
		{
			return Task.GetAwaiter();
		}

		/// <summary>
		/// Asynchronous infrastructure support.
		/// This method permits instances of <see cref="AsyncLazy{T}"/> to be awaited.
		/// </summary>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
		{
			return Task.ConfigureAwait(continueOnCapturedContext);
		}

		/// <summary>
		/// Starts the asynchronous initialization, if it has not already started.
		/// </summary>
		public void Start()
		{
			var unused = Task;
		}

		internal enum LazyState
		{
			NotStarted,
			Executing,
			Completed
		}

		[DebuggerNonUserCode]
		internal sealed class DebugView
		{
			private readonly AsyncLazy<T> mLazy;

			public DebugView(AsyncLazy<T> lazy)
			{
				mLazy = lazy;
			}

			public LazyState State => mLazy.GetStateForDebugger;

			public Task Task
			{
				get
				{
					if (!mLazy.mInstance.IsValueCreated)
						throw new InvalidOperationException("Not yet created.");

					return mLazy.mInstance.Value;
				}
			}

			public T Value
			{
				get
				{
					if (!mLazy.mInstance.IsValueCreated || !mLazy.mInstance.Value.IsCompleted)
						throw new InvalidOperationException("Not yet created.");

					return mLazy.mInstance.Value.Result;
				}
			}
		}
	}
}
