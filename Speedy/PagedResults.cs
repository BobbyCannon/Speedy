#region References

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a page of results for a paged request to a service.
	/// </summary>
	/// <typeparam name="T"> The type of the items in the results collection. </typeparam>
	public class PagedResults<T> : PagedResults
	{
		#region Constructors

		/// <summary>
		/// Instantiate an instance of the paged results.
		/// </summary>
		public PagedResults()
		{
		}

		/// <summary>
		/// Instantiate an instance of the paged results.
		/// </summary>
		/// <param name="items"> The items in this set of results. </param>
		/// <param name="perPage"> The items per page. </param>
		public PagedResults(IEnumerable<T> items, int perPage = 10)
		{
			var list = items.ToList();
			Results = list;
			PerPage = perPage;
			Page = 1;
			TotalCount = list.Count;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The results for a paged request.
		/// </summary>
		public IEnumerable<T> Results { get; set; }

		#endregion
	}

	/// <summary>
	/// Represents a page of results for a paged request to a service.
	/// </summary>
	public class PagedResults : Bindable
	{
		#region Properties

		/// <summary>
		/// The filter to limit the request to. Defaults to an empty filter.
		/// </summary>
		public string Filter { get; set; }

		/// <summary>
		/// The optional collection of filter values.
		/// </summary>
		public IEnumerable<string> FilterValues { get; set; }

		/// <summary>
		/// The value to determine if the request has more pages.
		/// </summary>
		public bool HasMore => Page > 0 && Page < TotalPages;

		/// <summary>
		/// The values to be include in the results. Defaults to an empty collection.
		/// </summary>
		public IEnumerable<string> Including { get; set; }

		/// <summary>
		/// The order the results are in.
		/// </summary>
		public string Order { get; set; }

		/// <summary>
		/// The page of these results.
		/// </summary>
		public int Page { get; set; }

		/// <summary>
		/// The maximum items per page.
		/// </summary>
		public int PerPage { get; set; }

		/// <summary>
		/// The total count of items for the request.
		/// </summary>
		public int TotalCount { get; set; }

		/// <summary>
		/// The total count of pages for the request.
		/// </summary>
		public int TotalPages => TotalCount > 0 ? TotalCount / PerPage + (TotalCount % PerPage > 0 ? 1 : 0) : 1;

		#endregion

		#region Methods

		/// <summary>
		/// Calculate the start and end pagination values.
		/// </summary>
		/// <returns></returns>
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

		#endregion
	}
}