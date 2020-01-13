namespace Speedy.Website.Samples.Entities
{
	public interface ILogEvent
	{
		#region Properties

		/// <summary>
		/// The message for the log event.
		/// </summary>
		string Message { get; set; }

		#endregion
	}
}