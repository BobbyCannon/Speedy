#region References

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Speedy.Data;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.Website.WebApi
{
	public abstract class BaseController : ControllerBase
	{
		#region Fields

		private AccountEntity _user;

		#endregion

		#region Constructors

		protected BaseController(IContosoDatabase database)
		{
			Database = database;
		}

		#endregion

		#region Properties

		public IContosoDatabase Database { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Gets the current logged in user using the provided session.
		/// </summary>
		/// <returns> The user of the logged in user. </returns>
		protected AccountEntity GetAuthenticatedAccount(Expression<Func<AccountEntity, AccountEntity>> cast, bool throwException)
		{
			if (_user != null)
			{
				return _user;
			}

			// Make sure we are authenticated.
			var user = HttpContext.User;
			if (user.Identity?.IsAuthenticated != true)
			{
				if (throwException)
				{
					throw new Exception(Constants.Unauthorized);
				}

				return null;
			}

			var userId = user.GetUserId();
			_user = Database.Accounts.Select(cast).FirstOrDefault(u => u.Id == userId);

			if (_user == null)
			{
				// Log the user out because we cannot find the user account.
				HttpContext.SignOutAsync();

				if (throwException)
				{
					throw new UnauthorizedAccessException(Constants.Unauthorized);
				}
			}

			return _user;
		}

		#endregion
	}
}