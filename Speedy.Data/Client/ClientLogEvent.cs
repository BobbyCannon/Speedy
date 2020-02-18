#region References

using System;
using System.Collections.Generic;
using Speedy.Data.WebApi;
using Speedy.Extensions;

#endregion

namespace Speedy.Data.Client
{
	public class ClientLogEvent : LogEvent
	{
		#region Properties

		/// <summary>
		/// Represents the last time this account was update on the client
		/// </summary>
		public DateTime LastClientUpdate { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public override bool CanBeModified()
		{
			return false;
		}

		/// <inheritdoc />
		protected override HashSet<string> GetDefaultExclusionsForIncomingSync()
		{
			return base.GetDefaultExclusionsForIncomingSync()
				.Append(nameof(LastClientUpdate));
		}

		/// <inheritdoc />
		protected override HashSet<string> GetDefaultExclusionsForOutgoingSync()
		{
			// Update defaults are the same as incoming sync defaults
			return GetDefaultExclusionsForIncomingSync();
		}

		/// <inheritdoc />
		protected override HashSet<string> GetDefaultExclusionsForSyncUpdate()
		{
			// Update defaults are the same as incoming sync defaults plus some
			return base.GetDefaultExclusionsForSyncUpdate()
				.Append(GetDefaultExclusionsForIncomingSync());
		}

		#endregion
	}
}