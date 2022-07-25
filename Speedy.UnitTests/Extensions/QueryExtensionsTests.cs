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
			var request = new PagedRequest { Page = 1, PerPage = 1 };
			var result = query.GetPagedResults(request, x => new Account { Id = x.Id }, x => x.Id);

			Assert.AreEqual(1, result.Page);
			Assert.AreEqual(1, result.TotalCount);
			Assert.AreEqual(1, result.Results.Count);
			Assert.AreEqual(1, result.Results[0].Id);
		}
		
		[TestMethod]
		public void ConvertResult()
		{
			var provider = TestHelper.GetMemoryProvider();
			using var database = provider.GetDatabase();

			var query = database.Accounts.Where(x => x.Id > 0);
			var request = new PagedRequest { Page = 1, PerPage = 1 };
			var anonymousResult = query.GetPagedResults(request, x => new { x.Id, x.Address.FullAddress }, x => x.Id);
			var typedResult = anonymousResult.ConvertResults(x => new AccountView { Id = x.Id, FullAddress = x.FullAddress });

			TestHelper.AreEqual<IPagedResults>(anonymousResult, typedResult, nameof(anonymousResult.Results));
			Assert.AreEqual(1, typedResult.Results.Count);
			Assert.AreEqual(1, typedResult.Results[0].Id);
			Assert.AreEqual("Line1\r\nCity, ST  12345", typedResult.Results[0].FullAddress);
		}
		
		[TestMethod]
		public void GetPagedResultsWithCustomRequestAndResult()
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

		public class AccountView
		{
			public int Id { get; set; }
			public string FullAddress { get; set; }
		}
	}
}