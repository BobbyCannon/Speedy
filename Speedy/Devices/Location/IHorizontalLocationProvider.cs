#region References

using System;
using Speedy.Serialization;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a horizontal location provider.
/// </summary>
public interface IHorizontalLocationProvider<T> : ILocationProvider<T>
	where T : class, IHorizontalLocation, ICloneable<T>
{
	#region Methods

	/// <summary>
	/// Gets the horizontal location.
	/// </summary>
	/// <returns> The horizontal location. </returns>
	T GetHorizontalLocation();

	#endregion
}