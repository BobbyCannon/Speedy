﻿#region References

using System;
using System.Linq;
using System.Text;
using System.Web;
using Speedy.Converters;
using Speedy.Extensions;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a paged request to a service.
	/// </summary>
	public class PagedRequest : PartialUpdate
	{
		#region Constructors

		/// <summary>
		/// Instantiates a paged request to a service.
		/// </summary>
		public PagedRequest() : this(null)
		{
		}

		/// <summary>
		/// Instantiates a paged request to a service.
		/// </summary>
		/// <param name="dispatcher"> An optional dispatcher. </param>
		public PagedRequest(IDispatcher dispatcher) : base(dispatcher)
		{
		}

		#endregion

		#region Properties

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
			Cleanup(Page, x => x <= 0, () => Page = PageDefault);
			Cleanup(PerPage, x => x <= 0, () => PerPage = PerPageDefault);
			Cleanup(PerPage, x => x > PerPageMaxDefault, () => PerPage = PerPageMaxDefault);
			return this;
		}

		/// <summary>
		/// Parse the paged request values from the query string.
		/// </summary>
		/// <param name="queryString"> The query string to process. </param>
		/// <remarks>
		/// see https://www.ietf.org/rfc/rfc2396.txt for details on url decoding
		/// </remarks>
		public void ParseQueryString(string queryString)
		{
			var collection = HttpUtility.ParseQueryString(queryString);
			var properties = GetType().GetCachedPropertyDictionary();

			foreach (var key in collection.AllKeys)
			{
				if (properties.ContainsKey(key))
				{
					var property = properties[key];

					if (StringConverter.TryParse(property.PropertyType, collection.Get(key), out var result))
					{
						Updates.AddOrUpdate(property.Name, new PartialUpdateValue(property.Name, property.PropertyType, result));
						continue;
					}
				}

				Updates.AddOrUpdate(key, new PartialUpdateValue(key, typeof(string), collection.Get(key)));
			}
		}

		/// <summary>
		/// Convert the request to the query string values.
		/// </summary>
		/// <returns> The request in a query string format. </returns>
		/// <remarks>
		/// see https://www.ietf.org/rfc/rfc2396.txt for details on url encoding
		/// </remarks>
		public string ToQueryString()
		{
			var builder = new StringBuilder();

			foreach (var update in Updates)
			{
				// https://www.ietf.org/rfc/rfc2396.txt
				var name = HttpUtility.UrlEncode(update.Key);
				var value = HttpUtility.UrlEncode(update.Value.Value.ToString());
				builder.Append($"&{name}={value}");
			}

			if ((builder.Length > 0) && (builder[0] == '&'))
			{
				builder.Remove(0, 1);
			}

			return builder.ToString();
		}

		/// <summary>
		/// Update the PagedRequest with an update.
		/// </summary>
		/// <param name="update"> The update to be applied. </param>
		/// <param name="exclusions"> An optional set of properties to exclude. </param>
		public virtual void UpdateWith(PagedRequest update, params string[] exclusions)
		{
			// If the update is null then there is nothing to do.
			if (update == null)
			{
				return;
			}

			// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

			if (exclusions.Length <= 0)
			{
				Page = update.Page;
				PerPage = update.PerPage;
				Updates.Reconcile(update.Updates);
			}
			else
			{
				this.IfThen(_ => !exclusions.Contains(nameof(Page)), x => x.Page = update.Page);
				this.IfThen(_ => !exclusions.Contains(nameof(PerPage)), x => x.PerPage = update.PerPage);
				this.IfThen(_ => !exclusions.Contains(nameof(Updates)), x => x.Updates.Reconcile(update.Updates));
			}

			//base.UpdateWith(update, exclusions);
		}

		/// <inheritdoc />
		public override void UpdateWith(object update, params string[] exclusions)
		{
			switch (update)
			{
				case PagedRequest options:
				{
					UpdateWith(options, exclusions);
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
		protected internal override void RefreshUpdates()
		{
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
}