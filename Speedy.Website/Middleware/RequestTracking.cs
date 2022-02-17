#region References

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Net.Http.Headers;
using Speedy.Profiling;
using Speedy.Website.Services;

#endregion

namespace Speedy.Website.Middleware
{
	public class RequestTracking
	{
		#region Fields

		private static readonly string[] _ignoredAnalytics;
		private readonly RequestDelegate _next;

		private TrackerPath _trackerPath;

		#endregion

		#region Constructors

		public RequestTracking(RequestDelegate next)
		{
			_next = next;
		}

		static RequestTracking()
		{
			_ignoredAnalytics = new[] { "/favicons/", "/signalr/", "/css/", "/js/", "/fonts/" };
		}

		#endregion

		#region Methods

		public async Task Invoke(HttpContext context)
		{
			var uri = context.Request.GetDisplayUrl().ToLower();

			if (BeginRequest(context, uri))
			{
				await _next.Invoke(context);

				EndRequest(context, uri);
			}
		}

		private bool BeginRequest(HttpContext context, string uri)
		{
			var userAgent = context.Request.Headers[HeaderNames.UserAgent].ToString();
			var referrer = context.Request.Headers[HeaderNames.Referer].ToString();

			if ((userAgent == "AlwaysOn") && (referrer == "::1"))
			{
				// This is Azure keeping the website running, we should ignore these
				return true;
			}

			if (!uri.ContainsAny(_ignoredAnalytics))
			{
				_trackerPath = Startup.Tracker?.StartNewPath(AnalyticEvents.WebRequest.ToString(),
					new TrackerPathValue("URI", uri),
					new TrackerPathValue("UrlReferrer", referrer),
					new TrackerPathValue("UserHostAddress", context.Connection.RemoteIpAddress?.ToString() ?? string.Empty),
					new TrackerPathValue("UserAgent", userAgent),
					new TrackerPathValue("IdentityName", context.User.Identity?.Name ?? string.Empty),
					new TrackerPathValue("HttpMethod", context.Request.Method)
				);
			}

			return true;
		}

		private void EndRequest(HttpContext context, string uri)
		{
			_trackerPath?.Values.Add(new TrackerPathValue("StatusCode", context.Response.StatusCode));
			_trackerPath?.Complete();
			_trackerPath = null;
		}

		#endregion
	}
}