#region References

using System;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Commands;

#endregion

namespace Speedy.Devices.Location
{
	/// <summary>
	/// Location Provider
	/// </summary>
	public abstract class LocationProvider : Bindable
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the location provider.
		/// </summary>
		protected LocationProvider(IDispatcher dispatcher) : base(dispatcher)
		{
			// Set properties
			LastReadPosition = new ProviderLocation(dispatcher);
			ListenerSettings = new ListenerSettings(dispatcher);

			// Commands
			StartListeningCommand = new RelayCommand(_ => StartListeningAsync(), x => !IsListening);
			StopListeningCommand = new RelayCommand(_ => StopListeningAsync(), x => !IsListening);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Desired accuracy in meters
		/// </summary>
		public virtual double DesiredAccuracy { get; set; }

		/// <summary>
		/// Gets if geolocation is available on device
		/// </summary>
		public virtual bool IsGeolocationAvailable { get; protected set; }

		/// <summary>
		/// Gets if geolocation is enabled on device
		/// </summary>
		public virtual bool IsGeolocationEnabled { get; protected set; }

		/// <summary>
		/// Gets if you are listening for location changes
		/// </summary>
		public virtual bool IsListening { get; protected set; }

		/// <summary>
		/// The last read position.
		/// </summary>
		public IProviderLocation LastReadPosition { get; }

		/// <summary>
		/// The settings for when the provider is listening.
		/// </summary>
		public ListenerSettings ListenerSettings { get; }

		/// <summary>
		/// The command for starting to listen.
		/// </summary>
		public RelayCommand StartListeningCommand { get; }

		/// <summary>
		/// The command to stop to listening.
		/// </summary>
		public RelayCommand StopListeningCommand { get; }

		/// <summary>
		/// Gets if device supports heading
		/// </summary>
		public virtual bool SupportsHeading { get; protected set; }

		#endregion

		#region Methods

		/// <summary>
		/// Gets the last known and most accurate location.
		/// This is usually cached and best to display first before querying for full position.
		/// </summary>
		/// <returns> Best and most recent location or null if none found </returns>
		public abstract Task<IProviderLocation> GetLastKnownLocationAsync();

		/// <summary>
		/// Gets position async with specified parameters
		/// </summary>
		/// <param name="timeout"> Timeout to wait, Default Infinite </param>
		/// <param name="token"> Cancellation token </param>
		/// <param name="includeHeading"> If you would like to include heading </param>
		/// <returns> ProviderLocation </returns>
		public abstract Task<IProviderLocation> GetPositionAsync(TimeSpan? timeout = null, CancellationToken? token = null, bool includeHeading = false);

		/// <summary>
		/// Start listening for changes
		/// </summary>
		public abstract Task<bool> StartListeningAsync();

		/// <summary>
		/// Stop listening
		/// </summary>
		/// <returns> If successfully stopped </returns>
		public abstract Task<bool> StopListeningAsync();

		/// <summary>
		/// Triggers event handler.
		/// </summary>
		/// <param name="e"> The value for the event handler. </param>
		protected virtual void OnPositionChanged(PositionEventArgs e)
		{
			PositionChanged?.Invoke(this, e);
		}

		/// <summary>
		/// Triggers event handler.
		/// </summary>
		/// <param name="e"> The value for the event handler. </param>
		protected virtual void OnPositionError(PositionErrorEventArgs e)
		{
			PositionError?.Invoke(this, e);
		}

		#endregion

		#region Events

		/// <summary>
		/// Provider location changed event handler.
		/// </summary>
		public event EventHandler<PositionEventArgs> PositionChanged;

		/// <summary>
		/// ProviderLocation error event handler
		/// </summary>
		public event EventHandler<PositionErrorEventArgs> PositionError;

		#endregion
	}
}