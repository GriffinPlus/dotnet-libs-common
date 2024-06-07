///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-common)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using GriffinPlus.Lib.Events;

namespace GriffinPlus.Lib.Configuration;

/// <summary>
/// An item in the <see cref="CascadedConfiguration"/>.
/// </summary>
[DebuggerDisplay("{" + nameof(DebugOutput) + "}")]
public sealed class CascadedConfigurationItem<T> : ICascadedConfigurationItem
{
	private readonly string mName;
	private readonly string mPath;
	private          T      mValue;
	private          string mComment;
	private          bool   mHasValue;

	/// <summary>
	/// Occurs when the value of a property changes (directly or indirectly).<br/>
	/// When the event is raised, the handler is scheduled using the synchronization context of the registering thread.<br/>
	/// If the thread does not have a synchronization context, the handler is scheduled on a worker thread.
	/// </summary>
	public event PropertyChangedEventHandler PropertyChanged
	{
		add => PropertyChangedEventManager.RegisterEventHandler(this, value, SynchronizationContext.Current, true);
		remove => PropertyChangedEventManager.UnregisterEventHandler(this, value);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CascadedConfigurationItem{T}"/> class.
	/// </summary>
	/// <param name="configuration">Configuration the item belongs to.</param>
	/// <param name="name">Name of the configuration item.</param>
	/// <param name="path">Path of the configuration item in the configuration hierarchy.</param>
	internal CascadedConfigurationItem(CascadedConfigurationBase configuration, string name, string path)
	{
		Configuration = configuration;
		InheritedItem = configuration.InheritedConfiguration?.GetItem<T>(CascadedConfigurationPathHelper.EscapeName(name));
		mName = name;
		mValue = default;
		mHasValue = false;
		mPath = path;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CascadedConfigurationItem{T}"/> class.
	/// </summary>
	/// <param name="configuration">Configuration the item belongs to.</param>
	/// <param name="name">Name of the configuration item.</param>
	/// <param name="path">Path of the configuration item in the configuration hierarchy.</param>
	/// <param name="value">Value of the configuration item.</param>
	internal CascadedConfigurationItem(
		CascadedConfigurationBase configuration,
		string                    name,
		string                    path,
		T                         value)
	{
		Configuration = configuration;
		InheritedItem = configuration.InheritedConfiguration?.GetItem<T>(CascadedConfigurationPathHelper.EscapeName(name));
		mName = name;
		mValue = value;
		mHasValue = true;
		mPath = path;
	}

	/// <inheritdoc/>
	public CascadedConfigurationBase Configuration { get; internal set; }

	/// <inheritdoc cref="ICascadedConfigurationItem.InheritedItem"/>
	public CascadedConfigurationItem<T> InheritedItem { get; internal set; }

	/// <inheritdoc/>
	public string Name
	{
		get
		{
			lock (Configuration.Sync)
			{
				return mName;
			}
		}
	}

	/// <inheritdoc/>
	public string Path
	{
		get
		{
			lock (Configuration.Sync)
			{
				return mPath;
			}
		}
	}

	/// <inheritdoc/>
	public Type Type => typeof(T); // immutable part => no synchronization necessary

	/// <inheritdoc/>
	public bool HasValue
	{
		get
		{
			lock (Configuration.Sync)
			{
				return mHasValue;
			}
		}
	}

	/// <inheritdoc cref="ICascadedConfigurationItem.Value"/>
	public T Value
	{
		get
		{
			lock (Configuration.Sync)
			{
				if (mHasValue) return mValue;
				bool found = Configuration.TryGetValue(mName, true, out T value);
				Debug.Assert(found);
				return value;
			}
		}

		set
		{
			lock (Configuration.Sync)
			{
				if (Configuration.PersistenceStrategy != null)
				{
					if (!Configuration.PersistenceStrategy.IsAssignable(Type, value))
					{
						throw new ConfigurationException(
							"The specified value is not supported for a configuration item of type '{0}'.",
							typeof(T).FullName);
					}
				}

				if (mHasValue && Equals(mValue, value))
					return;

				mValue = value;
				mHasValue = true;
				OnPropertyChanged();
				Configuration.NotifyItemValueChanged(this);
			}
		}
	}

	/// <inheritdoc/>
	public bool HasComment
	{
		get
		{
			lock (Configuration.Sync)
			{
				return mComment != null;
			}
		}
	}

	/// <inheritdoc/>
	public bool SupportsComments => Configuration.PersistenceStrategy == null || Configuration.PersistenceStrategy.SupportsComments;

	/// <inheritdoc/>
	public string Comment
	{
		get
		{
			lock (Configuration.Sync)
			{
				if (mComment != null) return mComment;
				Configuration.TryGetComment(mName, true, out string comment);
				return comment;
			}
		}

		set
		{
			lock (Configuration.Sync)
			{
				if (Configuration.PersistenceStrategy is { SupportsComments: false })
					throw new NotSupportedException("The persistence strategy does not support comments.");

				if (Equals(mComment, value))
					return;

				mComment = value;
				OnPropertyChanged();
				Configuration.NotifyItemCommentChanged(this);
			}
		}
	}

	/// <summary>
	/// Gets the string to show in the debugger when inspecting the object.
	/// </summary>
	private string DebugOutput
	{
		get
		{
			if (mHasValue)
			{
				return $"Item | Path: {mPath} | Value: {mValue}";
			}

			try
			{
				T value = mValue;
				return $"Item | Path: {mPath} | Value: <no value> (inherited: {value})";
			}
			catch (ConfigurationException)
			{
				return $"Item | Path: {mPath} | Value: <no value> (no inherited value)";
			}
		}
	}

	/// <summary>
	/// Resets the value of the configuration item, so an inherited configuration value is returned by the <see cref="Value"/> property.
	/// </summary>
	public void ResetValue()
	{
		lock (Configuration.Sync)
		{
			if (!mHasValue) return;
			mHasValue = false;
			mValue = default;
			OnPropertyChanged(nameof(Value));
			Configuration.NotifyItemValueChanged(this);
		}
	}

	/// <inheritdoc/>
	ICascadedConfigurationItem ICascadedConfigurationItem.InheritedItem => InheritedItem;

	/// <inheritdoc/>
	object ICascadedConfigurationItem.Value
	{
		get => Value;
		set => Value = (T)value;
	}

	/// <summary>
	/// Raises the <see cref="PropertyChanged"/> event.
	/// </summary>
	/// <param name="propertyName">The name of the property that has changed.</param>
	internal void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChangedEventManager.FireEvent(this, propertyName);
	}
}
