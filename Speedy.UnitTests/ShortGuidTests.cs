#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class ShortGuidTests
	{
		#region Methods

		[TestMethod]
		public void ParseGuid()
		{
			var actual = ShortGuid.ParseGuid("B2751797-F420-4FFD-A9B9-722C0A3C4FDF");
			Assert.AreEqual("lxd1siD0_U-puXIsCjxP3w", actual.ToString());
			Assert.AreEqual("b2751797-f420-4ffd-a9b9-722c0a3c4fdf", actual.Guid.ToString());
		}
		
		[TestMethod]
		public void ParseShortGuid()
		{
			var actual = ShortGuid.ParseShortGuid("lxd1siD0_U-puXIsCjxP3w");
			Assert.AreEqual("lxd1siD0_U-puXIsCjxP3w", actual.ToString());
			Assert.AreEqual("b2751797-f420-4ffd-a9b9-722c0a3c4fdf", actual.Guid.ToString());
		}

		#endregion
	}
}