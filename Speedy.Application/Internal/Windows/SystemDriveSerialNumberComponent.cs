#region References

using System;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;

#endregion

namespace Speedy.Application.Internal.Windows;

/// <summary>
/// An implementation of <see cref="IDeviceIdComponent" /> that uses the system drive's serial number.
/// </summary>
#if (NET6_0_OR_GREATER)
[SupportedOSPlatform("windows")]
#endif
internal class SystemDriveSerialNumberComponent : IDeviceIdComponent
{
	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="SystemDriveSerialNumberComponent" /> class.
	/// </summary>
	public SystemDriveSerialNumberComponent()
	{
	}

	#endregion

	#region Methods

	/// <summary>
	/// Gets the component value.
	/// </summary>
	/// <returns> The component value. </returns>
	public string GetValue()
	{
		var systemDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System);

		// SystemDirectory can sometimes be null or empty.
		// See: https://github.com/dotnet/runtime/issues/21430 and https://github.com/MatthewKing/DeviceId/issues/64
		if (string.IsNullOrEmpty(systemDirectory) || (systemDirectory.Length < 2))
		{
			return null;
		}

		var systemLogicalDiskDeviceId = systemDirectory.Substring(0, 2);

		var queryString = $"SELECT * FROM Win32_LogicalDisk where DeviceId = '{systemLogicalDiskDeviceId}'";
		using var searcher = new ManagementObjectSearcher(queryString);

		foreach (var disk in searcher.Get().OfType<ManagementObject>())
		{
			foreach (var partition in disk.GetRelated("Win32_DiskPartition").OfType<ManagementObject>())
			{
				foreach (var drive in partition.GetRelated("Win32_DiskDrive").OfType<ManagementObject>())
				{
					if (drive["SerialNumber"] is string serialNumber)
					{
						return serialNumber;
					}
				}
			}
		}

		return null;
	}

	#endregion
}