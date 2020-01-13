#region References

using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Web.Mvc;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Entities;
using Speedy.Website.Services;

#endregion

namespace Speedy.Website.Controllers
{
	public abstract class BaseController : Controller
	{
		#region Fields

		private AccountEntity _account;

		#endregion

		#region Constructors

		protected BaseController(IContosoDatabase database, IAuthenticationService authenticationService)
		{
			Database = database;
			AuthenticationService = authenticationService;
		}

		#endregion

		#region Properties

		public IAuthenticationService AuthenticationService { get; }

		public IContosoDatabase Database { get; }


		protected bool IsAuthenticated
		{
			get
			{
				if (_account == null)
				{
					GetAccount(false);
				}

				return _account != null && (Thread.CurrentPrincipal?.Identity.IsAuthenticated ?? false);
			}
		}

		protected bool MissingAuthenticatedUser
		{
			get
			{
				if (_account == null)
				{
					GetAccount(false);
				}

				return _account == null && (Thread.CurrentPrincipal?.Identity.IsAuthenticated ?? false);
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the current logged in user using the provided session.
		/// </summary>
		/// <param name="throwException"> If true then throw an exception if the user is not logged in else return null. </param>
		/// <returns> The user of the logged in user. </returns>
		protected AccountEntity GetAccount(bool throwException = true)
		{
			// Make sure we are authenticated.
			return _account ?? GetAccount(User?.Identity.Name, throwException);
		}

		/// <summary>
		/// Gets the current logged in user using the provided session.
		/// </summary>
		/// <param name="identity"> The name of the identity. </param>
		/// <param name="throwException"> If true then throw an exception if the user is not logged in else return null. </param>
		/// <returns> The user of the logged in user. </returns>
		private AccountEntity GetAccount(string identity, bool throwException = true)
		{
			var userId = identity?.GetUserId() ?? 0;

			_account = Database.Accounts.FirstOrDefault(u => u.Id == userId);

			if (_account != null)
			{
				return _account;
			}

			RoleService.ResetByIdentity(identity);
			AuthenticationService.LogOut();

			if (throwException)
			{
				throw new AuthenticationException(Constants.Unauthorized);
			}

			return _account;
		}

		#endregion
	}
}