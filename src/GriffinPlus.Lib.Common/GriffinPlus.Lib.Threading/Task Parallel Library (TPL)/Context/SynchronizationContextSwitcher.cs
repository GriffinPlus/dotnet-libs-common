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
using System.Threading;

using GriffinPlus.Lib.Disposables;

namespace GriffinPlus.Lib.Threading;

/// <summary>
/// Utility class for temporarily switching <see cref="SynchronizationContext"/> implementations.
/// </summary>
public sealed class SynchronizationContextSwitcher : SingleDisposable<object>
{
	/// <summary>
	/// The previous <see cref="SynchronizationContext"/>.
	/// </summary>
	private readonly SynchronizationContext mOldContext;

	/// <summary>
	/// Initializes a new instance of the <see cref="SynchronizationContextSwitcher"/> class,
	/// installing the new <see cref="SynchronizationContext"/>.
	/// </summary>
	/// <param name="newContext">
	/// The new <see cref="SynchronizationContext"/>.
	/// This can be <c>null</c> to remove an existing <see cref="SynchronizationContext"/>.
	/// </param>
	private SynchronizationContextSwitcher(SynchronizationContext newContext)
		: base(new object())
	{
		mOldContext = SynchronizationContext.Current;
		SynchronizationContext.SetSynchronizationContext(newContext);
	}

	/// <summary>
	/// Restores the old <see cref="SynchronizationContext"/>.
	/// </summary>
	protected override void Dispose(object context)
	{
		SynchronizationContext.SetSynchronizationContext(mOldContext);
	}

	/// <summary>
	/// Executes a synchronous delegate without the current <see cref="SynchronizationContext"/>.
	/// The current context is restored, when this function returns.
	/// </summary>
	/// <param name="action">The delegate to execute.</param>
	public static void NoContext(Action action)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));

		using (new SynchronizationContextSwitcher(null))
		{
			action();
		}
	}

	/// <summary>
	/// Executes a synchronous or asynchronous delegate without the current <see cref="SynchronizationContext"/>.
	/// The current context is restored, when this function synchronously returns.
	/// </summary>
	/// <param name="action">The delegate to execute.</param>
	public static T NoContext<T>(Func<T> action)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));

		using (new SynchronizationContextSwitcher(null))
		{
			return action();
		}
	}

	/// <summary>
	/// Executes a synchronous delegate with the specified <see cref="SynchronizationContext"/> as "current".
	/// The previous current context is restored when this function returns.
	/// </summary>
	/// <param name="context">The context to treat as "current". May be <c>null</c> to indicate the thread pool context.</param>
	/// <param name="action">The delegate to execute.</param>
	public static void ApplyContext(SynchronizationContext context, Action action)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));

		using (new SynchronizationContextSwitcher(context))
		{
			action();
		}
	}

	/// <summary>
	/// Executes a synchronous or asynchronous delegate without the specified <see cref="SynchronizationContext"/> as "current".
	/// The previous current context is restored when this function synchronously returns.
	/// </summary>
	/// <param name="context">The context to treat as "current". May be <c>null</c> to indicate the thread pool context.</param>
	/// <param name="action">The delegate to execute.</param>
	public static T ApplyContext<T>(SynchronizationContext context, Func<T> action)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));

		using (new SynchronizationContextSwitcher(context))
		{
			return action();
		}
	}
}
