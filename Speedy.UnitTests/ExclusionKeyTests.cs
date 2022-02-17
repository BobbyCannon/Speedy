#region References

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class ExclusionKeyTests
	{
		#region Methods

		[TestMethod]
		public void CompareToShouldEqual()
		{
			var expected = GetCombinations();
			var actual = GetCombinations();

			for (var i = 0; i < expected.Length; i++)
			{
				Assert.AreEqual(0, expected[i].CompareTo(actual[i]));
				Assert.AreEqual(0, actual[i].CompareTo(expected[i]));
			}
		}

		[TestMethod]
		public void CompareToShouldNotEqual()
		{
			var expected = GetCombinations();
			var actual = GetCombinations();

			for (var i = 0; i < actual.Length; i++)
			{
				for (var j = 0; j < expected.Length; j++)
				{
					if (i == j)
					{
						Assert.AreEqual(0, expected[j].CompareTo(actual[i]));
						Assert.AreEqual(0, actual[j].CompareTo(expected[i]));
					}
					else
					{
						Assert.AreEqual(-1, expected[j].CompareTo(actual[i]));
						Assert.AreEqual(-1, actual[j].CompareTo(expected[i]));
					}
				}
			}

			var key1 = new ExclusionKey(typeof(AccountEntity), true, true, true);
			Assert.AreEqual(-1, key1.CompareTo((object) null));
			Assert.AreEqual(-1, key1.CompareTo(null));
		}

		[TestMethod]
		public void EqualRanges()
		{
			var actual = new ExclusionKey(typeof(AccountEntity), true, true, true);
			Assert.IsFalse(actual.Equals(null));
			Assert.IsTrue(actual.Equals(actual));
			Assert.IsFalse(actual.Equals((object) null));
			Assert.IsTrue(actual.Equals((object) actual));
			// ReSharper disable once SuspiciousTypeConversion.Global
			Assert.IsFalse(actual.Equals(this));
		}

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

		[TestMethod]
		public void ToStringShouldBeFormatted()
		{
			var combinations = GetCombinations();
			var actual = combinations.Select(x => x.ToString()).ToArray();
			actual.Select(x => $"\"{x}\",").Dump();

			var expected = new[]
			{
				"Speedy.Website.Data.Entities.AddressEntity, I:False, O:False, U:False",
				"Speedy.Website.Data.Entities.AddressEntity, I:True, O:False, U:False",
				"Speedy.Website.Data.Entities.AddressEntity, I:False, O:True, U:False",
				"Speedy.Website.Data.Entities.AddressEntity, I:False, O:False, U:True",
				"Speedy.Website.Data.Entities.AddressEntity, I:True, O:True, U:False",
				"Speedy.Website.Data.Entities.AddressEntity, I:True, O:False, U:True",
				"Speedy.Website.Data.Entities.AddressEntity, I:False, O:True, U:True",
				"Speedy.Website.Data.Entities.AddressEntity, I:True, O:True, U:True",
				"Speedy.Website.Data.Entities.AccountEntity, I:False, O:False, U:False",
				"Speedy.Website.Data.Entities.AccountEntity, I:True, O:False, U:False",
				"Speedy.Website.Data.Entities.AccountEntity, I:False, O:True, U:False",
				"Speedy.Website.Data.Entities.AccountEntity, I:False, O:False, U:True",
				"Speedy.Website.Data.Entities.AccountEntity, I:True, O:True, U:False",
				"Speedy.Website.Data.Entities.AccountEntity, I:True, O:False, U:True",
				"Speedy.Website.Data.Entities.AccountEntity, I:False, O:True, U:True",
				"Speedy.Website.Data.Entities.AccountEntity, I:True, O:True, U:True"
			};

			TestHelper.AreEqual(expected, actual);
		}

		private static ExclusionKey[] GetCombinations()
		{
			var address = typeof(AddressEntity);
			var account = typeof(AccountEntity);

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
				new ExclusionKey(account, false, false, false),
				new ExclusionKey(account, true, false, false),
				new ExclusionKey(account, false, true, false),
				new ExclusionKey(account, false, false, true),
				new ExclusionKey(account, true, true, false),
				new ExclusionKey(account, true, false, true),
				new ExclusionKey(account, false, true, true),
				new ExclusionKey(account, true, true, true)
			};
		}

		#endregion
	}
}