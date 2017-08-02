#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Entities
{
	public class Address : SyncEntity
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public Address()
		{
			BillingPeople = new List<Person>();
			LinkedAddresses = new List<Address>();
			People = new List<Person>();
			ExcludedPropertiesForUpdate.AddRange(typeof(Address).GetVirtualPropertyNames().ToArray());
		}

		#endregion

		#region Properties

		public virtual ICollection<Person> BillingPeople { get; set; }
		public string City { get; set; }

		/// <summary>
		/// Read only property
		/// </summary>
		public string FullAddress => $"{Line1}{Environment.NewLine}{City}, {State}  {Postal}";

		public override int Id { get; set; }
		public string Line1 { get; set; }
		public string Line2 { get; set; }
		public virtual Address LinkedAddress { get; set; }
		public virtual ICollection<Address> LinkedAddresses { get; set; }
		public int? LinkedAddressId { get; set; }
		public Guid? LinkedAddressSyncId { get; set; }
		public virtual ICollection<Person> People { get; set; }
		public string Postal { get; set; }
		public string State { get; set; }

		#endregion
	}
}