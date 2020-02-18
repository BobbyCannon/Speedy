#region References

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
		public void NaturalSort()
		{
			var data = new[] { "100", "11", "9", "2" };
			var expected = new[] { "2", "9", "11", "100" };
			TestHelper.AreEqual(expected, data.NaturalSort().ToArray());
		}

		#endregion
	}
}