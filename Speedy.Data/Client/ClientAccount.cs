#region References

using System;
using System.Collections.Generic;
using Speedy.Extensions;
using Speedy.Sync;

#endregion

namespace Speedy.Data.Client
{
	/// <summary>
	/// Represents the client private account model.
	/// </summary>
	public class ClientAccount : SyncModel<int>
	{
		#region Properties

		/// <summary>
		/// The required address for this person.
		/// </summary>
		public virtual ClientAddress Address { get; set; }

		/// <summary>
		/// The ID for the address.
		/// </summary>
		public long AddressId { get; set; }

		/// <summary>
		/// The sync ID for the address.
		/// </summary>
		public Guid AddressSyncId { get; set; }

		/// <summary>
		/// The email address for the account.
		/// </summary>
		public string EmailAddress { get; set; }

		/// <inheritdoc />
		public override int Id { get; set; }

		/// <summary>
		/// Represents the last time this account was update on the client
		/// </summary>
		public DateTime LastClientUpdate { get; set; }

		/// <summary>
		/// The name of the account.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The roles in storage format.
		/// </summary>
		public string Roles { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Combine the roles into a custom format for client side storage.
		/// This format is different than the server format just to show that
		/// client and server formats can be different.
		/// </summary>
		/// <param name="roles"> The roles for the account. </param>
		/// <returns> The roles in client storage format. </returns>
		public static string CombineRoles(IEnumerable<string> roles)
		{
			return roles != null ? $";{string.Join(";", roles)};" : ";;";
		}

		/// <summary>
		/// Splits the roles from the custom format into an array.
		/// </summary>
		/// <param name="roles"> The roles for the account. </param>
		/// <returns> The array of roles. </returns>
		public static IEnumerable<string> SplitRoles(string roles)
		{
			return roles != null ? roles.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
		}

		/// <inheritdoc />
		protected override HashSet<string> GetDefaultExclusionsForIncomingSync()
		{
			return base.GetDefaultExclusionsForIncomingSync()
				.Append(nameof(Address), nameof(AddressId), nameof(LastClientUpdate));
		}

		/// <inheritdoc />
		protected override HashSet<string> GetDefaultExclusionsForOutgoingSync()
		{
			// Update defaults are the same as incoming sync defaults
			return GetDefaultExclusionsForIncomingSync();
		}

		/// <inheritdoc />
		protected override HashSet<string> GetDefaultExclusionsForSyncUpdate()
		{
			// Update defaults are the same as incoming sync defaults plus some
			return base.GetDefaultExclusionsForSyncUpdate()
				.Append(GetDefaultExclusionsForIncomingSync());
		}

		#endregion
	}
}