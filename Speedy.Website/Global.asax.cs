#region References

using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Speedy.Website.Data.Sql;
using Speedy.Website.Samples;
using Speedy.Website.Services;

#endregion

namespace Speedy.Website
{
	[ExcludeFromCodeCoverage]
	public class WebApplication : HttpApplication
	{
		#region Properties

		public static string ConnectionString => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

		#endregion

		#region Methods

		public static ContosoDatabase GetDatabase(DatabaseOptions options = null)
		{
			return ContosoSqlDatabase.UseSql(ConnectionString, options);
		}

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };
			GlobalConfiguration.Configuration.Formatters.Remove(GlobalConfiguration.Configuration.Formatters.XmlFormatter);
			GlobalConfiguration.Configuration.MessageHandlers.Add(new AuthorizationHeaderHandler());
			GlobalConfiguration.Configuration.EnsureInitialized();

			using (var database = GetDatabase())
			{
				database.Database.Migrate();
			}

			RoleService.Timeout = TimeSpan.FromSeconds(int.TryParse(ConfigurationManager.AppSettings["RoleCacheTimeout"], out var timeout) ? timeout : 60);
		}

		#endregion
	}
}