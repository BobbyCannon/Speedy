#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Speedy.Website.Samples.Entities;
using Speedy.Website.Samples.Enumerations;

#endregion

namespace Speedy.Website
{
	public static class Extensions
	{
		#region Methods

		public static string CleanMessage(this Exception ex)
		{
			var offset = ex.Message.IndexOf("\r\nParameter");
			return offset > 0 ? ex.Message.Substring(0, offset) : ex.Message;
		}

		/// <summary>
		/// Converts the string to an int. If it cannot be parse it will return the default value.
		/// </summary>
		/// <param name="input"> The string to convert. </param>
		/// <param name="defaultValue"> The default value to return. Defaults to 0. </param>
		/// <returns> The int value or the default value. </returns>
		public static int ConvertToInt(this string input, int defaultValue = 0)
		{
			return !int.TryParse(input, out var response) ? defaultValue : response;
		}

		public static string FromBase64(this string data)
		{
			var bytes = Convert.FromBase64String(data);
			return Encoding.UTF8.GetString(bytes);
		}

		public static IEnumerable<string> GetRoles(this AccountEntity account)
		{
			return account?.Roles?.GetRoles();
		}

		public static IEnumerable<string> GetRoles(this string roles)
		{
			return string.IsNullOrEmpty(roles) ? new string[0] : roles.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
		}

		/// <summary>
		/// Get the user ID.
		/// </summary>
		/// <param name="identity"> The user identity. </param>
		/// <returns> The ID of the user. </returns>
		public static int GetUserId(this IIdentity identity)
		{
			return identity.Name?.GetUserId() ?? 0;
		}

		/// <summary>
		/// Get the user ID.
		/// </summary>
		/// <param name="identity"> The user identity. </param>
		/// <returns> The ID of the user. </returns>
		public static int GetUserId(this string identity)
		{
			if (identity == null)
			{
				return 0;
			}

			return !identity.Contains(';') ? 0 : identity.Split(';').First().ConvertToInt();
		}

		public static string GetUserName(this IIdentity identity)
		{
			if (identity.Name == null)
			{
				return "Unknown";
			}

			return !identity.Name.Contains(';') ? "Unknown" : identity.Name.Split(';').Last();
		}

		public static bool InRole(this AccountEntity account, params AccountRole[] roles)
		{
			return roles.Any(x => account?.InRole(x.ToString()) ?? false);
		}

		public static bool InRole(this AccountEntity account, string roleName)
		{
			return account?.GetRoles().Any(x => x.Equals(roleName, StringComparison.OrdinalIgnoreCase)) ?? false;
		}

		#endregion
	}
}