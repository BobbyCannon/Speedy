#region References

using System;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Data.Location;
#if (NET6_0_OR_GREATER)
using Windows.Devices.Geolocation;
#endif

#endregion

namespace Speedy.Application.Wpf;

/// <summary>
/// Implementation for LocationProvider
/// </summary>
public class WpfLocationProvider : WpfLocationProvider<Location, IHorizontalLocation, IVerticalLocation, LocationProviderSettings>, ILocationProvider
{
	#region Constructors
	
	/// <summary>
	/// Constructor for Implementation
	/// </summary>
	public WpfLocationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion
}

/// <summary>
/// Implementation for LocationProvider
/// </summary>
public class WpfLocationProvider<TLocation, THorizontal, TVertical, TLocationProviderSettings>
	: LocationProvider<TLocation, THorizontal, TVertical, TLocationProviderSettings>
	where TLocation : class, ILocation<THorizontal, TVertical>, IUpdateable, new()
	where THorizontal : class, IHorizontalLocation, IUpdateable
	where TVertical : class, IVerticalLocation, IUpdateable
	where TLocationProviderSettings : ILocationProviderSettings, IBindable, new()
{
	#region Fields

	#if (NET6_0_OR_GREATER)
	private readonly Geolocator _locator;
	#endif

	#endregion

	#region Constructors

	/// <summary>
	/// Constructor for Implementation
	/// </summary>
	public WpfLocationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
		#if (NET6_0_OR_GREATER)
		_locator = new Geolocator();
		_locator.PositionChanged += LocatorOnPositionChanged;
		#endif

		CurrentValue.HorizontalLocation.ProviderName = ProviderName;
		CurrentValue.VerticalLocation.ProviderName = ProviderName;

		HasPermission = true;
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public override bool IsLocationAvailable => false;

	/// <inheritdoc />
	public override bool IsLocationEnabled => false;

	/// <inheritdoc />
	public sealed override string ProviderName => "WPF Windows";

	/// <inheritdoc />
	public sealed override bool HasPermission { get; protected set; }

	#endregion

	#region Methods

	/// <summary>
	/// Gets position async with specified parameters
	/// </summary>
	/// <param name="timeout"> Timeout to wait, Default Infinite </param>
	/// <param name="cancelToken"> Cancellation token </param>
	/// <returns> ProviderLocation </returns>
	public override async Task<TLocation> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null)
	{
		#if (NET6_0_OR_GREATER)
		var location = await _locator.GetGeopositionAsync();
		UpdateCurrentValue(location);
		return CurrentValue;
		#else
		return await Task.FromResult(CurrentValue);
		#endif
	}

	/// <inheritdoc />
	public override Task StartMonitoringAsync()
	{
		if (IsMonitoring)
		{
			return Task.CompletedTask;
		}

		IsMonitoring = true;

		return Task.CompletedTask;
	}

	/// <summary>
	/// Stop listening
	/// </summary>
	public override Task StopMonitoringAsync()
	{
		if (!IsMonitoring)
		{
			return Task.CompletedTask;
		}

		IsMonitoring = false;

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		#if (NET6_0_OR_GREATER)
		_locator.PositionChanged -= LocatorOnPositionChanged;
		#endif
		base.Dispose(disposing);
	}

	#if (NET6_0_OR_GREATER)

	private void LocatorOnPositionChanged(Geolocator sender, PositionChangedEventArgs args)
	{
		UpdateCurrentValue(args.Position);
	}

	private TLocation UpdateCurrentValue(Geoposition position)
	{
		CurrentValue.HorizontalLocation.Latitude = position.Coordinate.Point.Position.Latitude;
		CurrentValue.HorizontalLocation.Longitude = position.Coordinate.Point.Position.Longitude;
		CurrentValue.HorizontalLocation.HasValue = true;

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
		CurrentValue.VerticalLocation.AltitudeReference = ToAltitudeReferenceType(position.Coordinate.Point.AltitudeReferenceSystem);
		CurrentValue.VerticalLocation.HasValue = CurrentValue.VerticalLocation.AltitudeReference != AltitudeReferenceType.Unspecified;

		OnUpdated(CurrentValue);

		return CurrentValue;
	}

	private static AltitudeReferenceType ToAltitudeReferenceType(AltitudeReferenceSystem altitudeReference)
	{
		return altitudeReference switch
		{
			AltitudeReferenceSystem.Terrain => AltitudeReferenceType.Terrain,
			AltitudeReferenceSystem.Ellipsoid => AltitudeReferenceType.Ellipsoid,
			AltitudeReferenceSystem.Geoid => AltitudeReferenceType.Geoid,
			AltitudeReferenceSystem.Unspecified => AltitudeReferenceType.Unspecified,
			_ => AltitudeReferenceType.Unspecified
		};
	}

	#endif

	#endregion
}