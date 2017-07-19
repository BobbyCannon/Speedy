#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Entities
{
	public class Person : SyncEntity
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public Person()
		{
			Groups = new List<GroupMember>();
			Owners = new List<Pet>();
			ExcludedPropertiesForUpdate.AddRange(nameof(Address), nameof(AddressSyncId), nameof(Groups));
		}

		#endregion

		#region Properties

		public virtual Address Address { get; set; }
		public int AddressId { get; set; }
		public Guid AddressSyncId { get; set; }
		public virtual Address BillingAddress { get; set; }
		public int? BillingAddressId { get; set; }
		public Guid? BillingAddressSyncId { get; set; }
		public override int Id { get; set; }
		public virtual ICollection<GroupMember> Groups { get; set; }
		public string Name { get; set; }
		public virtual ICollection<Pet> Owners { get; set; }

		#endregion
	}
}