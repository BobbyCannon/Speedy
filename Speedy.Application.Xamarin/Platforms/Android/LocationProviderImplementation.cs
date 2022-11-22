#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Java.Lang;
using Speedy.Application.Internal;
using Speedy.Devices;
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
public class LocationProviderImplementation<TLocation, THorizontal, TVertical, TLocationProviderSettings>
	: LocationProvider<TLocation, THorizontal, TVertical, TLocationProviderSettings>
	where TLocation : class, ILocation<THorizontal, TVertical>, new()
	where THorizontal : class, IHorizontalLocation, IUpdatable<THorizontal>
	where TVertical : class, IVerticalLocation, IUpdatable<TVertical>
	where TLocationProviderSettings : ILocationProviderSettings, IBindable, new()
{
	#region Fields

	private FusedLocationProviderCallback _fusedCallback;

	private FusedLocationProviderClient _fusedListener;

	private GeolocationContinuousListener<TLocation, THorizontal, TVertical> _listener;
	private LocationManager _locationManager;
	private readonly object _positionSync;
	private GeolocationSingleListener<TLocation, THorizontal, TVertical> _singleListener;
	private IDictionary<string, SourceInformationProvider> _sourceProviders;
	private bool _usingGooglePlayFused;

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
	public bool IsLocationAvailable => SourceProviders.Any();

	/// <inheritdoc />
	public bool IsLocationEnabled => SourceProviders.Any(x => x.IsEnabled && Manager.IsProviderEnabled(x.ProviderName));

	/// <inheritdoc />
	public override string ProviderName => "Xamarin Android";

	/// <inheritdoc />
	public override IEnumerable<IInformationProvider> SourceProviders
	{
		get
		{
			if (_sourceProviders == null)
			{
				var defaultEnabled = new[] { LocationManager.GpsProvider };
				_sourceProviders = Manager
					.GetProviders(false)
					.Where(x =>
						x != LocationManager.PassiveProvider
						&& x != LocationManager.FusedProvider
					)
					.Select(x => new SourceInformationProvider
					{
						ProviderName = x,
						IsEnabled = defaultEnabled.Contains(x)
					})
					.ToDictionary(x => x.ProviderName, x => x);

				if (_sourceProviders.All(x => x.Key != LocationManager.FusedProvider))
				{
					var fusedSource = new SourceInformationProvider
					{
						IsEnabled = true,
						ProviderName = LocationManager.FusedProvider
					};
					_sourceProviders.Add(fusedSource.ProviderName, fusedSource);
					_usingGooglePlayFused = true;
				}

				OnPropertyChanged(nameof(HasSourceProviders));
			}

			return _sourceProviders.Values;
		}
	}

	/// <summary>
	/// True if the location provider has permission to be accessed.
	/// </summary>
	protected bool HasPermission { get; private set; }

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
		var providerSources = SourceProviders.ToArray();

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
				SourceProviders
					.Where(x => x.IsEnabled)
					.Where(x => Manager.IsProviderEnabled(x.ProviderName))
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
					if (Manager.IsProviderEnabled(providerSources[i].ProviderName))
					{
						enabled++;
					}

					Manager.RequestLocationUpdates(providerSources[i].ProviderName, 0, 0, _singleListener, looper);
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

		if (!IsGooglePlayServicesInstalled())
		{
			ListenerPositionError(this, LocationProviderError.MissingDependency);
			return Task.CompletedTask;
		}

		LocationProviderSettings.Cleanup();

		HasPermission = LocationProviderSettings.RequireLocationAlwaysPermission
			? CheckAlwaysPermissions()
			: CheckWhenInUsePermission();

		if (!HasPermission)
		{
			ListenerPositionError(this, LocationProviderError.Unauthorized);
			return Task.CompletedTask;
		}

		var sources = SourceProviders.Cast<SourceInformationProvider>().ToArray();
		var looper = Looper.MyLooper() ?? Looper.MainLooper;

		if ((XamarinPlatform.MainActivity != null)
			&& _usingGooglePlayFused
			&& _sourceProviders[LocationManager.FusedProvider].IsEnabled)
		{
			_fusedListener = LocationServices.GetFusedLocationProviderClient(XamarinPlatform.MainActivity);
			var locationRequest = LocationRequest.Create();
			locationRequest.SetPriority(Priority.PriorityHighAccuracy);
			locationRequest.SetInterval((long) LocationProviderSettings.MinimumTime.TotalMilliseconds);
			locationRequest.SetFastestInterval((long) LocationProviderSettings.MinimumTime.TotalMilliseconds);
			locationRequest.SetSmallestDisplacement(LocationProviderSettings.MinimumDistance);
			_fusedCallback = new FusedLocationProviderCallback(FusedLocationProviderLocationChanged);
			_fusedListener.RequestLocationUpdates(locationRequest, _fusedCallback, looper);
			_sourceProviders[LocationManager.FusedProvider].IsMonitoring = true;
		}

		_listener = new GeolocationContinuousListener<TLocation, THorizontal, TVertical>(Dispatcher, ProviderName, Manager, sources);
		_listener.LogEventWritten += ListenerOnLogEventWritten;
		_listener.PositionChanged += ListenerPositionChanged;
		_listener.PositionError += ListenerPositionError;

		for (var i = 0; i < sources.Length; ++i)
		{
			var source = sources[i];

			if ((source.ProviderName == LocationManager.FusedProvider) && _usingGooglePlayFused)
			{
				// This provider is handled above, differently
				continue;
			}

			if (!source.IsEnabled)
			{
				source.IsEnabled = false;
				continue;
			}

			Manager.RequestLocationUpdates(source.ProviderName,
				(long) LocationProviderSettings.MinimumTime.TotalMilliseconds,
				LocationProviderSettings.MinimumDistance,
				_listener,
				looper);

			source.IsMonitoring = true;
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

		if (_fusedListener != null)
		{
			_sourceProviders[LocationManager.FusedProvider].IsMonitoring = false;
			_fusedListener.RemoveLocationUpdates(_fusedCallback);

			_fusedListener.Dispose();
			_fusedCallback.Dispose();

			_fusedListener = null;
			_fusedCallback = null;
		}

		try
		{
			Manager.RemoveUpdates(_listener);
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Unable to remove updates: {ex}");
		}

		SourceProviders.Cast<SourceInformationProvider>().ForEach(x => x.IsMonitoring = false);
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

	private void FusedLocationProviderLocationChanged(Location obj)
	{
		var location = obj.ToPosition<TLocation, THorizontal, TVertical>(ProviderName);
		UpdateCurrentValue(location);
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