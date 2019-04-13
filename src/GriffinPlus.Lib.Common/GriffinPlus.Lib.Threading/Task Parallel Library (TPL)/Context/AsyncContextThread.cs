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

using GriffinPlus.Lib.Disposables;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading
{
	/// <summary>
	/// A thread that executes actions within an <see cref="AsyncContext"/>.
	/// </summary>
	[DebuggerTypeProxy(typeof(DebugView))]
	public sealed class AsyncContextThread : SingleDisposable<AsyncContext>
	{
		/// <summary>
		/// The child thread.
		/// </summary>
		private readonly Task mThread;

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncContextThread"/> class, creating a child thread waiting for commands.
		/// </summary>
		/// <param name="context">The context for this thread.</param>
		private AsyncContextThread(AsyncContext context)
			: base(context)
		{
			Context = context;
			mThread = Task.Factory.StartNew(
				() => { using (Context) Context.Execute(); },
				CancellationToken.None,
				TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach,
				TaskScheduler.Default);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncContextThread"/> class, creating a child thread waiting for commands.
		/// </summary>
		public AsyncContextThread()
			: this(CreateAsyncContext())
		{
		}

		/// <summary>
		/// Creates a new <see cref="AsyncContext"/> and increments its operation count.
		/// </summary>
		private static AsyncContext CreateAsyncContext()
		{
			var result = new AsyncContext();
			result.SynchronizationContext.OperationStarted();
			return result;
		}

		/// <summary>
		/// Gets the <see cref="AsyncContext"/> executed by this thread.
		/// </summary>
		public AsyncContext Context { get; }

		/// <summary>
		/// Permits the thread to exit, if we have not already done so.
		/// </summary>
		private void AllowThreadToExit()
		{
			Context.SynchronizationContext.OperationCompleted();
		}

		/// <summary>
		/// Requests the thread to exit and returns a task representing the exit of the thread.
		/// The thread will exit when all outstanding asynchronous operations complete.
		/// </summary>
		public Task JoinAsync()
		{
			Dispose();
			return mThread;
		}

		/// <summary>
		/// Requests the thread to exit and blocks until the thread exits.
		/// The thread will exit when all outstanding asynchronous operations complete.
		/// </summary>
		public void Join()
		{
			JoinAsync().WaitAndUnwrapException();
		}

		/// <summary>
		/// Requests the thread to exit.
		/// </summary>
		protected override void Dispose(AsyncContext context)
		{
			AllowThreadToExit();
		}

		/// <summary>
		/// Gets the <see cref="TaskFactory"/> for this thread, which can be used to schedule work to this thread.
		/// </summary>
		public TaskFactory Factory => Context.Factory;

		[DebuggerNonUserCode]
		internal sealed class DebugView
		{
			private readonly AsyncContextThread mThread;

			public DebugView(AsyncContextThread thread)
			{
				mThread = thread;
			}

			public AsyncContext Context => mThread.Context;

			public object Thread => mThread.mThread;
		}
	}
}