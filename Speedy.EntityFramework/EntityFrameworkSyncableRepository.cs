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
	/// <typeparam name="T2"> The type of the entity key. </typeparam>
	public class EntityFrameworkSyncableRepository<T, T2> : EntityFrameworkRepository<T, T2>, ISyncableRepository<T, T2> where T : SyncEntity<T2>
	{
		#region Constructors

		/// <summary>
		/// Instantiates a repository.
		/// </summary>
		/// <param name="database"> The database where this repository resides. </param>
		/// <param name="set"> The database set this repository is for. </param>
		public EntityFrameworkSyncableRepository(EntityFrameworkDatabase database, DbSet<T> set) : base(database, set)
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

		/// <inheritdoc />
		public void Add(ISyncEntity entity)
		{
			base.Add((T) entity);
		}

		/// <inheritdoc />
		public int GetChangeCount(DateTime since, DateTime until, SyncRepositoryFilter filter)
		{
			return GetChangesQuery(since, until, filter).Count();
		}

		/// <inheritdoc />
		public IEnumerable<SyncObject> GetChanges(DateTime since, DateTime until, int skip, int take, SyncRepositoryFilter filter)
		{
			var query = GetChangesQuery(since, until, filter);

			if (skip > 0)
			{
				query = query.Skip(skip);
			}

			var entities = query.Take(take).ToList();
			var objects = entities.Select(x => x.ToSyncObject()).ToList();
			return objects;
		}

		/// <inheritdoc />
		public ISyncEntity Read(Guid syncId)
		{
			return Set.FirstOrDefault(x => x.SyncId == syncId);
		}

		/// <inheritdoc />
		public void Remove(ISyncEntity entity)
		{
			base.Remove((T) entity);
		}

		private IQueryable<T> GetChangesQuery(DateTime since, DateTime until, SyncRepositoryFilter filter)
		{
			var query = Set.Where(x => (x.CreatedOn >= since && x.CreatedOn < until)
				|| (x.ModifiedOn >= since && x.ModifiedOn < until));

			if (filter is SyncRepositoryFilter<T> srf && srf.OutgoingExpression != null)
			{
				query = query.Where(srf.OutgoingFilter);
			}

			return query
				.OrderBy(x => x.ModifiedOn)
				.ThenBy(x => x.Id)
				.AsQueryable();
		}

		#endregion
	}
}