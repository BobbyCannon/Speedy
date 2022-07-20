#region References

using System;
using System.Diagnostics;
using System.Reflection;
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
		public IActionResult PagedRequest(CustomPagedRequest request = null)
		{
			request ??= new CustomPagedRequest();
			request.Cleanup();
			var result = new CustomPagedResults<object>(request, 156324,
				2,
				"foo",
				TimeService.UtcNow,
				TimeSpan.FromMilliseconds(123456),
				Assembly.GetAssembly(typeof(Entity))?.GetName().Version ?? new Version(1, 2, 3, 4)
			);
			return View(result);
		}

		[AllowAnonymous]
		public IActionResult Privacy()
		{
			return View();
		}

		#endregion
	}
}