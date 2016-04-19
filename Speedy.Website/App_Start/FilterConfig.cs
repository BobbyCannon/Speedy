#region References

using System.Web.Mvc;

#endregion

namespace Speedy.Website
{
	public static class FilterConfig
	{
		#region Methods

		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}

		#endregion
	}
}