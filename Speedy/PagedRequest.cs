#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
		/// <param name="key"> The key for the option. </param>
		/// <param name="value"> The value for the option. </param>
		public void AddOptions(string key, string value)
		{
			var index = Options.IndexOf(key);

			if (index >= 0)
			{
				Options[index] = key;
				OptionValues[index] = value;
			}
			else
			{
				Options.Add(key);
				OptionValues.Add(value);
			}
		}

		/// <summary>
		/// Cleanup the request. Set default values.
		/// </summary>
		public void Cleanup(int perPage = 10, int maxPerPage = 100)
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
		}

		/// <summary>
		/// Get the option value indexed at the key in the option index.
		/// </summary>
		/// <param name="option"> The option name. </param>
		/// <returns> The value in the same index of the key value in the option collection. </returns>
		public string GetOptionValue(string option)
		{
			var options = Options.ToList();
			var values = OptionValues.ToList();
			var i = options.IndexOf(option);
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