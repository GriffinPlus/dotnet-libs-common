///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace GriffinPlus.Lib.Events
{

	/// <summary>
	/// A weak event handler that provides a way to reference event recipients without preventing them from being
	/// garbage collected.
	/// </summary>
	/// <typeparam name="TEventArgs">Event arguments passed to the event handler.</typeparam>
	public class WeakEventHandler<TEventArgs> where TEventArgs : EventArgs
	{
		#region Types

		internal delegate void InvokeDelegate(object target, object sender, TEventArgs e);

		#endregion

		#region Class Variables

		/// <summary>
		/// Stores dynamically created delegates (value) that invoke a certain event handler method (key).
		/// </summary>
		private static readonly Dictionary<MethodInfo, InvokeDelegate> sInvokeMethods = new Dictionary<MethodInfo, InvokeDelegate>();

		#endregion

		#region Member Variables

		internal readonly WeakReference  Target;
		internal readonly MethodInfo     Method;
		internal readonly InvokeDelegate Invoker;

		#endregion

		#region Construction

		/// <summary>
		/// Creates a new instance of the <see cref="WeakEventHandler{EVENT_ARGS}"/> class.
		/// </summary>
		/// <param name="handler">Event handler to wrap in the weak event handler.</param>
		public WeakEventHandler(EventHandler<TEventArgs> handler)
		{
			Target = handler.Target != null ? new WeakReference(handler.Target) : null;
			Method = handler.Method;

			lock (sInvokeMethods)
			{
				if (!sInvokeMethods.TryGetValue(handler.Method, out Invoker))
				{
					ParameterExpression[] parameterExpressions =
					{
						Expression.Parameter(typeof(object), "target"),
						Expression.Parameter(typeof(object), "sender"),
						Expression.Parameter(typeof(TEventArgs), "e")
					};

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
		}

		#endregion

		#region Properties

		/// <summary>
		/// Checks whether the event handler is valid (true) or whether the target object has been garbage collected (false).
		/// </summary>
		public bool IsValid => Target == null || Target.IsAlive;

		#endregion

		#region Raising the Event

		/// <summary>
		/// Raises the event handler.
		/// </summary>
		/// <param name="sender">Sender of the event to pass to the event handler.</param>
		/// <param name="e">Event arguments to pass to the event handler.</param>
		/// <returns>
		/// true, if the event handler was called successfully;
		/// false, if the event handler was not called, because the object it belonged to, was collected.
		/// </returns>
		public bool Invoke(object sender, TEventArgs e)
		{
			if (Target != null)
			{
				// event handler is an instance method
				object target = Target.Target;
				if (target != null)
				{
					// the target instance is still alive
					// => invoke event handler...
					Invoker(target, sender, e);
					return true;
				}

				// object has been garbage collected
				return false;
			}

			// event handler is a static method
			// => invoke event handler...
			Invoker(null, sender, e);
			return true;
		}

		#endregion
	}

}
