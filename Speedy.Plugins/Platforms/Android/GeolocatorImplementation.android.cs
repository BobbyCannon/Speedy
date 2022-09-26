#region References

using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Java.Lang;
using Speedy.Devices.Location;
using Speedy.Plugins.Devices.Location;
using Address = Speedy.Plugins.Devices.Location.Address;
using Application = Android.App.Application;
using Debug = System.Diagnostics.Debug;
using Exception = System.Exception;
using Location = Android.Locations.Location;

#if NET6_0_ANDROID
#else
#endif

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Maui
{
	/// <summary>
	/// Implementation for Feature
	/// </summary>
	[Preserve(AllMembers = true)]
	public class GeolocatorImplementation : IGeolocator
	{
		#region Fields

		private IProviderLocation _lastPosition;
		private GeolocationContinuousListener _listener;
		private LocationManager _locationManager;
		private readonly object _positionSync;
		private GeolocationSingleListener _singleListener;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public GeolocatorImplementation()
		{
			_positionSync = new object();

			DesiredAccuracy = 10;
			ListeningProviders = new List<string>();
		}

		static GeolocatorImplementation()
		{
			ProvidersToUse = Array.Empty<string>();
			ProvidersToUseWhileListening = Array.Empty<string>();
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public double DesiredAccuracy { get; set; }

		/// <inheritdoc />
		public bool IsGeolocationAvailable => Providers.Length > 0;

		/// <inheritdoc />
		public bool IsGeolocationEnabled =>
			Providers.Any(p => !IgnoredProviders.Contains(p) &&
				Manager.IsProviderEnabled(p));

		/// <inheritdoc />
		public bool IsListening => _listener != null;

		/// <inheritdoc />
		public bool SupportsHeading => true;

		private string[] IgnoredProviders => new[] { LocationManager.PassiveProvider, "local_database" };

		private List<string> ListeningProviders { get; }

		private LocationManager Manager => _locationManager ??= (LocationManager) Application.Context.GetSystemService(Context.LocationService);

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
		/// Retrieve addresses for position.
		/// </summary>
		/// <param name="position"> Desired position (latitude and longitude) </param>
		/// <param name="mapKey"> The key for the map. </param>
		/// <returns> Addresses of the desired position </returns>
		public async Task<IEnumerable<Address>> GetAddressesForPositionAsync(IGeoLocation position, string mapKey = null)
		{
			if (position == null)
			{
				throw new ArgumentNullException(nameof(position));
			}

			using var geocoder = new Geocoder(Application.Context);
			var addressList = await geocoder.GetFromLocationAsync(position.Latitude, position.Longitude, 10);
			return addressList?.ToAddresses();
		}

		/// <summary>
		/// Gets the last known and most accurate location.
		/// This is usually cached and best to display first before querying for full position.
		/// </summary>
		/// <returns> Best and most recent location or null if none found </returns>
		public async Task<IProviderLocation> GetLastKnownLocationAsync()
		{
			var hasPermission = await CheckWhenInUsePermission();
			if (!hasPermission)
			{
				throw new GeolocationException(GeolocationError.Unauthorized);
			}

			Location bestLocation = null;
			foreach (var provider in Providers)
			{
				var location = Manager.GetLastKnownLocation(provider);
				if ((location != null) && GeolocationUtils.IsBetterLocation(location, bestLocation))
				{
					bestLocation = location;
				}
			}

			return bestLocation?.ToPosition();
		}

		/// <summary>
		/// Gets position async with specified parameters
		/// </summary>
		/// <param name="timeout"> Timeout to wait, Default Infinite </param>
		/// <param name="cancelToken"> Cancellation token </param>
		/// <param name="includeHeading"> If you would like to include heading </param>
		/// <returns> ProviderLocation </returns>
		public async Task<IProviderLocation> GetPositionAsync(TimeSpan? timeout, CancellationToken? cancelToken = null, bool includeHeading = false)
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
				throw new GeolocationException(GeolocationError.Unauthorized);
			}

			var tcs = new TaskCompletionSource<IProviderLocation>();

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

				_singleListener = new GeolocationSingleListener(Manager,
					(float) DesiredAccuracy,
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

						tcs.SetException(new GeolocationException(GeolocationError.PositionUnavailable));
						return await tcs.Task;
					}
				}
				catch (SecurityException ex)
				{
					tcs.SetException(new GeolocationException(GeolocationError.Unauthorized, ex));
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

					EventHandler<PositionEventArgs> gotPosition = null;

					gotPosition = (_, e) =>
					{
						tcs.TrySetResult(e.ProviderLocation);
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

		/// <summary>
		/// Retrieve positions for address.
		/// </summary>
		/// <param name="address"> Desired address </param>
		/// <param name="mapKey"> Map Key required only on UWP </param>
		/// <returns> Positions of the desired address </returns>
		public async Task<IEnumerable<IProviderLocation>> GetPositionsForAddressAsync(string address, string mapKey = null)
		{
			if (address == null)
			{
				throw new ArgumentNullException(nameof(address));
			}

			using var geocoder = new Geocoder(Application.Context);
			var addressList = await geocoder.GetFromLocationNameAsync(address, 10);
			if (addressList != null)
			{
				return addressList.Select(p => new ProviderLocation
				{
					Latitude = p.Latitude,
					Longitude = p.Longitude
				});
			}

			return Array.Empty<ProviderLocation>();
		}

		/// <summary>
		/// Start listening for changes
		/// </summary>
		/// <param name="minimumTime"> Time </param>
		/// <param name="minimumDistance"> Distance </param>
		/// <param name="includeHeading"> Include heading or not </param>
		/// <param name="listenerSettings"> Optional settings (iOS only) </param>
		public async Task<bool> StartListeningAsync(TimeSpan minimumTime, double minimumDistance, bool includeHeading = false, ListenerSettings listenerSettings = null)
		{
			bool hasPermission;

			if (listenerSettings?.RequireLocationAlwaysPermission ?? false)
			{
				hasPermission = await CheckAlwaysPermissions();
			}
			else
			{
				hasPermission = await CheckWhenInUsePermission();
			}

			if (!hasPermission)
			{
				throw new GeolocationException(GeolocationError.Unauthorized);
			}

			var minTimeMilliseconds = minimumTime.TotalMilliseconds;
			if (minTimeMilliseconds < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(minimumTime));
			}
			if (minimumDistance < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(minimumDistance));
			}
			if (IsListening)
			{
				throw new InvalidOperationException("This Geolocator is already listening");
			}

			var providers = Providers;
			_listener = new GeolocationContinuousListener(Manager, minimumTime, providers);
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
				Manager.RequestLocationUpdates(provider, (long) minTimeMilliseconds, (float) minimumDistance, _listener, looper);
			}

			return true;
		}

		/// <inheritdoc />
		public Task<bool> StopListeningAsync()
		{
			if (_listener == null)
			{
				return Task.FromResult(true);
			}

			if (ListeningProviders == null)
			{
				return Task.FromResult(true);
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
			return Task.FromResult(true);
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

		private void OnListenerPositionChanged(object sender, PositionEventArgs e)
		{
			// Ignore anything that might come in after stop listening
			if (!IsListening)
			{
				return;
			}

			lock (_positionSync)
			{
				_lastPosition = e.ProviderLocation;

				PositionChanged?.Invoke(this, e);
			}
		}

		private async void OnListenerPositionError(object sender, PositionErrorEventArgs e)
		{
			await StopListeningAsync();

			PositionError?.Invoke(this, e);
		}

		#endregion

		#region Events

		/// <inheritdoc />
		public event EventHandler<PositionEventArgs> PositionChanged;

		/// <inheritdoc />
		public event EventHandler<PositionErrorEventArgs> PositionError;

		#endregion
	}
}