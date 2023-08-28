#region References

using Speedy.Sync;

#endregion

namespace Speedy.Data.SyncApi
{
	public class LogEvent : SyncModel<long>
	{
		#region Constructors

		public LogEvent()
		{
			ResetHasChanges();
		}

		#endregion

		#region Properties

		public override long Id { get; set; }

		public LogLevel Level { get; set; }

		public string Message { get; set; }

		#endregion
	}
}