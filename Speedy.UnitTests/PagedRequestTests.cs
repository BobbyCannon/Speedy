#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.SyncApi;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.UnitTests.Factories;
using Speedy.Website.Models;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class PagedRequestTests
	{
		#region Methods

		[TestMethod]
		public void Cleanup()
		{
			var actual = new PagedRequest { Page = 0, PerPage = 1001 };
			Assert.AreEqual(0, actual.Page);
			Assert.AreEqual(1001, actual.PerPage);

			actual.Cleanup();

			var expected = new PagedRequest { Page = 1, PerPage = 1000 };
			TestHelper.AreEqual(expected, actual, nameof(PagedRequest.Updates));
		}

		[TestMethod]
		public void CustomPagedRequest()
		{
			var request = new CustomPagedRequest { Precision = 2.123, Page = 12, PerPage = 2 };
			var actual = request.ToJson();
			var expected = "{\"Page\":12,\"PerPage\":2,\"Precision\":2.123}";
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void DefaultEmptyJson()
		{
			var request = new PagedRequest();
			var actual = request.ToJson();
			actual.Escape().Dump();
			var expected = "{\"Page\":1,\"PerPage\":10}";
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void FromQueryString()
		{
			var request = new PagedRequest();
			request.ParseQueryString("?filter=test&page=23");
			Assert.AreEqual("test", request.Get("filter"));
			Assert.AreEqual(23, request.Get("page"));
		}

		[TestMethod]
		public void ToJson()
		{
			var request = new PagedRequest();
			var actual = request.ToRawJson();
			var expected = "{\"Page\":1,\"PerPage\":10}";
			actual.Escape().Dump();
			Assert.AreEqual(expected, actual);

			request = new PagedRequest { Page = 2, PerPage = 11 };
			actual = request.ToRawJson();
			expected = "{\"Page\":2,\"PerPage\":11}";
			actual.Escape().Dump();
			Assert.AreEqual(expected, actual);

			request.AddOrUpdate("Filter", "frogs");

			actual = request.ToRawJson();
			expected = "{\"Filter\":\"frogs\",\"Page\":2,\"PerPage\":11}";
			actual.Escape().Dump();
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void ToQueryString()
		{
			// Scenarios are cumulative so expect the next scenario to have the previous state
			(Action<PagedRequest> update, string expected)[] scenarios =
			{
				(_ => { }, ""),
				(x => x.Page = 2, "Page=2"),
				(x => x.PerPage = 1, "Page=2&PerPage=1"),
				(x => x.PerPage = 98, "Page=2&PerPage=98"),
				(x => x.AddOrUpdate("foo", "bar"), "foo=bar&Page=2&PerPage=98"),
				(x => x.AddOrUpdate("Foo", "Bar"), "Foo=Bar&Page=2&PerPage=98"),
				(x => x.AddOrUpdate("hello", "world"), "Foo=Bar&hello=world&Page=2&PerPage=98"),
				(x => x.AddOrUpdate("hello", "again"), "Foo=Bar&hello=again&Page=2&PerPage=98"),
				// Remove should not be case sensitive
				(x => x.Remove("HELLO"), "Foo=Bar&Page=2&PerPage=98"),
				(x => x.Remove("FOO"), "Page=2&PerPage=98"),
				// Should be able to handle special (reserved, delimiters, etc) characters
				(x => x.AddOrUpdate("filter", ";/?:@&=+$,"), "filter=%3b%2f%3f%3a%40%26%3d%2b%24%2c&Page=2&PerPage=98"),
				(x => x.AddOrUpdate("filter", "<>#%\""), "filter=%3c%3e%23%25%22&Page=2&PerPage=98"),
				(x => x.AddOrUpdate("filter", "{}|\"^[]`"), "filter=%7b%7d%7c%22%5e%5b%5d%60&Page=2&PerPage=98"),
				(x => x.Remove("FILTER"), "Page=2&PerPage=98"),
				// Should be able to handle special (reserved, delimiters, etc) characters as key
				(x => x.AddOrUpdate(";/?:@&=+$,<>#%\"{}|\"^[]`", "wow..."), "%3b%2f%3f%3a%40%26%3d%2b%24%2c%3c%3e%23%25%22%7b%7d%7c%22%5e%5b%5d%60=wow...&Page=2&PerPage=98")
			};

			var expected = new PagedRequest();

			foreach (var scenario in scenarios)
			{
				scenario.expected.Dump();
				scenario.update(expected);
				Assert.AreEqual(scenario.expected, expected.ToQueryString());

				var actual = new PagedRequest();
				actual.ParseQueryString(scenario.expected);

				TestHelper.AreEqual(expected, actual, nameof(Bindable.HasChanges));
			}
		}

		[TestMethod]
		public void TypedData()
		{
			var actual = new PagedResults<Address>
			{
				Results = new[] { DataFactory.GetAddress() }
			};
			Assert.AreEqual(1, actual.Results.Count);
		}

		#endregion
	}
}