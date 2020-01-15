#region References

using Speedy.Sync;

#endregion

namespace Speedy.Data.WebApi
{
	/// <summary>
	/// Represents the public address model.
	/// </summary>
	public class Address : SyncModel<long>
	{
		#region Properties

		/// <summary>
		/// The city for the address.
		/// </summary>
		public string City { get; set; }

		/// <summary>
		/// The ID for the address.
		/// </summary>
		public override long Id { get; set; }

		/// <summary>
		/// The line 1 for the address.
		/// </summary>
		public string Line1 { get; set; }

		/// <summary>
		/// The line 2 for the address.
		/// </summary>
		public string Line2 { get; set; }

		/// <summary>
		/// The postal for the address.
		/// </summary>
		public string Postal { get; set; }

		/// <summary>
		/// The state for the address.
		/// </summary>
		public string State { get; set; }

		#endregion
	}
}