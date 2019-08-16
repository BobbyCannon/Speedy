#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents an sync object.
	/// </summary>
	public class SyncObject
	{
		#region Fields

		private static readonly ConcurrentDictionary<string, JsonSerializerSettings> _cachedSettings;

		#endregion

		#region Constructors

		static SyncObject()
		{
			_cachedSettings = new ConcurrentDictionary<string, JsonSerializerSettings>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The serialized data of the object being synced.
		/// </summary>
		public string Data { get; set; }

		/// <summary>
		/// The date and time of the synced object.
		/// </summary>
		public DateTime ModifiedOn { get; set; }

		/// <summary>
		/// Gets or sets the status of this sync object.
		/// </summary>
		public SyncObjectStatus Status { get; set; }

		/// <summary>
		/// Gets or sets the ID of the sync object.
		/// </summary>
		public Guid SyncId { get; set; }

		/// <summary>
		/// Gets or sets the type name of the object. The data contains the serialized data.
		/// </summary>
		public string TypeName { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Convert this sync object to a different sync object
		/// </summary>
		/// <typeparam name="T1"> The sync entity type to convert from. </typeparam>
		/// <typeparam name="T2"> The primary key of the entity to convert from. </typeparam>
		/// <typeparam name="T3"> The sync entity type to convert to. </typeparam>
		/// <typeparam name="T4"> The primary key of the entity to convert to. </typeparam>
		/// <param name="convert"> An optional convert method to do some additional conversion. </param>
		/// <param name="excludePropertiesForSync"> If true excluded properties will not be set during sync. </param>
		/// <param name="excludePropertiesForUpdate"> If true excluded properties will not be set during update. </param>
		/// <returns> The converted sync entity in a sync object format. </returns>
		public SyncObject Convert<T1, T2, T3, T4>(Action<T1, T3> convert = null, bool excludePropertiesForSync = true, bool excludePropertiesForUpdate = true)
			where T1 : SyncEntity<T2>
			where T3 : SyncEntity<T4>
		{
			var syncEntity = ToSyncEntity<T1, T2>();

			if (!(syncEntity is T1 source))
			{
				return null;
			}

			var destination = Activator.CreateInstance<T3>();

			// Handle all one to one properties (same name & type) and all sync entity base properties.
			// This will override any exclusions. Meaning this entity will copy all possible properties.
			destination.UpdateWith(source, excludePropertiesForSync, excludePropertiesForUpdate);

			// Update will not set the sync ID
			destination.SyncId = source.SyncId;

			// Optional convert to do additional conversions
			convert?.Invoke(source, destination);

			return destination.ToSyncObject();
		}

		/// <summary>
		/// Convert this collection of sync object to a different sync object collection
		/// </summary>
		/// <typeparam name="T1"> The sync entity type to convert from. </typeparam>
		/// <typeparam name="T2"> The primary key of the entity to convert from. </typeparam>
		/// <typeparam name="T3"> The sync entity type to convert to. </typeparam>
		/// <typeparam name="T4"> The primary key of the entity to convert to. </typeparam>
		/// <param name="objects"> The collection of objects to convert. </param>
		/// <param name="convert"> An optional convert method to do some additional conversion. </param>
		/// <returns> The converted sync entity in a sync object format. </returns>
		public static IEnumerable<SyncObject> Convert<T1, T2, T3, T4>(IEnumerable<SyncObject> objects, Action<T1, T3> convert = null)
			where T1 : SyncEntity<T2>
			where T3 : SyncEntity<T4>
		{
			return objects
				.Select(x => x.Convert<T1, T2, T3, T4>(convert))
				.Where(x => x != null)
				.ToList();
		}

		/// <summary>
		/// Converts the sync object back into it's proper type.
		/// </summary>
		/// <returns> The deserialized sync object. </returns>
		public T ToSyncEntity<T, T2>() where T : SyncEntity<T2>
		{
			return ToSyncEntity() as T;
		}

		/// <summary>
		/// Converts the sync object back into it's proper type.
		/// </summary>
		/// <returns> The deserialized sync object. </returns>
		public ISyncEntity ToSyncEntity()
		{
			var type = Type.GetType(TypeName);

			if (type == null)
			{
				throw new InvalidDataException("The sync object has an invalid type name.");
			}

			var settings = GetCachedSettings(TypeName);

			if (settings == null)
			{
				if (!typeof(ISyncEntity).IsAssignableFrom(type))
				{
					throw new InvalidDataException("The sync object is not a sync entity.");
				}

				settings = GetOrAddCachedSettings(type);
			}

			return Data.FromJson(type, settings) as ISyncEntity;
		}

		internal static JsonSerializerSettings GetCachedSettings(string name)
		{
			return _cachedSettings.TryGetValue(name, out var value) ? value : null;
		}

		internal static JsonSerializerSettings GetOrAddCachedSettings(Type value)
		{
			return _cachedSettings.GetOrAdd(value.ToAssemblyName(), x => value.ToJsonSettings(ignoreReadOnly: true, ignoreVirtuals: true));
		}

		#endregion
	}
}