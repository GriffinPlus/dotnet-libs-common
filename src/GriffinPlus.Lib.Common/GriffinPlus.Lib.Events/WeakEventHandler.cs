///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/GriffinPlus/dotnet-libs-common)
//
// Copyright 2018-2019 Sascha Falk <sascha@falk-online.eu>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for
// the specific language governing permissions and limitations under the License.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace GriffinPlus.Lib.Events
{
	/// <summary>
	/// A weak event handler that provides a way to reference event receipients without preventing them from being
	/// garbage collected.
	/// </summary>
	/// <typeparam name="EVENT_ARGS">Event arguments passed to the event handler.</typeparam>
	public class WeakEventHandler<EVENT_ARGS> where EVENT_ARGS: EventArgs
	{
		#region Types

		internal delegate void InvokeDelegate(object target, object sender, EVENT_ARGS e);

		#endregion

		#region Class Variables

		/// <summary>
		/// Stores dynamically created delegates (value) that invoke a certain event handler method (key).
		/// </summary>
		private static Dictionary<MethodInfo,InvokeDelegate> sInvokeMethods = new Dictionary<MethodInfo,InvokeDelegate>();

		#endregion

		#region Member Variables

		internal readonly WeakReference       mTarget;
		internal readonly MethodInfo          mMethod;
		internal readonly InvokeDelegate      mInvoke;

		#endregion

		#region Construction

		/// <summary>
		/// Creates a new instance of the <see cref="WeakEventHandler{EVENT_ARGS}"/> class.
		/// </summary>
		/// <param name="handler">Event handler to wrap in the weak event handler.</param>
		public WeakEventHandler(EventHandler<EVENT_ARGS> handler)
		{
			mTarget = handler.Target != null ? new WeakReference(handler.Target) : null;
			mMethod = handler.Method;

			lock (sInvokeMethods)
			{
				if (!sInvokeMethods.TryGetValue(handler.Method, out mInvoke))
				{
					ParameterExpression[] parameterExpressions = new [] {
						Expression.Parameter(typeof(object), "target"),
						Expression.Parameter(typeof(object), "sender"),
						Expression.Parameter(typeof(EVENT_ARGS), "e")
					};

					LambdaExpression callerExpression = Expression.Lambda(
						typeof(InvokeDelegate),
						Expression.Call(
							Expression.Convert(parameterExpressions[0], handler.Method.DeclaringType),
							handler.Method, parameterExpressions[1], parameterExpressions[2]),
						parameterExpressions);


					mInvoke = (InvokeDelegate)callerExpression.Compile();
					sInvokeMethods.Add(handler.Method, mInvoke);
				}
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Checks whether the event handler is valid (true) or whether the target object has been garbage collected (false).
		/// </summary>
		public bool IsValid
		{
			get { return mTarget == null || mTarget.IsAlive; }
		}

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
		public bool Invoke(object sender, EVENT_ARGS e)
		{
			if (mTarget != null)
			{
				// event handler is an instance method
				object target = mTarget.Target;
				if (target != null)
				{
					// the target instance is still alive
					// => invoke event handler...
					mInvoke(target, sender, e);
					return true;
				}
				else
				{
					// object has been garbage collected
					return false;
				}
			}
			else
			{
				// event handler is a static method
				// => invoke event handler...
				mInvoke(null, sender, e);
				return true;
			}
		}

		#endregion
	}
}
