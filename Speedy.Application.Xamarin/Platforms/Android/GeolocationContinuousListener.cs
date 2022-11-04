#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Speedy.Devices.Location;
using Speedy.Logging;
using Location = Android.Locations.Location;
using Object = Java.Lang.Object;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Xamarin;

[Preserve(AllMembers = true)]
internal class GeolocationContinuousListener<T> : Object, ILocationListener
	where T : class, ILocation, new()
{
	#region Fields

	private LocationProviderSource _activeSource;
	private readonly HashSet<LocationProviderSource> _activeSources;
	private readonly IDispatcher _dispatcher;
	private Location _lastLocation;
	private readonly double _locationThreshold;
	private readonly TimeSpan _locationTimeout;
	private readonly LocationManager _manager;
	private IDictionary<string, LocationProviderSource> _sourceLookup;

	#endregion

	#region Constructors

	public GeolocationContinuousListener(IDispatcher dispatcher, LocationManager manager, TimeSpan locationTimeout, double locationThreshold, IList<LocationProviderSource> providerSources)
	{
		_activeSources = new HashSet<LocationProviderSource>();
		_dispatcher = dispatcher;
		_manager = manager;
		_locationTimeout = locationTimeout;
		_locationThreshold = locationThreshold;
		_sourceLookup = providerSources.ToDictionary(x => x.Provider, x => x);

		foreach (var source in providerSources)
		{
			if (manager.IsProviderEnabled(source.Provider))
			{
				_activeSources.Add(source);
			}
		}
	}

	#endregion

	#region Methods

	public void OnLocationChanged(Location location)
	{
		if (location.Provider == null)
		{
			return;
		}

		LogEventWritten?.Invoke(this, new LogEventArgs(location.GetTimestamp().UtcDateTime, EventLevel.Verbose, $"Location updated, source: {location.Provider}"));

		//var locationExpired = false;
		//var elapsed = TimeSpan.MinValue;

		//// Check to see if the provider of this location is different than the current provider.
		//if (location.Provider != _activeSource?.Provider)
		//{
		//	// Only test new location if there is an active provider and it's still enabled. There we can switch locations if
		//	// - there is no active provider therefore we can just use this location provider
		//	// - the current active provider was disabled, so switch to the new location provider
		//	if ((_activeSource?.Provider != null) && _manager.IsProviderEnabled(_activeSource.Provider))
		//	{
		//		// Get the provider for the location
		//		var locationProvider = _manager.GetProvider(location.Provider);
		//		if (locationProvider == null)
		//		{
		//			// Failed to find teh provider so just return;
		//			LogEventWritten?.Invoke(this, new LogEventArgs(location.GetTimestamp().UtcDateTime, EventLevel.Critical,
		//				$"Location ignored, provider not found: {location.Provider}"));
		//			return;
		//		}

		//		var accuracyChange = Math.Abs(location.Accuracy - _lastLocation.Accuracy);
		//		elapsed = GetTimeSpan(location.Time) - GetTimeSpan(_lastLocation.Time);
		//		locationExpired = elapsed >= _locationTimeout;

		//		// See if we should ignore this location due to it
		//		// - not having expired
		//		// - and the new location less accurate (higher is less) that current
		//		// - and the less accurate location is to large to accept
		//		if (!locationExpired
		//			&& (location.Accuracy > _lastLocation.Accuracy)
		//			&& (accuracyChange >= _locationThreshold))
		//		{
		//			LogEventWritten?.Invoke(this, new LogEventArgs(location.GetTimestamp().UtcDateTime, EventLevel.Verbose,
		//				$"Location ignored, source: {location.Provider}, {location.Accuracy} > {_lastLocation.Accuracy} ({accuracyChange})"));

		//			// The location has not expired and this location is not as accurate.
		//			// So just return out and not use the new location at all.
		//			location.Dispose();
		//			return;
		//		}
		//	}

		//	// Accept the location provider as the active provider
		//	_activeSource = _activeSources.FirstOrDefault(x => x.Provider == location.Provider);
		//}

		//LogEventWritten?.Invoke(this, new LogEventArgs(location.GetTimestamp().UtcDateTime, EventLevel.Verbose,
		//	locationExpired
		//		? $"Location changed because it expired after {elapsed} time."
		//		: location.Provider != _activeSource.Provider
		//			? $"Location changed by new provider of {location.Provider}"
		//			: "Location update by existing provider")
		//);

		var previous = Interlocked.Exchange(ref _lastLocation, location);
		previous?.Dispose();

		PositionChanged?.Invoke(this, location.ToPosition<T>());
	}

	public void OnProviderDisabled(string provider)
	{
		if (provider == LocationManager.PassiveProvider)
		{
			return;
		}

		lock (_activeSources)
		{
			var foundSource = _activeSources.FirstOrDefault(x => x.Provider == provider);
			if (foundSource == null)
			{
				return;
			}

			if (_activeSources.Remove(foundSource) && (_activeSources.Count == 0))
			{
				OnPositionError(LocationProviderError.PositionUnavailable);
			}
		}
	}

	public void OnProviderEnabled(string provider)
	{
		if (provider == LocationManager.PassiveProvider)
		{
			return;
		}

		lock (_activeSources)
		{
			var foundSource = _activeSources.FirstOrDefault(x => x.Provider == provider);
			if (foundSource != null)
			{
				foundSource.Enabled = true;
				return;
			}

			_activeSources.Add(new LocationProviderSource(_dispatcher) { Enabled = true, Provider = provider });
		}
	}

	public void OnStatusChanged(string provider, Availability status, Bundle extras)
	{
		switch (status)
		{
			case Availability.Available:
			{
				OnProviderEnabled(provider);
				break;
			}
			case Availability.OutOfService:
			{
				OnProviderDisabled(provider);
				break;
			}
		}
	}

	private TimeSpan GetTimeSpan(long time)
	{
		return new TimeSpan(TimeSpan.TicksPerMillisecond * time);
	}

	private void OnPositionError(LocationProviderError e)
	{
		PositionError?.Invoke(this, e);
	}

	#endregion

	#region Events

	public event EventHandler<LogEventArgs> LogEventWritten;
	public event EventHandler<T> PositionChanged;
	public event EventHandler<LocationProviderError> PositionError;

	#endregion
}