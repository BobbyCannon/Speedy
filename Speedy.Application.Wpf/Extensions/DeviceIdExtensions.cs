#region References

using System;
using System.ComponentModel;
using Microsoft.Win32;
using Speedy.Application.Wpf.Internal;

#endregion

namespace Speedy.Application.Wpf.Extensions;

/// <summary>
/// Extension methods for <see cref="DeviceId" />.
/// </summary>
public static class DeviceIdExtensions
{
	#region Methods

	/// <summary>
	/// Adds the MAC address to the device identifier, optionally excluding wireless adapters and/or non-physical adapters.
	/// </summary>
	/// <param name="excludeWireless"> A value indicating whether wireless adapters should be excluded. </param>
	/// <param name="excludeNonPhysical"> A value indicating whether non-physical adapters should be excluded. </param>
	/// <returns> The <see cref="DeviceId" /> instance. </returns>
	public static DeviceId AddMacAddressFromMmi(this DeviceId builder, bool excludeWireless, bool excludeNonPhysical)
	{
		return builder.AddComponent("MACAddress", new MmiMacAddressDeviceIdComponent(excludeWireless, excludeNonPhysical));
	}

	/// <summary>
	/// Adds the Machine GUID (from HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography\MachineGuid) to the device identifier.
	/// </summary>
	/// <returns> The <see cref="DeviceId" /> instance. </returns>
	public static DeviceId AddMachineGuid(this DeviceId builder)
	{
		return AddRegistryValue(builder,
			"MachineGuid",
			RegistryView.Registry64,
			RegistryHive.LocalMachine,
			@"SOFTWARE\Microsoft\Cryptography",
			"MachineGuid");
	}

	/// <summary>
	/// Adds the motherboard serial number to the device identifier.
	/// </summary>
	/// <returns> The <see cref="DeviceId" /> instance. </returns>
	public static DeviceId AddMotherboardSerialNumber(this DeviceId builder)
	{
		return builder.AddComponent("MotherboardSerialNumber", new MmiWqlDeviceIdComponent("Win32_BaseBoard", "SerialNumber"));
	}

	/// <summary>
	/// Adds the processor ID to the device identifier.
	/// </summary>
	/// <returns> The <see cref="DeviceId" /> instance. </returns>
	public static DeviceId AddProcessorId(this DeviceId builder)
	{
		return builder.AddComponent("ProcessorId", new MmiWqlDeviceIdComponent("Win32_Processor", "ProcessorId"));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RegistryValueDeviceIdComponent" /> class.
	/// </summary>
	/// <param name="componentName"> The name of the component. </param>
	/// <param name="registryView"> The registry view. </param>
	/// <param name="registryHive"> The registry hive. </param>
	/// <param name="registryKeyName"> The name of the registry key. </param>
	/// <param name="registryValueName"> The name of the registry value. </param>
	public static DeviceId AddRegistryValue(this DeviceId builder, string componentName, RegistryView registryView, RegistryHive registryHive, string registryKeyName, string registryValueName)
	{
		return builder.AddComponent(componentName, new RegistryValueDeviceIdComponent(registryView, registryHive, registryKeyName, registryValueName));
	}

	/// <summary>
	/// Adds the system serial drive number to the device identifier.
	/// </summary>
	/// <returns> The <see cref="DeviceId" /> instance. </returns>
	public static DeviceId AddSystemDriveSerialNumber(this DeviceId builder)
	{
		return builder.AddComponent("SystemDriveSerialNumber", new MmiSystemDriveSerialNumberDeviceIdComponent());
	}

	/// <summary>
	/// Adds the system serial drive number to the device identifier.
	/// </summary>
	/// <returns> The <see cref="DeviceId" /> instance. </returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This method name was a typo. Use AddSystemDriveSerialNumber instead.")]
	public static DeviceId AddSystemSerialDriveNumber(this DeviceId builder)
	{
		return builder.AddComponent("SystemDriveSerialNumber", new MmiSystemDriveSerialNumberDeviceIdComponent());
	}

	/// <summary>
	/// Adds the system UUID to the device identifier.
	/// </summary>
	/// <returns> The <see cref="DeviceId" /> instance. </returns>
	public static DeviceId AddSystemUuid(this DeviceId builder)
	{
		return builder.AddComponent("SystemUUID", new MmiWqlDeviceIdComponent("Win32_ComputerSystemProduct", "UUID"));
	}

	#endregion
}