namespace Speedy.Devices.Location;

/// <summary>
/// Represents a horizontal location provider.
/// </summary>
public interface IHorizontalLocationProvider<out T> : IBindable
	where T : IHorizontalLocation
{
	/// <summary>
	/// Gets the horizontal location.
	/// </summary>
	/// <returns> The horizontal location. </returns>
	T GetHorizontalLocation();
}