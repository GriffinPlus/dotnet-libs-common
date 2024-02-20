///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace GriffinPlus.Lib;

// ReSharper disable once UnusedMember.Global
partial class NativeBufferTests
{
	public class DisposableBufferMock : SafeHandle
	{
		private GCHandle mGcHandle;
		public  byte[]   Buffer;
		public  nint     Address;
		public  int      Size;
		public  bool     WasReleased;

		public DisposableBufferMock(int size) : base(IntPtr.Zero, true)
		{
			Buffer = new byte[size];
			mGcHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
			Address = mGcHandle.AddrOfPinnedObject();
			Size = Buffer.Length;
			handle = Address;
		}

		public override bool IsInvalid => WasReleased;

		protected override bool ReleaseHandle()
		{
			if (WasReleased) return true;
			mGcHandle.Free();
			WasReleased = true;
			return true;
		}
	}
}
