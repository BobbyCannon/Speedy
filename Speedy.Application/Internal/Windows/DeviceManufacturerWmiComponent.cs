#region References

using System.Runtime.Versioning;

#endregion

namespace Speedy.Application.Internal.Windows;

#if (NET6_0_OR_GREATER)
[SupportedOSPlatform("windows")]
#endif
internal class DeviceManufacturerWmiComponent : WmiComponent
{
	#region Constructors

	public DeviceManufacturerWmiComponent()
		: base("Win32_ComputerSystem", "Manufacturer")
	{
	}

	#endregion
}