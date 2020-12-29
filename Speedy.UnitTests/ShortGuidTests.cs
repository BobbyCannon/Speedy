#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class ShortGuidTests
	{
		#region Methods

		[TestMethod]
		public void Deserialize()
		{
			var guid = new ShortGuid(Guid.Parse("C64D607D-EC15-4F82-9B36-3DB2D17E037C"));
			Assert.AreEqual("c64d607d-ec15-4f82-9b36-3db2d17e037c", guid.Guid.ToString());
			Assert.AreEqual("fWBNxhXsgk-bNj2y0X4DfA", guid.Value);

			var data = JsonConvert.SerializeObject(guid);
			var expected = "{\"Guid\":\"c64d607d-ec15-4f82-9b36-3db2d17e037c\",\"Value\":\"fWBNxhXsgk-bNj2y0X4DfA\"}";
			Assert.AreEqual(expected, data);

			var actual = JsonConvert.DeserializeObject<ShortGuid>(expected);
			Assert.AreEqual(guid.Guid, actual.Guid);
			Assert.AreEqual(guid.Value, actual.Value);
		}

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