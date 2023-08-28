#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.Sync;

#endregion

namespace Speedy;

/// <summary>
/// Represents a Speedy entity.
/// </summary>
/// <typeparam name="T"> The type of the entity primary ID. </typeparam>
public abstract class Entity<T> : Entity, IUpdateable<T>
{
	#region Properties

	/// <summary>
	/// Gets or sets the ID of the entity.
	/// </summary>
	public abstract T Id { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public override bool IdIsSet()
	{
		return !Equals(Id, default(T));
	}

	/// <summary>
	/// Allows the entity to calculate the next key.
	/// </summary>
	/// <param name="currentKey"> The current version of the key. </param>
	/// <returns> The new key to be used in. </returns>
	public virtual T NewId(ref T currentKey)
	{
		currentKey = currentKey switch
		{
			sbyte sbKey => (T) (object) (sbKey + 1),
			byte bKey => (T) (object) (bKey + 1),
			short sKey => (T) (object) (sKey + 1),
			ushort usKey => (T) (object) (usKey + 1),
			int iKey => (T) (object) (iKey + 1),
			uint uiKey => (T) (object) (uiKey + 1),
			long lKey => (T) (object) (lKey + 1),
			ulong ulKey => (T) (object) (ulKey + 1),
			_ => currentKey
		};

		return currentKey;
	}

	/// <inheritdoc />
	public bool ShouldUpdate(T update)
	{
		return true;
	}

	/// <inheritdoc />
	public override bool TrySetId(string id)
	{
		try
		{
			Id = id.FromJson<T>();
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <inheritdoc />
	public bool TryUpdateWith(T update, params string[] exclusions)
	{
		return UpdateableExtensions.TryUpdateWith(this, update, exclusions);
	}

	/// <inheritdoc />
	public virtual bool UpdateWith(T update, params string[] exclusions)
	{
		return this.UpdateWithUsingReflection(update, exclusions);
	}

	/// <inheritdoc />
	public override bool UpdateWith(object update, params string[] exclusions)
	{
		return this.UpdateWithUsingReflection(update, exclusions);
	}

	/// <inheritdoc />
	public sealed override bool UpdateWith(object update, bool excludeVirtuals, params string[] exclusions)
	{
		var totalExclusions = new HashSet<string>(exclusions);
		if (excludeVirtuals)
		{
			totalExclusions.AddRange(GetRealType().GetVirtualPropertyNames());
		}

		return UpdateWith(update, totalExclusions.ToArray());
	}

	/// <inheritdoc />
	public sealed override bool UpdateWith(object update, bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync, bool excludePropertiesForSyncUpdate)
	{
		var exclusions = SyncEntity.GetExclusions(GetRealType(), excludePropertiesForIncomingSync, excludePropertiesForOutgoingSync, excludePropertiesForSyncUpdate);
		return UpdateWith(update, exclusions.ToArray());
	}

	#endregion
}

/// <summary>
/// Represents a Speedy entity.
/// </summary>
public abstract class Entity : Notifiable, IEntity, IUnwrappable
{
	#region Constructors

	/// <summary>
	/// Instantiates an entity
	/// </summary>
	protected Entity()
	{
		
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public virtual bool CanBeModified()
	{
		return true;
	}

	/// <inheritdoc />
	public object DeepClone(int? maxDepth = null)
	{
		return this.DeepCloneObject(maxDepth);
	}

	/// <inheritdoc />
	public virtual void EntityAdded()
	{
	}

	/// <inheritdoc />
	public virtual void EntityAddedDeletedOrModified()
	{
		if (this is IClientEntity clientEntity)
		{
			clientEntity.LastClientUpdate = TimeService.UtcNow;
		}
	}

	/// <inheritdoc />
	public virtual void EntityDeleted()
	{
	}

	/// <inheritdoc />
	public virtual void EntityModified()
	{
	}

	/// <summary>
	/// Default exclusions are all virtual members.
	/// </summary>
	/// <returns> The list of members to be excluded. </returns>
	public HashSet<string> GetUpdatableExclusions()
	{
		return SyncEntity.ExclusionCacheForUpdatableExclusions[GetRealType()];
	}

	/// <inheritdoc />
	public abstract bool IdIsSet();

	/// <inheritdoc />
	public bool IsPropertyExcludedForChangeTracking(string propertyName)
	{
		return SyncEntity.ExclusionCacheForChangeTracking[GetRealType()].Contains(propertyName);
	}

	/// <inheritdoc />
	public virtual object ShallowClone()
	{
		var test = Activator.CreateInstance(GetRealType());
		if (test is Entity entity)
		{
			entity.UpdateWith(this);
		}
		else
		{
			test.UpdateWithUsingReflection(this);
		}
		return test;
	}

	/// <inheritdoc />
	public abstract bool TrySetId(string id);

	/// <summary>
	/// Unwrap the entity from the proxy. Will ignore virtual properties.
	/// </summary>
	/// <returns>
	/// The real entity unwrapped from the Entity Framework proxy.
	/// </returns>
	public virtual object Unwrap()
	{
		var test = Activator.CreateInstance(GetRealType());
		if (test is Entity entity)
		{
			entity.UpdateWith(this, false, false, false);
		}
		else
		{
			test.UpdateWithUsingReflection(this);
		}
		return test;
	}

	/// <summary>
	/// Update all local sync IDs.
	/// </summary>
	public void UpdateLocalSyncIds()
	{
		var syncEntityInterface = typeof(ISyncEntity);
		var properties = GetRealType().GetCachedProperties().ToList();
		var entityRelationships = properties
			.Where(x => x.IsVirtual())
			.Where(x => syncEntityInterface.IsAssignableFrom(x.PropertyType))
			.ToList();

		foreach (var entityRelationship in entityRelationships)
		{
			var entityRelationshipSyncIdProperty = properties.FirstOrDefault(x => x.Name == $"{entityRelationship.Name}SyncId");

			if (entityRelationship.GetValue(this, null) is ISyncEntity syncEntity && (entityRelationshipSyncIdProperty != null))
			{
				var otherEntitySyncId = (Guid?) entityRelationshipSyncIdProperty.GetValue(this, null);
				var syncEntitySyncId = syncEntity.GetEntitySyncId();
				if (otherEntitySyncId != syncEntitySyncId)
				{
					// resets entitySyncId to entity.SyncId if it does not match
					entityRelationshipSyncIdProperty.SetValue(this, syncEntitySyncId, null);
				}
			}

			// todo: maybe?, support the setting Entity.Id would then query the entity sync id and set it?
		}
	}

		/// <summary>
	/// Allows updating of one type to another based on member Name and Type. Virtual properties are ignore by default.
	/// </summary>
	/// <param name="update"> The source of the updates. </param>
	/// <param name="excludePropertiesForIncomingSync"> If true excluded properties will not be set during incoming sync. </param>
	/// <param name="excludePropertiesForOutgoingSync"> If true excluded properties will not be set during outgoing sync. </param>
	/// <param name="excludePropertiesForSyncUpdate"> If true excluded properties will not be set during update. </param>
	public abstract bool UpdateWith(object update, bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync, bool excludePropertiesForSyncUpdate);

	/// <summary>
	/// Gets the default exclusions for change tracking. Warning: this is called during constructor, overrides need to be
	/// sure to only return static values as to not cause issues.
	/// </summary>
	/// <returns> The values to exclude during change tracking. </returns>
	protected internal virtual HashSet<string> GetDefaultExclusionsForChangeTracking()
	{
		return new HashSet<string>();
	}

	/// <summary>
	/// Gets the default exclusions for updatable. Warning: this is called during constructor, overrides need to be
	/// sure to only return static values as to not cause issues.
	/// </summary>
	/// <returns> The values to exclude during processing <see cref="IUpdateable" />. </returns>
	protected internal virtual HashSet<string> GetDefaultExclusionsForUpdatableExclusions()
	{
		return new HashSet<string>(GetRealType().GetVirtualPropertyNames());
	}

	#endregion
}

/// <summary>
/// Represents a Speedy entity.
/// </summary>
public interface IEntity : INotifyPropertyChanged, IUpdateable, IUpdateableExclusions, IChangeable, ICloneable
{
	#region Methods

	/// <summary>
	/// Checks to see if an entity can be modified.
	/// </summary>
	bool CanBeModified();

	/// <summary>
	/// Update an entity that has been added.
	/// </summary>
	void EntityAdded();

	/// <summary>
	/// Update an entity that has been added, deleted, or modified.
	/// </summary>
	void EntityAddedDeletedOrModified();

	/// <summary>
	/// Update an entity that has been deleted.
	/// </summary>
	void EntityDeleted();

	/// <summary>
	/// Update an entity that has been modified.
	/// </summary>
	void EntityModified();

	/// <summary>
	/// Cached version of the "real" type, meaning not EF proxy but rather root type
	/// </summary>
	Type GetRealType();

	/// <summary>
	/// Determine if the ID is set on the entity.
	/// </summary>
	/// <returns> True if the ID is set or false if otherwise. </returns>
	bool IdIsSet();

	/// <summary>
	/// Checks a property has been excluded for change tracking.
	/// </summary>
	/// <param name="propertyName"> The property name to be tested. </param>
	/// <returns> True if the property is excluded or false if otherwise. </returns>
	bool IsPropertyExcludedForChangeTracking(string propertyName);

	/// <summary>
	/// Try to set the ID from a serialized version.
	/// </summary>
	/// <returns> True if the ID is successfully set or false if otherwise. </returns>
	bool TrySetId(string id);

	#endregion
}
