#region References

using System;
using Speedy.Devices.Location;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Xamarin;

/// <summary>
/// Extensions for the Android platform.
/// </summary>
public static class PlatformExtensions
{
	#region Fields

	private static readonly DateTime _epoch;
	private static readonly int _twoMinutes;

	#endregion

	#region Constructors

	static PlatformExtensions()
	{
		_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		_twoMinutes = 120000;
	}

	#endregion

	#region Methods

	internal static DateTimeOffset GetTimestamp(this Android.Locations.Location location)
	{
		try
		{
			return new DateTimeOffset(_epoch.AddMilliseconds(location.Time));
		}
		catch (Exception)
		{
			return new DateTimeOffset(_epoch);
		}
	}

	internal static bool IsBetterLocation(this Android.Locations.Location location, Android.Locations.Location bestLocation)
	{
		if (bestLocation == null)
		{
			return true;
		}

		var timeDelta = location.Time - bestLocation.Time;
		var isSignificantlyNewer = timeDelta > _twoMinutes;
		var isSignificantlyOlder = timeDelta < -_twoMinutes;
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

	internal static bool IsSameProvider(this string provider1, string provider2)
	{
		if (provider1 == null)
		{
			return provider2 == null;
		}

		return provider1.Equals(provider2);
	}

	internal static T ToPosition<T>(this Android.Locations.Location location) where T : class, ILocation, new()
	{
		var sourceName = location.Provider ?? "unknown";
		var sourceTime = location.GetTimestamp().UtcDateTime;

		var response = new T
		{
			HasHeading = location.HasBearing,
			HasSpeed = location.HasSpeed,
			HorizontalSourceName = sourceName,
			HorizontalStatusTime = sourceTime,
			Longitude = location.Longitude,
			Latitude = location.Latitude,
			ProviderName = "Xamarin Android",
			VerticalSourceName = sourceName,
			VerticalStatusTime = sourceTime
		};

		if (location.HasAccuracy)
		{
			response.HorizontalAccuracy = location.Accuracy;
			response.HorizontalAccuracyReference = AccuracyReferenceType.Meters;
		}

		if (location.HasAltitude)
		{
			response.Altitude = location.Altitude;
			response.AltitudeReference = AltitudeReferenceType.Ellipsoid;
		}

		if (location.HasVerticalAccuracy)
		{
			response.VerticalAccuracy = location.VerticalAccuracyMeters;
			response.VerticalAccuracyReference = AccuracyReferenceType.Meters;
		}

		if (response.HasHeading)
		{
			response.Heading = location.Bearing;
		}

		if (response.HasSpeed)
		{
			response.Speed = location.Speed;
		}

		//response.IsFromMockProvider = (int) Build.VERSION.SdkInt >= 18 && location.IsFromMockProvider;

		return response;
	}

	#endregion
}