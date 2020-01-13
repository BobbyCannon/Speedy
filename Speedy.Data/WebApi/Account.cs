#region References

using System;
using System.Collections.Generic;
using Speedy.Sync;

#endregion

namespace Speedy.Data.WebApi
{
	public class Account : SyncModel<int>
	{
		#region Properties

		public Guid AddressSyncId { get; set; }

		public string EmailAddress { get; set; }

		public override int Id { get; set; }

		public string Name { get; set; }

		public IEnumerable<string> Roles { get; set; }

		#endregion
	}
}