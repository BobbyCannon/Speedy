#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace Speedy.Samples.Entities
{
	[Serializable]
	public class Address : BaseModifiableEntity
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInContructor")]
		public Address()
		{
			People = new Collection<Person>();
		}

		#endregion

		#region Properties

		public string City { get; set; }

		/// <summary>
		/// Read only property
		/// </summary>
		public string FullAddress => $"{Line1}{Environment.NewLine}{City}, {State}  {Postal}";

		public override int Id { get; set; }
		public string Line1 { get; set; }
		public string Line2 { get; set; }
		public virtual Address LinkedAddress { get; set; }
		public int? LinkedAddressId { get; set; }
		public Guid? LinkedAddressSyncId { get; set; }
		public virtual ICollection<Person> People { get; set; }
		public string Postal { get; set; }
		public string State { get; set; }

		#endregion
	}
}