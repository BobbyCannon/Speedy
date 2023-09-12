#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Sync;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class DatabaseOptionsTests : CloneableSpeedUnitTests
	{
		#region Methods

		[TestMethod]
		public void DatabaseOptionsShouldClone()
		{
			var testItems = new[]
			{
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = false, MaintainModifiedOn = false, MaintainSyncId = false, PermanentSyncEntityDeletions = false, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromSeconds(30), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = true, MaintainCreatedOn = false, MaintainModifiedOn = false, MaintainSyncId = false, PermanentSyncEntityDeletions = false, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromSeconds(30), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = true, MaintainModifiedOn = false, MaintainSyncId = false, PermanentSyncEntityDeletions = false, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromSeconds(30), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = false, MaintainModifiedOn = true, MaintainSyncId = false, PermanentSyncEntityDeletions = false, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromSeconds(30), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = false, MaintainModifiedOn = false, MaintainSyncId = true, PermanentSyncEntityDeletions = false, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromSeconds(30), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = false, MaintainModifiedOn = false, MaintainSyncId = false, PermanentSyncEntityDeletions = true, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromSeconds(30), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = false, MaintainModifiedOn = false, MaintainSyncId = false, PermanentSyncEntityDeletions = false, SyncOrder = new[] { "foo" }, Timeout = TimeSpan.FromSeconds(30), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = false, MaintainModifiedOn = false, MaintainSyncId = false, PermanentSyncEntityDeletions = false, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromTicks(1), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = false, MaintainModifiedOn = false, MaintainSyncId = false, PermanentSyncEntityDeletions = false, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromTicks(1), UnmaintainedEntities = new[] { typeof(ISyncEntity) } }
			};

			BaseShouldCloneTest(testItems);
		}

		#endregion
	}
}