#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy.Sync;

#endregion

namespace Speedy.Website.Samples.Entities
{
	public class AccountEntity : SyncEntity<int>, IUnwrappable
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public AccountEntity()
		{
			Groups = new List<GroupMemberEntity>();
			Pets = new List<PetEntity>();
		}

		#endregion

		#region Properties

		public virtual AddressEntity Address { get; set; }

		public long AddressId { get; set; }

		public Guid AddressSyncId { get; set; }

		public string EmailAddress { get; set; }

		public virtual ICollection<GroupMemberEntity> Groups { get; set; }

		public override int Id { get; set; }

		public DateTime LastLoginDate { get; set; }

		public string Name { get; set; }

		public string PasswordHash { get; set; }

		public virtual ICollection<PetEntity> Pets { get; set; }

		public string Roles { get; set; }

		#endregion

		#region Methods

		public string GetCookieValue()
		{
			return $"{Id};{Name}";
		}

		public override object Unwrap()
		{
			return new AccountEntity
			{
				AddressId = AddressId,
				AddressSyncId = AddressSyncId,
				Id = Id,
				Name = Name,
				CreatedOn = CreatedOn,
				ModifiedOn = ModifiedOn,
				IsDeleted = IsDeleted,
				SyncId = SyncId
			};
		}

		protected override HashSet<string> GetDefaultExclusionsForIncomingSync()
		{
			return base.GetDefaultExclusionsForIncomingSync()
				.Append(nameof(Address), nameof(AddressId), nameof(Groups), nameof(PasswordHash), nameof(Pets), nameof(Roles));
		}

		protected override HashSet<string> GetDefaultExclusionsForOutgoingSync()
		{
			// Update defaults are the same as incoming sync defaults
			return base.GetDefaultExclusionsForIncomingSync();
		}

		protected override HashSet<string> GetDefaultExclusionsForSyncUpdate()
		{
			// Update defaults are the same as incoming sync defaults plus some
			return base.GetDefaultExclusionsForSyncUpdate()
				.Append(GetDefaultExclusionsForIncomingSync())
				.Append(nameof(LastLoginDate));
		}

		#endregion
	}
}