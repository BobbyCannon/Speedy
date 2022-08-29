#region References

using System;
using System.Drawing;
using System.Linq;
using Speedy.Automation.Desktop;
using Speedy.Automation.Web;

#endregion

namespace Speedy.Automation
{
	/// <summary>
	/// Container for all extension methods.
	/// </summary>
	public static class Extensions
	{
		#region Methods

		/// <summary>
		/// Returns the number of browsers for this type.
		/// </summary>
		/// <param name="type"> The browser type that contains the configuration. </param>
		/// <returns> The number of browsers configured in the type. </returns>
		public static int Count(this BrowserType type)
		{
			return type.GetTypeArray().Length;
		}

		/// <summary>
		/// Run a test against each browser. BrowserType property will determine which browsers to run the test against.
		/// </summary>
		/// <param name="browserType"> The browser types to run the action against. </param>
		/// <param name="action"> The action to run each browser against. </param>
		/// <param name="useSecondaryMonitor"> The flag to determine to attempt to use secondary monitor. </param>
		/// <param name="resizeBrowsers"> The flag to determine to resize the browsers. </param>
		/// <param name="resizeType"> The resize type if browser resizing is enabled. </param>
		/// <param name="timeout"> The timeout in milliseconds. </param>
		/// <seealso cref="BrowserType" />
		/// <seealso cref="BrowserResizeType" />
		public static void ForAllBrowsers(this BrowserType browserType, Action<Browser> action, bool useSecondaryMonitor = false, bool resizeBrowsers = true, BrowserResizeType resizeType = BrowserResizeType.SideBySide, int timeout = 30000)
		{
			var browserTypes = browserType.GetTypeArray();
			var size = CalculateBrowserSize(useSecondaryMonitor, browserTypes, resizeType, out var leftOffset, out var topOffset);

			Browser.ForAllBrowsers(browser =>
			{
				try
				{
					if (resizeBrowsers)
					{
						var offset = CalculateStart(resizeType, browserTypes, browser, leftOffset, size, topOffset);
						//Debug.WriteLine($"{offset.X}, {offset.Y}, {size.Width}, {size.Height}");
						browser.MoveWindow(offset.X, offset.Y, size.Width, size.Height);
						browser.Application.MoveWindow(offset.X, offset.Y, size.Width, size.Height);
					}

					browser.BringToFront();
					action(browser);
				}
				catch (Exception ex)
				{
					throw new Exception($"Test failed using {browser.BrowserType}.", ex);
				}
			}, browserType, TimeSpan.FromMilliseconds(timeout));
		}

		/// <summary>
		/// Run a test against each browser. BrowserType property will determine which browsers to run the test against.
		/// </summary>
		/// <param name="browserType"> The browser types to run the action against. </param>
		/// <param name="action"> The action to run each browser against. </param>
		/// <param name="useSecondaryMonitor"> The flag to determine to attempt to use secondary monitor. </param>
		/// <param name="resizeBrowsers"> The flag to determine to resize the browsers. </param>
		/// <param name="resizeType"> The resize type if browser resizing is enabled. </param>
		/// <seealso cref="BrowserType" />
		/// <seealso cref="BrowserResizeType" />
		public static void ForEachBrowser(this BrowserType browserType, Action<Browser> action, bool useSecondaryMonitor = false, bool resizeBrowsers = true, BrowserResizeType resizeType = BrowserResizeType.SideBySide)
		{
			var browserTypes = browserType.GetTypeArray();
			var size = CalculateBrowserSize(useSecondaryMonitor, browserTypes, resizeType, out var leftOffset, out var topOffset);

			Browser.ForEachBrowser(browser =>
			{
				try
				{
					if (resizeBrowsers)
					{
						var offset = CalculateStart(resizeType, browserTypes, browser, leftOffset, size, topOffset);
						//Debug.WriteLine($"{offset.X}, {offset.Y}, {size.Width}, {size.Height}");
						browser.MoveWindow(offset.X, offset.Y, size.Width, size.Height);
						browser.Application.MoveWindow(offset.X, offset.Y, size.Width, size.Height);
					}

					browser.BringToFront();
					action(browser);
				}
				catch (Exception ex)
				{
					throw new Exception($"Test failed using {browser.BrowserType}.", ex);
				}
			}, browserType);
		}

		/// <summary>
		/// Converts a browser type enum into an array of only individual browser types.
		/// </summary>
		/// <param name="browserType"> The browser type to convert. </param>
		/// <returns> The individual browser type values in the provided type. </returns>
		public static BrowserType[] GetTypeArray(this BrowserType browserType)
		{
			var types = new[] { BrowserType.Chrome, BrowserType.Edge, BrowserType.Firefox };
			return types.Where(type => (browserType & type) == type).ToArray();
		}

		private static Size CalculateBrowserSize(bool useSecondaryMonitor, BrowserType[] browserTypes, BrowserResizeType resizeType, out int leftOffset, out int topOffset)
		{
			var screen = useSecondaryMonitor
				? Screen.AllScreens.FirstOrDefault(x => x.IsPrimary == false) ?? Screen.AllScreens.First(x => x.IsPrimary)
				: Screen.AllScreens.First(x => x.IsPrimary);

			topOffset = screen.WorkingArea.Top;

			switch (resizeType)
			{
				case BrowserResizeType.LeftSideBySide:
				case BrowserResizeType.RightSideBySide:
				{
					var horizontalCount = browserTypes.Length > 2 ? 2 : browserTypes.Length;
					var verticalCount = browserTypes.Length > 2 ? 2 : 1;
					var browserHeight = screen.WorkingArea.Height / verticalCount;
					var browserWidth = screen.WorkingArea.Width / 2 / horizontalCount;
					leftOffset = resizeType == BrowserResizeType.RightSideBySide
						? screen.WorkingArea.Left + (screen.WorkingArea.Width / 2)
						: screen.WorkingArea.Left;

					return new Size(browserWidth, browserHeight);
				}
				case BrowserResizeType.SideBySide:
				default:
				{
					var browserWidth = screen.WorkingArea.Width / browserTypes.Length;
					leftOffset = screen.WorkingArea.Left;
					return new Size(browserWidth, screen.WorkingArea.Height);
				}
			}
		}

		private static Point CalculateStart(BrowserResizeType resizeType, BrowserType[] browserTypes, Browser browser, int leftOffset, Size size, int topOffset)
		{
			var xOffset = Array.IndexOf(browserTypes, browser.BrowserType);
			var yOffset = 0;

			if (resizeType != BrowserResizeType.SideBySide)
			{
				yOffset = xOffset / 2;
			}

			if ((resizeType != BrowserResizeType.SideBySide) && (xOffset >= 2))
			{
				xOffset -= 2;
			}

			var x = leftOffset + (xOffset * size.Width);
			var y = topOffset + (yOffset * size.Height);
			return new Point(x, y);
		}

		#endregion
	}
}