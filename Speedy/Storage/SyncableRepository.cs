#region References

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
	[Serializable]
	internal class SyncableRepository<T> : Repository<T,int>, ISyncableRepository<T> where T : SyncEntity, new()
	{
		#region Constructors

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

		/// <summary>
		/// Add an entity to the repository. The ID of the entity must be the default value.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		public void Add(SyncEntity entity)
		{
			var entityCheck = entity as T;
			if (entityCheck == null)
			{
				throw new ArgumentException("The entity is not of the correct type.");
			}

			base.Add(entityCheck);
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
				.Where(x => x != null);
		}

		/// <summary>
		/// Gets the sync entity by the ID.
		/// </summary>
		/// <param name="syncId"> The ID of the sync entity. </param>
		/// <returns> The sync entity or null. </returns>
		public SyncEntity Read(Guid syncId)
		{
			var state = Cache.FirstOrDefault(x => ((SyncEntity) x.Entity).SyncId == syncId);
			return state == null ? Store?.FirstOrDefault(x => x.SyncId == syncId) : (SyncEntity) state.Entity;
		}

		/// <summary>
		/// Remove an entity to the repository.
		/// </summary>
		/// <param name="entity"> The entity to be removed. </param>
		public void Remove(SyncEntity entity)
		{
			var entityCheck = entity as T;
			if (entityCheck == null)
			{
				throw new ArgumentException("The entity is not of the correct type.");
			}

			var foundEntity = Cache.FirstOrDefault(x => x.Entity.Id == entity.Id);

			if (foundEntity == null)
			{
				foundEntity = new EntityState<T, int> { Entity = new T { Id = entity.Id, SyncId = entity.SyncId } };
				Cache.Add(foundEntity);
			}

			foundEntity.State = EntityStateType.Removed;
		}

		private IQueryable<T> GetChangesQuery(DateTime since, DateTime until)
		{
			return this.Where(x => x.ModifiedOn >= since && x.ModifiedOn < until)
				.OrderBy(x => x.ModifiedOn)
				.ThenBy(x => x.Id)
				.AsQueryable();
		}

		#endregion
	}
}