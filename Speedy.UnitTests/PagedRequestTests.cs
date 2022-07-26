#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.SyncApi;
using Speedy.Extensions;
using Speedy.UnitTests.Factories;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class PagedRequestTests : BaseModelTests<PagedRequest>
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
		public void DefaultEmptyJson()
		{
			var request = new PagedRequest();
			var actual = request.ToJson();
			actual.Escape().Dump();
			var expected = "{\"$id\":\"1\",\"Filter\":\"\",\"Order\":\"\",\"Page\":1,\"PerPage\":10}";
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void FromQueryString()
		{
			var request = new PagedRequest();
			request.ParseQueryString("?filter=test&page=23");
			Assert.AreEqual("test", request.Get("filter"));
			Assert.AreEqual(23, request.Get("page"));

			request.ParseQueryString("?options[]=foo&options[]=bar");
			var actual = request.Get<string[]>("options");
			TestHelper.AreEqual(new[] { "foo", "bar" }, actual);
		}

		[TestMethod]
		public void ToJson()
		{
			var request = new PagedRequest();
			var actual = request.ToJson();
			var expected = "{\"$id\":\"1\",\"Filter\":\"\",\"Order\":\"\",\"Page\":1,\"PerPage\":10}";
			actual.Escape().Dump();
			Assert.AreEqual(expected, actual);

			request = new PagedRequest { Page = 2, PerPage = 11 };
			actual = request.ToRawJson();
			expected = "{\"Filter\":\"\",\"Order\":\"\",\"Page\":2,\"PerPage\":11}";
			actual.Escape().Dump();
			Assert.AreEqual(expected, actual);

			request.AddOrUpdate("Filter", "frogs");

			actual = request.ToRawJson();
			expected = "{\"Filter\":\"frogs\",\"Order\":\"\",\"Page\":2,\"PerPage\":11}";
			actual.Escape().Dump();
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void ToQueryString()
		{
			// Scenarios are cumulative so expect the next scenario to have the previous state
			(Action<PagedRequest> update, string expected)[] scenarios =
			{
				(x => x.AddOrUpdate("options", new[] { "foo", "bar" }), "options[]=foo&options[]=bar"),
				(x => x.Remove("options"), ""),
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
				(x => x.AddOrUpdate("Filter", ";/?:@&=+$,"), "Filter=%3b%2f%3f%3a%40%26%3d%2b%24%2c&Page=2&PerPage=98"),
				(x => x.AddOrUpdate("Filter", "<>#%\""), "Filter=%3c%3e%23%25%22&Page=2&PerPage=98"),
				(x => x.AddOrUpdate("Filter", "{}|\"^[]`"), "Filter=%7b%7d%7c%22%5e%5b%5d%60&Page=2&PerPage=98"),
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

		[TestMethod]
		public void UpdateWith()
		{
			var actual = GetModel();
			var withValues = GetModelWithNonDefaultValues();
			actual.UpdateWith(withValues);
			TestHelper.AreEqual(withValues, actual, nameof(Bindable.HasChanges));
		}

		#endregion
	}
}