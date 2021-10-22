#region References

using System;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using Speedy.Data;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.Website.Services
{
	public class AccountService : BaseService
	{
		#region Constructors

		public AccountService(IContosoDatabase database, AccountEntity account = null) : base(database, account)
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
				.FirstOrDefault(x => x.EmailAddress == model.EmailAddress);

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

		public static string Hash(string password, string salt)
		{
			using HashAlgorithm algorithm = SHA256.Create();
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