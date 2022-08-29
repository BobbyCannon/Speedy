#region References

using System;
using System.Drawing;
using System.Runtime.InteropServices;

#endregion

namespace Speedy.Automation.Internal.Native
{
	/// <summary>
	/// References all of the Native Windows API methods for the WindowsInput functionality.
	/// </summary>
	internal static class NativeGeneral
	{
		#region Fields

		private static readonly IntPtr _notTopMost = new IntPtr(-2);
		private static readonly IntPtr _topMost = new IntPtr(-1);

		#endregion

		#region Methods

		public static void BringToTop(IntPtr handle)
		{
			SetWindowPos(handle, _notTopMost, 0, 0, 0, 0, SetWindowPosFlags.NoMove | SetWindowPosFlags.NoSize);
			SetWindowPos(handle, _topMost, 0, 0, 0, 0, SetWindowPosFlags.NoMove | SetWindowPosFlags.NoSize);
			SetWindowPos(handle, _notTopMost, 0, 0, 0, 0, SetWindowPosFlags.NoMove | SetWindowPosFlags.NoSize);
		}

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool BringWindowToTop(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetParent(IntPtr hWnd);

		public static WindowPlacement GetWindowPlacement(IntPtr handle)
		{
			var placement = new WindowPlacement();
			placement.length = Marshal.SizeOf(placement);
			GetWindowPlacement(handle, ref placement);
			return placement;
		}

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

		public static bool IsElevated(IntPtr handle)
		{
			const uint tokenQuery = 0x0008;

			if (!OpenProcessToken(handle, tokenQuery, out var hToken))
			{
				var error = Marshal.GetLastWin32Error();
				throw new Exception($"{error}: Failed to access the process token.");
			}

			var pElevationType = Marshal.AllocHGlobal(sizeof(TOKEN_ELEVATION_TYPE));
			GetTokenInformation(hToken, TokenInformationClass.TokenElevationType, pElevationType, sizeof(TOKEN_ELEVATION_TYPE), out var dwSize);
			var elevationType = (TOKEN_ELEVATION_TYPE) Marshal.ReadInt32(pElevationType);
			Marshal.FreeHGlobal(pElevationType);

			switch (elevationType)
			{
				case TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault:
					//Console.WriteLine("\nTokenElevationTypeDefault - User is not using a split token.\n");
					return false;

				case TOKEN_ELEVATION_TYPE.TokenElevationTypeFull:
					//Console.WriteLine("\nTokenElevationTypeFull - User has a split token, and the process is running elevated.\n");
					return true;

				case TOKEN_ELEVATION_TYPE.TokenElevationTypeLimited:
					//Console.WriteLine("\nTokenElevationTypeLimited - User has a split token, but the process is not running elevated.\n");
					return false;

				default:
					return false;
			}
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool isX86);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool SetFocus(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool GetTokenInformation(IntPtr TokenHandle, TokenInformationClass TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool GetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpwndpl);

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SetWindowPosFlags uFlags);

		#endregion

		#region Structures

		[StructLayout(LayoutKind.Sequential)]
		public struct Point
		{
			#region Fields

			public int X;
			public int Y;

			#endregion
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Rect
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;

			public Rect(int left, int top, int right, int bottom)
			{
				Left = left;
				Top = top;
				Right = right;
				Bottom = bottom;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WindowPlacement
		{
			public int length;
			public int flags;
			public int ShowState;
			public Point ptMinPosition;
			public Point ptMaxPosition;
			public Rectangle rcNormalPosition;
		}

		#endregion

		#region Enumerations

		[Flags]
		public enum SetWindowPosFlags : uint
		{
			/// <summary>
			/// Retains the current position (ignores X and Y parameters).
			/// </summary>
			NoMove = 0x0002,

			/// <summary>
			/// Retains the current size (ignores the cx and cy parameters).
			/// </summary>
			NoSize = 0x0001
		}

		// Define other methods and classes here
		private enum TOKEN_ELEVATION_TYPE
		{
			TokenElevationTypeDefault = 1,
			TokenElevationTypeFull,
			TokenElevationTypeLimited
		}

		private enum TokenInformationClass
		{
			TokenUser = 1,
			TokenGroups,
			TokenPrivileges,
			TokenOwner,
			TokenPrimaryGroup,
			TokenDefaultDacl,
			TokenSource,
			TokenType,
			TokenImpersonationLevel,
			TokenStatistics,
			TokenRestrictedSids,
			TokenSessionId,
			TokenGroupsAndPrivileges,
			TokenSessionReference,
			TokenSandBoxInert,
			TokenAuditPolicy,
			TokenOrigin,
			TokenElevationType,
			TokenLinkedToken,
			TokenElevation,
			TokenHasRestrictions,
			TokenAccessInformation,
			TokenVirtualizationAllowed,
			TokenVirtualizationEnabled,
			TokenIntegrityLevel,
			TokenUIAccess,
			TokenMandatoryPolicy,
			TokenLogonSid,
			MaxTokenInfoClass
		}

		#endregion

		#region Delegates

		public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

		#endregion
	}
}