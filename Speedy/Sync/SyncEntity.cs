#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represent an entity that can be synced.
	/// </summary>
	public abstract class SyncEntity<T> : Entity<T>, ISyncEntity
	{
		#region Constructors

		/// <summary>
		/// Instantiates a sync entity.
		/// </summary>
		protected SyncEntity()
		{
			ExclusionCacheForIncomingSync.GetOrAdd(RealType, x => GetDefaultExclusionsForIncomingSync());
			ExclusionCacheForOutgoingSync.GetOrAdd(RealType, x => GetDefaultExclusionsForOutgoingSync());
			ExclusionCacheForSyncUpdate.GetOrAdd(RealType, x => GetDefaultExclusionsForSyncUpdate());
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public DateTime CreatedOn { get; set; }

		/// <inheritdoc />
		public bool IsDeleted { get; set; }

		/// <inheritdoc />
		public DateTime ModifiedOn { get; set; }

		/// <inheritdoc />
		public Guid SyncId { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public object GetEntityId()
		{
			return Id;
		}

		/// <inheritdoc />
		public object GetEntitySyncId()
		{
			return SyncId;
		}

		/// <inheritdoc />
		public bool IsPropertyExcludedForIncomingSync(string propertyName)
		{
			return ExclusionCacheForIncomingSync[RealType].Contains(propertyName);
		}

		/// <inheritdoc />
		public bool IsPropertyExcludedForOutgoingSync(string propertyName)
		{
			return ExclusionCacheForOutgoingSync[RealType].Contains(propertyName);
		}

		/// <inheritdoc />
		public bool IsPropertyExcludedForSyncUpdate(string propertyName)
		{
			return ExclusionCacheForSyncUpdate[RealType].Contains(propertyName);
		}

		/// <inheritdoc />
		public SyncObject ToSyncObject()
		{
			return SyncObject.ToSyncObject(this);
		}

		/// <inheritdoc />
		public override object Unwrap()
		{
			if (this is IUnwrappable)
			{
				return ((IUnwrappable) this).Unwrap();
			}

			var syncObject = ToSyncObject();
			return syncObject.ToSyncEntity();
		}

		/// <inheritdoc />
		public virtual void UpdateLocalSyncIds()
		{
			var properties = RealType.GetCachedProperties().ToList();
			var entityRelationships = properties
				.Where(x => x.GetCachedAccessors()[0].IsVirtual)
				.Where(x => SyncEntityInterfaceType.IsAssignableFrom(x.PropertyType))
				.ToList();

			foreach (var entityRelationship in entityRelationships)
			{
				var entityRelationshipSyncIdProperty = properties.FirstOrDefault(x => x.Name == $"{entityRelationship.Name}SyncId");

				if (entityRelationship.GetValue(this, null) is ISyncEntity syncEntity && entityRelationshipSyncIdProperty != null)
				{
					var otherEntitySyncId = (Guid?) entityRelationshipSyncIdProperty.GetValue(this, null);
					if (otherEntitySyncId != syncEntity.SyncId)
					{
						// resets entitySyncId to entity.SyncId if it does not match
						entityRelationshipSyncIdProperty.SetValue(this, syncEntity.SyncId, null);
					}
				}

				// todo: maybe?, support setting EntityId would then query the entity sync id and set it?
			}
		}

		/// <inheritdoc />
		public void UpdateWith(ISyncEntity update, bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync, bool excludePropertiesForSyncUpdate)
		{
			var exclusions = GetExclusions(RealType, excludePropertiesForIncomingSync, excludePropertiesForOutgoingSync, excludePropertiesForSyncUpdate);
			UpdateWith(update, exclusions.ToArray());
		}

		/// <inheritdoc />
		public void UpdateWith(ISyncEntity update, bool excludeVirtuals, params string[] exclusions)
		{
			var totalExclusions = new HashSet<string>(exclusions);
			if (excludeVirtuals)
			{
				totalExclusions.AddRange(RealType.GetVirtualPropertyNames());
			}

			UpdateWith(update, totalExclusions.ToArray());
		}

		/// <inheritdoc />
		public void UpdateWith(ISyncEntity update, params string[] exclusions)
		{
			UpdatableExtensions.UpdateWith(this, update, exclusions);
		}

		/// <summary>
		/// Gets the default exclusions for incoming sync data. Warning: this is called during constructor,
		/// overrides need to be sure to only return static values as to not cause issues.
		/// </summary>
		/// <returns> The values to exclude during sync. </returns>
		protected virtual HashSet<string> GetDefaultExclusionsForIncomingSync()
		{
			return new() { nameof(Id) };
		}

		/// <summary>
		/// Gets the default exclusions for outgoing sync data. Warning: this is called during constructor,
		/// overrides need to be sure to only return static values as to not cause issues.
		/// </summary>
		/// <returns> The values to exclude during sync. </returns>
		protected virtual HashSet<string> GetDefaultExclusionsForOutgoingSync()
		{
			return new() { nameof(Id) };
		}

		/// <summary>
		/// Gets the default exclusions for update. Warning: this is called during constructor, overrides need to be
		/// sure to only return static values as to not cause issues.
		/// </summary>
		/// <returns> The values to exclude during update. </returns>
		protected virtual HashSet<string> GetDefaultExclusionsForSyncUpdate()
		{
			return new() { nameof(Id) };
		}

		#endregion
	}
}