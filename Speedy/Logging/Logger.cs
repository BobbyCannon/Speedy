#region References

using System;
using System.Diagnostics.Tracing;

#endregion

namespace Speedy.Logging
{
	/// <summary>
	/// Represents a logger for Speedy.
	/// </summary>
	[EventSource(Name = LoggerName, Guid = LoggerGuid)]
	public sealed class Logger : EventSource
	{
		#region Constants

		/// <summary>
		/// The GUID for the logger.
		/// </summary>
		public const string LoggerGuid = "D39D8F77-93AE-482F-9A63-42AD4A26F453";

		/// <summary>
		/// The Name for the logger.
		/// </summary>
		public const string LoggerName = "Speedy.Logger";

		#endregion

		#region Constructors

		static Logger()
		{
			Instance = new Logger();
		}

		private Logger()
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the global instance of the logger.
		/// </summary>
		public static Logger Instance { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Write a message to the log.
		/// </summary>
		/// <param name="sessionId"> The ID of the session this message is for. </param>
		/// <param name="message"> The message to be written. </param>
		/// <param name="level"> The level of this message. </param>
		[NonEvent]
		public void Write(Guid sessionId, string message, EventLevel level = EventLevel.Informational)
		{
			switch (level)
			{
				case EventLevel.LogAlways:
				case EventLevel.Critical:
					Critical(sessionId, message, TimeService.UtcNow);
					return;

				case EventLevel.Error:
					Error(sessionId, message, TimeService.UtcNow);
					return;

				case EventLevel.Informational:
					Information(sessionId, message, TimeService.UtcNow);
					return;

				case EventLevel.Verbose:
					Verbose(sessionId, message, TimeService.UtcNow);
					return;

				case EventLevel.Warning:
					Warning(sessionId, message, TimeService.UtcNow);
					return;
			}
		}

		[Event((int) EventLevel.Critical, Message = "{1}", Level = EventLevel.Critical)]
		private void Critical(Guid sessionId, string message, DateTime time)
		{
			WriteEvent((int) EventLevel.Critical, sessionId, message, time);
		}

		[Event((int) EventLevel.Error, Message = "{1}", Level = EventLevel.Error)]
		private void Error(Guid sessionId, string message, DateTime time)
		{
			WriteEvent((int) EventLevel.Error, sessionId, message, time);
		}

		[Event((int) EventLevel.Informational, Message = "{1}", Level = EventLevel.Informational)]
		private void Information(Guid sessionId, string message, DateTime time)
		{
			WriteEvent((int) EventLevel.Informational, sessionId, message, time);
		}

		[Event((int) EventLevel.Verbose, Message = "{1}", Level = EventLevel.Verbose)]
		private void Verbose(Guid sessionId, string message, DateTime time)
		{
			WriteEvent((int) EventLevel.Verbose, sessionId, message, time);
		}

		[Event((int) EventLevel.Warning, Message = "{1}", Level = EventLevel.Warning)]
		private void Warning(Guid sessionId, string message, DateTime time)
		{
			WriteEvent((int) EventLevel.Warning, sessionId, message, time);
		}

		#endregion
	}
}