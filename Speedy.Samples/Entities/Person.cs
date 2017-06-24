#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace Speedy.Samples.Entities
{
	[Serializable]
	public class Person : IncrementingModifiableEntity
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public Person()
		{
			Groups = new Collection<GroupMember>();
			Pets = new Collection<Pet>();
		}

		#endregion

		#region Properties

		public virtual Address Address { get; set; }
		public int AddressId { get; set; }
		public Guid AddressSyncId { get; set; }
		public virtual Address BillingAddress { get; set; }
		public int? BillingAddressId { get; set; }
		public Guid? BillingAddressSyncId { get; set; }
		public virtual ICollection<GroupMember> Groups { get; set; }
		public override int Id { get; set; }
		public string Name { get; set; }
		public virtual ICollection<Pet> Pets { get; set; }

		#endregion
	}
}