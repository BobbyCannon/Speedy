#region References

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Speedy.Extensions;
using Speedy.Storage;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a page of results for a paged request to a service.
	/// </summary>
	/// <typeparam name="T"> The type of the items in the results collection. </typeparam>
	public class PagedResults<T> : PagedResults<T, PagedRequest>
	{
		#region Constructors

		/// <summary>
		/// Instantiate an instance of the paged results.
		/// </summary>
		public PagedResults() : this(null, 0)
		{
		}

		/// <summary>
		/// Instantiate an instance of the paged results.
		/// </summary>
		/// <param name="request"> The request for the results. </param>
		/// <param name="totalCount"> The total amount of items for the request. </param>
		/// <param name="results"> The items in this set of results. </param>
		public PagedResults(PagedRequest request, int totalCount, params T[] results)
			: base(request, totalCount, results)
		{
		}

		#endregion
	}

	/// <summary>
	/// Represents a page of results for a paged request to a service.
	/// </summary>
	/// <typeparam name="T"> The type of the items in the results collection. </typeparam>
	/// <typeparam name="T2"> The type of the paged request. </typeparam>
	public abstract class PagedResults<T, T2> : PartialUpdate<PagedResults<T, T2>>, IPagedResults
		where T2 : PagedRequest
	{
		#region Constructors

		/// <summary>
		/// Instantiate an instance of the paged results.
		/// </summary>
		/// <param name="request"> The request for the results. </param>
		/// <param name="totalCount"> The total amount of items for the request. </param>
		/// <param name="results"> The items in this set of results. </param>
		protected PagedResults(T2 request, int totalCount, params T[] results)
		{
			Request = Activator.CreateInstance<T2>();
			Request.UpdateWith(request);

			Results = results.ToList();
			TotalCount = totalCount;

			// Ensure page is not greater than the total pages
			Request.Page = TotalPages < Request.Page ? TotalPages : Request.Page;
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public string Filter
		{
			get => Request.Filter;
			set => Request.Filter = value;
		}

		/// <inheritdoc />
		public bool HasMore => (Request.Page > 0) && (Request.Page < TotalPages);

		/// <inheritdoc />
		public string Order
		{
			get => Request.Order;
			set => Request.Order = value;
		}

		/// <inheritdoc />
		public int Page
		{
			get => Request.Page;
			set => Request.Page = value;
		}

		/// <inheritdoc />
		public int PerPage
		{
			get => Request.PerPage;
			set => Request.PerPage = value;
		}

		/// <summary>
		/// The incoming request. This will be flattened in the JSON.
		/// </summary>
		protected T2 Request { get; set; }

		/// <summary>
		/// The results for a paged request.
		/// </summary>
		public IList<T> Results { get; set; }

		/// <inheritdoc />
		public int TotalCount
		{
			get => Get(nameof(TotalCount), 1);
			set => Set(nameof(TotalCount), value);
		}

		/// <inheritdoc />
		public int TotalPages => TotalCount > 0 ? (TotalCount / Request.PerPage) + ((TotalCount % Request.PerPage) > 0 ? 1 : 0) : 1;

		#endregion

		#region Methods

		/// <inheritdoc />
		public (int start, int end) CalculatePaginationValues()
		{
			var start = Request.Page - 2;
			var end = Request.Page + 2;

			if (start < 1)
			{
				start = 1;
				end = 5;
			}

			if (end > TotalPages)
			{
				end = TotalPages;
				start = end - 4;
			}

			if (start < 1)
			{
				start = 1;
			}

			return (start, end);
		}

		/// <inheritdoc />
		protected internal override ExpandoObject GetExpandoObject()
		{
			var expando = new ExpandoObject();

			Request.RefreshUpdates();
			RefreshUpdates();

			foreach (var update in Request.Updates)
			{
				if (!ShouldProcessProperty(update.Key))
				{
					continue;
				}

				expando.AddOrUpdate(update.Key, update.Value.Value);
			}

			foreach (var update in Updates)
			{
				if (!ShouldProcessProperty(update.Key))
				{
					continue;
				}

				expando.AddOrUpdate(update.Key, update.Value.Value);
			}

			expando.AddOrUpdate(nameof(Results), Results);
			return expando;
		}

		/// <inheritdoc />
		protected internal override void RefreshUpdates()
		{
			// Setting values here
			AddOrUpdate(nameof(Filter), Filter);
			AddOrUpdate(nameof(Order), Order);
			AddOrUpdate(nameof(Page), Page);
			AddOrUpdate(nameof(PerPage), PerPage);
			AddOrUpdate(nameof(TotalCount), TotalCount);

			// Calculated properties here
			AddOrUpdate(nameof(TotalPages), TotalPages);
			AddOrUpdate(nameof(HasMore), HasMore);

			base.RefreshUpdates();
		}

		#endregion
	}

	/// <summary>
	/// Represents a page of results for a paged request to a service.
	/// </summary>
	public interface IPagedResults : IPagedRequest
	{
		#region Properties

		/// <summary>
		/// The value to determine if the request has more pages.
		/// </summary>
		bool HasMore { get; }

		/// <summary>
		/// The total count of items for the request.
		/// </summary>
		int TotalCount { get; set; }

		/// <summary>
		/// The total count of pages for the request.
		/// </summary>
		int TotalPages { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Calculate the start and end pagination values.
		/// </summary>
		/// <returns> </returns>
		public (int start, int end) CalculatePaginationValues();

		#endregion
	}
}