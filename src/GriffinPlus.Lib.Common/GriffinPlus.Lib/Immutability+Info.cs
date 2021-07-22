///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib
{

	public static partial class Immutability
	{
		/// <summary>
		/// Some information about a type that has been analyzed for immutability.
		/// </summary>
		public class Info
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="Info"/> class.
			/// </summary>
			/// <param name="type">The analyzed type.</param>
			/// <param name="isImmutable"><c>true</c> if the type is considered immutable; otherwise <c>false</c>.</param>
			/// <param name="reason">Reason describing what led to the immutability evaluation.</param>
			public Info(Type type, bool isImmutable, string reason)
			{
				Type = type;
				IsImmutable = isImmutable;
				Reason = reason;
			}

			/// <summary>
			/// Gets the type the information is about.
			/// </summary>
			public Type Type { get; }

			/// <summary>
			/// Gets a value indicating whether the type is considered immutable.
			/// May be <c>false</c> although the type is in fact immutable (false-positive),
			/// if the immutability analysis was not 100% sure that the type is immutable.
			/// </summary>
			public bool IsImmutable { get; }

			/// <summary>
			/// Gets the reason describing what led to the immutability evaluation.
			/// </summary>
			public string Reason { get; }
		}
	}

}
