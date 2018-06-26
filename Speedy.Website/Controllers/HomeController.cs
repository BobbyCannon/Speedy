#region References

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Speedy.Website.Models;

#endregion

namespace Speedy.Website.Controllers
{
	public class HomeController : Controller
	{
		#region Methods

		public IActionResult About()
		{
			ViewData["Message"] = "Your application description page.";

			return View();
		}

		public IActionResult Contact()
		{
			ViewData["Message"] = "Your contact page.";

			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		#endregion
	}
}