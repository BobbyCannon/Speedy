#region References

using System;
using System.Data;
using System.Net;
using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Speedy.Data;

#endregion

namespace Speedy.Website.Middleware
{
	public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
	{
		#region Constructors

		public HttpResponseExceptionFilter()
		{
			Order = int.MaxValue - 10;
		}

		#endregion

		#region Properties

		public int Order { get; set; }

		#endregion

		#region Methods

		public void OnActionExecuted(ActionExecutedContext context)
		{
			if (context.Exception == null)
			{
				return;
			}

			var path = context.HttpContext.Request.Path;
			var isApi = path.HasValue && (path.Value?.ToLower().StartsWith("/api/") ?? false);
			var message = context.Exception.Message;
			var authenticated = context.HttpContext.User.Identity?.IsAuthenticated ?? false;
			HttpStatusCode status;

			switch (context.Exception)
			{
				case AuthenticationException _:
				case UnauthorizedAccessException _:
					if (!isApi)
					{
						context.Result = new RedirectResult(authenticated ? "/Unauthorized" : "/Login");
						context.ExceptionHandled = true;
						return;
					}

					status = HttpStatusCode.Unauthorized;
					message = Constants.Unauthorized;
					break;

				case NotImplementedException _:
					status = HttpStatusCode.NotImplemented;
					message = string.Empty;
					break;

				case ArgumentException _:
				case ConstraintException _:
				default:
					status = HttpStatusCode.BadRequest;
					break;
			}

			if (isApi)
			{
				context.Result = new ContentResult { Content = message, StatusCode = (int) status };
				context.ExceptionHandled = true;
			}
		}

		public void OnActionExecuting(ActionExecutingContext context)
		{
		}

		#endregion
	}
}