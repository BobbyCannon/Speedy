#region References

using System;
using System.Diagnostics.Tracing;

#endregion

namespace Speedy.Logging;

public class LogEventArgs : EventArgs
{
	#region Constructors

	public LogEventArgs() : this(DateTime.MinValue, EventLevel.Verbose, string.Empty)
	{
	}

	public LogEventArgs(string message) : this(TimeService.UtcNow, EventLevel.Informational, message)
	{
	}

	public LogEventArgs(EventLevel level, string message) : this(TimeService.UtcNow, level, message)
	{
	}

	public LogEventArgs(DateTime dateTime, EventLevel level, string message)
	{
		DateTime = dateTime;
		Level = level;
		Message = message;
	}

	#endregion

	#region Properties

	public DateTime DateTime { get; set; }

	public EventLevel Level { get; set; }

	public string Message { get; set; }

	#endregion
}