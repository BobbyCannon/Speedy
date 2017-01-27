#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Speedy.Sync;

#endregion

namespace Speedy.IntegrationTests
{
	/// <summary>
	/// Summary description for SyncDatabaseProviderTests.
	/// </summary>
	[TestClass]
	public class SyncDatabaseProviderTests
	{
		#region Methods

		[TestMethod]
		public void GetDatabase()
		{
			var database = new Mock<ISyncableDatabase>();
			var provider = new SyncDatabaseProvider(() => database.Object);

			Assert.IsTrue(database.Object == provider.GetDatabase());
		}

		#endregion
	}
}