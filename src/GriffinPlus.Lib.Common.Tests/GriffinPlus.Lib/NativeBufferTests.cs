///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Xunit;

namespace GriffinPlus.Lib
{

	/// <summary>
	/// Unit tests targeting the <see cref="NativeBuffer"/> class.
	/// </summary>
	public class NativeBufferTests
	{
		#region Create(long size)

		public static IEnumerable<object[]> CreateTestData
		{
			get
			{
				foreach (long size in new[] { 0, 1, 4096 })
				{
					yield return new object[] { size };
				}
			}
		}

		[Theory]
		[MemberData(nameof(CreateTestData))]
		public void Create(long size)
		{
			// create a new instance
			var buffer = NativeBuffer.Create(size);
			Assert.NotNull(buffer);

			// the handle should be valid
			Assert.False(buffer.IsInvalid);

			// the native buffer should be owned
			Assert.True(buffer.OwnsBuffer);

			// the published address of the buffer should not be null and the size should reflect
			// the requested buffer size
			Assert.NotEqual(IntPtr.Zero, buffer.UnsafeAddress);
			Assert.Equal(size, buffer.Size);

			// an accessor working on top of the buffer should return the same address and size
			var accessor = buffer.GetAccessor();
			Assert.Equal(buffer.UnsafeAddress, accessor.Address);
			Assert.Equal(size, accessor.Size);

			// the actual address and the actual size of the native buffer can be different due to alignment adjustments
			Assert.NotEqual(IntPtr.Zero, buffer.UnsafeActualAddress);
			Assert.True(buffer.ActualSize >= size);

			// dispose the accessor and the buffer instance 
			accessor.Dispose();
			buffer.Dispose();

			// the handle should be invalid now
			Assert.True(buffer.IsInvalid);

			// the addresses of the buffer should be null and the size should be 0 now
			Assert.Equal(IntPtr.Zero, buffer.UnsafeAddress);
			Assert.Equal(0, buffer.Size);
			Assert.Equal(IntPtr.Zero, buffer.UnsafeActualAddress);
			Assert.Equal(0, buffer.ActualSize);
			Assert.Equal(IntPtr.Zero, accessor.Address);
			Assert.Equal(0, accessor.Size);
		}

		[Fact]
		public void Create_SizeIsNegative()
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => NativeBuffer.Create(-1));
			Assert.Equal("size", exception.ParamName);
		}

		[Fact]
		public void Create_SizeTooBigFor32Bit()
		{
			// note: of course this can only work when running the test with a 32 bit test agent
			if (IntPtr.Size == 4)
			{
				var exception = Assert.Throws<ArgumentOutOfRangeException>(() => NativeBuffer.Create((long)int.MaxValue + 1));
				Assert.Equal("size", exception.ParamName);
			}
		}

		#endregion

		#region CreateAligned(long size, long alignment)

		public static IEnumerable<object[]> CreateAlignedTestData
		{
			get
			{
				foreach (long size in new[] { 0, 1, 4096 })
				foreach (long alignment in new[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 })
				{
					yield return new object[] { size, alignment };
				}
			}
		}

		[Theory]
		[MemberData(nameof(CreateAlignedTestData))]
		public void CreateAligned(long size, long alignment)
		{
			// create a new instance
			var buffer = NativeBuffer.CreateAligned(size, alignment);
			Assert.NotNull(buffer);

			// the handle should be valid
			Assert.False(buffer.IsInvalid);

			// the native buffer should be owned
			Assert.True(buffer.OwnsBuffer);

			// the published address of the buffer should not be null and the size should reflect
			// the requested buffer size, the alignment should meet the requirements
			Assert.NotEqual(IntPtr.Zero, buffer.UnsafeAddress);
			Assert.Equal(size, buffer.Size);
			Assert.Equal(0, buffer.UnsafeAddress.ToInt64() & (alignment - 1));

			// an accessor working on top of the buffer should return the same address and size
			var accessor = buffer.GetAccessor();
			Assert.Equal(buffer.UnsafeAddress, accessor.Address);
			Assert.Equal(size, accessor.Size);

			// the actual address and the actual size of the native buffer can be different due to alignment adjustments
			Assert.NotEqual(IntPtr.Zero, buffer.UnsafeActualAddress);
			Assert.True(buffer.ActualSize >= size);

			// dispose the accessor and the buffer instance 
			accessor.Dispose();
			buffer.Dispose();

			// the handle should be invalid now
			Assert.True(buffer.IsInvalid);

			// the addresses of the buffer should be null and the size should be 0 now
			Assert.Equal(IntPtr.Zero, buffer.UnsafeAddress);
			Assert.Equal(0, buffer.Size);
			Assert.Equal(IntPtr.Zero, buffer.UnsafeActualAddress);
			Assert.Equal(0, buffer.ActualSize);
			Assert.Equal(IntPtr.Zero, accessor.Address);
			Assert.Equal(0, accessor.Size);
		}

		[Fact]
		public void CreateAligned_SizeIsNegative()
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => NativeBuffer.CreateAligned(-1, 1));
			Assert.Equal("size", exception.ParamName);
		}

		[Fact]
		public void CreateAligned_SizeTooBigFor32Bit()
		{
			// note: of course this can only work when running the test with a 32 bit test agent
			if (IntPtr.Size == 4)
			{
				var exception = Assert.Throws<ArgumentOutOfRangeException>(() => NativeBuffer.CreateAligned((long)int.MaxValue + 1, 1));
				Assert.Equal("size", exception.ParamName);
			}
		}

		[Theory]
		[InlineData(3)]
		[InlineData(5)]
		[InlineData(6)]
		[InlineData(7)]
		[InlineData(9)]
		[InlineData(10)]
		[InlineData(11)]
		[InlineData(12)]
		[InlineData(13)]
		[InlineData(14)]
		[InlineData(15)]
		public void CreateAligned_AlignmentNotPowerOfTwo(int alignment)
		{
			var exception = Assert.Throws<ArgumentException>(() => NativeBuffer.CreateAligned(1, alignment));
			Assert.Equal("alignment", exception.ParamName);
		}

		[Fact]
		public void CreateAligned_AlignmentGreaterThanPageSize()
		{
			int alignment = Environment.SystemPageSize << 1;
			var exception = Assert.Throws<ArgumentException>(() => NativeBuffer.CreateAligned(1, alignment));
			Assert.Equal("alignment", exception.ParamName);
		}

		#endregion

		#region CreatePageAligned(long size)

		public static IEnumerable<object[]> CreatePageAlignedTestData
		{
			get
			{
				foreach (long size in new[]
				         {
					         0,
					         1,
					         Environment.SystemPageSize - 1,
					         Environment.SystemPageSize,
					         Environment.SystemPageSize + 1,
					         2 * Environment.SystemPageSize - 1,
					         2 * Environment.SystemPageSize,
					         2 * Environment.SystemPageSize + 1
				         })
				{
					yield return new object[] { size };
				}
			}
		}

		[Theory]
		[MemberData(nameof(CreatePageAlignedTestData))]
		public void CreatePageAligned(long size)
		{
			// create a new instance
			var buffer = NativeBuffer.CreatePageAligned(size);
			Assert.NotNull(buffer);

			// the handle should be valid
			Assert.False(buffer.IsInvalid);

			// the native buffer should be owned
			Assert.True(buffer.OwnsBuffer);

			// the published address of the buffer should not be null and the size should reflect
			// the requested buffer size, the alignment should meet the requirements
			Assert.NotEqual(IntPtr.Zero, buffer.UnsafeAddress);
			Assert.Equal(size, buffer.Size);
			Assert.Equal(0, buffer.UnsafeAddress.ToInt64() & (Environment.SystemPageSize - 1));

			// an accessor working on top of the buffer should return the same address and size
			var accessor = buffer.GetAccessor();
			Assert.Equal(buffer.UnsafeAddress, accessor.Address);
			Assert.Equal(size, accessor.Size);

			// the actual address and the actual size of the native buffer can be different due to alignment adjustments
			Assert.NotEqual(IntPtr.Zero, buffer.UnsafeActualAddress);
			Assert.True(buffer.ActualSize >= size);

			// dispose the accessor and the buffer instance 
			accessor.Dispose();
			buffer.Dispose();

			// the handle should be invalid now
			Assert.True(buffer.IsInvalid);

			// the addresses of the buffer should be null and the size should be 0 now
			Assert.Equal(IntPtr.Zero, buffer.UnsafeAddress);
			Assert.Equal(0, buffer.Size);
			Assert.Equal(IntPtr.Zero, buffer.UnsafeActualAddress);
			Assert.Equal(0, buffer.ActualSize);
			Assert.Equal(IntPtr.Zero, accessor.Address);
			Assert.Equal(0, accessor.Size);
		}

		[Fact]
		public void CreatePageAligned_SizeIsNegative()
		{
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => NativeBuffer.CreatePageAligned(-1));
			Assert.Equal("size", exception.ParamName);
		}

		[Fact]
		public void CreatePageAligned_SizeTooBigFor32Bit()
		{
			// note: of course this can only work when running the test with a 32 bit test agent
			if (IntPtr.Size == 4)
			{
				var exception = Assert.Throws<ArgumentOutOfRangeException>(() => NativeBuffer.CreatePageAligned((long)int.MaxValue + 1));
				Assert.Equal("size", exception.ParamName);
			}
		}

		#endregion

		#region FromPointer(long size)

		public static IEnumerable<object[]> FromPointerTestData
		{
			get
			{
				foreach (bool ownsBuffer in new[] { false, true })
				{
					yield return new object[] { new IntPtr(1), 0, ownsBuffer };
					yield return new object[] { new IntPtr(1), 1, ownsBuffer };
				}
			}
		}

		[Theory]
		[MemberData(nameof(FromPointerTestData))]
		public void FromPointer(IntPtr address, long size, bool ownsBuffer)
		{
			// create a new instance on a fictional buffer
			bool freed = false;
			NativeBuffer buffer = null;

			void FreeCallback(NativeBuffer buf)
			{
				// ReSharper disable once AccessToModifiedClosure
				Assert.Same(buffer, buf);
				freed = true;
			}

			// create an instance wrapping the fictional buffer
			buffer = NativeBuffer.FromPointer(address, size, ownsBuffer, FreeCallback);
			Assert.NotNull(buffer);
			Assert.False(freed);

			// the handle should be valid
			Assert.False(buffer.IsInvalid);

			// the native buffer should reflect the 'owned' status properly
			Assert.Equal(ownsBuffer, buffer.OwnsBuffer);

			// the published address and the size of the buffer should be as specified
			Assert.NotEqual(address, buffer.UnsafeAddress);
			Assert.Equal(size, buffer.Size);
			Assert.Equal(address, buffer.UnsafeActualAddress);
			Assert.Equal(size, buffer.ActualSize);

			// an accessor working on top of the buffer should return the same address and size
			var accessor = buffer.GetAccessor();
			Assert.Equal(buffer.UnsafeAddress, accessor.Address);
			Assert.Equal(size, accessor.Size);

			// dispose the accessor and the buffer instance 
			accessor.Dispose();
			buffer.Dispose();

			// the handle should be invalid now
			Assert.True(buffer.IsInvalid);

			// the callback should have been invoked
			Assert.True(freed);

			// the addresses of the buffer should be null and the size should be 0 now
			Assert.Equal(IntPtr.Zero, buffer.UnsafeAddress);
			Assert.Equal(0, buffer.Size);
			Assert.Equal(IntPtr.Zero, buffer.UnsafeActualAddress);
			Assert.Equal(0, buffer.ActualSize);
			Assert.Equal(IntPtr.Zero, accessor.Address);
			Assert.Equal(0, accessor.Size);
		}

		[Fact]
		public void FromPointer_AddressIsNull()
		{
			void FreeCallback(NativeBuffer buf) { }
			var exception = Assert.Throws<ArgumentNullException>(() => NativeBuffer.FromPointer(IntPtr.Zero, 1, true, FreeCallback));
			Assert.Equal("address", exception.ParamName);
		}

		[Fact]
		public void FromPointer_SizeIsNegative()
		{
			void FreeCallback(NativeBuffer buf) { }
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => NativeBuffer.FromPointer(new IntPtr(1), -1, true, FreeCallback));
			Assert.Equal("size", exception.ParamName);
		}

		[Fact]
		public void FromPointer_SizeTooBigFor32Bit()
		{
			// note: of course this can only work when running the test with a 32 bit test agent
			if (IntPtr.Size == 4)
			{
				void FreeCallback(NativeBuffer buf) { }
				var exception = Assert.Throws<ArgumentOutOfRangeException>(() => NativeBuffer.FromPointer(new IntPtr(1), (long)int.MaxValue + 1, true, FreeCallback));
				Assert.Equal("size", exception.ParamName);
			}
		}

		[Fact]
		public void FromPointer_FreeCallbackIsNull()
		{
			var exception = Assert.Throws<ArgumentNullException>(() => NativeBuffer.FromPointer(new IntPtr(1), 1, true, null));
			Assert.Equal("freeCallback", exception.ParamName);
		}

		#endregion
	}

}
