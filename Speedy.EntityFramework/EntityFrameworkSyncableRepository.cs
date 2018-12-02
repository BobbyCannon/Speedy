#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Speedy.Sync;

#endregion

namespace Speedy.EntityFramework
{
	/// <summary>
	/// Represents a syncable repository.
	/// </summary>
	/// <typeparam name="T"> The entity for the repository. </typeparam>
	public class EntityFrameworkSyncableRepository<T> : EntityFrameworkRepository<T, int>, ISyncableRepository<T> where T : SyncEntity, new()
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

		#region Properties

		/// <summary>
		/// The type name this repository is for. Will be in assembly name format.
		/// </summary>
		public string TypeName => typeof(T).ToAssemblyName();

		#endregion

		#region Methods

		/// <summary>
		/// Add an entity to the repository. The ID of the entity must be the default value.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		public void Add(SyncEntity entity)
		{
			base.Add((T) entity);
		}

		/// <summary>
		/// Gets the count of changes from the repository.
		/// </summary>
		/// <param name="since"> The start date and time get changes for. </param>
		/// <param name="until"> The end date and time get changes for. </param>
		/// <returns> The count of changes from the repository. </returns>
		public int GetChangeCount(DateTime since, DateTime until)
		{
			return GetChangesQuery(since, until).Count();
		}

		/// <summary>
		/// Gets the changes from the repository.
		/// </summary>
		/// <param name="since"> The start date and time get changes for. </param>
		/// <param name="until"> The end date and time get changes for. </param>
		/// <param name="skip"> The number of items to skip. </param>
		/// <param name="take"> The number of items to take. </param>
		/// <returns> The list of changes from the repository. </returns>
		public IEnumerable<SyncObject> GetChanges(DateTime since, DateTime until, int skip, int take)
		{
			var query = GetChangesQuery(since, until);

			if (skip > 0)
			{
				query = query.Skip(skip);
			}

			return query
				.Take(take)
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

		private IQueryable<T> GetChangesQuery(DateTime since, DateTime until)
		{
			return Set.Where(x => x.ModifiedOn >= since && x.ModifiedOn < until)
				.OrderBy(x => x.ModifiedOn)
				.ThenBy(x => x.Id)
				.AsQueryable();
		}

		#endregion
	}
}