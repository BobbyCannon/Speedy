#region References

using System;
using Android.Locations;
using Speedy.Devices.Location;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Xamarin
{
	public static class LocationProviderUtilities
	{
		#region Fields

		private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private static readonly int twoMinutes = 120000;

		#endregion

		#region Methods

		internal static bool IsBetterLocation(Location location, Location bestLocation)
		{
			if (bestLocation == null)
			{
				return true;
			}

			var timeDelta = location.Time - bestLocation.Time;
			var isSignificantlyNewer = timeDelta > twoMinutes;
			var isSignificantlyOlder = timeDelta < -twoMinutes;
			var isNewer = timeDelta > 0;

			if (isSignificantlyNewer)
			{
				return true;
			}

			if (isSignificantlyOlder)
			{
				return false;
			}

			var accuracyDelta = (int) (location.Accuracy - bestLocation.Accuracy);
			var isLessAccurate = accuracyDelta > 0;
			var isMoreAccurate = accuracyDelta < 0;
			var isSignificantlyLessAccurate = accuracyDelta > 200;

			var isFromSameProvider = IsSameProvider(location.Provider, bestLocation.Provider);

			if (isMoreAccurate)
			{
				return true;
			}

			if (isNewer && !isLessAccurate)
			{
				return true;
			}

			if (isNewer && !isSignificantlyLessAccurate && isFromSameProvider)
			{
				return true;
			}

			return false;
		}

		internal static bool IsSameProvider(string provider1, string provider2)
		{
			if (provider1 == null)
			{
				return provider2 == null;
			}

			return provider1.Equals(provider2);
		}

		internal static IProviderLocation ToPosition(this Location location)
		{
			var response = new ProviderLocation
			{
				HasHeading = location.HasBearing,
				HasSpeed = location.HasSpeed,
				Longitude = location.Longitude,
				Latitude = location.Latitude,
				StatusTime = location.GetTimestamp().UtcDateTime,
				SourceName = location.Provider
			};

			if (location.HasAccuracy)
			{
				response.Accuracy = location.Accuracy;
				response.AccuracyReference = AccuracyReferenceType.Meters;
			}

			if (location.HasAltitude)
			{
				response.Altitude = location.Altitude;
				response.AltitudeReference = AltitudeReferenceType.Ellipsoid;
			}

			if (location.HasVerticalAccuracy)
			{
				response.AltitudeAccuracy = location.VerticalAccuracyMeters;
				response.AltitudeAccuracyReference = AccuracyReferenceType.Meters;
			}

			if (response.HasHeading)
			{
				response.Heading = location.Bearing;
			}

			if (response.HasSpeed)
			{
				response.Speed = location.Speed;
			}

			response.HasLatitudeLongitude = true;
			response.Longitude = location.Longitude;
			response.Latitude = location.Latitude;
			response.StatusTime = location.GetTimestamp().UtcDateTime;
			response.SourceName = location.Provider;
			//response.IsFromMockProvider = (int) Build.VERSION.SdkInt >= 18 && location.IsFromMockProvider;

			return response;
		}

		private static DateTimeOffset GetTimestamp(this Location location)
		{
			try
			{
				return new DateTimeOffset(epoch.AddMilliseconds(location.Time));
			}
			catch (Exception)
			{
				return new DateTimeOffset(epoch);
			}
		}

		#endregion
	}
}