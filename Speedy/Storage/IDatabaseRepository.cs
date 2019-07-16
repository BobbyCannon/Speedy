#region References

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

#endregion

namespace Speedy.Storage
{
	/// <summary>
	/// This class is an internal class.
	/// </summary>
	internal interface IDatabaseRepository : IDisposable, INotifyCollectionChanged
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

		/// <summary>
		/// Check too see if there are other entities that depends on this entity.
		/// </summary>
		/// <param name="value"> The values to check. </param>
		/// <param name="id"> The ID of the entity. </param>
		/// <returns> True if the entity exist and false if otherwise. </returns>
		bool HasDependentRelationship(object[] value, object id);

		/// <summary>
		/// Reads an object from the repository.
		/// </summary>
		/// <param name="id"> The ID of the item to read. </param>
		/// <returns> The object from the repository. </returns>
		object Read(object id);

		/// <summary>
		/// Remove the dependencies for the entity.
		/// </summary>
		/// <param name="value"> The values to check. </param>
		/// <param name="id"> The ID of the entity. </param>
		void RemoveDependent(object[] value, object id);

		/// <summary>
		/// Save the data to the data store.
		/// </summary>
		/// <returns> The number of items saved. </returns>
		int SaveChanges();

		/// <summary>
		/// Set the foreign key values for this dependent to null.
		/// </summary>
		/// <param name="value"> The values for processing. </param>
		/// <param name="id"> The ID of the entity. </param>
		void SetDependentToNull(object[] value, object id);

		/// <summary>
		/// Sorts the repository by the primary key.
		/// </summary>
		void Sort();

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