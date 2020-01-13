#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class ExclusionKeyTests
	{
		#region Methods

		[TestMethod]
		public void ShouldEqual()
		{
			var expected = GetCombinations();
			var actual = GetCombinations();

			for (var i = 0; i < expected.Length; i++)
			{
				Assert.AreEqual(expected[i], actual[i]);
			}
		}

		[TestMethod]
		public void ShouldNotEqual()
		{
			var expected = GetCombinations();
			var actual = GetCombinations();

			for (var i = 0; i < actual.Length; i++)
			{
				for (var j = 0; j < expected.Length; j++)
				{
					if (i == j)
					{
						Assert.AreEqual(expected[j], actual[i]);
					}
					else
					{
						Assert.AreNotEqual(expected[j], actual[i]);
					}
				}
			}
		}

		private static ExclusionKey[] GetCombinations()
		{
			var address = typeof(AddressEntity);
			var person = typeof(AccountEntity);

			return new[]
			{
				new ExclusionKey(address, false, false, false),
				new ExclusionKey(address, true, false, false),
				new ExclusionKey(address, false, true, false),
				new ExclusionKey(address, false, false, true),
				new ExclusionKey(address, true, true, false),
				new ExclusionKey(address, true, false, true),
				new ExclusionKey(address, false, true, true),
				new ExclusionKey(address, true, true, true),
				new ExclusionKey(person, false, false, false),
				new ExclusionKey(person, true, false, false),
				new ExclusionKey(person, false, true, false),
				new ExclusionKey(person, false, false, true),
				new ExclusionKey(person, true, true, false),
				new ExclusionKey(person, true, false, true),
				new ExclusionKey(person, false, true, true),
				new ExclusionKey(person, true, true, true)
			};
		}

		#endregion
	}
}