#region References

using System;
using System.IO;
using System.Reflection;
using System.Text;
using Speedy.Configuration.CommandLine;

#endregion

namespace Speedy.ServiceHosting
{
	/// <summary>
	/// Represents the options for a windows service.
	/// </summary>
	public class WindowsServiceOptions : CommandLineParser
	{
		#region Constructors

		/// <summary>
		/// Instantiates the options for a windows service.
		/// </summary>
		public WindowsServiceOptions()
		{
		}

		/// <summary>
		/// Instantiates the options for a windows service.
		/// </summary>
		public WindowsServiceOptions(Guid serviceId, string serviceName, string serviceDisplayName)
		{
			ServiceId = serviceId;
			ServiceName = serviceName;
			ServiceDisplayName = serviceDisplayName;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Flag to indicate to install the service.
		/// </summary>
		public bool InstallService => Property(nameof(InstallService))?.WasFound ?? false;

		/// <summary>
		/// Gets the directory for the service.
		/// </summary>
		public string ServiceDirectory { get; private set; }

		/// <summary>
		/// Gets the display name of the service.
		/// </summary>
		public string ServiceDisplayName { get; }

		/// <summary>
		/// Gets the file name of the service.
		/// </summary>
		public string ServiceFileName { get; private set; }

		/// <summary>
		/// Gets the file path of the service.
		/// </summary>
		public string ServiceFilePath { get; private set; }

		/// <summary>
		/// The ID of the service. You can listen for the logs of this service using this ID and LogListener.
		/// </summary>
		public Guid ServiceId { get; }

		/// <summary>
		/// Gets the name of the service.
		/// </summary>
		public string ServiceName { get; }

		/// <summary>
		/// Gets the process name for the service.
		/// </summary>
		public string ServiceProcessName { get; private set; }

		/// <summary>
		/// Gets the version of the service.
		/// </summary>
		public string ServiceVersion { get; private set; }

		/// <summary>
		/// Show the help for the service.
		/// </summary>
		public bool ShowHelp => Property(nameof(ShowHelp))?.WasFound ?? false;

		/// <summary>
		/// Flag to indicate to uninstall the service.
		/// </summary>
		public bool UninstallService => Property(nameof(UninstallService))?.WasFound ?? false;

		/// <summary>
		/// Flag to indicate to use verbose logging.
		/// </summary>
		public bool VerboseLogging => Property(nameof(VerboseLogging))?.WasFound ?? false;

		/// <summary>
		/// Flag to indicate to wait for a debugger.
		/// </summary>
		public bool WaitForDebugger => Property(nameof(WaitForDebugger))?.WasFound ?? false;

		#endregion

		#region Methods

		/// <inheritdoc />
		public override string BuildHelpInformation(StringBuilder builder = null, Func<CommandLineArgument, bool> shouldIncludeCheck = null)
		{
			builder ??= new StringBuilder();
			builder.AppendLine($"{ServiceDisplayName} {ServiceVersion}");
			return base.BuildHelpInformation(builder, shouldIncludeCheck);
		}
		
		/// <inheritdoc />
		public override string BuildIssueInformation(StringBuilder builder = null)
		{
			builder ??= new StringBuilder();
			builder.AppendLine($"{ServiceDisplayName} {ServiceVersion}");
			return base.BuildIssueInformation(builder);
		}

		/// <summary>
		/// Initialize the service options.
		/// </summary>
		public void Initialize(params string[] arguments)
		{
			var assembly = Assembly.GetCallingAssembly();
			Initialize(assembly);
			SetupArguments();
			Parse(arguments);
		}

		/// <summary>
		/// Setup arguments for a windows service.
		/// </summary>
		public virtual void SetupArguments()
		{
			Add(new WindowsServiceArgument
			{
				Help = "Install the service.",
				IncludeInServiceArguments = false,
				IsFlag = true,
				Name = "i",
				PropertyName = nameof(InstallService)
			});
			Add(new WindowsServiceArgument
			{
				Help = "Uninstall the service.",
				IncludeInServiceArguments = false,
				IsFlag = true,
				Name = "u",
				PropertyName = nameof(UninstallService)
			});
			Add(new WindowsServiceArgument
			{
				Help = "Wait for the debugger.",
				IsFlag = true,
				Name = "d",
				PropertyName = nameof(WaitForDebugger)
			});
			Add(new CommandLineArgument
			{
				Help = "Verbose logging.",
				IsFlag = true,
				Name = "v",
				PropertyName = nameof(VerboseLogging)
			});
			Add(new WindowsServiceArgument
			{
				Help = "Print out the help menu.",
				IncludeInServiceArguments = false,
				IsFlag = true,
				Name = "h",
				PropertyName = nameof(ShowHelp)
			});
		}

		/// <summary>
		/// Gets the string for the service.
		/// </summary>
		/// <returns> The string for the service. </returns>
		public string ToServiceString()
		{
			var builder = new StringBuilder();

			foreach (var argument in this)
			{
				if (argument is WindowsServiceArgument { IncludeInServiceArguments: false })
				{
					continue;
				}

				if (!argument.WasFound)
				{
					continue;
				}

				builder.Append(" ");
				builder.Append(argument);
			}

			foreach (var argument in UnknownArguments)
			{
				builder.Append(" ");
				builder.Append(argument.Key);

				if (!string.IsNullOrWhiteSpace(argument.Value))
				{
					builder.Append(" ");
					builder.Append(argument.Value);
				}
			}

			if (builder.Length > 0)
			{
				builder.Remove(0, 1);
			}

			return builder.ToString();
		}

		/// <summary>
		/// Initialize the service options.
		/// </summary>
		private void Initialize(Assembly assembly)
		{
			ServiceFilePath = assembly.Location;

			// .Net 5/6 (core) ends with dll but that is not correct.
			if (ServiceFilePath.ToLower().EndsWith(".dll"))
			{
				ServiceFilePath = $"{ServiceFilePath.Substring(0, ServiceFilePath.Length - 4)}.exe";
			}

			ServiceFileName = Path.GetFileName(ServiceFilePath);
			ServiceDirectory = Path.GetDirectoryName(ServiceFilePath);
			ServiceProcessName = Path.GetFileNameWithoutExtension(ServiceFilePath);
			ServiceVersion = assembly.GetName().Version.ToString();
		}

		#endregion
	}
}