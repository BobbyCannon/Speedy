#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy.Data.SyncApi;
using Speedy.Extensions;

#endregion

namespace Speedy.Website.Data.Entities
{
	public class AddressEntity : Address
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public AddressEntity()
		{
			Accounts = new List<AccountEntity>();
			LinkedAddresses = new List<AddressEntity>();
			ResetChangeTracking();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Represents the "primary" account for the address.
		/// </summary>
		public virtual AccountEntity Account { get; set; }

		/// <summary>
		/// The ID for the account.
		/// </summary>
		public int? AccountId { get; set; }

		/// <summary>
		/// The people associated with this address.
		/// </summary>
		public virtual ICollection<AccountEntity> Accounts { get; set; }

		/// <summary>
		/// The sync ID for the account.
		/// </summary>
		public Guid? AccountSyncId { get; set; }

		/// <summary>
		/// Read only property
		/// </summary>
		public string FullAddress => $"{Line1}{Environment.NewLine}{City}, {State}  {Postal}";

		public virtual AddressEntity LinkedAddress { get; set; }

		public virtual ICollection<AddressEntity> LinkedAddresses { get; set; }

		public long? LinkedAddressId { get; set; }

		public Guid? LinkedAddressSyncId { get; set; }

		#endregion

		#region Methods

		protected override HashSet<string> GetDefaultExclusionsForIncomingSync()
		{
			return base.GetDefaultExclusionsForIncomingSync()
				.Append(nameof(AccountId), nameof(Accounts), nameof(LinkedAddress), nameof(LinkedAddressId), nameof(LinkedAddresses));
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
				.Append(GetDefaultExclusionsForIncomingSync())
				.Append(nameof(IsDeleted));
		}

		#endregion
	}
}