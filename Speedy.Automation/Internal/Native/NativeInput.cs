#region References

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Speedy.Automation.Internal.Inputs;

#endregion

namespace Speedy.Automation.Internal.Native
{
	internal class NativeInput
	{
		#region Methods

		[DllImport("User32.dll")]
		public static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref KeyboardHookStruct lParam);

		[DllImport("user32.dll", EntryPoint = "GetCursorPos", SetLastError = true)]
		public static extern bool GetCursorPosition(out Point lpMousePoint);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern short GetKeyState(ushort virtualKeyCode);

		[DllImport("user32.dll")]
		public static extern IntPtr GetMessageExtraInfo();

		[DllImport("user32.dll")]
		public static extern uint MapVirtualKey(uint uCode, uint uMapType);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint SendInput(uint numberOfInputs, InputTypeWithData[] inputs, int sizeOfInputStructure);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "SetCursorPos")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetCursorPosition(int x, int y);

		[DllImport("User32.dll")]
		public static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookDelegate callback, IntPtr hInstance, uint threadId);

		[DllImport("User32.dll")]
		public static extern IntPtr UnhookWindowsHookEx(IntPtr hHook);

		[DllImport("user32.dll")]
		public static extern short VkKeyScan(char ch);

		#endregion

		#region Structures

		public struct KeyboardHookStruct
		{
			public int vkCode;
			public int scanCode;
			public int flags;
			public int time;
			public int dwExtraInfo;
		}

		#endregion

		#region Delegates

		public delegate int KeyboardHookDelegate(int code, int wParam, ref KeyboardHookStruct lParam);

		#endregion
	}
}