﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Threading;

/// <summary>
/// Holds the task for a cancellation token, as well as the token registration.
/// The registration is disposed when this instance is disposed.
/// </summary>
public sealed class CancellationTokenTaskSource<T> : IDisposable
{
	/// <summary>
	/// The cancellation token registration, if any.
	/// This is <c>null</c> if the registration was not necessary.
	/// </summary>
	private readonly IDisposable mRegistration;

	/// <summary>
	/// Creates a task for the specified cancellation token, registering with the token if necessary.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to observe.</param>
	public CancellationTokenTaskSource(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			Task = System.Threading.Tasks.Task.FromCanceled<T>(cancellationToken);
			return;
		}

		var tcs = new TaskCompletionSource<T>();
		mRegistration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken), false);
		Task = tcs.Task;
	}

	/// <summary>
	/// Gets the task for the source cancellation token.
	/// </summary>
	public Task<T> Task { get; }

	/// <summary>
	/// Disposes the cancellation token registration, if any.
	/// Note that this may cause <see cref="Task"/> to never complete.
	/// </summary>
	public void Dispose()
	{
		mRegistration?.Dispose();
	}
}
