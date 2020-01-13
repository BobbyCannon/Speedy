#region References

using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using System.Web.Routing;

#endregion

namespace Speedy.Website
{
	[ExcludeFromCodeCoverage]
	public static class RouteConfig
	{
		#region Methods

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.MapRoute("Account", "Account", new { controller = "Home", action = "Account" });
			routes.MapRoute("Error", "Error", new { controller = "Home", action = "Error" });
			routes.MapRoute("LogIn", "LogIn", new { controller = "Home", action = "LogIn" });
			routes.MapRoute("NotFound", "NotFound", new { controller = "Home", action = "NotFound" });
			routes.MapRoute("Unauthorized", "Unauthorized", new { controller = "Home", action = "Unauthorized" });
			routes.MapRoute("Default", "{controller}/{action}/{id}", new { controller = "Home", action = "Index", id = UrlParameter.Optional });
		}

		#endregion
	}
}