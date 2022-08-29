#region References

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using static Speedy.Automation.Internal.Native.NativeScreen;

#endregion

namespace Speedy.Automation.Desktop
{
	/// <summary>
	/// Represents a display device or multiple display devices on a single system.
	/// </summary>
	public class Screen
	{
		#region Fields

		private static Screen[] _screens;

		#endregion

		#region Constructors

		internal Screen(IntPtr monitor)
		{
			var info = new MONITORINFOEX();
			GetMonitorInfo(new HandleRef(null, monitor), info);

			DeviceName = new string(info.szDevice).TrimEnd((char) 0);
			IsPrimary = (info.dwFlags & 0x1) != 0;
			Location = new Point(info.rcMonitor.Left, info.rcMonitor.Top);
			Size = new Size(info.rcMonitor.Right - info.rcMonitor.Left, info.rcMonitor.Bottom - info.rcMonitor.Top);
			ScreenArea = new Rectangle(Location, Size);
			WorkingArea = new Rectangle(info.rcWork.Left, info.rcWork.Top, info.rcWork.Right - info.rcWork.Left, info.rcWork.Bottom - info.rcWork.Top);

			if (IsPrimary)
			{
				PrimaryScreen = this;
			}
		}

		static Screen()
		{
			Refresh();
		}

		#endregion

		#region Properties

		/// <summary>
		/// All screens that were enumerated. See <see cref="Refresh" /> to update.
		/// </summary>
		public static IEnumerable<Screen> AllScreens => _screens;

		/// <summary>
		/// Gets the device name.
		/// </summary>
		public string DeviceName { get; }

		/// <summary>
		/// The screen is the primary screen.
		/// </summary>
		public bool IsPrimary { get; }

		/// <summary>
		/// Gets the location of the screen.
		/// </summary>
		public Point Location { get; }

		/// <summary>
		/// Returns true if multiple screens are supported.
		/// </summary>
		public static bool MultipleScreenSupport => (uint) GetSystemMetrics(80) > 0U;

		/// <summary>
		/// Gets the primary screen.
		/// </summary>
		public static Screen PrimaryScreen { get; private set; }

		/// <summary>
		/// Gets the bounds of the monitor.
		/// </summary>
		public Rectangle ScreenArea { get; }

		/// <summary>
		/// Gets the size of the screen.
		/// </summary>
		public Size Size { get; }

		/// <summary> Gets the bounds of the virtual screen. </summary>
		/// <returns> A <see cref="T:System.Drawing.Rectangle" /> that specifies the bounding rectangle of the entire virtual screen. </returns>
		public static Rectangle VirtualScreenSize =>
			MultipleScreenSupport
				// VirtualScreen X, Y, Width, Height
				? new Rectangle(GetSystemMetrics(76), GetSystemMetrics(77), GetSystemMetrics(78), GetSystemMetrics(79))
				: PrimaryScreen.ScreenArea;

		/// <summary>
		/// Gets the bounds of the monitor.
		/// </summary>
		public Rectangle WorkingArea { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Gets the screen from the provided point.
		/// </summary>
		/// <param name="point"> The point that is in the screen you want. </param>
		/// <returns> The screen that contains the point. </returns>
		public static Screen FromPoint(Point point)
		{
			return _screens.FirstOrDefault(screen => screen.ScreenArea.Contains(point));
		}

		/// <summary>
		/// Gets the screen from the provided point.
		/// </summary>
		/// <param name="x"> The x location that is in the screen you want. </param>
		/// <param name="y"> The y location that is in the screen you want. </param>
		/// <returns> The screen that contains the point. </returns>
		public static Screen FromPoint(int x, int y)
		{
			return _screens.FirstOrDefault(screen => screen.ScreenArea.Contains(x, y));
		}

		/// <summary>
		/// Refresh the <see cref="AllScreens" /> collection.
		/// </summary>
		public static void Refresh()
		{
			var monitorEnumCallback = new MonitorEnumCallback();
			var monitorEnumProc = new MonitorEnumProc(monitorEnumCallback.Callback);
			EnumDisplayMonitors(NullHandleRef, null, monitorEnumProc, IntPtr.Zero);

			var screenArray = new Screen[monitorEnumCallback.screens.Count];
			monitorEnumCallback.screens.CopyTo(screenArray, 0);

			_screens = screenArray;
		}

		#endregion
	}
}