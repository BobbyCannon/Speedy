#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a paged request to a service.
	/// </summary>
	public class PagedRequest : PartialUpdate<PagedRequest>, IPagedRequest
	{
		#region Constructors

		/// <summary>
		/// Instantiates a paged request to a service.
		/// </summary>
		public PagedRequest() : this((IDispatcher) null)
		{
		}

		/// <summary>
		/// Instantiates a paged request to a service.
		/// </summary>
		/// <param name="dispatcher"> An optional dispatcher. </param>
		public PagedRequest(IDispatcher dispatcher) : base(dispatcher)
		{
		}

		/// <summary>
		/// Instantiates a paged request to a service.
		/// </summary>
		/// <param name="values"> A set of values to set. </param>
		public PagedRequest(Dictionary<string, object> values) : this(values, null)
		{
		}

		/// <summary>
		/// Instantiates a paged request to a service.
		/// </summary>
		/// <param name="values"> A set of values to set. </param>
		/// <param name="dispatcher"> An optional dispatcher. </param>
		public PagedRequest(Dictionary<string, object> values, IDispatcher dispatcher) : base(dispatcher)
		{
			values.ForEach(x => AddOrUpdate(x.Key, x.Value));
		}

		#endregion

		#region Properties

		/// <summary>
		/// An optional filter value.
		/// </summary>
		public string Filter
		{
			get => Get(nameof(Filter), string.Empty);
			set => Set(nameof(Filter), value);
		}

		/// <inheritdoc />
		public string Order
		{
			get => Get(nameof(Order), string.Empty);
			set => Set(nameof(Order), value);
		}

		/// <summary>
		/// The page to start the request on.
		/// </summary>
		public int Page
		{
			get => Get(nameof(Page), PageDefault);
			set => Set(nameof(Page), value);
		}

		/// <summary>
		/// The number of items per page.
		/// </summary>
		public int PerPage
		{
			get => Get(nameof(PerPage), PerPageDefault);
			set => Set(nameof(PerPage), value);
		}

		/// <summary>
		/// Default value for Page.
		/// </summary>
		protected virtual int PageDefault => 1;

		/// <summary>
		/// Default value for PerPage.
		/// </summary>
		protected virtual int PerPageDefault => 10;

		/// <summary>
		/// Default value for PerPage maximum value.
		/// </summary>
		protected virtual int PerPageMaxDefault => 1000;

		#endregion

		#region Methods

		/// <summary>
		/// Cleanup the request. Set default values.
		/// </summary>
		public virtual PagedRequest Cleanup()
		{
			Cleanup(Filter, x => x == null, () => Filter = string.Empty);
			Cleanup(Order, x => x == null, () => Order = string.Empty);
			Cleanup(Page, x => x <= 0, () => Page = PageDefault);
			Cleanup(PerPage, x => x <= 0, () => PerPage = PerPageDefault);
			Cleanup(PerPage, x => x > PerPageMaxDefault, () => PerPage = PerPageMaxDefault);
			return this;
		}

		/// <summary>
		/// Update the PagedRequest with an update.
		/// </summary>
		/// <param name="update"> The update to be applied. </param>
		/// <param name="exclusions"> An optional set of properties to exclude. </param>
		public virtual bool UpdateWith(PagedRequest update, params string[] exclusions)
		{
			// If the update is null then there is nothing to do.
			if (update == null)
			{
				return false;
			}

			// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

			if (exclusions.Length <= 0)
			{
				Updates.Reconcile(update.Updates);
			}
			else
			{
				this.IfThen(_ => !exclusions.Contains(nameof(Updates)), x => x.Updates.Reconcile(update.Updates));
			}

			return true;
		}

		/// <inheritdoc />
		public override bool UpdateWith(object update, params string[] exclusions)
		{
			return update switch
			{
				PagedRequest options => UpdateWith(options, exclusions),
				_ => base.UpdateWith(update, exclusions)
			};
		}

		/// <inheritdoc />
		protected internal override void RefreshUpdates()
		{
			AddOrUpdate(nameof(Filter), Filter);
			AddOrUpdate(nameof(Order), Order);
			AddOrUpdate(nameof(Page), Page);
			AddOrUpdate(nameof(PerPage), PerPage);
			base.RefreshUpdates();
		}

		/// <summary>
		/// Cleanup a single item based on the test.
		/// </summary>
		/// <typeparam name="T"> The item type to be cleaned up. </typeparam>
		/// <param name="item"> The item to test and clean up. </param>
		/// <param name="test"> The test for the time. </param>
		/// <param name="action"> The action to cleanup the item. </param>
		private static void Cleanup<T>(T item, Func<T, bool> test, Action action)
		{
			if (test(item))
			{
				action();
			}
		}

		#endregion
	}

	/// <summary>
	/// Represents a request for paged results from a service.
	/// </summary>
	public interface IPagedRequest
	{
		#region Properties

		/// <summary>
		/// The filter to limit the request to. Defaults to an empty filter.
		/// </summary>
		string Filter { get; set; }

		/// <summary>
		/// The value to order the request by.
		/// </summary>
		public string Order { get; set; }

		/// <summary>
		/// The page to start the request on.
		/// </summary>
		int Page { get; set; }

		/// <summary>
		/// The number of items per page.
		/// </summary>
		int PerPage { get; set; }

		#endregion
	}
}