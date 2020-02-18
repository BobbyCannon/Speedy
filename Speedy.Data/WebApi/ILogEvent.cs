namespace Speedy.Data.WebApi
{
	public interface ILogEvent
	{
		#region Properties

		/// <summary>
		/// The log level for the log event.
		/// </summary>
		LogLevel Level { get; set; }

		/// <summary>
		/// The message for the log event.
		/// </summary>
		string Message { get; set; }

		#endregion
	}
}