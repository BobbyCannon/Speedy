#region References

using System;
using Speedy.Devices;

#endregion

namespace Speedy.Sync;

/// <summary>
/// The sync client details.
/// </summary>
public class SyncClientDetails : ISyncClientDetails
{
	#region Properties

	/// <inheritdoc />
	public string ApplicationName { get; set; }

	/// <inheritdoc />
	public Version ApplicationVersion { get; set; }

	/// <inheritdoc />
	public string DeviceId { get; set; }

	/// <inheritdoc />
	public DevicePlatform DevicePlatform { get; set; }

	/// <inheritdoc />
	public DeviceType DeviceType { get; set; }

	#endregion
}

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