#region References

using System;

#endregion

namespace Speedy.Automation.Desktop
{
	/// <summary>
	/// The keyboard modifier.
	/// </summary>
	[Flags]
	public enum KeyboardModifier
	{
		/// <summary>
		/// No modifier
		/// </summary>
		None = 0,

		/// <summary>
		/// SHIFT key
		/// </summary>
		Shift = 0x0001,

		/// <summary>
		/// CTRL key
		/// </summary>
		Control = 0x0002,

		/// <summary>
		/// ALT key
		/// </summary>
		Alt = 0x0004,

		//
		// L* & R* - left and right Alt, Ctrl and Shift virtual keys.
		// Used only as parameters to GetAsyncKeyState() and GetKeyState().
		// No other API or message will distinguish left and right keys in this way.
		//

		/// <summary>
		/// Left SHIFT key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
		/// </summary>
		LeftShift = 0x0008,

		/// <summary>
		/// Right SHIFT key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
		/// </summary>
		RightShift = 0x0010,

		/// <summary>
		/// Left CONTROL key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
		/// </summary>
		LeftControl = 0x0020,

		/// <summary>
		/// Right CONTROL key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
		/// </summary>
		RightControl = 0x0040,

		/// <summary>
		/// Left ALT/MENU key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
		/// </summary>
		LeftAlt = 0x0080,

		/// <summary>
		/// Right ALT/MENU key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
		/// </summary>
		RightAlt = 0x0100
	}
}