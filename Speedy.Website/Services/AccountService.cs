#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Speedy.Data;
using Speedy.Data.WebApi;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Entities;
using Speedy.Website.Samples.Enumerations;

#endregion

namespace Speedy.Website.Services
{
	public class AccountService : BaseService
	{
		#region Constants

		public const string EmptyRoles = ",,";

		#endregion

		#region Constructors

		public AccountService(IContosoDatabase database, AccountEntity account) : base(database, account)
		{
		}

		#endregion

		#region Methods

		public AccountEntity AuthenticateAccount(Credentials model)
		{
			if (string.IsNullOrWhiteSpace(model.Password))
			{
				throw new AuthenticationException(Constants.LoginInvalidError);
			}

			var user = Database.Accounts
				.Where(x => !x.IsDeleted)
				.Select(x => new AccountEntity
				{
					Id = x.Id,
					EmailAddress = x.EmailAddress,
					Name = x.Name,
					PasswordHash = x.PasswordHash,
					Roles = x.Roles
				})
				.FirstOrDefault(x => x.EmailAddress.Equals(model.EmailAddress, StringComparison.OrdinalIgnoreCase));

			if (user == null)
			{
				throw new AuthenticationException(Constants.LoginInvalidError);
			}

			if (!user.PasswordHash.Equals(Hash(model.Password, user.Id.ToString()), StringComparison.OrdinalIgnoreCase))
			{
				throw new AuthenticationException(Constants.LoginInvalidError);
			}

			// Don't allow this update to bump the user modified on
			// See the SyncController comments on why, basically it causes sync issues
			var previousValue = Database.Options.MaintainModifiedOn;
			Database.Options.MaintainModifiedOn = false;
			user.LastLoginDate = TimeService.UtcNow;
			Database.SaveChanges();
			Database.Options.MaintainModifiedOn = previousValue;

			return user;
		}

		public static string CreateSalt()
		{
			var saltBytes = new byte[24];
			using var rng = new RNGCryptoServiceProvider();
			rng.GetBytes(saltBytes);
			return Convert.ToBase64String(saltBytes);
		}

		
		public Account GetAccount(int id)
		{
			ValidateAccountAsAdministrator();

			var user = Database.Accounts.FirstOrDefault(x => x.Id == id);
			if (user == null)
			{
				throw new InvalidOperationException(Constants.IdIsInvalid);
			}

			return ToModel(user);
		}

		public static string Hash(string password, string salt)
		{
			using HashAlgorithm algorithm = new SHA256Managed();
			var pBytes = Encoding.Unicode.GetBytes(password);
			var sBytes = Encoding.Unicode.GetBytes(salt);
			var tBytes = new byte[sBytes.Length + pBytes.Length];

			Buffer.BlockCopy(sBytes, 0, tBytes, 0, sBytes.Length);
			Buffer.BlockCopy(pBytes, 0, tBytes, sBytes.Length, pBytes.Length);

			var hash = algorithm.ComputeHash(tBytes);
			return Convert.ToBase64String(hash);
		}

		#endregion
	}
}