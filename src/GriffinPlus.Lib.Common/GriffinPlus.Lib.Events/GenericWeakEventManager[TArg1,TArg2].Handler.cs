///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace GriffinPlus.Lib.Events;

public partial class GenericWeakEventManager<TArg1, TArg2>
{
	/// <summary>
	/// A weak event handler that provides a way to reference event recipients without preventing them from being
	/// garbage collected.
	/// </summary>
	public class Handler
	{
		internal delegate void InvokeDelegate(object target, TArg1 arg1, TArg2 arg2);

		/// <summary>
		/// Stores dynamically created delegates (value) that invoke a certain event handler method (key).
		/// </summary>
		private static readonly Dictionary<MethodInfo, InvokeDelegate> sInvokeMethods = [];

		internal readonly WeakReference  Target;
		internal readonly MethodInfo     Method;
		internal readonly InvokeDelegate Invoker;

		/// <summary>
		/// Creates a new instance of the <see cref="Handler"/> class.
		/// </summary>
		/// <param name="handler">Event handler to wrap in the weak event handler.</param>
		public Handler(Action<TArg1, TArg2> handler)
		{
			Target = handler.Target != null ? new WeakReference(handler.Target) : null;
			Method = handler.Method;

			lock (sInvokeMethods)
			{
				if (sInvokeMethods.TryGetValue(handler.Method, out Invoker))
					return;

				ParameterExpression[] parameterExpressions =
				[
					Expression.Parameter(typeof(object), "target"),
					Expression.Parameter(typeof(TArg1), "arg1"),
					Expression.Parameter(typeof(TArg2), "arg2")
				];

				Debug.Assert(handler.Method.DeclaringType != null, "handler.Method.DeclaringType != null");

				LambdaExpression callerExpression = Expression.Lambda(
					typeof(InvokeDelegate),
					Expression.Call(
						Expression.Convert(parameterExpressions[0], handler.Method.DeclaringType),
						handler.Method,
						parameterExpressions[1],
						parameterExpressions[2]),
					parameterExpressions);


				Invoker = (InvokeDelegate)callerExpression.Compile();
				sInvokeMethods.Add(handler.Method, Invoker);
			}
		}


		/// <summary>
		/// Checks whether the event handler is valid (<c>true</c>) or
		/// whether the target object has been garbage collected (<c>false</c>).
		/// </summary>
		public bool IsValid => Target == null || Target.IsAlive;

		/// <summary>
		/// Raises the event handler.
		/// </summary>
		/// <param name="arg1">First argument to pass to the event handler.</param>
		/// <param name="arg2">Second argument to pass to the event handler.</param>
		/// <returns>
		/// <c>true</c> if the event handler was called successfully;<br/>
		/// <c>false</c> if the event handler was not called, because the object it belonged to, was collected.
		/// </returns>
		public bool Invoke(TArg1 arg1, TArg2 arg2)
		{
			if (Target != null)
			{
				// event handler is an instance method
				object target = Target.Target;

				// abort, if the object has been garbage collected
				if (target == null)
					return false;

				// the target instance is still alive
				// => invoke event handler...
				Invoker(target, arg1, arg2);
				return true;
			}

			// event handler is a static method
			// => invoke event handler...
			Invoker(null, arg1, arg2);
			return true;
		}
	}
}
