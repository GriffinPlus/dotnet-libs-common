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

namespace GriffinPlus.Lib.Configuration
{

	/// <summary>
	/// An item in the <see cref="CascadedConfiguration"/>.
	/// </summary>
	[DebuggerDisplay("{" + nameof(DebugOutput) + "}")]
	public sealed class CascadedConfigurationItem<T> : ICascadedConfigurationItemInternal
	{
		private readonly string mName;
		private readonly string mPath;
		private          T      mValue;
		private          string mComment;
		private          bool   mHasValue;
		private          bool   mHasComment;

		/// <summary>
		/// Occurs when the value of a property changes (directly or indirectly).
		/// When the event is raised, the handler is scheduled using the synchronization context of the registering thread.
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
		/// <param name="name">Name of the configuration item.</param>
		/// <param name="path">Path of the configuration item in the configuration hierarchy.</param>
		internal CascadedConfigurationItem(string name, string path)
		{
			mName = name;
			mValue = default;
			mHasValue = false;
			mHasComment = false;
			mPath = path;
		}

		/// <summary>
		/// Gets the configuration the current item is in.
		/// </summary>
		public CascadedConfiguration Configuration { get; internal set; }

		/// <summary>
		/// Gets the name of the configuration item.
		/// </summary>
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

		/// <summary>
		/// Gets the path of the configuration item in the configuration hierarchy.
		/// </summary>
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

		/// <summary>
		/// Gets the type of the value in the configuration item
		/// (the type of the actual value may be the type or a type deriving from this type).
		/// </summary>
		public Type Type => typeof(T); // immutable part => no synchronization necessary

		/// <summary>
		/// Gets a value indicating whether the configuration item contains a valid value.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the value of the configuration item.
		/// </summary>
		/// <exception cref="ConfigurationException">The configuration item does not have a value.</exception>
		/// <remarks>
		/// This property gets the value of the current configuration item, if the current configuration item provides
		/// a value for it. If it doesn't, inherited configurations in the configuration cascade are queried.
		/// Setting the property effects the current configuration item only.
		/// </remarks>
		public T Value
		{
			get
			{
				lock (Configuration.Sync)
				{
					if (mHasValue)
						return mValue;

					return Configuration.GetValue<T>(mName);
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

					if (!mHasValue || !Equals(mValue, value))
					{
						mValue = value;
						mHasValue = true;
						OnPropertyChanged();
						Configuration.NotifyItemValueChanged(this);
					}
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the configuration item contains a comment.
		/// </summary>
		public bool HasComment
		{
			get
			{
				lock (Configuration.Sync)
				{
					return mHasComment;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the configuration supports comments.
		/// </summary>
		public bool SupportsComments => Configuration.PersistenceStrategy == null || Configuration.PersistenceStrategy.SupportsComments;

		/// <summary>
		/// Gets or sets the comment describing the configuration item.
		/// </summary>
		/// <remarks>
		/// This property gets the comment of the current configuration item, if the current configuration item provides a comment.
		/// If it doesn't inherited configurations in the configuration cascade are queried. Setting the property effects the current
		/// configuration item only.
		/// </remarks>
		public string Comment
		{
			get
			{
				lock (Configuration.Sync)
				{
					if (mHasComment)
						return mComment;

					return Configuration.GetComment(mName);
				}
			}

			set
			{
				lock (Configuration.Sync)
				{
					if (Configuration.PersistenceStrategy != null && !Configuration.PersistenceStrategy.SupportsComments)
						throw new NotSupportedException("The persistence strategy does not support comments.");

					if (!mHasComment || !Equals(mComment, value))
					{
						mComment = value;
						mHasComment = true;
						OnPropertyChanged();
						Configuration.NotifyItemCommentChanged(this);
					}
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
				if (mHasValue)
				{
					mHasValue = false;
					mValue = default;
					OnPropertyChanged(nameof(Value));
					Configuration.NotifyItemValueChanged(this);
				}
			}
		}

		/// <summary>
		/// Resets the comment of the configuration item, so an inherited configuration value is returned by the <see cref="Comment"/> property.
		/// </summary>
		public void ResetComment()
		{
			lock (Configuration.Sync)
			{
				if (mHasComment)
				{
					mHasComment = false;
					mComment = null;
					OnPropertyChanged(nameof(Comment));
					Configuration.NotifyItemCommentChanged(this);
				}
			}
		}

		/// <summary>
		/// Gets or sets the value of the configuration item.
		/// </summary>
		object ICascadedConfigurationItem.Value
		{
			get => Value;
			set => Value = (T)value;
		}

		/// <summary>
		/// Sets the configuration the current item is in.
		/// </summary>
		/// <param name="configuration">Configuration to set.</param>
		void ICascadedConfigurationItemInternal.SetConfiguration(CascadedConfiguration configuration)
		{
			Configuration = configuration;
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

}
