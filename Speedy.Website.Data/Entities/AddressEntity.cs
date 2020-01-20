#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

		public virtual ICollection<AccountEntity> Accounts { get; set; }

		public string City { get; set; }

		/// <summary>
		/// Read only property
		/// </summary>
		public string FullAddress => $"{Line1}{Environment.NewLine}{City}, {State}  {Postal}";

		public override long Id { get; set; }

		public string Line1 { get; set; }

		public string Line2 { get; set; }

		public virtual AddressEntity LinkedAddress { get; set; }

		public virtual ICollection<AddressEntity> LinkedAddresses { get; set; }

		public long? LinkedAddressId { get; set; }

		public Guid? LinkedAddressSyncId { get; set; }

		public string Postal { get; set; }

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