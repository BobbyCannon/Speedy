#region References

using System;

#endregion

namespace Speedy.Mobile.Droid
{
	[Flags]
	public enum PermissionType
	{
		None = 0,
		Location = 0x0001,
		Camera = 0x0002,
		Bluetooth = 0x0004,
		Microphone = 0x0008,
		Network = 0x0010,
		Storage = 0x0020,
		Activity = 0x0040,
		All = 0xFFFF
	}
}