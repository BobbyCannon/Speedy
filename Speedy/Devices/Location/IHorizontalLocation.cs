#region References

using System;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a horizontal location (lat, long).
/// </summary>
public interface IHorizontalLocation : IMinimalHorizontalLocation
{
	#region Properties

	/// <summary>
	/// Specifies if the Accuracy value is valid.
	/// </summary>
	bool HasHorizontalAccuracy { get; }

	/// <summary>
	/// The accuracy of the horizontal location. (latitude, longitude)
	/// </summary>
	double HorizontalAccuracy { get; set; }

	/// <summary>
	/// The reference system for accuracy.
	/// </summary>
	AccuracyReferenceType HorizontalAccuracyReference { get; set; }

	/// <summary>
	/// The name of the source of the location. Ex. Wifi, GPS, Hardware, Simulated, etc
	/// </summary>
	string HorizontalSourceName { get; set; }

	/// <summary>
	/// The original time of the location was captured.
	/// </summary>
	DateTime HorizontalStatusTime { get; set; }

	#endregion
}

/// <summary>
/// Represents a horizontal location (lat, long).
/// </summary>
public interface IMinimalHorizontalLocation : IBindable
{
	#region Properties

	/// <summary>
	/// Ranges between -90 to 90 from North to South
	/// </summary>
	double Latitude { get; set; }

	/// <summary>
	/// Ranges between -180 to 180 from West to East
	/// </summary>
	double Longitude { get; set; }

	#endregion
}