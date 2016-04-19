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
	public class Address : SyncEntity
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
		public string Line1 { get; set; }
		public string Line2 { get; set; }
		public virtual Address LinkedAddress { get; set; }
		public int? LinkedAddressId { get; set; }
		public Guid? LinkedAddressSyncId { get; set; }
		public virtual ICollection<Person> People { get; set; }
		public string Postal { get; set; }
		public string State { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Update the entity with the changes.
		/// </summary>
		/// <param name="update"> The entity with the changes. </param>
		/// <param name="database"> The database to use for relationships. </param>
		public override void Update(SyncEntity update, IDatabase database)
		{
			var entity = update as Address;
			if (entity == null)
			{
				throw new ArgumentException("The update is not an address.", nameof(update));
			}

			IgnoreProperties.AddRange(nameof(LinkedAddress), nameof(LinkedAddressId));
			base.Update(update, database);

			UpdateLocalRelationships(database);
		}

		/// <summary>
		/// Updates the relation ids using the sync ids.
		/// </summary>
		public override void UpdateLocalRelationships(IDatabase database)
		{
			this.UpdateIf(() => LinkedAddressSyncId != null, () => LinkedAddress = database.GetRepository<Address>().First(x => x.SyncId == LinkedAddressSyncId));
		}

		/// <summary>
		/// Updates the sync ids of relationships.
		/// </summary>
		public override void UpdateLocalSyncIds()
		{
			this.UpdateIf(() => LinkedAddress != null && LinkedAddress.SyncId != LinkedAddressSyncId, () => LinkedAddressSyncId = LinkedAddressSyncId);
		}

		#endregion
	}
}