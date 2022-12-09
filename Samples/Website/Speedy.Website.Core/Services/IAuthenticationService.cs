#region References

using System.Security.Claims;
using Speedy.Net;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.Website.Core.Services
{
	public interface IAuthenticationService
	{
		#region Properties

		bool IsAuthenticated { get; }
		ClaimsPrincipal User { get; }
		int UserId { get; }
		string UserName { get; }

		#endregion

		#region Methods

		bool LogIn(WebCredential credentials);
		void LogIn(AccountEntity account);
		void LogOut();

		#endregion
	}
}