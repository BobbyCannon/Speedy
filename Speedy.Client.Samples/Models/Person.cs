#region References

using System;
using Speedy.Sync;

#endregion

namespace Speedy.Client.Samples.Models
{
	public class Person : SyncModel<int>
	{
		#region Constructors

		public Person()
		{
			ExcludePropertiesForUpdate(nameof(AddressId));
		}

		#endregion

		#region Properties

		public virtual Address Address { get; set; }

		public long AddressId { get; set; }

		public Guid AddressSyncId { get; set; }

		public override int Id { get; set; }

		public string Name { get; set; }

		#endregion
	}
}