#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

#endregion

namespace Speedy.Logging
{
	/// <summary>
	/// Log listener that will keep a history of events.
	/// </summary>
	public class MemoryLogListener : LogListener
	{
		#region Constructors

		internal MemoryLogListener(Guid sessionId, EventLevel level = EventLevel.Informational) : base(sessionId, level)
		{
			Events = new List<EventWrittenEventArgs>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The events that have been captured from the event source (logger).
		/// </summary>
		public List<EventWrittenEventArgs> Events { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Create an instance of the log listener and start listening.
		/// </summary>
		/// <param name="sessionId"> The session of the log to monitor. </param>
		/// <param name="level"> The level in which to log. </param>
		/// <param name="initialize"> An optional initialize action. </param>
		public new static MemoryLogListener CreateSession(Guid sessionId, EventLevel level, Action<LogListener> initialize = null)
		{
			var logListener = new MemoryLogListener(sessionId, level);
			initialize?.Invoke(logListener);
			logListener.Start();
			return logListener;
		}

		/// <inheritdoc />
		protected override void OnEventWritten(EventWrittenEventArgs args)
		{
			if ((SessionId != Guid.Empty) && !Equals(args.Payload[0], SessionId))
			{
				return;
			}

			if (OnlyEventsWithMessages && string.IsNullOrEmpty(args.Message))
			{
				return;
			}

			Events.Add(args);

			base.OnEventWritten(args);
		}

		#endregion
	}
}