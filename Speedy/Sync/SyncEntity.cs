#region References

using System;
using System.Collections.Generic;
using System.Data;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represent an entity that can be synced.
	/// </summary>
	public abstract class SyncEntity : ModifiableEntity
	{
		#region Constructors

		/// <summary>
		/// Instantiates a sync entity.
		/// </summary>
		protected SyncEntity()
		{
			IgnoreProperties = new HashSet<string>(new[] { "Id", "SyncId" });
		}

		#endregion

		#region Properties

		/// <summary>
		/// The ID of the sync entity.
		/// </summary>
		/// <remarks>
		/// This ID is should be globally unique. Never reuse GUIDs.
		/// </remarks>
		public Guid SyncId { get; set; }

		/// <summary>
		/// Properties to ignore when updating.
		/// </summary>
		protected HashSet<string> IgnoreProperties { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Converts the entity into an object to transmit.
		/// </summary>
		/// <returns> The sync object for this entity. </returns>
		public SyncObject ToSyncObject()
		{
			var json = this.ToJson(true);
			var type = this.GetRealType();

			return new SyncObject
			{
				Data = json,
				SyncId = SyncId,
				TypeName = type.ToAssemblyName(),
				Status = CreatedOn == ModifiedOn ? SyncStatus.Added : SyncStatus.Modified
			};
		}

		/// <summary>
		/// Creates a tombstone for this entity.
		/// </summary>
		/// <returns> The tombstone for this entity. </returns>
		public SyncTombstone ToSyncTombstone()
		{
			return new SyncTombstone { CreatedOn = DateTime.UtcNow, TypeName = this.GetRealType().ToAssemblyName(), SyncId = SyncId };
		}

		/// <summary>
		/// Update the entity with the changes.
		/// </summary>
		/// <param name="update"> The entity with the changes. </param>
		/// <param name="database"> The database to use for relationships. </param>
		public virtual void Update(SyncEntity update, IDatabase database)
		{
			var type = this.GetRealType();
			var typeName = type.ToAssemblyName();
			var updateTypeName = update.GetRealType().ToAssemblyName();

			if (typeName != updateTypeName)
			{
				throw new DataException("Trying update a sync entity with a mismatched type.");
			}

			var properties = type.GetCachedProperties();
			foreach (var property in properties)
			{
				if (IgnoreProperties.Contains(property.Name))
				{
					continue;
				}

				property.SetValue(this, property.GetValue(update));
			}
		}

		/// <summary>
		/// Updates the relation ids using the sync ids.
		/// </summary>
		public abstract void UpdateLocalRelationships(IDatabase database);

		/// <summary>
		/// Updates the sync ids using relationships.
		/// </summary>
		public abstract void UpdateLocalSyncIds();

		#endregion
	}
}