namespace Speedy.Application.Wpf.Internal
{
	/// <summary>
	/// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-computersystem
	/// </summary>
	internal class ManufacturerDeviceIdComponent : ManagementObjectDeviceIdComponent
	{
		#region Constructors

		public ManufacturerDeviceIdComponent()
			: base("Win32_ComputerSystem", "Manufacturer")
		{
		}

		#endregion
	}
}