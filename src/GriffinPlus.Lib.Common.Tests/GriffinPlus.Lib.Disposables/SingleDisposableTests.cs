///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
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
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace GriffinPlus.Lib.Disposables;

public class SingleDisposableUnitTests
{
	[Fact]
	public void ConstructedWithContext_DisposeReceivesThatContext()
	{
		object providedContext = new();
		object seenContext = null;
		var disposable = new DelegateSingleDisposable<object>(providedContext, context => { seenContext = context; });
		disposable.Dispose();
		Assert.Same(providedContext, seenContext);
	}

	[Fact]
	public void DisposeOnlyCalledOnce()
	{
		int counter = 0;
		var disposable = new DelegateSingleDisposable<object>(new object(), _ => { ++counter; });
		disposable.Dispose();
		disposable.Dispose();
		Assert.Equal(1, counter);
	}

	[Fact]
	public async Task DisposableWaitsForDisposeToComplete()
	{
		var ready = new ManualResetEventSlim();
		var signal = new ManualResetEventSlim();
		var disposable = new DelegateSingleDisposable<object>(
			new object(),
			_ =>
			{
				ready.Set();
				signal.Wait();
			});

		Task task1 = Task.Run(() => disposable.Dispose());
		ready.Wait();

		Task task2 = Task.Run(() => disposable.Dispose());
		Task timer = Task.Delay(500);
		Assert.Same(timer, await Task.WhenAny(task1, task2, timer));

		signal.Set();
		await task1;
		await task2;
	}

	private sealed class DelegateSingleDisposable<T>(T context, Action<T> callback) : SingleDisposable<T>(context)
		where T : class
	{
		protected override void Dispose(T context)
		{
			callback(context);
		}
	}
}
