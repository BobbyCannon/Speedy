#region References

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Extensions;
using Speedy.UnitTests;
using Speedy.UnitTests.Factories;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
	public class RepositoryTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void BulkAddAccountEntityWithRelationshipEntity()
		{
			TestHelper.GetDataContexts(initialize: false)
				.ForEach(provider =>
				{
					AccountEntity[] expected;

					using (var database = provider.GetDatabase())
					{
						var address = EntityFactory.GetAddress();
						database.Addresses.Add(address);
						database.SaveChanges();

						Console.WriteLine(database.GetType().Name);

						expected = new[]
						{
							EntityFactory.GetAccount(x => { x.Nickname = "Johnny"; }, "John Doe", address),
							EntityFactory.GetAccount(x => { x.Nickname = "Jane"; }, "Jane Doe", address)
						};

						var watch = Stopwatch.StartNew();
						database.Accounts.BulkAdd(expected);
						watch.Elapsed.Dump();
					}

					using (var database = provider.GetDatabase())
					{
						var actual = database.Accounts.ToList();
						Assert.AreEqual(2, actual.Count);
						Assert.AreEqual(1, actual[0].Id);
						AreEqual(expected[0].Unwrap(), actual[0].Unwrap(), "Id");
						Assert.AreEqual(2, actual[1].Id);
						AreEqual(expected[1].Unwrap(), actual[1].Unwrap(), "Id");
					}
				});
		}

		[TestMethod]
		public void BulkAddAddressEntity()
		{
			TestHelper.GetDataContexts(initialize: false)
				.ForEach(provider =>
				{
					AddressEntity[] expected;

					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						expected = new[]
						{
							EntityFactory.GetAddress(x =>
							{
								x.Line1 = "123 Main Street";
								x.Line2 = "";
								x.City = "Easley";
								x.State = "SC";
								x.Postal = "29640";
							}),
							EntityFactory.GetAddress(x =>
							{
								x.Line1 = "1 Hwy 81";
								x.Line2 = "";
								x.City = "Iva";
								x.State = "SC";
								x.Postal = "29655";
							})
						};

						database.Addresses.BulkAdd(expected);
					}

					using (var database = provider.GetDatabase())
					{
						var actual = database.Addresses.ToList();
						Assert.AreEqual(2, actual.Count);
						Assert.AreEqual(1, actual[0].Id);
						AreEqual(expected[0].Unwrap(), actual[0].Unwrap(), "Id");
						Assert.AreEqual(2, actual[1].Id);
						AreEqual(expected[1].Unwrap(), actual[1].Unwrap(), "Id");
					}
				});
		}

		[TestMethod]
		public void BulkAddOrUpdateAddressEntity()
		{
			TestHelper.GetDataContexts(initialize: false)
				.ForEach(provider =>
				{
					AddressEntity[] expected;

					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						expected = new[]
						{
							EntityFactory.GetAddress(x =>
							{
								x.Line1 = "123 Main Street";
								x.Line2 = "";
								x.City = "Easley";
								x.State = "SC";
								x.Postal = "29640";
								x.SyncId = Guid.Parse("665E5166-179E-4652-86A4-B246240EAD6B");
							}),
							EntityFactory.GetAddress(x =>
							{
								x.Line1 = "1 Hwy 81";
								x.Line2 = "";
								x.City = "Iva";
								x.State = "SC";
								x.Postal = "29655";
								x.SyncId = Guid.Parse("293BF763-0C4B-4606-A629-A6377DA52C87");
							})
						};

						database.Addresses.BulkAddOrUpdate(expected);
					}

					using (var database = provider.GetDatabase())
					{
						var actual = database.Addresses.ToList();
						Assert.AreEqual(2, actual.Count);
						Assert.AreEqual(1, actual[0].Id);
						AreEqual(expected[0].Unwrap(), actual[0].Unwrap(), "Id");
						Assert.AreEqual(2, actual[1].Id);
						AreEqual(expected[1].Unwrap(), actual[1].Unwrap(), "Id");
					}
				});
		}

		#endregion
	}
}