///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Buffers;
using System.Threading;

namespace GriffinPlus.Lib.Io
{

	class ArrayPoolMock : ArrayPool<byte>
	{
		private readonly ArrayPool<byte> mPool = Create();
		private          int             mRentedBufferCount;

		public int RentedBufferCount => Volatile.Read(ref mRentedBufferCount);

		public override byte[] Rent(int minimumLength)
		{
			Interlocked.Increment(ref mRentedBufferCount);
			return mPool.Rent(minimumLength);
		}

		public override void Return(byte[] array, bool clearArray = false)
		{
			Interlocked.Decrement(ref mRentedBufferCount);
			mPool.Return(array, clearArray);
		}
	}

}
