#region References

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

#endregion

namespace Speedy.Website
{
	public class Program
	{
		#region Methods

		public static IWebHostBuilder CreateWebHostBuilder(string[] args)
		{
			return WebHost.CreateDefaultBuilder(args)
				//.CaptureStartupErrors(true)
				//.UseSetting("detailedErrors", "true")
				.UseStartup<Startup>();
		}

		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
		}

		#endregion
	}
}