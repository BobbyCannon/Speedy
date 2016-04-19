﻿#region References

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
		public string TypeName { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Converts the tombstone back into the original entity.
		/// </summary>
		/// <returns> The sync entity as a deleted entity. </returns>
		public SyncObject ToSyncObject()
		{
			var type = Type.GetType(TypeName);
			if (type == null)
			{
				return null;
			}

			return new SyncObject
			{
				TypeName = TypeName,
				SyncId = SyncId,
				Status = SyncStatus.Deleted
			};
		}

		#endregion
	}
}