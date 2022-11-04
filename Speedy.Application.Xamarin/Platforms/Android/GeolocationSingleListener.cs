#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
internal class GeolocationSingleListener<T> : Object, ILocationListener
	where T : class, ILocation, new()
{
	#region Fields

	private readonly HashSet<LocationProviderSource> _activeSources;
	private Location _bestLocation;
	private readonly TaskCompletionSource<T> _completionSource;
	private readonly float _desiredAccuracy;
	private readonly IDispatcher _dispatcher;
	private readonly Action _finishedCallback;
	private readonly object _locationSync;
	private readonly Timer _timer;

	#endregion

	#region Constructors

	public GeolocationSingleListener(IDispatcher dispatcher, LocationManager manager, float desiredAccuracy, int timeout, IEnumerable<LocationProviderSource> activeSources, Action finishedCallback)
	{
		_activeSources = new HashSet<LocationProviderSource>(activeSources);
		_completionSource = new TaskCompletionSource<T>();
		_dispatcher = dispatcher;
		_desiredAccuracy = desiredAccuracy;
		_finishedCallback = finishedCallback;
		_locationSync = new object();

		foreach (var source in _activeSources)
		{
			var location = manager.GetLastKnownLocation(source.Provider);

			if ((location != null) && location.IsBetterLocation(_bestLocation))
			{
				_bestLocation = location;
			}
		}

		if (timeout != Timeout.Infinite)
		{
			_timer = new Timer(TimesUp, null, timeout, 0);
		}
	}

	#endregion

	#region Properties

	public Task<T> Task => _completionSource.Task;

	#endregion

	#region Methods

	public void Cancel()
	{
		_completionSource.TrySetCanceled();
	}

	public void OnLocationChanged(Location location)
	{
		if (location.Accuracy <= _desiredAccuracy)
		{
			Finish(location);
			return;
		}

		lock (_locationSync)
		{
			if (location.IsBetterLocation(_bestLocation))
			{
				_bestLocation = location;
			}
		}
	}

	public void OnProviderDisabled(string provider)
	{
		lock (_activeSources)
		{
			var foundSource = _activeSources.FirstOrDefault(x => x.Provider == provider);
			if (foundSource == null)
			{
				return;
			}

			if (_activeSources.Remove(foundSource) && (_activeSources.Count == 0))
			{
				_completionSource.TrySetException(new LocationProviderException(LocationProviderError.PositionUnavailable));
			}
		}
	}

	public void OnProviderEnabled(string provider)
	{
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

	private void Finish(Location location)
	{
		_finishedCallback?.Invoke();
		_completionSource.TrySetResult(location.ToPosition<T>());
	}

	private void TimesUp(object state)
	{
		lock (_locationSync)
		{
			if (_bestLocation == null)
			{
				if (_completionSource.TrySetCanceled())
				{
					_finishedCallback?.Invoke();
				}
			}
			else
			{
				Finish(_bestLocation);
			}
		}
	}

	#endregion
}