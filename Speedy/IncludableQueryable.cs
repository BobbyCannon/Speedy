#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace Speedy
{
	/// <inheritdoc />
	public class IncludableQueryable<T, T2> : IIncludableQueryable<T, T2> where T : class
	{
		#region Fields

		private readonly IQueryable<T> _query;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiate an instance of the IncludableQueryable
		/// </summary>
		/// <param name="query"> </param>
		public IncludableQueryable(IQueryable<T> query)
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
			return new IncludableQueryable<T, TProperty>(_query);
		}

		/// <inheritdoc />
		public IIncludableQueryable<T, TProperty> ThenInclude<TPreviousProperty, TProperty>(Expression<Func<TPreviousProperty, TProperty>> include)
		{
			return new IncludableQueryable<T, TProperty>(_query);
		}

		/// <inheritdoc />
		public IIncludableQueryable<T, TProperty> ThenInclude<TProperty>(Expression<Func<T2, TProperty>> include)
		{
			return new IncludableQueryable<T, TProperty>(_query);
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}