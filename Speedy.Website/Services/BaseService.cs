#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.Website.Services
{
	public abstract class BaseService
	{
		#region Constructors

		protected BaseService(IContosoDatabase database, AccountEntity account)
		{
			Database = database;
			Account = account;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the current user of this service.
		/// </summary>
		public AccountEntity Account { get; }

		/// <summary>
		/// Gets the database for this service.
		/// </summary>
		public IContosoDatabase Database { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Combines a list of tags into a single string. Stores only distinct tags and joins them with a comma ",".
		/// </summary>
		/// <param name="tags"> The tags to include. </param>
		/// <returns> The string of the tags. </returns>
		public static string CombineTags(params object[] tags)
		{
			return $",{string.Join(",", tags.Select(x => (x?.ToString() ?? string.Empty).Trim()).Distinct().OrderBy(x => x))},";
		}
		
		/// <summary>
		/// Combines a list of tags into a single string. Stores only distinct tags and joins them with a comma ",".
		/// </summary>
		/// <param name="tags"> The tags to include. </param>
		/// <returns> The string of the tags. </returns>
		public static string CombineTags(params string[] tags)
		{
			return $",{string.Join(",", tags.Select(x => x.Trim()).Distinct().OrderBy(x => x))},";
		}

		public static string ConvertTitleForLink(string value)
		{
			var regex = new Regex("[^a-zA-Z\\d]");
			return HttpUtility.HtmlEncode(regex.Replace(value ?? string.Empty, ""));
		}

		public static IEnumerable<string> SplitTags(string tags)
		{
			return tags.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Distinct().OrderBy(x => x).ToArray();
		}

		#endregion
	}
}