#region References

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represent an entity that can be synced.
	/// </summary>
	public abstract class SyncEntity<T> : Entity<T>, ISyncEntity
	{
		#region Fields

		/// <summary>
		/// Properties to ignore when syncing
		/// </summary>
		private HashSet<string> _excludedPropertiesForSync;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a sync entity.
		/// </summary>
		protected SyncEntity()
		{
			_excludedPropertiesForSync = new HashSet<string>(GetDefaultExclusionsForSync());
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
		public void ExcludePropertiesForSync(params string[] propertyNames)
		{
			foreach (var propertyName in propertyNames)
			{
				if (_excludedPropertiesForSync.Contains(propertyName))
				{
					continue;
				}

				_excludedPropertiesForSync.Add(propertyName);
			}
		}

		/// <summary>
		/// Gets the default exclusions for sync. Warning: this is called during constructor, overrides need to be 
		/// sure to only return static values as to not cause issues.
		/// </summary>
		/// <returns> The values to exclude during sync. </returns>
		public virtual HashSet<string> GetDefaultExclusionsForSync()
		{
			return new HashSet<string> { nameof(Id) };
		}

		/// <inheritdoc />
		public bool IsPropertyExcludedForSync(string propertyName)
		{
			return _excludedPropertiesForSync.Contains(propertyName);
		}

		/// <inheritdoc />
		public void ResetPropertySyncExclusions(bool setToDefault = true)
		{
			_excludedPropertiesForSync = setToDefault ? new HashSet<string>(GetDefaultExclusionsForSync()) : new HashSet<string>();
		}

		/// <inheritdoc />
		public void ResetId()
		{
			Id = default;
		}

		/// <inheritdoc />
		public SyncObject ToSyncObject()
		{
			var type = this.GetRealType();
			var settings = SyncObject.GetOrAddCachedSettings(type);
			var json = this.ToJson(settings);

			return new SyncObject
			{
				Data = json,
				ModifiedOn = ModifiedOn,
				SyncId = SyncId,
				TypeName = type.ToAssemblyName(),
				Status = CreatedOn == ModifiedOn ? SyncObjectStatus.Added : SyncObjectStatus.Modified
			};
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
		public void UpdateWith(ISyncEntity update, bool allowSyncExclusions = true, bool allowUpdateExclusions = true)
		{
			var destinationType = this.GetRealType();
			var sourceType = update.GetRealType();
			var destinationProperties = destinationType.GetCachedProperties();
			var sourceProperties = sourceType.GetCachedProperties();

			// Need to cache this? would this save on GC?
			var virtualProperties = destinationType.GetVirtualPropertyNames().ToArray();

			foreach (var thisProperty in destinationProperties)
			{
				// Ensure the destination can write this property
				var canWrite = thisProperty.CanWrite && thisProperty.SetMethod.IsPublic;
				if (!canWrite)
				{
					continue;
				}

				var isPropertyExcludedForSync = allowSyncExclusions && (IsPropertyExcludedForSync(thisProperty.Name) || update.IsPropertyExcludedForSync(thisProperty.Name));
				var isPropertyExcludedForUpdate = allowUpdateExclusions && (IsPropertyExcludedForUpdate(thisProperty.Name) || update.IsPropertyExcludedForUpdate(thisProperty.Name));

				// We always ignore virtual properties and possible some exclusions
				if (isPropertyExcludedForSync || isPropertyExcludedForUpdate || virtualProperties.Contains(thisProperty.Name))
				{
					continue;
				}

				// Check to see if the update source entity has the property
				var updateProperty = sourceProperties.FirstOrDefault(x => x.Name == thisProperty.Name && x.PropertyType == thisProperty.PropertyType);
				if (updateProperty == null)
				{
					// Skip this because target type does not have correct property.
					continue;
				}

				thisProperty.SetValue(this, updateProperty.GetValue(update));
			}
		}

		/// <inheritdoc />
		public virtual void UpdateLocalSyncIds()
		{
			var type = this.GetRealType();
			var baseEntityType = typeof(ISyncEntity);
			var properties = type.GetCachedProperties().ToList();

			var entityRelationships = properties
				.Where(x => x.GetCachedAccessors()[0].IsVirtual)
				.Where(x => baseEntityType.IsAssignableFrom(x.PropertyType))
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

		#endregion
	}
}