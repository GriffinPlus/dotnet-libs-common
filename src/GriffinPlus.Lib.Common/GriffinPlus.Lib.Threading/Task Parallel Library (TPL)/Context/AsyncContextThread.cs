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

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using GriffinPlus.Lib.Disposables;

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
				() =>
				{
					using (Context)
					{
						Context.Execute();
					}
				},
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
