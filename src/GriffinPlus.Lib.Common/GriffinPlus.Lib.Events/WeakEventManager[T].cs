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
using System.Threading;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace GriffinPlus.Lib.Events
{
	/// <summary>
	/// Event manager that administrates weak event handlers in a central place.
	/// Objects firing events do not need to implement own event add/remove logic, especially when it comes to firing events
	/// in the context of the thread that registered an event handler.
	/// </summary>
	/// <typeparam name="T">Type of the event arguments of the event.</typeparam>
	public static class WeakEventManager<T> where T: EventArgs
	{
		#region Internal Data Types

		/// <summary>
		/// Result values indicating whether a handler has matched.
		/// </summary>
		private enum ItemMatchResult
		{
			Match,
			NoMatch,
			Collected
		};

		/// <summary>
		/// A weak event handler item in the event manager.
		/// </summary>
		private struct Item
		{
			public SynchronizationContext SynchronizationContext;
			public WeakEventHandler<T> Handler;

			public Item(SynchronizationContext context, EventHandler<T> handler)
			{
				SynchronizationContext = context;
				Handler = new WeakEventHandler<T>(handler);
			}

			public ItemMatchResult IsHandler(EventHandler<T> handler)
			{
				if (Handler.mMethod != handler.Method) {
					return ItemMatchResult.NoMatch;
				}

				if (Handler.mTarget != null) {
					object target = Handler.mTarget.Target;
					if (target == null) return ItemMatchResult.Collected;
					if (!object.ReferenceEquals(target, handler.Target)) return ItemMatchResult.NoMatch;
				}

				return ItemMatchResult.Match;
			}

			public bool IsValid
			{
				get { return Handler.IsValid; }
			}

			public bool Fire(object sender, T e)
			{
				return Handler.Invoke(sender, e);
			}
		}

		#endregion

		#region Class Variables

		private static ConditionalWeakTable<object,Dictionary<string,Item[]>> mItemsByObject = new ConditionalWeakTable<object,Dictionary<string,Item[]>>();
		private static object sSync = new object();

		#endregion

		/// <summary>
		/// Registers an event handler for an event associated with the specified object.
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <param name="eventName">Name of the event.</param>
		/// <param name="handler">Event handler to register.</param>
		/// <param name="context">
		/// Synchronization context to use when calling the event handler
		/// (<c>null</c> to execute the event handler in the context of the thread firing the event).
		/// </param>
		/// <returns>Total number of registered event handlers (including the specified event handler).</returns>
		public static int RegisterEventHandler(
			object obj,
			string eventName,
			EventHandler<T> handler,
			SynchronizationContext context)
		{
			return RegisterEventHandler(obj, eventName, handler, context, false, null, null);
		}

		/// <summary>
		/// Registers an event handler for an event associated with the specified object and optionally fires the event handler after registration.
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <param name="eventName">Name of the event.</param>
		/// <param name="handler">Event handler to register.</param>
		/// <param name="context">
		/// Synchronization context to use when calling the event handler
		/// (<c>null</c> to execute the event handler in the context of the thread firing the event).
		/// </param>
		/// <param name="fireImmediately">
		/// true to register and fire the event handler immediately after registration;
		/// false to register the event handler only.
		/// </param>
		/// <param name="sender">Sender object to pass to the event handler that is fired immediately.</param>
		/// <param name="e">Event arguments to pass to the event handler that is fired immediately.</param>
		/// <returns>Total number of registered event handlers (including the specified event handler).</returns>
		public static int RegisterEventHandler(
			object obj,
			string eventName,
			EventHandler<T> handler,
			SynchronizationContext context,
			bool fireImmediately,
			object sender,
			T e)
		{
			if (handler == null) throw new ArgumentNullException(nameof(handler));

			Item[] newItems;

			lock (sSync)
			{
				Dictionary<string,Item[]> itemsByName;
				if (!mItemsByObject.TryGetValue(obj, out itemsByName)) {
					itemsByName = new Dictionary<string,Item[]>(1);
					mItemsByObject.Add(obj, itemsByName);
				}

				Item[] items;
				if (itemsByName.TryGetValue(eventName, out items))
				{
					newItems = new Item[items.Length+1];
					Array.Copy(items, newItems, items.Length);
					newItems[items.Length] = new Item(context, handler);
					itemsByName[eventName] = newItems;
				}
				else
				{
					newItems = new Item[1];
					newItems[0] = new Item(context, handler);
					itemsByName[eventName] = newItems;
				}
			}

			if (fireImmediately) {
				if (context != null) {
					context.Post((x) => { handler(sender, e); }, null);
				} else {
					handler(sender, e);
				}
			}

			return newItems.Length;
		}

		/// <summary>
		/// Unregisters an event handler from the specified event.
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <param name="eventName">Name of the event.</param>
		/// <param name="handler">Event handler to unregister.</param>
		/// <returns>
		/// Total number of registered event handlers after the specified handler has been removed;
		/// -1, if the specified event handler is not registered.
		/// </returns>
		public static int UnregisterEventHandler(object obj, string eventName, EventHandler<T> handler)
		{
			if (handler == null) throw new ArgumentNullException(nameof(handler));

			lock (sSync)
			{
				Item[] items;
				Dictionary<string,Item[]> itemsByName;
				if (!mItemsByObject.TryGetValue(obj, out itemsByName) || !itemsByName.TryGetValue(eventName, out items)) {
					// specified event handler was not registered
					return -1;
				}

				// remove specified handler from the handler list and tidy up handlers of collected objects as well
				List<Item> newItems = new List<Item>();
				bool removed = false;
				for (int i = 0; i < items.Length; i++) {
					Item item = items[i];
					ItemMatchResult matchResult = item.IsHandler(handler);
					if (matchResult == ItemMatchResult.Match) {
						removed = true;
					} else if (matchResult == ItemMatchResult.NoMatch) {
						newItems.Add(item);
					}
				}

				// exchange handler list
				if (newItems.Count == 0) {
					itemsByName.Remove(eventName);
					if (removed) return 0;
				} else if (newItems.Count != items.Length) {
					itemsByName[eventName] = newItems.ToArray();
					if (removed) return newItems.Count;
				}

				// handler was not removed
				return -1;
			}
		}

		/// <summary>
		/// Checks whether the specified event has event handlers attached to it.
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <param name="eventName">Name of the event.</param>
		/// <returns>true, if the specified event has event handlers attached; otherwise false.</returns>
		public static bool IsHandlerRegistered(object obj, string eventName)
		{
			lock (sSync)
			{
				Item[] items;
				Dictionary<string,Item[]> itemsByName;
				if (!mItemsByObject.TryGetValue(obj, out itemsByName) || !itemsByName.TryGetValue(eventName, out items)) {
					return false;
				}

				bool valid = true;
				for (int i = 0; i < items.Length; i++) {
					if (!items[i].IsValid) {
						valid = false;
						break;
					}
				}

				if (!valid)
				{
					// get handlers of vivid objects
					List<Item> newItems = new List<Item>();
					for (int i = 0; i < items.Length; i++) {
						if (items[i].IsValid) newItems.Add(items[i]);
					}

					// exchange handler list
					if (newItems.Count == 0) {
						itemsByName.Remove(eventName);
						return false;
					} else {
						itemsByName[eventName] = newItems.ToArray();
					}
				}

				return true;
			}
		}

		/// <summary>
		/// Checks whether the specified event handler is registered for the specified event.
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <param name="eventName">Name of the event.</param>
		/// <param name="handler">Event handler to check for.</param>
		/// <returns>true, if the specified event handler is attached to the event; otherwise false.</returns>
		public static bool IsHandlerRegistered(object obj, string eventName, EventHandler<T> handler)
		{
			bool registered = false;

			lock (sSync)
			{
				Item[] items;
				Dictionary<string,Item[]> itemsByName;
				if (!mItemsByObject.TryGetValue(obj, out itemsByName) || !itemsByName.TryGetValue(eventName, out items)) {
					return false;
				}

				bool valid = true;
				for (int i = 0; i < items.Length; i++) {
					ItemMatchResult match = items[i].IsHandler(handler);
					if (match == ItemMatchResult.Match) registered = true;
					if (match == ItemMatchResult.Collected) valid = false;
				}

				if (!valid)
				{
					// get handlers of vivid objects
					List<Item> newItems = new List<Item>();
					for (int i = 0; i < items.Length; i++) {
						if (items[i].IsValid) newItems.Add(items[i]);
					}

					// exchange handler list
					if (newItems.Count == 0) {
						itemsByName.Remove(eventName);
						return false;
					} else {
						itemsByName[eventName] = newItems.ToArray();
					}
				}
			}

			return registered;
		}

		/// <summary>
		/// Fires an event invoking all event handlers that are attached to it (event handlers that are associated with
		/// a synchronization context are executed in the thread the synchronization context belongs to).
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <param name="eventName">Name of the event.</param>
		/// <param name="sender">Sender object to pass to invoked event handlers.</param>
		/// <param name="e">Event arguments to pass to invoked event handlers.</param>
		public static void FireEvent(object obj, string eventName, object sender, T e)
		{
			Item[] items;

			lock (sSync)
			{
				Dictionary<string, Item[]> itemsByName;
				if (!mItemsByObject.TryGetValue(obj, out itemsByName)) return;
				if (!itemsByName.TryGetValue(eventName, out items)) return;
				items = RemoveInvalidHandlers(eventName, items, itemsByName);
			}

			// fire event
			foreach (Item item in items) {
				if (item.SynchronizationContext != null) {
					item.SynchronizationContext.Post((x) => { ((Item)x).Fire(sender, e); }, item);
				} else {
					item.Fire(sender, e);
				}
			}
		}

		/// <summary>
		/// Gets a multicast delegate that calls all event handlers that are attached to the specified event
		/// (event handlers that are associated with a synchronization context are executed in the thread the synchronization
		/// context belongs to).
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <param name="eventName">Name of the event.</param>
		/// <returns>Event callers.</returns>
		public static EventHandler<T> GetEventCallers(object obj, string eventName)
		{
			Item[] items;

			lock (sSync)
			{
				Dictionary<string,Item[]> itemsByName;
				if (!mItemsByObject.TryGetValue(obj, out itemsByName)) return null;
				if (!itemsByName.TryGetValue(eventName, out items)) return null;
				items = RemoveInvalidHandlers(eventName, items, itemsByName);
			}

			EventHandler<T> handlers = null;

			foreach (Item item in items)
			{
				if (item.SynchronizationContext != null)
				{
					handlers += (sender, e) => {
						item.SynchronizationContext.Post((x) => { ((Item)x).Fire(sender, e); }, item);
					};
				}
				else
				{
					Item itemCopy = item;
					handlers += (sender, e) => {
						itemCopy.Fire(sender, e);
					};
				}
			}

			return handlers;
		}

		/// <summary>
		/// Checks whether registered event handlers are still valid, removes invalid handlers and returns the cleaned up handler items.
		/// </summary>
		/// <param name="eventName">Name of the event the handlers belong to.</param>
		/// <param name="items">Handler items to check.</param>
		/// <param name="itemsByName">Event name to handler items mapping (will receive the modified handler items).</param>
		/// <returns>The cleaned up handler items.</returns>
		private static Item[] RemoveInvalidHandlers(string eventName, Item[] items, Dictionary<string, Item[]> itemsByName)
		{
			// check whether the handlers are still valid
			List<int> itemsToRemove = null;
			for (int i = 0; i < items.Length; i++)
			{
				Item item = items[i];
				if (!item.IsValid)
				{
					if (itemsToRemove == null) itemsToRemove = new List<int>();
					itemsToRemove.Add(i);
				}
			}

			// remove handlers that are not valid any more
			if (itemsToRemove != null)
			{
				if (itemsToRemove.Count == items.Length)
				{
					// all handlers have to be removed
					itemsByName.Remove(eventName);
				}
				else
				{
					// some handlers have to be removed
					List<Item> newItems = new List<Item>(items);
					for (int i = itemsToRemove.Count - 1; i >= 0; i--) {
						newItems.RemoveAt(itemsToRemove[i]);
					}

					itemsByName[eventName] = newItems.ToArray();
				}
			}

			return items;
		}
	}

}
