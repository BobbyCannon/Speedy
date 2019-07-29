#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

#endregion

namespace Speedy.EntityFramework
{
	/// <inheritdoc />
	public class EntityIncludableQueryable<T, T2> : IIncludableQueryable<T, T2> where T : class
	{
		#region Fields

		private readonly Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<T, T2> _query;

		#endregion

		#region Constructors

		/// <inheritdoc />
		public EntityIncludableQueryable(Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<T, T2> query)
		{
			_query = query;
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public Type ElementType => _query.ElementType;

		/// <inheritdoc />
		public Expression Expression => _query.Expression;

		/// <inheritdoc />
		public IQueryProvider Provider => _query.Provider;

		#endregion

		#region Methods

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator()
		{
			return _query.GetEnumerator();
		}

		/// <inheritdoc />
		public IIncludableQueryable<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> include)
		{
			return new EntityIncludableQueryable<T, TProperty>(_query.Include(include));
		}

		/// <inheritdoc />
		public IIncludableQueryable<T, TProperty> ProcessCollectionThenInclude<TPreviousProperty, TProperty>(Expression<Func<TPreviousProperty, TProperty>> include)
		{
			return typeof(IEnumerable<TPreviousProperty>).IsAssignableFrom(typeof(T2))
				? new EntityIncludableQueryable<T, TProperty>(((Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<T, IEnumerable<TPreviousProperty>>) _query).ThenInclude(include))
				: null;
		}

		/// <inheritdoc />
		public IIncludableQueryable<T, TProperty> ThenInclude<TProperty>(Expression<Func<T2, TProperty>> include)
		{
			return new EntityIncludableQueryable<T, TProperty>(_query.ThenInclude(include));
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}