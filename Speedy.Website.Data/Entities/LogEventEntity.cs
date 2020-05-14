#region References

using Speedy.Data.WebApi;
using Speedy.Sync;
using System;

#endregion

namespace Speedy.Website.Samples.Entities
{
	public class LogEventEntity : SyncEntity<long>, ILogEvent
	{
		#region Properties

		/// <summary>
		/// The Date / Time of the log entry acknowledgement.
		/// </summary>
		public DateTime? AcknowledgedOn { get; set; }

		/// <inheritdoc />
		public override long Id { get; set; }

		/// <inheritdoc />
		public LogLevel Level { get; set; }

		/// <summary>
		/// The Date / Time of the log entry.
		/// </summary>
		public DateTime LoggedOn { get; set; }

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