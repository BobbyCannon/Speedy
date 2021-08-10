#region References

using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Speedy.Extensions;
using Speedy.Storage;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a bindable object.
	/// </summary>
	public abstract class Bindable : INotifyPropertyChanged, IBindable, IUpdatable
	{
		#region Fields

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
		public virtual void OnPropertyChanged(string propertyName)
		{
			// Ensure we have not paused property notifications
			if (_pausePropertyChanged)
			{
				// Property change notifications have been paused so bounce
				return;
			}

			// todo: move this to another virtual method that would then be called by this on property changed
			// I had to move this dispatcher code up. Also would be nice to only dispatch specific properties, right?

			if (Dispatcher?.HasThreadAccess == false)
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
		public virtual void UpdateDispatcher(IDispatcher dispatcher)
		{
			Dispatcher = dispatcher;
		}

		/// <inheritdoc />
		public virtual void UpdateWith(object update, params string[] exclusions)
		{
			this.UpdateWithUsingReflection(update, exclusions);
		}

		#endregion

		#region Events

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}