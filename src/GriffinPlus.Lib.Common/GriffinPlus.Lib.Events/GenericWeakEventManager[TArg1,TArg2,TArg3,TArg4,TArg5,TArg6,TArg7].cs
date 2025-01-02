///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable StaticMemberInGenericType

namespace GriffinPlus.Lib.Events;

/// <summary>
/// Event manager that administrates weak event handlers in a central place (for generic actions using
/// <see cref="Action{TArg1,TArg2,TArg3,TArg4,TArg5,TArg6,TArg7}"/>).
/// <br/>
/// Objects firing events do not need to implement own event add/remove logic, especially when it comes to firing events
/// in the context of the thread that registered an event handler.
/// </summary>
/// <typeparam name="TArg1">Type of the first argument passed to event handlers.</typeparam>
/// <typeparam name="TArg2">Type of the second argument passed to event handlers.</typeparam>
/// <typeparam name="TArg3">Type of the third argument passed to event handlers.</typeparam>
/// <typeparam name="TArg4">Type of the fourth argument passed to event handlers.</typeparam>
/// <typeparam name="TArg5">Type of the fifth argument passed to event handlers.</typeparam>
/// <typeparam name="TArg6">Type of the sixth argument passed to event handlers.</typeparam>
/// <typeparam name="TArg7">Type of the seventh argument passed to event handlers.</typeparam>
public static partial class GenericWeakEventManager<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>
{
	private static readonly ConditionalWeakTable<object, Dictionary<string, Item[]>> sItemsByObject = new();
	private static readonly object                                                   sSync          = new();

	/// <summary>
	/// Registers an event handler for an event associated with the specified object.
	/// </summary>
	/// <param name="obj">Object providing the event.</param>
	/// <param name="eventName">Name of the event.</param>
	/// <param name="handler">Event handler to register.</param>
	/// <param name="context">Synchronization context to use when calling the event handler (it may also be <c>null</c>).</param>
	/// <param name="scheduleAlways">
	/// If <paramref name="context"/> is set:<br/>
	/// <c>true</c> to always schedule the event handler in the specified synchronization context,<br/>
	/// <c>false</c> to schedule the event handler in the specified context only, if the thread firing the event has some other synchronization context.<br/>
	/// If <paramref name="context"/> is <c>null</c>:<br/>
	/// <c>true</c> to always schedule the event handler in a worker thread,<br/>
	/// <c>false</c> to invoke the event handler in the thread that is firing the event (direct call).
	/// </param>
	/// <returns>Total number of registered event handlers (including the specified event handler).</returns>
	public static int RegisterEventHandler(
		object                                                  obj,
		string                                                  eventName,
		Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> handler,
		SynchronizationContext                                  context,
		bool                                                    scheduleAlways)
	{
		return RegisterEventHandler(
			obj,
			eventName,
			handler,
			context,
			scheduleAlways,
			false,
			default,
			default,
			default,
			default,
			default,
			default,
			default);
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
	/// If <paramref name="context"/> is set:<br/>
	/// <c>true</c> to always schedule the event handler in the specified synchronization context,<br/>
	/// <c>false</c> to schedule the event handler in the specified context only, if the thread firing the event has some other synchronization context.<br/>
	/// If <paramref name="context"/> is <c>null</c>:<br/>
	/// <c>true</c> to always schedule the event handler in a worker thread,<br/>
	/// <c>false</c> to invoke the event handler in the thread that is firing the event (direct call).
	/// </param>
	/// <param name="fireImmediately">
	/// <c>true</c> to register and fire the event handler immediately after registration;<br/>
	/// <c>false</c> to register the event handler only.
	/// </param>
	/// <param name="arg1">First argument to pass to the event handler that is fired immediately.</param>
	/// <param name="arg2">Second argument to pass to the event handler that is fired immediately.</param>
	/// <param name="arg3">Third argument to pass to the event handler that is fired immediately.</param>
	/// <param name="arg4">Fourth argument to pass to the event handler that is fired immediately.</param>
	/// <param name="arg5">Fifth argument to pass to the event handler that is fired immediately.</param>
	/// <param name="arg6">Sixth argument to pass to the event handler that is fired immediately.</param>
	/// <param name="arg7">Seventh argument to pass to the event handler that is fired immediately.</param>
	/// <returns>Total number of registered event handlers (including the specified event handler).</returns>
	public static int RegisterEventHandler(
		object                                                  obj,
		string                                                  eventName,
		Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> handler,
		SynchronizationContext                                  context,
		bool                                                    scheduleAlways,
		bool                                                    fireImmediately,
		TArg1                                                   arg1,
		TArg2                                                   arg2,
		TArg3                                                   arg3,
		TArg4                                                   arg4,
		TArg5                                                   arg5,
		TArg6                                                   arg6,
		TArg7                                                   arg7)
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

			if (itemsByName!.TryGetValue(eventName, out Item[] items))
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

		if (!fireImmediately)
			return newItems.Length;

		if (context != null)
		{
			if (scheduleAlways) context.Post(_ => handler(arg1, arg2, arg3, arg4, arg5, arg6, arg7), null);
			else handler(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}
		else
		{
			if (scheduleAlways)
			{
				while (!ThreadPool.QueueUserWorkItem(_ => handler(arg1, arg2, arg3, arg4, arg5, arg6, arg7)))
				{
					Thread.Sleep(50);
				}
			}
			else
			{
				handler(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
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
	/// Total number of registered event handlers after the specified handler has been removed;<br/>
	/// -1, if the specified event handler is not registered.
	/// </returns>
	public static int UnregisterEventHandler(
		object                                                  obj,
		string                                                  eventName,
		Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> handler)
	{
		if (handler == null) throw new ArgumentNullException(nameof(handler));

		lock (sSync)
		{
			// abort if specified event handler was not registered
			if (!sItemsByObject.TryGetValue(obj, out Dictionary<string, Item[]> itemsByName)) return -1;
			if (!itemsByName!.TryGetValue(eventName, out Item[] items)) return -1;

			// remove specified handler from the handler list and tidy up handlers of collected objects as well
			List<Item> newItems = null;
			bool removed = false;
			for (int i = 0; i < items.Length; i++)
			{
				Item item = items[i];
				ItemMatchResult matchResult = item.IsHandler(handler);
				switch (matchResult)
				{
					case ItemMatchResult.Match:
						removed = true;
						break;

					case ItemMatchResult.NoMatch:
						newItems ??= new List<Item>(items.Length - i);
						newItems.Add(item);
						break;

					case ItemMatchResult.Collected:
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			// exchange handler list
			if (newItems != null)
			{
				if (newItems.Count == items.Length) return -1;
				itemsByName[eventName] = [.. newItems];
				if (removed) return newItems.Count;
			}
			else
			{
				itemsByName.Remove(eventName);
				if (itemsByName.Count == 0) sItemsByObject.Remove(obj);
				if (removed) return 0;
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
	/// <c>true</c> if at least one event handler has been removed;<br/>
	/// <c>false</c> if no event handler was registered.
	/// </returns>
	public static bool UnregisterEventHandlers(object obj, string eventName)
	{
		lock (sSync)
		{
			// remove all handlers attached to events of the specified object, if requested
			if (eventName == null)
				return sItemsByObject.Remove(obj);

			// abort, if there is no event handler attached to the specified event
			if (!sItemsByObject.TryGetValue(obj, out Dictionary<string, Item[]> itemsByName)) return false;
			if (!itemsByName!.TryGetValue(eventName, out Item[] _)) return false;

			// remove all handlers attached to the specified event
			bool removed = itemsByName.Remove(eventName);
			if (itemsByName.Count == 0) sItemsByObject.Remove(obj);
			return removed;
		}
	}

	/// <summary>
	/// Checks whether the specified event has event handlers attached to it.
	/// </summary>
	/// <param name="obj">Object providing the event.</param>
	/// <param name="eventName">Name of the event.</param>
	/// <returns>
	/// <c>true</c> if the specified event has event handlers attached;<br/>
	/// otherwise <c>false</c>.
	/// </returns>
	public static bool IsHandlerRegistered(object obj, string eventName)
	{
		lock (sSync)
		{
			if (!sItemsByObject.TryGetValue(obj, out Dictionary<string, Item[]> itemsByName)) return false;
			if (!itemsByName!.TryGetValue(eventName, out Item[] items)) return false;

			bool registered = false;
			bool needsCleanup = false;
			for (int i = 0; i < items.Length; i++)
			{
				if (items[i].IsValid) registered = true;
				else needsCleanup = true;
			}

			if (needsCleanup)
				Cleanup(obj, eventName);

			return registered;
		}
	}

	/// <summary>
	/// Checks whether the specified event handler is registered for the specified event.
	/// </summary>
	/// <param name="obj">Object providing the event.</param>
	/// <param name="eventName">Name of the event.</param>
	/// <param name="handler">Event handler to check for.</param>
	/// <returns>
	/// <c>true</c> if the specified event handler is attached to the event;<br/>
	/// otherwise <c>false</c>.
	/// </returns>
	public static bool IsHandlerRegistered(
		object                                                  obj,
		string                                                  eventName,
		Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> handler)
	{
		lock (sSync)
		{
			if (!sItemsByObject.TryGetValue(obj, out Dictionary<string, Item[]> itemsByName)) return false;
			if (!itemsByName!.TryGetValue(eventName, out Item[] items)) return false;

			bool registered = false;
			bool needsCleanup = false;
			for (int i = 0; i < items.Length; i++)
			{
				ItemMatchResult match = items[i].IsHandler(handler);
				switch (match)
				{
					case ItemMatchResult.Match:
						registered = true;
						break;

					case ItemMatchResult.Collected:
						needsCleanup = true;
						break;

					case ItemMatchResult.NoMatch:
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			if (needsCleanup)
				Cleanup(obj, eventName);

			return registered;
		}
	}

	/// <summary>
	/// Fires an event invoking all event handlers that are attached to it (event handlers that are associated with
	/// a synchronization context are executed in the thread the synchronization context belongs to).
	/// </summary>
	/// <param name="obj">Object providing the event.</param>
	/// <param name="eventName">Name of the event.</param>
	/// <param name="arg1">First argument to pass to invoked event handlers.</param>
	/// <param name="arg2">Second argument to pass to invoked event handlers.</param>
	/// <param name="arg3">Third argument to pass to invoked event handlers.</param>
	/// <param name="arg4">Fourth argument to pass to invoked event handlers.</param>
	/// <param name="arg5">Fifth argument to pass to invoked event handlers.</param>
	/// <param name="arg6">Sixth argument to pass to invoked event handlers.</param>
	/// <param name="arg7">Seventh argument to pass to invoked event handlers.</param>
	public static void FireEvent(
		object obj,
		string eventName,
		TArg1  arg1,
		TArg2  arg2,
		TArg3  arg3,
		TArg4  arg4,
		TArg5  arg5,
		TArg6  arg6,
		TArg7  arg7)
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
					item.Fire(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
				}
				else
				{
					item.SynchronizationContext.Post(x => { ((Item)x!).Fire(arg1, arg2, arg3, arg4, arg5, arg6, arg7); }, item);
				}
			}
			else
			{
				// synchronization context was not specified at registration
				// => schedule handler in worker thread or invoke it directly
				if (item.ScheduleAlways)
				{
					while (!ThreadPool.QueueUserWorkItem(_ => item.Fire(arg1, arg2, arg3, arg4, arg5, arg6, arg7)))
					{
						Thread.Sleep(50);
					}
				}
				else
				{
					item.Fire(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
				}
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
	public static Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> GetEventCallers(object obj, string eventName)
	{
		Item[] items;

		lock (sSync)
		{
			items = CleanupAndGetHandlers(obj, eventName);
		}

		// abort, if there are no vivid event handlers registered
		if (items == null)
			return null;

		Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> handlers = null;
		foreach (Item item in items)
		{
			if (item.SynchronizationContext != null)
			{
				// synchronization context was specified at registration
				// => invoke the handler directly, if the current context is the same as the context at registration and scheduling is not enforced;
				//    otherwise schedule the handler using the context specified at registration
				handlers += (
					arg1,
					arg2,
					arg3,
					arg4,
					arg5,
					arg6,
					arg7) =>
				{
					if (!item.ScheduleAlways && ReferenceEquals(SynchronizationContext.Current, item.SynchronizationContext))
					{
						item.Fire(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
					}
					else
					{
						item.SynchronizationContext.Post(x => { ((Item)x!).Fire(arg1, arg2, arg3, arg4, arg5, arg6, arg7); }, item);
					}
				};
			}
			else
			{
				// synchronization context was not specified at registration
				// => schedule handler in worker thread or invoke it directly
				Item itemCopy = item;
				handlers += (
					arg1,
					arg2,
					arg3,
					arg4,
					arg5,
					arg6,
					arg7) =>
				{
					if (itemCopy.ScheduleAlways)
					{
						while (!ThreadPool.QueueUserWorkItem(_ => itemCopy.Fire(arg1, arg2, arg3, arg4, arg5, arg6, arg7)))
						{
							Thread.Sleep(50);
						}
					}
					else
					{
						itemCopy.Fire(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
					}
				};
			}
		}

		return handlers;
	}

	/// <summary>
	/// Removes event handlers of collected objects.
	/// </summary>
	/// <param name="obj">Object the event is associated with.</param>
	/// <param name="eventName">Name of the event.</param>
	private static void Cleanup(object obj, string eventName)
	{
		// get event management information
		if (!sItemsByObject.TryGetValue(obj, out Dictionary<string, Item[]> itemsByName)) return;
		if (!itemsByName!.TryGetValue(eventName, out Item[] items)) return;

		// get handlers of vivid objects
		List<Item> newItems = null;
		for (int i = 0; i < items.Length; i++)
		{
			Item item = items[i];
			if (!item.IsValid) continue;
			newItems ??= new List<Item>(items.Length - i);
			newItems.Add(item);
		}

		// handle that no vivid objects are left
		if (newItems == null)
		{
			itemsByName.Remove(eventName);
			if (itemsByName.Count == 0) sItemsByObject.Remove(obj);
			return;
		}

		// there is at least one vivid object left
		// => update handler list
		itemsByName[eventName] = [.. newItems];
	}

	/// <summary>
	/// Checks whether registered event handlers are still valid, removes invalid handlers and returns the cleaned up handler items.
	/// </summary>
	/// <param name="obj">Object providing the event.</param>
	/// <param name="eventName">Name of the event.</param>
	/// <returns>
	/// The cleaned up handler items;<br/>
	/// <c>null</c> if no handlers are left.
	/// </returns>
	private static Item[] CleanupAndGetHandlers(object obj, string eventName)
	{
		// get event management information
		if (!sItemsByObject.TryGetValue(obj, out Dictionary<string, Item[]> itemsByName)) return null;
		if (!itemsByName!.TryGetValue(eventName, out Item[] items)) return null;

		// check whether the handlers are still valid
		List<int> itemsToRemove = null;
		for (int i = 0; i < items.Length; i++)
		{
			Item item = items[i];
			if (item.IsValid) continue;
			itemsToRemove ??= [];
			itemsToRemove.Add(i);
		}

		// abort, if all handlers are still valid
		if (itemsToRemove == null)
			return items;

		// remove handlers that are not valid anymore
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

		return itemsByName[eventName] = [.. newItems];
	}
}
