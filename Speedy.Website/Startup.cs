#region References

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Speedy.Profiling;
using Speedy.Storage.KeyValue;
using Speedy.Website.Data.Sql;
using Speedy.Website.Models;
using Speedy.Website.Samples;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Newtonsoft.Json;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.Website.Middleware;
using Speedy.Website.Services;
using AuthenticationService = Speedy.Website.Services.AuthenticationService;
using IAuthenticationService = Speedy.Website.Services.IAuthenticationService;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;
using Speedy.Data;
using Microsoft.EntityFrameworkCore;
using NUglify;
using NUglify.Css;
using NUglify.JavaScript;

#endregion

namespace Speedy.Website
{
	public class Startup
	{
		#region Constructors

		public Startup(IConfiguration configuration, IWebHostEnvironment env)
		{
			Configuration = configuration;
			Environment = env;

			// Azure: d:\home\site\wwwroot\wwwroot
			// Local: c:\inetpub\Speedy\wwwroot
			var rootSitePath = Path.Combine(Environment.WebRootPath, "..\\..\\");
			var appDataPath = Path.Combine(rootSitePath, "EpicCodersData");

			AppDataPath = new DirectoryInfo(appDataPath);
			SerializerSettings = new SerializerSettings(false, true, false, false, true, false);

			// Load settings
			ConnectionStrings = Configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();
		}

		#endregion

		#region Properties

		public static DirectoryInfo AppDataPath { get; private set; }

		public IConfiguration Configuration { get; }

		public static ConnectionStrings ConnectionStrings { get; private set; }

		public static IWebHostEnvironment Environment { get; private set; }

		public static Tracker Tracker { get; private set; }

		public static bool IndentModelJson => true;

		public static SerializerSettings SerializerSettings { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		/// </summary>
		public void Configure(IApplicationBuilder app)
		{
			var analyticsPath = Path.Combine(AppDataPath.FullName, "Analytics");
			var client = new TrackerService(new DatabaseProvider<IContosoDatabase>(x => ContosoSqlDatabase.UseSql(ConnectionStrings.DefaultConnection)));
			var provider = new KeyValueRepositoryProvider<TrackerPath>(analyticsPath);

			Tracker = Tracker.Start(client, provider);

			if (Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");

				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseWebOptimizer();
			app.UseStaticFiles(new StaticFileOptions
			{
				OnPrepareResponse = ctx =>
				{
					const int durationInSeconds = 60 * 60 * 24;
					ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + durationInSeconds;
				},
				// https://github.com/dotnet/aspnetcore/blob/master/src/Middleware/StaticFiles/src/FileExtensionContentTypeProvider.cs
				ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
				{
					{ ".css", "text/css" },
					{ ".ico", "image/x-icon" },
					{ ".gif", "image/gif" },
					{ ".html", "text/html" },
					{ ".jpeg", "image/jpeg" },
					{ ".jpg", "image/jpeg" },
					{ ".js", "application/javascript" },
					{ ".png", "image/png" },
					{ ".ttc", "application/x-font-ttf" },
					{ ".ttf", "application/x-font-ttf" },
					{ ".txt", "text/plain" },
					{ ".woff", "application/font-woff" }, // https://www.w3.org/TR/WOFF/#appendix-b
					{ ".woff2", "font/woff2" } // https://www.w3.org/TR/WOFF2/#IMT
				})
			});

			app.UseMiddleware<RequestTracking>();
			app.UseSession();
			app.UseRouting();
			app.UseHttpsRedirection();
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				// Routes
				endpoints.MapControllerRoute("LogIn", "LogIn", new { controller = "Home", action = "LogIn" });
				endpoints.MapControllerRoute("LogOut", "LogOut", new { controller = "Home", action = "LogOut" });
				
				// Defaults
				endpoints.MapControllerRoute("Speedy", "{controller=Home}/{action=Index}/{id?}");
			});
		}

		/// <summary>
		/// This method gets called by the runtime. Use this method to add services to the container.
		/// </summary>
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc(options =>
				{
					var policy = new AuthorizationPolicyBuilder()
						.AddAuthenticationSchemes(
							BasicAuthenticationHandler.AuthenticationScheme,
							CookieAuthenticationDefaults.AuthenticationScheme
						)
						.RequireAuthenticatedUser()
						.Build();

					options.Filters.Add(new AuthorizeFilter(policy));
				})
				.AddNewtonsoftJson(options => UpdateSettings(options.SerializerSettings));

			services.AddSignalR();
			services.AddDistributedMemoryCache();
			services.AddSession(options =>
			{
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
				options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
			});

			services.AddControllersWithViews(options => options.Filters.Add(new HttpResponseExceptionFilter()))
				.AddNewtonsoftJson(options => UpdateSettings(options.SerializerSettings));

			var isDevelopment = Environment.IsDevelopment();
			var databaseProvider = new DatabaseProvider<IContosoDatabase>(o => ContosoSqlDatabase.UseSql(ConnectionStrings.DefaultConnection, o));

			services.AddWebOptimizer(pipeline =>
			{
				pipeline.AddCssBundle(
					"/css/bundle.css",
					new CssSettings
					{
						OutputMode = isDevelopment ? OutputMode.MultipleLines : OutputMode.SingleLine
					},
					"/lib/toastr/toastr.css",
					"/css/Site.css"
				);

				pipeline.AddJavaScriptBundle(
					"/js/bundle.js",
					new CodeSettings
					{
						MinifyCode = !isDevelopment
					},
					"/js/empty.js",
					"/lib/vue/vue.min.js",
					"/lib/vue-resource/vue-resource.min.js",
					"/lib/jquery/jquery.min.js",
					"/lib/underscore/underscore.js",
					"/lib/signalr/signalr.min.js",
					"/lib/moment/moment.js",
					"/lib/toastr/toastr.min.js",
					"/js/site.js"
				);
			});

			// configure basic authentication 
			services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(BasicAuthenticationHandler.AuthenticationScheme, null)
				.AddCookie(options =>
				{
					options.Cookie.Name = "speedy.cookies";
					options.Cookie.Domain = "speedy.local";
					options.Cookie.Path = "/";
					options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
					options.Cookie.SameSite = SameSiteMode.Strict;
					options.ExpireTimeSpan = TimeSpan.FromMinutes(16384);
					options.LoginPath = "/login";
					options.LogoutPath = "/logout";
					options.AccessDeniedPath = "/unauthorized";
					options.SlidingExpiration = true;
					options.Events.OnValidatePrincipal = context =>
					{
						AuthenticationService.ValidatePrincipal(context, databaseProvider);
						return Task.FromResult(0);
					};
					options.Events.OnRedirectToAccessDenied = async context =>
					{
						if (context.Request.Path.Value != null
							&& context.Request.Path.Value.Contains("/api/", StringComparison.OrdinalIgnoreCase))
						{
							// This will ignore all web api that are not authorized
							context.Response.Clear();
							context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
							await context.Response.WriteAsync(Constants.Unauthorized);
							return;
						}

						context.Response.Clear();
						context.Response.StatusCode = (int) HttpStatusCode.Redirect;
						context.Response.Redirect(context.RedirectUri);
					};
					options.Events.OnRedirectToLogin = async context =>
					{
						if (context.Request.Path.Value != null 
							&& context.Request.Path.Value.Contains("/api/", StringComparison.OrdinalIgnoreCase))
						{
							// This will ignore all web api that are not authorized
							context.Response.Clear();
							context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
							await context.Response.WriteAsync(Constants.Unauthorized);
							return;
						}

						context.Response.Clear();
						context.Response.StatusCode = (int) HttpStatusCode.Redirect;
						context.Response.Redirect(context.RedirectUri);
					};
				});

			services.AddSingleton(Configuration);
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			services.AddScoped<AccountService, AccountService>();
			services.AddScoped<IAuthenticationService, AuthenticationService>();
			services.AddScoped<IContosoDatabase, ContosoDatabase>(x => ContosoSqlDatabase.UseSql(ConnectionStrings.DefaultConnection));
			services.AddScoped<IDatabaseProvider<IContosoDatabase>, DatabaseProvider<IContosoDatabase>>(_ => databaseProvider);

			using var database = ContosoSqlDatabase.UseSql(ConnectionStrings.DefaultConnection);
			database.Database.SetCommandTimeout((int) TimeSpan.FromMinutes(15).TotalSeconds);
			database.Database.Migrate();
		}

		private void UpdateSettings(JsonSerializerSettings destination)
		{
			var settings = SerializerSettings.JsonSettings;
			destination.ContractResolver = settings.ContractResolver;
			destination.Converters.AddRange(settings.Converters);
			destination.DateTimeZoneHandling = settings.DateTimeZoneHandling;
			destination.DateFormatHandling = settings.DateFormatHandling;
			destination.ReferenceLoopHandling = settings.ReferenceLoopHandling;
			destination.NullValueHandling = settings.NullValueHandling;
		}

		#endregion
	}
}