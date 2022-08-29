#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Speedy.Data;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.Website.Services
{
	[ExcludeFromCodeCoverage]
	public class AuthenticationService : IAuthenticationService
	{
		#region Fields

		private readonly IHttpContextAccessor _contextAccessor;
		private readonly AccountService _service;
		private static readonly MemoryCache _userCache;

		#endregion

		#region Constructors

		public AuthenticationService(AccountService service, IHttpContextAccessor contextAccessor)
		{
			_service = service;
			_contextAccessor = contextAccessor;
		}

		static AuthenticationService()
		{
			_userCache = new MemoryCache(new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromSeconds(5) });
		}

		#endregion

		#region Properties

		public bool IsAuthenticated => _contextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

		public ClaimsPrincipal User => _contextAccessor.HttpContext?.User;

		public int UserId => User.GetUserId();

		public string UserName => User.Identity?.Name ?? string.Empty;

		#endregion

		#region Methods

		public static void AddOrUpdateCacheEntry(int userId, DateTime modifiedOn)
		{
			// Try and clear the cache first
			_userCache.Remove(userId);

			// Add the user modified on to the cache
			_userCache.Set(userId, modifiedOn, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(5) });
		}

		public static AuthenticationTicket CreateTicket(AccountEntity user, bool rememberMe, string authenticationType)
		{
			if (user == null)
			{
				throw new ArgumentException(nameof(user));
			}

			var roles = user.Roles.SplitTags();
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.Name)
			};

			claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

			var identity = new ClaimsIdentity(claims, authenticationType);
			var principal = new ClaimsPrincipal(identity);
			var ticket = new AuthenticationTicket(principal, authenticationType);
			ticket.Properties.IsPersistent = rememberMe;
			ticket.Properties.Items.Add(nameof(AccountEntity.ModifiedOn), user.ModifiedOn.Ticks.ToString());

			return ticket;
		}

		public bool LogIn(Credentials credentials)
		{
			if (string.IsNullOrWhiteSpace(credentials.Password))
			{
				throw new UnauthorizedAccessException(Constants.LoginInvalidError);
			}

			var user = _service.AuthenticateAccount(credentials);
			if (user == null)
			{
				return false;
			}

			user.LastLoginDate = TimeService.UtcNow;
			var ticket = CreateTicket(user, credentials.RememberMe, CookieAuthenticationDefaults.AuthenticationScheme);
			_contextAccessor.HttpContext?.SignInAsync(ticket.Principal, ticket.Properties);
			return true;
		}

		public void LogIn(AccountEntity user)
		{
			user.LastLoginDate = TimeService.UtcNow;
			var ticket = CreateTicket(user, true, CookieAuthenticationDefaults.AuthenticationScheme);
			_contextAccessor.HttpContext?.SignInAsync(ticket.Principal, ticket.Properties);
		}

		public void LogOut()
		{
			_contextAccessor.HttpContext?.SignOutAsync();
		}

		public static void ValidatePrincipal(CookieValidatePrincipalContext context, IDatabaseProvider<IContosoDatabase> databaseProvider)
		{
			if (!context.Properties.Items.ContainsKey(nameof(AccountEntity.ModifiedOn)))
			{
				context.RejectPrincipal();
				return;
			}

			var modifiedOnValue = context.Properties.Items[nameof(AccountEntity.ModifiedOn)];
			if (!long.TryParse(modifiedOnValue, out var modifiedOnTicks))
			{
				context.RejectPrincipal();
				return;
			}

			var modifiedOn = new DateTime(modifiedOnTicks);
			var userId = context.Principal.GetUserId();
			DateTime? userModifiedOn = null;

			if (_userCache.TryGetValue<DateTime>(userId, out var cachedValue))
			{
				userModifiedOn = cachedValue;
			}

			// Now unsure the user has not changed, if it has reject the principal or renew it?
			if (userModifiedOn == modifiedOn)
			{
				// Everything is fine so just bounce.
				return;
			}

			using var database = databaseProvider.GetDatabase();
			var userEntity = database.Accounts.FirstOrDefault(u => u.Id == userId);

			if (userEntity == null)
			{
				// Failed to find the user so reject the cookie
				_userCache.Remove(userId);
				context.RejectPrincipal();
				return;
			}

			// Update the cookie because something has changed. Ex. Roles, Name, etc.
			var ticket = CreateTicket(userEntity, true, CookieAuthenticationDefaults.AuthenticationScheme);
			context.HttpContext.SignInAsync(ticket.Principal, ticket.Properties);
			context.ReplacePrincipal(ticket.Principal);

			// Refresh the local cache
			AddOrUpdateCacheEntry(userId, userEntity.ModifiedOn);
		}

		#endregion
	}
}