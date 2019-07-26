#region References

using System;
using Speedy.Samples.Enumerations;

#endregion

namespace Speedy.Samples.Entities
{
	public class LogEventEntity : Entity<string>, ILogEvent, IModifiableEntity
	{
		#region Properties

		/// <inheritdoc />
		public DateTime CreatedOn { get; set; }

		/// <inheritdoc />
		public override string Id { get; set; }

		public LogLevel Level { get; set; }

		/// <inheritdoc />
		public string Message { get; set; }

		/// <inheritdoc />
		public DateTime ModifiedOn { get; set; }

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