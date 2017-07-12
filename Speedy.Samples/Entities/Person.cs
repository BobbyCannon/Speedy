#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy;

#endregion

namespace Speedy.Samples.Entities
{
	public class Person : IncrementingModifiableEntity
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public Person()
		{
			Members = new List<GroupMember>();
			Owners = new List<Pet>();
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
		public virtual ICollection<GroupMember> Members { get; set; }
		public string Name { get; set; }
		public virtual ICollection<Pet> Owners { get; set; }

		#endregion
	}
}