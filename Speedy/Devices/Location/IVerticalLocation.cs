namespace Speedy.Devices.Location;

/// <summary>
/// Represents a vertical location (alt, alt ref).
/// </summary>
public interface IVerticalLocation : IBindable
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