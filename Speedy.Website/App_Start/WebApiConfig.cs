#region References

using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
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

			// Enable BSON support
			var bson = new BsonMediaTypeFormatter();
			bson.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/bson"));
			config.Formatters.Add(bson);

			config.MapHttpAttributeRoutes();
			config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
			config.Filters.Add(new AuthorizeAttribute());
			config.Filters.Add(new ThrottleAttribute { RequestsPerSecond = requestPerSecond });
			config.Filters.Add(new WebApiExceptionFilterAttribute());
		}

		#endregion
	}
}