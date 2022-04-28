#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.SyncApi;
using Speedy.UnitTests.Factories;

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
			var actual = new PagedRequest
			{
				Page = 0,
				PerPage = 1000,
				Filter = null,
				FilterValues = null,
				Including = null,
				Options = null,
				OptionValues = null,
				Order = null
			};

			Assert.AreEqual(0, actual.Page);
			Assert.AreEqual(1000, actual.PerPage);
			Assert.AreEqual(null, actual.Filter);
			Assert.AreEqual(null, actual.FilterValues);
			Assert.AreEqual(null, actual.Including);
			Assert.AreEqual(null, actual.Options);
			Assert.AreEqual(null, actual.OptionValues);
			Assert.AreEqual(null, actual.Order);

			actual.Cleanup();

			var expected = new PagedRequest
			{
				Page = 1,
				PerPage = 100,
				Filter = string.Empty,
				FilterValues = new List<string>(),
				Including = new List<string>(),
				Options = new List<string>(),
				OptionValues = new List<string>(),
				Order = string.Empty
			};

			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void GetOption()
		{
			var actual = new PagedRequest().Cleanup();
			Assert.AreEqual(null, actual.GetOptionValue("Foo"));
			actual.AddOptions("Foo", "Bar");
			Assert.AreEqual("Bar", actual.GetOptionValue("Foo"));
			Assert.AreEqual("Bar", actual.GetOptionValue("fOO"));
		}

		[TestMethod]
		public void HasOption()
		{
			var actual = new PagedRequest().Cleanup();
			Assert.IsFalse(actual.HasOption("Foo"));
			actual.Options.Add("fOO");
			Assert.IsTrue(actual.HasOption("Foo"));
			Assert.IsTrue(actual.HasOption("fOO"));
		}

		[TestMethod]
		public void ToQueryString()
		{
			// Scenarios are cumulative so expect the next scenario to have the previous state
			(Action<PagedRequest> update, string expected)[] scenarios =
			{
				(x => x.Page = 2, "Page=2&PerPage=10"),
				(x => x.PerPage = 1, "Page=2&PerPage=1"),
				(x => x.PerPage = 98, "Page=2&PerPage=98"),
				(x => x.AddOptions("foo", "bar"), "Page=2&PerPage=98&Options=foo&OptionValues=bar"),
				(x => x.AddOptions("hello", "world"), "Page=2&PerPage=98&Options=foo&Options=hello&OptionValues=bar&OptionValues=world"),
				(x => x.AddOptions("hello", "again"), "Page=2&PerPage=98&Options=foo&Options=hello&OptionValues=bar&OptionValues=again"),
				(x => x.RemoveOption("hello"), "Page=2&PerPage=98&Options=foo&OptionValues=bar"),
				(x => x.RemoveOption("foo"), "Page=2&PerPage=98"),
				(x => x.Filter = "testing", "Page=2&PerPage=98&Filter=testing"),
				(x => x.FilterValues.Add("aoeu"), "Page=2&PerPage=98&Filter=testing&FilterValues=aoeu"),
				(x => x.FilterValues.Add("htns"), "Page=2&PerPage=98&Filter=testing&FilterValues=aoeu&FilterValues=htns"),
			};

			var actual = new PagedRequest().Cleanup();

			foreach (var scenario in scenarios)
			{
				scenario.expected.Dump();
				scenario.update(actual);
				Assert.AreEqual(scenario.expected, actual.ToQueryString());
				TestHelper.AreEqual(actual, PagedRequest.FromQueryString(scenario.expected));
			}
		}

		[TestMethod]
		public void TypedData()
		{
			var actual = new PagedResults<Address>
			{
				Results = new[] { DataFactory.GetAddress() }
			};
			Assert.AreEqual(1, actual.Results.Count());
		}

		#endregion
	}
}