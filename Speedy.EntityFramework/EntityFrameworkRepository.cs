#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Speedy.Exceptions;

#endregion

namespace Speedy.EntityFramework
{
	/// <summary>
	/// Represents a collection of entities for a Speedy database.
	/// </summary>
	/// <typeparam name="T"> The entity type this collection is for. </typeparam>
	/// <typeparam name="T2"> The type of the entity key. </typeparam>
	public class EntityFrameworkRepository<T, T2> : IRepository<T, T2> where T : Entity<T2>
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
		public Type ElementType => ((IQueryable<T>) Set).ElementType;

		/// <summary>
		/// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable" />.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.Expressions.Expression" /> that is associated with this instance of
		/// <see cref="T:System.Linq.IQueryable" />.
		/// </returns>
		public Expression Expression => ((IQueryable<T>) Set).Expression;

		/// <summary>
		/// Gets the query provider that is associated with this data source.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.IQueryProvider" /> that is associated with this data source.
		/// </returns>
		public IQueryProvider Provider => ((IQueryable<T>) Set).Provider;

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
			if (Set.Any(x => x.Id.Equals(entity.Id)))
			{
				Set.Update(entity);
				return;
			}

			Set.Add(entity);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator()
		{
			return ((IQueryable<T>) Set).GetEnumerator();
		}

		/// <summary>
		/// Configures the query to include related entities in the results.
		/// </summary>
		/// <param name="include"> The related entities to include. </param>
		/// <returns> The results of the query including the related entities. </returns>
		public IIncludableQueryable<T, T3> Include<T3>(Expression<Func<T, T3>> include)
		{
			return Including(include);
		}

		/// <summary>
		/// Configures the query to include multiple related entities in the results.
		/// </summary>
		/// <param name="includes"> The related entities to include. </param>
		/// <returns> The results of the query including the related entities. </returns>
		public IIncludableQueryable<T, T3> Including<T3>(params Expression<Func<T, T3>>[] includes)
		{
			var result = includes.Aggregate(Set.AsQueryable(), (current, include) => current.Include(include));
			if (result is Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<T, T3> aiq)
			{
				return new EntityIncludableQueryable<T, T3>(aiq);
			}

			// Try to find the internal includable queryable, not good but it is what we have to do...
			var includableQueryType = (Type) typeof(EntityFrameworkQueryableExtensions)
				.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic)
				.FirstOrDefault(x => x.Name == "IncludableQueryable`2");

			// Check to ensure we found the type
			if (includableQueryType == null)
			{
				throw new SpeedyException("Critical: Need to look into IncludableQueryable");
			}

			// Create an instance of the includable queryable so we can pass it to ThenInclude
			var includableQueryTypeGeneric = includableQueryType.MakeGenericType(typeof(T), typeof(T3));
			var instance = Activator.CreateInstance(includableQueryTypeGeneric, result);
			return new EntityIncludableQueryable<T, T3>((Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<T, T3>) instance);
		}

		/// <summary>
		/// Removes an entity from the repository.
		/// </summary>
		/// <param name="id"> The ID of the entity to remove. </param>
		public void Remove(T2 id)
		{
			var entity = Set.Local.FirstOrDefault(x => Equals(x.Id, id));
			if (entity == null)
			{
				entity = Activator.CreateInstance<T>();
				entity.Id = id;
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
		public void Remove(Expression<Func<T, bool>> filter)
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