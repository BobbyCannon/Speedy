#region References

using System;
using Microsoft.Management.Infrastructure;

#endregion

namespace Speedy.Application.Wpf.Internal
{
	/// <summary>
	/// An implementation of <see cref="IDeviceIdComponent" /> that uses the system drive's serial number.
	/// </summary>
	internal class MmiSystemDriveSerialNumberDeviceIdComponent : IDeviceIdComponent
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MmiSystemDriveSerialNumberDeviceIdComponent" /> class.
		/// </summary>
		public MmiSystemDriveSerialNumberDeviceIdComponent()
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
			var systemLogicalDiskDeviceId = Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 2);

			using var session = CimSession.Create(null);

			foreach (var logicalDiskAssociator in session.QueryInstances(@"root\cimv2", "WQL", $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID=\"{systemLogicalDiskDeviceId}\"}} WHERE ResultClass = Win32_DiskPartition"))
			{
				if (logicalDiskAssociator.CimClass.CimSystemProperties.ClassName != "Win32_DiskPartition")
				{
					continue;
				}
				if (logicalDiskAssociator.CimInstanceProperties["DeviceId"].Value is not string diskPartitionDeviceId)
				{
					continue;
				}
				foreach (var diskPartitionAssociator in session.QueryInstances(@"root\cimv2", "WQL", $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID=\"{diskPartitionDeviceId}\"}}"))
				{
					if (diskPartitionAssociator.CimClass.CimSystemProperties.ClassName != "Win32_DiskDrive")
					{
						continue;
					}

					if (diskPartitionAssociator.CimInstanceProperties["SerialNumber"].Value is string diskDriveSerialNumber)
					{
						return diskDriveSerialNumber;
					}
				}
			}

			return null;
		}

		#endregion
	}
}