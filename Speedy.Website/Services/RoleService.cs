#region References

using System;
using System.Linq;
using System.Web.Security;
using Microsoft.Extensions.Caching.Memory;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Website.Services
{
	public class RoleService : RoleProvider
	{
		#region Fields

		private static readonly MemoryCache _identityByEmailAddress;
		private static readonly MemoryCache _roles;

		#endregion

		#region Constructors

		static RoleService()
		{
			_roles = new MemoryCache(new MemoryCacheOptions());
			_identityByEmailAddress = new MemoryCache(new MemoryCacheOptions());
		}

		#endregion

		#region Properties

		public override string ApplicationName { get; set; }

		public static TimeSpan Timeout { get; set; }

		#endregion

		#region Methods

		public override void AddUsersToRoles(string[] usernames, string[] roleNames)
		{
			throw new NotImplementedException();
		}

		public override void CreateRole(string roleName)
		{
			throw new NotImplementedException();
		}

		public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
		{
			throw new NotImplementedException();
		}

		public override string[] FindUsersInRole(string roleName, string usernameToMatch)
		{
			throw new NotImplementedException();
		}

		public override string[] GetAllRoles()
		{
			throw new NotImplementedException();
		}

		public override string[] GetRolesForUser(string identity)
		{
			if (string.IsNullOrWhiteSpace(identity))
			{
				return new string[0];
			}

			if (_roles.TryGetValue(identity, out var result))
			{
				return (string[]) result;
			}

			using var database = WebApplication.GetDatabase();
			var userId = identity.GetUserId();
			var user = database.Accounts.Select(x => new AccountEntity { Id = x.Id, EmailAddress = x.EmailAddress, Roles = x.Roles }).FirstOrDefault(x => x.Id == userId);
			var roles = user?.GetRoles().ToArray() ?? new string[0];

			if (user != null)
			{
				_identityByEmailAddress.Set(user.EmailAddress, identity, TimeService.UtcNow.Add(Timeout));
			}

			if (Timeout != TimeSpan.Zero)
			{
				_roles.Set(identity, roles, TimeService.UtcNow.Add(Timeout));
			}

			return roles;
		}

		public override string[] GetUsersInRole(string roleName)
		{
			throw new NotImplementedException();
		}

		public override bool IsUserInRole(string identity, string roleName)
		{
			return GetRolesForUser(identity).Contains(roleName, StringComparer.OrdinalIgnoreCase);
		}

		public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
		{
			throw new NotImplementedException();
		}

		public static void ResetByEmailAddress(string emailAddress)
		{
			if (_identityByEmailAddress.TryGetValue(emailAddress, out var identity))
			{
				ResetByIdentity((string) identity);
			}
		}

		public static void ResetByIdentity(string identity)
		{
			_roles.Remove(identity);
		}

		public override bool RoleExists(string roleName)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}