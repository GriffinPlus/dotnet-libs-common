///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace GriffinPlus.Lib.Events;

public static partial class GenericWeakEventManager<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>
{
	/// <summary>
	/// A weak event handler item in the event manager.
	/// </summary>
	private readonly struct Item(
		SynchronizationContext                                         context,
		Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> handler,
		bool                                                           scheduleAlways)
	{
		public readonly  SynchronizationContext SynchronizationContext = context;
		public readonly  bool                   ScheduleAlways         = scheduleAlways;
		private readonly Handler                mHandler               = new(handler);

		public ItemMatchResult IsHandler(Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> handler)
		{
			if (mHandler.Method != handler.Method) return ItemMatchResult.NoMatch;
			if (mHandler.Target == null) return ItemMatchResult.Match;
			object target = mHandler.Target.Target;
			if (target == null) return ItemMatchResult.Collected;
			return ReferenceEquals(target, handler.Target) ? ItemMatchResult.Match : ItemMatchResult.NoMatch;
		}

		public bool IsValid => mHandler.IsValid;

		public bool Fire(
			TArg1 arg1,
			TArg2 arg2,
			TArg3 arg3,
			TArg4 arg4,
			TArg5 arg5,
			TArg6 arg6,
			TArg7 arg7,
			TArg8 arg8)
		{
			return mHandler.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
		}
	}

	/// <summary>
	/// Result values indicating whether a handler has matched.
	/// </summary>
	private enum ItemMatchResult
	{
		Match,
		NoMatch,
		Collected
	}
}
