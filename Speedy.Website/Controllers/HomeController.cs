#region References

using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Speedy.Website.Models;

#endregion

namespace Speedy.Website.Controllers
{
	public class HomeController : Controller
	{
		#region Fields

		private readonly ILogger<HomeController> _logger;

		#endregion

		#region Constructors

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		#endregion

		#region Methods

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public IActionResult Index()
		{
			return View();
		}
		
		[AllowAnonymous]
		public IActionResult LogIn()
		{
			return View();
		}
		
		[AllowAnonymous]
		public IActionResult LogOut()
		{
			return RedirectToAction(nameof(LogIn));
		}

		[AllowAnonymous]
		public IActionResult Privacy()
		{
			return View();
		}

		#endregion
	}
}