#region References

using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;

#endregion

namespace Speedy.Website.Attributes
{
	[ExcludeFromCodeCoverage]
	public class MvcAuthorizeAttribute : AuthorizeAttribute
	{
		#region Methods

		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
		{
			if (filterContext.HttpContext.User.Identity.IsAuthenticated)
			{
				filterContext.Result = new RedirectResult("/Unauthorized");
				return;
			}

			base.HandleUnauthorizedRequest(filterContext);
		}

		#endregion
	}
}