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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace GriffinPlus.Lib.Threading
{

	public class TaskExtensionsTests
	{
		#region Waiting for Task (Synchronous)

		[Fact]
		public void WaitAndUnwrapException_Completed_DoesNotBlock()
		{
			TaskConstants.Completed.WaitAndUnwrapException();
		}

		[Fact]
		public void WaitAndUnwrapException_Faulted_UnwrapsException()
		{
			var task = Task.Run(() => { throw new NotImplementedException(); });
			Assert.Throws<NotImplementedException>(() => task.WaitAndUnwrapException());
		}

		[Fact]
		public void WaitAndUnwrapExceptionWithCT_Completed_DoesNotBlock()
		{
			var cts = new CancellationTokenSource();
			TaskConstants.Completed.WaitAndUnwrapException(cts.Token);
		}

		[Fact]
		public void WaitAndUnwrapExceptionWithCT_Faulted_UnwrapsException()
		{
			var cts = new CancellationTokenSource();
			var task = Task.Run(() => { throw new NotImplementedException(); });
			Assert.Throws<NotImplementedException>(() => task.WaitAndUnwrapException(cts.Token));
		}

		[Fact]
		public void WaitAndUnwrapExceptionWithCT_CancellationTokenCancelled_Cancels()
		{
			var tcs = new TaskCompletionSource<object>();
			Task task = tcs.Task;
			var cts = new CancellationTokenSource();
			cts.Cancel();
			Assert.Throws<OperationCanceledException>(() => task.WaitAndUnwrapException(cts.Token));
		}

		[Fact]
		public void WaitAndUnwrapExceptionResult_Completed_DoesNotBlock()
		{
			TaskConstants.Int32Zero.WaitAndUnwrapException();
		}

		[Fact]
		public void WaitAndUnwrapExceptionResult_Faulted_UnwrapsException()
		{
			var task = Task.Run((Func<int>)(() => { throw new NotImplementedException(); }));
			Assert.Throws<NotImplementedException>(() => task.WaitAndUnwrapException());
		}

		[Fact]
		public void WaitAndUnwrapExceptionResultWithCT_Completed_DoesNotBlock()
		{
			var cts = new CancellationTokenSource();
			TaskConstants.Int32Zero.WaitAndUnwrapException(cts.Token);
		}

		[Fact]
		public void WaitAndUnwrapExceptionResultWithCT_Faulted_UnwrapsException()
		{
			var cts = new CancellationTokenSource();
			var task = Task.Run((Func<int>)(() => { throw new NotImplementedException(); }));
			Assert.Throws<NotImplementedException>(() => task.WaitAndUnwrapException(cts.Token));
		}

		[Fact]
		public void WaitAndUnwrapExceptionResultWithCT_CancellationTokenCancelled_Cancels()
		{
			var tcs = new TaskCompletionSource<int>();
			var cts = new CancellationTokenSource();
			cts.Cancel();
			Assert.Throws<OperationCanceledException>(() => tcs.Task.WaitAndUnwrapException(cts.Token));
		}

		[Fact]
		public void WaitWithoutException_Completed_DoesNotBlock()
		{
			TaskConstants.Completed.WaitWithoutException();
		}

		[Fact]
		public void WaitWithoutException_Canceled_DoesNotBlockOrThrow()
		{
			TaskConstants.Canceled.WaitWithoutException();
		}

		[Fact]
		public void WaitWithoutException_Faulted_DoesNotBlockOrThrow()
		{
			var task = Task.Run(() => { throw new NotImplementedException(); });
			task.WaitWithoutException();
		}

		[Fact]
		public void WaitWithoutExceptionResult_Completed_DoesNotBlock()
		{
			TaskConstants.Int32Zero.WaitWithoutException();
		}

		[Fact]
		public void WaitWithoutExceptionResult_Canceled_DoesNotBlockOrThrow()
		{
			TaskConstants<int>.Canceled.WaitWithoutException();
		}

		[Fact]
		public void WaitWithoutExceptionResult_Faulted_DoesNotBlockOrThrow()
		{
			var task = Task.Run((Func<int>)(() => { throw new NotImplementedException(); }));
			task.WaitWithoutException();
		}

		[Fact]
		public void WaitWithoutExceptionWithCancellationToken_Completed_DoesNotBlock()
		{
			TaskConstants.Completed.WaitWithoutException(new CancellationToken());
		}

		[Fact]
		public void WaitWithoutExceptionWithCancellationToken_Canceled_DoesNotBlockOrThrow()
		{
			TaskConstants.Canceled.WaitWithoutException(new CancellationToken());
		}

		[Fact]
		public void WaitWithoutExceptionWithCancellationToken_Faulted_DoesNotBlockOrThrow()
		{
			var task = Task.Run(() => { throw new NotImplementedException(); });
			task.WaitWithoutException(new CancellationToken());
		}

		[Fact]
		public void WaitWithoutExceptionResultWithCancellationToken_Completed_DoesNotBlock()
		{
			TaskConstants.Int32Zero.WaitWithoutException(new CancellationToken());
		}

		[Fact]
		public void WaitWithoutExceptionResultWithCancellationToken_Canceled_DoesNotBlockOrThrow()
		{
			TaskConstants<int>.Canceled.WaitWithoutException(new CancellationToken());
		}

		[Fact]
		public void WaitWithoutExceptionResultWithCancellationToken_Faulted_DoesNotBlockOrThrow()
		{
			var task = Task.Run((Func<int>)(() => { throw new NotImplementedException(); }));
			task.WaitWithoutException(new CancellationToken());
		}

		[Fact]
		public void WaitWithoutExceptionWithCancellationToken_CanceledToken_DoesNotBlockButThrowsException()
		{
			Task task = new TaskCompletionSource<object>().Task;
			var cts = new CancellationTokenSource();
			cts.Cancel();
			Assert.Throws<OperationCanceledException>(() => task.WaitWithoutException(cts.Token));
		}

		[Fact]
		public async Task WaitWithoutExceptionWithCancellationToken_TokenCanceled_ThrowsException()
		{
			Task sourceTask = new TaskCompletionSource<object>().Task;
			var cts = new CancellationTokenSource();
			var task = Task.Run(() => sourceTask.WaitWithoutException(cts.Token));
			bool result = task.Wait(500);
			Assert.False(result);
			cts.Cancel();
			await Assert.ThrowsAsync<OperationCanceledException>(() => task);
		}

		#endregion

		[Fact]
		public void WaitAsyncTResult_TokenThatCannotCancel_ReturnsSourceTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var task = tcs.Task.WaitAsync(CancellationToken.None);

			Assert.Same(tcs.Task, task);
		}

		[Fact]
		public void WaitAsyncTResult_AlreadyCanceledToken_ReturnsSynchronouslyCanceledTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var token = new CancellationToken(true);
			var task = tcs.Task.WaitAsync(token);

			Assert.True(task.IsCanceled);
			Assert.Equal(token, GetCancellationTokenFromTask(task));
		}

		[Fact]
		public async Task WaitAsyncTResult_TokenCanceled_CancelsTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var cts = new CancellationTokenSource();
			var task = tcs.Task.WaitAsync(cts.Token);
			Assert.False(task.IsCompleted);

			cts.Cancel();

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
			Assert.Equal(cts.Token, GetCancellationTokenFromTask(task));
		}

		[Fact]
		public void WaitAsync_TokenThatCannotCancel_ReturnsSourceTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var task = ((Task)tcs.Task).WaitAsync(CancellationToken.None);

			Assert.Same(tcs.Task, task);
		}

		[Fact]
		public void WaitAsync_AlreadyCanceledToken_ReturnsSynchronouslyCanceledTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var token = new CancellationToken(true);
			var task = ((Task)tcs.Task).WaitAsync(token);

			Assert.True(task.IsCanceled);
			Assert.Equal(token, GetCancellationTokenFromTask(task));
		}

		[Fact]
		public async Task WaitAsync_TokenCanceled_CancelsTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var cts = new CancellationTokenSource();
			var task = ((Task)tcs.Task).WaitAsync(cts.Token);
			Assert.False(task.IsCompleted);

			cts.Cancel();

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
			Assert.Equal(cts.Token, GetCancellationTokenFromTask(task));
		}

		[Fact]
		public void WhenAnyTResult_AlreadyCanceledToken_ReturnsSynchronouslyCanceledTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var token = new CancellationToken(true);
			var task = new[] { tcs.Task }.WhenAny(token);

			Assert.True(task.IsCanceled);
			Assert.Equal(token, GetCancellationTokenFromTask(task));
		}

		[Fact]
		public async Task WhenAnyTResult_TaskCompletes_CompletesTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var cts = new CancellationTokenSource();
			var task = new[] { tcs.Task }.WhenAny(cts.Token);
			Assert.False(task.IsCompleted);

			tcs.SetResult(null);

			var result = await task;
			Assert.Same(tcs.Task, result);
		}

		[Fact]
		public async Task WhenAnyTResult_TokenCanceled_CancelsTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var cts = new CancellationTokenSource();
			var task = new[] { tcs.Task }.WhenAny(cts.Token);
			Assert.False(task.IsCompleted);

			cts.Cancel();

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
			Assert.Equal(cts.Token, GetCancellationTokenFromTask(task));
		}

		[Fact]
		public void WhenAny_AlreadyCanceledToken_ReturnsSynchronouslyCanceledTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var token = new CancellationToken(true);
			var task = new Task[] { tcs.Task }.WhenAny(token);

			Assert.True(task.IsCanceled);
			Assert.Equal(token, GetCancellationTokenFromTask(task));
		}

		[Fact]
		public async Task WhenAny_TaskCompletes_CompletesTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var cts = new CancellationTokenSource();
			var task = new Task[] { tcs.Task }.WhenAny(cts.Token);
			Assert.False(task.IsCompleted);

			tcs.SetResult(null);

			var result = await task;
			Assert.Same(tcs.Task, result);
		}

		[Fact]
		public async Task WhenAny_TokenCanceled_CancelsTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var cts = new CancellationTokenSource();
			var task = new Task[] { tcs.Task }.WhenAny(cts.Token);
			Assert.False(task.IsCompleted);

			cts.Cancel();

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
			Assert.Equal(cts.Token, GetCancellationTokenFromTask(task));
		}

		[Fact]
		public async Task WhenAnyTResultWithoutToken_TaskCompletes_CompletesTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var task = new[] { tcs.Task }.WhenAny();
			Assert.False(task.IsCompleted);

			tcs.SetResult(null);

			var result = await task;
			Assert.Same(tcs.Task, result);
		}

		[Fact]
		public async Task WhenAnyWithoutToken_TaskCompletes_CompletesTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var task = new Task[] { tcs.Task }.WhenAny();
			Assert.False(task.IsCompleted);

			tcs.SetResult(null);

			var result = await task;
			Assert.Same(tcs.Task, result);
		}

		[Fact]
		public async Task WhenAllTResult_TaskCompletes_CompletesTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var task = new[] { tcs.Task }.WhenAll();
			Assert.False(task.IsCompleted);

			var expectedResult = new object();
			tcs.SetResult(expectedResult);

			var result = await task;
			Assert.Equal(new[] { expectedResult }, result);
		}

		[Fact]
		public async Task WhenAll_TaskCompletes_CompletesTask()
		{
			var tcs = new TaskCompletionSource<object>();
			var task = new Task[] { tcs.Task }.WhenAll();
			Assert.False(task.IsCompleted);

			var expectedResult = new object();
			tcs.SetResult(expectedResult);

			await task;
		}

		[Fact]
		public async Task OrderByCompletion_OrdersByCompletion()
		{
			var tcs = new[] { new TaskCompletionSource<int>(), new TaskCompletionSource<int>() };
			var results = tcs.Select(x => x.Task).OrderByCompletion();

			Assert.False(results[0].IsCompleted);
			Assert.False(results[1].IsCompleted);

			tcs[1].SetResult(13);
			int result0 = await results[0];
			Assert.False(results[1].IsCompleted);
			Assert.Equal(13, result0);

			tcs[0].SetResult(17);
			int result1 = await results[1];
			Assert.Equal(13, result0);
			Assert.Equal(17, result1);
		}

		[Fact]
		public async Task OrderByCompletion_PropagatesFaultOnFirstCompletion()
		{
			var tcs = new[] { new TaskCompletionSource<int>(), new TaskCompletionSource<int>() };
			var results = tcs.Select(x => x.Task).OrderByCompletion();

			tcs[1].SetException(new InvalidOperationException("test message"));
			try
			{
				await results[0];
			}
			catch (InvalidOperationException ex)
			{
				Assert.Equal("test message", ex.Message);
				return;
			}

			Assert.True(false);
		}

		[Fact]
		public async Task OrderByCompletion_PropagatesFaultOnSecondCompletion()
		{
			var tcs = new[] { new TaskCompletionSource<int>(), new TaskCompletionSource<int>() };
			var results = tcs.Select(x => x.Task).OrderByCompletion();

			tcs[0].SetResult(13);
			tcs[1].SetException(new InvalidOperationException("test message"));
			await results[0];
			try
			{
				await results[1];
			}
			catch (InvalidOperationException ex)
			{
				Assert.Equal("test message", ex.Message);
				return;
			}

			Assert.True(false);
		}

		[Fact]
		public async Task OrderByCompletion_PropagatesCancelOnFirstCompletion()
		{
			var tcs = new[] { new TaskCompletionSource<int>(), new TaskCompletionSource<int>() };
			var results = tcs.Select(x => x.Task).OrderByCompletion();

			tcs[1].SetCanceled();
			try
			{
				await results[0];
			}
			catch (OperationCanceledException)
			{
				return;
			}

			Assert.True(false);
		}

		[Fact]
		public async Task OrderByCompletion_PropagatesCancelOnSecondCompletion()
		{
			var tcs = new[] { new TaskCompletionSource<int>(), new TaskCompletionSource<int>() };
			var results = tcs.Select(x => x.Task).OrderByCompletion();

			tcs[0].SetResult(13);
			tcs[1].SetCanceled();
			await results[0];
			try
			{
				await results[1];
			}
			catch (OperationCanceledException)
			{
				return;
			}

			Assert.True(false);
		}

		private static CancellationToken GetCancellationTokenFromTask(Task task)
		{
			try
			{
				task.Wait();
			}
			catch (AggregateException ex)
			{
				if (ex.InnerException is OperationCanceledException oce)
					return oce.CancellationToken;
			}

			return CancellationToken.None;
		}
	}

}
