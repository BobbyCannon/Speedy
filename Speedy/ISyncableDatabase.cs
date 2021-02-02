#region References

using System;
using System.Collections.Generic;
using Speedy.Sync;

#endregion

namespace Speedy
{
	/// <summary>
	/// The interfaces for a Speedy syncable database.
	/// </summary>
	public interface ISyncableDatabase : IDatabase
	{
		#region Properties

		/// <summary>
		/// An optional key manager for caching entity IDs (primary and sync).
		/// </summary>
		DatabaseKeyCache KeyCache { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Gets a list of syncable repositories.
		/// </summary>
		/// <returns> The list of syncable repositories. </returns>
		IEnumerable<ISyncableRepository> GetSyncableRepositories(SyncOptions options);

		/// <summary>
		/// Gets a syncable repository of the requested entity.
		/// </summary>
		/// <typeparam name="T"> The type of the entity to get a repository for. </typeparam>
		/// <typeparam name="T2"> The type of the entity key. </typeparam>
		/// <returns> The repository of entities requested. </returns>
		ISyncableRepository<T, T2> GetSyncableRepository<T, T2>() where T : SyncEntity<T2>;

		/// <summary>
		/// Gets a syncable repository of the requested entity.
		/// </summary>
		/// <returns> The repository of entities requested. </returns>
		ISyncableRepository GetSyncableRepository(Type type);

		#endregion
	}
}