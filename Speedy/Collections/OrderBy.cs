#region References

using System;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace Speedy.Collections
{
	public class OrderBy<T>
	{
		#region Constructors

		public OrderBy( bool descending = false) : this(x => x, descending)
		{
		}
		
		public OrderBy(Expression<Func<T, object>> filter, bool descending = false)
		{
			Filter = filter;
			Descending = descending;
		}

		#endregion

		#region Properties

		public bool Descending { get; set; }

		public Expression<Func<T, object>> Filter { get; set; }

		#endregion

		#region Methods

		public IOrderedQueryable<T> Process(IQueryable<T> query, params OrderBy<T>[] thenBys)
		{
			var response = Descending ? query.OrderByDescending(Filter) : query.OrderBy(Filter);

			foreach (var thenBy in thenBys)
			{
				response = thenBy.Descending
					? response.ThenByDescending(thenBy.Filter)
					: response.ThenBy(thenBy.Filter);
			}

			return response;
		}

		#endregion
	}
}