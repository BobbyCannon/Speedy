#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NUglify;
using NUglify.Css;
using NUglify.JavaScript;
using Speedy.Data;
using Speedy.Extensions;
using Speedy.Profiling;
using Speedy.Serialization;
using Speedy.Storage.KeyValue;
using Speedy.Sync;
using Speedy.Website.Data;
using Speedy.Website.Data.Sql;
using Speedy.Website.Middleware;
using Speedy.Website.Models;
using Speedy.Website.Services;
using Speedy.Website.WebApi;
using AuthenticationService = Speedy.Website.Services.AuthenticationService;
using IAuthenticationService = Speedy.Website.Services.IAuthenticationService;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

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
			var appDataPath = Path.Combine(rootSitePath, "SpeedyAppData");

			AppDataPath = new DirectoryInfo(appDataPath);

			Serializer.DefaultSettings.CamelCase = true;
			Serializer.DefaultSettings.IgnoreVirtuals = true;

			// Load settings
			ConnectionStrings = Configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();
		}

		#endregion

		#region Properties

		public static DirectoryInfo AppDataPath { get; private set; }

		public IConfiguration Configuration { get; }

		public static ConnectionStrings ConnectionStrings { get; private set; }

		public static IWebHostEnvironment Environment { get; private set; }

		public static bool IndentModelJson => true;

		public static Tracker Tracker { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		/// </summary>
		public void Configure(IApplicationBuilder app)
		{
			var analyticsPath = Path.Combine(AppDataPath.FullName, "Analytics");
			var client = new TrackerService(new DatabaseProvider<IContosoDatabase>(x => ContosoSqlDatabase.UseSql(ConnectionStrings.DefaultConnection, x, null)));
			var provider = new KeyValueRepositoryProvider<TrackerPath>(analyticsPath);

			Tracker = Tracker.Start(client, provider);

			if (Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseHttpsRedirection();
			app.UseHsts();
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
			app.UseStaticFiles();
			app.UseRouting();
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
			var isDevelopment = Environment.IsDevelopment();

			if (isDevelopment)
			{
				services.AddHttpsRedirection(options =>
				{
					options.RedirectStatusCode = (int) HttpStatusCode.PermanentRedirect;
					options.HttpsPort = 443;
				});
			}

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
			services.AddControllersWithViews(options =>
			{
				options.Filters.Add(new HttpResponseExceptionFilter());
				options.ModelBinderProviders.Insert(0, new PagedRequestModelBinderProvider());
			});

			var databaseProvider = new DatabaseProvider<IContosoDatabase>(o => ContosoSqlDatabase.UseSql(ConnectionStrings.DefaultConnection, o, null), ContosoDatabase.GetDefaultOptions());
			var syncDatabaseProvider = new SyncableDatabaseProvider<IContosoDatabase>((o, c) => ContosoSqlDatabase.UseSql(ConnectionStrings.DefaultConnection, o, c), ContosoDatabase.GetDefaultOptions(), SyncController.KeyCache);

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
						if ((context.Request.Path.Value != null)
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
						if ((context.Request.Path.Value != null)
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
			services.AddScoped<IContosoDatabase, ContosoDatabase>(x => (ContosoDatabase) databaseProvider.GetDatabase());
			services.AddScoped<IDatabaseProvider<IContosoDatabase>, DatabaseProvider<IContosoDatabase>>(_ => databaseProvider);
			services.AddScoped<ISyncableDatabaseProvider<IContosoDatabase>, SyncableDatabaseProvider<IContosoDatabase>>(_ => syncDatabaseProvider);

			services.TryAddSingleton<IApiDescriptionGroupCollectionProvider, ApiDescriptionGroupCollectionProvider>();
			services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, DefaultApiDescriptionProvider>());

			using var database = ContosoSqlDatabase.UseSql(ConnectionStrings.DefaultConnection, null, null);
			database.Database.SetCommandTimeout((int) TimeSpan.FromMinutes(15).TotalSeconds);
			database.Database.Migrate();
		}

		private void UpdateSettings(JsonSerializerSettings destination)
		{
			var settings = Serializer.DefaultSettings.JsonSettings;

			destination.ContractResolver = settings.ContractResolver;
			destination.Converters.AddRange(settings.Converters);
			destination.DateTimeZoneHandling = settings.DateTimeZoneHandling;
			destination.DateFormatHandling = settings.DateFormatHandling;
			destination.ReferenceLoopHandling = settings.ReferenceLoopHandling;
			destination.NullValueHandling = settings.NullValueHandling;
		}

		#endregion
	}

	public class PagedRequestModelBinder : IModelBinder
	{
		#region Methods

		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			if (bindingContext == null)
			{
				throw new ArgumentNullException(nameof(bindingContext));
			}

			if (Activator.CreateInstance(bindingContext.ModelType) is not PagedRequest pagedRequest)
			{
				bindingContext.Result = ModelBindingResult.Failed();
				return Task.CompletedTask;
			}
			
			pagedRequest.ParseQueryString(bindingContext.HttpContext.Request.QueryString.ToString());
			bindingContext.Result = ModelBindingResult.Success(pagedRequest);
			return Task.CompletedTask;
		}

		#endregion
	}

	public class PagedRequestModelBinderProvider : IModelBinderProvider
	{
		#region Methods

		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			return typeof(PagedRequest).IsAssignableFrom(context.Metadata.ModelType)
				? new PagedRequestModelBinder()
				: null;
		}

		#endregion
	}
}