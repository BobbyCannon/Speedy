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
	public abstract class PagedResults<T, T2>
		: PartialUpdate<PagedResults<T, T2>>,
			IUpdatable<PagedResults<T, T2>>,
			IPagedResults
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
		public bool HasMore => (Request.Page > 0) && (Request.Page < TotalPages);

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
		public T2 Request { get; }

		/// <summary>
		/// The results for a paged request.
		/// </summary>
		public IList<T> Results { get; set; }

		/// <inheritdoc />
		public int TotalCount { get; set; }

		/// <inheritdoc />
		public int TotalPages => TotalCount > 0 ? (TotalCount / Request.PerPage) + ((TotalCount % Request.PerPage) > 0 ? 1 : 0) : 1;

		#endregion

		#region Methods

		/// <summary>
		/// Calculate the start and end pagination values.
		/// </summary>
		/// <returns> </returns>
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

		/// <summary>
		/// Update the PagedResults with an PagedRequest.
		/// </summary>
		/// <param name="update"> The paged request to be applied. </param>
		/// <param name="exclusions"> An optional set of properties to exclude. </param>
		public void UpdateWith(T2 update, params string[] exclusions)
		{
			Request.UpdateWith(update, exclusions);
		}

		/// <summary>
		/// Update the PagedResults with an update.
		/// </summary>
		/// <param name="update"> The update to be applied. </param>
		/// <param name="exclusions"> An optional set of properties to exclude. </param>
		public void UpdateWith(PagedResults<T, T2> update, params string[] exclusions)
		{
			// If the update is null then there is nothing to do.
			if (update == null)
			{
				return;
			}

			// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

			if (exclusions.Length <= 0)
			{
				ExcludedProperties.Reconcile(update.ExcludedProperties);
				IncludedProperties.Reconcile(update.IncludedProperties);
				Results.Reconcile(update.Results);
				Request.UpdateWith(update.Request);
			}
			else
			{
				this.IfThen(_ => !exclusions.Contains(nameof(ExcludedProperties)), x => x.ExcludedProperties.Reconcile(update.ExcludedProperties));
				this.IfThen(_ => !exclusions.Contains(nameof(IncludedProperties)), x => x.IncludedProperties.Reconcile(update.IncludedProperties));
				this.IfThen(_ => !exclusions.Contains(nameof(Results)), x => x.Results.Reconcile(update.Results));
				this.IfThen(_ => !exclusions.Contains(nameof(Request)), x => x.Request.UpdateWith(update.Request));
			}
		}

		/// <inheritdoc />
		public override void UpdateWith(object update, params string[] exclusions)
		{
			switch (update)
			{
				case PagedResults<T, T2> result:
				{
					UpdateWith(result, exclusions);
					return;
				}
				default:
				{
					base.UpdateWith(update, exclusions);
					return;
				}
			}
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
	public interface IPagedResults
	{
		#region Properties

		/// <summary>
		/// The value to determine if the request has more pages.
		/// </summary>
		bool HasMore { get; }

		/// <summary>
		/// The page to start the request on.
		/// </summary>
		int Page { get; set; }

		/// <summary>
		/// The number of items per page.
		/// </summary>
		int PerPage { get; set; }

		/// <summary>
		/// The total count of items for the request.
		/// </summary>
		int TotalCount { get; set; }

		/// <summary>
		/// The total count of pages for the request.
		/// </summary>
		int TotalPages { get; }

		#endregion
	}
}