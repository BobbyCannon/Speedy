#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

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
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the log listener.
		/// </summary>
		/// <param name="sessionId"> The session of the log to monitor. </param>
		/// <param name="level"> The level in which to log. </param>
		public LogListener(Guid sessionId, EventLevel level = EventLevel.Informational)
		{
			SessionId = sessionId;
			Level = level;

			Events = new List<EventWrittenEventArgs>();
			OnlyEventsWithMessages = true;

			EnableEvents(Logger.Instance, Level);
		}

		#endregion

		#region Properties

		/// <summary>
		/// The events that have been captured from the event source (logger).
		/// </summary>
		public List<EventWrittenEventArgs> Events { get; }

		/// <summary>
		/// The level in which to log.
		/// </summary>
		public EventLevel Level { get; set; }

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

		/// <summary>
		/// The ID of the session.
		/// </summary>
		public Guid SessionId { get; set; }

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
			if (SessionId != Guid.Empty && !Equals(args.Payload[0], SessionId))
			{
				return;
			}

			if (OnlyEventsWithMessages && string.IsNullOrEmpty(args.Message))
			{
				return;
			}

			Events.Add(args);
			EventWritten?.Invoke(this, args);

			if (OutputToConsole)
			{
				Console.WriteLine($"{args.Payload[0]}: {args.ToPayloadString()}");
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Occurs when an event is written.
		/// </summary>
		public event EventHandler<EventWrittenEventArgs> EventWritten;

		#endregion
	}
}