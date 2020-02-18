#region References

using Speedy.Data.WebApi;
using Speedy.Sync;

#endregion

namespace Speedy.Website.Samples.Entities
{
	public class LogEventEntity : SyncEntity<long>, ILogEvent
	{
		#region Properties

		/// <inheritdoc />
		public override long Id { get; set; }

		/// <inheritdoc />
		public LogLevel Level { get; set; }

		/// <inheritdoc />
		public string Message { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public override bool CanBeModified()
		{
			return false;
		}

		#endregion
	}
}