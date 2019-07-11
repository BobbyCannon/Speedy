#region References

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

#endregion

namespace Speedy.Website
{
	public class Program
	{
		#region Methods

		public static IWebHostBuilder CreateWebHostBuilder(string[] args)
		{
			return WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>();
		}

		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();

			var configurationBuilder = new ConfigurationBuilder();
			configurationBuilder.AddJsonFile("AppSettings.json");
			var configuration = configurationBuilder.Build();
		}

		#endregion
	}
}