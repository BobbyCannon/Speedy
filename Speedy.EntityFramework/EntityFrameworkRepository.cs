#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace Speedy.EntityFramework
{
	/// <summary>
	/// Represents a collection of entities for a Speedy database.
	/// </summary>
	/// <typeparam name="T"> The entity type this collection is for. </typeparam>
	public class EntityFrameworkRepository<T> : IRepository<T> where T : Entity, new()
	{
		#region Fields

		/// <summary>
		/// The set of the entities.
		/// </summary>
		protected readonly DbSet<T> Set;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a repository.
		/// </summary>
		/// <param name="set"> The database set this repository is for. </param>
		public EntityFrameworkRepository(DbSet<T> set)
		{
			Set = set;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the type of the element(s) that are returned when the expression tree associated with this instance of
		/// <see cref="T:System.Linq.IQueryable" /> is executed.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Type" /> that represents the type of the element(s) that are returned when the expression tree
		/// associated with this object is executed.
		/// </returns>
		public Type ElementType => ((IDbSet<T>) Set).ElementType;

		/// <summary>
		/// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable" />.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.Expressions.Expression" /> that is associated with this instance of
		/// <see cref="T:System.Linq.IQueryable" />.
		/// </returns>
		public Expression Expression => ((IDbSet<T>) Set).Expression;

		/// <summary>
		/// Gets the query provider that is associated with this data source.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.IQueryProvider" /> that is associated with this data source.
		/// </returns>
		public IQueryProvider Provider => ((IDbSet<T>) Set).Provider;

		#endregion

		#region Methods

		/// <summary>
		/// Add an entity to the repository. The ID of the entity must be the default value.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		public void Add(T entity)
		{
			Set.Add(entity);
		}

		/// <summary>
		/// Adds or updates an entity in the repository. The ID of the entity must be the default value to add and a value to
		/// update.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		public void AddOrUpdate(T entity)
		{
			Set.AddOrUpdate(entity);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator()
		{
			return ((IDbSet<T>) Set).GetEnumerator();
		}

		/// <summary>
		/// Configures the query to include related entities in the results.
		/// </summary>
		/// <param name="include"> The related entities to include. </param>
		/// <returns> The results of the query including the related entities. </returns>
		public IQueryable<T> Include(Expression<Func<T, object>> include)
		{
			return Set.Include(include);
		}

		/// <summary>
		/// Configures the query to include multiple related entities in the results.
		/// </summary>
		/// <param name="includes"> The related entities to include. </param>
		/// <returns> The results of the query including the related entities. </returns>
		public IQueryable<T> Including(params Expression<Func<T, object>>[] includes)
		{
			return includes.Aggregate(Set.AsQueryable(), (current, include) => current.Include(include));
		}

		/// <summary>
		/// Removes an entity from the repository.
		/// </summary>
		/// <param name="id"> The ID of the entity to remove. </param>
		public void Remove(int id)
		{
			var entity = Set.Local.FirstOrDefault(x => x.Id == id);
			if (entity == null)
			{
				entity = new T { Id = id };
				Set.Attach(entity);
			}

			Set.Remove(entity);
		}

		/// <summary>
		/// Removes an entity from the repository.
		/// </summary>
		/// <param name="entity"> The entity to remove. </param>
		public void Remove(T entity)
		{
			Set.Remove(entity);
		}

		/// <summary>
		/// Removes a set of entities from the repository.
		/// </summary>
		/// <param name="filter"> The filter of the entities to remove. </param>
		public void RemoveRange(Expression<Func<T, bool>> filter)
		{
			Set.RemoveRange(Set.Where(filter));
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}