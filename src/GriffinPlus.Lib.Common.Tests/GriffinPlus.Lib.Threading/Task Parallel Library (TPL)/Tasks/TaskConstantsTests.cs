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

namespace GriffinPlus.Lib.Threading
{

	public class TaskConstantsTests
	{
		[Fact]
		public void BooleanTrue_IsCompletedWithValueOfTrue()
		{
			Task<bool> task = TaskConstants.BooleanTrue;
			Assert.True(task.IsCompleted);
			Assert.True(task.Result);
		}

		[Fact]
		public void BooleanTrue_IsCached()
		{
			Task<bool> task1 = TaskConstants.BooleanTrue;
			Task<bool> task2 = TaskConstants.BooleanTrue;
			Assert.Same(task1, task2);
		}

		[Fact]
		public void BooleanFalse_IsCompletedWithValueOfFalse()
		{
			Task<bool> task = TaskConstants.BooleanFalse;
			Assert.True(task.IsCompleted);
			Assert.False(task.Result);
		}

		[Fact]
		public void BooleanFalse_IsCached()
		{
			Task<bool> task1 = TaskConstants.BooleanFalse;
			Task<bool> task2 = TaskConstants.BooleanFalse;
			Assert.Same(task1, task2);
		}

		[Fact]
		public void Int32Zero_IsCompletedWithValueOfZero()
		{
			Task<int> task = TaskConstants.Int32Zero;
			Assert.True(task.IsCompleted);
			Assert.Equal(0, task.Result);
		}

		[Fact]
		public void Int32Zero_IsCached()
		{
			Task<int> task1 = TaskConstants.Int32Zero;
			Task<int> task2 = TaskConstants.Int32Zero;
			Assert.Same(task1, task2);
		}

		[Fact]
		public void Int32NegativeOne_IsCompletedWithValueOfNegativeOne()
		{
			Task<int> task = TaskConstants.Int32NegativeOne;
			Assert.True(task.IsCompleted);
			Assert.Equal(-1, task.Result);
		}

		[Fact]
		public void Int32NegativeOne_IsCached()
		{
			Task<int> task1 = TaskConstants.Int32NegativeOne;
			Task<int> task2 = TaskConstants.Int32NegativeOne;
			Assert.Same(task1, task2);
		}

		[Fact]
		public void Completed_IsCompleted()
		{
			Task task = TaskConstants.Completed;
			Assert.True(task.IsCompleted);
		}

		[Fact]
		public void Completed_IsCached()
		{
			Task task1 = TaskConstants.Completed;
			Task task2 = TaskConstants.Completed;
			Assert.Same(task1, task2);
		}

		[Fact]
		public void Canceled_IsCanceled()
		{
			Task task = TaskConstants.Canceled;
			Assert.True(task.IsCanceled);
		}

		[Fact]
		public void Canceled_IsCached()
		{
			Task task1 = TaskConstants.Canceled;
			Task task2 = TaskConstants.Canceled;
			Assert.Same(task1, task2);
		}

		[Fact]
		public void Default_ReferenceType_IsCompletedWithValueOfNull()
		{
			Task<object> task = TaskConstants<object>.Default;
			Assert.True(task.IsCompleted);
			Assert.Null(task.Result);
		}

		[Fact]
		public void Default_ValueType_IsCompletedWithValueOfZero()
		{
			Task<byte> task = TaskConstants<byte>.Default;
			Assert.True(task.IsCompleted);
			Assert.Equal(0, task.Result);
		}

		[Fact]
		public void Default_IsCached()
		{
			Task<object> task1 = TaskConstants<object>.Default;
			Task<object> task2 = TaskConstants<object>.Default;
			Assert.Same(task1, task2);
		}

		[Fact]
		public void CanceledOfT_IsCanceled()
		{
			Task<object> task = TaskConstants<object>.Canceled;
			Assert.True(task.IsCanceled);
		}

		[Fact]
		public void CanceledOfT_IsCached()
		{
			Task<object> task1 = TaskConstants<object>.Canceled;
			Task<object> task2 = TaskConstants<object>.Canceled;
			Assert.Same(task1, task2);
		}
	}

}
