#region References

using System;
using System.Web;
using System.Web.Http;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Activation;
using Ninject.Web.Common;
using Ninject.Web.Common.WebHost;
using Ninject.Web.WebApi;
using Speedy.Website;
using Speedy.Website.Samples;
using Speedy.Website.Services;
using WebActivatorEx;

#endregion

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectConfig), "Start")]
[assembly: ApplicationShutdownMethod(typeof(NinjectConfig), "Stop")]

namespace Speedy.Website
{
	public static class NinjectConfig
	{
		#region Fields

		private static readonly Bootstrapper _bootstrapper;

		#endregion

		#region Constructors

		static NinjectConfig()
		{
			_bootstrapper = new Bootstrapper();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Starts the application
		/// </summary>
		public static void Start()
		{
			DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
			DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
			_bootstrapper.Initialize(CreateKernel);
		}

		/// <summary>
		/// Stops the application.
		/// </summary>
		public static void Stop()
		{
			_bootstrapper.ShutDown();
		}

		/// <summary>
		/// Creates the kernel that will manage your application.
		/// </summary>
		/// <returns> The created kernel. </returns>
		private static IKernel CreateKernel()
		{
			var kernel = new StandardKernel();
			try
			{
				kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
				kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
				GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(kernel);
				RegisterServices(kernel);
				return kernel;
			}
			catch
			{
				kernel.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Load your modules or register your services here!
		/// </summary>
		/// <param name="kernel"> The kernel. </param>
		private static void RegisterServices(IKernel kernel)
		{
			kernel.Bind<IAuthenticationService>().To<FormsAuthenticationService>();
			kernel.Bind<IContosoDatabase>().ToProvider(new GenericProvider<ContosoDatabase>(() => WebApplication.GetDatabase()));
			kernel.Bind<IDatabaseProvider<IContosoDatabase>>().ToProvider(new GenericProvider<IDatabaseProvider<ContosoDatabase>>(() => new DatabaseProvider<ContosoDatabase>(WebApplication.GetDatabase, ContosoDatabase.GetDefaultOptions())));

			// SignalR support, will do later
			//GlobalHost.DependencyResolver.Register(typeof(IContosoDatabase), () => WebApplication.GetDatabase());
		}

		#endregion
	}

	internal class GenericProvider<T> : Provider<T>
	{
		#region Fields

		private readonly Func<T> _action;

		#endregion

		#region Constructors

		public GenericProvider(Func<T> action)
		{
			_action = action;
		}

		#endregion

		#region Methods

		protected override T CreateInstance(IContext context)
		{
			return _action();
		}

		#endregion
	}
}