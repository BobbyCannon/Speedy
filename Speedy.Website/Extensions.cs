#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

#endregion

namespace Speedy.Website
{
	public static class Extensions
	{
		#region Methods

		/// <summary>
		/// Calculate an MD5 hash for the string.
		/// </summary>
		/// <param name="input"> The string to hash. </param>
		/// <returns> The MD5 formatted hash for the input. </returns>
		public static string CalculateHash(this string input)
		{
			// Calculate MD5 hash from input.
			var inputBytes = Encoding.ASCII.GetBytes(input);
			return inputBytes.CalculateHash();
		}

		/// <summary>
		/// Calculate an MD5 hash for the bytes.
		/// </summary>
		/// <param name="input"> The bytes to hash. </param>
		/// <returns> The MD5 formatted hash for the input. </returns>
		public static string CalculateHash(this byte[] input)
		{
			// Calculate MD5 hash from input.
			var md5 = MD5.Create();
			var hash = md5.ComputeHash(input);

			// Convert byte array to hex string.
			var sb = new StringBuilder();
			foreach (var item in hash)
			{
				sb.Append(item.ToString("X2"));
			}

			// Return the MD5 string.
			return sb.ToString().ToLower();
		}

		public static string CleanMessage(this Exception ex)
		{
			var offset = ex.Message.IndexOf("\r\nParameter");
			return offset > 0 ? ex.Message.Substring(0, offset) : ex.Message;
		}

		/// <summary>
		/// Return true if the value is in the provided collection or false if otherwise.
		/// </summary>
		/// <param name="value"> The value to check for. </param>
		/// <param name="values"> The values to check against. </param>
		/// <returns> True if the value was in the collection or false if otherwise. </returns>
		public static bool ContainsAny(this string value, IEnumerable<string> values)
		{
			return values.Any(value.Contains);
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

		/// <summary>
		/// Get the user ID from the claims principal.
		/// </summary>
		/// <param name="principal"> The claims principal. </param>
		/// <returns> The ID of the user. </returns>
		public static int GetUserId(this ClaimsPrincipal principal)
		{
			if (principal.Identity?.IsAuthenticated != true)
			{
				return 0;
			}

			var claim = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
			return int.TryParse(claim?.Value ?? "0", out var id) ? id : 0;
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

		/// <summary>
		/// Splits user Tags field into a collection.
		/// </summary>
		/// <param name="tags"> The tags in a comma separated format. </param>
		/// <returns> The tag collection. </returns>
		public static IEnumerable<string> SplitTags(this string tags)
		{
			return string.IsNullOrEmpty(tags) ? new string[0] : tags.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Distinct().OrderBy(x => x).ToArray();
		}

		#endregion
	}
}