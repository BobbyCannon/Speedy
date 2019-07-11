﻿#region References

using System;
using System.Collections.Generic;
using System.Linq;
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
			var objects = entities.Select(x => x.ToSyncObject()).Where(x => x != null).ToList();
			return objects;
		}

		/// <inheritdoc />
		public void Remove(ISyncEntity entity)
		{
			base.Remove((T) entity);
		}

		private IQueryable<T> GetChangesQuery(DateTime since, DateTime until, SyncRepositoryFilter filter)
		{
			var query = this.Where(x => x.ModifiedOn >= since && x.ModifiedOn < until);

			if (filter is SyncRepositoryFilter<T> srf && srf.OutgoingExpression != null)
			{
				query = query.Where(srf.OutgoingFilter);
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

		#endregion
	}
}