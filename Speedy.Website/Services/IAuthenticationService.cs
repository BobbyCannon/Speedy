#region References

using System.Security.Principal;
using Speedy.Data;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Website.Services
{
	public interface IAuthenticationService
	{
		#region Properties

		IIdentity Identity { get; }
		bool IsAuthenticated { get; }
		int UserId { get; }
		string UserName { get; }

		#endregion

		#region Methods

		bool LogIn(Credentials credentials);
		void LogOut();
		void UpdateLogin(AccountEntity account);

		#endregion
	}
}