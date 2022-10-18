#region References

using System;
using System.Collections.Generic;
using System.Threading;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Speedy.Devices.Location;
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
	private readonly TimeSpan _timePeriod;

	#endregion

	#region Constructors

	public GeolocationContinuousListener(LocationManager manager, TimeSpan timePeriod, IList<string> providers)
	{
		_manager = manager;
		_activeProviders = new HashSet<string>();
		_timePeriod = timePeriod;
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
		if (location.Provider != _activeProvider)
		{
			if ((_activeProvider != null) && _manager.IsProviderEnabled(_activeProvider))
			{
				var pr = _manager.GetProvider(location.Provider);
				var lapsed = GetTimeSpan(location.Time) - GetTimeSpan(_lastLocation.Time);

				if ((pr.Accuracy > _manager.GetProvider(_activeProvider).Accuracy)
					&& (lapsed < _timePeriod.Add(_timePeriod)))
				{
					location.Dispose();
					return;
				}
			}

			_activeProvider = location.Provider;
		}

		var previous = Interlocked.Exchange(ref _lastLocation, location);
		if (previous != null)
		{
			previous.Dispose();
		}

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
				PositionError?.Invoke(this, LocationProviderError.PositionUnavailable);
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
				OnProviderEnabled(provider);
				break;

			case Availability.OutOfService:
				OnProviderDisabled(provider);
				break;
		}
	}

	private TimeSpan GetTimeSpan(long time)
	{
		return new TimeSpan(TimeSpan.TicksPerMillisecond * time);
	}

	#endregion

	#region Events

	public event EventHandler<T> PositionChanged;
	public event EventHandler<LocationProviderError> PositionError;

	#endregion
}