#region References

using System.ComponentModel;
using Newtonsoft.Json;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync model, usually used in a web API model.
	/// </summary>
	/// <typeparam name="T"> The type for the key. </typeparam>
	public abstract class SyncModel<T> : SyncEntity<T>, IBindable
	{
		#region Fields

		private bool _pausePropertyChanged;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiate a sync model.
		/// </summary>
		/// <param name="dispatcher"> The optional dispatcher to use. </param>
		protected SyncModel(IDispatcher dispatcher = null)
		{
			Dispatcher = dispatcher;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Represents a thread dispatcher to help with cross threaded request.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		protected IDispatcher Dispatcher { get; private set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public IDispatcher GetDispatcher()
		{
			return Dispatcher;
		}

		/// <inheritdoc />
		public bool IsChangeNotificationsPaused()
		{
			return _pausePropertyChanged;
		}

		/// <summary>
		/// Indicates the property has changed on the bindable object.
		/// </summary>
		/// <param name="propertyName"> The name of the property has changed. </param>
		public override void OnPropertyChanged(string propertyName)
		{
			// Ensure we have not paused property notifications
			if (_pausePropertyChanged)
			{
				// Property change notifications have been paused so bounce
				return;
			}

			if (Dispatcher?.IsDispatcherThread == false)
			{
				Dispatcher.Run(() => OnPropertyChanged(propertyName));
				return;
			}

			base.OnPropertyChanged(propertyName);
		}

		/// <inheritdoc />
		public void PausePropertyChangeNotifications(bool pause = true)
		{
			_pausePropertyChanged = pause;
		}

		/// <inheritdoc />
		public virtual void UpdateDispatcher(IDispatcher dispatcher)
		{
			Dispatcher = dispatcher;
		}

		#endregion
	}
}