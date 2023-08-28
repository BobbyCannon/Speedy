#region References

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

#endregion

namespace Speedy.Website
{
	public class Program
	{
		#region Methods

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
		}

		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		#endregion
	}
}