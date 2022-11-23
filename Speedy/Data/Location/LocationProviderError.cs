namespace Speedy.Data.Location;

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
	/// The provider was unable to retrieve any location data.
	/// </summary>
	LocationUnavailable = 1,

	/// <summary>
	/// The app is not, or no longer, authorized to receive location data.
	/// </summary>
	Unauthorized = 2,

	/// <summary>
	/// The app is messing a required dependency.
	/// </summary>
	MissingDependency = 3
}