#region References

using Microsoft.Win32;
#if (NET6_0_OR_GREATER)
using System.Runtime.Versioning;
#endif

#endregion

namespace Speedy.Application.Internal.Windows;

#if (NET6_0_OR_GREATER)
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