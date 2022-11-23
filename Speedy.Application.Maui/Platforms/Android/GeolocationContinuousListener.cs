#region References

using System.Diagnostics.Tracing;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Speedy.Data;
using Speedy.Data.Location;
using Speedy.Logging;
using Location = Android.Locations.Location;
using LocationManager = Android.Locations.LocationManager;
using Object = Java.Lang.Object;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Maui;

[Preserve(AllMembers = true)]
internal class GeolocationContinuousListener<T, THorizontal, TVertical> : Object, ILocationListener
	where T : class, ILocation<THorizontal, TVertical>, new()
	where THorizontal : class, IHorizontalLocation, IUpdatable<THorizontal>
	where TVertical : class, IVerticalLocation, IUpdatable<TVertical>
{
	#region Fields

	private readonly IDispatcher _dispatcher;
	private Location _lastLocation;
	private readonly string _providerName;
	private readonly IEnumerable<IInformationProvider> _providerSources;

	#endregion

	#region Constructors

	public GeolocationContinuousListener(IDispatcher dispatcher, string providerName, LocationManager manager, IEnumerable<IInformationProvider> providerSources)
	{
		_dispatcher = dispatcher;
		_providerName = providerName;
		_providerSources = providerSources;
	}

	#endregion

	#region Methods

	public void OnLocationChanged(Location location)
	{
		if (location.Provider == null)
		{
			return;
		}

		lock (_providerSources)
		{
			// Check to see if the source is available and enabled
			var foundSource = _providerSources?.FirstOrDefault(x => x.ProviderName == location.Provider);
			if (foundSource is not { IsEnabled: true })
			{
				// Source is not found or is not enabled.
				return;
			}
		}

		LogEventWritten?.Invoke(this, new LogEventArgs(location.GetTimestamp().UtcDateTime, EventLevel.Verbose, $"Location updated, source: {location.Provider}"));

		var previous = Interlocked.Exchange(ref _lastLocation, location);
		previous?.Dispose();

		PositionChanged?.Invoke(this, location.ToPosition<T, THorizontal, TVertical>(_providerName));
	}

	public void OnProviderDisabled(string provider)
	{
		if (provider == LocationManager.PassiveProvider)
		{
			return;
		}

		lock (_providerSources)
		{
			var foundSource = _providerSources.FirstOrDefault(x => x.ProviderName == provider);
			if (foundSource == null)
			{
				return;
			}

			foundSource.IsEnabled = false;
		}
	}

	public void OnProviderEnabled(string provider)
	{
		if (provider == LocationManager.PassiveProvider)
		{
			return;
		}

		lock (_providerSources)
		{
			var foundSource = _providerSources.FirstOrDefault(x => x.ProviderName == provider);
			if (foundSource == null)
			{
				return;
			}

			foundSource.IsEnabled = true;
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