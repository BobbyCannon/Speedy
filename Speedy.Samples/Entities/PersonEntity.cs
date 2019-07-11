#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Entities
{
	public class PersonEntity : SyncEntity<int>
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public PersonEntity()
		{
			Groups = new List<GroupMemberEntity>();
			Owners = new List<PetEntity>();

			ExcludePropertiesForUpdate(nameof(Address), nameof(AddressId), nameof(Groups));
		}

		#endregion

		#region Properties

		public virtual AddressEntity Address { get; set; }

		public long AddressId { get; set; }

		public Guid AddressSyncId { get; set; }

		public virtual AddressEntity BillingAddress { get; set; }

		public long? BillingAddressId { get; set; }

		public Guid? BillingAddressSyncId { get; set; }

		public virtual ICollection<GroupMemberEntity> Groups { get; set; }

		public override int Id { get; set; }

		public string Name { get; set; }

		public virtual ICollection<PetEntity> Owners { get; set; }

		#endregion
	}
}