///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Events
{
	public class EventManagerEventArgs : EventArgs
	{
		public EventManagerEventArgs(string myString)
		{
			MyString = myString;
		}

		public string MyString { get; }
	}
}