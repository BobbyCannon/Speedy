#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Entities
{
	[Serializable]
	public class Person : SyncEntity
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public Person()
		{
			Groups = new Collection<GroupMember>();
			IgnoreProperties.AddRange(nameof(Address), nameof(AddressSyncId), nameof(Groups));
		}

		#endregion

		#region Properties

		public virtual Address Address { get; set; }
		public int AddressId { get; set; }
		public Guid AddressSyncId { get; set; }
		public virtual ICollection<GroupMember> Groups { get; set; }
		public string Name { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Updates the relation using the sync ids.
		/// </summary>
		public override void UpdateLocalRelationships(ISyncableDatabase database)
		{
			AddressId = database.GetRepository<Address>().First(x => x.SyncId == AddressSyncId).Id;
		}

		/// <summary>
		/// Updates the sync ids of relationships.
		/// </summary>
		public override void UpdateLocalSyncIds()
		{
			this.UpdateIf(() => Address != null && AddressSyncId != Address.SyncId, () => AddressSyncId = Address.SyncId);
		}

		#endregion
	}
}