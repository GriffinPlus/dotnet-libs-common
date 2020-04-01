﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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

using System.Threading.Tasks;
using Xunit;

namespace GriffinPlus.Lib.Threading
{
	public class TaskFactoryExtensionsTests
	{
		[Fact]
		public async Task RunAction_WithFactoryScheduler_UsesFactoryScheduler()
		{
			var scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
			var factory = new TaskFactory(scheduler);
			TaskScheduler result = null;

			var task = factory.Run(() =>
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
			var scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
			var testFactory = new TaskFactory(scheduler);
			Task task = null;
			TaskScheduler result = null;

			await testFactory.StartNew(async () =>
			{
				Assert.Same(scheduler, TaskScheduler.Current);
				Assert.Null(Task.Factory.Scheduler);
				task = Task.Factory.Run(() =>
				{
					result = TaskScheduler.Current;
				});
				await task;
			}).Unwrap();

			Assert.Same(TaskScheduler.Default, result);
			Assert.True((task.CreationOptions & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.DenyChildAttach);
		}

		[Fact]
		public async Task RunFunc_WithFactoryScheduler_UsesFactoryScheduler()
		{
			var scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
			var factory = new TaskFactory(scheduler);

			var task = factory.Run(() => TaskScheduler.Current);
			var result = await task;

			Assert.Same(scheduler, result);
			Assert.True((task.CreationOptions & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.DenyChildAttach);
		}

		[Fact]
		public async Task RunFunc_WithCurrentScheduler_UsesDefaultScheduler()
		{
			var scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
			var testFactory = new TaskFactory(scheduler);
			Task<TaskScheduler> task = null;
			TaskScheduler result = null;

			await testFactory.StartNew(async () =>
			{
				Assert.Same(scheduler, TaskScheduler.Current);
				Assert.Null(Task.Factory.Scheduler);
				task = Task.Factory.Run(() => TaskScheduler.Current);
				result = await task;
			}).Unwrap();

			Assert.Same(TaskScheduler.Default, result);
			Assert.True((task.CreationOptions & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.DenyChildAttach);
		}

		[Fact]
		public async Task RunAsyncAction_WithFactoryScheduler_UsesFactoryScheduler()
		{
			var scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
			var factory = new TaskFactory(scheduler);
			TaskScheduler result = null;
			TaskScheduler resultAfterAwait = null;

			var task = factory.Run(async () =>
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
			var scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
			var testFactory = new TaskFactory(scheduler);
			TaskScheduler result = null;
			TaskScheduler resultAfterAwait = null;

			await testFactory.StartNew(async () =>
			{
				Assert.Same(scheduler, TaskScheduler.Current);
				Assert.Null(Task.Factory.Scheduler);
				await Task.Factory.Run(async () =>
				{
					result = TaskScheduler.Current;
					await Task.Yield();
					resultAfterAwait = TaskScheduler.Current;
				});
			}).Unwrap();

			Assert.Same(TaskScheduler.Default, result);
			Assert.Same(TaskScheduler.Default, resultAfterAwait);
		}

		[Fact]
		public async Task RunAsyncFunc_WithFactoryScheduler_UsesFactoryScheduler()
		{
			var scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
			var factory = new TaskFactory(scheduler);
			TaskScheduler result = null;

			var resultAfterAwait = await factory.Run(async () =>
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
			var scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
			var testFactory = new TaskFactory(scheduler);
			TaskScheduler result = null;
			TaskScheduler resultAfterAwait = null;

			await testFactory.StartNew(async () =>
			{
				Assert.Same(scheduler, TaskScheduler.Current);
				Assert.Null(Task.Factory.Scheduler);
				resultAfterAwait = await Task.Factory.Run(async () =>
				{
					result = TaskScheduler.Current;
					await Task.Yield();
					return TaskScheduler.Current;
				});
			}).Unwrap();

			Assert.Same(TaskScheduler.Default, result);
			Assert.Same(TaskScheduler.Default, resultAfterAwait);
		}
	}
}