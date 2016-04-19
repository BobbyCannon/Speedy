#region References

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Speedy.Sync;

#endregion

namespace Speedy.EntityFramework
{
	/// <summary>
	/// Represents a syncable repository.
	/// </summary>
	/// <typeparam name="T"> The entity for the repository. </typeparam>
	public class EntityFrameworkSyncableRepository<T> : EntityFrameworkRepository<T>, ISyncableRepository<T> where T : SyncEntity, new()
	{
		#region Constructors

		/// <summary>
		/// Instantiates a repository.
		/// </summary>
		/// <param name="set"> The database set this repository is for. </param>
		public EntityFrameworkSyncableRepository(DbSet<T> set) : base(set)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// The type name this repository is for. Will be in assembly name format.
		/// </summary>
		public string TypeName => typeof(T).ToAssemblyName();

		/// <summary>
		/// Add an entity to the repository. The ID of the entity must be the default value.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		public void Add(SyncEntity entity)
		{
			base.Add((T) entity);
		}

		/// <summary>
		/// Gets the changes from the repository.
		/// </summary>
		/// <param name="since"> The date and time get changes for. </param>
		/// <returns> The list of changes from the repository. </returns>
		public IEnumerable<SyncObject> GetChanges(DateTime since)
		{
			return Set.Where(x => x.ModifiedOn >= since)
				.ToList()
				.Select(x => x.ToSyncObject())
				.ToList();
		}

		/// <summary>
		/// Gets the sync entity by the ID.
		/// </summary>
		/// <param name="syncId"> The ID of the sync entity. </param>
		/// <returns> The sync entity or null. </returns>
		public SyncEntity Read(Guid syncId)
		{
			return Set.FirstOrDefault(x => x.SyncId == syncId);
		}

		/// <summary>
		/// Remove an entity to the repository.
		/// </summary>
		/// <param name="entity"> The entity to be removed. </param>
		public void Remove(SyncEntity entity)
		{
			base.Remove((T) entity);
		}

		#endregion
	}
}