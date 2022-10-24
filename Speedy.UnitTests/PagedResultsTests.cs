#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Data.SyncApi;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.Sync;
using Speedy.UnitTests.Factories;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class PagedResultsTests : SpeedyUnitTest<PagedResults<object>>
	{
		#region Methods

		[TestMethod]
		public void CalculatePaginationValues()
		{
			// Should only have 1 page
			var results = new PagedResults<object> { Page = 1, PerPage = 10, TotalCount = 10 };
			var actual = results.CalculatePaginationValues();
			Assert.AreEqual(1, actual.start);
			Assert.AreEqual(1, actual.end);

			// Should only show first 5 pages
			results = new PagedResults<object> { Page = 1, PerPage = 10, TotalCount = 100 };
			actual = results.CalculatePaginationValues();
			Assert.AreEqual(1, actual.start);
			Assert.AreEqual(5, actual.end);

			// Should only show last 5 pages
			results = new PagedResults<object> { Page = 10, PerPage = 10, TotalCount = 100 };
			actual = results.CalculatePaginationValues();
			Assert.AreEqual(6, actual.start);
			Assert.AreEqual(10, actual.end);

			// Should only almost first 5 pages
			results = new PagedResults<object> { Page = 4, PerPage = 10, TotalCount = 100 };
			actual = results.CalculatePaginationValues();
			Assert.AreEqual(2, actual.start);
			Assert.AreEqual(6, actual.end);

			// Should only almost last 5 pages
			results = new PagedResults<object> { Page = 7, PerPage = 10, TotalCount = 100 };
			actual = results.CalculatePaginationValues();
			Assert.AreEqual(5, actual.start);
			Assert.AreEqual(9, actual.end);
		}

		[TestMethod]
		public void Constructor()
		{
			var request = new PagedRequest();
			var collection = new[] { new Address(), new Address() };
			var actual = new PagedResults<Address>(request, collection.Length, collection);
			Assert.AreEqual(2, actual.Results.ToList().Count);
		}

		[TestMethod]
		public void DefaultEmptyJson()
		{
			var request = new PagedRequest();
			var response = new PagedResults<object>(request, 0);
			var actual = response.ToJson();
			actual.Escape().Dump();
			var expected = "{\"$id\":\"1\",\"Filter\":\"\",\"HasMore\":false,\"Order\":\"\",\"Page\":1,\"PerPage\":10,\"TotalCount\":0,\"TotalPages\":1,\"Results\":[]}";
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void HasMore()
		{
			var actual = new PagedResults<object> { Page = 1, PerPage = 10, TotalCount = 10 };
			Assert.AreEqual(false, actual.HasMore);

			actual = new PagedResults<object> { Page = 1, PerPage = 10, TotalCount = 11 };
			Assert.AreEqual(true, actual.HasMore);

			actual = new PagedResults<object> { Page = 10, PerPage = 1, TotalCount = 11 };
			Assert.AreEqual(true, actual.HasMore);

			// This is not a valid result but should still "not" have more data
			actual = new PagedResults<object> { Page = 12, PerPage = 1, TotalCount = 11 };
			Assert.AreEqual(false, actual.HasMore);
		}

		[TestMethod]
		public void ResultsFromPagedRequest()
		{
			var request = new PagedRequest();
			request.ParseQueryString("?filter=foo&page=2&roleId=5");
			var actual = new PagedResults<object>(request, 400, 1, "bar");
			Assert.AreEqual(2, actual.Page);
			Assert.AreEqual(10, actual.PerPage);
			Assert.AreEqual(40, actual.TotalPages);
			Assert.AreEqual(400, actual.TotalCount);
			Assert.AreEqual(4, actual.Updates.Count);
			TestHelper.AreEqual(new []
			{
				new PartialUpdateValue("Filter", "foo"),
				new PartialUpdateValue("Page", 2),
				new PartialUpdateValue("roleId", "5"),
				new PartialUpdateValue("TotalCount", 400)
			}, actual.Updates.Values.ToArray());
		}

		[TestMethod]
		public void ToJsonThenFromJson()
		{
			var request = new PagedRequest { Page = 2, PerPage = 11 };
			var results = new PagedResults<object>(request, 12, 1, "foo", true);
			var actual = results.ToRawJson();
			var expected = "{\"Filter\":\"\",\"HasMore\":false,\"Order\":\"\",\"Page\":2,\"PerPage\":11,\"TotalCount\":12,\"TotalPages\":2,\"Results\":[1,\"foo\",true]}";
			actual.Escape().Dump();
			Assert.AreEqual(expected, actual);

			// Options and Updates are not required to be equal after serialization
			var update = expected.FromJson<PagedResults<object>>();
			TestHelper.AreEqual(results, update, nameof(results.Options), nameof(results.Updates));

			actual = results.ToRawJson(true, true);
			expected = "{\r\n  \"filter\": \"\",\r\n  \"hasMore\": false,\r\n  \"order\": \"\",\r\n  \"page\": 2,\r\n  \"perPage\": 11,\r\n  \"totalCount\": 12,\r\n  \"totalPages\": 2,\r\n  \"results\": [\r\n    1,\r\n    \"foo\",\r\n    true\r\n  ]\r\n}";
			actual.Escape().Dump();
			Assert.AreEqual(expected, actual);

			// Options and Updates are not required to be equal after serialization
			update = expected.FromJson<PagedResults<object>>();
			TestHelper.AreEqual(results, update);
		}

		[TestMethod]
		public void ToJsonThenFromJsonWithArrayOfArrays()
		{
			var request = new PagedRequest { Page = 2, PerPage = 11 };
			var results = new PagedResults<int[][]>(request, 12,
				new[]
				{
					new[] { 1, 2, 3 },
					new[] { 4, 5, 6 }
				},
				new[]
				{
					new[] { 7, 8, 9 },
					new[] { 10, 11, 12 }
				}
			);

			var actual = results.ToRawJson();
			var expected = "{\"Filter\":\"\",\"HasMore\":false,\"Order\":\"\",\"Page\":2,\"PerPage\":11,\"TotalCount\":12,\"TotalPages\":2,\"Results\":[[[1,2,3],[4,5,6]],[[7,8,9],[10,11,12]]]}";
			//actual.Escape().Dump();
			Assert.AreEqual(expected, actual);

			var actualPagedResults = actual.FromJson<PagedResults<int[][]>>();
			TestHelper.AreEqual(results, actualPagedResults);
			Assert.AreEqual(2, actualPagedResults.Results.Count);
		}
		
		[TestMethod]
		public void ToJsonThenFromJsonSyncObject()
		{
			var request = new PagedRequest { Page = 2, PerPage = 11 };
			var results = new PagedResults<SyncObject>(request, 12,
				EntityFactory.GetAccount(x =>
				{
					x.AddressSyncId = Guid.Parse("4562A619-89FA-4D64-A91A-66750184491F");
					x.SyncId = Guid.Parse("E778E441-4486-478B-8661-4AA107FC92E3");
					x.CreatedOn = new DateTime(2022, 10, 17, 05, 43, 21, DateTimeKind.Utc);
					x.ModifiedOn = new DateTime(2022, 10, 17, 05, 43, 22, DateTimeKind.Utc);
				}, "Bobby").ToSyncObject(),
				EntityFactory.GetAccount(x =>
				{
					x.AddressSyncId = Guid.Parse("E49366C5-0CDB-45F7-A997-8F27471B8FBE");
					x.SyncId = Guid.Parse("F77EC788-38F5-493D-9CA9-EC57BFCDCF8C");
					x.CreatedOn = new DateTime(2022, 10, 17, 05, 43, 23, DateTimeKind.Utc);
					x.ModifiedOn = new DateTime(2022, 10, 17, 05, 43, 24, DateTimeKind.Utc);
				}, "Fred").ToSyncObject()
			);
			var actual = results.ToRawJson();
			var expected = "{\"Filter\":\"\",\"HasMore\":false,\"Order\":\"\",\"Page\":2,\"PerPage\":11,\"TotalCount\":12,\"TotalPages\":2,\"Results\":[{\"Data\":\"{\\\"AddressId\\\":0,\\\"AddressSyncId\\\":\\\"4562a619-89fa-4d64-a91a-66750184491f\\\",\\\"CreatedOn\\\":\\\"2022-10-17T05:43:21Z\\\",\\\"EmailAddress\\\":null,\\\"ExternalId\\\":null,\\\"Id\\\":0,\\\"IsDeleted\\\":false,\\\"LastLoginDate\\\":\\\"0001-01-01T00:00:00\\\",\\\"ModifiedOn\\\":\\\"2022-10-17T05:43:22Z\\\",\\\"Name\\\":\\\"Bobby\\\",\\\"Nickname\\\":null,\\\"PasswordHash\\\":null,\\\"Roles\\\":null,\\\"SyncId\\\":\\\"e778e441-4486-478b-8661-4aa107fc92e3\\\"}\",\"ModifiedOn\":\"2022-10-17T05:43:22Z\",\"Status\":1,\"SyncId\":\"e778e441-4486-478b-8661-4aa107fc92e3\",\"TypeName\":\"Speedy.Website.Data.Entities.AccountEntity,Speedy.Website.Data\"},{\"Data\":\"{\\\"AddressId\\\":0,\\\"AddressSyncId\\\":\\\"e49366c5-0cdb-45f7-a997-8f27471b8fbe\\\",\\\"CreatedOn\\\":\\\"2022-10-17T05:43:23Z\\\",\\\"EmailAddress\\\":null,\\\"ExternalId\\\":null,\\\"Id\\\":0,\\\"IsDeleted\\\":false,\\\"LastLoginDate\\\":\\\"0001-01-01T00:00:00\\\",\\\"ModifiedOn\\\":\\\"2022-10-17T05:43:24Z\\\",\\\"Name\\\":\\\"Fred\\\",\\\"Nickname\\\":null,\\\"PasswordHash\\\":null,\\\"Roles\\\":null,\\\"SyncId\\\":\\\"f77ec788-38f5-493d-9ca9-ec57bfcdcf8c\\\"}\",\"ModifiedOn\":\"2022-10-17T05:43:24Z\",\"Status\":1,\"SyncId\":\"f77ec788-38f5-493d-9ca9-ec57bfcdcf8c\",\"TypeName\":\"Speedy.Website.Data.Entities.AccountEntity,Speedy.Website.Data\"}]}";
			//actual.Escape().Dump();
			Assert.AreEqual(expected, actual);

			var actualPagedResults = actual.FromJson<PagedResults<SyncObject>>();
			TestHelper.AreEqual(results, actualPagedResults);
			Assert.AreEqual(2, actualPagedResults.Results.Count);
		}

		public class Test
		{
			public List<SyncObject> Results { get; set; }
		}

		[TestMethod]
		public void TotalPages()
		{
			var actual = new PagedResults<object> { Page = 1, PerPage = 10, TotalCount = 10 };
			Assert.AreEqual(1, actual.TotalPages);

			actual = new PagedResults<object> { Page = 1, PerPage = 10, TotalCount = 11 };
			Assert.AreEqual(2, actual.TotalPages);

			actual = new PagedResults<object> { Page = 10, PerPage = 1, TotalCount = 10 };
			Assert.AreEqual(10, actual.TotalPages);

			actual = new PagedResults<object> { Page = 12, PerPage = 1, TotalCount = 10 };
			Assert.AreEqual(10, actual.TotalPages);
		}

		[TestMethod]
		public void UpdateWith()
		{
			var actual = GetModel();
			var withValues = GetModelWithNonDefaultValues();
			actual.UpdateWith(withValues);
			TestHelper.AreEqual(withValues, actual, nameof(Bindable.HasChanges));
		}

		[TestMethod]
		public void VersionsResults()
		{
			var results = new PagedResults<Version>
			{
				Results = new List<Version>
				{
					new Version(1, 2, 3, 4),
					new Version(5, 6, 7, 8),
					new Version(9, 10, 11),
					new Version(12, 13)
				},
				TotalCount = 4
			};

			var actual = results.ToJson();
			//actual.Escape().CopyToClipboard().Dump();
			var expected = "{\"$id\":\"1\",\"Filter\":\"\",\"HasMore\":false,\"Order\":\"\",\"Page\":1,\"PerPage\":10,\"TotalCount\":4,\"TotalPages\":1,\"Results\":[\"1.2.3.4\",\"5.6.7.8\",\"9.10.11\",\"12.13\"]}";
			Assert.AreEqual(expected, actual);
		}

		#endregion
	}
}