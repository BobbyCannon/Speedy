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
	internal class GeolocationSingleListener
		: Object, ILocationListener
	{
		#region Fields

		private readonly HashSet<string> activeProviders = new HashSet<string>();
		private Location bestLocation;
		private readonly TaskCompletionSource<IProviderLocation> completionSource = new TaskCompletionSource<IProviderLocation>();
		private readonly float desiredAccuracy;

		private readonly Action finishedCallback;

		private readonly object locationSync = new object();
		private readonly Timer timer;

		#endregion

		#region Constructors

		public GeolocationSingleListener(LocationManager manager, float desiredAccuracy, int timeout, IEnumerable<string> activeProviders, Action finishedCallback)
		{
			this.desiredAccuracy = desiredAccuracy;
			this.finishedCallback = finishedCallback;

			this.activeProviders = new HashSet<string>(activeProviders);

			foreach (var provider in activeProviders)
			{
				var location = manager.GetLastKnownLocation(provider);
				if ((location != null) && GeolocationUtils.IsBetterLocation(location, bestLocation))
				{
					bestLocation = location;
				}
			}

			if (timeout != Timeout.Infinite)
			{
				timer = new Timer(TimesUp, null, timeout, 0);
			}
		}

		#endregion

		#region Properties

		public Task<IProviderLocation> Task => completionSource.Task;

		#endregion

		#region Methods

		public void Cancel()
		{
			completionSource.TrySetCanceled();
		}

		public void OnLocationChanged(Location location)
		{
			if (location.Accuracy <= desiredAccuracy)
			{
				Finish(location);
				return;
			}

			lock (locationSync)
			{
				if (GeolocationUtils.IsBetterLocation(location, bestLocation))
				{
					bestLocation = location;
				}
			}
		}

		public void OnProviderDisabled(string provider)
		{
			lock (activeProviders)
			{
				if (activeProviders.Remove(provider) && (activeProviders.Count == 0))
				{
					completionSource.TrySetException(new GeolocationException(GeolocationError.PositionUnavailable));
				}
			}
		}

		public void OnProviderEnabled(string provider)
		{
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

		private void Finish(Location location)
		{
			finishedCallback?.Invoke();
			completionSource.TrySetResult(location.ToPosition());
		}

		private void TimesUp(object state)
		{
			lock (locationSync)
			{
				if (bestLocation == null)
				{
					if (completionSource.TrySetCanceled())
					{
						finishedCallback?.Invoke();
					}
				}
				else
				{
					Finish(bestLocation);
				}
			}
		}

		#endregion
	}
}