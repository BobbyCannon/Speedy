namespace Speedy.Samples.Entities
{
	public class LogEvent : Entity, ILogEvent
	{
		#region Properties

		public string Message { get; set; }

		#endregion
	}
}