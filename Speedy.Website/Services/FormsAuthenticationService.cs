#region References

using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Authentication;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using Speedy.Data;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Website.Services
{
	[ExcludeFromCodeCoverage]
	public class FormsAuthenticationService : IAuthenticationService
	{
		#region Fields

		private readonly AccountService _service;

		#endregion

		#region Constructors

		public FormsAuthenticationService(AccountService service)
		{
			_service = service;
		}

		#endregion

		#region Properties

		public IIdentity Identity => HttpContext.Current.User.Identity;

		public bool IsAuthenticated => UserId > 0;

		public int UserId => Identity.GetUserId();

		public string UserName => Identity.GetUserName();

		#endregion

		#region Methods

		public bool LogIn(Credentials credentials)
		{
			if (string.IsNullOrWhiteSpace(credentials.Password))
			{
				throw new AuthenticationException(Constants.LoginInvalidError);
			}

			try
			{
				var user = _service.AuthenticateAccount(credentials);
				user.LastLoginDate = TimeService.UtcNow;
				SetCookie(user, credentials.RememberMe);
				return true;
			}
			catch (AuthenticationException)
			{
				return false;
			}
		}

		public void LogIn(AccountEntity account)
		{
			account.LastLoginDate = TimeService.UtcNow;
			SetCookie(account, true);
		}

		public void LogOut()
		{
			FormsAuthentication.SignOut();
			HttpContext.Current.Session?.Abandon();
		}

		public void UpdateLogin(AccountEntity account)
		{
			var rememberMe = false;
			var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

			if (authCookie != null)
			{
				var ticket = FormsAuthentication.Decrypt(authCookie.Value);
				rememberMe = ticket != null && ticket.IsPersistent;
			}

			SetCookie(account, rememberMe);
		}

		private static void SetCookie(AccountEntity account, bool rememberMe)
		{
			if (account == null)
			{
				throw new ArgumentException("user");
			}

			HttpContext.Current.Response.Cookies.Clear();
			FormsAuthentication.SetAuthCookie(account.GetCookieValue(), rememberMe);
		}

		#endregion
	}
}