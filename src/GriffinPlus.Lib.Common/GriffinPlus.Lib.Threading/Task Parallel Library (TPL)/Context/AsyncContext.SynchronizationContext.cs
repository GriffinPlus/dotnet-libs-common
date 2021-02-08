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

using System.Threading;

namespace GriffinPlus.Lib.Threading
{

	public sealed partial class AsyncContext
	{
		/// <summary>
		/// The <see cref="SynchronizationContext"/> implementation used by <see cref="AsyncContext"/>.
		/// </summary>
		private sealed class AsyncContextSynchronizationContext : SynchronizationContext
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="AsyncContextSynchronizationContext"/> class.
			/// </summary>
			/// <param name="context">The async context.</param>
			public AsyncContextSynchronizationContext(AsyncContext context)
			{
				Context = context;
			}

			/// <summary>
			/// Gets the async context.
			/// </summary>
			public AsyncContext Context { get; }

			/// <summary>
			/// Dispatches an asynchronous message to the async context.
			/// If all tasks have been completed and the outstanding asynchronous operation count is zero, then this method has undefined behavior.
			/// </summary>
			/// <param name="d">The <see cref="System.Threading.SendOrPostCallback"/> delegate to call. May not be <c>null</c>.</param>
			/// <param name="state">The object passed to the delegate.</param>
			public override void Post(SendOrPostCallback d, object state)
			{
				Context.Enqueue(Context.mTaskFactory.Run(() => d(state)), true);
			}

			/// <summary>
			/// Dispatches an asynchronous message to the async context, and waits for it to complete.
			/// </summary>
			/// <param name="d">The <see cref="System.Threading.SendOrPostCallback"/> delegate to call. May not be <c>null</c>.</param>
			/// <param name="state">The object passed to the delegate.</param>
			public override void Send(SendOrPostCallback d, object state)
			{
				if (AsyncContext.Current == Context)
				{
					d(state);
				}
				else
				{
					var task = Context.mTaskFactory.Run(() => d(state));
					task.WaitAndUnwrapException();
				}
			}

			/// <summary>
			/// Responds to the notification that an operation has started by incrementing the outstanding asynchronous operation count.
			/// </summary>
			public override void OperationStarted()
			{
				Context.OperationStarted();
			}

			/// <summary>
			/// Responds to the notification that an operation has completed by decrementing the outstanding asynchronous operation count.
			/// </summary>
			public override void OperationCompleted()
			{
				Context.OperationCompleted();
			}

			/// <summary>
			/// Creates a copy of the synchronization context.
			/// </summary>
			/// <returns>A new <see cref="System.Threading.SynchronizationContext"/> object.</returns>
			public override SynchronizationContext CreateCopy()
			{
				return new AsyncContextSynchronizationContext(Context);
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <returns>
			/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
			/// </returns>
			public override int GetHashCode()
			{
				return Context.GetHashCode();
			}

			/// <summary>
			/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
			/// It is considered equal if it refers to the same underlying async context as this instance.
			/// </summary>
			/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
			/// <returns>
			/// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance;
			/// otherwise, <c>false</c>.
			/// </returns>
			public override bool Equals(object obj)
			{
				var other = obj as AsyncContextSynchronizationContext;
				if (other == null) return false;
				return Context == other.Context;
			}
		}
	}

}
