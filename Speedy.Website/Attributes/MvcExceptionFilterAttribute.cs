#region References

using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Security.Authentication;
using System.Web.Configuration;
using System.Web.Mvc;
using ExceptionContext = System.Web.Mvc.ExceptionContext;

#endregion

namespace Speedy.Website.Attributes
{
	[ExcludeFromCodeCoverage]
	public class MvcExceptionFilterAttribute : FilterAttribute, IExceptionFilter
	{
		#region Methods

		public void OnException(ExceptionContext context)
		{
			if (context.Exception != null)
			{
				//WebApplication.Tracker.AddException(context.Exception);
			}

			var customErrorsSection = (CustomErrorsSection) ConfigurationManager.GetSection("system.web/customErrors");
			if (customErrorsSection.Mode == CustomErrorsMode.Off)
			{
				return;
			}

			var authenticated = context.HttpContext.Request.IsAuthenticated;

			switch (context.Exception)
			{
				case AuthenticationException _:
				case UnauthorizedAccessException _:
					context.Result = new RedirectResult(authenticated ? "/Unauthorized" : "/LogIn");
					context.ExceptionHandled = true;
					context.HttpContext.Response.Clear();
					return;
			}

			context.Result = new RedirectResult("/Error");
			context.ExceptionHandled = true;
			context.HttpContext.Response.Clear();
		}

		#endregion
	}
}