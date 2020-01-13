#region References

using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using Microsoft.EntityFrameworkCore;
using Speedy.Exceptions;

#endregion

namespace Speedy.Website.Attributes
{
	[ExcludeFromCodeCoverage]
	public class WebApiExceptionFilterAttribute : ExceptionFilterAttribute
	{
		#region Methods

		public override void OnException(HttpActionExecutedContext context)
		{
			if (context.Exception != null)
			{
				//WebApplication.Tracker.AddException(context.Exception);
			}

			var genericMessage = "An issue has occurred, someone has been alerted and will be working on the issue";
			var updateException = context.Exception as UpdateException;

			if (updateException?.InnerException is DbUpdateException dbUpdateException)
			{
				if (dbUpdateException.InnerException is SqlException sqlException)
				{
					var debugging = HttpContext.Current.IsDebuggingEnabled || HttpContext.Current.User.IsInRole("administrator");
					var error = debugging ? string.Join(", ", sqlException.Errors.Cast<SqlError>().Select(x => x.Message)) : genericMessage;
					context.Response = context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
					return;
				}
			}
			
			var message = context.Exception?.CleanMessage() ?? genericMessage;

			switch (context.Exception)
			{
				case NotImplementedException _:
					context.Response = context.Request.CreateErrorResponse(HttpStatusCode.NotImplemented, message);
					return;

				case UnauthorizedAccessException _:
					context.Response = context.Request.CreateResponse(HttpStatusCode.Unauthorized);
					return;

				case ArgumentException _:
					context.Response = context.Request.CreateResponse(HttpStatusCode.BadRequest, message);
					return;
			}

			context.Response = context.Request.CreateResponse(HttpStatusCode.BadRequest, message);
		}

		#endregion
	}
}