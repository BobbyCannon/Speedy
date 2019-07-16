#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Entities
{
	public class AddressEntity : SyncEntity<long>
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public AddressEntity()
		{
			LinkedAddresses = new List<AddressEntity>();
			People = new List<PersonEntity>();
		}

		#endregion

		#region Properties

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

		public virtual ICollection<PersonEntity> People { get; set; }

		public string Postal { get; set; }

		public string State { get; set; }

		#endregion
	}
}