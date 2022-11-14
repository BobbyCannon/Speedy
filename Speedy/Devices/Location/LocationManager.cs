#region References

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Collections;
using Speedy.Serialization;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// The manager for location.
/// </summary>
public class LocationManager : LocationManager<Location, IHorizontalLocation, IVerticalLocation, LocationProviderSettings>
{
	#region Constructors

	/// <inheritdoc />
	public LocationManager(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion
}

/// <summary>
/// The manager for location.
/// </summary>
/// <typeparam name="TLocation"> The full location type. </typeparam>
/// <typeparam name="THorizontal"> The horizontal type. </typeparam>
/// <typeparam name="TVertical"> The vertical type. </typeparam>
/// <typeparam name="TLocationProviderSettings"> The location settings for the provider. </typeparam>
public class LocationManager<TLocation, THorizontal, TVertical, TLocationProviderSettings>
	: LocationProvider<TLocation, TLocationProviderSettings>
	where TLocation : class, ILocation, THorizontal, TVertical, new()
	where THorizontal : class, IHorizontalLocation
	where TVertical : class, IVerticalLocation
	where TLocationProviderSettings : class, ILocationProviderSettings, new()
{
	#region Constructors

	/// <summary>
	/// Instantiates a location manager.
	/// </summary>
	public LocationManager(IDispatcher dispatcher) : base(dispatcher)
	{
		LocationProviders = new BaseObservableCollection<ILocationProvider<TLocation>>();
		LocationProviders.CollectionChanged += LocationProvidersOnCollectionChanged;

		Comparer = new LocationComparer<TLocation, THorizontal, TVertical>();
		Settings = new TLocationProviderSettings();
		Settings.UpdateDispatcher(dispatcher);
	}

	#endregion

	#region Properties

	/// <summary>
	/// The comparer for location updates.
	/// </summary>
	public LocationComparer<TLocation, THorizontal, TVertical> Comparer { get; }

	/// <summary>
	/// The list of location providers.
	/// </summary>
	public ObservableCollection<ILocationProvider<TLocation>> LocationProviders { get; }

	/// <summary>
	/// Default settings for the location providers.
	/// </summary>
	public TLocationProviderSettings Settings { get; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public override Task<TLocation> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null)
	{
		return Task.FromResult(Comparer.Value);
	}

	/// <inheritdoc />
	public override async Task StartListeningAsync()
	{
		foreach (var provider in LocationProviders)
		{
			provider.LocationChanged += ProviderOnLocationChanged;
			await provider.StartListeningAsync();
		}

		IsListening = true;
	}

	/// <inheritdoc />
	public override async Task StopListeningAsync()
	{
		foreach (var provider in LocationProviders)
		{
			provider.LocationChanged -= ProviderOnLocationChanged;
			await provider.StopListeningAsync();
		}

		IsListening = false;
	}

	private void LocationProvidersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		// todo:
	}

	private void ProviderOnLocationChanged(object sender, TLocation e)
	{
		if (!Comparer.Refresh(e))
		{
			return;
		}

		OnLocationChanged(((ICloneable<TLocation>) Comparer.Value).ShallowClone());
	}

	#endregion
}