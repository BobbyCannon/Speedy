#region References

using System.Web.Mvc;
using Speedy.Samples;

#endregion

namespace Speedy.Website.Controllers
{
	public class HomeController : BaseController
	{
		#region Constructors

		public HomeController() : this(new EntityFrameworkContosoDatabase())
		{
		}

		public HomeController(IContosoDatabase database) : base(database)
		{
		}

		#endregion

		#region Methods

		public ActionResult Index()
		{
			ViewBag.Title = "Home Page";
			return View(Database.Addresses);
		}

		#endregion
	}
}