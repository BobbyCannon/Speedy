namespace Speedy.Plugins.Devices.Location
{
	//public class DefaultLocationProvider : LocationProvider
	//{
	//	#region Fields

	//	private static int _instanceCount;
	//	private static readonly IGeolocator _locator;
	//	private static readonly object _lockObject;

	//	#endregion

	//	#region Constructors

	//	public DefaultLocationProvider(IDispatcher dispatcher) : base(dispatcher)
	//	{
	//		lock (_lockObject)
	//		{
	//			_locator.PositionChanged += LocatorOnPositionChanged;
	//		}
	//	}

	//	static DefaultLocationProvider()
	//	{
	//		_instanceCount = 0;
	//		_lockObject = new object();
	//		_locator = LocationProvider.Current;
	//		_locator.DesiredAccuracy = 10;
	//	}

	//	#endregion

	//	#region Properties

	//	/// <inheritdoc />
	//	public override LocationProviderType LocationProviderType => LocationProviderType.Default;

	//	#endregion

	//	#region Methods

	//	public override Task<IProviderLocation> RefreshLocationAsync()
	//	{
	//		return GetLocationAsync();
	//	}

	//	public override void StartMonitoring(TimeSpan minimumTime)
	//	{
	//		if (IsMonitoring)
	//		{
	//			return;
	//		}

	//		lock (_lockObject)
	//		{
	//			try
	//			{
	//				if ((_instanceCount == 0) && !_locator.IsListening)
	//				{
	//					var time = TimeSpan.FromSeconds(Math.Max(minimumTime.TotalSeconds, 1));

	//					IsMonitoring = _locator
	//						.StartListeningAsync(time, 0, true, new ListenerSettings
	//						{
	//							AllowBackgroundUpdates = true,
	//							PauseLocationUpdatesAutomatically = false,
	//							ListenForSignificantChanges = false,
	//							DeferLocationUpdates = false
	//						})
	//						.AwaitResults();
	//				}
	//				else
	//				{
	//					IsMonitoring = true;
	//				}

	//				_instanceCount++;

	//				if (LocationManager.SettingsManager.LogSettings.VerboseLocationProvider)
	//				{
	//					LocationManager.LogManager?.VerboseWrite("Default provider has started monitoring.");
	//				}
	//			}
	//			catch (Exception ex)
	//			{
	//				LocationManager.LogManager?.DebugWrite("Failed to start listening on the default location provider... " + ex.Message);

	//				IsMonitoring = false;

	//				StopMonitoring();
	//			}
	//		}
	//	}

	//	public override void StopMonitoring()
	//	{
	//		if (!IsMonitoring)
	//		{
	//			return;
	//		}

	//		lock (_lockObject)
	//		{
	//			IsMonitoring = false;

	//			_instanceCount--;

	//			if (LocationManager.SettingsManager.LogSettings.VerboseLocationProvider)
	//			{
	//				LocationManager.LogManager?.VerboseWrite($"Default provider has requested to stopped monitoring. Instance Count: {_instanceCount}");
	//			}

	//			if (_instanceCount > 0)
	//			{
	//				// we have more listener so just return
	//				return;
	//			}

	//			_locator.StopListeningAsync().AwaitResults();
	//			_instanceCount = 0; // in the event that we were somehow less than 0

	//			if (LocationManager.SettingsManager.LogSettings.VerboseLocationProvider)
	//			{
	//				LocationManager.LogManager?.VerboseWrite($"Default provider has stopped monitoring. Instance Count: {_instanceCount}");
	//			}
	//		}
	//	}

	//	/// <inheritdoc />
	//	protected override void Dispose(bool disposing)
	//	{
	//		if (!disposing)
	//		{
	//			return;
	//		}

	//		lock (_lockObject)
	//		{
	//			_locator.PositionChanged -= LocatorOnPositionChanged;
	//		}
	//	}

	//	[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
	//	private async Task<IProviderLocation> GetLocationAsync()
	//	{
	//		if (!_locator.IsGeolocationEnabled || !_locator.IsGeolocationAvailable)
	//		{
	//			return LastReadPosition;
	//		}

	//		try
	//		{
	//			var position = await _locator.GetPositionAsync(TimeSpan.FromSeconds(10), null, true);

	//			if ((position == null) || ((position.Latitude == 0) && (position.Longitude == 0)))
	//			{
	//				// Could not get a new location, so check if a cached location is available
	//				position = await _locator.GetLastKnownLocationAsync();
	//			}

	//			if ((position != null) && (position.Latitude != 0) && (position.Longitude != 0))
	//			{
	//				UpdateLastReadPosition(position);
	//			}
	//		}
	//		catch (Exception ex)
	//		{
	//			LocationManager.LogManager?.DebugWrite($"Unable to get location for default provider: {ex}");
	//		}

	//		return LastReadPosition;
	//	}

	//	private void LocatorOnPositionChanged(object sender, PositionEventArgs e)
	//	{
	//		if (!IsMonitoring)
	//		{
	//			return;
	//		}

	//		Status = e.ProviderLocation.StatusTime.ToString("MM/dd/yyyy hh:mm:ss tt");

	//		UpdateLastReadPosition(e.ProviderLocation);

	//		if (LocationManager.SettingsManager.LogSettings.VerboseLocationProvider)
	//		{
	//			LocationManager.LogManager?.VerboseWrite($"Default provider position has changed. {LastReadPosition}");
	//		}
	//	}

	//	private void UpdateLastReadPosition(IProviderLocation e)
	//	{
	//		LastReadPosition.UpdateWith(e);
	//		OnPositionChanged(LastReadPosition);
	//	}

	//	#endregion
	//}
}