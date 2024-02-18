///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Threading;

// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery

namespace GriffinPlus.Lib.Threading
{

	/// <summary>
	/// Provides some information about synchronization contexts that may be needed to make decisions
	/// about marshalling calls to associated threads.
	/// </summary>
	public static class SynchronizationContextInfo
	{
		/// <summary>
		/// A predefined set of synchronization contexts that are known to be serializing
		/// (the full type name is used to avoid referencing the assemblies here)
		/// </summary>
		private static readonly string[] sPredefinedSynchronizingContexts =
		[
			"System.Windows.Forms.WindowsFormsSynchronizationContext",  // Windows Forms
			"System.Windows.Threading.DispatcherSynchronizationContext" // WPF
		];

		/// <summary>
		/// A set of synchronization contexts that are known to be serializing.
		/// </summary>
		private static Type[] sSerializingContextTypes = Type.EmptyTypes;

		/// <summary>
		/// Synchronizes accesses to data structures.
		/// </summary>
		private static readonly ReaderWriterLockSlim sLock = new(LockRecursionPolicy.NoRecursion);

		/// <summary>
		/// Adds the specified synchronization context to the list of serializing contexts,
		/// i.e. it takes care of running calls one after the other to avoid race conditions.
		/// </summary>
		/// <typeparam name="TSynchronizationContext">Synchronization context to register.</typeparam>
		public static void RegisterSerializingContext<TSynchronizationContext>() where TSynchronizationContext : SynchronizationContext
		{
			RegisterSerializingContext(typeof(TSynchronizationContext));
		}

		/// <summary>
		/// Adds the specified synchronization context to the list of serializing contexts,
		/// i.e. it takes care of running calls one after the other to avoid race conditions.
		/// </summary>
		/// <param name="synchronizationContextType">Synchronization context type to register.</param>
		/// <exception cref="ArgumentNullException">The specified type is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">The specified type does not derive from <see cref="SynchronizationContext"/>.</exception>
		public static void RegisterSerializingContext(Type synchronizationContextType)
		{
			if (synchronizationContextType == null) throw new ArgumentNullException(nameof(synchronizationContextType));
			if (!typeof(SynchronizationContext).IsAssignableFrom(synchronizationContextType)) throw new ArgumentException($"The type must derive from {typeof(SynchronizationContext).FullName}.");

			using (sLock.LockReadWrite())
			{
				if (!sSerializingContextTypes.Contains(synchronizationContextType))
				{
					var newTypes = new Type[sSerializingContextTypes.Length + 1];
					Array.Copy(sSerializingContextTypes, newTypes, sSerializingContextTypes.Length);
					newTypes[sSerializingContextTypes.Length] = synchronizationContextType;
					sSerializingContextTypes = newTypes;
				}
			}
		}

		/// <summary>
		/// Checks whether the specified synchronization context is known to be a serializing context,
		/// i.e. it takes care of running calls one after the other to avoid race conditions.
		/// </summary>
		/// <param name="context">Context to check.</param>
		/// <returns>
		/// <c>true</c>, if the synchronization context is known to be a serializing context;
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool IsSerializingSynchronizationContext(SynchronizationContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			Type contextType = context.GetType();
			bool registerType = false;

			using (sLock.LockReadOnly())
			{
				// check list of registered types
				for (int i = 0; i < sSerializingContextTypes.Length; i++)
				{
					if (sSerializingContextTypes[i] == contextType)
						return true;
				}

				// check list of predefined types (by full name)
				// and register the correct type object of the synchronization context, if it is one of the predefined types
				string contextTypeName = contextType.FullName;
				for (int i = 0; i < sPredefinedSynchronizingContexts.Length; i++)
				{
					if (sPredefinedSynchronizingContexts[i] == contextTypeName)
					{
						registerType = true;
						break;
					}
				}
			}

			if (registerType)
			{
				RegisterSerializingContext(contextType);
				return true;
			}

			return false;
		}
	}

}
