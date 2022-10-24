#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
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

	private string _activeProvider;
	private readonly HashSet<string> _activeProviders;
	private Location _lastLocation;
	private readonly LocationManager _manager;
	private IList<string> _providers;
	private readonly TimeSpan _locationTimeout;
	private readonly double _locationThreshold;

	#endregion

	#region Constructors

	public GeolocationContinuousListener(LocationManager manager, TimeSpan locationTimeout, double locationThreshold, IList<string> providers)
	{
		_activeProviders = new HashSet<string>();
		_manager = manager;
		_locationTimeout = locationTimeout;
		_locationThreshold = locationThreshold;
		_providers = providers;

		foreach (var p in providers)
		{
			if (manager.IsProviderEnabled(p))
			{
				_activeProviders.Add(p);
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

		var locationExpired = false;
		var elapsed = TimeSpan.MinValue;

		// Check to see if the provider of this location is different than the current provider.
		if (location.Provider != _activeProvider)
		{
			// Only test new location if there is an active provider and it's still enabled. There we can switch locations if
			// - there is no active provider therefore we can just use this location provider
			// - the current active provider was disabled, so switch to the new location provider
			if ((_activeProvider != null) && _manager.IsProviderEnabled(_activeProvider))
			{
				// Get the provider for the location
				var locationProvider = _manager.GetProvider(location.Provider);
				if (locationProvider == null)
				{
					// Failed to find teh provider so just return;
					LogEventWritten?.Invoke(this, new LogEventArgs(location.GetTimestamp().UtcDateTime, EventLevel.Critical,
						$"Location ignored, provider not found: {location.Provider}"));
					return;
				}

				var accuracyChange = Math.Abs(location.Accuracy - _lastLocation.Accuracy);
				elapsed = GetTimeSpan(location.Time) - GetTimeSpan(_lastLocation.Time);
				locationExpired = elapsed >= _locationTimeout;
				
				// See if we should ignore this location due to it
				// - not having expired
				// - and the new location less accurate (higher is less) that current
				// - and the less accurate location is to large to accept
				if (!locationExpired
					&& location.Accuracy > _lastLocation.Accuracy
					&& accuracyChange >= _locationThreshold)
				{
					LogEventWritten?.Invoke(this, new LogEventArgs(location.GetTimestamp().UtcDateTime, EventLevel.Verbose,
						$"Location ignored, source: {location.Provider}, {location.Accuracy} > {_lastLocation.Accuracy} ({accuracyChange})"));

					// The location has not expired and this location is not as accurate.
					// So just return out and not use the new location at all.
					location.Dispose();
					return;
				}
			}

			// Accept the location provider as the active provider
			_activeProvider = location.Provider;
		}

		LogEventWritten?.Invoke(this, new LogEventArgs(location.GetTimestamp().UtcDateTime, EventLevel.Verbose,
			locationExpired
				? $"Location changed because it expired after {elapsed} time."
				: location.Provider != _activeProvider
					? $"Location changed by new provider of {location.Provider}"
					: "Location update by existing provider")
		);

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

		lock (_activeProviders)
		{
			if (_activeProviders.Remove(provider) && (_activeProviders.Count == 0))
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

		lock (_activeProviders)
		{
			_activeProviders.Add(provider);
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