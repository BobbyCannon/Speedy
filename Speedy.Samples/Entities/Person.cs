#region References

using System;
using System.Linq;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Entities
{
	[Serializable]
	public class Person : SyncEntity
	{
		#region Properties

		public virtual Address Address { get; set; }
		public int AddressId { get; set; }
		public Guid AddressSyncId { get; set; }
		public string Name { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Update the entity with the changes.
		/// </summary>
		/// <param name="update"> The entity with the changes. </param>
		/// <param name="database"> The database for relationships. </param>
		public override void Update(SyncEntity update, IDatabase database)
		{
			var entity = update as Person;
			if (entity == null)
			{
				throw new ArgumentException("The update is not a person.", nameof(update));
			}

			IgnoreProperties.AddRange("AddressId", "Address");
			base.Update(update, database);

			UpdateLocalRelationships(database);
		}

		/// <summary>
		/// Updates the sync ids of relationships.
		/// </summary>
		public override void UpdateLocalSyncIds()
		{
			this.UpdateIf(() => Address != null && AddressSyncId != Address.SyncId, () => AddressSyncId = Address.SyncId);
		}

		/// <summary>
		/// Updates the relation using the sync ids.
		/// </summary>
		public override void UpdateLocalRelationships(IDatabase database)
		{
			Address = database.GetRepository<Address>().First(x => x.SyncId == AddressSyncId);
		}

		#endregion
	}
}