#region References

using System.Web.Mvc;
using Speedy.Data;
using Speedy.Website.Samples;
using Speedy.Website.Services;

#endregion

namespace Speedy.Website.Controllers
{
	public class HomeController : BaseController
	{
		#region Constructors

		public HomeController(IContosoDatabase database, IAuthenticationService authenticationService) : base(database, authenticationService)
		{
		}

		#endregion

		#region Methods

		public ActionResult Account()
		{
			var user = GetAccount();
			var service = new ViewService(Database, user);
			return View(service.GetAccount());
		}

		public ActionResult Index()
		{
			ViewBag.Title = "Home Page";
			return View();
		}

		[AllowAnonymous]
		public ActionResult LogIn(string returnUrl)
		{
			if (IsAuthenticated)
			{
				return Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
			}

			if (MissingAuthenticatedUser)
			{
				return RedirectToAction("LogIn", "Home");
			}

			ViewBag.ReturnUrl = returnUrl;

			return View(new Credentials());
		}

		[HttpPost]
		[AllowAnonymous]
		public ActionResult LogIn(Credentials model, string returnUrl)
		{
			if (!ModelState.IsValid)
			{
				ModelState.AddModelError("EmailAddress", Constants.LoginInvalidError);
				return View(model);
			}

			if (!AuthenticationService.LogIn(model))
			{
				ModelState.AddModelError("EmailAddress", Constants.LoginInvalidError);
				return View(model);
			}

			Database.SaveChanges();

			return Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
		}

		public ActionResult LogOut()
		{
			AuthenticationService.LogOut();
			return Redirect("/");
		}

		#endregion
	}
}