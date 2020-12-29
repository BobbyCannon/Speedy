#region References

using System;
using System.Linq;
using System.Linq.Expressions;
using Speedy.Collections;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for queryable
	/// </summary>
	public static class QueryableExtensions
	{
		#region Methods

		/// <summary>
		/// Gets paged results.
		/// </summary>
		/// <typeparam name="T1"> The type of item in the query. </typeparam>
		/// <typeparam name="T2"> The type of the item returned. </typeparam>
		/// <param name="query"> The queryable collection. </param>
		/// <param name="request"> The request values. </param>
		/// <param name="transform"> The function to transfer the results. </param>
		/// <param name="order"> An optional order of the collection. </param>
		/// <returns> The paged results. </returns>
		public static PagedResults<T2> GetPagedResults<T1, T2>(this IQueryable<T1> query, PagedRequest request, Func<T1, T2> transform, Expression<Func<T1, object>> order = null)
		{
			return GetPagedResults(query, request, transform, order == null ? null : new OrderBy<T1>(order));
		}

		/// <summary>
		/// Gets paged results.
		/// </summary>
		/// <typeparam name="T1"> The type of item in the query. </typeparam>
		/// <typeparam name="T2"> The type of the item returned. </typeparam>
		/// <param name="query"> The queryable collection. </param>
		/// <param name="request"> The request values. </param>
		/// <param name="transform"> The function to transfer the results. </param>
		/// <param name="order"> The order of the collection. </param>
		/// <param name="thenBys"> An optional then bys to order the collection. </param>
		/// <returns> The paged results. </returns>
		public static PagedResults<T2> GetPagedResults<T1, T2>(this IQueryable<T1> query, PagedRequest request, Func<T1, T2> transform, OrderBy<T1> order, params OrderBy<T1>[] thenBys)
		{
			var orderedQuery = order?.Process(query, thenBys) ?? query;
			var response = new PagedResults<T2>
			{
				Filter = request.Filter,
				FilterValues = request.FilterValues,
				Including = request.Including,
				TotalCount = query.Count(),
				Order = request.Order,
				PerPage = request.PerPage
			};

			response.Page = response.TotalPages < request.Page ? response.TotalPages : request.Page;
			response.Results = new BaseObservableCollection<T2>(orderedQuery
				.Skip((response.Page - 1) * response.PerPage)
				.Take(response.PerPage)
				.ToList()
				.Select(transform)
				.ToList());

			return response;
		}

		/// <summary>
		/// Gets paged results.
		/// </summary>
		/// <typeparam name="T1"> The type of item in the query. </typeparam>
		/// <typeparam name="T2"> The type of the item returned. </typeparam>
		/// <param name="query"> The queryable collection. </param>
		/// <param name="request"> The request values. </param>
		/// <param name="transform"> The function to transfer the results. </param>
		/// <returns> The paged results. </returns>
		public static PagedResults<T2> GetPagedResults<T1, T2>(this IQueryable<T1> query, PagedRequest request, Func<T1, T2> transform)
		{
			var response = new PagedResults<T2>
			{
				Filter = request.Filter,
				FilterValues = request.FilterValues,
				TotalCount = query.Count(),
				Order = request.Order,
				PerPage = request.PerPage
			};

			response.Page = response.TotalPages < request.Page ? response.TotalPages : request.Page;
			response.Results = new BaseObservableCollection<T2>(query
				.Skip((response.Page - 1) * response.PerPage)
				.Take(response.PerPage)
				.ToList()
				.Select(transform)
				.ToList());

			return response;
		}

		#endregion
	}
}