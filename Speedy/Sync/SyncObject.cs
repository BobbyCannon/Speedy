#region References

using System;
using System.Collections.Concurrent;
using System.IO;
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

		internal static JsonSerializerSettings GetOrAddCachedSettings(Type value)
		{
			return _cachedSettings.GetOrAdd(value.ToAssemblyName(), x => value.ToJsonSettings(ignoreReadOnly: true, ignoreVirtuals: true));
		}

		private static JsonSerializerSettings GetCachedSettings(string name)
		{
			return _cachedSettings.TryGetValue(name, out var value) ? value : null;
		}

		#endregion
	}
}