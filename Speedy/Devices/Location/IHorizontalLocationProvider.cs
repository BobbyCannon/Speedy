namespace Speedy.Devices.Location;

/// <summary>
/// Represents a horizontal location provider.
/// </summary>
public interface IHorizontalLocationProvider<out T> : IBindable
	where T : IHorizontalLocation
{
	#region Methods

	/// <summary>
	/// Gets the horizontal location.
	/// </summary>
	/// <returns> The horizontal location. </returns>
	T GetHorizontalLocation();

	#endregion
}