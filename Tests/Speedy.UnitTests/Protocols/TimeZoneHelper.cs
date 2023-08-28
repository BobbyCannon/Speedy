#region References

using System;
using System.Diagnostics;

#endregion

namespace Speedy.UnitTests.Protocols
{
	public static class TimeZoneHelper
	{
		#region Methods

		public static string GetSystemTimeZone()
		{
			var process = Process.Start(new ProcessStartInfo
			{
				FileName = "tzutil.exe",
				Arguments = "/g",
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true
			});

			if (process == null)
			{
				return string.Empty;
			}
			process.WaitForExit();
			var output = process.StandardOutput.ReadToEnd();
			TimeZoneInfo.ClearCachedData();
			return output;
		}

		public static void SetSystemTimeZone(string timeZoneId)
		{
			var process = Process.Start(new ProcessStartInfo
			{
				FileName = "tzutil.exe",
				Arguments = "/s \"" + timeZoneId + "\"",
				UseShellExecute = false,
				CreateNoWindow = true
			});

			if (process != null)
			{
				process.WaitForExit();
				TimeZoneInfo.ClearCachedData();
			}
		}

		#endregion
	}
}