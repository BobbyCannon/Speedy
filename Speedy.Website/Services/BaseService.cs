#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Speedy.Data;
using Speedy.Data.WebApi;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Entities;
using Speedy.Website.Samples.Enumerations;
using Speedy.Website.ViewModels;

#endregion

namespace Speedy.Website.Services
{
	public abstract class BaseService
	{
		#region Constructors

		protected BaseService(IContosoDatabase database, AccountEntity account)
		{
			Database = database;
			Account = account;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the current user of this service.
		/// </summary>
		public AccountEntity Account { get; }

		/// <summary>
		/// Gets the database for the service.
		/// </summary>
		public IContosoDatabase Database { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Combines a list of roles into a single string. Stores only distinct roles and joins them with a comma ",".
		/// </summary>
		/// <param name="roles"> The roles to combine. </param>
		/// <returns> The delimited string of the roles. </returns>
		public static string CombineRoles(params AccountRole[] roles)
		{
			return CombineRoles(roles.Select(x => x.ToString()).ToArray());
		}

		/// <summary>
		/// Combines a list of tags into a single string. Stores only distinct tags and joins them with a comma ",".
		/// </summary>
		/// <param name="tags"> The tags to include. </param>
		/// <returns> The delimited string of the tags. </returns>
		public static string CombineRoles(params string[] tags)
		{
			return $",{string.Join(",", tags.Select(x => x.Trim()).Distinct().OrderBy(x => x))},";
		}

		public static IEnumerable<string> SplitRoles(string tags)
		{
			return tags.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Distinct().OrderBy(x => x).ToArray();
		}

		public static Account ToModel(AccountEntity entity)
		{
			return new Account
			{
				CreatedOn = entity.CreatedOn,
				Id = entity.Id,
				IsDeleted = entity.IsDeleted,
				ModifiedOn = entity.ModifiedOn,
				Name = entity.Name,
				SyncId = entity.SyncId
			};
		}

		public static AccountView ToView(AccountEntity account, DateTime now)
		{
			return new AccountView
			{
				CreatedOn = account.CreatedOn,
				EmailAddress = account.EmailAddress,
				Id = account.Id,
				IsDeleted = account.IsDeleted,
				LastLoginDate = account.LastLoginDate,
				MemberFor = now.Subtract(account.CreatedOn).Humanize(),
				ModifiedOn = account.ModifiedOn,
				Name = account.Name,
				Roles = account.GetRoles(),
				SyncId = account.SyncId,
			};
		}

		protected bool AccountInAnyRole(params string[] roles)
		{
			return AccountInAnyRole(Account, roles);
		}

		protected bool AccountInAnyRole(AccountEntity account, params string[] roles)
		{
			return roles.Any(account.InRole);
		}

		protected bool AccountInAnyRole(params AccountRole[] roles)
		{
			return AccountInAnyRole(Account, roles);
		}

		protected bool AccountInAnyRole(AccountEntity account, params AccountRole[] roles)
		{
			return roles.Any(x => account.InRole(x));
		}

		protected bool AccountIsAdministrator(AccountEntity account)
		{
			return AccountInAnyRole(account, AccountRole.Administrator);
		}

		protected void ValidateAccount(string message = Constants.Unauthorized)
		{
			if (Account == null)
			{
				throw new UnauthorizedAccessException(message);
			}
		}

		protected void ValidateAccount(string message, string role)
		{
			if (Account == null || !Account.InRole(role))
			{
				throw new UnauthorizedAccessException(message);
			}
		}

		protected void ValidateAccount(string message, params string[] roles)
		{
			if (Account == null || !AccountInAnyRole(roles))
			{
				throw new UnauthorizedAccessException(message);
			}
		}

		protected void ValidateAccount(string message, params AccountRole[] roles)
		{
			if (Account == null || !AccountInAnyRole(roles))
			{
				throw new UnauthorizedAccessException(message);
			}
		}

		protected void ValidateAccountAsAdministrator()
		{
			ValidateAccount(Constants.Unauthorized, AccountRole.Administrator);
		}

		#endregion
	}
}