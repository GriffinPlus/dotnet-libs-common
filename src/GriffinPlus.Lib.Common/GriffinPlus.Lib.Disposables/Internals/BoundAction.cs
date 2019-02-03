///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/GriffinPlus/dotnet-libs-common)
//
// Copyright 2019 Sascha Falk <sascha@falk-online.eu>
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for
// the specific language governing permissions and limitations under the License.
//
// This file incorporates work covered by the following copyright and permission notice:
//
//     MIT License
//
//     Copyright (c) 2016 Stephen Cleary
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in all
//     copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//     SOFTWARE.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace GriffinPlus.Lib.Disposables.Internal
{
	/// <summary>
	/// A field containing a bound action.
	/// </summary>
	/// <typeparam name="T">The type of context for the action.</typeparam>
	internal sealed class BoundActionField<T>
	{
		private BoundAction mField;

		/// <summary>
		/// Initializes the field with the specified action and context.
		/// </summary>
		/// <param name="action">The action delegate.</param>
		/// <param name="context">The context.</param>
		public BoundActionField(Action<T> action, T context)
		{
			mField = new BoundAction(action, context);
		}

		/// <summary>
		/// Whether the field is empty.
		/// </summary>
		public bool IsEmpty => Interlocked.CompareExchange(ref mField, null, null) == null;

		/// <summary>
		/// Atomically retrieves the bound action from the field and sets the field to <c>null</c>.
		/// May return <c>null</c>.
		/// </summary>
		public IBoundAction TryGetAndUnset()
		{
			return Interlocked.Exchange(ref mField, null);
		}

		/// <summary>
		/// Attempts to update the context of the bound action stored in the field.
		/// Returns <c>false</c> if the field is <c>null</c>.
		/// </summary>
		/// <param name="contextUpdater">
		/// The function used to update an existing context.
		/// This may be called more than once, if more than one thread attempts to simultanously update the context.
		/// </param>
		public bool TryUpdateContext(Func<T, T> contextUpdater)
		{
			while (true)
			{
				var original = Interlocked.CompareExchange(ref mField, mField, mField);
				if (original == null) return false;
				var updatedContext = new BoundAction(original, contextUpdater);
				var result = Interlocked.CompareExchange(ref mField, updatedContext, original);
				if (ReferenceEquals(original, result)) return true;
			}
		}

		/// <summary>
		/// An action delegate bound with its context.
		/// </summary>
		public interface IBoundAction
		{
			/// <summary>
			/// Executes the action.
			/// This should only be done after the bound action is retrieved from a field by <see cref="TryGetAndUnset"/>.
			/// </summary>
			void Invoke();
		}

		private sealed class BoundAction : IBoundAction
		{
			private readonly Action<T> mAction;
			private readonly T mContext;

			public BoundAction(Action<T> action, T context)
			{
				mAction = action;
				mContext = context;
			}

			public BoundAction(BoundAction originalBoundAction, Func<T, T> contextUpdater)
			{
				mAction = originalBoundAction.mAction;
				mContext = contextUpdater(originalBoundAction.mContext);
			}

			public void Invoke() => mAction?.Invoke(mContext);
		}
	}
}