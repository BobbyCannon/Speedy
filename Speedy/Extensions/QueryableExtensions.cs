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
		/// <typeparam name="T"> The type of the item returned. </typeparam>
		/// <param name="query"> The queryable collection. </param>
		/// <param name="request"> The request values. </param>
		/// <returns> The paged results. </returns>
		public static PagedResults<T> GetPagedResults<T>(this IQueryable<T> query, PagedRequest request)
		{
			var total = query.Count();
			var results = query
				.Skip((request.Page - 1) * request.PerPage)
				.Take(request.PerPage)
				.ToArray();

			var response = (PagedResults<T>) Activator.CreateInstance(typeof(PagedResults<T>), request, total, results);
			return response;
		}

		/// <summary>
		/// Gets paged results.
		/// </summary>
		/// <typeparam name="T"> The type of the item returned. </typeparam>
		/// <typeparam name="T2"> The PagedResults type </typeparam>
		/// <typeparam name="T3"> The PagedRequests for the PagedResults. </typeparam>
		/// <param name="query"> The queryable collection. </param>
		/// <param name="request"> The request values. </param>
		/// <returns> The paged results. </returns>
		public static T2 GetPagedResults<T, T2, T3>(this IQueryable<T> query, T3 request)
			where T2 : PagedResults<T, T3>
			where T3 : PagedRequest
		{
			var total = query.Count();
			var results = query
				.Skip((request.Page - 1) * request.PerPage)
				.Take(request.PerPage)
				.ToArray();

			var response = (T2) Activator.CreateInstance(typeof(T2), request, total, results);
			return response;
		}

		/// <summary>
		/// Gets paged results. Transform is executed as part of the query.
		/// </summary>
		/// <typeparam name="T1"> The type of item in the query. </typeparam>
		/// <typeparam name="T2"> The type of the item returned. </typeparam>
		/// <param name="query"> The queryable collection. </param>
		/// <param name="request"> The request values. </param>
		/// <param name="tranform"> The function to transform the results. </param>
		/// <returns> The paged results. </returns>
		public static PagedResults<T2> GetPagedResults<T1, T2>(this IQueryable<T1> query, PagedRequest request, Expression<Func<T1, T2>> tranform)
		{
			var total = query.Count();
			var results = query
				.Skip((request.Page - 1) * request.PerPage)
				.Take(request.PerPage)
				.Select(tranform)
				.ToArray();

			var response = (PagedResults<T2>) Activator.CreateInstance(typeof(PagedResults<T2>), request, total, results);
			return response;
		}

		/// <summary>
		/// Gets paged results. Transform is executed as part of the query.
		/// </summary>
		/// <typeparam name="T1"> The type of item in the query. </typeparam>
		/// <typeparam name="T2"> The type of the item returned. </typeparam>
		/// <param name="query"> The queryable collection. </param>
		/// <param name="request"> The request values. </param>
		/// <param name="tranform"> The function to transfer the results. </param>
		/// <param name="order"> An optional order of the collection. </param>
		/// <returns> The paged results. </returns>
		public static PagedResults<T2> GetPagedResults<T1, T2>(this IQueryable<T1> query, PagedRequest request, Expression<Func<T1, T2>> tranform, Expression<Func<T1, object>> order)
		{
			return GetPagedResults(query, request, tranform, order == null ? null : new OrderBy<T1>(order));
		}

		/// <summary>
		/// Gets paged results. Transform is executed as part of the query.
		/// </summary>
		/// <typeparam name="T1"> The type of item in the query. </typeparam>
		/// <typeparam name="T2"> The type of the item returned. </typeparam>
		/// <param name="query"> The queryable collection. </param>
		/// <param name="request"> The request values. </param>
		/// <param name="transform"> The function to transfer the results. </param>
		/// <param name="order"> The order of the collection. </param>
		/// <param name="thenBys"> An optional then bys to order the collection. </param>
		/// <returns> The paged results. </returns>
		public static PagedResults<T2> GetPagedResults<T1, T2>(this IQueryable<T1> query, PagedRequest request, Expression<Func<T1, T2>> transform, OrderBy<T1> order, params OrderBy<T1>[] thenBys)
		{
			var orderedQuery = order?.Process(query, thenBys) ?? query;
			var total = query.Count();
			var results = orderedQuery
				.Skip((request.Page - 1) * request.PerPage)
				.Take(request.PerPage)
				.Select(transform)
				.ToArray();

			var response = (PagedResults<T2>) Activator.CreateInstance(typeof(PagedResults<T2>), request, total, results);
			return response;
		}

		/// <summary>
		/// Gets paged results. Transform is executed as part of the query.
		/// </summary>
		/// <typeparam name="T1"> The type of item in the query. </typeparam>
		/// <typeparam name="T2"> The type of the item returned. </typeparam>
		/// <typeparam name="T3"> The PagedResults type </typeparam>
		/// <typeparam name="T4"> The PagedRequests for the PagedResults. </typeparam>
		/// <param name="query"> The queryable collection. </param>
		/// <param name="request"> The request values. </param>
		/// <param name="transform"> The function to transfer the results. </param>
		/// <returns> The paged results. </returns>
		public static T3 GetPagedResults<T1, T2, T3, T4>(this IQueryable<T1> query, T4 request, Expression<Func<T1, T2>> transform)
			where T3 : PagedResults<T2, T4>
			where T4 : PagedRequest
		{
			var total = query.Count();
			var results = query
				.Skip((request.Page - 1) * request.PerPage)
				.Take(request.PerPage)
				.Select(transform)
				.ToArray();

			var response = (T3) Activator.CreateInstance(typeof(T3), request, total, results);
			return response;
		}

		/// <summary>
		/// Gets paged results. Transform is executed as part of the query.
		/// </summary>
		/// <typeparam name="T1"> The type of item in the query. </typeparam>
		/// <typeparam name="T2"> The type of the item returned. </typeparam>
		/// <typeparam name="T3"> The PagedResults type </typeparam>
		/// <typeparam name="T4"> The PagedRequests for the PagedResults. </typeparam>
		/// <param name="query"> The queryable collection. </param>
		/// <param name="request"> The request values. </param>
		/// <param name="transform"> The function to transfer the results. </param>
		/// <param name="order"> An optional order of the collection. </param>
		/// <returns> The paged results. </returns>
		public static T3 GetPagedResults<T1, T2, T3, T4>(this IQueryable<T1> query, T4 request, Expression<Func<T1, T2>> transform, Expression<Func<T1, object>> order)
			where T3 : PagedResults<T2, T4>
			where T4 : PagedRequest
		{
			return GetPagedResults<T1, T2, T3, T4>(query, request, transform, order == null ? null : new OrderBy<T1>(order));
		}

		/// <summary>
		/// Gets paged results. Transform is executed as part of the query.
		/// </summary>
		/// <typeparam name="T1"> The type of item in the query. </typeparam>
		/// <typeparam name="T2"> The type of the item returned. </typeparam>
		/// <typeparam name="T3"> The PagedResults type </typeparam>
		/// <typeparam name="T4"> The PagedRequests for the PagedResults. </typeparam>
		/// <param name="query"> The queryable collection. </param>
		/// <param name="request"> The request values. </param>
		/// <param name="transform"> The function to transfer the results. </param>
		/// <param name="order"> The order of the collection. </param>
		/// <param name="thenBys"> An optional then bys to order the collection. </param>
		/// <returns> The paged results. </returns>
		public static T3 GetPagedResults<T1, T2, T3, T4>(this IQueryable<T1> query, T4 request, Expression<Func<T1, T2>> transform, OrderBy<T1> order, params OrderBy<T1>[] thenBys)
			where T3 : PagedResults<T2, T4>
			where T4 : PagedRequest
		{
			var orderedQuery = order?.Process(query, thenBys) ?? query;
			var total = query.Count();
			var results = orderedQuery
				.Skip((request.Page - 1) * request.PerPage)
				.Take(request.PerPage)
				.Select(transform)
				.ToArray();

			var response = (T3) Activator.CreateInstance(typeof(T3), request, total, results);
			return response;
		}

		/// <summary>
		/// Gets paged results. Transform is executed on the client after the results are queried.
		/// </summary>
		/// <typeparam name="T1"> The type of item in the query. </typeparam>
		/// <typeparam name="T2"> The type of the item returned. </typeparam>
		/// <param name="query"> The queryable collection. </param>
		/// <param name="request"> The request values. </param>
		/// <param name="transform"> The function to transfer the results. </param>
		/// <returns> The paged results. </returns>
		public static PagedResults<T2> GetPagedResultsClientTransform<T1, T2>(this IQueryable<T1> query, PagedRequest request, Func<T1, T2> transform)
		{
			var total = query.Count();
			var results = query
				.Skip((request.Page - 1) * request.PerPage)
				.Take(request.PerPage)
				.ToList()
				.Select(transform)
				.ToArray();

			var response = (PagedResults<T2>) Activator.CreateInstance(typeof(PagedResults<T2>), request, total, results);
			return response;
		}

		/// <summary>
		/// Gets paged results. Transform is executed on the client after the results are queried.
		/// </summary>
		/// <typeparam name="T1"> The type of item in the query. </typeparam>
		/// <typeparam name="T2"> The type of the item returned. </typeparam>
		/// <param name="query"> The queryable collection. </param>
		/// <param name="request"> The request values. </param>
		/// <param name="transform"> The function to transfer the results. </param>
		/// <param name="order"> An optional order of the collection. </param>
		/// <returns> The paged results. </returns>
		public static PagedResults<T2> GetPagedResultsClientTransform<T1, T2>(this IQueryable<T1> query, PagedRequest request, Func<T1, T2> transform, Expression<Func<T1, object>> order)
		{
			return GetPagedResultsClientTransform(query, request, transform, order == null ? null : new OrderBy<T1>(order));
		}

		/// <summary>
		/// Gets paged results. Transform is executed on the client after the results are queried.
		/// </summary>
		/// <typeparam name="T1"> The type of item in the query. </typeparam>
		/// <typeparam name="T2"> The type of the item returned. </typeparam>
		/// <param name="query"> The queryable collection. </param>
		/// <param name="request"> The request values. </param>
		/// <param name="transform"> The function to transfer the results. </param>
		/// <param name="order"> The order of the collection. </param>
		/// <param name="thenBys"> An optional then bys to order the collection. </param>
		/// <returns> The paged results. </returns>
		public static PagedResults<T2> GetPagedResultsClientTransform<T1, T2>(this IQueryable<T1> query, PagedRequest request, Func<T1, T2> transform, OrderBy<T1> order, params OrderBy<T1>[] thenBys)
		{
			var orderedQuery = order?.Process(query, thenBys) ?? query;
			var total = query.Count();
			var results = orderedQuery
				.Skip((request.Page - 1) * request.PerPage)
				.Take(request.PerPage)
				.ToList()
				.Select(transform)
				.ToArray();

			var response = (PagedResults<T2>) Activator.CreateInstance(typeof(PagedResults<T2>), request, total, results);
			return response;
		}

		#endregion
	}
}