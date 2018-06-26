#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

#endregion

namespace Speedy.EntityFramework
{
	/// <summary>
	/// Represents a read only collection of entities for a Speedy database.
	/// </summary>
	/// <typeparam name="T"> The entity type this collection is for. </typeparam>
	/// <typeparam name="T2"> The type of the entity key. </typeparam>
	[ExcludeFromCodeCoverage]
	public class ReadOnlyEntityFrameworkRepository<T, T2> : IRepository<T, T2> where T : Entity<T2>, new()
	{
		#region Fields

		private readonly IQueryable<T> _query;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a repository.
		/// </summary>
		/// <param name="set"> The database set this repository is for. </param>
		public ReadOnlyEntityFrameworkRepository(DbSet<T> set)
		{
			_query = set.AsNoTracking().AsQueryable();
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
		public Type ElementType => _query.ElementType;

		/// <summary>
		/// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable" />.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.Expressions.Expression" /> that is associated with this instance of
		/// <see cref="T:System.Linq.IQueryable" />.
		/// </returns>
		public Expression Expression => _query.Expression;

		/// <summary>
		/// Gets the query provider that is associated with this data source.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.IQueryProvider" /> that is associated with this data source.
		/// </returns>
		public IQueryProvider Provider => _query.Provider;

		#endregion

		#region Methods

		/// <summary>
		/// Add an entity to the repository. The ID of the entity must be the default value.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		public void Add(T entity)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Adds or updates an entity in the repository. The ID of the entity must be the default value to add and a value to
		/// update.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		public void AddOrUpdate(T entity)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator()
		{
			return _query.GetEnumerator();
		}

		/// <summary>
		/// Configures the query to include related entities in the results.
		/// </summary>
		/// <param name="include"> The related entities to include. </param>
		/// <returns> The results of the query including the related entities. </returns>
		public IIncludableQueryable<T, T3> Include<T3>(Expression<Func<T, T3>> include)
		{
			return _query.Include(include);
		}

		/// <summary>
		/// Configures the query to include multiple related entities in the results.
		/// </summary>
		/// <param name="includes"> The related entities to include. </param>
		/// <returns> The results of the query including the related entities. </returns>
		public IIncludableQueryable<T, T3> Including<T3>(params Expression<Func<T, T3>>[] includes)
		{
			return (IIncludableQueryable<T, T3>) includes.Aggregate(_query, (current, include) => current.Include(include));
		}

		/// <summary>
		/// Removes an entity from the repository.
		/// </summary>
		/// <param name="id"> The ID of the entity to remove. </param>
		public void Remove(T2 id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Removes an entity from the repository.
		/// </summary>
		/// <param name="entity"> The entity to remove. </param>
		public void Remove(T entity)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes a set of entities from the repository.
		/// </summary>
		/// <param name="filter"> The filter of the entities to remove. </param>
		public void Remove(Expression<Func<T, bool>> filter)
		{
			throw new NotImplementedException();
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