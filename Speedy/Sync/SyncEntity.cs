#region References

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represent an entity that can be synced.
	/// </summary>
	public abstract class SyncEntity : IncrementingModifiableEntity
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
			var json = this.ToJson(ignoreVirtuals: true);
			var type = this.GetRealType();

			return new SyncObject
			{
				Data = json,
				SyncId = SyncId,
				TypeName = type.ToAssemblyName(),
				Status = CreatedOn == ModifiedOn ? SyncObjectStatus.Added : SyncObjectStatus.Modified
			};
		}

		/// <summary>
		/// Creates a tombstone for this entity.
		/// </summary>
		/// <param name="referenceId"> The reference ID to the owner of this tombstone. </param>
		/// <returns> The tombstone for this entity. </returns>
		public SyncTombstone ToSyncTombstone(string referenceId)
		{
			return new SyncTombstone { CreatedOn = DateTime.UtcNow, TypeName = this.GetRealType().ToAssemblyName(), SyncId = SyncId, ReferenceId = referenceId ?? string.Empty };
		}

		/// <summary>
		/// Update the entity with the changes.
		/// </summary>
		/// <param name="update"> The entity with the changes. </param>
		public void Update(SyncEntity update)
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
				if (!property.CanWrite || IgnoreProperties.Contains(property.Name))
				{
					continue;
				}

				property.SetValue(this, property.GetValue(update));
			}
		}

		/// <summary>
		/// Updates the sync ids using relationships.
		/// </summary>
		public virtual void UpdateLocalSyncIds()
		{
			var type = this.GetRealType();
			var baseEntityType = typeof(SyncEntity);
			var properties = type.GetCachedProperties().ToList();

			var entityRelationships = properties
				.Where(x => x.GetCachedAccessors()[0].IsVirtual)
				.Where(x => baseEntityType.IsAssignableFrom(x.PropertyType))
				.ToList();

			foreach (var entityRelationship in entityRelationships)
			{
				var syncEntity = entityRelationship.GetValue(this, null) as SyncEntity;
				var entityRelationshipSyncIdProperty = properties.FirstOrDefault(x => x.Name == entityRelationship.Name + "SyncId");

				if (syncEntity != null && entityRelationshipSyncIdProperty != null)
				{
					var otherEntitySyncId = (Guid?) entityRelationshipSyncIdProperty.GetValue(this, null);
					if (otherEntitySyncId != syncEntity.SyncId)
					{
						// resets entitySyncId to entity.SyncId if it does not match
						entityRelationshipSyncIdProperty.SetValue(this, syncEntity.SyncId, null);
					}
				}
			}
		}

		#endregion
	}
}