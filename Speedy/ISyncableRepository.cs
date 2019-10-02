#region References

using System;
using System.Collections.Generic;
using Speedy.Sync;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a collection of entities for a Speedy database.
	/// </summary>
	/// <typeparam name="T"> The type of the entity of the collection. </typeparam>
	/// <typeparam name="T2"> The type of the entity key. </typeparam>
	public interface ISyncableRepository<T, in T2> : ISyncableRepository, IRepository<T, T2> where T : SyncEntity<T2>
	{
	}

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
		/// Adds a sync entity to the repository.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		void Add(ISyncEntity entity);

		/// <summary>
		/// Gets the count of changes from the repository.
		/// </summary>
		/// <param name="since"> The start date and time get changes for. </param>
		/// <param name="until"> The end date and time get changes for. </param>
		/// <param name="filter"> The optional filter expression to filter changes. </param>
		/// <returns> The count of changes from the repository. </returns>
		int GetChangeCount(DateTime since, DateTime until, SyncRepositoryFilter filter);

		/// <summary>
		/// Gets the changes from the repository. The results are read only and will not have tracking enabled.
		/// </summary>
		/// <param name="since"> The start date and time get changes for. </param>
		/// <param name="until"> The end date and time get changes for. </param>
		/// <param name="skip"> The number of items to skip. </param>
		/// <param name="take"> The number of items to take. </param>
		/// <param name="filter"> The optional filter expression to filter changes. </param>
		/// <returns> The list of changes from the repository. </returns>
		IEnumerable<SyncObject> GetChanges(DateTime since, DateTime until, int skip, int take, SyncRepositoryFilter filter);

		/// <summary>
		/// Gets the sync entity by the ID.
		/// </summary>
		/// <param name="syncId"> The ID of the sync entity. </param>
		/// <returns> The sync entity or null. </returns>
		ISyncEntity Read(Guid syncId);
		
		/// <summary>
		/// Gets the sync entity by the ID.
		/// </summary>
		/// <param name="entity"> The entity to use with the filter. </param>
		/// <param name="filter"> An optional sync filter to locate the entity. </param>
		/// <returns> The sync entity or null. </returns>
		ISyncEntity Read(ISyncEntity entity, SyncRepositoryFilter filter);

		/// <summary>
		/// Removes a sync entity to the repository.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		void Remove(ISyncEntity entity);

		#endregion
	}
}