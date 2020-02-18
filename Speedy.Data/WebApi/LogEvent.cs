#region References

using Speedy.Sync;

#endregion

namespace Speedy.Data.WebApi
{
	public class LogEvent : SyncModel<long>
	{
		#region Properties

		public override long Id { get; set; }

		public LogLevel Level { get; set; }

		public string Message { get; set; }

		#endregion
	}
}