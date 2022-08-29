#region References

using System;
using System.Diagnostics;
using Speedy.Automation.Desktop;
using Speedy.Automation.Desktop.Elements;

#endregion

namespace Speedy.Automation.Internal
{
	internal static class ApplicationFrameHostManager
	{
		#region Methods

		public static IntPtr Refresh(SafeProcess process, TimeSpan? timeout = null)
		{
			var handle = IntPtr.Zero;

			Utility.Wait(() =>
				{
					var frameHosts = Process.GetProcessesByName("ApplicationFrameHost");

					foreach (var host in frameHosts)
					{
						using var application = new Application(host);
						application.Refresh();

						foreach (var c in application.Children)
						{
							if (!(c is Window window))
							{
								continue;
							}

							foreach (var cc in c.Children)
							{
								if (!(cc is Window ww))
								{
									continue;
								}

								if (ww.NativeElement.CurrentProcessId != process.Id)
								{
									continue;
								}

								ww.Dispose();
								handle = window.Handle;
								return true;
							}
						}
					}

					return false;
				},
				(timeout ?? TimeSpan.Zero).TotalMilliseconds,
				25);

			return handle;
		}

		#endregion
	}
}