#region References

using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http;
using Speedy.Website.Attributes;

#endregion

namespace Speedy.Website
{
	[ExcludeFromCodeCoverage]
	public static class WebApiConfig
	{
		#region Methods

		public static void Register(HttpConfiguration config)
		{
			var requestPerSecond = int.Parse(ConfigurationManager.AppSettings["WebApiRequestPerSecond"]);

			config.MapHttpAttributeRoutes();
			config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
			config.Filters.Add(new AuthorizeAttribute());
			config.Filters.Add(new ThrottleAttribute { RequestsPerSecond = requestPerSecond });
			config.Filters.Add(new WebApiExceptionFilterAttribute());
		}

		#endregion
	}
}