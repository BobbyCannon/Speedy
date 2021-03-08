#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Sync;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class EnumExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void SetFlag()
		{
			var status = SyncResultStatus.Started;
			status = status.SetFlag(SyncResultStatus.Cancelled);

			Assert.IsTrue((status & SyncResultStatus.Started) == SyncResultStatus.Started, "Flag was not set");
			Assert.IsTrue((status & SyncResultStatus.Cancelled) == SyncResultStatus.Cancelled, "Flag was not set");
		}

		#endregion
	}
}