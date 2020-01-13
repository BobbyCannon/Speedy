#region References

using Speedy.Sync;

#endregion

namespace Speedy.Data.WebApi
{
	public class Address : SyncModel<long>
	{
		#region Properties

		public string City { get; set; }

		public override long Id { get; set; }

		public string Line1 { get; set; }

		public string Line2 { get; set; }

		public string Postal { get; set; }

		public string State { get; set; }

		#endregion
	}
}