#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Collections;
using Speedy.Commands;
using Speedy.Extensions;
using Speedy.Logging;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a location provider for a location (horizontal and vertical location).
/// </summary>
/// <typeparam name="T"> The location type. </typeparam>
/// <typeparam name="T2"> The setting type. </typeparam>
public abstract class LocationProvider<T, T2>
	: Bindable, ILocationProvider<T, T2>, IHorizontalLocationProvider<T>, IVerticalLocationProvider<T>
	where T : class, ILocation, new()
	where T2 : LocationProviderSettings, IBindable, new()
{
	#region Constructors

	/// <summary>
	/// Instantiates an instance of the location provider.
	/// </summary>
	protected LocationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
		// Set properties
		ProviderSources = new BaseObservableCollection<LocationProviderSource>();
		LastReadLocation = new T();
		LastReadLocation.UpdateDispatcher(dispatcher);
		LocationProviderSettings = new T2();
		LocationProviderSettings.UpdateDispatcher(dispatcher);

		// Commands
		StartListeningCommand = new RelayCommand(_ => StartListeningAsync(), x => !IsListening);
		StopListeningCommand = new RelayCommand(_ => StopListeningAsync(), x => !IsListening);
	}

	#endregion

	#region Properties

	/// <summary>
	/// Returns true if there is a set of internal providers.
	/// </summary>
	public bool HasProviderSources => ProviderSources.Any();

	/// <inheritdoc />
	public bool HasPermission { get; protected set; }

	/// <summary>
	/// A list of providers internal to the single location providers.
	/// </summary>
	public virtual IEnumerable<LocationProviderSource> ProviderSources { get; }

	/// <inheritdoc />
	public virtual bool IsListening { get; protected set; }

	/// <inheritdoc />
	public virtual bool IsLocationAvailable { get; protected set; }

	/// <inheritdoc />
	public virtual bool IsLocationEnabled { get; protected set; }

	/// <inheritdoc />
	public T LastReadLocation { get; }

	/// <inheritdoc />
	public T2 LocationProviderSettings { get; }

	/// <inheritdoc />
	public RelayCommand StartListeningCommand { get; }

	/// <inheritdoc />
	public string Status { get; protected set; }

	/// <inheritdoc />
	public RelayCommand StopListeningCommand { get; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public virtual T GetCurrentLocation(TimeSpan? timeout = null, CancellationToken? cancelToken = null)
	{
		return GetCurrentLocationAsync().AwaitResults();
	}

	/// <inheritdoc />
	public abstract Task<T> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null);

	/// <inheritdoc />
	public T GetHorizontalLocation()
	{
		return LastReadLocation;
	}

	/// <inheritdoc />
	public T GetVerticalLocation()
	{
		return LastReadLocation;
	}

	/// <inheritdoc />
	public abstract Task StartListeningAsync();

	/// <inheritdoc />
	public abstract Task StopListeningAsync();

	/// <summary>
	/// Triggers event handler
	/// </summary>
	/// <param name="e"> The value for the event handler. </param>
	protected virtual void OnLogEventWritten(LogEventArgs e)
	{
		LogEventWritten?.Invoke(this, e);
	}

	/// <summary>
	/// Triggers event handler.
	/// </summary>
	/// <param name="e"> The value for the event handler. </param>
	protected virtual void OnPositionChanged(T e)
	{
		PositionChanged?.Invoke(this, e);
	}

	/// <summary>
	/// Triggers event handler.
	/// </summary>
	/// <param name="e"> The value for the event handler. </param>
	protected virtual void OnPositionError(LocationProviderError e)
	{
		PositionError?.Invoke(this, e);
	}

	/// <summary>
	/// Update the last read location.
	/// </summary>
	/// <param name="update"> The newly read location. </param>
	/// <remarks>
	/// Ensure you are adhering to the <seealso cref="LocationProviderSettings" /> options.
	/// </remarks>
	protected virtual void UpdateLastReadLocation(T update)
	{
		if (update == null)
		{
			return;
		}

		LastReadLocation.UpdateWith(update);
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event EventHandler<LogEventArgs> LogEventWritten;

	/// <inheritdoc />
	public event EventHandler<T> PositionChanged;

	/// <inheritdoc />
	public event EventHandler<LocationProviderError> PositionError;

	#endregion
}

/// <summary>
/// Represents a location provider for a location (horizontal and vertical location).
/// </summary>
/// <typeparam name="T"> The location type. </typeparam>
/// <typeparam name="T2"> The setting type. </typeparam>
public interface ILocationProvider<T, out T2>
	where T : class, ILocation, new()
	where T2 : LocationProviderSettings
{
	#region Properties

	/// <summary>
	/// Gets if the app has permission to access location information.
	/// </summary>
	bool HasPermission { get; }

	/// <summary>
	/// Gets if the provider is listening for location changes.
	/// </summary>
	bool IsListening { get; }

	/// <summary>
	/// Gets if location is available from the provider.
	/// </summary>
	bool IsLocationAvailable { get; }

	/// <summary>
	/// Gets if location is enabled on the provider.
	/// </summary>
	bool IsLocationEnabled { get; }

	/// <summary>
	/// The last read location.
	/// </summary>
	T LastReadLocation { get; }

	/// <summary>
	/// The settings for when the location provider.
	/// </summary>
	T2 LocationProviderSettings { get; }

	/// <summary>
	/// The command for starting to listen for location changes.
	/// </summary>
	RelayCommand StartListeningCommand { get; }

	/// <summary>
	/// The status of the provider
	/// </summary>
	string Status { get; }

	/// <summary>
	/// The command to stop to listening for location changes.
	/// </summary>
	RelayCommand StopListeningCommand { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Gets the current location from the provider.
	/// </summary>
	/// <param name="timeout"> Timeout to wait. If null we use the default time from <see cref="LocationProviderSettings" />. </param>
	/// <param name="cancelToken"> An optional cancellation token. </param>
	/// <returns> The current location or null if not available. </returns>
	T GetCurrentLocation(TimeSpan? timeout = null, CancellationToken? cancelToken = null);

	/// <summary>
	/// Gets the current location from the provider.
	/// </summary>
	/// <param name="timeout"> Timeout to wait. If null we use the default time from <see cref="LocationProviderSettings" />. </param>
	/// <param name="cancelToken"> An optional cancellation token. </param>
	/// <returns> The current location or null if not available. </returns>
	Task<T> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null);

	/// <summary>
	/// Start listening for location changes.
	/// </summary>
	Task StartListeningAsync();

	/// <summary>
	/// Stop listening for location changes.
	/// </summary>
	Task StopListeningAsync();

	#endregion

	#region Events

	/// <summary>
	/// Provider has written a log event.
	/// </summary>
	event EventHandler<LogEventArgs> LogEventWritten;

	/// <summary>
	/// Provider location changed event handler.
	/// </summary>
	event EventHandler<T> PositionChanged;

	/// <summary>
	/// ProviderLocation error event handler
	/// </summary>
	event EventHandler<LocationProviderError> PositionError;

	#endregion
}