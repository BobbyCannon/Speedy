#region References

using System.Runtime.Versioning;

#endregion

namespace Speedy.Application.Internal.Windows;

#if (NET6_0_OR_GREATER)
[SupportedOSPlatform("windows")]
#endif
internal class DeviceModelWmiComponent : WmiComponent
{
	#region Constructors

	public DeviceModelWmiComponent()
		: base("Win32_ComputerSystem", "Model")
	{
	}

	#endregion
}