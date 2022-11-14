#region References

using System;
using Speedy.Devices;
using Speedy.Devices.Location;

#endregion

namespace Speedy.Sync;

/// <summary>
/// Represents a sync device.
/// </summary>
/// <typeparam name="T"> The type of the ID for the sync model. </typeparam>
public class SyncDevice<T> : SyncModel<T>, ISyncDevice
{
	#region Properties

	/// <inheritdoc />
	public double Altitude { get; set; }

	/// <inheritdoc />
	public AltitudeReferenceType AltitudeReference { get; set; }

	/// <inheritdoc />
	public string ApplicationName { get; set; }

	/// <inheritdoc />
	public Version ApplicationVersion { get; set; }

	/// <inheritdoc />
	public decimal ApplicationVersionHash { get; set; }

	/// <inheritdoc />
	public string DeviceId { get; set; }

	/// <inheritdoc />
	public DevicePlatform DevicePlatform { get; set; }

	/// <inheritdoc />
	public DeviceType DeviceType { get; set; }

	/// <inheritdoc />
	public override T Id { get; set; }

	/// <inheritdoc />
	public double Latitude { get; set; }

	/// <inheritdoc />
	public double Longitude { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Load the sync options.
	/// </summary>
	/// <param name="syncOptions"> The options to be loaded. </param>
	public void Load(SyncOptions syncOptions)
	{
	}

	#endregion
}

/// <summary>
/// Represents a sync device.
/// </summary>
public interface ISyncDevice : IBasicLocation, ISyncClientDetails
{
	#region Properties

	/// <summary>
	/// The application version in a number hash.
	/// </summary>
	decimal ApplicationVersionHash { get; set; }

	#endregion
}