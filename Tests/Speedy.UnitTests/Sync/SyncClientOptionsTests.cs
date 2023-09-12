#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Sync;

#endregion

namespace Speedy.UnitTests.Sync
{
	[TestClass]
	public class SyncClientOptionsTests : CloneableSpeedUnitTests
	{
		#region Methods

		[TestMethod]
		public void SyncClientOptionsShouldClone()
		{
			var testItems = new[]
			{
				new SyncClientOptions { EnablePrimaryKeyCache = false, IsServerClient = false },
				new SyncClientOptions { EnablePrimaryKeyCache = true, IsServerClient = false },
				new SyncClientOptions { EnablePrimaryKeyCache = false, IsServerClient = false },
				new SyncClientOptions { EnablePrimaryKeyCache = false, IsServerClient = true }
			};

			BaseShouldCloneTest(testItems);
		}

		#endregion
	}
}