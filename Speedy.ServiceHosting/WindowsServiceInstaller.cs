#region References

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.ServiceProcess;
using System.Text;
using Speedy.Extensions;

#endregion

namespace Speedy.ServiceHosting
{
	[ExcludeFromCodeCoverage]
	internal static class WindowsServiceInstaller
	{
		#region Fields

		private static readonly string _systemDirectory = Environment.SystemDirectory;

		#endregion

		#region Methods

		/// <summary>
		/// Install an executable as a service.
		/// </summary>
		/// <param name="serviceFilePath"> The path to the service executable. </param>
		/// <param name="serviceArguments"> The arguments for the service. </param>
		/// <param name="serviceName"> The name of the service. </param>
		/// <param name="displayName"> THe display name of the service. </param>
		/// <param name="startType"> The startup type. </param>
		/// <param name="userName"> The username to run as. </param>
		/// <param name="password"> The password of the user. </param>
		public static void InstallService(string serviceFilePath, string serviceArguments, string serviceName, string displayName,
			ServiceStartMode startType, string userName = "", string password = "")
		{
			var serviceCommandLine = $"\"{serviceFilePath}\" {serviceArguments.Escape()}";
			var serviceControl = new Process();

			var serviceControlArguments = new StringBuilder();
			serviceControlArguments.Append($"create {serviceName}");
			serviceControlArguments.Append($" start= \"{ToCommandLineValue(startType)}\"");
			serviceControlArguments.Append($" binpath= \"{serviceCommandLine}\"");
			serviceControlArguments.Append($" displayname= \"{displayName}\"");

			if (!string.IsNullOrWhiteSpace(userName))
			{
				serviceControlArguments.Append($" obj= \"{userName}\"");
			}

			if (!string.IsNullOrWhiteSpace(password))
			{
				serviceControlArguments.Append($" password= \"{password}\"");
			}

			serviceControl.StartInfo.FileName = Path.Combine(_systemDirectory, "sc.exe");
			serviceControl.StartInfo.Arguments = serviceControlArguments.ToString();
			serviceControl.StartInfo.Verb = "runas";

			serviceControl.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			serviceControl.StartInfo.RedirectStandardOutput = true;
			serviceControl.StartInfo.UseShellExecute = false;

			serviceControl.Start();
			var results = serviceControl.StandardOutput.ReadToEnd();
			serviceControl.WaitForExit();

			if (serviceControl.ExitCode != 0)
			{
				throw new Exception($"Failed: {serviceControl.ExitCode}, {results}");
			}
		}

		/// <summary>
		/// Uninstall a service by name.
		/// </summary>
		/// <param name="serviceName"> The name of the service. </param>
		public static void UninstallService(string serviceName)
		{
			using var serviceControl = new Process();
			serviceControl.StartInfo.FileName = Path.Combine(_systemDirectory, "sc.exe");
			serviceControl.StartInfo.Arguments = $"delete {serviceName}";
			serviceControl.StartInfo.Verb = "runas";

			serviceControl.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			serviceControl.StartInfo.RedirectStandardOutput = true;
			serviceControl.StartInfo.UseShellExecute = false;

			serviceControl.Start();
			var results = serviceControl.StandardOutput.ReadToEnd();
			serviceControl.WaitForExit();

			if (serviceControl.ExitCode != 0)
			{
				throw new Exception($"Failed: {serviceControl.ExitCode}, {results}");
			}
		}

		private static string ToCommandLineValue(ServiceStartMode startMode)
		{
			return startMode switch
			{
				ServiceStartMode.Boot => "boot",
				ServiceStartMode.System => "system",
				ServiceStartMode.Automatic => "auto",
				ServiceStartMode.Manual => "demand",
				ServiceStartMode.Disabled => "disabled",
				_ => "auto"
			};
		}

		#endregion
	}
}