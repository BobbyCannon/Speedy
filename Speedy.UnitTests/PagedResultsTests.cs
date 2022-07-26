#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.SyncApi;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.Website.Models;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class PagedResultsTests : BaseModelTests<PagedResults<object>>
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
		public void CustomPagedResults()
		{
			var request = new CustomPagedRequest { Precision = 2.123, Page = 12, PerPage = 2 };
			var results = new CustomPagedResults<object>(request, 1234, 2, 546, "aoeu", false);
			var actual = results.ToJson();
			var expected = "{\"$id\":\"1\",\"Filter\":\"\",\"Order\":\"\",\"Page\":12,\"PerPage\":2,\"Precision\":2.123,\"HasMore\":true,\"TotalCount\":1234,\"TotalPages\":617,\"Results\":[2,546,\"aoeu\",false]}";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void CustomPagedResultsFromUri()
		{
			var request = new CustomPagedRequest();
			request.ParseQueryString("?precision=321.41");
			var results = new CustomPagedResults<object>(request, 1234, 2, 546, "aoeu", false);
			var actual = results.ToJson();
			var expected = "{\"$id\":\"1\",\"Filter\":\"\",\"Order\":\"\",\"Page\":1,\"PerPage\":11,\"Precision\":321.41,\"HasMore\":true,\"TotalCount\":1234,\"TotalPages\":113,\"Results\":[2,546,\"aoeu\",false]}";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void DefaultEmptyJson()
		{
			var request = new PagedRequest();
			var response = new PagedResults<object>(request, 0);
			var actual = response.ToJson();
			actual.Escape().Dump();
			var expected = "{\"$id\":\"1\",\"Filter\":\"\",\"Order\":\"\",\"Page\":1,\"PerPage\":10,\"HasMore\":false,\"TotalCount\":0,\"TotalPages\":1,\"Results\":[]}";
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
			request.ParseQueryString("?filter=foo&page=2");
			var actual = new PagedResults<object>(request, 400, 1, "bar");
			Assert.AreEqual(2, actual.Page);
			Assert.AreEqual(10, actual.PerPage);
			Assert.AreEqual(40, actual.TotalPages);
			Assert.AreEqual(400, actual.TotalCount);
		}

		[TestMethod]
		public void ToJsonThenFromJson()
		{
			var request = new PagedRequest { Page = 2, PerPage = 11 };
			var results = new PagedResults<object>(request, 12, 1, "foo", true);
			var actual = results.ToRawJson();
			var expected = "{\"Filter\":\"\",\"Order\":\"\",\"Page\":2,\"PerPage\":11,\"HasMore\":false,\"TotalCount\":12,\"TotalPages\":2,\"Results\":[1,\"foo\",true]}";
			actual.Escape().Dump();
			Assert.AreEqual(expected, actual);

			// Options and Updates are not required to be equal after serialization
			var update = expected.FromJson<PagedResults<object>>();
			TestHelper.AreEqual(results, update, nameof(results.Options), nameof(results.Updates));

			actual = results.ToRawJson(true, true);
			expected = "{\r\n  \"filter\": \"\",\r\n  \"order\": \"\",\r\n  \"page\": 2,\r\n  \"perPage\": 11,\r\n  \"hasMore\": false,\r\n  \"totalCount\": 12,\r\n  \"totalPages\": 2,\r\n  \"results\": [\r\n    1,\r\n    \"foo\",\r\n    true\r\n  ]\r\n}";
			actual.Escape().Dump();
			Assert.AreEqual(expected, actual);

			// Options and Updates are not required to be equal after serialization
			update = expected.FromJson<PagedResults<object>>();
			TestHelper.AreEqual(results, update);
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
			var expected = "{\"$id\":\"1\",\"Filter\":\"\",\"Order\":\"\",\"Page\":1,\"PerPage\":10,\"HasMore\":false,\"TotalCount\":4,\"TotalPages\":1,\"Results\":[\"1.2.3.4\",\"5.6.7.8\",\"9.10.11\",\"12.13\"]}";
			Assert.AreEqual(expected, actual);
		}

		#endregion
	}
}