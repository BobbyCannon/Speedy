namespace Speedy.Devices.Location;

/// <summary>
/// Represents the reference point for the altitude.
/// </summary>
public enum AltitudeReferenceType
{
	/// <summary>
	/// Unknown / Unspecified
	/// </summary>
	Unspecified = 0,

	/// <summary>
	/// Distance above terrain or ground level.
	/// </summary>
	/// <remarks>
	/// Terrain is very volatile, be careful using the concept of terrain.
	/// </remarks>
	Terrain = 1,

	/// <summary>
	/// Ellipsoid which is a mathematical approximation of the shape of the Earth.
	/// </summary>
	Ellipsoid = 2,

	/// <summary>
	/// Distance above sea level.
	/// </summary>
	Geoid = 3
}