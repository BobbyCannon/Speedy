#region References

using System.Collections.Generic;
using Speedy.Extensions;
using Speedy.Sync;

#endregion

namespace Speedy.Website.Data.Entities
{
	public class SettingEntity : SyncEntity<long>
	{
		#region Properties

		public override long Id { get; set; }

		public string Name { get; set; }

		public string Value { get; set; }

		#endregion

		#region Methods

		protected override HashSet<string> GetDefaultExclusionsForIncomingSync()
		{
			return base.GetDefaultExclusionsForIncomingSync()
				.Append(nameof(SyncId));
		}

		protected override HashSet<string> GetDefaultExclusionsForOutgoingSync()
		{
			return base.GetDefaultExclusionsForOutgoingSync()
				.Append(GetDefaultExclusionsForIncomingSync());
		}

		protected override HashSet<string> GetDefaultExclusionsForSyncUpdate()
		{
			return base.GetDefaultExclusionsForSyncUpdate()
				.Append(GetDefaultExclusionsForIncomingSync());
		}

		#endregion
	}
}