#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Devices.Location;
using static System.Math;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for location related code.
/// </summary>
public static class LocationExtensions
{
	#region Methods

	/// <summary>
	/// Calculate the degrees per meter.
	/// </summary>
	/// <param name="latitude"> The latitude to calculate for. </param>
	/// <returns> Returns degrees per latitude and per longitude. </returns>
	public static IBasicLocation CalculateDegreesPerMeter(double latitude)
	{
		var values = CalculateMetersPerDegrees(latitude);
		return new BasicLocation(1 / values.Latitude, 1 / values.Longitude);
	}

	/// <summary>
	/// Calculate the degrees per meter.
	/// </summary>
	/// <param name="location"> The latitude to calculate for. </param>
	/// <returns> Returns degrees per latitude and per longitude. </returns>
	public static IBasicLocation CalculateDegreesPerMeter(this IBasicLocation location)
	{
		var values = location.CalculateMetersPerDegrees();
		return new BasicLocation(1 / values.Latitude, 1 / values.Longitude);
	}

	/// <summary>
	/// Calculate the heading from one location to another.
	/// </summary>
	/// <param name="latitudeStart"> Latitude start. </param>
	/// <param name="longitudeStart"> Longitude start. </param>
	/// <param name="latitudeEnd"> Latitude end. </param>
	/// <param name="longitudeEnd"> Longitude end. </param>
	/// <returns> The heading. </returns>
	public static double CalculateHeading(double latitudeStart, double longitudeStart, double latitudeEnd, double longitudeEnd)
	{
		var dLon = ToRadians(longitudeEnd - longitudeStart);
		var dPhi = Log(Tan((ToRadians(latitudeEnd) / 2) + (PI / 4)) / Tan((ToRadians(latitudeStart) / 2) + (PI / 4)));
		if (Abs(dLon) > PI)
		{
			dLon = dLon > 0 ? -((2 * PI) - dLon) : (2 * PI) + dLon;
		}
		return ToHeading(Atan2(dLon, dPhi));
	}

	/// <summary>
	/// Calculate the heading from one location to another.
	/// </summary>
	/// <param name="start"> The starting location. </param>
	/// <param name="end"> The ending location. </param>
	/// <returns> The heading. </returns>
	public static double CalculateHeading(this IBasicLocation start, IBasicLocation end)
	{
		return CalculateHeading(start.Latitude, start.Longitude, end.Latitude, end.Longitude);
	}

	/// <summary>
	/// Calculate the meters per degree.
	/// </summary>
	/// <param name="latitude"> The location to calculate for. </param>
	/// <returns> Returns meters for both latitude and longitude. </returns>
	public static IBasicLocation CalculateMetersPerDegrees(double latitude)
	{
		var rLat = latitude * (PI / 180.0);
		var metersPerDegreeLatitude = 111131.77741377673104 / Pow(1 + (0.0033584313098335197297 * Cos(2 * rLat)), 3d / 2);
		var metersPerDegreeLongitude = (111506.26354049367285 * Cos(rLat)) / Pow(1 + (0.0033584313098335197297 * Cos(2 * rLat)), 1d / 2);
		return new BasicLocation(metersPerDegreeLatitude, metersPerDegreeLongitude);
	}

	/// <summary>
	/// Calculate the meters per degree.
	/// </summary>
	/// <param name="location"> The location to calculate for. </param>
	/// <returns> Returns meters for both latitude and longitude. </returns>
	public static IBasicLocation CalculateMetersPerDegrees(this IBasicLocation location)
	{
		var rLat = location.Latitude * (PI / 180.0);
		var metersPerDegreeLatitude = 111131.77741377673104 / Pow(1 + (0.0033584313098335197297 * Cos(2 * rLat)), 3d / 2);
		var metersPerDegreeLongitude = (111506.26354049367285 * Cos(rLat)) / Pow(1 + (0.0033584313098335197297 * Cos(2 * rLat)), 1d / 2);
		return new BasicLocation(metersPerDegreeLatitude, metersPerDegreeLongitude);
	}

	/// <summary>
	/// Calculates the distance between two locations in meters.
	/// </summary>
	/// <param name="latitudeStart"> Latitude start. </param>
	/// <param name="longitudeStart"> Longitude start. </param>
	/// <param name="latitudeEnd"> Latitude end. </param>
	/// <param name="longitudeEnd"> Longitude end. </param>
	/// <returns> The distance in meters. </returns>
	public static double DistanceBetween(double latitudeStart, double longitudeStart, double latitudeEnd, double longitudeEnd)
	{
		const double earthRadius = 6378137.0;

		var dLon = ToRadians(longitudeEnd - longitudeStart);
		var theta1 = ToRadians(latitudeStart);
		var theta2 = ToRadians(latitudeEnd);
		var s1 = Sin(theta2 - theta1);
		var s2 = Sin(dLon / 2.0);
		var a = (s1 * s1) + (Cos(theta1) * Cos(theta2) * s2 * s2);

		var distanceInMeters = earthRadius * 2 * Atan2(Sqrt(a), Sqrt(1.0 - a));
		return distanceInMeters;
	}

	/// <summary>
	/// Calculates the distance between two locations in meters.
	/// </summary>
	/// <param name="latitudeStart"> Latitude start. </param>
	/// <param name="longitudeStart"> Longitude start. </param>
	/// <param name="altitudeStart"> Altitude start. </param>
	/// <param name="latitudeEnd"> Latitude end. </param>
	/// <param name="longitudeEnd"> Longitude end. </param>
	/// <param name="altitudeEnd"> Altitude end. </param>
	/// <returns> The distance in meters. </returns>
	public static double DistanceBetween(double latitudeStart, double longitudeStart, double altitudeStart, double latitudeEnd, double longitudeEnd, double altitudeEnd)
	{
		const double earthRadius = 6378137.0;

		var dLon = ToRadians(longitudeEnd - longitudeStart);
		var theta1 = ToRadians(latitudeStart);
		var theta2 = ToRadians(latitudeEnd);
		var s1 = Sin(theta2 - theta1);
		var s2 = Sin(dLon / 2.0);
		var a = (s1 * s1) + (Cos(theta1) * Cos(theta2) * s2 * s2);

		// Lateral distance:
		s2 = earthRadius * 2 * Atan2(Sqrt(a), Sqrt(1.0 - a));

		// Vertical distance (ignoring distortion - it'll be minor as the points are very close in this use case):
		s1 = altitudeEnd - altitudeStart;

		// Sqrt for the distance:
		s1 = Sqrt((s2 * s2) + (s1 * s1));

		return s1;
	}

	/// <summary>
	/// Calculates the distance between two locations in meters.
	/// </summary>
	/// <param name="locationStart"> The starting location. </param>
	/// <param name="locationEnd"> The ending location. </param>
	/// <returns> The distance in meters. </returns>
	public static double DistanceBetween(this IBasicLocation locationStart, IBasicLocation locationEnd)
	{
		return DistanceBetween(locationStart.Latitude, locationStart.Longitude, locationEnd.Latitude, locationEnd.Longitude);
	}

	/// <summary>
	/// Gets the center of a set of locations.
	/// </summary>
	/// <param name="locations"> The locations to get the center for. </param>
	/// <returns> The center of the locations. </returns>
	public static IBasicLocation GetCenter(this IEnumerable<IBasicLocation> locations)
	{
		var totalX = 0.0;
		var totalY = 0.0;
		var locationList = locations.ToList();

		foreach (var location in locationList)
		{
			totalX += location.Latitude;
			totalY += location.Longitude;
		}

		return new BasicLocation(totalX / locationList.Count, totalY / locationList.Count);
	}

	/// <summary>
	/// Gets the radius of a set of locations.
	/// </summary>
	/// <param name="locations"> The locations to get the center for. </param>
	/// <returns> The radius of the locations. </returns>
	public static double GetRadius(this IEnumerable<IBasicLocation> locations)
	{
		var locationList = locations.ToList();
		var center = locationList.GetCenter();
		var minLatitude = locationList.Min(x => x.Latitude);
		var maxLatitude = locationList.Max(x => x.Latitude);
		var minLongitude = locationList.Min(x => x.Longitude);
		var maxLongitude = locationList.Max(x => x.Longitude);
		var latitudeDegrees = Abs(maxLatitude - minLatitude);
		var longitudeDegrees = Abs(maxLongitude - minLongitude);
		var metersPerDegrees = CalculateMetersPerDegrees(center);
		var latitudeMeters = latitudeDegrees * metersPerDegrees.Latitude;
		var longitudeMeters = longitudeDegrees * metersPerDegrees.Longitude;
		var value = Max(latitudeMeters, longitudeMeters);
		return value;
	}

	/// <summary>
	/// Check a location to determine if <see cref="ILocation.Accuracy" /> is available.
	/// </summary>
	/// <param name="location"> The location to validate. </param>
	/// <returns> True if the value is available. </returns>
	public static bool HasAccuracy(this ILocation location)
	{
		return location.AccuracyReference != AccuracyReferenceType.Unknown;
	}

	/// <summary>
	/// Check a location to determine if <see cref="IVerticalLocation.Altitude" /> is available.
	/// </summary>
	/// <param name="location"> The location to validate. </param>
	/// <returns> True if the value is available. </returns>
	public static bool HasAltitude(this ILocation location)
	{
		return location.AltitudeReference != AltitudeReferenceType.Unspecified;
	}

	/// <summary>
	/// Check a location to determine if <see cref="ILocation.AltitudeAccuracy" /> is available.
	/// </summary>
	/// <param name="location"> The location to validate. </param>
	/// <returns> True if the value is available. </returns>
	public static bool HasAltitudeAccuracy(this ILocation location)
	{
		return location.AltitudeAccuracyReference != AccuracyReferenceType.Unknown;
	}

	/// <summary>
	/// Check a location to determine if <see cref="ILocation.Heading" /> is available.
	/// </summary>
	/// <param name="location"> The location to validate. </param>
	/// <returns> True if the value is available. </returns>
	public static bool HasHeading(this ILocation location)
	{
		return location.LocationFlags.HasFlag(LocationFlags.HasHeading);
	}

	/// <summary>
	/// Check a location to determine if <see cref="IHorizontalLocation.Latitude" /> and <see cref="IHorizontalLocation.Longitude" /> is available.
	/// </summary>
	/// <param name="location"> The location to validate. </param>
	/// <returns> True if the value is available. </returns>
	public static bool HasLatitudeLongitude(this ILocation location)
	{
		return location.AccuracyReference != AccuracyReferenceType.Unknown;
	}

	/// <summary>
	/// Check a location to determine if <see cref="ILocation.Speed" /> is available.
	/// </summary>
	/// <param name="location"> The location to validate. </param>
	/// <returns> True if the value is available. </returns>
	public static bool HasSpeed(this ILocation location)
	{
		return location.LocationFlags.HasFlag(LocationFlags.HasSpeed);
	}

	/// <summary>
	/// Determine if the location is inside a set of locations.
	/// </summary>
	/// <param name="locations"> The locations to get the center for. </param>
	/// <param name="location"> The location to check for. </param>
	/// <returns> True if the location is inside the set. </returns>
	public static bool IsLocationInside(this IList<IBasicLocation> locations, IBasicLocation location)
	{
		return IsLocationInside(locations, location.Latitude, location.Longitude);
	}

	/// <summary>
	/// Determine if the location is inside a set of locations.
	/// </summary>
	/// <param name="locations"> The locations to get the center for. </param>
	/// <param name="latitude"> The latitude to check for. </param>
	/// <param name="longitude"> The longitude to check for. </param>
	/// <returns> True if the location is inside the set. </returns>
	public static bool IsLocationInside(this IList<IBasicLocation> locations, double latitude, double longitude)
	{
		if (locations == null)
		{
			return false;
		}

		var j = locations.Count - 1;
		var response = false;

		for (var i = 0; i < locations.Count; i++)
		{
			if (((locations[i].Longitude < longitude) && (locations[j].Longitude >= longitude)) || ((locations[j].Longitude < longitude) && (locations[i].Longitude >= longitude)))
			{
				if ((locations[i].Latitude + (((longitude - locations[i].Longitude) / (locations[j].Longitude - locations[i].Longitude)) * (locations[j].Latitude - locations[i].Latitude))) < latitude)
				{
					response = !response;
				}
			}

			j = i;
		}

		return response;
	}

	/// <summary>
	/// Return true if the location is valid.
	/// Note: we consider invalid a location with both Latitude 0 and Longitude 0.
	/// </summary>
	/// <param name="location"> The location to validate </param>
	/// <returns> </returns>
	public static bool IsValidLocation(this IBasicLocation location)
	{
		return location is { Latitude: >= -90 and <= 90, Longitude: >= -180 and <= 180 }
			&& !double.IsInfinity(location.Latitude)
			&& !double.IsInfinity(location.Longitude)
			&& !((location.Latitude == 0)
				&& (location.Longitude == 0))
			&& !((Abs(location.Latitude - -1) < double.Epsilon)
				&& (Abs(location.Longitude - -1) < double.Epsilon));
	}

	/// <summary>
	/// Convert Kilometers to Miles
	/// </summary>
	/// <param name="kilometers"> </param>
	/// <returns> </returns>
	public static double KilometersToMiles(double kilometers)
	{
		return kilometers * .62137119;
	}

	/// <summary>
	/// Calculate the meters per second.
	/// </summary>
	/// <param name="distanceInMeters"> The distances in meters. </param>
	/// <param name="span"> The span of time. </param>
	/// <returns> The speed in meters per second. </returns>
	public static double MetersPerSecond(this double distanceInMeters, TimeSpan span)
	{
		return distanceInMeters / span.TotalSeconds;
	}

	/// <summary>
	/// Convert Miles to Kilometers
	/// </summary>
	/// <param name="miles"> </param>
	/// <returns> </returns>
	public static double MilesToKilometers(double miles)
	{
		return miles * 1.609344;
	}

	/// <summary>
	/// Updates a location with a new location.
	/// </summary>
	/// <param name="location"> The location to be updated. </param>
	/// <param name="update"> The new location. </param>
	/// <param name="exclusions"> An optional set of exclusions. </param>
	public static void Update(this IBasicLocation location, IBasicLocation update, params string[] exclusions)
	{
		// If the update is null then there is nothing to do.
		if (update == null)
		{
			return;
		}

		// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

		if (exclusions.Length <= 0)
		{
			location.Altitude = update.Altitude;
			location.AltitudeReference = update.AltitudeReference;
			location.Latitude = update.Latitude;
			location.Longitude = update.Longitude;
		}
		else
		{
			location.IfThen(x => !exclusions.Contains(nameof(IBasicLocation.Altitude)), x => x.Altitude = update.Altitude);
			location.IfThen(x => !exclusions.Contains(nameof(IBasicLocation.AltitudeReference)), x => x.AltitudeReference = update.AltitudeReference);
			location.IfThen(x => !exclusions.Contains(nameof(IBasicLocation.Latitude)), x => x.Latitude = update.Latitude);
			location.IfThen(x => !exclusions.Contains(nameof(IBasicLocation.Longitude)), x => x.Longitude = update.Longitude);
		}
	}

	/// <summary>
	/// Update the location's HasHeading location flag.
	/// </summary>
	/// <param name="location"> The location to validate. </param>
	/// <param name="value"> True to set HasHeading otherwise clear. </param>
	public static void UpdateHasHeading(this ILocation location, bool value)
	{
		location.LocationFlags = value
			? location.LocationFlags.SetFlag(LocationFlags.HasHeading)
			: location.LocationFlags.ClearFlag(LocationFlags.HasHeading);
	}

	/// <summary>
	/// Update the location's HasSpeed location flag.
	/// </summary>
	/// <param name="location"> The location to validate. </param>
	/// <param name="value"> True to set HasSpeedy otherwise clear. </param>
	public static void UpdateHasSpeed(this ILocation location, bool value)
	{
		location.LocationFlags = value
			? location.LocationFlags.SetFlag(LocationFlags.HasSpeed)
			: location.LocationFlags.ClearFlag(LocationFlags.HasSpeed);
	}

	private static double ToDegrees(double radians)
	{
		return (radians * 180) / PI;
	}

	private static double ToHeading(double radians)
	{
		// convert radians to degrees (as heading: 0...360)
		return (ToDegrees(radians) + 360) % 360;
	}

	private static double ToRadians(double degrees)
	{
		return 0.017453292519943295 * degrees;
	}

	#endregion
}