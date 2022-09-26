#region References

using System;

#endregion

namespace Speedy.Devices
{
	/// <inheritdoc />
	public class DefaultRuntimeInformation : IRuntimeInformation
	{
		#region Properties

		/// <inheritdoc />
		public string ApplicationName { get; set; }

		/// <inheritdoc />
		public Version ApplicationVersion { get; set; }

		/// <inheritdoc />
		public string DeviceId { get; set; }

		/// <inheritdoc />
		public string DeviceManufacturer { get; set; }

		/// <inheritdoc />
		public string DeviceModel { get; set; }

		/// <inheritdoc />
		public string DeviceName { get; set; }

		/// <inheritdoc />
		public string DeviceType { get; set; }

		/// <inheritdoc />
		public string PlatformName { get; set; }

		#endregion
	}
}