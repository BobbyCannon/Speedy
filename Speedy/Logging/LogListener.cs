#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

#endregion

namespace Speedy.Logging
{
	/// <summary>
	/// Represents an example on how to listen to the logger.
	/// </summary>
	/// <remarks>
	/// LogListener must be in the same process as the logger. See other ETW examples on how
	/// to capture logger from outside the process.
	/// </remarks>
	public class LogListener : EventListener
	{
		#region Fields

		private readonly EventLevel _level;

		private readonly Guid _sessionId;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the log listener.
		/// </summary>
		/// <param name="sessionId"> The session of the log to monitor. </param>
		/// <param name="level"> The level in which to log. </param>
		public LogListener(Guid sessionId, EventLevel level = EventLevel.Informational)
		{
			_sessionId = sessionId;
			_level = level;

			Events = new List<EventWrittenEventArgs>();
			OnlyEventsWithMessages = true;

			EnableEvents(Logger.Instance, _level);
		}

		#endregion

		#region Properties

		/// <summary>
		/// The events that have been captured from the event source (logger).
		/// </summary>
		public List<EventWrittenEventArgs> Events { get; }

		/// <summary>
		/// Flag to capture only events with messages
		/// </summary>
		/// <remarks>
		/// Defaults to true so only events with a message will be captured.
		/// </remarks>
		public bool OnlyEventsWithMessages { get; set; }

		/// <summary>
		/// Flag to write incoming events to the console.
		/// </summary>
		public bool OutputToConsole { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing"> True if disposing and false if otherwise. </param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			DisableEvents(Logger.Instance);
		}

		/// <inheritdoc />
		protected override void OnEventWritten(EventWrittenEventArgs args)
		{
			if (_sessionId != Guid.Empty && !Equals(args.Payload[0], _sessionId))
			{
				return;
			}

			if (OnlyEventsWithMessages && string.IsNullOrEmpty(args.Message))
			{
				return;
			}

			Events.Add(args);

			if (OutputToConsole)
			{
				Console.WriteLine("SessionId: {0}", args.Payload[0]);
				Console.WriteLine(args.Message, args.Payload.ToArray());
			}
		}

		#endregion
	}
}