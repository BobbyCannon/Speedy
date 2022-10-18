namespace Speedy.Devices.Location;

/// <summary>
/// Represents a vertical location provider.
/// </summary>
public interface IVerticalLocationProvider<out T> : IBindable
	where T : IVerticalLocation
{
	#region Methods

	/// <summary>
	/// Gets the vertical location.
	/// </summary>
	/// <returns> The vertical location. </returns>
	T GetVerticalLocation();

	#endregion
}