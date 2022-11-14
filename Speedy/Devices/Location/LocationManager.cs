#region References

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// The manager for location.
/// </summary>
public class LocationManager : DeviceInformationManager<Location>
{
	#region Constructors

	/// <inheritdoc />
	public LocationManager(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion
}