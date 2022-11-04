#region References

using System;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a vertical location (alt, alt ref, acc, acc ref).
/// </summary>
public interface IVerticalLocation : IMinimalVerticalLocation
{
	#region Properties

	/// <summary>
	/// Specifies if the Altitude Accuracy value is valid
	/// </summary>
	bool HasVerticalAccuracy { get; }

	/// <summary>
	/// The accuracy of the vertical location. (altitude)
	/// </summary>
	double VerticalAccuracy { get; set; }

	/// <summary>
	/// The reference system for altitude accuracy.
	/// </summary>
	AccuracyReferenceType VerticalAccuracyReference { get; set; }

	/// <summary>
	/// The name of the source of the location. Ex. Wifi, GPS, Hardware, Simulated, etc
	/// </summary>
	string VerticalSourceName { get; set; }

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

	/// <summary>
	/// Specifies if the Altitude value is valid
	/// </summary>
	bool HasAltitude { get; }

	#endregion
}