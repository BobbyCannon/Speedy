#region References

using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.Storage;
using ICloneable = Speedy.Serialization.ICloneable;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a bindable object.
	/// </summary>
	public abstract class Bindable<T> : Bindable, IUpdatable<T>, ICloneable<T> where T : new()
	{
		#region Fields

		// ReSharper disable once StaticMemberInGenericType
		private static readonly SerializerSettings _defaultDeepCloneSettings;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a bindable object.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		protected Bindable(IDispatcher dispatcher = null) : base(dispatcher)
		{
		}

		static Bindable()
		{
			_defaultDeepCloneSettings = new SerializerSettings { IgnoreVirtuals = true, IgnoreReadOnly = true };
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public new virtual T DeepClone(int levels = -1)
		{
			// Defaults to serializer deep clone. Type would want to override this to make it more efficient.
			return this.ToJson(_defaultDeepCloneSettings).FromJson<T>();
		}

		/// <inheritdoc />
		public new virtual T ShallowClone()
		{
			var response = new T();
			response.UpdateWith(this);
			return response;
		}

		/// <inheritdoc />
		public new virtual T ShallowCloneExcept(params string[] exclusions)
		{
			var response = new T();
			response.UpdateWith(this, exclusions);
			return response;
		}

		/// <inheritdoc />
		public new virtual void UpdateWith(object update, params string[] exclusions)
		{
			UpdateWith((T) update, exclusions);
		}

		/// <inheritdoc />
		public virtual void UpdateWith(T update, params string[] exclusions)
		{
			UpdatableExtensions.UpdateWith(this, update, exclusions);
		}

		#endregion
	}

	/// <summary>
	/// Represents a bindable object.
	/// </summary>
	public abstract class Bindable : IUpdatable, IUnwrappable, INotifyPropertyChanged, IBindable, ICloneable
	{
		#region Fields

		private static readonly SerializerSettings _defaultDeepCloneSettings;
		private bool _pausePropertyChanged;
		private Type _realType;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a bindable object.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		protected Bindable(IDispatcher dispatcher = null)
		{
			Dispatcher = dispatcher;
		}

		static Bindable()
		{
			_defaultDeepCloneSettings = new SerializerSettings { IgnoreVirtuals = true, IgnoreReadOnly = true };
		}

		#endregion

		#region Properties

		/// <summary>
		/// Determines if the object has changes.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public virtual bool HasChanges { get; set; }

		/// <summary>
		/// Represents a thread dispatcher to help with cross threaded request.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		protected IDispatcher Dispatcher { get; private set; }

		/// <summary>
		/// Cached version of the "real" type, meaning not EF proxy but rather root type
		/// </summary>
		internal Type RealType => _realType ??= this.GetRealType();

		#endregion

		#region Methods

		/// <inheritdoc />
		public object DeepClone(int levels = -1)
		{
			return this.ToJson(_defaultDeepCloneSettings).FromJson(RealType);
		}

		/// <inheritdoc />
		public IDispatcher GetDispatcher()
		{
			return Dispatcher;
		}

		/// <summary>
		/// Return true if the change notifications are paused or otherwise false.
		/// </summary>
		public virtual bool IsChangeNotificationsPaused()
		{
			return _pausePropertyChanged;
		}

		/// <summary>
		/// Indicates the property has changed on the bindable object.
		/// </summary>
		/// <param name="propertyName"> The name of the property has changed. </param>
		public virtual void OnPropertyChanged(string propertyName = null)
		{
			// Ensure we have not paused property notifications
			if (_pausePropertyChanged)
			{
				// Property change notifications have been paused so bounce
				return;
			}

			// todo: move this to another virtual method that would then be called by this on property changed
			// I had to move this dispatcher code up. Also would be nice to only dispatch specific properties, right?

			if (Dispatcher != null && !Dispatcher.HasThreadAccess)
			{
				Dispatcher.Run(() => OnPropertyChanged(propertyName));
				return;
			}

			if (propertyName != nameof(HasChanges))
			{
				HasChanges = true;
			}

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Pause / Un-pause the property change notifications
		/// </summary>
		public virtual void PausePropertyChangeNotifications(bool pause = true)
		{
			_pausePropertyChanged = pause;
		}

		/// <inheritdoc />
		public object ShallowClone()
		{
			var response = (IUpdatable) Activator.CreateInstance(GetType());
			response.UpdateWith(this);
			return response;
		}

		/// <inheritdoc />
		public object ShallowCloneExcept(params string[] exclusions)
		{
			var response = (IUpdatable) Activator.CreateInstance(GetType());
			response.UpdateWith(this, exclusions);
			return response;
		}

		/// <inheritdoc />
		public object Unwrap()
		{
			return ShallowClone();
		}

		/// <inheritdoc />
		public virtual void UpdateDispatcher(IDispatcher dispatcher)
		{
			Dispatcher = dispatcher;
		}

		/// <inheritdoc />
		public virtual void UpdateWith(object update, params string[] exclusions)
		{
			UpdatableExtensions.UpdateWith(this, update, exclusions);
		}

		#endregion

		#region Events

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}