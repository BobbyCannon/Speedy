#region References

using System;
using Speedy.Data.SyncApi;

#endregion

namespace Speedy.Website.Data.Entities
{
	public class LogEventEntity : LogEvent, ILogEvent
	{
		#region Constructors

		public LogEventEntity()
		{
			ResetHasChanges();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The Date / Time of the log entry acknowledgement.
		/// </summary>
		public DateTime? AcknowledgedOn { get; set; }

		/// <summary>
		/// The Date / Time of the log entry.
		/// </summary>
		public DateTime LoggedOn { get; set; }

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