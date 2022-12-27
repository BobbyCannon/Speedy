#region References

using System.Collections.Generic;

#endregion

namespace Speedy.Application.Internal;

/// <summary>
/// Provides a method to combine a number of component values into a single device identifier string.
/// </summary>
internal interface IDeviceIdFormatter
{
	#region Methods

	/// <summary>
	/// Returns the device identifier string created by combining the specified components.
	/// </summary>
	/// <param name="components"> A dictionary containing the components. </param>
	/// <returns> The device identifier string. </returns>
	string GetDeviceId(IDictionary<string, IDeviceIdComponent> components);

	#endregion
}