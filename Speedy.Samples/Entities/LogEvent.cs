#region References

#endregion

namespace Speedy.Samples.Entities
{
	public class LogEvent : CreatedEntity<string>, ILogEvent
	{
		#region Properties

		public override string Id { get; set; }
		public string Message { get; set; }

		#endregion
	}
}