#region References

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Speedy.Devices.Location;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Xamarin;

/// <summary>
/// Implementation for LocationProvider
/// </summary>
public class LocationProviderImplementation<TLocation, THorizontal, TVertical, TLocationProviderSettings>
	: LocationProvider<TLocation, THorizontal, TVertical, TLocationProviderSettings>
	where TLocation : class, ILocation<THorizontal, TVertical>, new()
	where THorizontal : class, IHorizontalLocation, IUpdatable<THorizontal>
	where TVertical : class, IVerticalLocation, IUpdatable<TVertical>
	where TLocationProviderSettings : ILocationProviderSettings, IBindable, new()
{
	#region Fields

	private Geolocator _locator;

	#endregion

	#region Constructors

	/// <summary>
	/// Constructor for Implementation
	/// </summary>
	public LocationProviderImplementation(IDispatcher dispatcher) : base(dispatcher)
	{
		_locator = new Geolocator();

		CurrentValue.ProviderName = "Xamarin Windows";
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public bool IsLocationAvailable
	{
		get
		{
			var status = GetGeolocatorStatus();

			while (status == PositionStatus.Initializing)
			{
				Task.Delay(10).Wait();
				status = GetGeolocatorStatus();
			}

			return status != PositionStatus.NotAvailable;
		}
	}

	/// <inheritdoc />
	public bool IsLocationEnabled
	{
		get
		{
			var status = GetGeolocatorStatus();

			while (status == PositionStatus.Initializing)
			{
				Task.Delay(10).Wait();
				status = GetGeolocatorStatus();
			}

			return (status != PositionStatus.Disabled) && (status != PositionStatus.NotAvailable);
		}
	}

	#endregion

	#region Methods

	/// <summary>
	/// Gets position async with specified parameters
	/// </summary>
	/// <param name="timeout"> Timeout to wait, Default Infinite </param>
	/// <param name="cancelToken"> Cancellation token </param>
	/// <returns> ProviderLocation </returns>
	public override Task<TLocation> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null)
	{
		var timeoutMilliseconds = timeout.HasValue ? (int) timeout.Value.TotalMilliseconds : Timeout.Infite;

		if ((timeoutMilliseconds < 0) && (timeoutMilliseconds != Timeout.Infite))
		{
			throw new ArgumentOutOfRangeException(nameof(timeout));
		}

		cancelToken ??= CancellationToken.None;

		var pos = GetGeolocator().GetGeopositionAsync(TimeSpan.FromTicks(0), TimeSpan.FromDays(365));
		cancelToken.Value.Register(o => ((IAsyncOperation<Geoposition>) o).Cancel(), pos);

		var timer = new Timeout(timeoutMilliseconds, pos.Cancel);
		var tcs = new TaskCompletionSource<TLocation>();

		pos.Completed = (op, s) =>
		{
			timer.Cancel();

			switch (s)
			{
				case AsyncStatus.Canceled:
				{
					tcs.SetCanceled();
					break;
				}
				case AsyncStatus.Completed:
				{
					tcs.SetResult(UpdateLastReadPosition(op.GetResults()));
					break;
				}
				case AsyncStatus.Error:
				{
					var ex = op.ErrorCode;
					if (ex is UnauthorizedAccessException)
					{
						ex = new LocationProviderException(LocationProviderError.Unauthorized, ex);
					}

					tcs.SetException(ex);
					break;
				}
			}
		};

		return tcs.Task;
	}

	/// <inheritdoc />
	public override Task StartListeningAsync()
	{
		if (IsListening)
		{
			return Task.CompletedTask;
		}

		try
		{
			// todo: support HasPermission?

			var locator = GetGeolocator();

			locator.ReportInterval = (uint) LocationProviderSettings.MinimumTime.TotalMilliseconds;
			locator.DesiredAccuracy = LocationProviderSettings.DesiredAccuracy < 100 ? PositionAccuracy.High : PositionAccuracy.Default;
			locator.MovementThreshold = LocationProviderSettings.MinimumDistance;
			locator.PositionChanged += OnLocatorPositionChanged;
			locator.StatusChanged += OnLocatorStatusChanged;

			IsListening = true;

			return Task.CompletedTask;
		}
		catch
		{
			IsListening = false;

			throw;
		}
	}

	/// <summary>
	/// Stop listening
	/// </summary>
	public override Task StopListeningAsync()
	{
		if (!IsListening)
		{
			return Task.CompletedTask;
		}

		_locator.PositionChanged -= OnLocatorPositionChanged;
		_locator.StatusChanged -= OnLocatorStatusChanged;

		IsListening = false;

		return Task.CompletedTask;
	}

	private Geolocator GetGeolocator()
	{
		var locator = _locator;

		if (locator != null)
		{
			return locator;
		}

		_locator = new Geolocator();

		return _locator;
	}

	private PositionStatus GetGeolocatorStatus()
	{
		var locator = GetGeolocator();
		return locator.LocationStatus;
	}

	private void OnLocatorPositionChanged(Geolocator sender, PositionChangedEventArgs args)
	{
		UpdateLastReadPosition(args.Position);
		OnChanged(CurrentValue);
	}

	private async void OnLocatorStatusChanged(Geolocator sender, StatusChangedEventArgs args)
	{
		LocationProviderError error;

		switch (args.Status)
		{
			case PositionStatus.Disabled:
			{
				error = LocationProviderError.Unauthorized;
				break;
			}
			case PositionStatus.NoData:
			{
				error = LocationProviderError.LocationUnavailable;
				break;
			}
			case PositionStatus.Ready:
			case PositionStatus.Initializing:
			case PositionStatus.NotInitialized:
			case PositionStatus.NotAvailable:
			default:
			{
				return;
			}
		}

		if (IsListening)
		{
			await StopListeningAsync();
			OnLocationProviderError(error);
		}

		_locator = null;
	}

	private void SetMapKey(string mapKey)
	{
		if (string.IsNullOrWhiteSpace(mapKey) && string.IsNullOrWhiteSpace(MapService.ServiceToken))
		{
			Debug.WriteLine("Map API key is required on UWP to reverse geolocate.");
			throw new ArgumentNullException(nameof(mapKey));
		}

		if (!string.IsNullOrWhiteSpace(mapKey))
		{
			MapService.ServiceToken = mapKey;
		}
	}

	private TLocation UpdateLastReadPosition(Geoposition position)
	{
		CurrentValue.HorizontalLocation.Latitude = position.Coordinate.Point.Position.Latitude;
		CurrentValue.HorizontalLocation.Longitude = position.Coordinate.Point.Position.Longitude;

		CurrentValue.HorizontalLocation.Accuracy = position.Coordinate.Accuracy;
		CurrentValue.HorizontalLocation.AccuracyReference = AccuracyReferenceType.Meters;

		CurrentValue.HorizontalLocation.SourceName = position.Coordinate.PositionSource.ToString();
		CurrentValue.HorizontalLocation.StatusTime = position.Coordinate.Timestamp.UtcDateTime;

		CurrentValue.VerticalLocation.SourceName = position.Coordinate.PositionSource.ToString();
		CurrentValue.VerticalLocation.StatusTime = position.Coordinate.Timestamp.UtcDateTime;

		if (position.Coordinate.Heading != null)
		{
			CurrentValue.HorizontalLocation.HasHeading = true;
			CurrentValue.HorizontalLocation.Heading = position.Coordinate.Heading.Value;
		}
		else
		{
			CurrentValue.HorizontalLocation.HasHeading = false;
		}

		if (position.Coordinate.Speed != null)
		{
			CurrentValue.HorizontalLocation.HasSpeed = true;
			CurrentValue.HorizontalLocation.Speed = position.Coordinate.Speed.Value;
		}
		else
		{
			CurrentValue.HorizontalLocation.HasSpeed = false;
		}

		if (position.Coordinate.AltitudeAccuracy.HasValue)
		{
			CurrentValue.VerticalLocation.Accuracy = position.Coordinate.AltitudeAccuracy.Value;
			CurrentValue.VerticalLocation.AccuracyReference = AccuracyReferenceType.Meters;
		}
		else
		{
			CurrentValue.VerticalLocation.AccuracyReference = AccuracyReferenceType.Unspecified;
		}

		CurrentValue.VerticalLocation.Altitude = position.Coordinate.Point.Position.Altitude;
		CurrentValue.VerticalLocation.AltitudeReference = position.Coordinate.Point.AltitudeReferenceSystem.ToAltitudeReferenceType();

		return CurrentValue;
	}

	#endregion
}