///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.Configuration
{

	/// <summary>
	/// An exception that is thrown, if an error occurs in a configuration (missing configuration items, type mismatch etc.).
	/// </summary>
	[Serializable]
	public class ConfigurationException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationException"/> class.
		/// </summary>
		public ConfigurationException() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationException"/> class.
		/// </summary>
		/// <param name="message">Message describing the reason why the exception is thrown.</param>
		public ConfigurationException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationException"/> class.
		/// </summary>
		/// <param name="format">String that is used to format the final message describing the reason why the exception is thrown.</param>
		/// <param name="args">Arguments used to format the final exception message.</param>
		public ConfigurationException(string format, params object[] args) :
			base(string.Format(format, args)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationException"/> class.
		/// </summary>
		/// <param name="message">Message describing the reason why the exception is thrown.</param>
		/// <param name="ex">Some other exception that caused the exception to be thrown.</param>
		public ConfigurationException(string message, Exception ex) : base(message, ex) { }
	}

}
