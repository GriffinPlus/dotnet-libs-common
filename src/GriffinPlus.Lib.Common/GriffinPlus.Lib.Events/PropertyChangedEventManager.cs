﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GriffinPlus.Lib.Events
{

	/// <summary>
	/// Event manager that administrates event handlers in a central place.
	/// Specifically made for <see cref="INotifyPropertyChanged.PropertyChanged"/> events.
	/// Objects firing events do not need to implement own event add/remove logic, especially when it comes to firing
	/// events asynchronously.
	/// </summary>
	static class PropertyChangedEventManager
	{
		#region Internal Data Types

		/// <summary>
		/// An event handler item in the event manager.
		/// </summary>
		private readonly struct Item
		{
			public readonly SynchronizationContext      SynchronizationContext;
			public readonly PropertyChangedEventHandler Handler;
			public readonly bool                        ScheduleAlways;

			public Item(SynchronizationContext context, PropertyChangedEventHandler handler, bool scheduleAlways)
			{
				SynchronizationContext = context;
				Handler = handler;
				ScheduleAlways = scheduleAlways;
			}
		}

		#endregion

		#region Class Variables

		private static readonly ConditionalWeakTable<object, Item[]> sItemsByObject =
			new ConditionalWeakTable<object, Item[]>();

		private static readonly object sSync = new object();

		#endregion

		/// <summary>
		/// Registers an event handler for an event associated with the specified object.
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
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
		/// <returns>Total number of registered event handlers (including the specified event handler).</returns>
		public static int RegisterEventHandler(
			object                      obj,
			PropertyChangedEventHandler handler,
			SynchronizationContext      context,
			bool                        scheduleAlways)
		{
			return RegisterEventHandler(
				obj,
				handler,
				context,
				scheduleAlways,
				false,
				null,
				null);
		}

		/// <summary>
		/// Registers an event handler for an event associated with the specified object and optionally fires the
		/// event handler after registration.
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
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
		/// <param name="sender">
		/// Sender object to pass to the event handler that is fired immediately.
		/// </param>
		/// <param name="propertyName">
		/// Name of the property that has changed (is passed to the event handler that is fired immediately).
		/// </param>
		/// <returns>
		/// Total number of registered event handlers (including the specified event handler).
		/// </returns>
		public static int RegisterEventHandler(
			object                      obj,
			PropertyChangedEventHandler handler,
			SynchronizationContext      context,
			bool                        scheduleAlways,
			bool                        fireImmediately,
			object                      sender,
			string                      propertyName)
		{
			Item[] newItems;

			lock (sSync)
			{
				if (sItemsByObject.TryGetValue(obj, out var items))
				{
					newItems = new Item[items.Length + 1];
					Array.Copy(items, newItems, items.Length);
					newItems[items.Length] = new Item(context, handler, scheduleAlways);
					sItemsByObject.Remove(obj);
					sItemsByObject.Add(obj, newItems);
				}
				else
				{
					newItems = new[] { new Item(context, handler, scheduleAlways) };
					sItemsByObject.Remove(obj);
					sItemsByObject.Add(obj, newItems);
				}
			}

			if (fireImmediately)
			{
				if (context != null)
				{
					if (scheduleAlways)
						context.Post(_ => handler(sender, new PropertyChangedEventArgs(propertyName)), null);
					else handler(sender, new PropertyChangedEventArgs(propertyName));
				}
				else
				{
					if (scheduleAlways) Task.Run(() => handler(sender, new PropertyChangedEventArgs(propertyName)));
					else handler(sender, new PropertyChangedEventArgs(propertyName));
				}
			}

			return newItems.Length;
		}

		/// <summary>
		/// Unregisters an event handler from the event.
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <param name="handler">Event handler to unregister.</param>
		/// <returns>
		/// Total number of registered event handlers after the specified handler has been removed;
		/// -1, if the specified event handler is not registered.
		/// </returns>
		public static int UnregisterEventHandler(object obj, PropertyChangedEventHandler handler)
		{
			lock (sSync)
			{
				if (!sItemsByObject.TryGetValue(obj, out var items))
					return -1; // specified event handler was not registered

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
							sItemsByObject.Remove(obj);
							sItemsByObject.Add(obj, newItems);
							return newItems.Length;
						}

						sItemsByObject.Remove(obj);
						return 0;
					}
				}

				// specified event handler was not registered
				return -1;
			}
		}

		/// <summary>
		/// Unregisters all event handlers associated with the specified object.
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <returns>
		/// true, if a least one event handler has been removed;
		/// false, if no event handler was registered.
		/// </returns>
		public static bool UnregisterEventHandlers(object obj)
		{
			lock (sSync)
			{
				return sItemsByObject.Remove(obj);
			}
		}

		/// <summary>
		/// Checks whether the specified event has event handlers attached to it.
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <returns>true, if the specified event has event handlers attached; otherwise false.</returns>
		public static bool IsHandlerRegistered(object obj)
		{
			lock (sSync)
			{
				if (!sItemsByObject.TryGetValue(obj, out _))
					return false;

				return true;
			}
		}

		/// <summary>
		/// Fires the <see cref="INotifyPropertyChanged.PropertyChanged"/> event invoking all event handlers that
		/// are attached to it (event handlers that are associated with a synchronization context are executed in
		/// the thread the synchronization context belongs to).
		/// </summary>
		/// <param name="obj">
		/// Object providing the event (is passed as the 'sender' object to the event handler as well).
		/// </param>
		/// <param name="propertyName">Name of the property that has changed.</param>
		public static void FireEvent(object obj, string propertyName)
		{
			Item[] items;

			lock (sSync)
			{
				if (!sItemsByObject.TryGetValue(obj, out items))
					return;
			}

			var e = new PropertyChangedEventArgs(propertyName);
			foreach (var item in items)
			{
				if (item.SynchronizationContext != null)
				{
					// synchronization context was specified at registration
					// => invoke the handler directly, if the current context is the same as the context at registration and scheduling is not enforced;
					//    otherwise schedule the handler using the context specified at registration
					if (!item.ScheduleAlways && ReferenceEquals(SynchronizationContext.Current, item.SynchronizationContext))
					{
						item.Handler(obj, e);
					}
					else
					{
						item.SynchronizationContext.Post(x => ((Item)x).Handler(obj, e), item);
					}
				}
				else
				{
					// synchronization context was not specified at registration
					// => schedule handler in worker thread or invoke it directly
					if (item.ScheduleAlways) Task.Run(() => item.Handler(obj, e));
					else item.Handler(obj, e);
				}
			}
		}

		/// <summary>
		/// Gets a multicast delegate that calls all event handlers that are attached to the
		/// <see cref="INotifyPropertyChanged.PropertyChanged"/> event (event handlers that are associated with a
		/// synchronization context are executed in the thread the synchronization context belongs to).
		/// </summary>
		/// <param name="obj">Object providing the event.</param>
		/// <returns>Event callers.</returns>
		public static PropertyChangedEventHandler GetEventCallers(object obj)
		{
			Item[] items;

			lock (sSync)
			{
				if (!sItemsByObject.TryGetValue(obj, out items))
					return null;
			}

			PropertyChangedEventHandler handlers = null;

			foreach (var item in items)
			{
				if (item.SynchronizationContext != null)
				{
					handlers += (sender, e) =>
					{
						// synchronization context was specified at registration
						// => invoke the handler directly, if the current context is the same as the context at registration and scheduling is not enforced;
						//    otherwise schedule the handler using the context specified at registration
						if (!item.ScheduleAlways && ReferenceEquals(SynchronizationContext.Current, item.SynchronizationContext))
						{
							item.Handler(sender, e);
						}
						else
						{
							item.SynchronizationContext.Post(x => ((Item)x).Handler(sender, e), item);
						}
					};
				}
				else
				{
					var itemCopy = item;
					handlers += (sender, e) =>
					{
						// synchronization context was not specified at registration
						// => schedule handler in worker thread or invoke it directly
						if (itemCopy.ScheduleAlways) Task.Run(() => itemCopy.Handler(sender, e));
						else itemCopy.Handler(sender, e);
					};
				}
			}

			return handlers;
		}
	}

}
