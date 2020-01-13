#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy.Data.WebApi;

#endregion

namespace Speedy.Data.Client
{
	public class ClientAddress : Address
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public ClientAddress()
		{
			Accounts = new List<ClientAccount>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The people associated with this address.
		/// </summary>
		public virtual ICollection<ClientAccount> Accounts { get; set; }

		/// <inheritdoc />
		public override long Id { get; set; }

		/// <summary>
		/// Represents the last time this account was update on the client
		/// </summary>
		public DateTime LastClientUpdate { get; set; }

		#endregion

		#region Methods

		protected override HashSet<string> GetDefaultExclusionsForIncomingSync()
		{
			return base.GetDefaultExclusionsForIncomingSync()
				.Append(nameof(Accounts), nameof(LastClientUpdate));
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