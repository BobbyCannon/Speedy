#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy.Extensions;
using Speedy.Sync;

#endregion

namespace Speedy.Website.Samples.Entities
{
	public class AddressEntity : SyncEntity<long>
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public AddressEntity()
		{
			Accounts = new List<AccountEntity>();
			LinkedAddresses = new List<AddressEntity>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The people associated with this address.
		/// </summary>
		public virtual ICollection<AccountEntity> Accounts { get; set; }

		/// <summary>
		/// The city for the address.
		/// </summary>
		public string City { get; set; }

		/// <summary>
		/// Read only property
		/// </summary>
		public string FullAddress => $"{Line1}{Environment.NewLine}{City}, {State}  {Postal}";

		/// <inheritdoc />
		public override long Id { get; set; }

		/// <summary>
		/// The line 1 for the address.
		/// </summary>
		public string Line1 { get; set; }

		/// <summary>
		/// The line 2 for the address.
		/// </summary>
		public string Line2 { get; set; }

		public virtual AddressEntity LinkedAddress { get; set; }

		public virtual ICollection<AddressEntity> LinkedAddresses { get; set; }

		public long? LinkedAddressId { get; set; }

		public Guid? LinkedAddressSyncId { get; set; }

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

		protected override HashSet<string> GetDefaultExclusionsForIncomingSync()
		{
			return base.GetDefaultExclusionsForIncomingSync()
				.Append(nameof(Accounts), nameof(LinkedAddress), nameof(LinkedAddressId), nameof(LinkedAddresses));
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