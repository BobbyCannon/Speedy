#region References

using System;
using System.Collections;
using System.Runtime.InteropServices;
using Speedy.Automation.Desktop;

#endregion

namespace Speedy.Automation.Internal.Native
{
	internal static class NativeScreen
	{
		#region Constants

		public const int CCHDEVICENAME = 32;
		public const int CCHFORMNAME = 32;

		#endregion

		#region Fields

		public static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

		#endregion

		#region Methods

		[DllImport("user32.dll")]
		public static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

		[DllImport("user32.dll")]
		public static extern int ChangeDisplaySettingsEx(string lpszDeviceName, [In] ref DEVMODE lpDevMode, IntPtr hwnd, int dwFlags, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern bool EnumDisplayDevices(string lpDevice, int iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, int dwFlags);

		[DllImport("user32.dll")]
		public static extern bool EnumDisplayMonitors(HandleRef hdc, COMRECT rcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

		[DllImport("user32.dll")]
		public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool GetMonitorInfo(HandleRef hmonitor, [In] [Out] MONITORINFOEX info);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int GetSystemMetrics(int nIndex);

		#endregion

		#region Classes

		[StructLayout(LayoutKind.Sequential)]
		public class COMRECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		public class MonitorEnumCallback
		{
			#region Fields

			public readonly ArrayList screens = new ArrayList();

			#endregion

			#region Methods

			public virtual bool Callback(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lparam)
			{
				screens.Add(new Screen(monitor));
				return true;
			}

			#endregion
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
		public class MONITORINFOEX
		{
			public int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
			public NativeGeneral.Rect rcMonitor = new NativeGeneral.Rect();
			public NativeGeneral.Rect rcWork = new NativeGeneral.Rect();
			public int dwFlags = 0;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public char[] szDevice = new char[32];
		}

		#endregion

		#region Structures

		[StructLayout(LayoutKind.Sequential)]
		public struct POINTL
		{
			[MarshalAs(UnmanagedType.I4)]
			public int x;

			[MarshalAs(UnmanagedType.I4)]
			public int y;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct DEVMODE
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string dmDeviceName;

			[MarshalAs(UnmanagedType.U2)]
			public ushort dmSpecVersion;

			[MarshalAs(UnmanagedType.U2)]
			public ushort dmDriverVersion;

			[MarshalAs(UnmanagedType.U2)]
			public ushort dmSize;

			[MarshalAs(UnmanagedType.U2)]
			public ushort dmDriverExtra;

			[MarshalAs(UnmanagedType.U4)]
			public DEVMODE_Flags dmFields;

			public POINTL dmPosition;

			[MarshalAs(UnmanagedType.U4)]
			public uint dmDisplayOrientation;

			[MarshalAs(UnmanagedType.U4)]
			public uint dmDisplayFixedOutput;

			[MarshalAs(UnmanagedType.I2)]
			public short dmColor;

			[MarshalAs(UnmanagedType.I2)]
			public short dmDuplex;

			[MarshalAs(UnmanagedType.I2)]
			public short dmYResolution;

			[MarshalAs(UnmanagedType.I2)]
			public short dmTTOption;

			[MarshalAs(UnmanagedType.I2)]
			public short dmCollate;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string dmFormName;

			[MarshalAs(UnmanagedType.U2)]
			public ushort dmLogPixels;

			[MarshalAs(UnmanagedType.U4)]
			public uint dmBitsPerPel;

			[MarshalAs(UnmanagedType.U4)]
			public uint dmPelsWidth;

			[MarshalAs(UnmanagedType.U4)]
			public uint dmPelsHeight;

			[MarshalAs(UnmanagedType.U4)]
			public uint dmDisplayFlags;

			[MarshalAs(UnmanagedType.U4)]
			public uint dmDisplayFrequency;

			[MarshalAs(UnmanagedType.U4)]
			public uint dmICMMethod;

			[MarshalAs(UnmanagedType.U4)]
			public uint dmICMIntent;

			[MarshalAs(UnmanagedType.U4)]
			public uint dmMediaType;

			[MarshalAs(UnmanagedType.U4)]
			public uint dmDitherType;

			[MarshalAs(UnmanagedType.U4)]
			public uint dmReserved1;

			[MarshalAs(UnmanagedType.U4)]
			public uint dmReserved2;

			[MarshalAs(UnmanagedType.U4)]
			public uint dmPanningWidth;

			[MarshalAs(UnmanagedType.U4)]
			public uint dmPanningHeight;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct DISPLAY_DEVICE
		{
			[MarshalAs(UnmanagedType.U4)]
			public int cb;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string DeviceName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string DeviceString;

			[MarshalAs(UnmanagedType.U4)]
			public DisplayDeviceStateFlags StateFlags;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string DeviceID;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string DeviceKey;
		}

		#endregion

		#region Enumerations

		public enum DeviceFlags
		{
			CDS_FULLSCREEN = 0x4,
			CDS_GLOBAL = 0x8,
			CDS_NORESET = 0x10000000,
			CDS_RESET = 0x40000000,
			CDS_SET_PRIMARY = 0x10,
			CDS_TEST = 0x2,
			CDS_UPDATEREGISTRY = 0x1,
			CDS_VIDEOPARAMETERS = 0x20
		}

		public enum DEVMODE_Flags
		{
			DM_BITSPERPEL = 0x40000,
			DM_DISPLAYFLAGS = 0x200000,
			DM_DISPLAYFREQUENCY = 0x400000,
			DM_PELSHEIGHT = 0x100000,
			DM_PELSWIDTH = 0x80000,
			DM_POSITION = 0x20
		}

		public enum DEVMODE_SETTINGS
		{
			ENUM_CURRENT_SETTINGS = -1,
			ENUM_REGISTRY_SETTINGS = -2
		}

		public enum Display_Device_Stateflags
		{
			DISPLAY_DEVICE_ATTACHED_TO_DESKTOP = 0x1,
			DISPLAY_DEVICE_MIRRORING_DRIVER = 0x8,
			DISPLAY_DEVICE_MODESPRUNED = 0x8000000,
			DISPLAY_DEVICE_MULTI_DRIVER = 0x2,
			DISPLAY_DEVICE_PRIMARY_DEVICE = 0x4,
			DISPLAY_DEVICE_VGA_COMPATIBLE = 0x10
		}

		[Flags]
		public enum DisplayDeviceStateFlags
		{
			/// <summary> The device is part of the desktop. </summary>
			AttachedToDesktop = 0x1,
			MultiDriver = 0x2,

			/// <summary> The device is part of the desktop. </summary>
			PrimaryDevice = 0x4,

			/// <summary> Represents a pseudo device used to mirror application drawing for remoting or other purposes. </summary>
			MirroringDriver = 0x8,

			/// <summary> The device is VGA compatible. </summary>
			VGACompatible = 0x10,

			/// <summary> The device is removable; it cannot be the primary display. </summary>
			Removable = 0x20,

			/// <summary> The device has more display modes than its output devices support. </summary>
			ModesPruned = 0x8000000,
			Remote = 0x4000000,
			Disconnect = 0x2000000
		}

		public enum DisplaySetting_Results
		{
			DISP_CHANGE_BADFLAGS = -4,
			DISP_CHANGE_BADMODE = -2,
			DISP_CHANGE_BADPARAM = -5,
			DISP_CHANGE_FAILED = -1,
			DISP_CHANGE_NOTUPDATED = -3,
			DISP_CHANGE_RESTART = 1,
			DISP_CHANGE_SUCCESSFUL = 0
		}

		#endregion

		#region Delegates

		public delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lParam);

		#endregion
	}
}