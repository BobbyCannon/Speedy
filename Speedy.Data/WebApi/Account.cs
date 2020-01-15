#region References

using System;
using System.Collections.Generic;
using Speedy.Sync;

#endregion

namespace Speedy.Data.WebApi
{
	/// <summary>
	/// Represents the public account model.
	/// </summary>
	public class Account : SyncModel<int>
	{
		#region Properties

		/// <summary>
		/// The sync ID for the account.
		/// </summary>
		public Guid AddressSyncId { get; set; }

		/// <summary>
		/// The email address for the account.
		/// </summary>
		public string EmailAddress { get; set; }

		/// <summary>
		/// The ID of the account.
		/// </summary>
		public override int Id { get; set; }

		/// <summary>
		/// The name of the account.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The list of roles for the account.
		/// </summary>
		public IEnumerable<string> Roles { get; set; }

		#endregion
	}
}