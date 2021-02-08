///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GriffinPlus.Lib.Events
{

	/// <summary>
	/// Event manager that administrates event handlers in a central place.
	/// Specifically made for <see cref="INotifyPropertyChanged.PropertyChanged"/> events.
	/// Objects firing events do not need to implement own event add/remove logic, especially when it comes to firing
	/// events asynchronously.
	/// </summary>
	public static class PropertyChangedEventManager
	{
		#region Internal Data Types

		/// <summary>
		/// An event handler item in the event manager.
		/// </summary>
		private struct Item
		{
			public readonly SynchronizationContext      SynchronizationContext;
			public readonly PropertyChangedEventHandler Handler;

			public Item(SynchronizationContext context, PropertyChangedEventHandler handler)
			{
				SynchronizationContext = context;
				Handler = handler;
			}
		}

		#endregion

		#region Class Variables

		private static readonly ConditionalWeakTable<object, Item[]> mItemsByObject = new ConditionalWeakTable<object, Item[]>();
		private static readonly object                               sSync          = new object();

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
		/// <returns>Total number of registered event handlers (including the specified event handler).</returns>
		public static int RegisterEventHandler(
			object                      obj,
			PropertyChangedEventHandler handler,
			SynchronizationContext      context)
		{
			return RegisterEventHandler(obj, handler, context, false, null, null);
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
			bool                        fireImmediately,
			object                      sender,
			string                      propertyName)
		{
			Item[] newItems;

			lock (sSync)
			{
				Item[] items;
				if (mItemsByObject.TryGetValue(obj, out items))
				{
					newItems = new Item[items.Length + 1];
					Array.Copy(items, newItems, items.Length);
					newItems[items.Length] = new Item(context, handler);
					mItemsByObject.Remove(obj);
					mItemsByObject.Add(obj, newItems);
				}
				else
				{
					newItems = new[] { new Item(context, handler) };
					mItemsByObject.Remove(obj);
					mItemsByObject.Add(obj, newItems);
				}
			}

			if (fireImmediately)
			{
				if (context != null)
				{
					context.Post(x => { handler(sender, new PropertyChangedEventArgs(propertyName)); }, null);
				}
				else
				{
					handler(sender, new PropertyChangedEventArgs(propertyName));
				}
			}

			return newItems.Length;
		}

		/// <summary>
		/// Unregisters an event handler from the specified event.
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
				Item[] items;
				if (!mItemsByObject.TryGetValue(obj, out items))
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
							mItemsByObject.Remove(obj);
							mItemsByObject.Add(obj, newItems);
							return newItems.Length;
						}

						mItemsByObject.Remove(obj);
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
		/// <returns>true, if the specified event has event handlers attached; otherwise false.</returns>
		public static bool IsHandlerRegistered(object obj)
		{
			lock (sSync)
			{
				Item[] items;
				if (!mItemsByObject.TryGetValue(obj, out items))
				{
					return false;
				}

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
				if (!mItemsByObject.TryGetValue(obj, out items)) return;
			}

			var e = new PropertyChangedEventArgs(propertyName);
			foreach (var item in items)
			{
				if (item.SynchronizationContext != null)
				{
					item.SynchronizationContext.Post(x => { ((Item)x).Handler(obj, e); }, item);
				}
				else
				{
					item.Handler(obj, e);
				}
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
		/// <param name="objectToKeepAlive">
		/// Some object to keep alive until all handlers have run (useful when working with weak references).
		/// </param>
		public static void FireEvent<T>(object obj, string propertyName, T objectToKeepAlive) where T : class
		{
			Item[] items;

			lock (sSync)
			{
				if (!mItemsByObject.TryGetValue(obj, out items)) return;
			}

			var e = new PropertyChangedEventArgs(propertyName);
			foreach (var item in items)
			{
				if (item.SynchronizationContext != null)
				{
					item.SynchronizationContext.Post(
						x =>
						{
							((Item)x).Handler(obj, e);
							GC.KeepAlive(objectToKeepAlive);
						},
						item);
				}
				else
				{
					item.Handler(obj, e);
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
				if (!mItemsByObject.TryGetValue(obj, out items))
				{
					return null;
				}
			}

			PropertyChangedEventHandler handlers = null;

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
