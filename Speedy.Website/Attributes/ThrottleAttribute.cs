#region References

using System;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Caching;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Humanizer;

#endregion

namespace Speedy.Website.Attributes
{
	public class ThrottleAttribute : ActionFilterAttribute
	{
		#region Properties

		/// <inheritdoc />
		public override bool AllowMultiple => false;

		/// <summary>
		/// The number of request per second allowed.
		/// </summary>
		public int RequestsPerSecond { get; set; }

		/// <summary>
		/// The number of ticks per microsecond.
		/// </summary>
		public long TicksPerMicroseconds => TimeSpan.TicksPerMillisecond / 1000;

		#endregion

		#region Methods

		/// <inheritdoc />
		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			var name = $"{actionContext.Request.Method}-{actionContext.Request.RequestUri}";
			var key = string.Concat(name, "-", GetClientIp(actionContext.Request));
			var ticksPerRequest = TimeSpan.TicksPerSecond / RequestsPerSecond;

			if (HttpRuntime.Cache[key] == null)
			{
				HttpRuntime.Cache.Add(key, true, null, TimeService.UtcNow.AddTicks(ticksPerRequest), Cache.NoSlidingExpiration, CacheItemPriority.Low, null);
				return;
			}

			var often = ticksPerRequest < TimeSpan.TicksPerMillisecond ? $"{ticksPerRequest / TicksPerMicroseconds} µs" : TimeSpan.FromTicks(ticksPerRequest).Humanize();
			var message = $"You may only perform this action once every {often}.";
			actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Conflict, message);
		}

		private static string GetClientIp(HttpRequestMessage request)
		{
			if (request.Properties.ContainsKey("MS_HttpContext"))
			{
				return ((HttpContextWrapper) request.Properties["MS_HttpContext"]).Request.UserHostAddress;
			}

			if (!request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
			{
				return null;
			}

			var prop = (RemoteEndpointMessageProperty) request.Properties[RemoteEndpointMessageProperty.Name];
			return prop.Address;
		}

		#endregion
	}
}