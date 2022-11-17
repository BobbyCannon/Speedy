#region References

using System;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Logging;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a location provider.
/// </summary>
public abstract class LocationProvider<T, THorizontalLocation, TVerticalLocation, T2>
	: DeviceInformationProvider<T>, ILocationProvider<T, THorizontalLocation, TVerticalLocation, T2>
	where T : class, ILocation<THorizontalLocation, TVerticalLocation>, new()
	where THorizontalLocation : class, IHorizontalLocation, IUpdatable<THorizontalLocation>
	where TVerticalLocation : class, IVerticalLocation, IUpdatable<TVerticalLocation>
	where T2 : ILocationProviderSettings, new()
{
	#region Constructors

	/// <summary>
	/// Creates an instance of a location provider.
	/// </summary>
	/// <param name="dispatcher"> An optional dispatcher. </param>
	protected LocationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
		ComparerForHorizontal = new LocationComparer<THorizontalLocation>(dispatcher);
		ComparerForVertical = new LocationComparer<TVerticalLocation>(dispatcher);

		LocationProviderSettings = new T2();
	}

	#endregion

	#region Properties

	/// <summary>
	/// The settings for the location provider.
	/// </summary>
	public T2 LocationProviderSettings { get; }

	/// <summary>
	/// The status of the provider.
	/// </summary>
	public string Status { get; protected set; }

	/// <summary>
	/// Comparer for the horizontal location.
	/// </summary>
	protected LocationComparer<THorizontalLocation> ComparerForHorizontal { get; }

	/// <summary>
	/// Comparer for the vertical location.
	/// </summary>
	protected LocationComparer<TVerticalLocation> ComparerForVertical { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Gets position async with specified parameters
	/// </summary>
	/// <param name="timeout"> Timeout to wait, Default Infinite </param>
	/// <param name="cancelToken"> Cancellation token </param>
	/// <returns> ProviderLocation </returns>
	public abstract Task<T> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null);

	/// <inheritdoc />
	public override bool ShouldUpdate(T value, T update)
	{
		return ComparerForHorizontal.ShouldUpdate(value, update)
			|| ComparerForVertical.ShouldUpdate(value, update);
	}

	/// <inheritdoc />
	public override bool UpdateWith(ref T value, T update, params string[] exclusions)
	{
		return value.HorizontalLocation.UpdateWith(update, exclusions);
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

	/// <summary>
	/// ProviderLocation error event handler
	/// </summary>
	public event EventHandler<LocationProviderError> ErrorReceived;

	/// <summary>
	/// Provider has written a log event.
	/// </summary>
	public event EventHandler<LogEventArgs> LogEventWritten;

	#endregion
}

/// <summary>
/// Represents a location provider.
/// </summary>
public interface ILocationProvider<T, THorizontalLocation, TVerticalLocation, out T2>
	: IDeviceInformationProvider<T>
	where T : class, ILocation<THorizontalLocation, TVerticalLocation>, new()
	where THorizontalLocation : class, IHorizontalLocation, IUpdatable<THorizontalLocation>
	where TVerticalLocation : class, IVerticalLocation, IUpdatable<TVerticalLocation>
	where T2 : ILocationProviderSettings, new()
{
	#region Properties

	/// <summary>
	/// The settings for the location provider.
	/// </summary>
	public T2 LocationProviderSettings { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Gets position async with specified parameters
	/// </summary>
	/// <param name="timeout"> Timeout to wait, Default Infinite </param>
	/// <param name="cancelToken"> Cancellation token </param>
	/// <returns> ProviderLocation </returns>
	Task<T> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null);

	#endregion
}