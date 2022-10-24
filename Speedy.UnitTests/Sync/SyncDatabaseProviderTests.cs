#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Speedy.Sync;

#endregion

namespace Speedy.UnitTests.Sync
{
	/// <summary>
	/// Summary description for SyncDatabaseProviderTests.
	/// </summary>
	[TestClass]
	public class SyncDatabaseProviderTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void GetDatabase()
		{
			var database = new Mock<ISyncableDatabase>();
			var provider = new SyncableDatabaseProvider((x, y) => database.Object, null, null);

			Assert.IsTrue(database.Object == provider.GetSyncableDatabase());
		}

		#endregion
	}
}