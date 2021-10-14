///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace GriffinPlus.Lib
{

	/// <summary>
	/// Provides test data for unit tests targeting type decomposition.
	/// </summary>
	public partial class DecomposedTypeTestData
	{
		/// <summary>
		/// Test data for unit tests targeting type decomposition.
		/// Fields:
		/// [0] Composed Type (type: System.Type)
		/// [1] Decomposed Type (type: GriffinPlus.Lib.DecomposedType)
		/// </summary>
		public static IEnumerable<object[]> TestData
		{
			get
			{
				///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// non-generic types
				///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

				// enum
				yield return new object[]
				{
					typeof(TestEnum),
					new DecomposedType(typeof(TestEnum), typeof(TestEnum), DecomposedType.EmptyTypes)
				};

				///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// struct
				///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

				// generic type definition:
				// GenericTestStruct<>
				yield return new object[]
				{
					typeof(GenericTestStruct<>),
					new DecomposedType(
						typeof(GenericTestStruct<>),
						typeof(GenericTestStruct<>),
						DecomposedType.EmptyTypes)
				};

				// generic type definition:
				// GenericTestStruct<>.NestedTestStruct
				yield return new object[]
				{
					typeof(GenericTestStruct<>.NestedTestStruct),
					new DecomposedType(
						typeof(GenericTestStruct<>.NestedTestStruct),
						typeof(GenericTestStruct<>.NestedTestStruct),
						DecomposedType.EmptyTypes)
				};

				// generic type definition:
				// GenericTestStruct<>.NestedGenericTestStruct<>
				yield return new object[]
				{
					typeof(GenericTestStruct<>.NestedGenericTestStruct<>),
					new DecomposedType(
						typeof(GenericTestStruct<>.NestedGenericTestStruct<>),
						typeof(GenericTestStruct<>.NestedGenericTestStruct<>),
						DecomposedType.EmptyTypes)
				};

				// generic type definition:
				// GenericTestStruct<,>
				yield return new object[]
				{
					typeof(GenericTestStruct<,>),
					new DecomposedType(
						typeof(GenericTestStruct<,>),
						typeof(GenericTestStruct<,>),
						DecomposedType.EmptyTypes)
				};

				// generic type definition:
				// GenericTestStruct<,>.NestedTestStruct
				yield return new object[]
				{
					typeof(GenericTestStruct<,>.NestedTestStruct),
					new DecomposedType(
						typeof(GenericTestStruct<,>.NestedTestStruct),
						typeof(GenericTestStruct<,>.NestedTestStruct),
						DecomposedType.EmptyTypes)
				};

				// generic type definition:
				// GenericTestStruct<,>.NestedGenericTestStruct<,>
				yield return new object[]
				{
					typeof(GenericTestStruct<,>.NestedGenericTestStruct<,>),
					new DecomposedType(
						typeof(GenericTestStruct<,>.NestedGenericTestStruct<,>),
						typeof(GenericTestStruct<,>.NestedGenericTestStruct<,>),
						DecomposedType.EmptyTypes)
				};

				// non-generic type:
				// TestStruct
				yield return new object[]
				{
					typeof(TestStruct),
					new DecomposedType(
						typeof(TestStruct),
						typeof(TestStruct),
						DecomposedType.EmptyTypes)
				};

				// non-generic type:
				// TestStruct.NestedTestStruct
				yield return new object[]
				{
					typeof(TestStruct.NestedTestStruct),
					new DecomposedType(
						typeof(TestStruct.NestedTestStruct),
						typeof(TestStruct.NestedTestStruct),
						DecomposedType.EmptyTypes)
				};

				// closed generic type:
				// GenericTestStruct<int>
				yield return new object[]
				{
					typeof(GenericTestStruct<int>),
					new DecomposedType(
						typeof(GenericTestStruct<int>),
						typeof(GenericTestStruct<>),
						new List<DecomposedType>
						{
							new DecomposedType(typeof(int), typeof(int), DecomposedType.EmptyTypes)
						})
				};

				// closed generic type:
				// GenericTestStruct<int>.NestedTestStruct
				yield return new object[]
				{
					typeof(GenericTestStruct<int>.NestedTestStruct),
					new DecomposedType(
						typeof(GenericTestStruct<int>.NestedTestStruct),
						typeof(GenericTestStruct<>.NestedTestStruct),
						new List<DecomposedType>
						{
							new DecomposedType(typeof(int), typeof(int), DecomposedType.EmptyTypes)
						})
				};

				// closed generic type:
				// GenericTestStruct<int>.NestedTestStruct<long>
				yield return new object[]
				{
					typeof(GenericTestStruct<int>.NestedGenericTestStruct<long>),
					new DecomposedType(
						typeof(GenericTestStruct<int>.NestedGenericTestStruct<long>),
						typeof(GenericTestStruct<>.NestedGenericTestStruct<>),
						new List<DecomposedType>
						{
							new DecomposedType(typeof(int), typeof(int), DecomposedType.EmptyTypes),
							new DecomposedType(typeof(long), typeof(long), DecomposedType.EmptyTypes)
						})
				};

				// closed generic type:
				// GenericTestStruct<int,uint>
				yield return new object[]
				{
					typeof(GenericTestStruct<int, uint>),
					new DecomposedType(
						typeof(GenericTestStruct<int, uint>),
						typeof(GenericTestStruct<,>),
						new List<DecomposedType>
						{
							new DecomposedType(typeof(int), typeof(int), DecomposedType.EmptyTypes),
							new DecomposedType(typeof(uint), typeof(uint), DecomposedType.EmptyTypes)
						})
				};

				// closed generic type:
				// GenericTestStruct<int,uint>.NestedTestStruct
				yield return new object[]
				{
					typeof(GenericTestStruct<int, uint>.NestedTestStruct),
					new DecomposedType(
						typeof(GenericTestStruct<int, uint>.NestedTestStruct),
						typeof(GenericTestStruct<,>.NestedTestStruct),
						new List<DecomposedType>
						{
							new DecomposedType(typeof(int), typeof(int), DecomposedType.EmptyTypes),
							new DecomposedType(typeof(uint), typeof(uint), DecomposedType.EmptyTypes)
						})
				};

				// closed generic type:
				// GenericTestStruct<int,uint>.NestedTestStruct<long,ulong>
				yield return new object[]
				{
					typeof(GenericTestStruct<int, uint>.NestedGenericTestStruct<long, ulong>),
					new DecomposedType(
						typeof(GenericTestStruct<int, uint>.NestedGenericTestStruct<long, ulong>),
						typeof(GenericTestStruct<,>.NestedGenericTestStruct<,>),
						new List<DecomposedType>
						{
							new DecomposedType(typeof(int), typeof(int), DecomposedType.EmptyTypes),
							new DecomposedType(typeof(uint), typeof(uint), DecomposedType.EmptyTypes),
							new DecomposedType(typeof(long), typeof(long), DecomposedType.EmptyTypes),
							new DecomposedType(typeof(ulong), typeof(ulong), DecomposedType.EmptyTypes)
						})
				};

				// partially open generic type:
				// GenericTestStruct<int,List<>>
				yield return new object[]
				{
					typeof(GenericTestStruct<,>).MakeGenericType(typeof(int), typeof(List<>)),
					new DecomposedType(
						typeof(GenericTestStruct<,>).MakeGenericType(typeof(int), typeof(List<>)),
						typeof(GenericTestStruct<,>),
						new List<DecomposedType>
						{
							new DecomposedType(
								typeof(int),
								typeof(int),
								DecomposedType.EmptyTypes),
							new DecomposedType(
								typeof(List<>), // open generic
								typeof(List<>),
								DecomposedType.EmptyTypes)
						})
				};

				///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// class
				///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

				// generic type definition:
				// GenericTestClass<>
				yield return new object[]
				{
					typeof(GenericTestClass<>),
					new DecomposedType(
						typeof(GenericTestClass<>),
						typeof(GenericTestClass<>),
						DecomposedType.EmptyTypes)
				};

				// generic type definition:
				// GenericTestClass<,>
				yield return new object[]
				{
					typeof(GenericTestClass<,>),
					new DecomposedType(
						typeof(GenericTestClass<,>),
						typeof(GenericTestClass<,>),
						DecomposedType.EmptyTypes)
				};

				// generic type definition:
				// GenericTestClass<,>.NestedTestStruct
				yield return new object[]
				{
					typeof(GenericTestClass<,>.NestedTestClass),
					new DecomposedType(
						typeof(GenericTestClass<,>.NestedTestClass),
						typeof(GenericTestClass<,>.NestedTestClass),
						DecomposedType.EmptyTypes)
				};

				// generic type definition:
				// GenericTestClass<,>.NestedGenericTestStruct<,>
				yield return new object[]
				{
					typeof(GenericTestClass<,>.NestedGenericTestClass<,>),
					new DecomposedType(
						typeof(GenericTestClass<,>.NestedGenericTestClass<,>),
						typeof(GenericTestClass<,>.NestedGenericTestClass<,>),
						DecomposedType.EmptyTypes)
				};

				// non-generic type:
				// TestClass
				yield return new object[]
				{
					typeof(TestClass),
					new DecomposedType(
						typeof(TestClass),
						typeof(TestClass),
						DecomposedType.EmptyTypes)
				};

				// non-generic type:
				// TestClass.NestedTestClass
				yield return new object[]
				{
					typeof(TestClass.NestedTestClass),
					new DecomposedType(
						typeof(TestClass.NestedTestClass),
						typeof(TestClass.NestedTestClass),
						DecomposedType.EmptyTypes)
				};

				// closed generic type:
				// GenericTestClass<int>
				yield return new object[]
				{
					typeof(GenericTestClass<int>),
					new DecomposedType(
						typeof(GenericTestClass<int>),
						typeof(GenericTestClass<>),
						new List<DecomposedType>
						{
							new DecomposedType(typeof(int), typeof(int), DecomposedType.EmptyTypes)
						})
				};

				// closed generic type:
				// GenericTestClass<int>.NestedTestClass
				yield return new object[]
				{
					typeof(GenericTestClass<int>.NestedTestClass),
					new DecomposedType(
						typeof(GenericTestClass<int>.NestedTestClass),
						typeof(GenericTestClass<>.NestedTestClass),
						new List<DecomposedType>
						{
							new DecomposedType(typeof(int), typeof(int), DecomposedType.EmptyTypes)
						})
				};

				// closed generic type:
				// GenericTestClass<int>.NestedTestClass<long>
				yield return new object[]
				{
					typeof(GenericTestClass<int>.NestedGenericTestClass<long>),
					new DecomposedType(
						typeof(GenericTestClass<int>.NestedGenericTestClass<long>),
						typeof(GenericTestClass<>.NestedGenericTestClass<>),
						new List<DecomposedType>
						{
							new DecomposedType(typeof(int), typeof(int), DecomposedType.EmptyTypes),
							new DecomposedType(typeof(long), typeof(long), DecomposedType.EmptyTypes)
						})
				};

				// closed generic type:
				// GenericTestClass<int,uint>
				yield return new object[]
				{
					typeof(GenericTestClass<int, uint>),
					new DecomposedType(
						typeof(GenericTestClass<int, uint>),
						typeof(GenericTestClass<,>),
						new List<DecomposedType>
						{
							new DecomposedType(typeof(int), typeof(int), DecomposedType.EmptyTypes),
							new DecomposedType(typeof(uint), typeof(uint), DecomposedType.EmptyTypes)
						})
				};

				// closed generic type:
				// GenericTestClass<int,uint>.NestedTestClass
				yield return new object[]
				{
					typeof(GenericTestClass<int, uint>.NestedTestClass),
					new DecomposedType(
						typeof(GenericTestClass<int, uint>.NestedTestClass),
						typeof(GenericTestClass<,>.NestedTestClass),
						new List<DecomposedType>
						{
							new DecomposedType(typeof(int), typeof(int), DecomposedType.EmptyTypes),
							new DecomposedType(typeof(uint), typeof(uint), DecomposedType.EmptyTypes)
						})
				};

				// closed generic type:
				// GenericTestClass<int,uint>.NestedTestClass<long,ulong>
				yield return new object[]
				{
					typeof(GenericTestClass<int, uint>.NestedGenericTestClass<long, ulong>),
					new DecomposedType(
						typeof(GenericTestClass<int, uint>.NestedGenericTestClass<long, ulong>),
						typeof(GenericTestClass<,>.NestedGenericTestClass<,>),
						new List<DecomposedType>
						{
							new DecomposedType(typeof(int), typeof(int), DecomposedType.EmptyTypes),
							new DecomposedType(typeof(uint), typeof(uint), DecomposedType.EmptyTypes),
							new DecomposedType(typeof(long), typeof(long), DecomposedType.EmptyTypes),
							new DecomposedType(typeof(ulong), typeof(ulong), DecomposedType.EmptyTypes)
						})
				};

				// partially open generic type:
				// GenericTestClass<int,List<>>
				yield return new object[]
				{
					typeof(GenericTestClass<,>).MakeGenericType(typeof(int), typeof(List<>)),
					new DecomposedType(
						typeof(GenericTestClass<,>).MakeGenericType(typeof(int), typeof(List<>)),
						typeof(GenericTestClass<,>),
						new List<DecomposedType>
						{
							new DecomposedType(
								typeof(int),
								typeof(int),
								DecomposedType.EmptyTypes),
							new DecomposedType(
								typeof(List<>), // open generic
								typeof(List<>),
								DecomposedType.EmptyTypes)
						})
				};
			}
		}
	}

}
