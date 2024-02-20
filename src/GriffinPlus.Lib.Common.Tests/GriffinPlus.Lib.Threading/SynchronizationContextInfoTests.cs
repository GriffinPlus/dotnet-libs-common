///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#if NETFRAMEWORK

using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;

using Xunit;

namespace GriffinPlus.Lib.Threading
{

	/// <summary>
	/// Unit tests targeting the <see cref="SynchronizationContextInfo"/> class.
	/// </summary>
	public class SynchronizationContextInfoTests
	{
		#region RegisterSerializingContext()

		private sealed class TestSynchronizationContext1 : SynchronizationContext;

		[Fact]
		public void RegisterSerializingContext_GenericParameter()
		{
			var context = new TestSynchronizationContext1();
			Assert.False(SynchronizationContextInfo.IsSerializingSynchronizationContext(context));
			SynchronizationContextInfo.RegisterSerializingContext<TestSynchronizationContext1>();
			Assert.True(SynchronizationContextInfo.IsSerializingSynchronizationContext(context));
		}

		private sealed class TestSynchronizationContext2 : SynchronizationContext;

		[Fact]
		public void RegisterSerializingContext_TypeParameter()
		{
			var context = new TestSynchronizationContext2();
			Assert.False(SynchronizationContextInfo.IsSerializingSynchronizationContext(context));
			SynchronizationContextInfo.RegisterSerializingContext(typeof(TestSynchronizationContext2));
			Assert.True(SynchronizationContextInfo.IsSerializingSynchronizationContext(context));
		}

		#endregion

		#region IsSerializingSynchronizationContext()

		public static IEnumerable<object[]> IsSerializingSynchronizationContextTestData_Predefined
		{
			get
			{
				yield return
				[
					new WindowsFormsSynchronizationContext()
				];

				yield return
				[
					new DispatcherSynchronizationContext()
				];
			}
		}

		[Theory]
		[MemberData(nameof(IsSerializingSynchronizationContextTestData_Predefined))]
		public void IsSerializingSynchronizationContext_Predefined(SynchronizationContext context)
		{
			Assert.True(SynchronizationContextInfo.IsSerializingSynchronizationContext(context));
		}

		public static IEnumerable<object[]> IsSerializingSynchronizationContextTestData_Unknown
		{
			get
			{
				yield return
				[
					new SynchronizationContext()
				];
			}
		}

		[Theory]
		[MemberData(nameof(IsSerializingSynchronizationContextTestData_Unknown))]
		public void IsSerializingSynchronizationContext_Unknown(SynchronizationContext context)
		{
			Assert.False(SynchronizationContextInfo.IsSerializingSynchronizationContext(context));
		}

		#endregion
	}

}

#endif
