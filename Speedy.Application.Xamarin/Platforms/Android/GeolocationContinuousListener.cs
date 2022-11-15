#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Speedy.Application.Internal;
using Speedy.Devices.Location;
using Speedy.Logging;
using Location = Android.Locations.Location;
using LocationManager = Android.Locations.LocationManager;
using Object = Java.Lang.Object;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Xamarin;

[Preserve(AllMembers = true)]
internal class GeolocationContinuousListener<T, THorizontal, TVertical> : Object, ILocationListener
	where T : class, ILocation<THorizontal, TVertical>, new()
	where THorizontal : class, IHorizontalLocation, IUpdatable<THorizontal>
	where TVertical : class, IVerticalLocation, IUpdatable<TVertical>
{
	#region Fields

	private readonly HashSet<LocationProviderSource> _activeSources;
	private readonly IDispatcher _dispatcher;
	private Location _lastLocation;
	
	#endregion

	#region Constructors

	public GeolocationContinuousListener(IDispatcher dispatcher, LocationManager manager, IEnumerable<LocationProviderSource> providerSources)
	{
		_activeSources = new HashSet<LocationProviderSource>();
		_dispatcher = dispatcher;
		
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

		var previous = Interlocked.Exchange(ref _lastLocation, location);
		previous?.Dispose();

		PositionChanged?.Invoke(this, location.ToPosition<T, THorizontal, TVertical>());
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
				OnPositionError(LocationProviderError.LocationUnavailable);
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