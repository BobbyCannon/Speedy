#region References

using Android.Locations;
using Android.OS;
using Android.Runtime;
using Speedy.Plugins.Devices.Location;
using Location = Android.Locations.Location;
using Object = Java.Lang.Object;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Maui
{
	[Preserve(AllMembers = true)]
	internal class GeolocationContinuousListener
		: Object, ILocationListener
	{
		#region Fields

		private string activeProvider;
		private readonly HashSet<string> activeProviders = new HashSet<string>();
		private Location lastLocation;
		private readonly LocationManager manager;
		private IList<string> providers;
		private readonly TimeSpan timePeriod;

		#endregion

		#region Constructors

		public GeolocationContinuousListener(LocationManager manager, TimeSpan timePeriod, IList<string> providers)
		{
			this.manager = manager;
			this.timePeriod = timePeriod;
			this.providers = providers;

			foreach (var p in providers)
			{
				if (manager.IsProviderEnabled(p))
				{
					activeProviders.Add(p);
				}
			}
		}

		#endregion

		#region Methods

		public void OnLocationChanged(Location location)
		{
			if (location.Provider != activeProvider)
			{
				if ((activeProvider != null) && manager.IsProviderEnabled(activeProvider))
				{
					var pr = manager.GetProvider(location.Provider);
					var lapsed = GetTimeSpan(location.Time) - GetTimeSpan(lastLocation.Time);

					if ((pr.Accuracy > manager.GetProvider(activeProvider).Accuracy)
						&& (lapsed < timePeriod.Add(timePeriod)))
					{
						location.Dispose();
						return;
					}
				}

				activeProvider = location.Provider;
			}

			var previous = Interlocked.Exchange(ref lastLocation, location);
			if (previous != null)
			{
				previous.Dispose();
			}

			PositionChanged?.Invoke(this, new PositionEventArgs(location.ToPosition()));
		}

		public void OnProviderDisabled(string provider)
		{
			if (provider == LocationManager.PassiveProvider)
			{
				return;
			}

			lock (activeProviders)
			{
				if (activeProviders.Remove(provider) && (activeProviders.Count == 0))
				{
					OnPositionError(new PositionErrorEventArgs(GeolocationError.PositionUnavailable));
				}
			}
		}

		public void OnProviderEnabled(string provider)
		{
			if (provider == LocationManager.PassiveProvider)
			{
				return;
			}

			lock (activeProviders)
			{
				activeProviders.Add(provider);
			}
		}

		public void OnStatusChanged(string provider, Availability status, Bundle extras)
		{
			switch (status)
			{
				case Availability.Available:
					OnProviderEnabled(provider);
					break;

				case Availability.OutOfService:
					OnProviderDisabled(provider);
					break;
			}
		}

		private TimeSpan GetTimeSpan(long time)
		{
			return new TimeSpan(TimeSpan.TicksPerMillisecond * time);
		}

		private void OnPositionError(PositionErrorEventArgs e)
		{
			PositionError?.Invoke(this, e);
		}

		#endregion

		#region Events

		public event EventHandler<PositionEventArgs> PositionChanged;

		public event EventHandler<PositionErrorEventArgs> PositionError;

		#endregion
	}
}