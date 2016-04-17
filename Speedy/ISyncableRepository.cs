#region References

using System;
using Speedy.Sync;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a syncable repository.
	/// </summary>
	public interface ISyncableRepository
	{
		#region Methods

		/// <summary>
		/// Add an entity to the repository. The ID of the entity must be the default value.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		void Add(SyncEntity entity);

		/// <summary>
		/// Gets the sync entity by the ID.
		/// </summary>
		/// <param name="syncId"> The ID of the sync entity. </param>
		/// <returns> The sync entity or null. </returns>
		SyncEntity Read(Guid syncId);

		/// <summary>
		/// Remove an entity to the repository.
		/// </summary>
		/// <param name="entity"> The entity to be removed. </param>
		void Remove(SyncEntity entity);

		#endregion
	}
}