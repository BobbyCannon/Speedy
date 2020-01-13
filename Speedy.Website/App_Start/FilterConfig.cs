#region References

using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Speedy.Website.Attributes;

#endregion

namespace Speedy.Website
{
	[ExcludeFromCodeCoverage]
	public static class FilterConfig
	{
		#region Methods

		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
			filters.Add(new MvcAuthorizeAttribute());
			filters.Add(new MvcExceptionFilterAttribute());
		}

		#endregion
	}
}