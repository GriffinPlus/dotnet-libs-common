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

using Xunit;

namespace GriffinPlus.Lib.Threading
{
	public class TaskConstantsTests
	{
		[Fact]
		public void BooleanTrue_IsCompletedWithValueOfTrue()
		{
			var task = TaskConstants.BooleanTrue;
			Assert.True(task.IsCompleted);
			Assert.True(task.Result);
		}

		[Fact]
		public void BooleanTrue_IsCached()
		{
			var task1 = TaskConstants.BooleanTrue;
			var task2 = TaskConstants.BooleanTrue;
			Assert.Same(task1, task2);
		}

		[Fact]
		public void BooleanFalse_IsCompletedWithValueOfFalse()
		{
			var task = TaskConstants.BooleanFalse;
			Assert.True(task.IsCompleted);
			Assert.False(task.Result);
		}

		[Fact]
		public void BooleanFalse_IsCached()
		{
			var task1 = TaskConstants.BooleanFalse;
			var task2 = TaskConstants.BooleanFalse;
			Assert.Same(task1, task2);
		}

		[Fact]
		public void Int32Zero_IsCompletedWithValueOfZero()
		{
			var task = TaskConstants.Int32Zero;
			Assert.True(task.IsCompleted);
			Assert.Equal(0, task.Result);
		}

		[Fact]
		public void Int32Zero_IsCached()
		{
			var task1 = TaskConstants.Int32Zero;
			var task2 = TaskConstants.Int32Zero;
			Assert.Same(task1, task2);
		}

		[Fact]
		public void Int32NegativeOne_IsCompletedWithValueOfNegativeOne()
		{
			var task = TaskConstants.Int32NegativeOne;
			Assert.True(task.IsCompleted);
			Assert.Equal(-1, task.Result);
		}

		[Fact]
		public void Int32NegativeOne_IsCached()
		{
			var task1 = TaskConstants.Int32NegativeOne;
			var task2 = TaskConstants.Int32NegativeOne;
			Assert.Same(task1, task2);
		}

		[Fact]
		public void Completed_IsCompleted()
		{
			var task = TaskConstants.Completed;
			Assert.True(task.IsCompleted);
		}

		[Fact]
		public void Completed_IsCached()
		{
			var task1 = TaskConstants.Completed;
			var task2 = TaskConstants.Completed;
			Assert.Same(task1, task2);
		}

		[Fact]
		public void Canceled_IsCanceled()
		{
			var task = TaskConstants.Canceled;
			Assert.True(task.IsCanceled);
		}

		[Fact]
		public void Canceled_IsCached()
		{
			var task1 = TaskConstants.Canceled;
			var task2 = TaskConstants.Canceled;
			Assert.Same(task1, task2);
		}

		[Fact]
		public void Default_ReferenceType_IsCompletedWithValueOfNull()
		{
			var task = TaskConstants<object>.Default;
			Assert.True(task.IsCompleted);
			Assert.Null(task.Result);
		}

		[Fact]
		public void Default_ValueType_IsCompletedWithValueOfZero()
		{
			var task = TaskConstants<byte>.Default;
			Assert.True(task.IsCompleted);
			Assert.Equal(0, task.Result);
		}

		[Fact]
		public void Default_IsCached()
		{
			var task1 = TaskConstants<object>.Default;
			var task2 = TaskConstants<object>.Default;
			Assert.Same(task1, task2);
		}

		[Fact]
		public void CanceledOfT_IsCanceled()
		{
			var task = TaskConstants<object>.Canceled;
			Assert.True(task.IsCanceled);
		}

		[Fact]
		public void CanceledOfT_IsCached()
		{
			var task1 = TaskConstants<object>.Canceled;
			var task2 = TaskConstants<object>.Canceled;
			Assert.Same(task1, task2);
		}
	}
}