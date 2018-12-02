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

		private readonly Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<T,T2> _query;
		
		#endregion

		#region Constructors

		/// <inheritdoc />
		public EntityIncludableQueryable(Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<T,T2> query)
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

		public IIncludableQueryable<T, TProperty> ProcessCollectionThenInclude<TPreviousProperty, TProperty>(Expression<Func<TPreviousProperty, TProperty>> include)
		{
			return typeof(IEnumerable<TPreviousProperty>).IsAssignableFrom(typeof(T2)) 
				? new EntityIncludableQueryable<T, TProperty>(((Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<T, IEnumerable<TPreviousProperty>>) _query).ThenInclude(include))
				: null;
		}

		/// <inheritdoc />
		public IIncludableQueryable<T, T3> ThenInclude<T3>(Expression<Func<T2, T3>> include)
		{
			return new EntityIncludableQueryable<T, T3>(_query.ThenInclude(include));
		}

		

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}