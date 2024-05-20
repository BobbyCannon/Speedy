#region References

using System.Runtime.Versioning;
using Microsoft.Win32;

#endregion

namespace Speedy.Internal.Windows;

#if (!NETSTANDARD)
[SupportedOSPlatform("windows")]
#endif
internal class DeviceManufacturerRegistryComponent : RegistryComponent
{
	#region Constructors

	public DeviceManufacturerRegistryComponent()
		: base(RegistryView.Registry64, RegistryHive.LocalMachine,
			@"SOFTWARE\Microsoft\Windows\CurrentVersion\OEMInformation", "Manufacturer")
	{
		// HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\OEMInformation
	}

	#endregion
}