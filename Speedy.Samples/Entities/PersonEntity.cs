#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Entities
{
	public class PersonEntity : SyncEntity<int>, IUnwrappable
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public PersonEntity()
		{
			Groups = new List<GroupMemberEntity>();
			Owners = new List<PetEntity>();
		}

		#endregion

		#region Properties

		public virtual AddressEntity Address { get; set; }

		public long AddressId { get; set; }

		public Guid AddressSyncId { get; set; }

		public virtual ICollection<GroupMemberEntity> Groups { get; set; }

		public override int Id { get; set; }

		public string Name { get; set; }

		public virtual ICollection<PetEntity> Owners { get; set; }

		#endregion

		#region Methods
		
		public override object Unwrap()
		{
			return new PersonEntity
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
			return new HashSet<string> { nameof(Address), nameof(AddressId), nameof(Groups), nameof(Id) };
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