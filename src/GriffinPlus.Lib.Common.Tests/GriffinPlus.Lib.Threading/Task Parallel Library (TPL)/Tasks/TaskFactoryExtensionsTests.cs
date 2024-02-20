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

using System.Threading.Tasks;

using Xunit;

namespace GriffinPlus.Lib.Threading;

public class TaskFactoryExtensionsTests
{
	[Fact]
	public async Task RunAction_WithFactoryScheduler_UsesFactoryScheduler()
	{
		TaskScheduler scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
		var factory = new TaskFactory(scheduler);
		TaskScheduler result = null;

		Task task = factory.Run(
			() =>
			{
				result = TaskScheduler.Current;
			});
		await task;

		Assert.Same(scheduler, result);
		Assert.True((task.CreationOptions & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.DenyChildAttach);
	}

	[Fact]
	public async Task RunAction_WithCurrentScheduler_UsesDefaultScheduler()
	{
		TaskScheduler scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
		var testFactory = new TaskFactory(scheduler);
		Task task = null;
		TaskScheduler result = null;

		await testFactory.StartNew(
				() =>
				{
					Assert.Same(scheduler, TaskScheduler.Current);
					Assert.Null(Task.Factory.Scheduler);
					task = Task.Factory.Run(
						() =>
						{
							result = TaskScheduler.Current;
						});
					return task;
				})
			.Unwrap();

		Assert.Same(TaskScheduler.Default, result);
		Assert.True((task.CreationOptions & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.DenyChildAttach);
	}

	[Fact]
	public async Task RunFunc_WithFactoryScheduler_UsesFactoryScheduler()
	{
		TaskScheduler scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
		var factory = new TaskFactory(scheduler);

		Task<TaskScheduler> task = factory.Run(() => TaskScheduler.Current);
		TaskScheduler result = await task;

		Assert.Same(scheduler, result);
		Assert.True((task.CreationOptions & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.DenyChildAttach);
	}

	[Fact]
	public async Task RunFunc_WithCurrentScheduler_UsesDefaultScheduler()
	{
		TaskScheduler scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
		var testFactory = new TaskFactory(scheduler);
		Task<TaskScheduler> task = null;
		TaskScheduler result = null;

		await testFactory.StartNew(
				async () =>
				{
					Assert.Same(scheduler, TaskScheduler.Current);
					Assert.Null(Task.Factory.Scheduler);
					task = Task.Factory.Run(() => TaskScheduler.Current);
					result = await task;
				})
			.Unwrap();

		Assert.Same(TaskScheduler.Default, result);
		Assert.True((task.CreationOptions & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.DenyChildAttach);
	}

	[Fact]
	public async Task RunAsyncAction_WithFactoryScheduler_UsesFactoryScheduler()
	{
		TaskScheduler scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
		var factory = new TaskFactory(scheduler);
		TaskScheduler result = null;
		TaskScheduler resultAfterAwait = null;

		Task task = factory.Run(
			async () =>
			{
				result = TaskScheduler.Current;
				await Task.Yield();
				resultAfterAwait = TaskScheduler.Current;
			});
		await task;

		Assert.Same(scheduler, result);
		Assert.Same(scheduler, resultAfterAwait);
	}

	[Fact]
	public async Task RunAsyncAction_WithCurrentScheduler_UsesDefaultScheduler()
	{
		TaskScheduler scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
		var testFactory = new TaskFactory(scheduler);
		TaskScheduler result = null;
		TaskScheduler resultAfterAwait = null;

		await testFactory.StartNew(
				() =>
				{
					Assert.Same(scheduler, TaskScheduler.Current);
					Assert.Null(Task.Factory.Scheduler);
					return Task.Factory.Run(
						async () =>
						{
							result = TaskScheduler.Current;
							await Task.Yield();
							resultAfterAwait = TaskScheduler.Current;
						});
				})
			.Unwrap();

		Assert.Same(TaskScheduler.Default, result);
		Assert.Same(TaskScheduler.Default, resultAfterAwait);
	}

	[Fact]
	public async Task RunAsyncFunc_WithFactoryScheduler_UsesFactoryScheduler()
	{
		TaskScheduler scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
		var factory = new TaskFactory(scheduler);
		TaskScheduler result = null;

		TaskScheduler resultAfterAwait = await factory.Run(
			                                 async () =>
			                                 {
				                                 result = TaskScheduler.Current;
				                                 await Task.Yield();
				                                 return TaskScheduler.Current;
			                                 });

		Assert.Same(scheduler, result);
		Assert.Same(scheduler, resultAfterAwait);
	}

	[Fact]
	public async Task RunAsyncFunc_WithCurrentScheduler_UsesDefaultScheduler()
	{
		TaskScheduler scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
		var testFactory = new TaskFactory(scheduler);
		TaskScheduler result = null;
		TaskScheduler resultAfterAwait = null;

		await testFactory.StartNew(
				async () =>
				{
					Assert.Same(scheduler, TaskScheduler.Current);
					Assert.Null(Task.Factory.Scheduler);
					resultAfterAwait = await Task.Factory.Run(
						                   async () =>
						                   {
							                   result = TaskScheduler.Current;
							                   await Task.Yield();
							                   return TaskScheduler.Current;
						                   });
				})
			.Unwrap();

		Assert.Same(TaskScheduler.Default, result);
		Assert.Same(TaskScheduler.Default, resultAfterAwait);
	}
}
