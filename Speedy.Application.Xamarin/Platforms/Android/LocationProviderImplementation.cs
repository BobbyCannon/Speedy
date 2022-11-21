#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Java.Lang;
using Speedy.Application.Internal;
using Speedy.Devices.Location;
using Speedy.Extensions;
using Speedy.Logging;
using Xamarin.Essentials;
using Debug = System.Diagnostics.Debug;
using Exception = System.Exception;
#if GOOGLEPLAY
using Android.Gms.Common;
using Android.Gms.Location;
using LocationRequest = Android.Gms.Location.LocationRequest;
using XamarinForms = Xamarin.Forms;
using Location = Android.Locations.Location;
#endif

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Xamarin;

/// <summary>
/// Implementation for Feature
/// </summary>
[Preserve(AllMembers = true)]
public class LocationProviderImplementation<TLocation, THorizontal, TVertical, TLocationProviderSettings>
	: LocationProvider<TLocation, THorizontal, TVertical, TLocationProviderSettings>
	where TLocation : class, ILocation<THorizontal, TVertical>, new()
	where THorizontal : class, IHorizontalLocation, IUpdatable<THorizontal>
	where TVertical : class, IVerticalLocation, IUpdatable<TVertical>
	where TLocationProviderSettings : ILocationProviderSettings, IBindable, new()
{
	#region Fields

	#if GOOGLEPLAY
	private const string FusedGooglePlusKey = "fused g+";
	private FusedLocationProviderClient _fusedListener;
	private FusedLocationProviderCallback _fusedCallback;
	#endif

	private GeolocationContinuousListener<TLocation, THorizontal, TVertical> _listener;
	private LocationManager _locationManager;
	private readonly object _positionSync;
	private IDictionary<string, LocationProviderSource> _providerSources;
	private GeolocationSingleListener<TLocation, THorizontal, TVertical> _singleListener;

	#endregion

	#region Constructors

	/// <summary>
	/// Default constructor
	/// </summary>
	protected LocationProviderImplementation(IDispatcher dispatcher) : base(dispatcher)
	{
		_positionSync = new object();
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public bool IsLocationAvailable => ProviderSources.Any();

	/// <inheritdoc />
	public bool IsLocationEnabled => ProviderSources.Any(x => x.Enabled && Manager.IsProviderEnabled(x.Provider));

	/// <inheritdoc />
	public override string ProviderName => "Xamarin Android";

	protected bool HasPermission { get; private set; }

	/// <summary>
	/// Gets all providers from the Location Manager.
	/// </summary>
	internal IEnumerable<LocationProviderSource> ProviderSources
	{
		get
		{
			if (_providerSources == null)
			{
				var defaultEnabled = new[] { LocationManager.GpsProvider, "fused" };
				_providerSources = Manager.GetProviders(false)
					.Select(x => new LocationProviderSource
					{
						Provider = x,
						Enabled = defaultEnabled.Contains(x)
					})
					.ToDictionary(x => x.Provider, x => x);

				#if GOOGLEPLAY
				var t = new LocationProviderSource { Enabled = true, Provider = FusedGooglePlusKey };
				_providerSources.Add(t.Provider, t);
				#endif
			}
			return _providerSources.Values;
		}
	}

	/// <summary>
	/// The android location manager.
	/// </summary>
	private LocationManager Manager => _locationManager ??= (LocationManager) Android.App.Application.Context.GetSystemService(Context.LocationService);

	#endregion

	#region Methods

	/// <summary>
	/// Gets position async with specified parameters
	/// </summary>
	/// <param name="timeout"> Timeout to wait, Default Infinite </param>
	/// <param name="cancelToken"> Cancellation token </param>
	/// <returns> ProviderLocation </returns>
	/// <remarks>
	/// bug: we must work on thread safety of this method.
	/// Touching "Manager", which is global, can be very dangerous.
	/// </remarks>
	public override async Task<TLocation> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null)
	{
		var timeoutMilliseconds = timeout.HasValue
			? (int) timeout.Value.TotalMilliseconds
			: Timeout.Infinite;

		if ((timeoutMilliseconds <= 0) && (timeoutMilliseconds != Timeout.Infinite))
		{
			throw new ArgumentOutOfRangeException(nameof(timeout), "timeout must be greater than or equal to 0");
		}

		cancelToken ??= CancellationToken.None;

		var hasPermission = CheckWhenInUsePermission();
		if (!hasPermission)
		{
			throw new LocationProviderException(LocationProviderError.Unauthorized);
		}

		var tcs = new TaskCompletionSource<TLocation>();
		var providerSources = ProviderSources.ToArray();

		if (!IsMonitoring)
		{
			void singleListenerFinishCallback()
			{
				if (_singleListener == null)
				{
					return;
				}

				for (var i = 0; i < providerSources.Length; ++i)
				{
					Manager.RemoveUpdates(_singleListener);
				}
			}

			_singleListener = new GeolocationSingleListener<TLocation, THorizontal, TVertical>(Dispatcher,
				ProviderName,
				Manager,
				LocationProviderSettings.DesiredAccuracy,
				timeoutMilliseconds,
				ProviderSources
					.Where(x => x.Enabled)
					.Where(x => Manager.IsProviderEnabled(x.Provider))
					.ToList(),
				singleListenerFinishCallback);

			if (cancelToken != CancellationToken.None)
			{
				cancelToken.Value.Register(() =>
				{
					_singleListener.Cancel();

					for (var i = 0; i < providerSources.Length; ++i)
					{
						Manager.RemoveUpdates(_singleListener);
					}
				}, true);
			}

			try
			{
				var looper = Looper.MyLooper() ?? Looper.MainLooper;
				var enabled = 0;

				for (var i = 0; i < providerSources.Length; ++i)
				{
					if (Manager.IsProviderEnabled(providerSources[i].Provider))
					{
						enabled++;
					}

					Manager.RequestLocationUpdates(providerSources[i].Provider, 0, 0, _singleListener, looper);
				}

				if (enabled == 0)
				{
					for (var i = 0; i < providerSources.Length; ++i)
					{
						Manager.RemoveUpdates(_singleListener);
					}

					tcs.SetException(new LocationProviderException(LocationProviderError.LocationUnavailable));
					return await tcs.Task;
				}
			}
			catch (SecurityException ex)
			{
				tcs.SetException(new LocationProviderException(LocationProviderError.Unauthorized, ex));
				return await tcs.Task;
			}

			return await _singleListener.Task;
		}

		// If we're already listening, just use the current listener
		lock (_positionSync)
		{
			tcs.SetResult(CurrentValue);
		}

		return await tcs.Task;
	}

	/// <inheritdoc />
	public override Task StartMonitoringAsync()
	{
		if (IsMonitoring)
		{
			return Task.CompletedTask;
		}

		#if GOOGLEPLAY
		if (!IsGooglePlayServicesInstalled())
		{
			return;
		}
		#endif

		LocationProviderSettings.Cleanup();

		HasPermission = LocationProviderSettings.RequireLocationAlwaysPermission
			? CheckAlwaysPermissions()
			: CheckWhenInUsePermission();

		if (!HasPermission)
		{
			ListenerPositionError(this, LocationProviderError.Unauthorized);
			return Task.CompletedTask;
		}

		var sources = ProviderSources.ToArray();
		var looper = Looper.MyLooper() ?? Looper.MainLooper;

		#if GOOGLEPLAY
		if ((XamarinPlatform.MainActivity != null) && _providerSources[FusedGooglePlusKey].Enabled)
		{
			_fusedListener = LocationServices.GetFusedLocationProviderClient(XamarinPlatform.MainActivity);
			var locationRequest = LocationRequest.Create();
			locationRequest.SetPriority(Priority.PriorityHighAccuracy);
			locationRequest.SetInterval((long) LocationProviderSettings.MinimumTime.TotalMilliseconds);
			locationRequest.SetFastestInterval((long) LocationProviderSettings.MinimumTime.TotalMilliseconds);
			locationRequest.SetSmallestDisplacement(LocationProviderSettings.MinimumDistance);
			_fusedCallback = new FusedLocationProviderCallback(FusedLocationProviderLocationChanged);
			_fusedListener.RequestLocationUpdates(locationRequest, _fusedCallback, looper);
			_providerSources[FusedGooglePlusKey].Listening = true;
		}
		#endif

		_listener = new GeolocationContinuousListener<TLocation, THorizontal, TVertical>(Dispatcher, ProviderName, Manager, sources);
		_listener.LogEventWritten += ListenerOnLogEventWritten;
		_listener.PositionChanged += ListenerPositionChanged;
		_listener.PositionError += ListenerPositionError;

		for (var i = 0; i < sources.Length; ++i)
		{
			var source = sources[i];

			#if GOOGLEPLAY
			if (source.Provider == FusedGooglePlusKey)
			{
				continue;
			}
			#endif

			if (!source.Enabled)
			{
				source.Listening = false;
				source.Enabled = false;
				continue;
			}

			Manager.RequestLocationUpdates(source.Provider,
				(long) LocationProviderSettings.MinimumTime.TotalMilliseconds,
				LocationProviderSettings.MinimumDistance,
				_listener,
				looper);

			source.Listening = true;
		}

		Status = "Is Monitoring";
		IsMonitoring = true;

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public override Task StopMonitoringAsync()
	{
		if (_listener == null)
		{
			return Task.CompletedTask;
		}

		_listener.LogEventWritten -= ListenerOnLogEventWritten;
		_listener.PositionChanged -= ListenerPositionChanged;
		_listener.PositionError -= ListenerPositionError;

		#if GOOGLEPLAY
		if (_fusedListener != null)
		{
			_providerSources[FusedGooglePlusKey].Listening = false;
			_fusedListener.RemoveLocationUpdates(_fusedCallback);

			_fusedListener.Dispose();
			_fusedCallback.Dispose();

			_fusedListener = null;
			_fusedCallback = null;
		}
		#endif

		try
		{
			Manager.RemoveUpdates(_listener);
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Unable to remove updates: {ex}");
		}

		_providerSources.ForEach(x => x.Value.Listening = false);
		_listener = null;

		Status = string.Empty;
		IsMonitoring = false;

		return Task.CompletedTask;
	}

	private bool CheckAlwaysPermissions()
	{
		var permissionStatus = Permissions
			.CheckStatusAsync<Permissions.LocationWhenInUse>()
			.AwaitResults(TimeSpan.FromSeconds(1));

		if (permissionStatus == PermissionStatus.Granted)
		{
			return true;
		}

		Status = "Currently does not have Location permissions, requesting permissions";

		permissionStatus = Permissions
			.RequestAsync<Permissions.LocationAlways>()
			.AwaitResults(new TimeSpan(0, 0, 1));

		if (permissionStatus == PermissionStatus.Granted)
		{
			return true;
		}

		Status = "Location permission denied.";
		return false;
	}

	private bool CheckWhenInUsePermission()
	{
		var permissionStatus = Permissions
			.CheckStatusAsync<Permissions.LocationWhenInUse>()
			.AwaitResults(TimeSpan.FromSeconds(1));

		if (permissionStatus == PermissionStatus.Granted)
		{
			return true;
		}

		Status = "Currently does not have Location permissions, requesting permissions";

		permissionStatus = Permissions
			.RequestAsync<Permissions.LocationWhenInUse>()
			.AwaitResults(TimeSpan.FromSeconds(1));

		if (permissionStatus == PermissionStatus.Granted)
		{
			return true;
		}

		Status = "Location permission denied.";
		return false;
	}

	#if GOOGLEPLAY
	private void FusedLocationProviderLocationChanged(Location obj)
	{
		var t = obj.ToPosition<T>();
		t.HorizontalSourceName = FusedGooglePlusKey;
		t.VerticalSourceName = FusedGooglePlusKey;
		OnPositionChanged(t);
	}


	private bool IsGooglePlayServicesInstalled()
	{
		var queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(XamarinPlatform.MainActivity);
		if (queryResult == ConnectionResult.Success)
		{
			Status = "Google Play Services is installed on this device.";
			return true;
		}

		if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
		{
			// Check if there is a way the user can resolve the issue
			var errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
			Status = $"There is a problem with Google Play Services on this device: {queryResult} - {errorString}";
		}

		return false;
	}

	#endif
	private void ListenerOnLogEventWritten(object sender, LogEventArgs e)
	{
		OnLogEventWritten(e);
	}

	private void ListenerPositionChanged(object sender, TLocation e)
	{
		// Ignore anything that might come in after stop listening
		if (!IsMonitoring || e is null)
		{
			return;
		}

		lock (_positionSync)
		{
			UpdateCurrentValue(e);
		}
	}

	private async void ListenerPositionError(object sender, LocationProviderError e)
	{
		await StopMonitoringAsync();
		OnLocationProviderError(e);
	}

	#endregion

	#region Classes

	#if GOOGLEPLAY
	private class FusedLocationProviderCallback : LocationCallback
	{
		#region Fields

		private readonly Action<Location> _callback;

		#endregion

		#region Constructors

		public FusedLocationProviderCallback(Action<Location> callback)
		{
			_callback = callback;
		}

		#endregion

		#region Methods

		public override void OnLocationAvailability(LocationAvailability locationAvailability)
		{
			//Log.Debug("FusedLocationProviderSample", "IsLocationAvailable: {0}",locationAvailability.IsLocationAvailable);
		}

		public override void OnLocationResult(LocationResult result)
		{
			if (result.Locations.Any())
			{
				var location = result.Locations.First();
				_callback.Invoke(location);
				//Log.Debug("Sample", "The latitude is :" + location.Latitude);
			}
			// No locations to work with.
		}

		#endregion
	}
	#endif

	#endregion
}