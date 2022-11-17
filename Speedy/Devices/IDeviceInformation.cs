#region References

using System;
using Speedy.Devices.Location;

#endregion

namespace Speedy.Devices;

/// <summary>
/// Represents information for a device.
/// </summary>
public interface IDeviceInformation : IBindable
{
	#region Properties

	/// <summary>
	/// The accuracy of the information.
	/// </summary>
	double Accuracy { get; set; }

	/// <summary>
	/// The reference system for accuracy.
	/// </summary>
	AccuracyReferenceType AccuracyReference { get; set; }

	/// <summary>
	/// Specifies if the Accuracy value is valid.
	/// </summary>
	bool HasAccuracy { get; }

	/// <summary>
	/// Determines if the device information has a value.
	/// </summary>
	/// <remarks>
	/// Each device information could / will have different value members.
	/// Ex. VerticalLocation will have Altitude, where HorizontalLocation will have Latitude, Longitude.
	/// </remarks>
	bool HasValue { get; }

	/// <summary>
	/// The name of the provider that is the source of this information.
	/// </summary>
	string ProviderName { get; set; }

	/// <summary>
	/// The name of the source of the information. Ex. Hardware, Software, Simulated, Wifi, GPS, etc
	/// </summary>
	string SourceName { get; set; }

	/// <summary>
	/// The original time of the information was captured.
	/// </summary>
	DateTime StatusTime { get; set; }

	#endregion
}