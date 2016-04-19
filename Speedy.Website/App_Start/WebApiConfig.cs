#region References

using System.Web.Http;

#endregion

namespace Speedy.Website
{
	public static class WebApiConfig
	{
		#region Methods

		public static void Register(HttpConfiguration config)
		{
			config.MapHttpAttributeRoutes();
			config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
		}

		#endregion
	}
}