#region References

using System;
using System.Collections.Generic;
using Speedy.Sync;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a syncable repository.
	/// </summary>
	public interface ISyncableRepository
	{
		#region Properties

		/// <summary>
		/// The type name this repository is for. Will be in assembly name format.
		/// </summary>
		string TypeName { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Add an entity to the repository. The ID of the entity must be the default value.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		void Add(SyncEntity entity);

		/// <summary>
		/// Gets the count of changes from the repository.
		/// </summary>
		/// <param name="since"> The start date and time get changes for. </param>
		/// <param name="until"> The end date and time get changes for. </param>
		/// <returns> The count of changes from the repository. </returns>
		int GetChangeCount(DateTime since, DateTime until);

		/// <summary>
		/// Gets the changes from the repository.
		/// </summary>
		/// <param name="since"> The start date and time get changes for. </param>
		/// <param name="until"> The end date and time get changes for. </param>
		/// <param name="take"> The number of items to take. </param>
		/// <returns> The list of changes from the repository. </returns>
		IEnumerable<SyncObject> GetChanges(DateTime since, DateTime until, int take);

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