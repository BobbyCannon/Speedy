#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a collection of entities for a Speedy database.
	/// </summary>
	/// <typeparam name="T"> The type of the entity of the collection. </typeparam>
	/// <typeparam name="T2"> The type of the entity key. </typeparam>
	public interface IRepository<T, in T2> : IQueryable<T> where T : Entity<T2>
	{
		#region Methods

		/// <summary>
		/// Add an entity to the repository. The ID of the entity must be the default value.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		void Add(T entity);

		/// <summary>
		/// Adds or updates an entity in the repository. The ID of the entity must be the default value to add and a value to
		/// update.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		void AddOrUpdate(T entity);

		/// <summary>
		/// Bulk add to do more performant additions.
		/// </summary>
		/// <param name="entities"> The items to be inserted. </param>
		public int BulkAdd(params T[] entities);

		/// <summary>
		/// Bulk add to do more performant additions.
		/// </summary>
		/// <param name="entities"> The items to be inserted. </param>
		public int BulkAdd(IEnumerable<T> entities);

		/// <summary>
		/// Bulk command to do more performant additions and / or updates.
		/// </summary>
		/// <param name="entities"> The items to be inserted. </param>
		public int BulkAddOrUpdate(params T[] entities);

		/// <summary>
		/// Bulk command to do more performant additions and / or updates.
		/// </summary>
		/// <param name="entities"> The items to be inserted. </param>
		public int BulkAddOrUpdate(IEnumerable<T> entities);

		/// <summary>
		/// Bulk remove based on provided filter. Only simple expressions are supported.
		/// </summary>
		/// <param name="filter"> The filter for the items to be removed. </param>
		/// <remarks>
		/// Please let me know if you find an expression that does not work.
		/// </remarks>
		int BulkRemove(Expression<Func<T, bool>> filter);

		/// <summary>
		/// Bulk update base on provided query and update expression. Only simple expressions are supported.
		/// </summary>
		/// <param name="filter"> The filter for the items to be updated. </param>
		/// <param name="update"> The update to be applied. </param>
		/// <remarks>
		/// Please let me know if you find an expression that does not work.
		/// </remarks>
		int BulkUpdate(Expression<Func<T, bool>> filter, Expression<Func<T, T>> update);

		/// <summary>
		/// Configures the query to include related entities in the results.
		/// </summary>
		/// <param name="include"> The related entities to include. </param>
		/// <returns> The results of the query including the related entities. </returns>
		IIncludableQueryable<T, T3> Include<T3>(Expression<Func<T, T3>> include);

		/// <summary>
		/// Configures the query to include multiple related entities in the results.
		/// </summary>
		/// <param name="includes"> The related entities to include. </param>
		/// <returns> The results of the query including the related entities. </returns>
		IIncludableQueryable<T, object> Including(params Expression<Func<T, object>>[] includes);

		/// <summary>
		/// Configures the query to include multiple related entities in the results.
		/// </summary>
		/// <param name="includes"> The related entities to include. </param>
		/// <returns> The results of the query including the related entities. </returns>
		IIncludableQueryable<T, T3> Including<T3>(params Expression<Func<T, T3>>[] includes);

		/// <summary>
		/// Removes an entity from the repository.
		/// </summary>
		/// <param name="id"> The ID of the entity to remove. </param>
		void Remove(T2 id);

		/// <summary>
		/// Removes an entity from the repository.
		/// </summary>
		/// <param name="entity"> The entity to remove. </param>
		void Remove(T entity);

		/// <summary>
		/// Removes a set of entities from the repository.
		/// </summary>
		/// <param name="filter"> The filter of the entities to remove. </param>
		void Remove(Expression<Func<T, bool>> filter);

		#endregion
	}
}