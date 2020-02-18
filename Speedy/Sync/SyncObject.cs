#region References

using System;
using System.IO;
using Speedy.Extensions;
using Speedy.Serialization;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents an sync object.
	/// </summary>
	public class SyncObject
	{
		#region Fields

		private static readonly SerializerSettings _cachedSettings;

		#endregion

		#region Constructors

		static SyncObject()
		{
			_cachedSettings = new SerializerSettings(false, false, false, true, true, false);
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

			return Data.FromJson(type, _cachedSettings) as ISyncEntity;
		}

		internal static SyncObject ToSyncObject<T>(SyncEntity<T> syncEntity)
		{
			var json = syncEntity.ToJson(_cachedSettings);

			return new SyncObject
			{
				Data = json,
				ModifiedOn = syncEntity.ModifiedOn,
				SyncId = syncEntity.SyncId,
				TypeName = syncEntity.RealType.ToAssemblyName(),
				Status = syncEntity.CreatedOn == syncEntity.ModifiedOn ? SyncObjectStatus.Added : SyncObjectStatus.Modified
			};
		}

		#endregion
	}
}