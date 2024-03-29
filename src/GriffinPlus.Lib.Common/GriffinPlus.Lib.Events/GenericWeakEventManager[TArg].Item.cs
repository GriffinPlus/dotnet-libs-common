﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace GriffinPlus.Lib.Events;

public static partial class GenericWeakEventManager<TArg>
{
	/// <summary>
	/// A weak event handler item in the event manager.
	/// </summary>
	private readonly struct Item(SynchronizationContext context, Action<TArg> handler, bool scheduleAlways)
	{
		public readonly  SynchronizationContext SynchronizationContext = context;
		public readonly  bool                   ScheduleAlways         = scheduleAlways;
		private readonly Handler                mHandler               = new(handler);

		public ItemMatchResult IsHandler(Action<TArg> handler)
		{
			if (mHandler.Method != handler.Method) return ItemMatchResult.NoMatch;
			if (mHandler.Target == null) return ItemMatchResult.Match;
			object target = mHandler.Target.Target;
			if (target == null) return ItemMatchResult.Collected;
			return ReferenceEquals(target, handler.Target) ? ItemMatchResult.Match : ItemMatchResult.NoMatch;
		}

		public bool IsValid => mHandler.IsValid;

		public bool Fire(TArg arg)
		{
			return mHandler.Invoke(arg);
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
