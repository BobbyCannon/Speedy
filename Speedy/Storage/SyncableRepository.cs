#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Exceptions;
using Speedy.Extensions;
using Speedy.Sync;

#endregion

namespace Speedy.Storage
{
	/// <summary>
	/// Represents a collection of entities for a Speedy database.
	/// </summary>
	/// <typeparam name="T"> The type contained in the repository. </typeparam>
	/// <typeparam name="T2"> The type of the entity key. </typeparam>
	[Serializable]
	internal class SyncableRepository<T, T2> : Repository<T, T2>, ISyncableRepository<T, T2> where T : SyncEntity<T2>
	{
		#region Constructors

		/// <summary>
		/// Instantiates a syncable repository for the provided database.
		/// </summary>
		/// <param name="database"> The database this repository is for. </param>
		public SyncableRepository(Database database) : base(database)
		{
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public Type RealType => typeof(T);

		/// <inheritdoc />
		public string TypeName => RealType.ToAssemblyName();

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
			var query = GetChangesQuery(since, until, filter)
				.OrderBy(x => x.ModifiedOn)
				.ThenBy(x => x.Id)
				.AsQueryable();

			if (skip > 0)
			{
				query = query.Skip(skip);
			}

			var entities = query.Take(take).ToList();
			var objects = entities.Select(x => x.ToSyncObject()).Where(x => x != null).ToList();
			return objects;
		}

		/// <inheritdoc />
		public IDictionary<Guid, object> ReadAllKeys()
		{
			return this.ToDictionary(x => x.SyncId, x => (object) x.Id);
		}

		/// <inheritdoc />
		public void Remove(ISyncEntity entity)
		{
			base.Remove((T) entity);
		}

		private IQueryable<T> GetChangesQuery(DateTime since, DateTime until, SyncRepositoryFilter filter)
		{
			var query = this.Where(x => x.CreatedOn >= since && x.CreatedOn < until
				|| x.ModifiedOn >= since && x.ModifiedOn < until);

			// Disable merge because merged expression is very hard to read
			// ReSharper disable once MergeSequentialPatterns
			if (filter is SyncRepositoryFilter<T> srf && srf.OutgoingExpression != null)
			{
				query = query.Where(srf.OutgoingFilter);
			}

			// If we have never synced, meaning we are syncing from DateTime.MinValue, and
			// the repository has a filter that say we should skip deleted item on initial sync.
			// The "SyncEntity.IsDeleted" is a soft deleted flag that suggest an item is deleted
			// but it still exist in the database. If an item is "soft deleted" we will normally
			// still sync the item to allow the clients (non-server) to have the opportunity to
			// hard delete the item on their end.
			if (since == DateTime.MinValue && filter?.SkipDeletedItemsOnInitialSync == true)
			{
				// We can skip soft deleted items that we will hard deleted on clients anyways
				query = query.Where(x => !x.IsDeleted);
			}

			return query
				.OrderBy(x => x.ModifiedOn)
				.ThenBy(x => x.Id)
				.AsQueryable();
		}

		/// <inheritdoc />
		ISyncEntity ISyncableRepository.Read(Guid syncId)
		{
			var state = Cache.FirstOrDefault(x => x.Entity.SyncId == syncId);
			return state?.Entity;
		}

		/// <inheritdoc />
		ISyncEntity ISyncableRepository.Read(ISyncEntity syncEntity, SyncRepositoryFilter filter)
		{
			if (!(syncEntity is T entity))
			{
				throw new SpeedyException(SpeedyException.SyncEntityIncorrectType);
			}

			if (filter is SyncRepositoryFilter<T> srf && srf.LookupFilter != null)
			{
				return Cache.Select(x => x.Entity).AsQueryable().FirstOrDefault(srf.LookupFilter.Invoke(entity));
			}

			var state = Cache.FirstOrDefault(x => x.Entity.SyncId == syncEntity.SyncId);
			return state?.Entity;
		}

		#endregion
	}
}