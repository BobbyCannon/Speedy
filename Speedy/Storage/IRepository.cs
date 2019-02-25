#region References

using System;
using System.Collections.Generic;

#endregion

namespace Speedy.Storage
{
	internal interface IRepository : IDisposable
	{
		#region Methods

		/// <summary>
		/// Assign primary keys to the entity.
		/// </summary>
		/// <param name="item"> The entity to assign a key to. </param>
		/// <param name="processed"> The list of entities that have already been processed. </param>
		void AssignKey(IEntity item, List<IEntity> processed);

		/// <summary>
		/// Assign primary keys to all entities.
		/// </summary>
		/// <param name="processed"> The list of entities that have already been processed. </param>
		void AssignKeys(List<IEntity> processed);

		/// <summary>
		/// Discard all changes made in this repository.
		/// </summary>
		int DiscardChanges();

		/// <summary>
		/// Determines if the repository has changes.
		/// </summary>
		/// <returns> True if there are changes and false if otherwise. </returns>
		bool HasChanges();

		bool HasDependentRelationship(object[] value, object id);

		/// <summary>
		/// Reads an object from the repository.
		/// </summary>
		/// <param name="id"> The ID of the item to read. </param>
		/// <returns> The object from the repository. </returns>
		object Read(object id);

		/// <summary>
		/// Save the data to the data store.
		/// </summary>
		/// <returns> The number of items saved. </returns>
		int SaveChanges();

		/// <summary>
		/// Updates the relationships for this entity.
		/// </summary>
		void UpdateRelationships();

		/// <summary>
		/// Validates all entities in the repository.
		/// </summary>
		void ValidateEntities();

		#endregion
	}
}