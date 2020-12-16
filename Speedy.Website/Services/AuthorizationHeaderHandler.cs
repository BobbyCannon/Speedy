#region References

using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Speedy.Data;

#endregion

namespace Speedy.Website.Services
{
	public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
	{
		#region Constants

		public const string AuthenticationScheme = "BasicAuthentication";

		#endregion

		#region Fields

		private readonly AccountService _accountService;

		#endregion

		#region Constructors

		public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, AccountService accountService)
			: base(options, logger, encoder, clock)
		{
			_accountService = accountService;
		}

		#endregion

		#region Methods

		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			if (Request.HttpContext.User.Identity?.IsAuthenticated == true || !Request.Headers.ContainsKey("Authorization"))
			{
				return Task.FromResult(AuthenticateResult.NoResult());
			}

			try
			{
				var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
				if (authHeader.Parameter == null)
				{
					return Task.FromResult(AuthenticateResult.NoResult());
				}

				var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
				var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
				var username = credentials[0];
				var password = credentials[1];

				var user = _accountService.AuthenticateAccount(new Credentials { EmailAddress = username, Password = password, RememberMe = false });
				if (user == null)
				{
					return Task.FromResult(AuthenticateResult.NoResult());
				}

				var ticket = AuthenticationService.CreateTicket(user, false, AuthenticationScheme);
				return Task.FromResult(AuthenticateResult.Success(ticket));
			}
			catch
			{
				return Task.FromResult(AuthenticateResult.NoResult());
			}
		}

		#endregion
	}
}