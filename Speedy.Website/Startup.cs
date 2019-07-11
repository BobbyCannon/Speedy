﻿#region References

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Speedy.Samples;
using Speedy.Samples.Sql;
using Speedy.Website.Handlers;

#endregion

namespace Speedy.Website
{
	public class Startup
	{
		#region Constructors

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		#endregion

		#region Properties

		public IConfiguration Configuration { get; }

		#endregion

		#region Methods

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();
			app.UseAuthentication();

			app.UseMvc(routes => { routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}"); });

			using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
			using (var context = serviceScope.ServiceProvider.GetService<ContosoSqlDatabase>())
			{
				context.Database.Migrate();
			}
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			// configure basic authentication 
			services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

			// The database context will be disposed of at the end of each request
			services.AddDbContext<ContosoSqlDatabase>(ContosoSqlDatabase.ConfigureOptions);
			services.AddTransient<IContosoDatabase, ContosoSqlDatabase>();
			services.AddTransient<IDatabaseProvider<IContosoDatabase>, DatabaseProvider<IContosoDatabase>>(x => new DatabaseProvider<IContosoDatabase>(y => ContosoSqlDatabase.UseSql(options: y)));
		}

		#endregion
	}
}