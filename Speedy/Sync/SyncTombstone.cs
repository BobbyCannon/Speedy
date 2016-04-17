#region References

using System;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a tombstone for a deleted entity.
	/// </summary>
	public class SyncTombstone : Entity
	{
		#region Properties

		/// <summary>
		/// The sync ID of the entity.
		/// </summary>
		public Guid SyncId { get; set; }

		/// <summary>
		/// The type full name.
		/// </summary>
		public string TypeFullName { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Converts the tombstone back into the original entity.
		/// </summary>
		/// <returns> The sync entity as a deleted entity. </returns>
		public SyncEntity ToSyncEntity()
		{
			var type = Type.GetType(TypeFullName);
			if (type == null)
			{
				return null;
			}

			var instance = Activator.CreateInstance(type) as SyncEntity;
			if (instance == null)
			{
				return null;
			}

			instance.SyncId = SyncId;
			instance.SyncStatus = SyncStatus.Deleted;
			return instance;
		}

		#endregion
	}
}