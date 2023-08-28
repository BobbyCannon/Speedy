#region References

using System.Collections.Generic;
using Speedy.Data.SyncApi;
using Speedy.Extensions;

#endregion

namespace Speedy.Website.Data.Entities
{
	public class SettingEntity : Setting
	{
		#region Constructors

		public SettingEntity()
		{
			ResetHasChanges();
		}

		#endregion

		#region Methods

		protected override HashSet<string> GetDefaultExclusionsForIncomingSync()
		{
			return base.GetDefaultExclusionsForIncomingSync();
		}

		protected override HashSet<string> GetDefaultExclusionsForOutgoingSync()
		{
			return base.GetDefaultExclusionsForOutgoingSync()
				.Append(GetDefaultExclusionsForIncomingSync());
		}

		protected override HashSet<string> GetDefaultExclusionsForSyncUpdate()
		{
			return base.GetDefaultExclusionsForSyncUpdate()
				.Append(GetDefaultExclusionsForIncomingSync())
				.Append(nameof(IsDeleted));
		}

		#endregion
	}
}