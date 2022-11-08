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
public class LocationProviderImplementation<T, T2> : LocationProvider<T, T2>
	where T : class, ILocation, new()
	where T2 : LocationProviderSettings, new()
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

		LastReadLocation.ProviderName = "Xamarin Windows";
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public override bool IsLocationAvailable
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
	public override bool IsLocationEnabled
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
	public override Task<T> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null)
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
		var tcs = new TaskCompletionSource<T>();

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
		OnPositionChanged(LastReadLocation);
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
				error = LocationProviderError.PositionUnavailable;
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
			OnPositionError(error);
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

	private T UpdateLastReadPosition(Geoposition position)
	{
		LastReadLocation.Latitude = position.Coordinate.Point.Position.Latitude;
		LastReadLocation.Longitude = position.Coordinate.Point.Position.Longitude;

		LastReadLocation.HorizontalAccuracy = position.Coordinate.Accuracy;
		LastReadLocation.HorizontalAccuracyReference = AccuracyReferenceType.Meters;

		LastReadLocation.HorizontalSourceName = position.Coordinate.PositionSource.ToString();
		LastReadLocation.HorizontalStatusTime = position.Coordinate.Timestamp.UtcDateTime;

		LastReadLocation.VerticalSourceName = position.Coordinate.PositionSource.ToString();
		LastReadLocation.VerticalStatusTime = position.Coordinate.Timestamp.UtcDateTime;

		if (position.Coordinate.Heading != null)
		{
			LastReadLocation.HasHorizontalHeading = true;
			LastReadLocation.HorizontalHeading = position.Coordinate.Heading.Value;
		}

		if (position.Coordinate.Speed != null)
		{
			LastReadLocation.HasHorizontalSpeed = true;
			LastReadLocation.HorizontalSpeed = position.Coordinate.Speed.Value;
		}

		if (position.Coordinate.AltitudeAccuracy.HasValue)
		{
			LastReadLocation.VerticalAccuracy = position.Coordinate.AltitudeAccuracy.Value;
		}

		LastReadLocation.Altitude = position.Coordinate.Point.Position.Altitude;
		LastReadLocation.AltitudeReference = position.Coordinate.Point.AltitudeReferenceSystem.ToAltitudeReferenceType();

		return LastReadLocation;
	}

	#endregion
}