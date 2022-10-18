namespace Speedy.Devices.Location;

/// <summary>
/// Error for location provider.
/// </summary>
public enum LocationProviderError
{
	/// <summary>
	/// The provider ran into an unknown error.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// The provider was unable to retrieve any position data.
	/// </summary>
	PositionUnavailable = 1,

	/// <summary>
	/// The app is not, or no longer, authorized to receive location data.
	/// </summary>
	Unauthorized = 2
}