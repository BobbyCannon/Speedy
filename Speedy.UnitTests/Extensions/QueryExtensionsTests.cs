#region References

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.SyncApi;
using Speedy.Extensions;
using Speedy.Website.Data.Entities;
using Speedy.Website.Models;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class QueryExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void GetPagedResults()
		{
			var provider = TestHelper.GetMemoryProvider();
			using var database = provider.GetDatabase();

			var query = database.Accounts.Where(x => x.Id > 0);
			var request = new CustomPagedRequest { Page = 1, PerPage = 1 };
			var result = query
				.GetPagedResults<AccountEntity, Account, CustomPagedResults<Account>, CustomPagedRequest>(request,
					x => new Account { Id = x.Id }, x => x.Id);

			Assert.AreEqual(1, result.Page);
			Assert.AreEqual(1, result.TotalCount);
			Assert.AreEqual(1, result.Results.Count);
			Assert.AreEqual(1, result.Results[0].Id);
		}

		[TestMethod]
		public void GetPagedResultsNoTransform()
		{
			var provider = TestHelper.GetMemoryProvider();
			using var database = provider.GetDatabase();

			var query = database.Accounts.Where(x => x.Id > 0);
			var request = new CustomPagedRequest { Page = 1, PerPage = 1 };
			var result = query.GetPagedResults<AccountEntity, CustomPagedResults<AccountEntity>, CustomPagedRequest>(request);

			Assert.AreEqual(1, result.Page);
			Assert.AreEqual(1, result.TotalCount);
			Assert.AreEqual(1, result.Results.Count);
			Assert.AreEqual(1, result.Results[0].Id);
		}

		#endregion
	}
}