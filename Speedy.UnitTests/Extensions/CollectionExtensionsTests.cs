#region References

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class CollectionExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void AddIfMissingTests()
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.AddIfMissing("foo", "bar");
			dictionary.AddIfMissing("foo", "bar");
			dictionary.AddIfMissing("foo", "bar");
			dictionary.AddIfMissing("foo", "bar");
			Assert.AreEqual(1, dictionary.Count);
		}
		
		[TestMethod]
		public void AddOrUpdateTests()
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.AddOrUpdate("foo", "bar1");
			dictionary.AddOrUpdate("foo", "bar2");
			dictionary.AddOrUpdate("foo", "bar3");
			dictionary.AddOrUpdate("foo", "bar4");
			Assert.AreEqual(1, dictionary.Count);
			Assert.AreEqual("bar4", dictionary["foo"]);
		}

		[TestMethod]
		public void NaturalSort()
		{
			var data = new[] { "100", "11", "9", "2" };
			var expected = new[] { "2", "9", "11", "100" };
			TestHelper.AreEqual(expected, data.NaturalSort().ToArray());
		}

		#endregion
	}
}