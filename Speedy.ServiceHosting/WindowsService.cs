#region References

using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.ServiceProcess;
using System.Threading;
using Speedy.Extensions;
using Speedy.Logging;
using Speedy.ServiceHosting.Internal;

#endregion

namespace Speedy.ServiceHosting
{
	/// <summary>
	/// Represents a windows service.
	/// </summary>
	public abstract class WindowsService : WindowsService<WindowsServiceOptions>
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the WindowsService class.
		/// </summary>
		protected WindowsService(WindowsServiceOptions options) : base(options)
		{
		}

		#endregion
	}

	/// <summary>
	/// Represents a windows service.
	/// </summary>
	public abstract class WindowsService<T> : ServiceBase
		where T : WindowsServiceOptions
	{
		#region Fields

		private readonly Thread _serviceThread;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the WindowsService class.
		/// </summary>
		protected WindowsService(T options)
		{
			Options = options;
			ServiceName = options.ServiceName;

			_serviceThread = new Thread(ServiceThread);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating if the service is running.
		/// </summary>
		public bool IsRunning { get; private set; }

		/// <summary>
		/// Gets a value indicating if the service is being trigger.
		/// </summary>
		public bool TriggerPending { get; protected set; }

		/// <summary>
		/// Gets the options for the service.
		/// </summary>
		protected T Options { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Allows public access to the OnStart method.
		/// </summary>
		public int Start()
		{
			// Show help if asked or if no argument were provided
			if (Options.ShowHelp)
			{
				WriteLine(Options.BuildHelpInformation());
				return 0;
			}

			if (!Options.IsValid)
			{
				WriteLine(Options.BuildIssueInformation());
				return -1;
			}

			WriteLine($"{Options.ServiceDisplayName} v{Options.ServiceVersion}");

			if (Options.InstallService)
			{
				return InstallService();
			}

			if (Options.UninstallService)
			{
				return UninstallService();
			}

			// Check to see if we need to run in service mode.
			if (!Environment.UserInteractive)
			{
				// Run the service in service mode.
				Run(this);
				return 0;
			}

			// Start the process in debug mode.
			OnStart(null);
			HandleConsole();
			return 0;
		}

		/// <summary>
		/// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system
		/// starts (for a service that starts automatically). Specifies actions to take when the service starts.
		/// </summary>
		/// <param name="args"> Data passed by the start command. </param>
		protected override void OnStart(string[] args)
		{
			WriteLine("Starting the service...");
			IsRunning = true;
			_serviceThread.Start();
			WriteLine("The service has started.");
			base.OnStart(args);
		}

		/// <summary>
		/// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when
		/// a service stops running.
		/// </summary>
		protected override void OnStop()
		{
			if (!IsRunning)
			{
				// Return because we are not running.
				return;
			}

			WriteLine("Stopping the service...");
			IsRunning = false;
			_serviceThread.Join(new TimeSpan(0, 1, 0));
			WriteLine("The service has stopped.");
			base.OnStop();
		}

		/// <summary>
		/// The thread for the service.
		/// </summary>
		protected abstract void Process();

		/// <summary>
		/// Puts the service to sleep for provided delay (in milliseconds). The service will be woke up if the service gets a request to close or to trigger the service.
		/// </summary>
		protected void Sleep(int delay)
		{
			Sleep(TimeSpan.FromMilliseconds(delay));
		}

		/// <summary>
		/// Puts the service to sleep for provided delay. The service will be woke up if the service gets a request to close or to trigger the service.
		/// </summary>
		protected void Sleep(TimeSpan delay)
		{
			var watch = Stopwatch.StartNew();

			while ((watch.Elapsed < delay) && IsRunning && !TriggerPending)
			{
				Thread.Sleep(50);
			}

			// Clear the pending trigger.
			TriggerPending = false;
		}

		/// <summary>
		/// Writes an message to the logger at a provided level.
		/// </summary>
		/// <param name="message"> The message to write. </param>
		/// <param name="level"> The level at which to write the message. </param>
		protected virtual void WriteLine(string message, EventLevel level = EventLevel.Informational)
		{
			Logger.Instance.Write(Options.ServiceId, message, level);

			if (EventLog.SourceExists(ServiceName))
			{
				EventLog.WriteEntry(ServiceName, message, ToEventLogEntryType(level));
			}
		}

		/// <summary>
		/// Grab the console and wait for it to close.
		/// </summary>
		private void HandleConsole()
		{
			// Redirects the 'X' for the console window so we can close the service cleanly.
			var stopDebugHandler = new NativeMethods.HandlerRoutine(OnStop);
			NativeMethods.SetConsoleCtrlHandler(stopDebugHandler, true);

			// Loop here while the service is running.
			while (IsRunning)
			{
				// Minor delay for process management.
				Thread.Sleep(50);

				// Check to see if someone pressed a key.
				if (Console.KeyAvailable)
				{
					// Check to see if the key was a space.
					if (Console.ReadKey(true).Key != ConsoleKey.Spacebar)
					{
						// It was not a space so break the running loop and close the service.
						break;
					}

					// Set the pending trigger flag. This allows the service to break out of a delay.
					TriggerPending = true;
				}
			}

			// It was not a space so break the running loop and close the service.
			OnStop();

			// If we don't have this the handler will get garbage collected and will result in a
			// null reference exception when the console windows is closed with the 'X'.
			GC.KeepAlive(stopDebugHandler);
		}

		/// <summary>
		/// Install the service as a windows service.
		/// </summary>
		private int InstallService()
		{
			try
			{
				WindowsServiceInstaller.InstallService(Options.ServiceFilePath, Options.ToServiceString(), ServiceName, Options.ServiceDisplayName, ServiceStartMode.Automatic);

				// Create the source, if it does not already exist.
				if (!EventLog.SourceExists(ServiceName))
				{
					// An event log source should not be created and immediately used. There is a latency time to enable the source, it should be created
					// prior to executing the application that uses the source. Install first then after that it can be used.
					EventLog.CreateEventSource(ServiceName, "Application");
				}

				return 0;
			}
			catch (Exception ex)
			{
				WriteLine(ex.ToDetailedString(), EventLevel.Critical);
				return -1;
			}
		}

		/// <summary>
		/// Internal management of service thread.
		/// </summary>
		private void ServiceThread()
		{
			try
			{
				// Run the windows service process.
				Process();
			}
			catch (Exception ex)
			{
				WriteLine(ex.ToDetailedString(), EventLevel.Critical);
			}
		}

		private EventLogEntryType ToEventLogEntryType(EventLevel level)
		{
			return level switch
			{
				EventLevel.Critical => EventLogEntryType.Error,
				EventLevel.Error => EventLogEntryType.Error,
				EventLevel.Warning => EventLogEntryType.Warning,
				EventLevel.Verbose => EventLogEntryType.Information,
				EventLevel.Informational => EventLogEntryType.Information,
				EventLevel.LogAlways => EventLogEntryType.Information,
				_ => EventLogEntryType.Information
			};
		}

		/// <summary>
		/// Uninstalls the service.
		/// </summary>
		private int UninstallService()
		{
			try
			{
				WindowsServiceInstaller.UninstallService(ServiceName);

				// If the source exists we want to remove it
				if (EventLog.SourceExists(ServiceName))
				{
					EventLog.DeleteEventSource(ServiceName);
				}

				return 0;
			}
			catch (Exception ex)
			{
				WriteLine(ex.ToDetailedString(), EventLevel.Critical);
				return -1;
			}
		}

		#endregion
	}
}