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

namespace GriffinPlus.Lib.Disposables
{
	public class SingleNonblockingDisposableUnitTests
	{
		[Fact]
		public void ConstructedWithContext_DisposeReceivesThatContext()
		{
			var providedContext = new object();
			object seenContext = null;
			var disposable = new DelegateSingleDisposable<object>(providedContext, context => { seenContext = context; });
			disposable.Dispose();
			Assert.Same(providedContext, seenContext);
		}

		[Fact]
		public void DisposeOnlyCalledOnce()
		{
			var counter = 0;
			var disposable = new DelegateSingleDisposable<object>(new object(), _ => { ++counter; });
			disposable.Dispose();
			disposable.Dispose();
			Assert.Equal(1, counter);
		}

		[Fact]
		public async Task DisposeIsNonblocking()
		{
			var ready = new ManualResetEventSlim();
			var signal = new ManualResetEventSlim();
			var disposable = new DelegateSingleDisposable<object>(new object(), _ =>
			{
				ready.Set();
				signal.Wait();
			});

			var task1 = Task.Run(() => disposable.Dispose());
			ready.Wait();

			await Task.Run(() => disposable.Dispose());

			signal.Set();
			await task1;
		}

		private sealed class DelegateSingleDisposable<T> : SingleNonblockingDisposable<T>
			where T : class
		{
			private readonly Action<T> mCallback;

			public DelegateSingleDisposable(T context, Action<T> callback)
				: base(context)
			{
				mCallback = callback;
			}

			protected override void Dispose(T context)
			{
				mCallback(context);
			}
		}
	}
}