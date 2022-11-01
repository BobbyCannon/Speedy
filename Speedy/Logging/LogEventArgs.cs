#region References

using System;
using System.Diagnostics.Tracing;

#endregion

namespace Speedy.Logging;

/// <summary>
/// Represents a log event argument.
/// </summary>
public class LogEventArgs : EventArgs
{
	#region Constructors

	/// <summary>
	/// Instantiates a log event argument.
	/// </summary>
	public LogEventArgs() : this(DateTime.MinValue, EventLevel.Verbose, string.Empty)
	{
	}

	/// <summary>
	/// Instantiates a log event argument.
	/// </summary>
	public LogEventArgs(string message) : this(TimeService.UtcNow, EventLevel.Informational, message)
	{
	}

	/// <summary>
	/// Instantiates a log event argument.
	/// </summary>
	public LogEventArgs(EventLevel level, string message) : this(TimeService.UtcNow, level, message)
	{
	}

	/// <summary>
	/// Instantiates a log event argument.
	/// </summary>
	public LogEventArgs(DateTime dateTime, EventLevel level, string message)
	{
		DateTime = dateTime;
		Level = level;
		Message = message;
	}

	#endregion

	#region Properties

	/// <summary>
	/// The date time of the event args.
	/// </summary>
	public DateTime DateTime { get; set; }

	/// <summary>
	/// The level of the event.
	/// </summary>
	public EventLevel Level { get; set; }

	/// <summary>
	/// The message for the event.
	/// </summary>
	public string Message { get; set; }

	#endregion
}