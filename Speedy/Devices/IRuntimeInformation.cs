#region References

using System;

#endregion

namespace Speedy.Devices
{
	/// <summary>
	/// Represents the information of the client at runtime.
	/// </summary>
	public interface IRuntimeInformation
	{
		#region Properties

		/// <summary>
		/// The name of the application.
		/// </summary>
		string ApplicationName { get; }

		/// <summary>
		/// The version of the application.
		/// </summary>
		Version ApplicationVersion { get; }

		/// <summary>
		/// The ID of the device.
		/// </summary>
		string DeviceId { get; }

		/// <summary>
		/// The name of the device manufacturer.
		/// </summary>
		string DeviceManufacturer { get; }

		/// <summary>
		/// The model of the device.
		/// </summary>
		string DeviceModel { get; }

		/// <summary>
		/// The name of the device.
		/// </summary>
		string DeviceName { get; }

		/// <summary>
		/// The type of the device.
		/// </summary>
		string DeviceType { get; }

		/// <summary>
		/// The name of the platform.
		/// </summary>
		string PlatformName { get; }

		#endregion
	}
}