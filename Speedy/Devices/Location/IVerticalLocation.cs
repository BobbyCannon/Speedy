#region References

using System;
using Speedy.Storage;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a vertical location (alt, alt ref, acc, acc ref).
/// </summary>
public interface IVerticalLocation
	: IUpdatable<IVerticalLocation>,
		IMinimalVerticalLocation
{
	#region Properties

	/// <summary>
	/// Specifies if the Altitude value is valid
	/// </summary>
	bool HasAltitude { get; }

	/// <summary>
	/// Specifies if the Altitude Accuracy value is valid
	/// </summary>
	bool HasVerticalAccuracy { get; }

	/// <summary>
	/// Specifies if the Heading value is valid
	/// </summary>
	bool HasVerticalHeading { get; set; }

	/// <summary>
	/// Specifies if the Speed value is valid
	/// </summary>
	bool HasVerticalSpeed { get; set; }

	/// <summary>
	/// The accuracy of the vertical location. (altitude)
	/// </summary>
	double VerticalAccuracy { get; set; }

	/// <summary>
	/// The reference system for altitude accuracy.
	/// </summary>
	AccuracyReferenceType VerticalAccuracyReference { get; set; }

	/// <summary>
	/// Flags for the location of the provider.
	/// </summary>
	LocationFlags VerticalFlags { get; set; }

	/// <summary>
	/// The heading of a device.
	/// </summary>
	double VerticalHeading { get; set; }

	/// <summary>
	/// The name of the source of the location. Ex. Wifi, GPS, Hardware, Simulated, etc
	/// </summary>
	string VerticalSourceName { get; set; }

	/// <summary>
	/// The vertical speed of the device in meters per second.
	/// </summary>
	double VerticalSpeed { get; set; }

	/// <summary>
	/// The original time of the location was captured.
	/// </summary>
	DateTime VerticalStatusTime { get; set; }

	#endregion
}

/// <summary>
/// Represents a vertical location (alt, alt ref).
/// </summary>
public interface IMinimalVerticalLocation : IBindable
{
	#region Properties

	/// <summary>
	/// The altitude of the location
	/// </summary>
	double Altitude { get; set; }

	/// <summary>
	/// The reference type for the altitude value.
	/// </summary>
	AltitudeReferenceType AltitudeReference { get; set; }

	#endregion
}