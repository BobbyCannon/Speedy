#region References

using System;
using Speedy.Devices;

#endregion

namespace Speedy.Sync;

/// <summary>
/// The details for a sync client.
/// </summary>
public interface ISyncClientDetails
{
	#region Properties

	/// <summary>
	/// The ApplicationName value for Sync Client Details.
	/// </summary>
	string ApplicationName { get; set; }

	/// <summary>
	/// The ApplicationVersion value for Sync Client Details.
	/// </summary>
	Version ApplicationVersion { get; set; }

	/// <summary>
	/// The DeviceId value for Sync Client Details.
	/// </summary>
	string DeviceId { get; set; }

	/// <summary>
	/// The DevicePlatform value for Sync Client Details.
	/// </summary>
	DevicePlatform DevicePlatform { get; set; }

	/// <summary>
	/// The DeviceType value for Sync Client Details.
	/// </summary>
	DeviceType DeviceType { get; set; }

	#endregion
}