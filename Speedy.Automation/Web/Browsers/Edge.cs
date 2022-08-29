#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Speedy.Exceptions;

#endregion

namespace Speedy.Automation.Web.Browsers
{
	/// <summary>
	/// Represents an Edge browser.
	/// </summary>
	public class Edge : ChromiumBrowser
	{
		#region Constants

		/// <summary>
		/// The name of the browser.
		/// </summary>
		public const string BrowserName = "msedge.exe";

		/// <summary>
		/// The debug port for the browser.
		/// </summary>
		public const int DebugPort = 9223;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the Edge class.
		/// </summary>
		/// <param name="application"> The window of the existing browser. </param>
		/// <param name="windowsToIgnore"> The windows to ignore. Optional. </param>
		private Edge(Application application, ICollection<IntPtr> windowsToIgnore = null)
			: base(DebugPort, application, windowsToIgnore)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the type of the browser.
		/// </summary>
		public override BrowserType BrowserType => BrowserType.Edge;

		#endregion

		#region Methods

		/// <summary>
		/// Attempts to attach to an existing browser.
		/// </summary>
		/// <param name="bringToFront"> The option to bring the application to the front. This argument is optional and defaults to true. </param>
		/// <returns> The browser instance or null if not found. </returns>
		public static Browser Attach(bool bringToFront = true)
		{
			var application = Application.Attach(BrowserName, GetDebugArguments(DebugPort), false, bringToFront);
			if (application == null)
			{
				return null;
			}

			var browser = new Edge(application);
			browser.Connect();
			browser.Refresh();

			return browser;
		}

		/// <summary>
		/// Attempts to attach to an existing browser.
		/// </summary>
		/// <param name="process"> The process to attach to. </param>
		/// <param name="bringToFront"> The option to bring the application to the front. This argument is optional and defaults to true. </param>
		/// <returns> The browser instance or null if not found. </returns>
		public static Browser Attach(Process process, bool bringToFront = true)
		{
			if (process.ProcessName != BrowserName)
			{
				return null;
			}

			if (!Application.Exists(BrowserName, GetDebugArguments(DebugPort)))
			{
				throw new ArgumentException("The process was not started with the debug arguments.", nameof(process));
			}

			var application = Application.Attach(process, false, bringToFront);
			var browser = new Edge(application);
			browser.Connect();
			browser.Refresh();
			return browser;
		}

		/// <summary>
		/// Attempts to attach to an existing browser. If one is not found then create and return a new one.
		/// </summary>
		/// <param name="bringToFront"> The option to bring the application to the front. This argument is optional and defaults to true. </param>
		/// <returns> The browser instance. </returns>
		public static Browser AttachOrCreate(bool bringToFront = true)
		{
			return Attach(bringToFront) ?? Create(bringToFront);
		}

		/// <summary>
		/// Attempts to create a new browser. If one is not found then we'll make sure it was started with the
		/// remote debugger argument. If not an exception will be thrown.
		/// </summary>
		/// <param name="bringToFront"> The option to bring the application to the front. This argument is optional and defaults to true. </param>
		/// <returns> The browser instance. </returns>
		public static Browser Create(bool bringToFront = true)
		{
			if (Application.Exists(BrowserName) && !Application.Exists(BrowserName, GetDebugArguments(DebugPort)))
			{
				throw new SpeedyException("The first instance of Edge was not started with the remote debugger enabled.");
			}

			Edge browser;

			// See if edge is already running
			var application = Application.Attach(BrowserName, GetDebugArguments(DebugPort), false, bringToFront);
			if (application != null)
			{
				try
				{
					Application.Create(BrowserName, GetDebugArguments(DebugPort), false, bringToFront).Dispose();
				}
				catch
				{
					// Process may close during creation causing error.
				}

				browser = new Edge(application);
			}
			else
			{
				Application.Create(BrowserName, GetDebugArguments(DebugPort), false, bringToFront);
				application = Application.Attach(BrowserName, GetDebugArguments(DebugPort), false, bringToFront);
				browser = new Edge(application);
			}

			browser.Connect();
			browser.NavigateTo("about:blank");
			return browser;
		}

		#endregion
	}
}