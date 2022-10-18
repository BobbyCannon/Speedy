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
using Speedy.Devices.Location;
using Xamarin.Essentials;
using Debug = System.Diagnostics.Debug;
using Exception = System.Exception;
using Location = Android.Locations.Location;

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
	#region Fields

	private T _lastPosition;
	private GeolocationContinuousListener<T> _listener;
	private LocationManager _locationManager;
	private readonly object _positionSync;
	private GeolocationSingleListener<T> _singleListener;

	#endregion

	#region Constructors

	/// <summary>
	/// Default constructor
	/// </summary>
	public LocationProviderImplementation(IDispatcher dispatcher) : base(dispatcher)
	{
		_positionSync = new object();

		ListeningProviders = new List<string>();
	}

	static LocationProviderImplementation()
	{
		ProvidersToUse = Array.Empty<string>();
		ProvidersToUseWhileListening = Array.Empty<string>();
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public override bool IsListening => _listener != null;

	/// <inheritdoc />
	public override bool IsLocationAvailable => Providers.Length > 0;

	/// <inheritdoc />
	public override bool IsLocationEnabled =>
		Providers.Any(p => !IgnoredProviders.Contains(p) &&
			Manager.IsProviderEnabled(p));

	private string[] IgnoredProviders => new[] { LocationManager.PassiveProvider, "local_database" };

	private List<string> ListeningProviders { get; }

	private LocationManager Manager => _locationManager ??= (LocationManager) Android.App.Application.Context.GetSystemService(Context.LocationService);

	private string[] Providers => Manager.GetProviders(false).ToArray();

	/// <summary>
	/// Gets or sets the location manager providers to ignore when getting position.
	/// </summary>
	private static string[] ProvidersToUse { get; }

	/// <summary>
	/// Gets or sets the location manager providers to ignore when doing continuous listening.
	/// </summary>
	private static string[] ProvidersToUseWhileListening { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Gets position async with specified parameters
	/// </summary>
	/// <param name="timeout"> Timeout to wait, Default Infinite </param>
	/// <param name="cancelToken"> Cancellation token </param>
	/// <returns> ProviderLocation </returns>
	public override async Task<T> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null)
	{
		var timeoutMilliseconds = timeout.HasValue ? (int) timeout.Value.TotalMilliseconds : Timeout.Infinite;
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

		if (!IsListening)
		{
			var providers = new List<string>();
			if ((ProvidersToUse == null) || (ProvidersToUse.Length == 0))
			{
				providers.AddRange(Providers);
			}
			else
			{
				// Only add providers requested.
				foreach (var provider in Providers)
				{
					if ((ProvidersToUse == null) || !ProvidersToUse.Contains(provider))
					{
						continue;
					}

					providers.Add(provider);
				}
			}

			void singleListenerFinishCallback()
			{
				if (_singleListener == null)
				{
					return;
				}

				for (var i = 0; i < providers.Count; ++i)
				{
					Manager.RemoveUpdates(_singleListener);
				}
			}

			_singleListener = new GeolocationSingleListener<T>(Manager,
				(float) LocationProviderSettings.DesiredAccuracy,
				timeoutMilliseconds,
				providers.Where(Manager.IsProviderEnabled),
				singleListenerFinishCallback);

			if (cancelToken != CancellationToken.None)
			{
				cancelToken.Value.Register(() =>
				{
					_singleListener.Cancel();

					for (var i = 0; i < providers.Count; ++i)
					{
						Manager.RemoveUpdates(_singleListener);
					}
				}, true);
			}

			try
			{
				var looper = Looper.MyLooper() ?? Looper.MainLooper;
				var enabled = 0;

				for (var i = 0; i < providers.Count; ++i)
				{
					if (Manager.IsProviderEnabled(providers[i]))
					{
						enabled++;
					}

					Manager.RequestLocationUpdates(providers[i], 0, 0, _singleListener, looper);
				}

				if (enabled == 0)
				{
					for (var i = 0; i < providers.Count; ++i)
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
			if (_lastPosition == null)
			{
				if (cancelToken != CancellationToken.None)
				{
					cancelToken.Value.Register(() => tcs.TrySetCanceled());
				}

				EventHandler<T> gotPosition = null;

				gotPosition = (_, e) =>
				{
					tcs.TrySetResult(e);
					PositionChanged -= gotPosition;
				};

				PositionChanged += gotPosition;
			}
			else
			{
				tcs.SetResult(_lastPosition);
			}
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

		bool hasPermission;

		if (LocationProviderSettings.RequireLocationAlwaysPermission)
		{
			hasPermission = await CheckAlwaysPermissions();
		}
		else
		{
			hasPermission = await CheckWhenInUsePermission();
		}

		if (!hasPermission)
		{
			return;
		}

		var providers = Providers;
		_listener = new GeolocationContinuousListener<T>(Manager, LocationProviderSettings.MinimumTime, providers);
		_listener.PositionChanged += OnListenerPositionChanged;
		_listener.PositionError += OnListenerPositionError;

		var looper = Looper.MyLooper() ?? Looper.MainLooper;
		ListeningProviders.Clear();

		for (var i = 0; i < providers.Length; ++i)
		{
			var provider = providers[i];

			// we have limited set of providers
			if (ProvidersToUseWhileListening is { Length: > 0 })
			{
				//the provider is not in the list, so don't use it.
				if (!ProvidersToUseWhileListening.Contains(provider))
				{
					continue;
				}
			}

			ListeningProviders.Add(provider);
			Manager.RequestLocationUpdates(provider,
				(long) LocationProviderSettings.MinimumTime.TotalMilliseconds,
				(float) LocationProviderSettings.MinimumDistance,
				_listener,
				looper);
		}
	}

	/// <inheritdoc />
	public override Task StopListeningAsync()
	{
		if (_listener == null)
		{
			return Task.CompletedTask;
		}

		if (ListeningProviders == null)
		{
			return Task.CompletedTask;
		}

		var providers = ListeningProviders;
		_listener.PositionChanged -= OnListenerPositionChanged;
		_listener.PositionError -= OnListenerPositionError;

		for (var i = 0; i < providers.Count; i++)
		{
			try
			{
				Manager.RemoveUpdates(_listener);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to remove updates: " + ex);
			}
		}

		_listener = null;
		return Task.CompletedTask;
	}

	private async Task<bool> CheckAlwaysPermissions()
	{
		var status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
		if (status == PermissionStatus.Granted)
		{
			return true;
		}

		Console.WriteLine("Currently does not have Location permissions, requesting permissions");

		status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();

		if (status == PermissionStatus.Granted)
		{
			return true;
		}

		Console.WriteLine("Location permission denied, can not get positions async.");
		return false;
	}

	private async Task<bool> CheckWhenInUsePermission()
	{
		var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
		if (status == PermissionStatus.Granted)
		{
			return true;
		}

		Console.WriteLine("Currently does not have Location permissions, requesting permissions");

		status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

		if (status == PermissionStatus.Granted)
		{
			return true;
		}

		Console.WriteLine("Location permission denied, can not get positions async.");
		return false;
	}

	private void OnListenerPositionChanged(object sender, T e)
	{
		// Ignore anything that might come in after stop listening
		if (!IsListening)
		{
			return;
		}

		lock (_positionSync)
		{
			_lastPosition = e;

			OnPositionChanged(e);
		}
	}

	private async void OnListenerPositionError(object sender, LocationProviderError e)
	{
		await StopListeningAsync();

		OnPositionError(e);
	}

	#endregion
}