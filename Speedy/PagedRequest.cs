#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Speedy.Extensions;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a paged request to a service.
	/// </summary>
	public class PagedRequest : Bindable
	{
		#region Constructors

		/// <summary>
		/// Instantiates a paged request.
		/// </summary>
		public PagedRequest()
		{
			Cleanup();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The filter to limit the request to. Defaults to an empty filter.
		/// </summary>
		public string Filter { get; set; }

		/// <summary>
		/// The optional collection of filter values.
		/// </summary>
		public IList<string> FilterValues { get; set; }

		/// <summary>
		/// The values to be include in the results. Defaults to an empty collection.
		/// </summary>
		public IList<string> Including { get; set; }

		/// <summary>
		/// The optional collection of request options.
		/// </summary>
		public IList<string> Options { get; set; }

		/// <summary>
		/// The optional collection of option values.
		/// </summary>
		public IList<string> OptionValues { get; set; }

		/// <summary>
		/// The value to order the request by.
		/// </summary>
		public string Order { get; set; }

		/// <summary>
		/// The page to start the request on.
		/// </summary>
		public int Page { get; set; }

		/// <summary>
		/// The number of items per page.
		/// </summary>
		public int PerPage { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Add values to be included.
		/// </summary>
		/// <param name="values"> The values to be included. </param>
		public void AddIncluding(params string[] values)
		{
			foreach (var value in values)
			{
				if (Including.Contains(value))
				{
					continue;
				}

				Including.Add(value);
			}
		}

		/// <summary>
		/// Add values of options for the request.
		/// </summary>
		/// <param name="name"> The key for the option. </param>
		/// <param name="value"> The value for the option. </param>
		public void AddOptions(string name, string value)
		{
			var index = Options.IndexOf(name);

			if (index >= 0)
			{
				Options[index] = name;
				OptionValues[index] = value;
			}
			else
			{
				Options.Add(name);
				OptionValues.Add(value);
			}
		}

		/// <summary>
		/// Cleanup the request. Set default values.
		/// </summary>
		public PagedRequest Cleanup(int perPage = 10, int maxPerPage = 100)
		{
			Cleanup(Filter, x => x == null, () => Filter = string.Empty);
			Cleanup(FilterValues, x => x == null, () => FilterValues = new List<string>());
			Cleanup(Including, x => x == null, () => Including = new List<string>());
			Cleanup(Options, x => x == null, () => Options = new List<string>());
			Cleanup(OptionValues, x => x == null, () => OptionValues = new List<string>());
			Cleanup(Order, x => x == null, () => Order = string.Empty);
			Cleanup(Page, x => x <= 0, () => Page = 1);
			Cleanup(PerPage, x => x <= 0, () => PerPage = perPage);
			Cleanup(PerPage, x => x > maxPerPage, () => PerPage = maxPerPage);
			return this;
		}

		/// <summary>
		/// Parse a paged request from the query string.
		/// </summary>
		/// <param name="queryString"> The query string to process. </param>
		/// <returns> The paged request value represented by the query string. </returns>
		public static PagedRequest FromQueryString(string queryString)
		{
			var collection = HttpUtility.ParseQueryString(queryString);
			var response = new PagedRequest();

			if (collection.TryGet(nameof(Page), out var pageValue))
			{
				response.Page = int.TryParse(pageValue, out var pValue) ? pValue : 1;
			}

			if (collection.TryGet(nameof(PerPage), out var perPageValue))
			{
				response.PerPage = int.TryParse(perPageValue, out var pValue) ? pValue : 1;
			}

			if (collection.TryGet(nameof(Options), out var optionsValue))
			{
				response.Options = optionsValue?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
			}

			if (collection.TryGet(nameof(OptionValues), out var optionValuesValue))
			{
				response.OptionValues = optionValuesValue?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
			}

			if (collection.TryGet(nameof(Filter), out var filterValue))
			{
				response.Filter = filterValue;
			}
			
			if (collection.TryGet(nameof(FilterValues), out var filterValuesValue))
			{
				response.FilterValues = filterValuesValue?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
			}

			return response;
		}

		/// <summary>
		/// Get the option value indexed at the key in the option index.
		/// </summary>
		/// <param name="name"> The option name. </param>
		/// <returns> The value in the same index of the key value in the option collection. </returns>
		public string GetOptionValue(string name)
		{
			var options = Options.ToList();
			var values = OptionValues.ToList();
			var i = options.FindIndex(0, x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
			return (i < 0) || (i >= values.Count) ? null : values[i];
		}

		/// <summary>
		/// Performs case insensitive search on the options list.
		/// </summary>
		/// <param name="option"> The option to check for. </param>
		/// <returns> True if the option exists and false if otherwise. </returns>
		public bool HasOption(string option)
		{
			return Options.Any(x => x.Equals(option, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Returns a processed including values. This method separates the key from the filter.
		/// </summary>
		/// <returns> The dictionary of includes. </returns>
		public Dictionary<string, string> ProcessIncluding()
		{
			return Including
				.Select(x => x.Split('|'))
				.ToDictionary(x => x[0].ToLower(), x => x.Length > 1 ? x[1] : string.Empty);
		}

		/// <summary>
		/// Removes value of an option for the request.
		/// </summary>
		/// <param name="name"> The name of the option. </param>
		public void RemoveOption(string name)
		{
			var options = Options.ToList();
			var i = options.FindIndex(0, x => x.Equals(name, StringComparison.OrdinalIgnoreCase));

			if (i < 0)
			{
				return;
			}

			Options.RemoveAt(i);

			if (i >= OptionValues.Count)
			{
				return;
			}

			OptionValues.RemoveAt(i);
		}

		/// <summary>
		/// Convert the request to the query string values.
		/// </summary>
		/// <returns> The request in a query string format. </returns>
		public string ToQueryString()
		{
			var builder = new StringBuilder();

			builder.Append("Page=");
			builder.Append(Page);

			builder.Append("&PerPage=");
			builder.Append(PerPage);

			if (Options.Count > 0)
			{
				foreach (var option in Options)
				{
					builder.Append($"&Options={string.Join(",", HttpUtility.UrlEncode(option))}");
				}
			}

			if (OptionValues.Count > 0)
			{
				foreach (var optionValue in OptionValues)
				{
					builder.Append($"&OptionValues={string.Join(",", HttpUtility.UrlEncode(optionValue))}");
				}
			}

			if (!string.IsNullOrWhiteSpace(Filter))
			{
				builder.Append($"&Filter={HttpUtility.UrlEncode(Filter)}");
			}

			if (FilterValues.Count > 0)
			{
				foreach (var filerValue in FilterValues)
				{
					builder.Append($"&FilterValues={string.Join(",", HttpUtility.UrlEncode(filerValue))}");
				}
			}

			return builder.ToString();
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