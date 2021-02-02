#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class GuidExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void NumberToGuidShouldWork()
		{
			Assert.AreEqual(Guid.Parse("00000000-0000-0000-0000-000000000001"), 1.ToGuid());
			Assert.AreEqual(Guid.Parse("00000000-0000-0000-0000-000000123456"), 123456.ToGuid());
			Assert.AreEqual(Guid.Parse("00000000-0000-0000-0012-345678901234"), 12345678901234L.ToGuid());
			Assert.AreEqual(Guid.Parse("00000000-0000-0000-0123-456789012345"), 123456789012345U.ToGuid());
		}

		#endregion
	}
}