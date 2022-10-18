namespace Speedy.Devices.Location;

/// <summary>
/// Represents a horizontal location (lat, long).
/// </summary>
public interface IHorizontalLocation : IBindable
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