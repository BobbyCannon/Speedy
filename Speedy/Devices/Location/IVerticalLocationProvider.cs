#region References

using Speedy.Serialization;
using System;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a vertical location provider.
/// </summary>
public interface IVerticalLocationProvider<T> : ILocationProvider<T>
	where T : class, IVerticalLocation, ICloneable<T>
{
	#region Methods

	/// <summary>
	/// Gets the vertical location.
	/// </summary>
	/// <returns> The vertical location. </returns>
	T GetVerticalLocation();

	#endregion
}