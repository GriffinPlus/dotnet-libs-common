///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GriffinPlus.Lib.Events
{

	/// <summary>
	/// Event manager that administrates event handlers in a central place.
	/// Made for events of the <see cref="EventHandler{TEventArgs}"/> type.
	/// Objects firing events do not need to implement own event add/remove logic, especially when it comes to firing
	/// events asynchronously.
	/// </summary>
	/// <typeparam name="T">Type of the event arguments of the event.</typeparam>
	public static class EventManager<T> where T : EventArgs
	{
		#region Internal Data Types

		/// <summary>
		/// An event handler item in the event manager.
		/// </summary>
		private struct Item
		{
			public readonly SynchronizationContext SynchronizationContext;
			public readonly EventHandler<T>        Handler;

			public Item(SynchronizationContext context, EventHandler<T> handler)
			{
				SynchronizationContext = context;
				Handler = handler;
			}
		}

		#endregion

		#region Class Variables

		private static readonly ConditionalWeakTable<object, Dictionary<string, Item[]>> mItemsByObject = new ConditionalWeakTable<object, Dictionary<string, Item[]>>();
		private static readonly object                                                   sSync          = new object();

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
			object                 obj,
			string                 eventName,
			EventHandler<T>        handler,
			SynchronizationContext context)
		{
			return RegisterEventHandler(obj, eventName, handler, context, false, null, null);
		}

		/// <summary>
		/// Registers an event handler for an event associated with the specified object and optionally fires the
		/// event handler after registration.
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
			object                 obj,
			string                 eventName,
			EventHandler<T>        handler,
			SynchronizationContext context,
			bool                   fireImmediately,
			object                 sender,
			T                      e)
		{
			if (handler == null) throw new ArgumentNullException(nameof(handler));

			Item[] newItems;

			lock (sSync)
			{
				Dictionary<string, Item[]> itemsByName;
				if (!mItemsByObject.TryGetValue(obj, out itemsByName))
				{
					itemsByName = new Dictionary<string, Item[]>(1);
					mItemsByObject.Add(obj, itemsByName);
				}

				Item[] items;
				if (itemsByName.TryGetValue(eventName, out items))
				{
					newItems = new Item[items.Length + 1];
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

			if (fireImmediately)
			{
				if (context != null)
				{
					context.Post(x => { handler(sender, e); }, null);
				}
				else
				{
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
				Dictionary<string, Item[]> itemsByName;
				if (!mItemsByObject.TryGetValue(obj, out itemsByName) || !itemsByName.TryGetValue(eventName, out items))
				{
					// specified event handler was not registered
					return -1;
				}

				for (int i = 0; i < items.Length; i++)
				{
					var registeredHandler = items[i].Handler;
					if (registeredHandler == handler)
					{
						var newItems = new Item[items.Length - 1];
						for (int j = 0, k = 0; j < items.Length; j++)
						{
							if (j != i) newItems[k++] = items[j];
						}

						if (newItems.Length > 0)
						{
							itemsByName[eventName] = newItems;
							return newItems.Length;
						}

						itemsByName.Remove(eventName);
						return 0;
					}
				}

				// specified event handler was not registered
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
				Dictionary<string, Item[]> itemsByName;
				if (!mItemsByObject.TryGetValue(obj, out itemsByName) || !itemsByName.TryGetValue(eventName, out items))
				{
					return false;
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
			lock (sSync)
			{
				Item[] items;
				Dictionary<string, Item[]> itemsByName;
				if (!mItemsByObject.TryGetValue(obj, out itemsByName) || !itemsByName.TryGetValue(eventName, out items))
				{
					return false;
				}

				for (int i = 0; i < items.Length; i++)
				{
					if (items[i].Handler == handler) return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Fires an event invoking all event handlers that are attached to it (event handlers that are associated
		/// with a synchronization context are executed in the thread the synchronization context belongs to).
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <param name="eventName">Name of the event.</param>
		/// <param name="sender">Sender object to pass to invoked event handlers.</param>
		/// <param name="e">Event arguments to pass to invoked event handlers.</param>
		public static void FireEvent(
			object obj,
			string eventName,
			object sender,
			T      e)
		{
			Item[] items;

			lock (sSync)
			{
				Dictionary<string, Item[]> itemsByName;
				if (!mItemsByObject.TryGetValue(obj, out itemsByName)) return;
				if (!itemsByName.TryGetValue(eventName, out items)) return;
			}

			foreach (var item in items)
			{
				if (item.SynchronizationContext != null)
				{
					item.SynchronizationContext.Post(x => { ((Item)x).Handler(sender, e); }, item);
				}
				else
				{
					item.Handler(sender, e);
				}
			}
		}

		/// <summary>
		/// Gets a multicast delegate that calls all event handlers that are attached to the specified event
		/// (event handlers that are associated with a synchronization context are executed in the thread the
		/// synchronization context belongs to).
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <param name="eventName">Name of the event.</param>
		/// <returns>Event callers.</returns>
		public static EventHandler<T> GetEventCallers(object obj, string eventName)
		{
			Item[] items;

			lock (sSync)
			{
				Dictionary<string, Item[]> itemsByName;
				if (!mItemsByObject.TryGetValue(obj, out itemsByName)) return null;
				if (!itemsByName.TryGetValue(eventName, out items)) return null;
			}

			EventHandler<T> handlers = null;

			foreach (var item in items)
			{
				if (item.SynchronizationContext != null)
				{
					handlers += (sender, e) =>
					{
						item.SynchronizationContext.Post(x => { ((Item)x).Handler(sender, e); }, item);
					};
				}
				else
				{
					var itemCopy = item;
					handlers += (sender, e) =>
					{
						itemCopy.Handler(sender, e);
					};
				}
			}

			return handlers;
		}
	}

}
