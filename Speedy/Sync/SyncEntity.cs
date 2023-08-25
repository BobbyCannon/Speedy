#region References

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy.Sync;

/// <summary>
/// Represent an entity that can be synced.
/// </summary>
/// <typeparam name="T"> The type of the entity primary ID. </typeparam>
public abstract class SyncEntity<T> : Entity<T>, ISyncEntity
{
	#region Constructors

	/// <summary>
	/// Instantiates a sync entity.
	/// </summary>
	protected SyncEntity()
	{
		SyncEntity.ExclusionCacheForIncomingSync.GetOrAdd(GetRealType(), _ => GetDefaultExclusionsForIncomingSync());
		SyncEntity.ExclusionCacheForOutgoingSync.GetOrAdd(GetRealType(), _ => GetDefaultExclusionsForOutgoingSync());
		SyncEntity.ExclusionCacheForSyncUpdate.GetOrAdd(GetRealType(), _ => GetDefaultExclusionsForSyncUpdate());
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
	public Guid GetEntitySyncId()
	{
		return SyncId;
	}

	/// <summary>
	/// Get exclusions for the provided type.
	/// </summary>
	/// <param name="excludePropertiesForIncomingSync"> If true excluded properties will not be set during incoming sync. </param>
	/// <param name="excludePropertiesForOutgoingSync"> If true excluded properties will not be set during outgoing sync. </param>
	/// <param name="excludePropertiesForSyncUpdate"> If true excluded properties will not be set during update. </param>
	/// <returns> The list of members to be excluded. </returns>
	public HashSet<string> GetSyncExclusions(bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync, bool excludePropertiesForSyncUpdate)
	{
		return SyncEntity.GetExclusions(GetRealType(), excludePropertiesForIncomingSync, excludePropertiesForOutgoingSync, excludePropertiesForSyncUpdate);
	}

	/// <inheritdoc />
	public bool IsPropertyExcludedForIncomingSync(string propertyName)
	{
		return SyncEntity.ExclusionCacheForIncomingSync[GetRealType()].Contains(propertyName);
	}

	/// <inheritdoc />
	public bool IsPropertyExcludedForOutgoingSync(string propertyName)
	{
		return SyncEntity.ExclusionCacheForOutgoingSync[GetRealType()].Contains(propertyName);
	}

	/// <inheritdoc />
	public bool IsPropertyExcludedForSyncUpdate(string propertyName)
	{
		return SyncEntity.ExclusionCacheForSyncUpdate[GetRealType()].Contains(propertyName);
	}

	/// <inheritdoc />
	public void SetEntitySyncId(Guid syncId)
	{
		SyncId = syncId;
	}

	/// <inheritdoc />
	public SyncObject ToSyncObject()
	{
		return SyncObject.ToSyncObject(this);
	}

	/// <inheritdoc />
	public bool UpdateSyncEntity(ISyncEntity update, bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync, bool excludePropertiesForSyncUpdate)
	{
		var exclusions = SyncEntity.GetExclusions(GetRealType(), excludePropertiesForIncomingSync, excludePropertiesForOutgoingSync, excludePropertiesForSyncUpdate);
		return UpdateWith(update, exclusions.ToArray());
	}

	/// <summary>
	/// Gets the default exclusions for incoming sync data. Warning: this is called during constructor,
	/// overrides need to be sure to only return static values as to not cause issues.
	/// </summary>
	/// <returns> The values to exclude during sync. </returns>
	protected virtual HashSet<string> GetDefaultExclusionsForIncomingSync()
	{
		return new HashSet<string> { nameof(Id) };
	}

	/// <summary>
	/// Gets the default exclusions for outgoing sync data. Warning: this is called during constructor,
	/// overrides need to be sure to only return static values as to not cause issues.
	/// </summary>
	/// <returns> The values to exclude during sync. </returns>
	protected virtual HashSet<string> GetDefaultExclusionsForOutgoingSync()
	{
		return new HashSet<string> { nameof(Id) };
	}

	/// <summary>
	/// Gets the default exclusions for update. Warning: this is called during constructor, overrides need to be
	/// sure to only return static values as to not cause issues.
	/// </summary>
	/// <returns> The values to exclude during update. </returns>
	protected virtual HashSet<string> GetDefaultExclusionsForSyncUpdate()
	{
		return new HashSet<string> { nameof(Id), nameof(SyncId) };
	}

	/// <summary>
	/// Gets the default properties for partial tracking sync . Warning: this is called during constructor, overrides need to be
	/// sure to only return static values as to not cause issues.
	/// </summary>
	/// <returns> The values to for partial tracking. </returns>
	protected string GetPartialJson(IEnumerable<string> properties)
	{
		var jToken = new JTokenWriter();
		var propertiesInfos = SyncEntity.GetPropertyDictionary(GetRealType());

		jToken.WriteStartObject();

		foreach (var property in properties)
		{
			var propertyInfo = propertiesInfos.ContainsKey(property)
				? propertiesInfos[property]
				: null;

			if (propertyInfo == null)
			{
				continue;
			}

			jToken.WritePropertyName(property);

			var value = propertyInfo.GetValue(this);

			if (value is IEnumerable vArray && (value.GetType() != typeof(string)))
			{
				jToken.WriteStartArray();
				foreach (var item in vArray)
				{
					jToken.WriteValue(item);
				}
				jToken.WriteEndArray();
			}
			else
			{
				jToken.WriteValue(value);
			}
		}

		jToken.WriteEndObject();

		return jToken.Token?.ToString();
	}

	#endregion
}

/// <summary>
/// Internal static class for internal collections
/// </summary>
internal static class SyncEntity
{
	#region Constructors

	static SyncEntity()
	{
		ExclusionCacheForChangeTracking = new ConcurrentDictionary<Type, HashSet<string>>();
		ExclusionCacheForUpdatableExclusions = new ConcurrentDictionary<Type, HashSet<string>>();
		ExclusionCacheForIncomingSync = new ConcurrentDictionary<Type, HashSet<string>>();
		ExclusionCacheForOutgoingSync = new ConcurrentDictionary<Type, HashSet<string>>();
		ExclusionCacheForSyncUpdate = new ConcurrentDictionary<Type, HashSet<string>>();
		ExcludedPropertiesForSyncEntity = new ConcurrentDictionary<ExclusionKey, HashSet<string>>();
		PropertyDictionaryForType = new ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>>();
	}

	#endregion

	#region Properties

	/// <summary>
	/// All hash sets for types, this is for optimization
	/// </summary>
	public static ConcurrentDictionary<Type, HashSet<string>> ExclusionCacheForChangeTracking { get; }

	/// <summary>
	/// All hash sets for types, this is for optimization
	/// </summary>
	public static ConcurrentDictionary<Type, HashSet<string>> ExclusionCacheForUpdatableExclusions { get; }

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
	/// All hash sets for types, this is for optimization
	/// </summary>
	private static ConcurrentDictionary<ExclusionKey, HashSet<string>> ExcludedPropertiesForSyncEntity { get; }

	/// <summary>
	/// Property dictionary for a sync object, this is for optimization
	/// </summary>
	private static ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> PropertyDictionaryForType { get; }

	#endregion

	#region Methods

	public static void AppendToAllExclusions(Type realType, ICollection<string> properties)
	{
		foreach (var update in ExcludedPropertiesForSyncEntity)
		{
			if (update.Key.Type != realType)
			{
				continue;
			}

			if (update.Key.ExcludeIncomingSync
				|| update.Key.ExcludeOutgoingSync
				|| update.Key.ExcludeSyncUpdate)
			{
				update.Value.AddRange(properties);
			}
		}
	}

	/// <summary>
	/// Get sync exclusions for the provided type.
	/// </summary>
	/// <param name="type"> The type to get exclusions for. </param>
	/// <param name="excludePropertiesForIncomingSync"> If true excluded properties will not be set during incoming sync. </param>
	/// <param name="excludePropertiesForOutgoingSync"> If true excluded properties will not be set during outgoing sync. </param>
	/// <param name="excludePropertiesForSyncUpdate"> If true excluded properties will not be set during update. </param>
	/// <returns> The list of members to be excluded. </returns>
	public static HashSet<string> GetExclusions(Type type, bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync, bool excludePropertiesForSyncUpdate)
	{
		var key = new ExclusionKey(type, excludePropertiesForIncomingSync, excludePropertiesForOutgoingSync, excludePropertiesForSyncUpdate);

		return ExcludedPropertiesForSyncEntity.GetOrAdd(key, x =>
		{
			// Ensure the exclusion have been populated
			Activator.CreateInstance(x.Type);

			var exclusions = new HashSet<string>();
			exclusions.AddRange(ExclusionCacheForUpdatableExclusions[type]);

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

	public static Dictionary<string, PropertyInfo> GetPropertyDictionary(Type type)
	{
		return PropertyDictionaryForType.GetOrAdd(type, x =>
		{
			var properties = x.GetCachedProperties().OrderBy(p => p.Name).ToArray();
			return properties.ToDictionary(p => p.Name, p => p);
		});
	}

	#endregion
}