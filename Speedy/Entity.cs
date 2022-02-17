#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.Storage;
using ICloneable = Speedy.Serialization.ICloneable;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a Speedy entity.
	/// </summary>
	/// <typeparam name="T"> The type of the entity primary ID. </typeparam>
	public abstract class Entity<T> : Entity
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
		public override void UpdateWith(object update, params string[] exclusions)
		{
			this.UpdateWithUsingReflection(update, exclusions);
		}

		/// <inheritdoc />
		public sealed override void UpdateWith(object update, bool excludeVirtuals, params string[] exclusions)
		{
			var totalExclusions = new HashSet<string>(exclusions);
			if (excludeVirtuals)
			{
				totalExclusions.AddRange(RealType.GetVirtualPropertyNames());
			}

			UpdateWith(update, totalExclusions.ToArray());
		}

		/// <inheritdoc />
		public sealed override void UpdateWith(object update, bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync, bool excludePropertiesForSyncUpdate)
		{
			var exclusions = GetExclusions(RealType, excludePropertiesForIncomingSync, excludePropertiesForOutgoingSync, excludePropertiesForSyncUpdate);
			UpdateWith(update, exclusions.ToArray());
		}

		#endregion
	}

	/// <summary>
	/// Represents a Speedy entity.
	/// </summary>
	public abstract class Entity : IEntity, IUnwrappable
	{
		#region Fields

		/// <summary>
		/// Cache of combination of exclusions.
		/// </summary>
		private static readonly ConcurrentDictionary<ExclusionKey, HashSet<string>> _excludedProperties;

		/// <summary>
		/// All hash sets for types, this is for optimization
		/// </summary>
		private static readonly ConcurrentDictionary<Type, HashSet<string>> _exclusionCacheForChangeTracking;

		/// <summary>
		/// Cached version of the "real" type, meaning not EF proxy but rather root type
		/// </summary>
		private Type _realType;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an entity
		/// </summary>
		protected Entity()
		{
			ChangedProperties = new HashSet<string>();

			_exclusionCacheForChangeTracking.GetOrAdd(RealType, x => new HashSet<string>(GetDefaultExclusionsForChangeTracking()));
		}

		/// <summary>
		/// Instantiates an entity
		/// </summary>
		static Entity()
		{
			_exclusionCacheForChangeTracking = new ConcurrentDictionary<Type, HashSet<string>>();
			ExclusionCacheForIncomingSync = new ConcurrentDictionary<Type, HashSet<string>>();
			ExclusionCacheForOutgoingSync = new ConcurrentDictionary<Type, HashSet<string>>();
			ExclusionCacheForSyncUpdate = new ConcurrentDictionary<Type, HashSet<string>>();
			_excludedProperties = new ConcurrentDictionary<ExclusionKey, HashSet<string>>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The properties that has changed since last <see cref="ResetChangeTracking" /> event.
		/// </summary>
		protected HashSet<string> ChangedProperties { get; }

		/// <summary>
		/// All hash sets for types, this is for optimization
		/// </summary>
		internal static ConcurrentDictionary<Type, HashSet<string>> ExclusionCacheForIncomingSync { get; }

		/// <summary>
		/// All hash sets for types, this is for optimization
		/// </summary>
		internal static ConcurrentDictionary<Type, HashSet<string>> ExclusionCacheForOutgoingSync { get; }

		/// <summary>
		/// All hash sets for types, this is for optimization
		/// </summary>
		internal static ConcurrentDictionary<Type, HashSet<string>> ExclusionCacheForSyncUpdate { get; }

		/// <summary>
		/// Cached version of the "real" type, meaning not EF proxy but rather root type
		/// </summary>
		internal Type RealType => _realType ??= this.GetRealType();

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
		public virtual void EntityDeleted()
		{
		}

		/// <inheritdoc />
		public virtual void EntityModified()
		{
		}

		/// <summary>
		/// Determines if the object has changes.
		/// </summary>
		public virtual bool HasChanges()
		{
			return ChangedProperties.Count > 0;
		}

		/// <inheritdoc />
		public abstract bool IdIsSet();

		/// <inheritdoc />
		public bool IsPropertyExcludedForChangeTracking(string propertyName)
		{
			return _exclusionCacheForChangeTracking[RealType].Contains(propertyName);
		}

		/// <summary>
		/// Notify that a property has changed
		/// </summary>
		/// <param name="propertyName"> The name of the property that changed. </param>
		public virtual void OnPropertyChanged(string propertyName)
		{
			if ((propertyName != null)
				&& !_exclusionCacheForChangeTracking[RealType].Contains(propertyName)
				&& !ChangedProperties.Contains(propertyName))
			{
				ChangedProperties.Add(propertyName);
			}

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Reset the change tracking flag.
		/// </summary>
		public virtual void ResetChangeTracking()
		{
			ChangedProperties.Clear();
		}

		/// <inheritdoc />
		public virtual object ShallowClone()
		{
			var test = Activator.CreateInstance(RealType);
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
			var test = Activator.CreateInstance(RealType);
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

		/// <inheritdoc />
		public abstract void UpdateWith(object update, params string[] exclusions);

		/// <inheritdoc />
		public abstract void UpdateWith(object update, bool excludeVirtuals, params string[] exclusions);

		/// <summary>
		/// Allows updating of one type to another based on member Name and Type. Virtual properties are ignore by default.
		/// </summary>
		/// <param name="update"> The source of the updates. </param>
		/// <param name="excludePropertiesForIncomingSync"> If true excluded properties will not be set during incoming sync. </param>
		/// <param name="excludePropertiesForOutgoingSync"> If true excluded properties will not be set during outgoing sync. </param>
		/// <param name="excludePropertiesForSyncUpdate"> If true excluded properties will not be set during update. </param>
		public abstract void UpdateWith(object update, bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync, bool excludePropertiesForSyncUpdate);

		/// <summary>
		/// Gets the default exclusions for change tracking. Warning: this is called during constructor, overrides need to be
		/// sure to only return static values as to not cause issues.
		/// </summary>
		/// <returns> The values to exclude during change tracking. </returns>
		protected virtual HashSet<string> GetDefaultExclusionsForChangeTracking()
		{
			return new HashSet<string>();
		}

		/// <summary>
		/// Get exclusions for the provided type.
		/// </summary>
		/// <param name="type"> The type to get exclusions for. </param>
		/// <param name="excludePropertiesForIncomingSync"> If true excluded properties will not be set during incoming sync. </param>
		/// <param name="excludePropertiesForOutgoingSync"> If true excluded properties will not be set during outgoing sync. </param>
		/// <param name="excludePropertiesForSyncUpdate"> If true excluded properties will not be set during update. </param>
		/// <returns> The list of members to be excluded. </returns>
		protected static HashSet<string> GetExclusions(Type type, bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync, bool excludePropertiesForSyncUpdate)
		{
			var key = new ExclusionKey(type, excludePropertiesForIncomingSync, excludePropertiesForOutgoingSync, excludePropertiesForSyncUpdate);

			return _excludedProperties.GetOrAdd(key, x =>
			{
				var exclusions = new HashSet<string>();
				exclusions.AddRange(type.GetVirtualPropertyNames());

				if (excludePropertiesForIncomingSync)
				{
					exclusions.AddRange(ExclusionCacheForIncomingSync[type]);
				}

				if (excludePropertiesForOutgoingSync)
				{
					exclusions.AddRange(ExclusionCacheForOutgoingSync[type]);
				}

				if (excludePropertiesForSyncUpdate)
				{
					exclusions.AddRange(ExclusionCacheForSyncUpdate[type]);
				}

				return exclusions;
			});
		}

		#endregion

		#region Events

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}

	/// <summary>
	/// Represents a Speedy entity.
	/// </summary>
	public interface IEntity : INotifyPropertyChanged, IUpdatable, ICloneable
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
		/// Update an entity that has been deleted.
		/// </summary>
		void EntityDeleted();

		/// <summary>
		/// Update an entity that has been modified.
		/// </summary>
		void EntityModified();

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

	/// <summary>
	/// Represents a Speedy entity that track the date and time it was created.
	/// </summary>
	public interface ICreatedEntity : IEntity
	{
		#region Properties

		/// <summary>
		/// Gets or sets the date and time the entity was created.
		/// </summary>
		DateTime CreatedOn { get; set; }

		#endregion
	}

	/// <summary>
	/// Represents a Speedy entity that track the date and time it was last modified.
	/// </summary>
	public interface IModifiableEntity : ICreatedEntity
	{
		#region Properties

		/// <summary>
		/// Gets or sets the date and time the entity was modified.
		/// </summary>
		DateTime ModifiedOn { get; set; }

		#endregion
	}
}