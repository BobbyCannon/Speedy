#region References

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a page of results for a paged request to a service.
	/// </summary>
	/// <typeparam name="T"> The type of the items in the results collection. </typeparam>
	public class PagedResults<T> : PagedRequest, IPagedResults
	{
		#region Constructors

		/// <summary>
		/// Instantiate an instance of the paged results.
		/// </summary>
		public PagedResults() : this(new PagedRequest(), 0)
		{
		}

		/// <summary>
		/// Instantiate an instance of the paged results.
		/// </summary>
		/// <param name="request"> The request for the results. </param>
		/// <param name="totalCount"> The total amount of items for the request. </param>
		/// <param name="results"> The items in this set of results. </param>
		public PagedResults(PagedRequest request, int totalCount, params T[] results)
		{
			Initialize(request);
			
			Results = results.ToList();
			TotalCount = totalCount;

			// Ensure page is not greater than the total pages
			Page = TotalPages < Page ? TotalPages : Page;
		}

		#endregion

		/// <summary>
		/// Update the PagedRequest with an update.
		/// </summary>
		/// <param name="update"> The update to be applied. </param>
		private void Initialize(PagedRequest update)
		{
			Filter = update.Filter;
			Order = update.Order;
			Page = update.Page;
			PerPage = update.PerPage;
			Updates.Reconcile(update.Updates);
		}

		/// <summary>
		/// Convert the results of to a different type.
		/// </summary>
		/// <typeparam name="T2"> The type to convert into. </typeparam>
		/// <param name="convert"> The function to convert from the current type into the requested type. </param>
		/// <returns> The new paged results for the provided type. </returns>
		public PagedResults<T2> ConvertResults<T2>(Func<T, T2> convert)
		{
			var response = new PagedResults<T2>
			{
				Results = Results.Select(convert).ToList(),
				Filter = Filter,
				Order = Order,
				Page = Page,
				PerPage = PerPage,
				TotalCount = TotalCount
			};

			response.Updates.Reconcile(Updates);
			response.Options.UpdateWith(Options);

			return response;
		}

		#region Properties

		/// <inheritdoc />
		public bool HasMore => (Page > 0) && (Page < TotalPages);

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
		public int TotalPages => TotalCount > 0 ? (TotalCount / PerPage) + ((TotalCount % PerPage) > 0 ? 1 : 0) : 1;

		#endregion

		#region Methods

		/// <inheritdoc />
		public (int start, int end) CalculatePaginationValues()
		{
			var start = Page - 2;
			var end = Page + 2;

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

			RefreshUpdates();

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