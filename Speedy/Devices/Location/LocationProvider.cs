#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Collections;
using Speedy.Commands;
using Speedy.Extensions;
using Speedy.Logging;
using Speedy.Serialization;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a location provider for a location (horizontal and vertical location).
/// </summary>
/// <typeparam name="T"> The location type. </typeparam>
/// <typeparam name="T2"> The setting type. </typeparam>
public abstract class LocationProvider<T, T2>
	: Bindable, ILocationProvider<T, T2>, IHorizontalLocationProvider<T>, IVerticalLocationProvider<T>
	where T : class, ILocation, ICloneable<T>, new()
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

	/// <inheritdoc />
	public bool HasPermission { get; protected set; }

	/// <summary>
	/// Returns true if there is a set of internal providers.
	/// </summary>
	public bool HasProviderSources => ProviderSources.Any();

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

	/// <summary>
	/// A list of providers internal to the single location providers.
	/// </summary>
	public virtual IEnumerable<LocationProviderSource> ProviderSources { get; }

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
	/// Triggers event handler.
	/// </summary>
	/// <param name="e"> The value for the event handler. </param>
	protected virtual void OnLocationChanged(T e)
	{
		LocationChanged?.Invoke(this, e);
	}

	/// <summary>
	/// Trigger the horizontal location changed event.
	/// </summary>
	/// <param name="e"> The updated location. </param>
	protected virtual void OnLocationChangedHorizontalOnly(T e)
	{
		LocationChangedHorizontalOnly?.Invoke(this, e);
	}

	/// <summary>
	/// Trigger the vertical location changed event.
	/// </summary>
	/// <param name="e"> The updated location. </param>
	protected virtual void OnLocationChangedVerticalOnly(T e)
	{
		LocationChangedVerticalOnly?.Invoke(this, e);
	}

	/// <summary>
	/// Triggers event handler.
	/// </summary>
	/// <param name="e"> The value for the event handler. </param>
	protected virtual void OnLocationProviderError(LocationProviderError e)
	{
		ErrorReceived?.Invoke(this, e);
	}

	/// <summary>
	/// Triggers event handler
	/// </summary>
	/// <param name="e"> The value for the event handler. </param>
	protected virtual void OnLogEventWritten(LogEventArgs e)
	{
		LogEventWritten?.Invoke(this, e);
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event EventHandler<LocationProviderError> ErrorReceived;

	/// <inheritdoc />
	public event EventHandler<T> LocationChanged;

	/// <inheritdoc />
	public event EventHandler<T> LocationChangedHorizontalOnly;

	/// <inheritdoc />
	public event EventHandler<T> LocationChangedVerticalOnly;

	/// <inheritdoc />
	public event EventHandler<LogEventArgs> LogEventWritten;

	#endregion
}

/// <summary>
/// Represents a location provider for a location (horizontal and vertical location).
/// </summary>
/// <typeparam name="T"> The location type. </typeparam>
/// <typeparam name="T2"> The setting type. </typeparam>
public interface ILocationProvider<T, out T2> : ILocationProvider<T>
	where T : class, ILocation, ICloneable<T>
	where T2 : LocationProviderSettings
{
	#region Properties

	/// <summary>
	/// Gets if the app has permission to access location information.
	/// </summary>
	bool HasPermission { get; }

	/// <summary>
	/// Gets if location is available from the provider.
	/// </summary>
	bool IsLocationAvailable { get; }

	/// <summary>
	/// Gets if location is enabled on the provider.
	/// </summary>
	bool IsLocationEnabled { get; }

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

	#endregion

	#region Events

	/// <summary>
	/// ProviderLocation error event handler
	/// </summary>
	event EventHandler<LocationProviderError> ErrorReceived;

	/// <summary>
	/// Provider location changed event handler.
	/// </summary>
	event EventHandler<T> LocationChanged;

	/// <summary>
	/// Provider has written a log event.
	/// </summary>
	event EventHandler<LogEventArgs> LogEventWritten;

	#endregion
}

/// <summary>
/// Represents a location provider for a location for a specific type.
/// </summary>
public interface ILocationProvider<T> : ILocationProvider
	where T : class, ICloneable<T>
{
	#region Properties

	/// <summary>
	/// The last read location.
	/// </summary>
	public T LastReadLocation { get; }

	#endregion

	#region Events

	/// <summary>
	/// The location changed.
	/// </summary>
	event EventHandler<T> LocationChanged;

	#endregion
}

/// <summary>
/// Represents a location provider for a location.
/// </summary>
public interface ILocationProvider : IBindable
{
	#region Properties

	/// <summary>
	/// Gets if the provider is listening for altitude changes.
	/// </summary>
	bool IsListening { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Start listening for location changes.
	/// </summary>
	Task StartListeningAsync();

	/// <summary>
	/// Stop listening for location changes.
	/// </summary>
	Task StopListeningAsync();

	#endregion
}