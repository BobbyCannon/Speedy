namespace Speedy.Samples.Entities
{
	public class LogEvent : IncrementingCreatedEntity, ILogEvent
	{
		#region Properties

		public override int Id { get; set; }

		public string Message { get; set; }

		#endregion
	}
}