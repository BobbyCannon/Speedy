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
	internal class SyncableRepository<T> : Repository<T>, ISyncableRepository<T> where T : SyncEntity, new()
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
		/// Gets the changes from the repository.
		/// </summary>
		/// <param name="since"> The date and time get changes for. </param>
		/// <returns> The list of changes from the repository. </returns>
		public IEnumerable<SyncObject> GetChanges(DateTime since)
		{
			return this.Where(x => x.ModifiedOn >= since)
				.ToList()
				.Select(x => x.ToSyncObject())
				.Where(x => x != null)
				.ToList();
		}

		/// <summary>
		/// Gets the sync entity by the ID.
		/// </summary>
		/// <param name="syncId"> The ID of the sync entity. </param>
		/// <returns> The sync entity or null. </returns>
		public SyncEntity Read(Guid syncId)
		{
			var state = Cache.FirstOrDefault(x => ((SyncEntity) x.Entity).SyncId == syncId);
			return state == null ? Store?.Read(syncId.ToString()) : (SyncEntity) state.Entity;
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

			base.Remove(entityCheck);
		}

		#endregion
	}
}