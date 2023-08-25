#region References

using System;
using System.IO;
using Newtonsoft.Json;
using Speedy.Extensions;
using Speedy.Serialization;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents an sync object.
	/// </summary>
	public struct SyncObject : IComparable<SyncObject>, IEquatable<SyncObject>
	{
		#region Constructors

		static SyncObject()
		{
			CachedSerializerSettings = new SerializerSettings(false, false, false, true, true)
			{
				JsonSettings =
				{
					PreserveReferencesHandling = PreserveReferencesHandling.None
				}
			};
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

		/// <summary>
		/// Cached serializer settings.
		/// </summary>
		internal static SerializerSettings CachedSerializerSettings { get; }

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

			return Data.FromJson(type, CachedSerializerSettings) as ISyncEntity;
		}

		internal static SyncObject ToSyncObject<T>(SyncEntity<T> syncEntity)
		{
			var json = syncEntity.ToJson(CachedSerializerSettings);

			return new SyncObject
			{
				Data = json,
				ModifiedOn = syncEntity.ModifiedOn,
				SyncId = syncEntity.GetEntitySyncId(),
				TypeName = syncEntity.GetRealType().ToAssemblyName(),
				Status = syncEntity.IsDeleted
					? SyncObjectStatus.Deleted
					: syncEntity.CreatedOn == syncEntity.ModifiedOn
						? SyncObjectStatus.Added
						: SyncObjectStatus.Modified
			};
		}

		#endregion

		/// <inheritdoc />
		public bool Equals(SyncObject other)
		{
			return (Data == other.Data)
				&& ModifiedOn.Equals(other.ModifiedOn)
				&& (Status == other.Status)
				&& SyncId.Equals(other.SyncId)
				&& (TypeName == other.TypeName);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is SyncObject other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Data != null ? Data.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ ModifiedOn.GetHashCode();
				hashCode = (hashCode * 397) ^ (int) Status;
				hashCode = (hashCode * 397) ^ SyncId.GetHashCode();
				hashCode = (hashCode * 397) ^ (TypeName != null ? TypeName.GetHashCode() : 0);
				return hashCode;
			}
		}

		/// <inheritdoc />
		public int CompareTo(SyncObject other)
		{
			var dataComparison = string.Compare(Data, other.Data, StringComparison.Ordinal);
			if (dataComparison != 0)
			{
				return dataComparison;
			}
			var modifiedOnComparison = ModifiedOn.CompareTo(other.ModifiedOn);
			if (modifiedOnComparison != 0)
			{
				return modifiedOnComparison;
			}
			var statusComparison = Status.CompareTo(other.Status);
			if (statusComparison != 0)
			{
				return statusComparison;
			}
			var syncIdComparison = SyncId.CompareTo(other.SyncId);
			if (syncIdComparison != 0)
			{
				return syncIdComparison;
			}
			return string.Compare(TypeName, other.TypeName, StringComparison.Ordinal);
		}
	}
}