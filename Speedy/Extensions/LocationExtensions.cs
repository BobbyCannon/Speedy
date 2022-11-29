#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Data;
using Speedy.Data.Location;
using static System.Math;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for location related code.
/// </summary>
public static class LocationExtensions
{
	#region Constants

	/// <summary>
	/// Radius of the earth.
	/// </summary>
	public const double EarthRadius = 6378137.0;

	#endregion

	#region Constructors

	static LocationExtensions()
	{
		SupportedAccuracyReferenceTypes = EnumExtensions.GetEnumValues(AccuracyReferenceType.Unspecified);
		SupportedAccuracyReferenceTypesForHorizontal = EnumExtensions.GetEnumValues(AccuracyReferenceType.Unspecified);
		SupportedAccuracyReferenceTypesForVertical = EnumExtensions.GetEnumValues(AccuracyReferenceType.Unspecified);
		SupportedAltitudeReferenceTypes = EnumExtensions.GetEnumValues(AltitudeReferenceType.Unspecified);
	}

	#endregion

	#region Properties

	/// <summary>
	/// Globally supported Accuracy Reference types. Changing this affects extension methods like HasAccuracy.
	/// </summary>
	public static HashSet<AccuracyReferenceType> SupportedAccuracyReferenceTypes { get; }

	/// <summary>
	/// Globally supported Accuracy Reference types for horizontal locations. Changing this affects extension methods like HasAccuracy.
	/// </summary>
	public static HashSet<AccuracyReferenceType> SupportedAccuracyReferenceTypesForHorizontal { get; }

	/// <summary>
	/// Globally supported Accuracy Reference types for vertical locations. Changing this affects extension methods like HasAccuracy.
	/// </summary>
	public static HashSet<AccuracyReferenceType> SupportedAccuracyReferenceTypesForVertical { get; }

	/// <summary>
	/// Globally supported Altitude Reference types. Changing this affects extension methods like HasAltitude.
	/// </summary>
	public static HashSet<AltitudeReferenceType> SupportedAltitudeReferenceTypes { get; }

	#endregion

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
	public static IBasicLocation CalculateDegreesPerMeter(this IMinimalHorizontalLocation location)
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
	public static double CalculateHeading(this IMinimalHorizontalLocation start, IMinimalHorizontalLocation end)
	{
		return CalculateHeading(start.Latitude, start.Longitude, end.Latitude, end.Longitude);
	}

	/// <summary>
	/// Calculate a key that consist of {ProviderName}-{SourceName}-{InformationId}.
	/// </summary>
	/// <param name="information"> The information to create the key for. </param>
	/// <returns> The key for the information. </returns>
	public static string CalculateKey(this IAccurateInformation information)
	{
		return $"{information.ProviderName}-{information.SourceName}-{information.InformationId}";
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
	public static IBasicLocation CalculateMetersPerDegrees(this IMinimalHorizontalLocation location)
	{
		var rLat = location.Latitude * (PI / 180.0);
		var metersPerDegreeLatitude = 111131.77741377673104 / Pow(1 + (0.0033584313098335197297 * Cos(2 * rLat)), 3d / 2);
		var metersPerDegreeLongitude = (111506.26354049367285 * Cos(rLat)) / Pow(1 + (0.0033584313098335197297 * Cos(2 * rLat)), 1d / 2);
		return new BasicLocation(metersPerDegreeLatitude, metersPerDegreeLongitude);
	}

	/// <summary>
	/// Calculate the corners of a circle
	/// </summary>
	/// <param name="center"> The center of the circle. </param>
	/// <param name="radius"> The radius of the circle. </param>
	/// <param name="circlePoints"> The amount of points for the circle. </param>
	/// <returns> The points for the circle. </returns>
	public static IEnumerable<IBasicLocation> CreateCircle(this IMinimalHorizontalLocation center, double radius, byte circlePoints = 18)
	{
		var locations = new List<BasicLocation>();
		var geoLatitude = (center.Latitude * PI) / 180.0;
		var geoLongitude = (center.Longitude * PI) / 180.0;
		var x = radius / EarthRadius;
		var increment = 360 / circlePoints;

		for (var i = 0; i <= 360; i += increment)
		{
			var altitudeRadians = (i * PI) / 180.0;
			var latitudeRadians = Asin((Sin(geoLatitude) * Cos(x)) + (Cos(geoLatitude) * Sin(x) * Cos(altitudeRadians)));
			var longitudeRadians = geoLongitude + Atan2(Sin(altitudeRadians) * Sin(x) * Cos(geoLatitude), Cos(x) - (Sin(geoLatitude) * Sin(latitudeRadians)));
			var location = new BasicLocation
			{
				Latitude = (180.0 * latitudeRadians) / PI,
				Longitude = (180.0 * longitudeRadians) / PI
			};
			locations.Add(location);
		}

		return locations;
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
		var dLon = ToRadians(longitudeEnd - longitudeStart);
		var theta1 = ToRadians(latitudeStart);
		var theta2 = ToRadians(latitudeEnd);
		var s1 = Sin(theta2 - theta1);
		var s2 = Sin(dLon / 2.0);
		var a = (s1 * s1) + (Cos(theta1) * Cos(theta2) * s2 * s2);

		var distanceInMeters = EarthRadius * 2 * Atan2(Sqrt(a), Sqrt(1.0 - a));
		return distanceInMeters;
	}

	/// <summary>
	/// Calculates the distance between two locations in meters.
	/// </summary>
	/// <param name="start"> Start. </param>
	/// <param name="end"> End. </param>
	/// <returns> The distance in meters. </returns>
	public static double DistanceBetween(this Location start, Location end)
	{
		if (!start.VerticalLocation.HasValue
			|| !end.VerticalLocation.HasValue)
		{
			return DistanceBetween(
				start.HorizontalLocation.Latitude,
				start.HorizontalLocation.Longitude,
				end.HorizontalLocation.Latitude,
				end.HorizontalLocation.Longitude
			);
		}

		return DistanceBetween(
			start.HorizontalLocation.Latitude,
			start.HorizontalLocation.Longitude,
			start.VerticalLocation.Altitude,
			end.HorizontalLocation.Latitude,
			end.HorizontalLocation.Longitude,
			end.VerticalLocation.Altitude
		);
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
	public static double DistanceBetween(this IMinimalHorizontalLocation locationStart, IMinimalHorizontalLocation locationEnd)
	{
		return DistanceBetween(locationStart.Latitude, locationStart.Longitude, locationEnd.Latitude, locationEnd.Longitude);
	}

	/// <summary>
	/// Gets the center of a set of locations.
	/// </summary>
	/// <param name="locations"> The locations to get the center for. </param>
	/// <returns> The center of the locations. </returns>
	public static IBasicLocation GetCenter(this IEnumerable<IMinimalHorizontalLocation> locations)
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
	/// Tries to get an ellipsoid altitude for the vertical location.
	/// </summary>
	/// <param name="location"> The location to process. </param>
	/// <param name="relativeTo"> A relative location to base non-ellipsoid vertical locations to. </param>
	/// <returns> The final ellipsoid altitude or best guess. </returns>
	public static double GetEllipsoidAltitude(this IMinimalVerticalLocation location, IMinimalVerticalLocation relativeTo = null)
	{
		if (location == null)
		{
			return 0;
		}

		return location.AltitudeReference switch
		{
			AltitudeReferenceType.Ellipsoid => location.Altitude,
			AltitudeReferenceType.Terrain => location.Altitude + (relativeTo?.GetEllipsoidAltitude() ?? 0),
			_ => relativeTo?.GetEllipsoidAltitude() ?? 0
		};
	}

	/// <summary>
	/// Gets the radius of a set of locations.
	/// </summary>
	/// <param name="locations"> The locations to get the center for. </param>
	/// <returns> The radius of the locations. </returns>
	public static double GetRadius(this IEnumerable<IMinimalHorizontalLocation> locations)
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
	/// Handles triggering dependent properties for <see cref="ILocationInformation.Flags" />.
	/// </summary>
	/// <param name="information"> The location information for a device. </param>
	/// <param name="propertyName"> The name of the property that changed. </param>
	public static void HandleFlagsChanged(this ILocationInformation information, string propertyName)
	{
		if (propertyName != nameof(ILocationInformation.Flags))
		{
			return;
		}

		information.OnPropertyChanged(nameof(ILocationInformation.HasHeading));
		information.OnPropertyChanged(nameof(ILocationInformation.HasSpeed));
		information.OnPropertyChanged(nameof(ILocationInformation.HasValue));
	}

	/// <summary>
	/// Check a information to determine if <see cref="IAccurateInformation.Accuracy" /> is available.
	/// </summary>
	/// <param name="information"> The information to validate. </param>
	/// <returns> True if the value is available. </returns>
	public static bool HasAccuracy(this IAccurateInformation information)
	{
		return information switch
		{
			IHorizontalLocation => SupportedAccuracyReferenceTypesForHorizontal.Contains(information.AccuracyReference),
			IVerticalLocation => SupportedAccuracyReferenceTypesForVertical.Contains(information.AccuracyReference),
			_ => SupportedAccuracyReferenceTypes.Contains(information.AccuracyReference)
		};
	}

	/// <summary>
	/// Check a information to determine if a location device information has a heading.
	/// </summary>
	/// <param name="information"> The information to validate. </param>
	/// <returns> True if the heading is available otherwise false. </returns>
	public static bool HasHeading(this ILocationInformation information)
	{
		return information.Flags.HasFlag(LocationFlags.HasHeading);
	}

	/// <summary>
	/// Check a information to determine if a location device information has a location.
	/// </summary>
	/// <param name="information"> The information to validate. </param>
	/// <returns> True if the value is available. </returns>
	public static bool HasLocation(this ILocationInformation information)
	{
		return information.Flags.HasFlag(LocationFlags.HasLocation);
	}

	/// <summary>
	/// Check a information to determine if a location device information has a speed.
	/// </summary>
	/// <param name="information"> The information to validate. </param>
	/// <returns> True if the speed is available otherwise false. </returns>
	public static bool HasSpeed(this ILocationInformation information)
	{
		return information.Flags.HasFlag(LocationFlags.HasSpeed);
	}

	/// <summary>
	/// Check a location to determine if <see cref="IMinimalVerticalLocation.Altitude" /> is available.
	/// </summary>
	/// <param name="location"> The location to validate. </param>
	/// <returns> True if the value is available. </returns>
	public static bool HasSupportedAltitude(this IMinimalVerticalLocation location)
	{
		return SupportedAltitudeReferenceTypes.Contains(location.AltitudeReference);
	}

	/// <summary>
	/// Is the left location greater than the right.
	/// </summary>
	/// <param name="left"> The left location. </param>
	/// <param name="right"> The right location. </param>
	/// <returns> True if the location is greater than the provided location. </returns>
	public static bool IsGreaterThan(this IMinimalVerticalLocation left, IMinimalVerticalLocation right)
	{
		return InternalGreaterThan(left, right, false);
	}

	/// <summary>
	/// Is the left location greater than or equal to the right.
	/// </summary>
	/// <param name="left"> The left location. </param>
	/// <param name="right"> The right location. </param>
	/// <returns> True if the location is greater than or equal to the provided location. </returns>
	public static bool IsGreaterThanOrEqualTo(this IMinimalVerticalLocation left, IMinimalVerticalLocation right)
	{
		return InternalGreaterThan(left, right, true);
	}

	/// <summary>
	/// Is the left location less than the right.
	/// </summary>
	/// <param name="left"> The left location. </param>
	/// <param name="right"> The right location. </param>
	/// <returns> True if the location is less than the provided location. </returns>
	public static bool IsLessThan(this IMinimalVerticalLocation left, IMinimalVerticalLocation right)
	{
		return InternalLessThan(left, right, false);
	}

	/// <summary>
	/// Is the left location less than or equal to the right.
	/// </summary>
	/// <param name="left"> The left location. </param>
	/// <param name="right"> The right location. </param>
	/// <returns> True if the location is less than or equal to the provided location. </returns>
	public static bool IsLessThanOrEqualTo(this IMinimalVerticalLocation left, IMinimalVerticalLocation right)
	{
		return InternalLessThan(left, right, true);
	}

	/// <summary>
	/// Determine if the location is inside a set of locations.
	/// </summary>
	/// <param name="locations"> The locations to get the center for. </param>
	/// <param name="location"> The location to check for. </param>
	/// <returns> True if the location is inside the set. </returns>
	public static bool IsLocationInside(this IList<IBasicLocation> locations, IMinimalHorizontalLocation location)
	{
		return IsLocationInside(locations.Cast<IMinimalHorizontalLocation>().ToList(), location.Latitude, location.Longitude);
	}

	/// <summary>
	/// Determine if the location is inside a set of locations.
	/// </summary>
	/// <param name="locations"> The locations to get the center for. </param>
	/// <param name="location"> The location to check for. </param>
	/// <returns> True if the location is inside the set. </returns>
	public static bool IsLocationInside(this IList<IMinimalHorizontalLocation> locations, IMinimalHorizontalLocation location)
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
	public static bool IsLocationInside(this IList<IMinimalHorizontalLocation> locations, double latitude, double longitude)
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
	public static bool IsValidLocation(this IMinimalHorizontalLocation location)
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
	/// Extension method to convert a basic location to another instance.
	/// </summary>
	/// <param name="location"> The location to convert to just a basic location. </param>
	/// <returns> A new instance of the basic location. </returns>
	public static IBasicLocation ToBasicLocation(this IBasicLocation location)
	{
		var bindable = location as IBindable;
		return new BasicLocation(location, bindable?.GetDispatcher());
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
	/// <param name="value"> True to set HasHeading flag otherwise clear the flag. </param>
	public static void UpdateHasHeading(this ILocationInformation location, bool value)
	{
		location.Flags = value
			? location.Flags.SetFlag(LocationFlags.HasHeading)
			: location.Flags.ClearFlag(LocationFlags.HasHeading);
	}

	/// <summary>
	/// Update the location's HasLocation flag.
	/// </summary>
	/// <param name="location"> The location to validate. </param>
	/// <param name="value"> True to set HasLocation flag otherwise clear the flag. </param>
	public static void UpdateHasLocation(this ILocationInformation location, bool value)
	{
		location.Flags = value
			? location.Flags.SetFlag(LocationFlags.HasLocation)
			: location.Flags.ClearFlag(LocationFlags.HasLocation);
	}

	/// <summary>
	/// Update the location's HasSpeed location flag.
	/// </summary>
	/// <param name="location"> The location to validate. </param>
	/// <param name="value"> True to set HasSpeedy flag otherwise clear the flag. </param>
	public static void UpdateHasSpeed(this ILocationInformation location, bool value)
	{
		location.Flags = value
			? location.Flags.SetFlag(LocationFlags.HasSpeed)
			: location.Flags.ClearFlag(LocationFlags.HasSpeed);
	}

	/// <summary>
	/// Update the horizontal location (lat, long).
	/// </summary>
	/// <param name="location"> The location to update. </param>
	/// <param name="update"> The update with new values. </param>
	public static void UpdateLocation(this IHorizontalLocation location, IHorizontalLocation update)
	{
		location.Latitude = update.Latitude;
		location.Longitude = update.Longitude;
		location.Accuracy = update.Accuracy;
		location.AccuracyReference = update.AccuracyReference;
	}

	/// <summary>
	/// Update the BasicLocation with an update.
	/// </summary>
	/// <param name="location"> The location to update. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public static bool UpdateWith(this IBasicLocation location, IBasicLocation update, params string[] exclusions)
	{
		return UpdateWith(location, (IMinimalHorizontalLocation) update, exclusions)
			| UpdateWith(location, (IMinimalVerticalLocation) update, exclusions);
	}

	/// <summary>
	/// Update the BasicLocation with an update.
	/// </summary>
	/// <param name="location"> The location to update. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public static bool UpdateWith(this IBasicLocation location, ILocation<IHorizontalLocation, IVerticalLocation> update, params string[] exclusions)
	{
		return UpdateWith(location, update.HorizontalLocation, exclusions)
			| UpdateWith(location, update.VerticalLocation, exclusions);
	}

	/// <summary>
	/// Update the BasicLocation with an update.
	/// </summary>
	/// <param name="location"> The location to update. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public static bool UpdateWith(this IBasicLocation location, IMinimalHorizontalLocation update, params string[] exclusions)
	{
		// If the update is null then there is nothing to do.
		if (update == null)
		{
			return false;
		}

		// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

		if (exclusions.Length <= 0)
		{
			location.Latitude = update.Latitude;
			location.Longitude = update.Longitude;
		}
		else
		{
			location.IfThen(_ => !exclusions.Contains(nameof(IBasicLocation.Latitude)), x => x.Latitude = update.Latitude);
			location.IfThen(_ => !exclusions.Contains(nameof(IBasicLocation.Longitude)), x => x.Longitude = update.Longitude);
		}

		return true;
	}

	/// <summary>
	/// Update the BasicLocation with an update.
	/// </summary>
	/// <param name="location"> The location to update. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public static bool UpdateWith(this IBasicLocation location, IMinimalVerticalLocation update, params string[] exclusions)
	{
		// If the update is null then there is nothing to do.
		if (update == null)
		{
			return false;
		}

		// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

		if (exclusions.Length <= 0)
		{
			location.Altitude = update.Altitude;
			location.AltitudeReference = update.AltitudeReference;
		}
		else
		{
			location.IfThen(_ => !exclusions.Contains(nameof(IBasicLocation.Altitude)), x => x.Altitude = update.Altitude);
			location.IfThen(_ => !exclusions.Contains(nameof(IBasicLocation.AltitudeReference)), x => x.AltitudeReference = update.AltitudeReference);
		}

		return true;
	}

	private static bool InternalGreaterThan(this IMinimalVerticalLocation left, IMinimalVerticalLocation right, bool inclusive)
	{
		if (left.AltitudeReference == right.AltitudeReference)
		{
			return inclusive
				? left.Altitude >= right.Altitude
				: left.Altitude > right.Altitude;
		}

		return false;
	}

	private static bool InternalLessThan(this IMinimalVerticalLocation left, IMinimalVerticalLocation right, bool inclusive)
	{
		if (left.AltitudeReference == right.AltitudeReference)
		{
			return inclusive
				? left.Altitude <= right.Altitude
				: left.Altitude < right.Altitude;
		}

		return false;
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