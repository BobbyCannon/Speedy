#region References

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Authentication;
using System.Web.Http;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Entities;
using Speedy.Website.Services;
using RoleService = Speedy.Website.Services.RoleService;

#endregion

namespace Speedy.Website.WebApi
{
	public abstract class BaseController : ApiController
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

		#endregion

		#region Methods

		/// <summary> Releases the unmanaged resources that are used by the object and, optionally, releases the managed resources. </summary>
		/// <param name="disposing">
		/// true to release both managed and unmanaged resources; false to release only unmanaged
		/// resources.
		/// </param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Database?.Dispose();
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Gets the current logged in account using the provided session.
		/// </summary>
		/// <returns> The logged in account. </returns>
		protected AccountEntity GetCurrentAccount()
		{
			// Make sure we are authenticated.
			return _account ??= GetCurrentAccount(User?.Identity.Name, true);
		}

		/// <summary>
		/// Gets the current logged in account using the provided session.
		/// </summary>
		/// <param name="throwException"> If true then throw an exception if the account is not logged in else return null. </param>
		/// <returns> The logged in account. </returns>
		protected AccountEntity GetCurrentAccount(bool throwException)
		{
			// Make sure we are authenticated.
			return _account ??= GetCurrentAccount(User?.Identity.Name, throwException);
		}

		/// <summary>
		/// Gets the current logged in account using the provided session.
		/// </summary>
		/// <param name="identity"> The name of the identity. </param>
		/// <param name="throwException"> If true then throw an exception if the account is not logged in else return null. </param>
		/// <returns> The logged in account. </returns>
		protected AccountEntity GetCurrentAccount(string identity, bool throwException)
		{
			return _account ??= GetCurrentAccount(x => x, identity, throwException);
		}

		/// <summary>
		/// Gets the current logged in account using the provided session.
		/// </summary>
		/// <param name="cast"> Cast to query only some account data. </param>
		/// <param name="throwException"> If true then throw an exception if the account is not logged in else return null. </param>
		/// <returns> The logged in account. </returns>
		protected AccountEntity GetCurrentAccount(Expression<Func<AccountEntity, AccountEntity>> cast, bool throwException = true)
		{
			// Make sure we are authenticated.
			return _account ?? GetCurrentAccount(cast, User?.Identity.Name, throwException);
		}

		/// <summary>
		/// Gets the current logged in account using the provided session.
		/// </summary>
		/// <param name="cast"> Cast to query only some account data. </param>
		/// <param name="identity"> The name of the identity. </param>
		/// <param name="throwException"> If true then throw an exception if the account is not logged in else return null. </param>
		/// <returns> The logged in account. </returns>
		private AccountEntity GetCurrentAccount(Expression<Func<AccountEntity, AccountEntity>> cast, string identity, bool throwException = true)
		{
			var userId = identity?.GetUserId() ?? 0;
			var account = Database.Accounts
				.Select(cast)
				.FirstOrDefault(u => u.Id == userId);

			if (account == null)
			{
				RoleService.ResetByIdentity(identity);
				AuthenticationService.LogOut();

				if (throwException)
				{
					throw new AuthenticationException(Constants.Unauthorized);
				}
			}

			return account;
		}
		#endregion
	}
}