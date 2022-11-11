///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable ForCanBeConvertedToForeach

namespace GriffinPlus.Lib.Events
{

	/// <summary>
	/// Event manager that administrates weak event handlers in a central place.
	/// Objects firing events do not need to implement own event add/remove logic, especially when it comes to firing events
	/// in the context of the thread that registered an event handler.
	/// </summary>
	/// <typeparam name="TEventArgs">Type of the event arguments of the event.</typeparam>
	public static class WeakEventManager<TEventArgs> where TEventArgs : EventArgs
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
		}

		/// <summary>
		/// A weak event handler item in the event manager.
		/// </summary>
		private readonly struct Item
		{
			public readonly SynchronizationContext       SynchronizationContext;
			public readonly WeakEventHandler<TEventArgs> Handler;
			public readonly bool                         ScheduleAlways;

			public Item(SynchronizationContext context, EventHandler<TEventArgs> handler, bool scheduleAlways)
			{
				SynchronizationContext = context;
				Handler = new WeakEventHandler<TEventArgs>(handler);
				ScheduleAlways = scheduleAlways;
			}

			public ItemMatchResult IsHandler(EventHandler<TEventArgs> handler)
			{
				if (Handler.Method != handler.Method)
				{
					return ItemMatchResult.NoMatch;
				}

				if (Handler.Target != null)
				{
					object target = Handler.Target.Target;
					if (target == null) return ItemMatchResult.Collected;
					if (!ReferenceEquals(target, handler.Target)) return ItemMatchResult.NoMatch;
				}

				return ItemMatchResult.Match;
			}

			public bool IsValid => Handler.IsValid;

			public bool Fire(object sender, TEventArgs e)
			{
				return Handler.Invoke(sender, e);
			}
		}

		#endregion

		#region Class Variables

		private static readonly ConditionalWeakTable<object, Dictionary<string, Item[]>> sItemsByObject = new ConditionalWeakTable<object, Dictionary<string, Item[]>>();

		// ReSharper disable once StaticMemberInGenericType
		private static readonly object sSync = new object();

		#endregion

		/// <summary>
		/// Registers an event handler for an event associated with the specified object.
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <param name="eventName">Name of the event.</param>
		/// <param name="handler">Event handler to register.</param>
		/// <param name="context">Synchronization context to use when calling the event handler (may be null).</param>
		/// <param name="scheduleAlways">
		/// If <paramref name="context"/> is set:
		/// <c>true</c> to always schedule the event handler in the specified synchronization context,
		/// <c>false</c> to schedule the event handler in the specified context only, if the thread firing the event has some other synchronization context.
		/// If <paramref name="context"/> is <c>null</c>:
		/// <c>true</c> to always schedule the event handler in a worker thread,
		/// <c>false</c> to invoke the event handler in the thread that is firing the event (direct call).
		/// </param>
		/// <returns>Total number of registered event handlers (including the specified event handler).</returns>
		public static int RegisterEventHandler(
			object                   obj,
			string                   eventName,
			EventHandler<TEventArgs> handler,
			SynchronizationContext   context,
			bool                     scheduleAlways)
		{
			return RegisterEventHandler(obj, eventName, handler, context, scheduleAlways, false, null, null);
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
		/// <param name="scheduleAlways">
		/// If <paramref name="context"/> is set:
		/// <c>true</c> to always schedule the event handler in the specified synchronization context,
		/// <c>false</c> to schedule the event handler in the specified context only, if the thread firing the event has some other synchronization context.
		/// If <paramref name="context"/> is <c>null</c>:
		/// <c>true</c> to always schedule the event handler in a worker thread,
		/// <c>false</c> to invoke the event handler in the thread that is firing the event (direct call).
		/// </param>
		/// <param name="fireImmediately">
		/// true to register and fire the event handler immediately after registration;
		/// false to register the event handler only.
		/// </param>
		/// <param name="sender">Sender object to pass to the event handler that is fired immediately.</param>
		/// <param name="e">Event arguments to pass to the event handler that is fired immediately.</param>
		/// <returns>Total number of registered event handlers (including the specified event handler).</returns>
		public static int RegisterEventHandler(
			object                   obj,
			string                   eventName,
			EventHandler<TEventArgs> handler,
			SynchronizationContext   context,
			bool                     scheduleAlways,
			bool                     fireImmediately,
			object                   sender,
			TEventArgs               e)
		{
			if (handler == null) throw new ArgumentNullException(nameof(handler));

			Item[] newItems;

			lock (sSync)
			{
				if (!sItemsByObject.TryGetValue(obj, out Dictionary<string, Item[]> itemsByName))
				{
					itemsByName = new Dictionary<string, Item[]>(1);
					sItemsByObject.Add(obj, itemsByName);
				}

				if (itemsByName.TryGetValue(eventName, out Item[] items))
				{
					newItems = new Item[items.Length + 1];
					Array.Copy(items, newItems, items.Length);
					newItems[items.Length] = new Item(context, handler, scheduleAlways);
					itemsByName[eventName] = newItems;
				}
				else
				{
					newItems = new Item[1];
					newItems[0] = new Item(context, handler, scheduleAlways);
					itemsByName[eventName] = newItems;
				}
			}

			if (fireImmediately)
			{
				if (context != null)
				{
					if (scheduleAlways) context.Post(_ => handler(sender, e), null);
					else handler(sender, e);
				}
				else
				{
					if (scheduleAlways) Task.Run(() => handler(sender, e));
					else handler(sender, e);
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
		public static int UnregisterEventHandler(object obj, string eventName, EventHandler<TEventArgs> handler)
		{
			if (handler == null) throw new ArgumentNullException(nameof(handler));

			lock (sSync)
			{
				if (!sItemsByObject.TryGetValue(obj, out Dictionary<string, Item[]> itemsByName) || !itemsByName.TryGetValue(eventName, out Item[] items))
					return -1; // specified event handler was not registered

				// remove specified handler from the handler list and tidy up handlers of collected objects as well
				var newItems = new List<Item>();
				bool removed = false;
				for (int i = 0; i < items.Length; i++)
				{
					Item item = items[i];
					ItemMatchResult matchResult = item.IsHandler(handler);
					if (matchResult == ItemMatchResult.Match)
					{
						removed = true;
					}
					else if (matchResult == ItemMatchResult.NoMatch)
					{
						newItems.Add(item);
					}
				}

				// exchange handler list
				if (newItems.Count == 0)
				{
					itemsByName.Remove(eventName);
					if (itemsByName.Count == 0) sItemsByObject.Remove(obj);
					if (removed) return 0;
				}
				else if (newItems.Count != items.Length)
				{
					itemsByName[eventName] = newItems.ToArray();
					if (removed) return newItems.Count;
				}

				// handler was not removed
				return -1;
			}
		}

		/// <summary>
		/// Unregisters all event handlers from the specified event.
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <param name="eventName">Name of the event (<c>null</c> to remove all handlers attached to events of the specified object).</param>
		/// <returns>
		/// true, if a least one event handler has been removed;
		/// false, if no event handler was registered.
		/// </returns>
		public static bool UnregisterEventHandlers(object obj, string eventName)
		{
			lock (sSync)
			{
				if (eventName != null)
				{
					// abort, if there is no event handler attached to the specified event
					if (!sItemsByObject.TryGetValue(obj, out Dictionary<string, Item[]> itemsByName) || !itemsByName.TryGetValue(eventName, out _))
						return false;

					// remove all handlers attached to the specified event
					bool removed = itemsByName.Remove(eventName);
					if (itemsByName.Count == 0) sItemsByObject.Remove(obj);
					return removed;
				}

				// remove all handlers attached to events of the specified object
				return sItemsByObject.Remove(obj);
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
				if (!sItemsByObject.TryGetValue(obj, out Dictionary<string, Item[]> itemsByName) || !itemsByName.TryGetValue(eventName, out Item[] items))
					return false;

				bool valid = true;
				for (int i = 0; i < items.Length; i++)
				{
					if (!items[i].IsValid)
					{
						valid = false;
						break;
					}
				}

				if (!valid)
				{
					// get handlers of vivid objects
					var newItems = new List<Item>();
					for (int i = 0; i < items.Length; i++)
					{
						if (items[i].IsValid) newItems.Add(items[i]);
					}

					// exchange handler list
					if (newItems.Count == 0)
					{
						itemsByName.Remove(eventName);
						if (itemsByName.Count == 0) sItemsByObject.Remove(obj);
						return false;
					}

					itemsByName[eventName] = newItems.ToArray();
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
		public static bool IsHandlerRegistered(object obj, string eventName, EventHandler<TEventArgs> handler)
		{
			bool registered = false;

			lock (sSync)
			{
				if (!sItemsByObject.TryGetValue(obj, out Dictionary<string, Item[]> itemsByName) || !itemsByName.TryGetValue(eventName, out Item[] items))
					return false;

				bool valid = true;
				for (int i = 0; i < items.Length; i++)
				{
					ItemMatchResult match = items[i].IsHandler(handler);
					if (match == ItemMatchResult.Match) registered = true;
					if (match == ItemMatchResult.Collected) valid = false;
				}

				if (!valid)
				{
					// get handlers of vivid objects
					var newItems = new List<Item>();
					for (int i = 0; i < items.Length; i++)
					{
						if (items[i].IsValid)
							newItems.Add(items[i]);
					}

					// exchange handler list
					if (newItems.Count == 0)
					{
						itemsByName.Remove(eventName);
						if (itemsByName.Count == 0) sItemsByObject.Remove(obj);
						return false;
					}

					itemsByName[eventName] = newItems.ToArray();
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
		public static void FireEvent(
			object     obj,
			string     eventName,
			object     sender,
			TEventArgs e)
		{
			Item[] items;

			lock (sSync)
			{
				items = CleanupAndGetHandlers(obj, eventName);
			}

			// abort, if no handlers have been registered
			if (items == null)
				return;

			// fire event
			foreach (Item item in items)
			{
				if (item.SynchronizationContext != null)
				{
					// synchronization context was specified at registration
					// => invoke the handler directly, if the current context is the same as the context at registration and scheduling is not enforced;
					//    otherwise schedule the handler using the context specified at registration
					if (!item.ScheduleAlways && ReferenceEquals(SynchronizationContext.Current, item.SynchronizationContext))
					{
						item.Fire(sender, e);
					}
					else
					{
						item.SynchronizationContext.Post(x => { ((Item)x).Fire(sender, e); }, item);
					}
				}
				else
				{
					// synchronization context was not specified at registration
					// => schedule handler in worker thread or invoke it directly
					if (item.ScheduleAlways) Task.Run(() => item.Fire(sender, e));
					else item.Fire(sender, e);
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
		public static EventHandler<TEventArgs> GetEventCallers(object obj, string eventName)
		{
			Item[] items;

			lock (sSync)
			{
				items = CleanupAndGetHandlers(obj, eventName);
			}

			EventHandler<TEventArgs> handlers = null;

			if (items != null)
			{
				foreach (Item item in items)
				{
					if (item.SynchronizationContext != null)
					{
						// synchronization context was specified at registration
						// => invoke the handler directly, if the current context is the same as the context at registration and scheduling is not enforced;
						//    otherwise schedule the handler using the context specified at registration
						handlers += (sender, e) =>
						{
							if (!item.ScheduleAlways && ReferenceEquals(SynchronizationContext.Current, item.SynchronizationContext))
							{
								item.Fire(sender, e);
							}
							else
							{
								item.SynchronizationContext.Post(x => { ((Item)x).Fire(sender, e); }, item);
							}
						};
					}
					else
					{
						// synchronization context was not specified at registration
						// => schedule handler in worker thread or invoke it directly
						Item itemCopy = item;
						handlers += (sender, e) =>
						{
							if (itemCopy.ScheduleAlways) Task.Run(() => itemCopy.Fire(sender, e));
							else itemCopy.Fire(sender, e);
						};
					}
				}
			}

			return handlers;
		}

		/// <summary>
		/// Checks whether registered event handlers are still valid, removes invalid handlers and returns the cleaned up handler items.
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <param name="eventName">Name of the event.</param>
		/// <returns>The cleaned up handler items; null, if no handlers are left.</returns>
		private static Item[] CleanupAndGetHandlers(object obj, string eventName)
		{
			if (!sItemsByObject.TryGetValue(obj, out Dictionary<string, Item[]> itemsByName)) return null;
			if (!itemsByName.TryGetValue(eventName, out Item[] items)) return null;

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

			// abort, if all handlers are still valid
			if (itemsToRemove == null)
				return items;

			// remove handlers that are not valid any more
			if (itemsToRemove.Count == items.Length)
			{
				// all handlers have to be removed
				itemsByName.Remove(eventName);
				if (itemsByName.Count == 0) sItemsByObject.Remove(obj);
				return null;
			}

			// some handlers have to be removed
			var newItems = new List<Item>(items);
			for (int i = itemsToRemove.Count - 1; i >= 0; i--)
			{
				newItems.RemoveAt(itemsToRemove[i]);
			}

			return itemsByName[eventName] = newItems.ToArray();
		}
	}

}
