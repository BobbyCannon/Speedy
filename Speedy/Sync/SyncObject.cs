#region References

using System;
using System.IO;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents an sync object.
	/// </summary>
	public class SyncObject
	{
		#region Properties

		/// <summary>
		/// The serialized data of the object being synced.
		/// </summary>
		public string Data { get; set; }

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
		public SyncEntity ToSyncEntity()
		{
			var type = Type.GetType(TypeName);
			if (type == null)
			{
				throw new InvalidDataException("The sync object has an invalid type name.");
			}

			if (Status != SyncObjectStatus.Deleted)
			{
				return Data.FromJson(type) as SyncEntity;
			}

			var instance = (SyncEntity) Activator.CreateInstance(type);
			instance.SyncId = SyncId;
			return instance;
		}

		#endregion
	}
}