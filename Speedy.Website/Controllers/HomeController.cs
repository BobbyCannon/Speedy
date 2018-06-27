#region References

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Speedy.Samples;
using Speedy.Website.Models;

#endregion

namespace Speedy.Website.Controllers
{
	public class HomeController : Controller
	{
		#region Fields

		private readonly IContosoDatabase _database;

		#endregion

		#region Constructors

		public HomeController(IContosoDatabase database)
		{
			_database = database;
		}

		#endregion

		#region Methods

		public IActionResult Boom()
		{
			throw new Exception("Boom");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public IActionResult Index()
		{
			var model = new IndexViewModel
			{
				AddressCount = _database.Addresses.Count(),
				PeopleCount = _database.People.Count()
			};

			return View(model);
		}

		protected override void Dispose(bool disposing)
		{
			_database.Dispose();
			base.Dispose(disposing);
		}

		#endregion
	}
}