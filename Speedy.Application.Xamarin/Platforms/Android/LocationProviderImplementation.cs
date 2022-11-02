#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Gms.Location;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Java.Lang;
using Speedy.Devices.Location;
using Speedy.Extensions;
using Speedy.Logging;
using Xamarin.Essentials;
using Debug = System.Diagnostics.Debug;
using Exception = System.Exception;
using Location = Android.Locations.Location;
using LocationRequest = Android.Gms.Location.LocationRequest;
using XamarinForms = Xamarin.Forms;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Xamarin;

/// <summary>
/// Implementation for Feature
/// </summary>
[Preserve(AllMembers = true)]
public class LocationProviderImplementation<T, T2> : LocationProvider<T, T2>
	where T : class, ILocation, new()
	where T2 : LocationProviderSettings, new()
{
	#region Constants

	private const string FusedGooglePlusKey = "fused g+";

	#endregion

	#region Fields

	private FusedLocationProviderCallback _fusedCallback;
	private FusedLocationProviderClient _fusedListener;
	private GeolocationContinuousListener<T> _listener;
	private LocationManager _locationManager;
	private readonly object _positionSync;
	private IDictionary<string, LocationProviderSource> _providerSources;
	private GeolocationSingleListener<T> _singleListener;

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
	public override bool IsListening => _listener != null;

	/// <inheritdoc />
	public override bool IsLocationAvailable => HasProviderSources;

	/// <inheritdoc />
	public override bool IsLocationEnabled => ProviderSources.Any(x => x.Enabled && Manager.IsProviderEnabled(x.Provider));

	/// <summary>
	/// Gets all providers from the Location Manager.
	/// </summary>
	public override IEnumerable<LocationProviderSource> ProviderSources
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
				var t = new LocationProviderSource { Enabled = true, Provider = FusedGooglePlusKey };
				_providerSources.Add(t.Provider, t);
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
	public override async Task<T> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null)
	{
		var timeoutMilliseconds = timeout.HasValue
			? (int) timeout.Value.TotalMilliseconds
			: Timeout.Infinite;

		if ((timeoutMilliseconds <= 0) && (timeoutMilliseconds != Timeout.Infinite))
		{
			throw new ArgumentOutOfRangeException(nameof(timeout), "timeout must be greater than or equal to 0");
		}

		cancelToken ??= CancellationToken.None;

		var hasPermission = await CheckWhenInUsePermission();
		if (!hasPermission)
		{
			throw new LocationProviderException(LocationProviderError.Unauthorized);
		}

		var tcs = new TaskCompletionSource<T>();
		var providerSources = ProviderSources.ToArray();

		if (!IsListening)
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

			_singleListener = new GeolocationSingleListener<T>(Dispatcher,
				Manager,
				(float) LocationProviderSettings.DesiredAccuracy,
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

					tcs.SetException(new LocationProviderException(LocationProviderError.PositionUnavailable));
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
			tcs.SetResult(LastReadLocation);
		}

		return await tcs.Task;
	}

	/// <inheritdoc />
	public override async Task StartListeningAsync()
	{
		if (IsListening)
		{
			return;
		}

		LocationProviderSettings.Cleanup();

		if (LocationProviderSettings.RequireLocationAlwaysPermission)
		{
			HasPermission = await CheckAlwaysPermissions();
		}
		else
		{
			HasPermission = await CheckWhenInUsePermission();
		}

		if (!HasPermission)
		{
			ListenerPositionError(this, LocationProviderError.Unauthorized);
			return;
		}

		var sources = ProviderSources.ToArray();
		var looper = Looper.MyLooper() ?? Looper.MainLooper;
		
		if ((XamarinPlatform.MainActivity != null) && _providerSources[FusedGooglePlusKey].Enabled)
		{
			_fusedListener = LocationServices.GetFusedLocationProviderClient(XamarinPlatform.MainActivity);
			var locationRequest = LocationRequest.Create();
			locationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
			locationRequest.SetInterval((long) LocationProviderSettings.MinimumTime.TotalMilliseconds);
			locationRequest.SetFastestInterval((long) LocationProviderSettings.MinimumTime.TotalMilliseconds);
			locationRequest.SetSmallestDisplacement((float) LocationProviderSettings.MinimumDistance);
			_fusedCallback = new FusedLocationProviderCallback(FusedLocationProviderLocationChanged);
			_fusedListener.RequestLocationUpdates(locationRequest, _fusedCallback, looper);
			_providerSources[FusedGooglePlusKey].Listening = true;
		}

		_listener = new GeolocationContinuousListener<T>(Dispatcher,
			Manager,
			LocationProviderSettings.LocationChangeTimeout,
			LocationProviderSettings.LocationChangeAccuracyThreshold,
			sources);

		_listener.LogEventWritten += ListenerOnLogEventWritten;
		_listener.PositionChanged += ListenerPositionChanged;
		_listener.PositionError += ListenerPositionError;

		for (var i = 0; i < sources.Length; ++i)
		{
			var source = sources[i];

			if (source.Provider == FusedGooglePlusKey)
			{
				continue;
			}

			if (!source.Enabled)
			{
				source.Listening = false;
				continue;
			}

			Manager.RequestLocationUpdates(source.Provider,
				(long) LocationProviderSettings.MinimumTime.TotalMilliseconds,
				(float) LocationProviderSettings.MinimumDistance,
				_listener,
				looper);

			source.Listening = true;
		}

		Status = "Is Listening";
		OnPropertyChanged(nameof(IsListening));
	}

	/// <inheritdoc />
	public override Task StopListeningAsync()
	{
		if (_listener == null)
		{
			return Task.CompletedTask;
		}

		_listener.LogEventWritten -= ListenerOnLogEventWritten;
		_listener.PositionChanged -= ListenerPositionChanged;
		_listener.PositionError -= ListenerPositionError;

		if (_fusedListener != null)
		{
			_providerSources[FusedGooglePlusKey].Listening = false;
			_fusedListener.RemoveLocationUpdates(_fusedCallback);

			_fusedListener.Dispose();
			_fusedCallback.Dispose();

			_fusedListener = null;
			_fusedCallback = null;
		}

		var count = ProviderSources.Count();

		// Remove sources, todo: why we loop here? weird?
		for (var i = 0; i < count; i++)
		{
			try
			{
				Manager.RemoveUpdates(_listener);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to remove updates: {ex}");
			}
		}

		_providerSources.ForEach(x => x.Value.Listening = false);
		_listener = null;

		Status = string.Empty;
		OnPropertyChanged(nameof(IsListening));

		return Task.CompletedTask;
	}

	private async Task<bool> CheckAlwaysPermissions()
	{
		var permissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
		if (permissionStatus == PermissionStatus.Granted)
		{
			return true;
		}

		Status = "Currently does not have Location permissions, requesting permissions";

		permissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();

		if (permissionStatus == PermissionStatus.Granted)
		{
			return true;
		}

		Status = "Location permission denied.";
		return false;
	}

	private async Task<bool> CheckWhenInUsePermission()
	{
		var permissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
		if (permissionStatus == PermissionStatus.Granted)
		{
			return true;
		}

		Status = "Currently does not have Location permissions, requesting permissions";

		permissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

		if (permissionStatus == PermissionStatus.Granted)
		{
			return true;
		}

		Status = "Location permission denied.";
		return false;
	}

	private void FusedLocationProviderLocationChanged(Location obj)
	{
		var t = obj.ToPosition<T>();
		t.SourceName = FusedGooglePlusKey;
		OnPositionChanged(t);
	}

	private bool IsGooglePlayServicesInstalled()
	{
		//var queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
		//if (queryResult == ConnectionResult.Success)
		//{
		//	//Log.Info("MainActivity", "Google Play Services is installed on this device.");
		//	return true;
		//}

		//if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
		//{
		//	// Check if there is a way the user can resolve the issue
		//	var errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
		//	//Log.Error("MainActivity", "There is a problem with Google Play Services on this device: {0} - {1}",
		//	//	queryResult, errorString);

		//	// Alternately, display the error to the user.
		//}

		return false;
	}

	private void ListenerOnLogEventWritten(object sender, LogEventArgs e)
	{
		OnLogEventWritten(e);
	}

	private void ListenerPositionChanged(object sender, T e)
	{
		// Ignore anything that might come in after stop listening
		if (!IsListening)
		{
			return;
		}

		lock (_positionSync)
		{
			UpdateLastReadLocation(e);
			OnPositionChanged(e);
		}
	}

	private async void ListenerPositionError(object sender, LocationProviderError e)
	{
		await StopListeningAsync();
		OnPositionError(e);
	}

	#endregion

	#region Classes

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

	#endregion
}