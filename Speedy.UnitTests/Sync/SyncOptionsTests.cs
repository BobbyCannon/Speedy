#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Sync;

#endregion

namespace Speedy.UnitTests.Sync
{
	[TestClass]
	public class SyncOptionsTests : BaseModelTests<SyncOptions>
	{
		#region Methods

		[TestMethod]
		public void AllPropertiesSet()
		{
			ValidateModel(GetModelWithNonDefaultValues());
		}

		#endregion
	}
}