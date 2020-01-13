#region References

using System;
using System.Collections.Generic;
using Speedy.Data.WebApi;

#endregion

namespace Speedy.Data.Client
{
	public class ClientAccount : Account
	{
		#region Properties

		/// <summary>
		/// The required address for this person.
		/// </summary>
		public virtual ClientAddress Address { get; set; }

		/// <summary>
		/// The ID for the address.
		/// </summary>
		public long AddressId { get; set; }

		/// <inheritdoc />
		public override int Id { get; set; }

		/// <summary>
		/// Represents the last time this account was update on the client
		/// </summary>
		public DateTime LastClientUpdate { get; set; }

		#endregion

		#region Methods

		protected override HashSet<string> GetDefaultExclusionsForIncomingSync()
		{
			return base.GetDefaultExclusionsForIncomingSync()
				.Append(nameof(Address), nameof(AddressId), nameof(LastClientUpdate));
		}

		protected override HashSet<string> GetDefaultExclusionsForOutgoingSync()
		{
			// Update defaults are the same as incoming sync defaults
			return GetDefaultExclusionsForIncomingSync();
		}

		protected override HashSet<string> GetDefaultExclusionsForSyncUpdate()
		{
			// Update defaults are the same as incoming sync defaults plus some
			return base.GetDefaultExclusionsForSyncUpdate()
				.Append(GetDefaultExclusionsForIncomingSync());
		}

		#endregion
	}
}