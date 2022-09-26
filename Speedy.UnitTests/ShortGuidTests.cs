#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Speedy.Extensions;

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
		[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
		public void Equals()
		{
			var expected = Guid.Parse("B2751797-F420-4FFD-A9B9-722C0A3C4FDF");
			var guid1 = ShortGuid.ParseGuid("B2751797-F420-4FFD-A9B9-722C0A3C4FDF");
			var guid2 = new ShortGuid(expected);

			Assert.IsTrue(guid1 == guid2);
			Assert.IsTrue(guid1.Equals(guid2));
			Assert.IsTrue(Equals(guid1, guid2));
			Assert.IsTrue(Equals(guid1, expected));
			Assert.IsTrue(Equals(guid1, "lxd1siD0_U-puXIsCjxP3w"));

			// Not equal but using Equals method
			Assert.IsFalse(guid1.Equals(ShortGuid.Empty));
			Assert.IsFalse(guid1.Equals(null));
			Assert.IsFalse(guid1.Equals(21));
			Assert.IsFalse(guid1.Equals(true));
		}

		[TestMethod]
		public void EqualsNot()
		{
			var guid1 = ShortGuid.Empty;
			var guid2 = new ShortGuid(Guid.Parse("B2751797-F420-4FFD-A9B9-722C0A3C4FDF"));

			Assert.IsTrue(guid1 != guid2);
		}

		[TestMethod]
		public void GetHashCodeTest()
		{
			var guid1 = ShortGuid.Empty;
			var guid2 = new ShortGuid(Guid.Parse("B2751797-F420-4FFD-A9B9-722C0A3C4FDF"));

			Assert.AreEqual(0, guid1.GetHashCode());
			Assert.AreEqual(246769172, guid2.GetHashCode());
		}

		[TestMethod]
		public void ImplicitConversions()
		{
			var expectedGuid = Guid.Parse("B2751797-F420-4FFD-A9B9-722C0A3C4FDF");
			var expectedShortGuid = expectedGuid.ToShortGuid();
			var expectedShortGuidString = expectedShortGuid.Value;

			ShortGuid actual1 = "lxd1siD0_U-puXIsCjxP3w";
			string actual2 = expectedShortGuid;
			Guid actual3 = expectedShortGuid;
			ShortGuid actual4 = expectedGuid;

			Assert.AreEqual(expectedShortGuid, actual1);
			Assert.AreEqual(expectedShortGuidString, actual2);
			Assert.AreEqual(expectedGuid, actual3);
			Assert.AreEqual(expectedShortGuid, actual4);
		}

		[TestMethod]
		public void NewGuid()
		{
			var actual = ShortGuid.NewGuid();
			Assert.AreNotEqual(ShortGuid.Empty, actual);
			Assert.AreNotEqual(0, actual.GetHashCode());
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