#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy.Extensions;
using Speedy.Sync;

#endregion

namespace Speedy.Data.Client
{
	/// <summary>
	/// Represents the client private address model.
	/// </summary>
	public class ClientAddress : SyncModel<long>
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

		/// <summary>
		/// The city for the address.
		/// </summary>
		public string City { get; set; }

		/// <inheritdoc />
		public override long Id { get; set; }

		/// <summary>
		/// Represents the last time this account was update on the client
		/// </summary>
		public DateTime LastClientUpdate { get; set; }

		/// <summary>
		/// The line 1 for the address.
		/// </summary>
		public string Line1 { get; set; }

		/// <summary>
		/// The line 2 for the address.
		/// </summary>
		public string Line2 { get; set; }

		/// <summary>
		/// The postal for the address.
		/// </summary>
		public string Postal { get; set; }

		/// <summary>
		/// The state for the address.
		/// </summary>
		public string State { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		protected override HashSet<string> GetDefaultExclusionsForIncomingSync()
		{
			return base.GetDefaultExclusionsForIncomingSync()
				.Append(nameof(Accounts), nameof(LastClientUpdate));
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