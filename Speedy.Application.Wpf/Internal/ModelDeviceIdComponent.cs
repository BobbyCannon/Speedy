namespace Speedy.Application.Wpf.Internal
{
	/// <summary>
	/// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-computersystem
	/// </summary>
	internal class ModelDeviceIdComponent : ManagementObjectDeviceIdComponent
	{
		#region Constructors

		public ModelDeviceIdComponent()
			: base("Win32_ComputerSystem", "Model")
		{
		}

		#endregion
	}
}